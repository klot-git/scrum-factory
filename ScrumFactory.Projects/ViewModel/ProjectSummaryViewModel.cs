using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Windows.Input;
using System.Linq;
using ScrumFactory.Composition.View;
using System;

namespace ScrumFactory.Projects.ViewModel {

 

    public enum IndicatorStatus : int {
        NORMAL,
        ATTENTION,
        OVER
    }

    
    /// <summary>
    /// Project Summary Tab View Model.
    /// </summary>
    [Export(typeof(IProjectTabViewModel))]
    [Export(typeof(ProjectSummaryViewModel))]
    public class ProjectSummaryViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {
        
        
        private Project project;

        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private IProjectsService projectsService;
        private IProposalsService proposalsService;
        private ITasksService tasksService;
        private IBacklogService backlogService;
        private IDialogService dialogService;
        private IAuthorizationService autorizator;

        private decimal budgetIndicator = 0;
        private decimal qualityIndicator = 0;
        private decimal velocityIndicator = 0;
        private decimal factoryVelocityIndicator = 0;
        private decimal hoursInThisProject;
        private MemberProfile projectScrumMaster;


        private ICollection<ProjectMembership> userEngages = null;

        public ICollection<Task> Tasks { get; set; }

        [Import]
        private IProjectConstraintsService constraintsService { get; set; }


        [Import]
        private CreateRepositoryViewModel CreateRepositoryViewModel { get; set; }

        [Import]
        public ProjectViewModel ProjectDetailViewModel { get; set; }

        [Import]
        public IArtifactsListViewModel ArtifactListViewModel { get; set; }

        [ImportMany]
        public ICodeRepositoryService[] CodeRepositories { get; set; }

       
        [ImportingConstructor]
        public ProjectSummaryViewModel(
            [Import] IEventAggregator aggregator,
            [Import] IBackgroundExecutor executor,
            [Import] IProjectsService projectsService,            
            [Import] IProposalsService proposalsService,
            [Import] ITasksService tasksService,
            [Import] IBacklogService backlogService,
            [Import] IDialogService dialogService,
            [Import] IAuthorizationService autorizatorService) {

                this.aggregator = aggregator;
                this.executor = executor;
                this.projectsService = projectsService;
                this.proposalsService = proposalsService;
                this.tasksService = tasksService;
                this.backlogService = backlogService;
                this.dialogService = dialogService;
                this.autorizator = autorizatorService;

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails, 100);
                aggregator.Subscribe<Project>(ScrumFactoryEvent.ProjectCreated, OnProjectCreation);
                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

                aggregator.Subscribe<string>(ScrumFactoryEvent.ShowCodeRepositoryLog, ShowProjectRepositoryLog);
                aggregator.Subscribe<string>(ScrumFactoryEvent.ShowCodeRepository, p => {
                    if (String.IsNullOrEmpty(p))
                        p = Project.CodeRepositoryPath;
                    OpenPath(p);
                });

                aggregator.Subscribe<string>(ScrumFactoryEvent.ShowDocRepository, p => {
                    if (String.IsNullOrEmpty(p))
                        p = Project.DocRepositoryPath;
                    OpenPath(p);
                });

                aggregator.Subscribe<ICollection<MemberProfile>>(ScrumFactoryEvent.ProjectMembersChanged, m => {                    
                    OnMembersLoaded(m);
                    ((DelegateCommand)SendMailCommand).NotifyCanExecuteChanged();
                });
            
                StartProjectCommand = new DelegateCommand(CanStartProject, StartProject);
                CloseProjectCommand = new DelegateCommand(CanCloseProject, CloseProject);
                EditProjectCommand = new DelegateCommand(CanEditProject, EditProject);

                ShowRepositoryCommand = new DelegateCommand<string>(OpenPath);

                ShowReportCommand = new DelegateCommand<string>(ShowReport);
                
                ShowProjectTeamCommand = new DelegateCommand(()=> { this.aggregator.Publish(ScrumFactoryEvent.ShowProjectTeam); });

                ChangeUserEngageCommand = new DelegateCommand<ProjectMembership>(ChangeUserEngage);

