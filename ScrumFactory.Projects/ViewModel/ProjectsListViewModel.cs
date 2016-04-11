using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Services;
using System.Linq;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;
using System.Windows.Data;

namespace ScrumFactory.Projects.ViewModel {

    public enum ProjectStatusFilter : short {
        OPEN_PROJECTS,        
        CLOSED_PROJECTS,
        ALL_PROJECTS
    }

    public enum ProjectCreateDateFilter {
        ONE_YEAR_AGO,
        TWO_YEARS_AGO,
        FIVE_YEARS_AGO,
        ANYTIME
    }



    /// <summary>
    /// Implements the Projects List ViewModel.
    /// </summary>
    [Export]    
    [Export(typeof(ITopMenuViewModel))]    
    public class ProjectsListViewModel : BasePanelViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        private Project selectedProject;
        private string searchFilterText;
        private ProjectStatusFilter statusFilter;
        private ProjectCreateDateFilter createDateFilter;

        private  DelayAction delayFilter;


        private System.Windows.Data.CollectionViewSource projectsViewSource;
        
        private IEventAggregator aggregator; 
        private IProjectsService projectsService;
        private IBackgroundExecutor executor;
        private IAuthorizationService authorizator;
        private IDialogService dialogs;

        private int? projectNumberToShowOnInit;

