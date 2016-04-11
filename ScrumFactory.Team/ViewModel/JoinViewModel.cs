using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Configuration;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team.ViewModel {

    [Export]
    [Export(typeof(IStartsWithApp))]
    public class JoinViewModel : BasePanelViewModel, IStartsWithApp, IViewModel, INotifyPropertyChanged  {

        private IAuthorizationService authorizator;
        private IEventAggregator aggregator;        
        private IDialogService dialogs;

        private IDialogViewModel window;

        private ICollection<Role> roles;
        private Role selectedRole;

        private int allocation;

        [ImportingConstructor]
        public JoinViewModel(
            [Import(typeof(IEventAggregator))] IEventAggregator aggregator,            
            [Import(typeof(IAuthorizationService))] IAuthorizationService authorizator,
            [Import(typeof(IDialogService))] IDialogService dialogService) {

            this.aggregator = aggregator;            
            this.authorizator = authorizator;
            this.dialogs = dialogService;

            aggregator.Subscribe(ScrumFactoryEvent.ShowJoinDialog, Show);
            aggregator.Subscribe<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, OnRolesChanged);

            JoinProjectCommand = new DelegateCommand(CanJoinProject, JoinProject);
        }

        [Import]
        private ProjectTeamViewModel ProjectTeamViewModel { get; set; }

        private bool CanJoinProject() {
            return SelectedRole != null;
        }

        private void JoinProject() {
            if (SelectedRole == null || SelectedRole.RoleUId == "0")
                return;
            ProjectTeamViewModel.JoinProject(SelectedRole, Allocation);
            window.Close();
        }

        public ICollection<Role> Roles {
            get {
                return roles;
            }
            set {
                roles = value;
                OnPropertyChanged("Roles");
            }
        }

        public Role SelectedRole {
            get {
                return selectedRole;
            }
            set {
                selectedRole = value;
                ((DelegateCommand)JoinProjectCommand).NotifyCanExecuteChanged();
                OnPropertyChanged("SelectedRole");
            }
        }

        public int Allocation {
            get {
                return allocation;
            }
            set {
                allocation = value;
                OnPropertyChanged("Allocation");
            }
        }

        private void OnRolesChanged(ICollection<Role> roles) {
            List<Role> roleList  = new List<Role>();
            roleList.Add(new Role() { RoleUId = "0", RoleName = "" });

            if (authorizator == null || authorizator.SignedMemberProfile == null || !authorizator.SignedMemberProfile.IsFactoryOwner)
                Roles = roleList.Concat(roles.Where(r => r.PermissionSet == (short)PermissionSets.TEAM)).ToArray();
            else
                Roles = roleList.Concat(roles).ToArray();
            
        }

        [Import(typeof(JoinDialog))]
        public IView View { get; set; }

        public void Show() {

            if (View.IsVisible)
                return;

            SelectedRole = Roles.FirstOrDefault();
            Allocation = 0;

            window = dialogs.NewDialog(Properties.Resources.Join_this_project, View);
            window.Show();

        }

        public ICommand JoinProjectCommand { get; set; }
    }

}
