using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition.ViewModel;
using System.Windows.Input;
using System;


namespace ScrumFactory.Reports.ViewModel {


    /// <summary>
    /// Delivery Report view model.
    /// Show the project delivery in a report form.
    /// </summary>
    [Export]
    [Export(typeof(IStartsWithApp))]
    public class DefaultReportViewModel : BasePanelViewModel, IViewModel, IStartsWithApp, INotifyPropertyChanged {

        private string panelName;

        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private ReportHelper.Report reports;
        private IDialogService dialogs;

        public ReportHelper.ReportConfig Config { get; set; }

        private string reportXaml = null;

        [Import]
        private Lazy<IProjectContainer> projectContainer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultReportViewModel"/> class.
        /// </summary>
        /// <param name="aggregator">The event aggregator.</param>
        [ImportingConstructor]
        public DefaultReportViewModel(
            [Import] IEventAggregator aggregator,
            [Import] ReportHelper.Report reports,
            [Import] IBackgroundExecutor executor,
            [Import] IDialogService dialogs) {

                this.executor = executor;
                this.aggregator = aggregator;
                this.reports = reports;
                this.dialogs = dialogs;

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p => { Project = p; });
                aggregator.Subscribe<ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, Show);

                CloseWindowCommand = new DelegateCommand(CloseWindow);
                RefreshCommand = new DelegateCommand(Refresh);
                PrintCommand = new DelegateCommand(Print);
                SaveAsCommand = new DelegateCommand(SaveAs);
            
        }

        private string SuggestedFileName {
            get {
                return Config.ReportName + " " + Project.ClientName + " - " + Project.ProjectName;
            }            
        }

        private void Print() {                        
            if (View.Print()) {
                // need to refresh do clean VISUAL PARENTS
                //Refresh();
            }
        }

        private void SaveAs() {

            if (reportXaml == null || Config==null || Project==null)
                return;

            string filename = dialogs.ShowSaveFileDialog(Properties.Resources.Save_as, Project.DocRepositoryPath, SuggestedFileName, Properties.Resources.xps_files, null, true);
            if (filename == null)
                return;

            // creates the paginator
            var paginator = reports.CreatePaginator(View.Report, SuggestedFileName);

            // REFRESH TO AVOID BIND ERRORS AFTER SAVE/PRINT
            View.SetReportDocument(reportXaml, SuggestedFileName, reports, Config);

            var ext = System.IO.Path.GetExtension(filename);

            // SAVE
            if(ext.ToLower()==".xps")
                reports.FlowDocumentToXps(paginator, filename, SuggestedFileName, true);
            else
                reports.FlowDocumentToPDF(paginator, filename, SuggestedFileName, true);

            
        }

        public  void Refresh() {
            IsLoadingData = true;
            executor.StartBackgroundTask<string>(
                () => {
                    if (Config.StaticXAMLReport != null)
                        return Config.StaticXAMLReport;
                    else
                        return reports.CreateReportXAML(serverUrl.Url, Config); },
               xaml => {
                   IsLoadingData = false;
                   try {
                       reportXaml = xaml;
                       View.SetReportDocument(reportXaml, SuggestedFileName, reports, Config);                    
                   }
                   catch (System.Exception) {
                       throw new ScrumFactory.Exceptions.ScrumFactoryException("Error parsing Report XAML");
                   }
               });
        }

 

        private void CloseWindow() {            
            foreach (string name in Config.ReportViewModels.Keys)
                View.SetElementViewModel(name, null);
            View.SetReportDocument(null, null, null, null);
           Close();
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show(ReportHelper.ReportConfig config) {
            Config = config;
            PanelName = Config.ReportName;            
            Refresh();
            Show(projectContainer.Value);          
        }

        public string PanelName {
            get {
                return panelName;
            }
            set {
                panelName = value;
                OnPropertyChanged("PanelName");
            }
        }

        public View.IReportView View { get; set; }

        [Import(typeof(DefaultReport))]
        IView IViewModel.View {
            get {
                return this.View;
            }
            set {
                this.View = (View.IReportView) value;
            }
        }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        private Project project;
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>The project.</value>
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
                OnPropertyChanged("Project");
            }
        }
        
        public ICommand RefreshCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }

        public ICommand SaveAsCommand { get; set; }

    }
}
