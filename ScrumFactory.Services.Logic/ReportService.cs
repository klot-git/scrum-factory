﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;
using System.Threading;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.Serialization;
using System.Web.Hosting;

namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(IReportService))]    
    public class ReportService : IReportService {
        
        [Import]
        private ITeamService_ServerSide teamService { get; set; }

        [Import]
        private IProjectsService projectService { get; set; }

        [Import]
        private IProposalsService proposalService { get; set; }

        [Import]
        private ICalendarService calendar { get; set; }

        [Import]
        private IBacklogService backlogService { get; set; }

        [Import]
        private IProjectConstraintsService constraintsService { get; set; }

        private ReportHelper.ReportConfig config = null;
        private string serverUrl = null;
        private string format = "pdf";

        private string fileName = null;

        private byte[] pdfBytes = new byte[0];

        private string errorMessage = "";



        //http://localhost:1335/reportservice/SFReports/SprintReview/scope/1c286ff3-3060-4c55-b467-ae1aa1d9e52b

        [STAThread]
        [WebGet(UriTemplate = "SFReports/{templateGroup}/{template}/{projectUId}/?format={format}&proposalUId={proposalUId}", ResponseFormat = WebMessageFormat.Json)]
        public byte[] GetReport(string templateGroup, string template, string projectUId, string format, string proposalUId) {

            errorMessage = "";

            fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", Guid.NewGuid() + "." + format);                

            serverUrl = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.Scheme + "://" +
                        WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.Authority;

            this.format = format;

            config = CreateReportConfig(projectUId, templateGroup, template, "report");

            if (templateGroup == "ProposalReport") {
                ProposalConfig(config, projectUId, proposalUId);                
            }
            
            Thread thread = new Thread(_GetReport);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (!String.IsNullOrEmpty(errorMessage))
                throw new WebFaultException<string>("ERRO:" + errorMessage, System.Net.HttpStatusCode.BadRequest);

            return pdfBytes;

            
        }

        private void _GetReport() {            
            var document = CreateFlowDocument();            
            ForceRenderFlowDocument(document);            
            CreateReport(document);
        }

        private ReportHelper.ReportConfig CreateReportConfig(string projectUId, string group, string template, string name) {

            var config = new ReportHelper.ReportConfig(group, template, name);

            var project = projectService.GetProject(projectUId);

            // add project
            config.ReportObjects.Add(project);

            // get project members
            var members = teamService.GetProjectMembers(projectUId);
            foreach (var mm in project.Memberships)
                mm.Member = members.SingleOrDefault(m => m.MemberUId == mm.MemberUId);

            // add risks
            ICollection<Risk> risks = projectService.GetProjectRisks(projectUId);
            config.ReportObjects.Add(risks);

            // add groups
            ICollection<BacklogItemGroup> groups = backlogService.GetBacklogItemGroups(projectUId);
            config.ReportObjects.Add(groups);

            // add itens
            ICollection<BacklogItem> items = backlogService.GetBacklog(projectUId, null, (short)ScrumFactory.Services.BacklogFiltersMode.ALL);
            
            foreach (BacklogItem item in items) {
                item.ValidPlannedHours = item.GetValidPlannedHours();

                int? firstSprint = item.ValidPlannedHours.Min(h => h.SprintNumber);
                item.FirstSprintWorked = firstSprint.HasValue ? firstSprint.Value : project.LastSprint.SprintNumber;

                int? lastSprint = item.ValidPlannedHours.Max(h => h.SprintNumber);
                item.LastSprintWorked = lastSprint.HasValue ? lastSprint.Value : project.LastSprint.SprintNumber;

                if (item.FirstSprintWorked < project.CurrentValidSprint.SprintNumber)
                    item.OrderSprintWorked = item.LastSprintWorked;
                else
                    item.OrderSprintWorked = item.FirstSprintWorked;

                item.Group = groups.SingleOrDefault(g => g.GroupUId == item.GroupUId);
            }

            config.ReportObjects.Add(items);
                        

            // add constraints
            ICollection<ProjectConstraint> constraints = constraintsService.GetProjectConstraints(projectUId);
            config.ReportObjects.Add(constraints);

            // add end date
            config.AddReportVar("ProjectEndDate", project.LastSprint.EndDate);

            if (project.CurrentSprint != null) {
                if (project.CurrentSprint.SprintNumber > 1) {
                    config.ReportVars.Add("ProjectCurrentSprintNumber", project.CurrentSprint.SprintNumber.ToString());
                    config.ReportVars.Add("ProjectPreviousSprintNumber", (project.CurrentSprint.SprintNumber - 1).ToString());
                }
                else {
                    if (project.Sprints.Count > project.CurrentSprint.SprintNumber + 1)
                        config.ReportVars.Add("ProjectCurrentSprintNumber", (project.CurrentSprint.SprintNumber + 1).ToString());
                    else
                        config.ReportVars.Add("ProjectCurrentSprintNumber", project.CurrentSprint.SprintNumber.ToString());

                    config.ReportVars.Add("ProjectPreviousSprintNumber", project.CurrentSprint.SprintNumber.ToString());
                }
            }

            
            return config;

        }

        private void ProposalConfig(ReportHelper.ReportConfig config, string projectUId, string proposalUId) {

            var proposal = proposalService.GetProjectProposal(projectUId, proposalUId);

            if (proposal.ProposalStatus != (short)ProposalStatus.PROPOSAL_WAITING) {
                ProposalDocument document = proposalService.GetProposalDocument(projectUId, proposalUId);
                config.StaticXAMLReport = document.ProposalXAML;
                return;
            }

            // proposta
            config.ReportObjects.Add(proposal);

            // calcs the work days
            int dayCount = calendar.CalcWorkDayCount(proposal.EstimatedStartDate, proposal.EstimatedEndDate);
            config.ReportVars.Add("workDaysCount", dayCount.ToString());

            // currency rate
            if (proposal.CurrencyRate == null)
                proposal.CurrencyRate = 1;

            // hourscosts
            var hourCosts = proposalService.GetHourCosts(projectUId);
            config.ReportObjects.Add(hourCosts);

            // creates proposal items with price            
            List<ProposalItemWithPrice> itemsWithValue = new List<ProposalItemWithPrice>();
            ICollection<BacklogItem> items = backlogService.GetBacklog(projectUId, null, (short)ScrumFactory.Services.BacklogFiltersMode.ALL);
            foreach (var item in proposal.Items) {
                var itemB = items.SingleOrDefault(i => i.BacklogItemUId == item.BacklogItemUId);
                if (itemB != null) {
                    var price = proposal.CalcItemPrice(itemB, hourCosts);
                    itemsWithValue.Add(new ProposalItemWithPrice(proposal.ProposalUId, itemB, price));
                }
            }
            config.ReportObjects.Add(itemsWithValue);


        }


        public System.Windows.Documents.FlowDocument CreateFlowDocument() {

            var report = new ReportHelper.Report();

            var xaml = report.CreateReportXAML(serverUrl, config);
            xaml = CleanInvalidXmlChars(xaml);

            System.Windows.Documents.FlowDocument document = null;

            document = System.Windows.Markup.XamlReader.Parse(xaml) as System.Windows.Documents.FlowDocument;

            foreach (string name in config.ReportViewModels.Keys)
                SetElementViewModel(name, config.ReportViewModels[name], document);
            
            return document;

        }

        private static string ForceRenderFlowDocumentXaml = @"<Window xmlns=""http://schemas.microsoft.com/netfx/2007/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><FlowDocumentScrollViewer Name=""viewer""/></Window>";

        private void ForceRenderFlowDocument(FlowDocument document) {
            using (var reader = new XmlTextReader(new StringReader(ForceRenderFlowDocumentXaml))) {
                Window window = XamlReader.Load(reader) as Window;
                FlowDocumentScrollViewer viewer = LogicalTreeHelper.FindLogicalNode(window, "viewer") as FlowDocumentScrollViewer;
                viewer.Document = document;
                // Show the window way off-screen
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                //window.Top = Int32.MaxValue;
                //window.Left = Int32.MaxValue;
                //window.ShowInTaskbar = false;
                window.Show();
                // Ensure that dispatcher has done the layout and render passes
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
                viewer.Document = null;
                window.Close();
            }
        }


        private void CreateReport(System.Windows.Documents.FlowDocument document) {
            
            if (document == null)
                return;

          
            var report = new ReportHelper.Report();

            var paginator = report.CreatePaginator(document, "title");
            
            try {
                
                if (format == "pdf")
                    report.FlowDocumentToPDF(paginator, fileName, "report", true);
                else
                    report.FlowDocumentToXps(paginator, fileName, "report", true);
            } catch(Exception ex) {
                errorMessage = ex.Message;
                return;
            }
            
            pdfBytes = File.ReadAllBytes(fileName);

            File.Delete(fileName);

        }


        private void SetElementViewModel(string name, object model, System.Windows.Documents.FlowDocument document) {
            if (document == null)
                return;
            System.Windows.FrameworkElement element = document.FindName(name) as System.Windows.FrameworkElement;
            if (element == null)
                return;
            element.DataContext = model;
        }

        private string CleanInvalidXmlChars(string xml) {
            string pattern = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF]);";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (regex.IsMatch(xml))
                xml = regex.Replace(xml, String.Empty);
            return xml;
        }

        private void LogError(Exception ex) {
            
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "log.txt");
                
            System.IO.FileStream fstream = new FileStream(logPath, FileMode.Append);
                
            StreamWriter writer = new StreamWriter(fstream);
            writer.WriteLine(DateTime.Now);
            writer.WriteLine(ex.Message);

            if (ex.InnerException != null) {
                writer.WriteLine("INNER: " + ex.InnerException.Message);
                writer.WriteLine(ex.InnerException.StackTrace.ToString());
            }
            else
                writer.WriteLine(ex.StackTrace.ToString());

            writer.WriteLine("------------------------------------------------------------------------------");
            writer.Close();
            

        }

        private void LogMessage(string message) {

            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "log.txt");

            System.IO.FileStream fstream = new FileStream(logPath, FileMode.Append);

            StreamWriter writer = new StreamWriter(fstream);
            writer.WriteLine(DateTime.Now);
            writer.WriteLine(message);
            
            writer.WriteLine("------------------------------------------------------------------------------");
            writer.Close();


        }


    }

    


    public class ProposalItemWithPrice : ProposalItem {

        [DataMember]
        public decimal Price { get; set; }

        public ProposalItemWithPrice() : base() { }

        public ProposalItemWithPrice(string proposalUId, BacklogItem item, decimal value) {
            Price = value;
            BacklogItemUId = item.BacklogItemUId;
            Item = item;
            ProposalUId = ProposalUId;
        }
    }

}