                RemoveUserEngageCommand = new DelegateCommand<ProjectMembership>(RemoveUserEngage);

                SendMailCommand = new DelegateCommand(CanSendMail, SendMail);

                JoinProjectCommand = new DelegateCommand(ShowJoinDialog);

                CreateCodeRepCommand = new DelegateCommand<ICodeRepositoryService>(CreateCodeRep);
        }

        [Import]
        public BurndownViewModel BurndownViewModel { get; set; }

        [Import]
        public HoursGraphViewModel HoursGraphViewModel { get; set; }

        private string RepositoryLogExternalCommand {
            get {
                return Properties.Settings.Default.RepositoryLogExternalCommand;
            }
        }

        private void ShowProjectRepositoryLog(string filter) {

            if (String.IsNullOrEmpty(RepositoryLogExternalCommand))
                return;

            string fullCommand = String.Format(RepositoryLogExternalCommand, Project.CodeRepositoryPath, filter);
            int exeIdx = fullCommand.IndexOf(".exe");

            string command = fullCommand.Substring(0, exeIdx + 4);
            string args = fullCommand.Replace(command, "");

            try {
                System.Diagnostics.Process.Start(command, args);
            } catch (Exception) {

            }
        }

        public string CodeRep {
            get {
                if (project == null)
                    return null;
                return Project.CodeRepositoryPath;
            }
            set {
                if (project == null)
                    return;
                Project.CodeRepositoryPath = value;
                OnPropertyChanged("CodeRep");
            }
        }

        public string DocRep {
            get {
                if (project == null)
                    return null;
                return Project.DocRepositoryPath;
            }
            set {
                if (project == null)
                    return;
                Project.DocRepositoryPath = value;
                OnPropertyChanged("DocRep");
            }
        }

        private void CreateCodeRep(ICodeRepositoryService codeRepService) {
            if (codeRepService == null)
                codeRepService = CodeRepositories[0];
            CreateRepositoryViewModel.Show(Project, codeRepService);
            OnPropertyChanged("CodeRep"); 
        }

       

        private bool burndownTabIsSelected;
        public bool BurndownTabIsSelected {
            get {
                return burndownTabIsSelected;
            }
            set {
                burndownTabIsSelected = value;
                OnPropertyChanged("BurndownTabIsSelected");
            }
        }

        private ICollection<ProjectMembership> EmailMembers {
            get {
                if (Project == null)
                    return new ProjectMembership[0];
                return Project.Memberships
                        .Where(m => m.MemberUId != autorizator.SignedMemberProfile.MemberUId
                        && m.Role.PermissionSet != (int)PermissionSets.PRODUCT_OWNER
                        && m.IsActive==true)
                        .ToArray();
            }
        }

        private bool CanSendMail() {            
            return EmailMembers.Count > 0;
        }

        private void SendMail() {
            string emails = EmailMembers.Select(m => m.Member.EmailAccount).Aggregate((a, b) => a + ", " + b);
            string subject = Project.ClientName + " - " + Project.ProjectName + " (" + Project.ProjectNumber + ") : ";
            System.Diagnostics.Process.Start("mailto:" + emails + "?subject=" + subject);
        }

        private void RemoveUserEngage(ProjectMembership membership) {

            // can only change my engage
            if (autorizator.SignedMemberProfile == null || autorizator.SignedMemberProfile.MemberUId != membership.MemberUId)
                return;

            executor.StartBackgroundTask<ProjectMembership>(
               () => { 
                    projectsService.RemoveProjectMembership(membership.ProjectUId, membership.MemberUId, membership.RoleUId);

                    // if the user already has tasks he will be not removed, just flaged as inative
                    // so need to get the project again to know whenever it was removed or flagged
                    // THE GOOD WAY SHOULD BE the REST return it, but dont want to change Services interface at this point
                    Project p = projectsService.GetProject(Project.ProjectUId);
                    var removed = p.Memberships.Where(m => m.MemberUId == membership.MemberUId && m.RoleUId == membership.RoleUId).SingleOrDefault();
                    if (removed != null)
                        membership.IsActive = removed.IsActive;
                    return membership;
               },
               removedMembership => {
                   UserEngages.Remove(membership);
                   aggregator.Publish<ProjectMembership>(ScrumFactoryEvent.ProjectMembershipRemoved, removedMembership);
               });
        }

