using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using ScrumFactory.Windows.Helpers.Extensions;
using ScrumFactory.Extensions;

namespace ScrumFactory.Team.ViewModel {

    /// <summary>
    /// Project Team View Model.
    /// </summary>
    [Export(typeof(IProjectTabViewModel))]
    [Export(typeof(ProjectTeamViewModel))]    
    public class ProjectTeamViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {


        private IEventAggregator aggregator;
        private ITeamService teamServices;
        private IProjectsService projectsServices;
        private ITasksService taskServices;
        private IBackgroundExecutor executor;
        private IAuthorizationService authorizator;

        private IDialogService dialogs;

        private System.Windows.Data.CollectionViewSource membershipViewSource;

        private ICollection<MemberViewModel> members;

        private Role newRole;


        [Import]
        private MemberViewModel MemberViewModel { get; set; }

        [ImportingConstructor]
        public ProjectTeamViewModel(
            [Import]IEventAggregator eventAggregator,
            [Import]IProjectsService projectsServices,
            [Import]ITeamService teamServices,
            [Import]ITasksService taskServices,
            [Import]IBackgroundExecutor backgroundExecutor,
            [Import] IDialogService dialogs,
            [Import]IAuthorizationService authorizationService) {

            this.taskServices = taskServices;
            this.aggregator = eventAggregator;
            this.projectsServices = projectsServices;
            this.teamServices = teamServices;
            this.executor = backgroundExecutor;
            this.authorizator = authorizationService;
            this.dialogs = dialogs;

            ShowRolesListCommand = new DelegateCommand(CanShowRoleList, ShowRolesList);
            AddNewMemberCommand = new DelegateCommand(CanAddNewMember, AddNewMember);
            
            RemoveMemberCommand = new DelegateCommand<ProjectMembershipViewModel>(CanRemoveMember, RemoveMember);            
      
            ShowJoinDialogCommand = new DelegateCommand(CanJoinProject, ShowJoinDialog);

            ShowContactListCommand = new DelegateCommand(ShowContactList);

            SelectNewRoleCommand = new DelegateCommand<Role>(SelectNewRole);

            membershipViewSource = new System.Windows.Data.CollectionViewSource();            
            membershipViewSource.SortDescriptions.Add(new SortDescription("SortPriority", ListSortDirection.Ascending));
            membershipViewSource.SortDescriptions.Add(new SortDescription("ProjectMembership.Role.RoleName", ListSortDirection.Ascending));
            membershipViewSource.SortDescriptions.Add(new SortDescription("Member.FullName", ListSortDirection.Ascending));

            membershipViewSource.Filter += new System.Windows.Data.FilterEventHandler(membershipViewSource_Filter);            


            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
          
            aggregator.Subscribe<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, roles => { OnPropertyChanged("Roles"); });
            aggregator.Subscribe<Role>(ScrumFactoryEvent.ProjectRoleChanged, role => { membershipViewSource.View.Refresh(); });

            aggregator.Subscribe(ScrumFactoryEvent.ShowProjectTeam, () => { aggregator.Publish<IProjectTabViewModel>(ScrumFactoryEvent.ShowProjectTab, this); });

            // need thi when membership is removed from the project module
            aggregator.Subscribe<ProjectMembership>(ScrumFactoryEvent.ProjectMembershipRemoved, AfterRemoveMember);

            MemberCustomFilter = MemberFilter;

            RefreshMemberFilter = LoadMembers;

        }