        private const int MAX_PROJECTS_PER_REQUEST = 100;

     
        [ImportingConstructor()]
        public ProjectsListViewModel(
            [Import] IEventAggregator aggregator,
            [Import] IProjectsService projectsService,
            [Import] IBackgroundExecutor executor,
            [Import] IDialogService dialogs,
            [Import] IAuthorizationService authorizator) {

            this.aggregator = aggregator;
            this.projectsService = projectsService;
            this.executor = executor;
            this.dialogs = dialogs;
            this.authorizator = authorizator;

            projectsViewSource = new System.Windows.Data.CollectionViewSource();
            projectsViewSource.SortDescriptions.Add(new SortDescription("Status", ListSortDirection.Ascending));
            projectsViewSource.SortDescriptions.Add(new SortDescription("TotalDayAllocation", ListSortDirection.Descending));
            projectsViewSource.SortDescriptions.Add(new SortDescription("ProjectNumber", ListSortDirection.Descending));
            

            projectsViewSource.Filter += new System.Windows.Data.FilterEventHandler(FilteredProjects_Filter);
            delayFilter = new DelayAction(500, new DelayAction.ActionDelegate(() => {
                if (allDataLoaded && FilteredProjects != null) {
                    FilteredProjects.Refresh();
                    SetGroupCount();
                } else {
                    LoadProjectList();
                }
            }));
            
            aggregator.Subscribe<Project>(ScrumFactoryEvent.ProjectStatusChanged, OnProjectStatusChanged);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ProjectCreated, OnProjectCreated);

            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged,
                m => {
                    ((DelegateCommand)CreateProjectCommand).NotifyCanExecuteChanged();
                    if (m == null) {                        
                        OnProjectListLoaded(new List<Project>());
                        SelectedProject = null;
                        aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, SelectedProject);
                        return;
                    }
                    LoadProjectList();
                    ShowDetailOnInit();
                });

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p => {
                if (SelectedProject != p)
                    SelectedProject = p;
            });

            aggregator.Subscribe(ScrumFactoryEvent.CreateNewProject, () => {
                dialogs.SelectTopMenu(this);
                OpenCreateProjectWindow();
            });

            aggregator.Subscribe<int>(ScrumFactoryEvent.ProjectArgOnInit, OnProjectArgOnInit);

            OnLoadCommand = new DelegateCommand(() => { if (NeedRefresh) LoadProjectList(); });

            RefreshProjectListCommand = new DelegateCommand(() => { LoadProjectList(); });
            ShowDetailWindowCommand = new DelegateCommand<Project>(p => { ShowDetailWindow(p.ProjectUId); });
            CreateProjectCommand = new DelegateCommand(CanCreateProject, OpenCreateProjectWindow);

            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);



            LoadMoreCommand = new DelegateCommand(() => { LoadProjectList(false); });

            NeedRefresh = true;
            
        }

   

     

        private void OnProjectArgOnInit(int projectNumber) {

            projectNumberToShowOnInit = projectNumber;

            // if not logged yet, wait for login
            if (authorizator.SignedMemberProfile == null)
                return;

            ShowDetailOnInit();
        }

        private void ShowDetailOnInit() {
            if (projectNumberToShowOnInit != null) {
                ShowDetailWindow(projectNumberToShowOnInit.ToString());
                projectNumberToShowOnInit = null;
            }
        }

        private void OnProjectStatusChanged(Project project) {
            Project projectAtList = Projects.SingleOrDefault(p => p.ProjectUId == project.ProjectUId);
            if (projectAtList == null)
                return;

            projectAtList.IsSuspended = project.IsSuspended;
            projectAtList.Status = project.Status;
            projectAtList.StartDate = project.StartDate;
            projectAtList.EndDate = project.EndDate;

            SetGroupCount();

            // refresh item
            Projects.Remove(projectAtList);
            Projects.Add(projectAtList);

            
        }

        private bool loadingProject;
        private void ShowDetailWindow(string projectUId) {


            if (loadingProject)
                return;

            loadingProject = true;

            if (authorizator.SignedMemberProfile == null)
                SelectedProject = null;

            IsLoadingData = true;
            executor.StartBackgroundTask<Project>(
                () => {
                    if (projectUId != null && authorizator.SignedMemberProfile != null)
                        return projectsService.GetProject(projectUId);
                    else
                        return null;
                },
                p => {
                    SelectedProject = p;
                    loadingProject = false;
                    aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, SelectedProject);
                    IsLoadingData = false;
                },
                ex => {
                    loadingProject = false;
                    throw ex;
                });                                
        }

        private bool onlyMemberProjects = true;
        public bool OnlyMemberProjects {
            get {
                return onlyMemberProjects;
            }
            set {
                onlyMemberProjects = value;
                OnPropertyChanged("OnlyMemberProjects");
                LoadProjectList();
            }
        }

        private bool CanCreateProject() {
            return (authorizator.SignedMemberProfile != null);
        }

        /// <summary>
        /// Opens the create project window.
        /// </summary>
        private void OpenCreateProjectWindow() {
            CreateProjectViewModel.Show(this);
        }
        
        private void OnProjectCreated(Project newProject) {            
            Projects.Add(newProject);
            ShowDetailWindow(newProject.ProjectUId);            
        }

        [Import(typeof(CreateProjectViewModel))]
        private CreateProjectViewModel CreateProjectViewModel { get; set; }

        /// <summary>
        /// Loads the project list from the service.
        /// </summary>
        private void LoadProjectList(bool clearList = true) {

            IsLoadingData = true;

            if (clearList)
                Projects = new ObservableCollection<Project>();
            int skip = Projects.Count;

            executor.StartBackgroundTask<ICollection<Project>>(
                () => {
                    string memberUId = null;
                    if (OnlyMemberProjects)
                        memberUId = "me";

                    return projectsService.GetProjects(StartDateFromFilter, null, StatusFilter.ToString(), memberUId, SearchFilterText, MAX_PROJECTS_PER_REQUEST, skip);
                }, ps => { OnProjectListLoaded(ps, clearList); });
        }

        private bool hasMoreToLoad = false;
        public bool HasMoreToLoad {
            get {
                return hasMoreToLoad;
            }
            set {
                hasMoreToLoad = value;
                OnPropertyChanged("HasMoreToLoad");
            }
        }

        private bool allDataLoaded = false;

        private Project autoSelectedProject = null;
        private void OnProjectListLoaded(ICollection<Project> projects, bool clearList = true) {

            NeedRefresh = false;

            if (projects == null) {
                HasMoreToLoad = false;
                return;
            }

            // if return less, then there is no more to load
            // if return more, then is an old version, 
            // if returns MAX_PROJECTS_PER_REQUEST, then there is more pages
            HasMoreToLoad = (projects.Count == MAX_PROJECTS_PER_REQUEST);

            // if has no more pages and the filter was empty, all the data has been loaded
            allDataLoaded = (!HasMoreToLoad && string.IsNullOrEmpty(SearchFilterText));

            ShowDetailWindowCommand = null;
            if(clearList)
                this.Projects.Clear();
            foreach (var project in projects) {
                this.Projects.Add(project);
            }
            projectsViewSource.GroupDescriptions.Clear();
            projectsViewSource.Source = Projects;

            SetGroupCount();

            ShowDetailWindowCommand = new DelegateCommand<Project>(p => { ShowDetailWindow(p.ProjectUId); });

            projectsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Status"));

            // wpf auto selectes the first project....i dont want that
            if(Projects.Count>0)
                autoSelectedProject = Projects[0];
            IsLoadingData = false;                
            OnPropertyChanged("FilteredProjects");
        }

        private int groupSizeInPixels;
        public int GroupSizeInPixels {
            get {
                return groupSizeInPixels;
            }
            set {
                TemplateLineHeightInPixels = 22;
                groupSizeInPixels = value;
                if (groupSizeInPixels < ProjectGroupSize.TemplateMinSize) {
                    TemplateSizeInPixels = groupSizeInPixels - 2;
                    TemplateLineHeightInPixels = 45;
                } else {
                    TemplateSizeInPixels = (groupSizeInPixels / (groupSizeInPixels / ProjectGroupSize.TemplateMinSize)) - 2;
                }
                OnPropertyChanged("GroupSizeInPixels");
                OnPropertyChanged("TemplateSizeInPixels");
                OnPropertyChanged("TemplateLineHeightInPixels");
            }
        }

        public int TemplateSizeInPixels { get; set; }
        public int TemplateLineHeightInPixels { get; set; }
       

        private void SetGroupCount() {

            if (FilteredProjects.Groups != null && !string.IsNullOrEmpty(SearchFilterText)) {
                GroupSizeInPixels = ProjectGroupSize.GetGroupSizeinPixels(FilteredProjects.Groups.Count);
                return;
            }

            if (Projects != null)
                GroupSizeInPixels = ProjectGroupSize.GetGroupSizeinPixels(Projects.Select(p => p.Status).Distinct().Count());
        }

        private string StartDateFromFilter {
            get {
                if (StatusFilter.Equals(ProjectStatusFilter.OPEN_PROJECTS))
                    return null;

                if (CreateDateFilter.Equals(ProjectCreateDateFilter.FIVE_YEARS_AGO))
                    return System.DateTime.Today.AddYears(-5).ToString("yyyy-MM-dd");

                if (CreateDateFilter.Equals(ProjectCreateDateFilter.TWO_YEARS_AGO))
                    return System.DateTime.Today.AddYears(-2).ToString("yyyy-MM-dd");

                if (CreateDateFilter.Equals(ProjectCreateDateFilter.ONE_YEAR_AGO))
                    return System.DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd");

                return null;
            }
        }
       
        /// <summary>
        /// Handles the Filter event of the FilteredProjects control.
        /// Only projects that fits the status filter and the search filter text are accepted.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Data.FilterEventArgs"/> instance containing the event data.</param>
        private void FilteredProjects_Filter(object sender, System.Windows.Data.FilterEventArgs e) {
            Project item = e.Item as Project;
            if (item == null) {
                e.Accepted = false;
                return;
            }

            if (string.IsNullOrEmpty(SearchFilterText)) {
                e.Accepted = true;
                return;
            }

            
            string[] tags = SearchFilterText.ToLower().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            e.Accepted = tags.All(t =>
                item.ProjectName.NormalizedContains(t) ||
                item.ClientName.NormalizedContains(t)||
                item.ProjectNumber.ToString().Equals(t));

        }

        /// <summary>
        /// Verifies if the text contains all words.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="words">The words.</param>
        /// <returns>True if the text contains all words.</returns>
        private bool StringContainsAll(string text, string[] words) {
            if (words == null)
                return true;
            bool containsAll = true;
            foreach (string word in words) {
                containsAll = containsAll && text.ToUpper().Contains(word.ToUpper());
                if (!containsAll)
                    break;
            }

            return containsAll;
        }

        #region IPanelViewModel Members

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.My_projects;
            }
        }

        /// <summary>
        /// Gets the panel placement.
        /// </summary>
        /// <value>The panel placement.</value>
        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Normal; }
        }

      

        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get {
                return 100;
            }
        }

      

        #endregion

        #region IProjectsListViewModel Members


        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        [Import(typeof(ProjectsList))]
        public IView View { get; set; }

        /// <summary>
        /// Gets the filtered projects.
        /// </summary>
        /// <value>The filtered projects.</value>
        public ICollectionView FilteredProjects {
            get {
                return projectsViewSource.View;
            }
        }       

        /// <summary>
        /// Gets the projects.
        /// </summary>
        /// <value>The projects.</value>
        public ObservableCollection<Project> Projects { get; private set; }

        /// <summary>
        /// Gets or sets the status filter.
        /// </summary>
        /// <value>The status filter.</value>
        public ProjectStatusFilter StatusFilter {
            get {
                return this.statusFilter;
            }
            set {
                this.statusFilter = value;
                OnPropertyChanged("StatusFilter");
                LoadProjectList();
            }
        }

        public ProjectCreateDateFilter CreateDateFilter {
            get {
                return createDateFilter;
            }
            set {
                createDateFilter = value;
                LoadProjectList();
            }
        }

        /// <summary>
        /// Gets the selected project.
        /// </summary>
        /// <value>The selected project.</value>
        public Project SelectedProject {
            get {
                return selectedProject;
            }
            set {
                //if (value == autoSelectedProject) {
                //    autoSelectedProject = null;
                //    return;
                //}
                selectedProject = value;
                OnPropertyChanged("SelectedProject");
            }
        }


        /// <summary>
        /// Gets or sets the search filter text.
        /// </summary>
        /// <value>The search filter text.</value>
        public string SearchFilterText {
            get {
                return this.searchFilterText;
            }
            set {
                this.searchFilterText = value;
                OnPropertyChanged("SearchFilterText");                
                delayFilter.StartAction();
            }
        }

        public string ImageUrl {
            get {
                return "\\Images\\TopMenu\\Lupe.png";
            }
        }

        private void CopyToClipboard() {

            string textData = string.Empty;
            string htmlData = "<table>";
            foreach (Project p in FilteredProjects) {                
                textData = textData + p.ToString() + System.Environment.NewLine;
                htmlData = htmlData + "<tr>" +  p.ToHTMLString(authorizator.ServerUrl.Url) + "</tr>";
            }

            htmlData = htmlData + "</table>";
            htmlData = HTMLClipboardHelper.GetHtmlDataString(htmlData);

            System.Windows.DataObject data = new System.Windows.DataObject();
            data.SetData(System.Windows.DataFormats.UnicodeText, textData, true);
            data.SetData(System.Windows.DataFormats.Text, textData, true);
            data.SetData(System.Windows.DataFormats.OemText, textData, true);
            data.SetData(System.Windows.DataFormats.Html, htmlData, true);
            
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data);
        }

        /// <summary>
        /// Gets the show detail window command.
        /// </summary>
        /// <value>The show detail window command.</value>
        public System.Windows.Input.ICommand ShowDetailWindowCommand { get; private set; }


        /// <summary>
        /// Gets the load once project list command.
        /// This command is executed only once, when the View is showed.
        /// </summary>
        /// <value>The load once project list command.</value>
        public System.Windows.Input.ICommand LoadOnceProjectListCommand { get; private set;}

        /// <summary>
        /// Gets the create project command.
        /// </summary>
        /// <value>The create project command.</value>
        public System.Windows.Input.ICommand CreateProjectCommand { get; private set; }

        /// <summary>
        /// Gets the refresh project list command.
        /// </summary>
        /// <value>The refresh project list command.</value>
        public System.Windows.Input.ICommand RefreshProjectListCommand { get; private set; }

        public System.Windows.Input.ICommand OnLoadCommand { get; set; }

        public System.Windows.Input.ICommand CopyToClipboardCommand { get; set; }

     

        public System.Windows.Input.ICommand LoadMoreCommand { get; set; }

        #endregion
    }
}