        public decimal HoursInThisProject {
            get {
                return hoursInThisProject;
            }
            set
            {
                hoursInThisProject = value;
                OnPropertyChanged("HoursInThisProject");
            }
        }

        private void ChangeUserEngage(ProjectMembership membership) {
            // can only change my engage
            if (autorizator.SignedMemberProfile == null || autorizator.SignedMemberProfile.MemberUId != membership.MemberUId)
                return;

            executor.StartBackgroundTask(
               () => { projectsService.UpdateProjectMembershipAllocation(membership.ProjectUId, membership.MemberUId, membership.RoleUId, (int)membership.DayAllocation); },
               () => { });
        }

        private void ShowJoinDialog() {
            if (autorizator.SignedMemberProfile==null || UserEngages.Any(u => u.MemberUId == autorizator.SignedMemberProfile.MemberUId))
                return;
            aggregator.Publish(ScrumFactoryEvent.ShowJoinDialog);
        }

        public ICollection<ProjectMembership> UserEngages {
            get {
                return userEngages;
            }
            set {
                userEngages = value;
                OnPropertyChanged("UserEngages");
            }
        }

        public MemberProfile ProjectScrumMaster {
            get {
                return projectScrumMaster;
            }
            set {
                projectScrumMaster = value;
                OnPropertyChanged("ProjectScrumMaster");
            }
        }

        public bool IsSuspended {
            get {
                if (Project == null)
                    return false;
                return Project.IsSuspended;
            }
            set {
                executor.StartBackgroundTask(
                    () => { projectsService.ChangeProjectIsSuspended(Project.ProjectUId, value); },
                    () => { 
                        Project.IsSuspended = value;
                        OnPropertyChanged("IsSuspended");
                        aggregator.Publish<Project>(ScrumFactoryEvent.ProjectStatusChanged, Project);
                    }
             );
                
            }
        }

        private void OnMembersLoaded(ICollection<MemberProfile> members) {            
            
            if (Project == null || Project.Memberships == null)
                return;
            
            ProjectMembership scrumMS = Project.Memberships.FirstOrDefault(ms => ms.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER && ms.IsActive);
            if (scrumMS != null)                
                ProjectScrumMaster = members.FirstOrDefault(m => m.MemberUId == scrumMS.MemberUId);
            SetUserEngage();

        }

        public decimal FactoryVelocityIndicator {
            get {
                return factoryVelocityIndicator;
            }
            set {
                factoryVelocityIndicator = value;
                OnPropertyChanged("FactoryVelocityIndicator");
                OnPropertyChanged("VelocityIndicatorStatus");
            }
        }

        public decimal BudgetIndicator {
            get {
                return budgetIndicator;
            }
            set {
                budgetIndicator = value;
                OnPropertyChanged("BudgetIndicator");
                OnPropertyChanged("BudgetIndicatorStatus");
            }
        }

        public int BudgetIndicatorStatus {
            get {
                if (BudgetIndicator < 80)
                    return (int) IndicatorStatus.NORMAL;
                if (BudgetIndicator < 100)
                    return (int)IndicatorStatus.ATTENTION;                
                return (int)IndicatorStatus.OVER;
            }
        }

        public decimal QualityIndicator {
            get {
                return qualityIndicator;
            }
            set {
                qualityIndicator = value;
                OnPropertyChanged("QualityIndicator");
                OnPropertyChanged("QualityIndicatorStatus");
            }
        }

        public int QualityIndicatorStatus {
            get {
                if (QualityIndicator < 5)
                    return (int)IndicatorStatus.NORMAL;
                if (QualityIndicator < 10)
                    return (int)IndicatorStatus.ATTENTION;
                return (int)IndicatorStatus.OVER;
            }
        }

        public decimal VelocityIndicator {
            get {
                return velocityIndicator;
            }
            set {
                velocityIndicator = value;
                OnPropertyChanged("VelocityIndicator");
                OnPropertyChanged("VelocityIndicatorStatus");
            }
        }

