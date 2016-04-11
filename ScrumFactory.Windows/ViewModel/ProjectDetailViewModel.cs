using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition.ViewModel;

namespace ScrumFactory.Windows.ViewModel {

    [Export]
    [Export(typeof(ITopMenuViewModel))]
    [Export(typeof(IProjectContainer))]
    public class ProjectDetailViewModel : BasePanelViewModel, ITopMenuViewModel, IProjectContainer, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        
        private List<IProjectTabViewModel> projectPanels;

        [Import]
        private IDialogService dialogs { get; set; }

        [Import]
        private IServerUrl ServerUrl { get; set; }

        private Project project;
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
                OnPropertyChanged("Project");
                OnPropertyChanged("ProjectSprints");
            }
        }

        [ImportingConstructor]
        public ProjectDetailViewModel([Import] IEventAggregator eventAggregator) {

            this.aggregator = eventAggregator;
          

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, ViewProjectDetails);

            aggregator.Subscribe<Task>(ScrumFactoryEvent.ShowTaskDetail, t => { if (!View.IsVisible) dialogs.SelectTopMenu(this);}, 5);
            aggregator.Subscribe<BacklogItem>(ScrumFactoryEvent.ShowItemDetail, i => { if (!View.IsVisible) dialogs.SelectTopMenu(this); });

            aggregator.Subscribe<BacklogItem>(ScrumFactoryEvent.ShowItemDetail, i => { if (!View.IsVisible) dialogs.SelectTopMenu(this); });

            aggregator.Subscribe<IProjectTabViewModel>(ScrumFactoryEvent.ShowProjectTab,
             w => {
                 if (w == SelectedProjectTab)
                     return;
                 CloseAllChildWindows();
                 SelectedProjectTab = w;
             });

            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, m => {
                SelectedProjectTab = ProjectPanels.FirstOrDefault();
            });

            aggregator.Subscribe<Sprint>(ScrumFactoryEvent.SprintAdded, s => { RefreshProject(); });
            aggregator.Subscribe<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, s => { RefreshProject(); });
            aggregator.Subscribe(ScrumFactoryEvent.SprintsShifted, RefreshProject);

            

            ShowCodeRepositoryCommand = new DelegateCommand<string>(ShowCodeRepository);
            ShowDocRepositoryCommand = new DelegateCommand<string>(ShowDocRepository);
            CopyProjectUrlCommand = new DelegateCommand(CopyProjectUrl);

        }

        private void RefreshProject() {
            OnPropertyChanged("Project");
            OnPropertyChanged("ProjectSprints");
        }

        public Sprint[] ProjectSprints {
            get {
                if (Project == null || Project.Sprints==null)
                    return null;
                return Project.Sprints.OrderBy(s => s.SprintNumber).ToArray();
            }
        }
      
        private void ViewProjectDetails(Project p) {

           CloseAllChildWindows();

            if (!View.IsVisible)
                dialogs.SelectTopMenu(this);

            Project = p;


        }

        public string PanelName {
            get { return null; }
        }

        public int PanelDisplayOrder {
            get { return int.MaxValue; }
        }

        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Project; }
        }

        public string ImageUrl {
            get { return null; }
        }

        [Import(typeof(ProjectDetail))]
        public IView View {
            get;
            set;
        }

        private IProjectTabViewModel selectedProjectTab;
        public IProjectTabViewModel SelectedProjectTab {
            get {
                return selectedProjectTab;
            }
            set {
                selectedProjectTab = value;
                OnPropertyChanged("SelectedProjectTab");
            }
        }

        [ImportMany(typeof(IProjectTabViewModel))]
        public IEnumerable<IProjectTabViewModel> ProjectPanels {
            get {
                return this.projectPanels;
            }
            set {
                projectPanels = value.OrderBy(p => p.PanelDisplayOrder).ToList();
                OnPropertyChanged("ProjectPanels");
            }
        }

   

        private void ShowCodeRepository(string path) {
            aggregator.Publish<string>(ScrumFactoryEvent.ShowCodeRepository, path);
        }

        private void ShowDocRepository(string path) {
            aggregator.Publish<string>(ScrumFactoryEvent.ShowDocRepository, path);
        }

        private void CopyProjectUrl() {
            string htmlData = Project.ClientName + " - " + Project.ProjectName + " (<a href=\"" + ServerUrl.Url +"/" + Project.ProjectNumber + "\">" + Project.ProjectNumber + "</a>)";
            string textData = ServerUrl.Url + "/" + Project.ProjectNumber;

            htmlData = ScrumFactory.Windows.Helpers.HTMLClipboardHelper.GetHtmlDataString(htmlData);

            System.Windows.DataObject data = new System.Windows.DataObject();
            data.SetData(System.Windows.DataFormats.UnicodeText, textData, true);
            data.SetData(System.Windows.DataFormats.Text, textData, true);
            data.SetData(System.Windows.DataFormats.OemText, textData, true);
            data.SetData(System.Windows.DataFormats.Html, htmlData, true);

            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data);

        }

        private void CloseAllChildWindows() {        
            OpenedWindows.Clear();
        }

        public ICommand ShowCodeRepositoryCommand { get; set; }
        public ICommand ShowDocRepositoryCommand { get; set; }
        public ICommand CopyProjectUrlCommand { get; set; }
    }
}
