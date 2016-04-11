using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;

namespace ScrumFactory.Team.ViewModel {

    /// <summary>
    /// Roles List View Model.
    /// </summary>
    [Export]
    public class RolesListViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        private IProjectsService projectsServices;
        private IBackgroundExecutor executor;
        private IDialogService dialogs;

        private RoleViewModel selectedRole;
        private Role oldSelectedRole;

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }
        
        [ImportingConstructor]
        public RolesListViewModel(
            [Import]IEventAggregator eventAggregator,
            [Import]IProjectsService projectsServices,
            [Import]IDialogService dialogs,
            [Import]IBackgroundExecutor backgroundExecutor
            ) {

            this.aggregator = eventAggregator;
            this.projectsServices = projectsServices;
            this.executor = backgroundExecutor;
            this.dialogs = dialogs;

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
         

            CloseWindowCommand = new DelegateCommand(CloseWindow);
            AddNewRoleCommand = new DelegateCommand(AddNewRole);
            DeleteRoleCommand = new DelegateCommand<RoleViewModel>(DeleteRole);
        }

        private void OnViewProjectDetails(Project project) {            
            Project = project;
            if (project == null)
                return;
            CreateRoleViewModels();
        }
      
        private void CreateRoleViewModels() {
            if (Project.Roles == null)
                Project.Roles = new List<Role>();
            
            Roles = new ObservableCollection<RoleViewModel>();
            if (Project.Roles != null) {
                foreach (Role r in Project.Roles) {
                    Roles.Add(new RoleViewModel(r));
                }
            }
            OnPropertyChanged("Roles");
            aggregator.Publish<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, Project.Roles);
        }

        
        private void DeleteRole(RoleViewModel roleVM) {                    
            executor.StartBackgroundTask(
                () => { projectsServices.DeleteProjectRole(roleVM.Role.ProjectUId, roleVM.Role.RoleUId); },
                () => { OnProjectRoleDeleted(roleVM); });
        }

        private void OnProjectRoleDeleted(RoleViewModel deletedRole) {            
            Project.Roles.Remove(deletedRole.Role);
            Roles.Remove(deletedRole);
            aggregator.Publish<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, Project.Roles);            
        }
      

        private void AddNewRole() {
            Role newRole = new Role() { 
                ProjectUId = Project.ProjectUId,
                RoleUId = Guid.NewGuid().ToString(),
                RoleName = Properties.Resources.New_role_name,
                RoleShortName = Properties.Resources.New_role_short_name,
                PermissionSet = (short)PermissionSets.TEAM,
                IsPlanned = true };
            
            executor.StartBackgroundTask(
                () => { projectsServices.UpdateProjectRole(SelectedRole.Role.ProjectUId, newRole.RoleUId, newRole);},
                () => {
                    Project.Roles.Add(newRole);
                    RoleViewModel roleVm = new RoleViewModel(newRole);                    
                    Roles.Add(roleVm);
                    SelectedRole = roleVm;
                    roleVm.NotifyAdded();
                });

            aggregator.Publish<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, Project.Roles);
        }
 
        /// <summary>
        /// Closes the window, invoke the OnCloseAction and publishes the CloseWindow event.
        /// </summary>
        private void CloseWindow() { 
            if(HasRoleChanged)           
                SaveOldRole();
            Close();        
        }

        public void Show() {
            Show(projectContainer.Value);
        }

        private void AdjustDefaultRole(Role defaultRole) {
            foreach (RoleViewModel roleVM in Roles.Where(r => r.Role.RoleUId != defaultRole.RoleUId))                 
                roleVM.IsDefaultRole = false;
            
        }

        private bool HasRoleChanged {
            get {
                if (oldSelectedRole == null)
                    return false;
                Role actualRole = Project.Roles.SingleOrDefault(r => r.RoleUId == oldSelectedRole.RoleUId);
                if (actualRole == null)
                    return false;
                return !actualRole.IsTheSame(oldSelectedRole);
            }
        }

        private void SaveOldRole() {
            if (oldSelectedRole == null)
                return;

            Role actualRole = Project.Roles.SingleOrDefault(r => r.RoleUId == oldSelectedRole.RoleUId);
            if (actualRole==null)
                return;

            AdjustDefaultRole(actualRole);

            executor.StartBackgroundTask(
                () => {
                    projectsServices.UpdateProjectRole(SelectedRole.Role.ProjectUId, actualRole.RoleUId, actualRole);
                },
                () => { });

            aggregator.Publish<Role>(ScrumFactoryEvent.ProjectRoleChanged, actualRole);
        }


        [Import(typeof(RolesList))]
        public IView View { get; set; }

        
        #region IRolesListViewModel Members

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>The project.</value>
        public Project Project { get; private set; }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public ICollection<RoleViewModel> Roles { get; private set; }

        /// <summary>
        /// Gets or sets the selected role.
        /// </summary>
        /// <value>The selected role.</value>
        public RoleViewModel SelectedRole {
            get {
                return selectedRole;
            }
            set {
                if (HasRoleChanged) 
                    SaveOldRole();
                                    
                if (value != null && value.Role != null)
                    oldSelectedRole = value.Role.Clone();
                else
                    oldSelectedRole = null;
                
                selectedRole = value;
                OnPropertyChanged("SelectedRole");
            }
        }
        
        /// <summary>
        /// Gets the add new role command.
        /// </summary>
        /// <value>The add new role command.</value>
        public ICommand AddNewRoleCommand { get; private set; }

        /// <summary>
        /// Gets the remove role command.
        /// </summary>
        /// <value>The remove role command.</value>
        public ICommand DeleteRoleCommand { get; private set; }

        /// <summary>
        /// Gets the close window command.
        /// </summary>
        /// <value>The close window command.</value>
        public ICommand CloseWindowCommand { get; private set; }

        #endregion

        #region IPanelViewModel Members

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.Roles;
            }
        }


        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get { return 0; }
        }

 

        #endregion

 
    }
}