        public int VelocityIndicatorStatus {
            get {
                if (VelocityIndicator < factoryVelocityIndicator)
                    return (int)IndicatorStatus.OVER;
                if (VelocityIndicator < factoryVelocityIndicator * 1.1m)
                    return (int)IndicatorStatus.ATTENTION;
                return (int)IndicatorStatus.NORMAL;
            }
        }

        private void OnSignedMemberChanged(MemberProfile member) {
            executor.StartBackgroundTask<decimal>(
                () => { return backlogService.GetVelocityIndicator(); },
                v => {
                    FactoryVelocityIndicator = v;                    
                });
        }
        

        private void EditProject() {
            ProjectDetailViewModel.Show(this);
        }

        private bool CanEditProject() {
            return CanEdit;
        }

        public bool CanEdit {
            get {
                if (Project == null)
                    return false;
                return Project.HasPermission(autorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

    


        private bool CanCloseProject() {
            if (Project == null)
                return false;
            if (Project.Status == (short)ProjectStatus.PROJECT_DONE ||                
                Project.Status == (short)ProjectStatus.PROPOSAL_REJECTED)
                return false;
            return Project.HasPermission(autorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }

        private bool CanStartProject() {
            if (Project == null)
                return false;
            if (Project.Status == (short)ProjectStatus.PROJECT_DONE ||
                Project.Status == (short)ProjectStatus.PROJECT_SUPPORT ||
                Project.Status == (short)ProjectStatus.PROJECT_STARTED ||                
                Project.Status == (short)ProjectStatus.PROPOSAL_REJECTED)
                return false;
            return Project.HasPermission(autorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }

        private void CloseProject() {

            System.Windows.MessageBoxResult result;

            bool hasNotFinishedItems = false;
            ICollection<BacklogItem> items = backlogService.GetCurrentBacklog(Project.ProjectUId, (short)BacklogFiltersMode.PENDING);
            if(items!=null && items.Count>0)
                hasNotFinishedItems = true;
            
            if (!hasNotFinishedItems)
                result = dialogService.ShowMessageBox(
                    Properties.Resources.Close_project,
                    Properties.Resources.Close_project_message,
                    System.Windows.MessageBoxButton.OKCancel,
                    "/Images/Dialogs/close.png");
            else
                result = dialogService.ShowMessageBox(
                    Properties.Resources.Close_project,
                    Properties.Resources.Close_project_message_close_items,
                    System.Windows.MessageBoxButton.YesNoCancel,
                    "/Images/Dialogs/close.png");

            if (result == System.Windows.MessageBoxResult.Cancel)
                return;

            string option = string.Empty;
            if (result == System.Windows.MessageBoxResult.Yes)
                option = "CLOSE_ITEMS";

            executor.StartBackgroundTask(
                () => { return projectsService.ChangeProjectStatus(project.ProjectUId, option, (short)ProjectStatus.PROJECT_DONE); },
                p => {
                    aggregator.Publish<Project>(ScrumFactoryEvent.ProjectStatusChanged, p);
                    aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);
                });    
        }

        private void StartProject() {

            System.Windows.MessageBoxResult result;
            System.DateTime startDate = Project.FirstSprint.StartDate.Date;
            if (startDate.Equals(System.DateTime.Today)) 
                result = dialogService.ShowMessageBox(
                    Properties.Resources.Start_project,
                    Properties.Resources.Start_project_message,
                    System.Windows.MessageBoxButton.OKCancel,
                    "/Images/Dialogs/Start.png");
            else
                result = dialogService.ShowMessageBox(
                    Properties.Resources.Start_project,
                    string.Format(Properties.Resources.Start_project_message_move_date, startDate),
                    System.Windows.MessageBoxButton.YesNoCancel,
                    "/Images/Dialogs/Start.png");

            if (result == System.Windows.MessageBoxResult.Cancel)
                return;

            string option = string.Empty;
            if (result == System.Windows.MessageBoxResult.Yes)
                option = "MOVE_SPRINT_DATE";
                
            executor.StartBackgroundTask(
                () => { 
                    if(!project.IsTicketProject)
                        return projectsService.ChangeProjectStatus(project.ProjectUId, option, (short)ProjectStatus.PROJECT_STARTED);
                    else
                        return projectsService.ChangeProjectStatus(project.ProjectUId, option, (short)ProjectStatus.PROJECT_SUPPORT);
                },
                p => {
                    aggregator.Publish<Project>(ScrumFactoryEvent.ProjectStatusChanged, p);
                    aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);
                });    
            

        }

        /// <summary>
        /// Called when the project details is viewed.
        /// </summary>
        /// <param name="newProject">The project.</param>
        private void OnViewProjectDetails(Project newProject) {

            if (newProject == null) {
                IsVisible = false;
                return;
            }

            
            // everytime the project changes go to summary tab
            if (Project==null || newProject.ProjectUId != Project.ProjectUId) 
                aggregator.Publish<IProjectTabViewModel>(ScrumFactoryEvent.ShowProjectTab, this);
            
            Project = newProject;

            if (justCreatedProject!=null && Project!=null && Project.ProjectUId != justCreatedProject.ProjectUId)
                justCreatedProject = null;

            UpdateHelpStatus();

            BurndownTabIsSelected = true;
            
            LoadBudgetIndicator();
            LoadQualityIndicator();
            LoadVelocityIndicator();

            LoadHoursInThisProject();
            
                        
        }

        private void LoadHoursInThisProject() {
            string memberUId = autorizator.SignedMemberProfile.MemberUId;
            executor.StartBackgroundTask<decimal>(() => {
                var hoursByMember = tasksService.GetProjectTotalEffectiveHoursByMember(project.ProjectUId, memberUId);
                if (hoursByMember == null || !hoursByMember.Keys.Contains(memberUId))
                    return 0;
                return hoursByMember[memberUId];
            },
            hours => {
                HoursInThisProject = hours;
            });

        }

        private void SetUserEngage() {
            if (Project == null)
                return;
            UserEngages = new System.Collections.ObjectModel.ObservableCollection<ProjectMembership>(
                Project.Memberships.Where(m => m.MemberUId == autorizator.SignedMemberProfile.MemberUId && m.IsActive).OrderBy(m => m.Role.PermissionSet));
        }

        private void LoadBudgetIndicator() {
            if (Project == null)
                return;
            executor.StartBackgroundTask<decimal>(
                () => { return proposalsService.GetBudgetIndicator(Project.ProjectUId); },
                i => { BudgetIndicator = i; }
            );
        }

        private void LoadQualityIndicator() {
            if (Project == null)
                return;
            executor.StartBackgroundTask<decimal>(
                () => { return tasksService.GetReworkIndicator(Project.ProjectUId); },
                i => { QualityIndicator = i; }
            );
        }

        private void LoadVelocityIndicator() {
            if (Project == null)
                return;
            executor.StartBackgroundTask<decimal>(
                () => { return backlogService.GetProjectVelocityIndicator(Project.ProjectUId); },
                i => { VelocityIndicator = i; }
            );
        }

        private Project justCreatedProject = null;

        public string CurrentSprintLabel {
            get {
                if (Project == null)
                    return null;

                if (justCreatedProject!=null)
                    return Properties.Resources.Project_created;

                if (Project.Sprints == null || Project.Sprints.Count == 0)
                    return null;

                if (Project.Status == (short)ProjectStatus.PROJECT_DONE)
                    return Properties.Resources.Project_finished;

                if (Project.LastSprint.EndDate < System.DateTime.Today)
                    return Properties.Resources.Project_expired;

                if (!Project.IsRunning)
                    return Properties.Resources.Project_not_started;

                if (Project.IsRunning) {
                    if (Project.IsTicketProject)
                        return "";
                    if (Project.CurrentSprint != null) {                        
                        return string.Format(Properties.Resources.N_days_to_end_sprint_N_of_N, Project.CurrentSprint.DaysLeft, Project.CurrentSprint.SprintNumber, Project.Sprints.Count);
                    }
                    return Properties.Resources.Sprint_1_not_started_yet;
                }

                return null;
            }
        }

        /// <summary>
        /// Called when a project is created.
        /// </summary>
        /// <param name="project">The created project.</param>
        private void OnProjectCreation(Project project) {
            aggregator.Publish<IProjectTabViewModel>(ScrumFactoryEvent.ShowProjectTab, this);
            justCreatedProject = project;
            UpdateHelpStatus();
        }

        private ReportHelper.ReportConfig PrepareReportCheckingServerVersion(string reportTemplate, string title) {
            ReportHelper.ReportConfig config = null;
            try {
                config = new ReportHelper.ReportConfig("SprintReview", reportTemplate, title);
                PrepareReport(config);
            } catch (Exception) {
                dialogService.ShowAlertMessage("Wrong server version", "Please update your server to 3.1 version in order to access this report", null);
                return null;
            }

            return config;
        }

        private void PrepareReport(ReportHelper.ReportConfig config) {
            // add project
            config.ReportObjects.Add(Project);

            // add risks
            ICollection<Risk> risks = projectsService.GetProjectRisks(Project.ProjectUId);
            config.ReportObjects.Add(risks);
            
            // add itens
            ICollection<BacklogItem> items = backlogService.GetBacklog(Project.ProjectUId, null, (short)ScrumFactory.Services.BacklogFiltersMode.ALL);

            foreach (BacklogItem item in items) {
                item.ValidPlannedHours = item.GetValidPlannedHours();
            }

            config.ReportObjects.Add(items);

            // add constraints
            ICollection<ProjectConstraint> constraints = constraintsService.GetProjectConstraints(Project.ProjectUId);
            config.ReportObjects.Add(constraints);
            
            
            // add end date
            config.AddReportVar("ProjectEndDate", Project.LastSprint.EndDate);

        }

        private void UpdateHelpStatus() {
            OnPropertyChanged("ShowHelp_CreateProject");
            OnPropertyChanged("IsThereAnyHelp");            
        }
        
        public bool ShowHelp_CreateProject {
            get {
                return justCreatedProject!=null;
            }        
        }

        public bool IsThereAnyHelp {
            get {
                return ShowHelp_CreateProject;
            }
        }

        private void ShowProjectGuideReport() {

            ReportHelper.ReportConfig config = PrepareReportCheckingServerVersion("guide", Properties.Resources.Project_guide_report);
            if (config == null)
                return;

            // add groups
            ICollection<BacklogItemGroup> groups = backlogService.GetBacklogItemGroups(Project.ProjectUId);
            config.ReportObjects.Add(groups);

            aggregator.Publish<ScrumFactory.ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, config);
        }

        private void ShowScopeReport() {
            
            ReportHelper.ReportConfig config = PrepareReportCheckingServerVersion("scope", Properties.Resources.Project_scope_report);
            if (config == null)
                return;

            // add groups
            ICollection<BacklogItemGroup> groups = backlogService.GetBacklogItemGroups(Project.ProjectUId);
            config.ReportObjects.Add(groups);

            aggregator.Publish<ScrumFactory.ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, config);
        }

        private void ShowReport(string report) {
            if (report == "REVIEW")
                ShowReviewReport();
            if (report == "INDICATORS")
                ShowIndicatorsReport();
            if (report == "SCOPE")
                ShowScopeReport();
            if (report == "GUIDE")
                ShowProjectGuideReport();

        }

        private void ShowIndicatorsReport() {

            ReportHelper.ReportConfig config = PrepareReportCheckingServerVersion("indicators", Properties.Resources.Review_report);
            if (config == null)
                return;

            // add burndown
            config.ReportViewModels.Add("burndown", BurndownViewModel);

            // add hours graph
            HoursGraphViewModel.LoadData();
            config.ReportViewModels.Add("hoursgraph", HoursGraphViewModel);

            // add indicators
            config.ReportVars.Add("BudgetIndicator", BudgetIndicator.ToString("0.0") + " %");
            config.ReportVars.Add("BudgetIndicatorStatus", BudgetIndicatorStatus.ToString());
            config.ReportVars.Add("QualityIndicator", QualityIndicator.ToString("0.0") + " %");
            config.ReportVars.Add("QualityIndicatorStatus", QualityIndicatorStatus.ToString());
            config.ReportVars.Add("VelocityIndicator", VelocityIndicator.ToString("0.0") + " pts/hrs");
            config.ReportVars.Add("VelocityIndicatorStatus", VelocityIndicatorStatus.ToString());
            
            

            aggregator.Publish<ScrumFactory.ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, config);
        }