        /// <summary>
        /// Gets whenever the add member panel should be visible.
        /// </summary>
        public bool IsAddMemberVisible {
            get {
                if (Project == null)
                    return false;
                if (authorizator == null || authorizator.SignedMemberProfile == null)
                    return false;
                return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);                 
            }
        }

     
      
        private void SelectNewRole(Role role){
            NewRole = role;
        }


        public Role NewRole {
            get {
                return newRole;
            }
            set {
                newRole = value;
                OnPropertyChanged("NewRole");
                OnPropertyChanged("ShowAddMemberBox");
            }
        }

        internal void JoinProject(Role role, int allocation) {

            // creates a viewmodel jut to define the avatar URL
            MemberViewModel memberVM = new MemberViewModel(
                authorizator.SignedMemberProfile,
                ServerUrl,
                authorizator);
    
            AddProjectMembership(memberVM.MemberProfile, role, allocation);            
        }

        private bool IsSignedMemberAtProject {
            get {
                if (Project == null)
                    return false;                
                return Project.Memberships.Any(ms => ms.MemberUId == authorizator.SignedMemberProfile.MemberUId && ms.IsActive==true);
            }
        }

        private bool IsSignedMemberScrumMaster {
            get {
                if (Project == null)
                    return false;
                return Project.Memberships.Any(ms => ms.MemberUId == authorizator.SignedMemberProfile.MemberUId && ms.IsActive == true && ms.Role.PermissionSet==(short)PermissionSets.SCRUM_MASTER);
            }
        }

        private bool CanJoinProject() {
            if (Project==null || !Project.AnyoneCanJoin)
                return false;

            if (authorizator!=null && authorizator.SignedMemberProfile!=null && authorizator.SignedMemberProfile.IsFactoryOwner && !IsSignedMemberScrumMaster)
                return true;

            return !IsSignedMemberAtProject;
        }

        private void ShowJoinDialog() {
            aggregator.Publish(ScrumFactoryEvent.ShowJoinDialog);
        }

        public System.Action<string> RefreshMemberFilter { get; set; }

        public ICollection<MemberViewModel> Members {
            get {
                return members;
            }
            set {
                members = value;
                OnPropertyChanged("Members");
            }
        }

        private void RemoveMember(ProjectMembershipViewModel membershipVM) {
            executor.StartBackgroundTask<ProjectMembership>(
                () => {
                    projectsServices.RemoveProjectMembership(
                        membershipVM.ProjectMembership.ProjectUId,
                        membershipVM.ProjectMembership.MemberUId,
                        membershipVM.ProjectMembership.RoleUId);

                    // if the user already has tasks he will be not removed, just flaged as inative
                    // so need to get the project again to know whenever it was removed or flagged
                    // THE GOOD WAY SHOULD BE the REST return it, but dont want to change Services interface at this point
                    Project p = projectsServices.GetProject(Project.ProjectUId);
                    var removed = p.Memberships.Where(m => m.MemberUId == membershipVM.ProjectMembership.MemberUId && m.RoleUId == membershipVM.ProjectMembership.RoleUId).SingleOrDefault();
                    if (removed != null)
                        membershipVM.ProjectMembership.IsActive = removed.IsActive;
                    return membershipVM.ProjectMembership;

                }, 
                AfterRemoveMember);            
        }

        private void AfterRemoveMember(ProjectMembership membership) {

            // finds membership vm
            Collection<ProjectMembershipViewModel> memberships = membershipViewSource.Source as Collection<ProjectMembershipViewModel>;
            ProjectMembershipViewModel membershipVM = memberships.FirstOrDefault(ms => ms.ProjectMembership.MemberUId == membership.MemberUId && ms.ProjectMembership.RoleUId == membership.RoleUId && ms.ProjectMembership.ProjectUId == membership.ProjectUId);
            if (membershipVM == null)
                return;

            // if membership still there and just turned inactive, just refreshs the view
            if (membership.IsActive==false) {
                membershipVM.ProjectMembership.IsActive = membership.IsActive;
                membershipViewSource.View.Refresh();
                PublishMemberChanged();
                return;
            }

            // refresh the view
            // and removes from the collection (i dont now why we need to do this one, the statment above should have done that)
            ((IEditableCollectionView)membershipViewSource.View).Remove(membershipVM);
            memberships.Remove(membershipVM);
            
            // removes from project
            Project.Memberships.Remove(membershipVM.ProjectMembership);

            // dispose
            membershipVM.Dispose();

            PublishMemberChanged();
        }

        private void OnViewProjectDetails(Project project) {

            Project = project;

            if (project == null)
                return;

            LoadProjectMembers();

            LoadProjectMembersWorkedHours();

            OnPropertyChanged("IsAddMemberVisible");
            
            

            (ShowRolesListCommand as DelegateCommand).NotifyCanExecuteChanged();
            (AddNewMemberCommand as DelegateCommand).NotifyCanExecuteChanged();            
            (RemoveMemberCommand as DelegateCommand<ProjectMembershipViewModel>).NotifyCanExecuteChanged();            
            (ShowJoinDialogCommand as DelegateCommand).NotifyCanExecuteChanged();
            
        }


        private void LoadProjectMembers() {
            if (Project == null)
                return;
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => teamServices.GetProjectMembers(Project.ProjectUId), OnProjectMembersLoaded);

        }

        private void OnProjectMembersLoaded(ICollection<MemberProfile> members) {

            // free memory
            RemoveMemberCommand = null;

            // free memory --- TRICK ONE
            // GROUP DESCRIPTION KEEPS MEMBERSHIPS IN MEMORY
            if (membershipViewSource.Source != null) {
                ((Collection<ProjectMembershipViewModel>)membershipViewSource.Source).ClearAndDispose();
                membershipViewSource.GroupDescriptions.Clear();                
            }

            RemoveMemberCommand = new DelegateCommand<ProjectMembershipViewModel>(CanRemoveMember, RemoveMember);            
                
            Collection<ProjectMembershipViewModel> memberships = new Collection<ProjectMembershipViewModel>();

            foreach (ProjectMembership ms in Project.Memberships) {
                ms.Member = members.Where(m => m.MemberUId == ms.MemberUId).SingleOrDefault();
                ms.Role = Project.Roles.Where(r => r.RoleUId == ms.RoleUId).SingleOrDefault();
                MemberViewModel.DefineMemberAvatarUrl(ms.Member);
                memberships.Add(new ProjectMembershipViewModel(executor, projectsServices, authorizator, ms, ms.Member));
            }
           
            membershipViewSource.Source = memberships;
            membershipViewSource.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("ProjectMembership.Role.RoleName"));


            OnPropertyChanged("GroupedProjectMemberships");

            ((DelegateCommand)ShowJoinDialogCommand).NotifyCanExecuteChanged();

            IsLoadingData = false;

            PublishMemberChanged();

            LoadProjectMembersWorkedHours();

        }

        private void LoadProjectMembersWorkedHours() {
            executor.StartBackgroundTask<IDictionary<string, decimal>>(
                () => { return taskServices.GetProjectTotalEffectiveHoursByMember(project.ProjectUId, null); },
                hoursByMember => {
                    var memberships =  membershipViewSource.Source as Collection<ProjectMembershipViewModel>;
                    if(memberships==null)
                        return;
                    foreach (var membership in memberships) {
                        if (membership.Member!=null && hoursByMember.ContainsKey(membership.Member.MemberUId))
                            membership.TotalHoursInThisProject = hoursByMember[membership.Member.MemberUId];
                    }
                });
        }

        void membershipViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {
            ProjectMembershipViewModel ms = e.Item as ProjectMembershipViewModel;
            if (ms == null || ms.ProjectMembership==null)
                e.Accepted = false;
            else 
                e.Accepted = ms.ProjectMembership.IsActive;            
        }

        private void PublishMemberChanged() {            
            aggregator.Publish<ICollection<MemberProfile>>(ScrumFactoryEvent.ProjectMembersChanged, Project.Memberships.Where(ms => ms.Member!=null).Select(ms => ms.Member).ToList());
        }

        public void LoadMembers(string filter) {
           executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => { return teamServices.GetMembers(filter, SearchMemberAvailability, Project.ClientName, true, null, 9); }, OnMembersLoaded);
                
        }

        private void OnMembersLoaded(ICollection<MemberProfile> members) {
            ObservableCollection<MemberViewModel> membersVM = new ObservableCollection<MemberViewModel>();
            if (members != null)
                foreach (MemberProfile m in members)
                    membersVM.Add(new ViewModel.MemberViewModel(m, ServerUrl, authorizator));
            if (Members != null)
                Members.ClearAndDispose();
            Members = membersVM;
        }
    

        public ICollectionView GroupedProjectMemberships {
            get {
                if (membershipViewSource == null)
                    return null;
                return membershipViewSource.View;                
            }
        }

        private bool CanRemoveMember() {
            if (Project == null)
                return false;            
            return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }


        [Import(typeof(ProjectTeam))]
        public IView View { get; set; }

        /// <summary>
        /// Gets or sets the roles list view.
        /// </summary>
        /// <value>The roles list view.</value>
        [Import]
        public RolesListViewModel RolesListViewModel { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl ServerUrl { get; set; }

        [Import]
        public ContactListViewModel ContactListViewModel { get; set; }

        /// <summary>
        /// Shows the roles list.
        /// </summary>
        private void ShowRolesList() {
            RolesListViewModel.Show();            
        }

        private void ShowContactList() {
            ContactListViewModel.Show();            
        }

        private bool CanAddNewMember() {
            if (NewMember == null)
                return false;
            if (Project == null)
                return false;            
            return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            
        }

        private bool CanShowRoleList() {
            if (Project == null)
                return false;
            return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }


        private void AddNewMember() {

            if (NewMember == null)
                return;

            if (NewRole == null)
                return;

            AddProjectMembership(NewMember.MemberProfile, NewRole, null);
        }

        private void AddProjectMembership(MemberProfile member, Role role, int? allocation) {
            ProjectMembership ms = new ProjectMembership() {
                MemberUId = member.MemberUId,
                Member = member,
                IsActive = true,
                RoleUId = role.RoleUId,
                Role = role,
                DayAllocation = allocation,
                ProjectUId = Project.ProjectUId
            };
            executor.StartBackgroundTask(
                () => {
                    projectsServices.AddProjectMembership(ms.ProjectUId, ms);
                },
                () => {
                    Collection<ProjectMembershipViewModel> memberships = membershipViewSource.Source as Collection<ProjectMembershipViewModel>;                     
                    ProjectMembershipViewModel msVM = memberships.FirstOrDefault(m => m.ProjectMembership.MemberUId == ms.MemberUId && m.ProjectMembership.RoleUId == ms.RoleUId);
                    if (msVM == null) {
                        msVM = (ProjectMembershipViewModel)((IEditableCollectionView)membershipViewSource.View).AddNew();
                        msVM.Init(executor, projectsServices, authorizator, ms, ms.Member);
                        ((IEditableCollectionView)membershipViewSource.View).CommitNew();
                        memberships.Add(msVM);
                        Project.Memberships.Add(ms);
                    }
                    else {
                        msVM.ProjectMembership.IsActive = true;
                        ((IEditableCollectionView)membershipViewSource.View).EditItem(msVM);
                        ((IEditableCollectionView)membershipViewSource.View).CommitEdit();
                    }
                                        
                    msVM.NotifyAdded();
                    NewMember = null;
                   
                    ((DelegateCommand)ShowJoinDialogCommand).NotifyCanExecuteChanged();
                    PublishMemberChanged();
                });            
        }


        public int SearchMemberAvailability { get; set; }


        public ICommand SelectNewRoleCommand { get; set; }

        #region IPanelViewModel Members

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.Team;
            }
        }

        

        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get {
                return 500;
            }
        }

     

        #endregion

        #region IProjectTeamViewModel Members

    


        private MemberViewModel newMember = null;
        public MemberViewModel NewMember {
            get {
                return newMember;
            }
            set {
                newMember = value;
                OnPropertyChanged("NewMember");
                ((DelegateCommand)AddNewMemberCommand).NotifyCanExecuteChanged();
            }
        }

        public ScrumFactory.Windows.Helpers.CustomFilterDelegate MemberCustomFilter { private set; get; }
        
        public bool MemberFilter(string text, object value) {
            MemberViewModel memberVM = value as MemberViewModel;
            if (memberVM == null)
                return false;

            return memberVM.FilterTest(text);            
        }

        private Project project;

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
            }
        }


        /// <summary>
        /// Gets the project roles.
        /// </summary>
        /// <value>The roles.</value>
        public ICollection<RoleViewModel> Roles {
            get {
                return RolesListViewModel.Roles;
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

        /// <summary>
        /// Gets the show roles list command.
        /// </summary>
        /// <value>The show roles list command.</value>
        public ICommand ShowRolesListCommand { get; private set; }


        /// <summary>
        /// Gets the add new member command.
        /// </summary>
        /// <value>The add new member command.</value>
        public ICommand AddNewMemberCommand { get; private set; }

        /// <summary>
        /// Gets the remove member command.
        /// </summary>
        /// <value>The remove member command.</value>
        public ICommand RemoveMemberCommand { get; private set; }


        public ICommand ShowContactListCommand { get; set; }

        public ICommand ShowJoinDialogCommand { get; set; }

        #endregion

    }
}
