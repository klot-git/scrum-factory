using System.Collections.Generic;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition.View;
using System.Windows.Shell;
using System.Reflection;
using System.Windows;
using System.Deployment.Application;




namespace ScrumFactory.Windows.ViewModel {
    

    [Export]
    public class ShellViewModel : INotifyPropertyChanged {

        private Project selectedProject;

        
        private List<ITopMenuViewModel> topMenuItems;

        private IBackgroundExecutor executor;
        private IDialogService dialogs;
        private IEventAggregator aggregator;
        private Services.IAuthorizationService authorizator;
        

        private System.Collections.Specialized.NameValueCollection args;

        private List<ProjectInfo> recentProjects;
        private JumpList jumpList = new JumpList();

        private bool isSigning;

        [Import]
        private OptionsViewModel OptionsViewModel { get; set; }

        [Import]
        private IFactoryServerConfigurationViewModel ServerConfigurationViewModel { get; set; }

        [Import]
        private IProjectContainer ProjectContainer { get; set; }


        [Import]
        private Services.ICalendarService calendar { get; set; }

        [ImportingConstructor()]
        public ShellViewModel(            
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogs,
            [Import] Services.IAuthorizationService authorizator) {

                AlertMessageViewModel = new MessageBoxViewModel();

                this.executor = backgroundExecutor;
                this.aggregator = eventAggregator;
                this.dialogs = dialogs;
                this.authorizator = authorizator;
                
                signinMenuText = Properties.Resources.Sign_in;

                aggregator.Subscribe(ScrumFactoryEvent.ShowFullScreen, () => { IsOnTaskMode = false; });
                aggregator.Subscribe<Task>(ScrumFactoryEvent.ShowTaskDetail, t => { IsOnTaskMode = false; }, 10);

                //aggregator.Subscribe(ScrumFactoryEvent.ApplicationWhentForeground, () => { IsOnTaskMode = true; });

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnMemberSignin);

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ProjectDetailsChanged, p => { SelectedProject = p; });


                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, ViewProjectDetails);

                aggregator.Subscribe<bool>(ScrumFactoryEvent.Signing,
                    signing => {
                        if (signing) {
                            IsSigning = true;
                            SigninMenuText = Properties.Resources.Sigining;
                        }
                        else {
                            IsSigning = false;
                            SigninMenuText = Properties.Resources.Sign_in;
                        }
                    });

                SignInCommand = new DelegateCommand(() => {
                    //CloseAllChildWindows();
                    eventAggregator.Publish(ScrumFactoryEvent.ShowLogin);
                });

           

                MoveWindowCommand = new DelegateCommand(() => { View.DragMove(); });
                CloseWindowCommand = new DelegateCommand(Close);
                MinimizeWindowCommand = new DelegateCommand(Minimize);                

                ShowMyProfileCommand = new DelegateCommand(ShowMyProfile);

                ShowAboutDialogCommand = new DelegateCommand(ShowAboutDialog);

                ShowWhatIsNewCommand = new DelegateCommand(ShowWhatIsNewDialog);

                ShowOwnersListCommand = new DelegateCommand(CanShowOwnersList, ShowOwnersList);

                RefreshCommand = new DelegateCommand(Refresh);

                ShowHideTopMenuCommand = new DelegateCommand(ShowHideTopMenu);

                ShowMyTasksCommand = new DelegateCommand(ShowMyTasks);

                ShowOptionsCommand = new DelegateCommand(ShowOptions);

                ShowFactorySettingsCommand = new DelegateCommand(CanShowOwnersList, ShowFactorySettings);



        }

        private void ShowFactorySettings() {
            ServerConfigurationViewModel.Show();
        }

        private void LoadHolidays() {            
            executor.StartBackgroundTask(() => { calendar.LoadHolidays();},
            () => {
                ScrumFactory.Windows.Helpers.Converters.HolidayConverter.Holidays = calendar.Holidays;
            });
        }


        private void ShowOptions() {
            OptionsViewModel.Show();
        }

        private bool isOnTaskMode = false;
        public bool IsOnTaskMode {
            get {
                return isOnTaskMode;
            }
            set {
                isOnTaskMode = value;
                OnPropertyChanged("IsOnTaskMode");
                if (isOnTaskMode) {
                    IsTaskListVisible = true;
                    ((MainWindow)View).SetTaskModeWindow();
                } else {
                    IsTaskListVisible = false;
                    ((MainWindow)View).SetFullModeWindow();

                }
            }
        }

        private bool isTaskListVisible = false;
        public bool IsTaskListVisible {
            get {
                return isTaskListVisible;
            }
            set {
                isTaskListVisible = value;
                OnPropertyChanged("IsTaskListVisible");                
            }
        }

        private void Minimize() {            
            View.WindowState = System.Windows.WindowState.Minimized;
        }

        private void ShowMyTasks() {

            // if is on task mode, go to Full Mode
            if (IsOnTaskMode) {
                IsOnTaskMode = false;
                return;
            }

            IsTaskListVisible = !IsTaskListVisible;
            if (IsTaskListVisible)
                aggregator.Publish(ScrumFactoryEvent.ShowUserTasksSelector);
        }

        private void ShowHideTopMenu() {
            if (SelectedProject == null) {
                ShowTopMenu = true;
                return;
            }
            ShowTopMenu = !ShowTopMenu;
        }

        private void Close() {
            AnswerSurvey();
            View.Close();
        }

        [Import]
        public IUserTasksSelectorViewModel TaskSelector { get; set; }
        
        [Import]
        private IServerUrl ServerUrl { get; set; }

        public string ImageLogoUrl {
            get {
                return "http://www.scrum-factory.com/logoimage.aspx?a=" + Uri.EscapeDataString(ServerUrl.Url);
            }
        }

        private void AnswerSurvey() {
            if (Properties.Settings.Default.SurveyAnswered)
                return;

            if (ServerUrl!=null && ServerUrl.Url!=null &&  ServerUrl.Url.ToLower().Contains("foster"))
                return;

            System.Windows.MessageBoxResult d = 
                dialogs.ShowMessageBox(Properties.Resources.Feedback_survey, Properties.Resources.Feedback_survey_text, System.Windows.MessageBoxButton.YesNo, "/Images/question.png");

            if (d == System.Windows.MessageBoxResult.No)
                return;

            Properties.Settings.Default.SurveyAnswered = true;
            Properties.Settings.Default.Save();

            System.Diagnostics.Process.Start("http://www.surveymonkey.com/s/DZK3XGW");

           

        }

        public bool IsSigning {
            get {
                return isSigning;
            }
            set {
                isSigning = value;
                OnPropertyChanged("IsSigning");
            }
        }

        private void AddToRecentProjects(Project project) {
            if (project == null)
                return;

            ProjectInfo alreadyThere = RecentProjects.SingleOrDefault(p => p.ProjectUId == project.ProjectUId);

            if (alreadyThere != null) {
                int idx = RecentProjects.IndexOf(alreadyThere);
                RecentProjects.Remove(alreadyThere);

                if(idx >=0 && idx < jumpList.JumpItems.Count)
                    jumpList.JumpItems.RemoveAt(idx);
            }
            
            ProjectInfo pInfo = new ProjectInfo(project);
            jumpList.JumpItems.Insert(0, CreateProjectJumpItem(pInfo));
            RecentProjects.Insert(0, pInfo);

            if (RecentProjects.Count > 10) {
                int toBeRemoved = RecentProjects.Count - 10;
                RecentProjects.RemoveRange(10, toBeRemoved);                
            }

            if (jumpList.JumpItems.Count > 10) {
                int toBeRemoved = jumpList.JumpItems.Count - 10;                
                jumpList.JumpItems.RemoveRange(10, toBeRemoved);
            }

            aggregator.Publish<ICollection<ProjectInfo>>(ScrumFactoryEvent.RecentProjectChanged, RecentProjects);

            jumpList.Apply();

        }

        public void SaveRecentProjects() {
            Properties.Settings.Default.RecentProjects = RecentProjects.ToArray();
            Properties.Settings.Default.Save();
        }

        private void LoadRecentProjects() {            
            if(Properties.Settings.Default.RecentProjects!=null)
                RecentProjects = new List<ProjectInfo>(Properties.Settings.Default.RecentProjects);            
            else
                RecentProjects = new List<ProjectInfo>();            
        }

        public List<ProjectInfo> RecentProjects {
            get {
                return recentProjects;
            }
            set {
                recentProjects = value;
                OnPropertyChanged("RecentProjects");
                aggregator.Publish<ICollection<ProjectInfo>>(ScrumFactoryEvent.RecentProjectChanged, recentProjects);
            }
        }

        private void Refresh() {
            if (SelectedTopMenuItem.PanelPlacement != PanelPlacements.Project)
                return;
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, SelectedProject);
        }

        private void MainWindow_StateChanged(object sender, System.EventArgs e) {
            if (System.Windows.Application.Current.MainWindow.WindowState != System.Windows.WindowState.Minimized)
                aggregator.Publish(ScrumFactoryEvent.ApplicationWhentForeground);
            else {
                IsOnTaskMode = true;
                aggregator.Publish(ScrumFactoryEvent.ApplicationWhentBackground);
            }

            
        }

        private bool showTopMenu = true;
        public bool ShowTopMenu {
            get {
                return showTopMenu;
            }
            set {
                showTopMenu = value;
                
                OnPropertyChanged("ShowTopMenu");
            }
        }

        [Import]
        private AboutViewModel AboutDialogViewModel { get; set; }

        private void ShowAboutDialog() {            
            IDialogViewModel about = dialogs.NewDialog(Properties.Resources.About_Scrum_Factory, AboutDialogViewModel.View);
            about.Show();

        }

     
        [Import]
        private WhatIsNewViewModel WhatIsNewViewModel { get; set; } 

        private void ShowWhatIsNewDialog(){
            SelectedTopMenuItem = WhatIsNewViewModel;
        }

        private void ShowOwnersList() {
            aggregator.Publish(ScrumFactoryEvent.ShowOwnersList);
        }

        private bool CanShowOwnersList() {
            if (authorizator == null || authorizator.SignedMemberProfile == null)
                return false;
            return authorizator.SignedMemberProfile.IsFactoryOwner;        
        }

  
        private string signinMenuText;
        public string SigninMenuText {
            get {
                return signinMenuText;
            }
            set {
                signinMenuText = value;
                OnPropertyChanged("SigninMenuText");
            }
        }

        private MemberProfile signedMember = null;

        public MemberProfile SignedMember {
            get {
                return signedMember;
            }
            set {
                signedMember = value;                
                OnPropertyChanged("SignedMember");
                OnPropertyChanged("SignedMemberName");
                OnPropertyChanged("IsMemberSigned");
                OnPropertyChanged("SignedMemberUId");
            }
        }

        

        public string SignedMemberName {
            get {
                if (SignedMember == null)
                    return null;

                //if (!String.IsNullOrEmpty(SignedMember.FullName))
                //    return SignedMember.FullName;

                if (!String.IsNullOrEmpty(SignedMember.MemberUId))
                    return SignedMember.MemberUId;

                return Properties.Resources.My_profile;
                    
            }
        }

        private void OnMemberSignin(MemberProfile member) {            
            SignedMember = member;
            if (SignedMember != null) 
                ShowTopMenu = true;                
            else 
                ShowTopMenu = false;
          
            ((DelegateCommand)ShowOwnersListCommand).NotifyCanExecuteChanged();

            LoadHolidays();
        }
        
        private void ShowMyProfile() {
            dialogs.SetBackTopMenu();
            aggregator.Publish(ScrumFactoryEvent.ShowProfile);
        }

       

        public bool IsMemberSigned {
            get {
                return SignedMember != null;
            }
        }
        

        /// <summary>
        /// Gets and sets the selected project.
        /// </summary>
        public Project SelectedProject {
            get {
                return this.selectedProject;
            }
            private set {
                this.selectedProject = value;
                OnPropertyChanged("SelectedProject");
                OnPropertyChanged("WindowTitle");                                               
            }
        }

        private void ViewProjectDetails(Project p) {

            SelectedProject = p;
            ShowTopMenu = false;
            AddToRecentProjects(p);
        }


        private MessageBoxViewModel alertMessageViewModel;
        public MessageBoxViewModel AlertMessageViewModel {
            get {
                return alertMessageViewModel;
            }
            set {
                alertMessageViewModel = value;
                OnPropertyChanged("AlertMessageViewModel");
            }
        }

        public void HandleScrumFactoryException(ScrumFactory.Exceptions.ScrumFactoryException ex) {
            if (ex is ScrumFactory.Exceptions.NotAuthorizedException)
                aggregator.Publish(ScrumFactoryEvent.NotAuthorizedServerExceptionThrowed);                
            else
                ShowAlertMessage(ex);
        }


        private void ShowAlertMessage(ScrumFactory.Exceptions.ScrumFactoryException ex) {

            if (!ex.IsLocalized)
                ex.LocalizeException(Properties.Resources.ResourceManager);

            string exceptionType = ex.GetType().ToString().Replace('.', '_');
            string imageSource = Properties.Resources.ResourceManager.GetString(exceptionType + "_errorImageSource");
            if (imageSource != null)
                AlertMessageViewModel.ImageSource = Properties.Resources.ResourceManager.GetString(exceptionType + "_errorImageSource");
            else
                AlertMessageViewModel.ResetErrorImage();

            AlertMessageViewModel.Title = ex.LocalizedMessageTitle;
            AlertMessageViewModel.Message = ex.LocalizedMessage;

            ShowTopMenu = true;
            AlertMessageViewModel.Show();                        
        }

        internal void ShowAlertMessage(string title, string message, string imageSource) {

            if (imageSource != null)
                AlertMessageViewModel.ImageSource = imageSource;
            else
                AlertMessageViewModel.ResetErrorImage();

            AlertMessageViewModel.Title = title;
            AlertMessageViewModel.Message = message;

            ShowTopMenu = true;
            AlertMessageViewModel.Show();
        }

        internal void CloseAlertMessage() {
            alertMessageViewModel.CloseWindowCommand.Execute(null);
        }



        [Import(typeof(MainWindow))]
        public IDialogView View
        {
            get;
            set;
        }

       

        public string WindowTitle {
            get {
                if (SelectedProject == null)
                    return "The Scrum Factory";
                return SelectedProject.ClientName + " - " +  SelectedProject.ProjectName + " [" + SelectedProject.ProjectNumber + "]";
            }
        }
       
        private ITopMenuViewModel backTopMenu = null;        
        public void SetBackTopMenu(ITopMenuViewModel viewModel) {
            backTopMenu = viewModel;
           
        }

        public void GoBackSelectedTopMenu() {
            if(backTopMenu==null)
                return;
            SelectedTopMenuItem = backTopMenu;
            backTopMenu = null;
            
        }


        public void SetTopMenu(ITopMenuViewModel viewmodel, bool cleanBack = true) {
            ITopMenuViewModel back = backTopMenu;
            SelectedTopMenuItem = viewmodel;
            if (!cleanBack)
                SetBackTopMenu(back);
        }

        private ITopMenuViewModel selectedTopMenuItem;
        public ITopMenuViewModel SelectedTopMenuItem {
            get {
                return selectedTopMenuItem;
            }
            set {             
                selectedTopMenuItem = value;
                backTopMenu = null;
                OnPropertyChanged("SelectedTopMenuItem");
            }
        }

        public void GoToFirstTopMenu() {
            if (TopMenuItems == null)
                return;
            SelectedTopMenuItem = TopMenuItems.First();
        }


        [ImportMany(typeof(ITopMenuViewModel))]
        public IEnumerable<ITopMenuViewModel> TopMenuItems {
            get {
                return topMenuItems;
            }
            set {                
                topMenuItems = value.OrderBy(p => p.PanelDisplayOrder).ToList();
                OnPropertyChanged("TopMenuItems");
            }
        }

        public void Show(System.Collections.Specialized.NameValueCollection args) {
            this.args = args;
            
            System.Windows.Application.Current.MainWindow = (System.Windows.Window) this.View;
            System.Windows.Application.Current.MainWindow.StateChanged -= new System.EventHandler(MainWindow_StateChanged);
            System.Windows.Application.Current.MainWindow.StateChanged += new System.EventHandler(MainWindow_StateChanged);
            System.Windows.Application.Current.MainWindow.Activate();
            this.View.Show();


            LoadRecentProjects();

            CreateJumpList();

            string   Version = "DEV";
            if (ApplicationDeployment.IsNetworkDeployed)
                Version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

            if (Version != Properties.Settings.Default.VersionWhatIsNew || Version=="DEV"){
                Properties.Settings.Default.VersionWhatIsNew = Version;
                Properties.Settings.Default.Save();
                ShowWhatIsNewDialog();
            }

            HandleBeforeStartArgs();

            aggregator.Publish(ScrumFactoryEvent.ShellStarted);

            HandleArgs();


            
        }

        private void HandleBeforeStartArgs() {
            if (args == null)
                return;

            string server = args["server"];
            if (!string.IsNullOrEmpty(server))
                aggregator.Publish<string>(ScrumFactoryEvent.ServerArgOnInit, server);
        }

        private void HandleArgs() {
            if (args == null)
                return;
                        
            
            string projectNumber = args["projectNumber"];
            if (!string.IsNullOrEmpty(projectNumber)) {
                int number;
                int.TryParse(projectNumber, out number);
                if(number>0)
                    aggregator.Publish<int>(ScrumFactoryEvent.ProjectArgOnInit, number);
            }

            string taskNumber = args["taskNumber"];
            if (!string.IsNullOrEmpty(taskNumber)) {
                int number;
                int.TryParse(taskNumber, out number);
                if (number > 0)
                    aggregator.Publish<int>(ScrumFactoryEvent.TaskArgOnInit, number);
            }
           
        }


        private void CreateJumpList() {
            JumpList.SetJumpList(System.Windows.Application.Current, jumpList);

            foreach (ScrumFactory.ProjectInfo pInfo in RecentProjects) {
                JumpTask goToProject = CreateProjectJumpItem(pInfo);
                jumpList.JumpItems.Add(goToProject);
            }

            jumpList.Apply();
        }

        private JumpTask CreateProjectJumpItem(ProjectInfo pInfo) {
            JumpTask goToProject = new JumpTask();
            goToProject.CustomCategory = ScrumFactory.Windows.Properties.Resources.Recent_projects;
            goToProject.Title = pInfo.ProjectName + " - " + pInfo.ClientName + " [" + pInfo.ProjectNumber + "]";
            goToProject.Description = String.Format(ScrumFactory.Windows.Properties.Resources.Go_to_project_N, pInfo.ProjectNumber);
            goToProject.ApplicationPath = Assembly.GetEntryAssembly().Location;
            goToProject.IconResourcePath = goToProject.ApplicationPath;
            goToProject.Arguments = "projectNumber=" + pInfo.ProjectNumber;
            return goToProject;
        }

        public ICommand MoveWindowCommand { get; private set; }

        public ICommand CloseWindowCommand { get; private set; }

        public ICommand MinimizeWindowCommand { get; private set; }

        public ICommand MaximizeWindowCommand { get; private set; }

        public ICommand SignInCommand { get; private set; }

        public ICommand ShowMyProfileCommand { get; private set; }

        public ICommand ShowAboutDialogCommand { get; private set; }

     

        public ICommand ShowWhatIsNewCommand { get; private set; }

        public ICommand ShowOwnersListCommand { get; private set; }

        public ICommand RefreshCommand { get; set; }

    

        public ICommand ShowHideTopMenuCommand { get; set; }

        public ICommand ShowMyTasksCommand { get; set; }

        public ICommand ShowOptionsCommand { get; set; }

        public ICommand ShowFactorySettingsCommand { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        
    }
}