        private void ShowReviewReport() {

            ReportHelper.ReportConfig config = PrepareReportCheckingServerVersion("default", Properties.Resources.Review_report);
            if (config == null)
                return;
            
            // add burndown
            config.ReportViewModels.Add("burndown", BurndownViewModel);

            // add current and previous sprint numbers
            if (Project.CurrentSprint != null) {                
                if(Project.CurrentSprint.SprintNumber > 1) {
                    config.ReportVars.Add("ProjectCurrentSprintNumber", Project.CurrentSprint.SprintNumber.ToString());
                    config.ReportVars.Add("ProjectPreviousSprintNumber", (Project.CurrentSprint.SprintNumber - 1).ToString());
                }
                else {
                    if(Project.Sprints.Count > Project.CurrentSprint.SprintNumber + 1)
                        config.ReportVars.Add("ProjectCurrentSprintNumber", (Project.CurrentSprint.SprintNumber + 1).ToString());
                    else
                        config.ReportVars.Add("ProjectCurrentSprintNumber", Project.CurrentSprint.SprintNumber.ToString());

                    config.ReportVars.Add("ProjectPreviousSprintNumber", Project.CurrentSprint.SprintNumber.ToString());
                }
            }

            aggregator.Publish<ScrumFactory.ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, config);
        }

        private void OpenPath(string path) {
            try {
                System.Diagnostics.Process.Start(path);
            }
            catch (System.Exception) {
                System.Windows.MessageBox.Show(String.Format(Properties.Resources.Could_not_open_path_N, path));
                
            }
        }
     
        #region IProjectSummaryViewModel Members

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>The project.</value>
        public Project Project {
            get {
                return project;
            }
            private set {
                project = value;

                IsVisible = (project != null);

                OnPropertyChanged("Project");
                OnPropertyChanged("IsSuspended");
                OnPropertyChanged("CurrentSprintLabel");
                OnPropertyChanged("CanEdit");
                OnPropertyChanged("CodeRep");
                OnPropertyChanged("DocRep");

                (CloseProjectCommand as DelegateCommand).NotifyCanExecuteChanged();
                (StartProjectCommand as DelegateCommand).NotifyCanExecuteChanged();
                (EditProjectCommand as DelegateCommand).NotifyCanExecuteChanged();
                (SendMailCommand as DelegateCommand).NotifyCanExecuteChanged();

                ArtifactListViewModel.ChangeContext(ArtifactContexts.PROJECT_ARTIFACT, project.ProjectUId);
            }
        }

        


        /// <summary>
        /// Gets the view for this View Model.
        /// </summary>
        /// <value>The view.</value>
        [Import(typeof(ProjectSummary))]
        public IView View { get; set; }

        

        /// <summary>
        /// Gets or sets the start project command.
        /// </summary>
        /// <value>The start project command.</value>
        public ICommand StartProjectCommand { get; set; }

        /// <summary>
        /// Gets or sets the close project command.
        /// </summary>
        /// <value>The close project command.</value>
        public ICommand CloseProjectCommand { get; set; }

        /// <summary>
        /// Gets or sets the edit project command.
        /// </summary>
        /// <value>The edit project command.</value>
        public ICommand EditProjectCommand { get; set; }

        public ICommand ShowProjectTeamCommand { get; set; }

        public ICommand ShowReportCommand { get; set; }
        
        public ICommand ShowRepositoryCommand { get; set; }

        public ICommand ChangeUserEngageCommand { get; set; }

        public ICommand RemoveUserEngageCommand { get; set; }

        public ICommand SendMailCommand{ get; set; }

        public ICommand JoinProjectCommand { get; set; }

        public ICommand CreateCodeRepCommand { get; set; }

        #endregion

        #region IPanelViewModel Members



        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.Summary;                
            }
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

        private bool isVisible = false;
        public bool IsVisible {
            get {
                return isVisible;
            }
            set {
                isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public bool IsEspecialTab {
            get {
                return false;
            }
        }



        #endregion
    }
}
