using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;
using System.Windows.Documents;

using System;
using System.Windows;

namespace ScrumFactory.Reports {
    
    /// <summary>    
    /// Interaction logic for DeliveryReport.xaml
    /// </summary>
    [Export]
    public partial class DefaultReport : UserControl, View.IReportView {

        private object model;

        //private System.Windows.Documents.FlowDocument Document;

        public DefaultReport() {
            InitializeComponent();
        }

        [Import]
        private IServerUrl serverUrl { get; set; }

        private ReportHelper.Report reports;
        private ReportHelper.ReportConfig config;
        private string title;
        private string reportXaml;
       
        [Import(typeof(ViewModel.DefaultReportViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }

        public FlowDocument Report {
            get {
                return documentReader.Document;
            }
        }

        

        public void SetReportDocument(string xaml, string title, ReportHelper.Report reports, ReportHelper.ReportConfig config) {

            this.reportXaml = xaml;
            this.reports = reports;
            this.config = config;
            this.title = title;

            if (reportXaml == null) {

                documentReader.Document = null;
                return;
            } 

            xaml = CleanInvalidXmlChars(xaml);
            var document = System.Windows.Markup.XamlReader.Parse(xaml) as System.Windows.Documents.FlowDocument;
            documentReader.Document = document;

            foreach (string name in config.ReportViewModels.Keys)
                SetElementViewModel(name, config.ReportViewModels[name]);

            ScrumFactory.ReportHelper.Paginator.LoadLogo(serverUrl.Url + "/images/companyLogo.png");
        }

        public string CleanInvalidXmlChars(string xml) {
            string pattern = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF]);";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (regex.IsMatch(xml))
                xml = regex.Replace(xml, String.Empty);
            return xml;
        }

        public void SetElementViewModel(string name, object model) {
            SetElementViewModel(name, model, documentReader.Document);
        }

        private void SetElementViewModel(string name, object model, System.Windows.Documents.FlowDocument document) {
            if (document == null)
                return;
            System.Windows.FrameworkElement element = document.FindName(name) as System.Windows.FrameworkElement;
            if (element == null)
                return;
            element.DataContext = model;
        }


        public bool Print() {

            var printDialog = new PrintDialog();

            // auto sets landscape mode
            if(Report.PageWidth > 1000)
                printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

            if (printDialog.ShowDialog() == false)
                return false;

            var paginator = reports.CreatePaginator(documentReader.Document, title);
            SetReportDocument(reportXaml, title, reports, config); // NEED THIS TO AVOID BIND ERRORS AFTER PRINT
            printDialog.PrintDocument(paginator, title);

            return true;
                
            
        }

        
    }
}
