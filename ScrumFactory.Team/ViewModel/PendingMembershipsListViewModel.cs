using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers.Extensions;

namespace ScrumFactory.Team.ViewModel {

    [Export]    
    [Export(typeof(IPendingMembershipsListViewModel))]
    public class PendingMembershipsListViewModel : BasePanelViewModel, IPendingMembershipsListViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;        
        private IProjectsService projectServices;        
        private IBackgroundExecutor executor;
        
        private ICollection<Project> pendingProjects;

        [ImportingConstructor]
        public PendingMembershipsListViewModel(
            [Import]IEventAggregator eventAggregator,
            [Import]IProjectsService projectServices,            
            [Import]IBackgroundExecutor backgroundExecutor,
            [Import]IAuthorizationService authorizationService) {

            this.aggregator = eventAggregator;
            this.projectServices = projectServices;            
            this.executor = backgroundExecutor;


            RefuseCommand = new DelegateCommand<ProjectMembership>(Refuse);
            EngageCommand = new DelegateCommand<ProjectMembership>(Engage);
        
            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);
            
        }

        private void Refuse(ProjectMembership membership) {
            executor.StartBackgroundTask(
                () => { projectServices.RemoveProjectMembership(membership.ProjectUId, membership.MemberUId, membership.RoleUId); },
                () => { RemoveProject(membership); }
            );
        }

        private void Engage(ProjectMembership membership) {

            if (!membership.DayAllocation.HasValue)
                membership.DayAllocation = 0;

            executor.StartBackgroundTask(
                () => { projectServices.UpdateProjectMembershipAllocation(membership.ProjectUId, membership.MemberUId, membership.RoleUId, (int)membership.DayAllocation); },
                () => { RemoveProject(membership); }
            );
            
        }

        private void RemoveProject(ProjectMembership membership) {
            Project pendingProject = PendingProjects.SingleOrDefault(p => p.Memberships.Any(ms => ms.RoleUId == membership.RoleUId && ms.ProjectUId == membership.ProjectUId));
            if(pendingProject==null)
                return;
            PendingProjects.Remove(pendingProject);

            if (PendingProjects.Count == 0)
                IsVisible = false;
        }

        public ICollection<Project> PendingProjects {
            get {
                return pendingProjects;
            }
            set {
                pendingProjects = value;
                OnPropertyChanged("PendingProjects");
            }
        }

        private void LoadPendingProjects() {
            executor.StartBackgroundTask<ICollection<Project>>(
                () => { return projectServices.GetPendingEngagementProjects(); },
                ps => {
                    PendingProjects.ClearAndDispose();
                    PendingProjects = new ObservableCollection<Project>(ps);
                    if (PendingProjects.Count > 0)
                        Show();
                });
        }

        private void OnSignedMemberChanged(MemberProfile member) {
            if (member == null)
                return;
            LoadPendingProjects();
        }


        private void Show() {
            IsVisible = true;
        }

        private bool isVisible;
        public bool IsVisible {
            get {
                return isVisible;
            }
            private set {
                isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }


        [Import(typeof(PendingMemberships))]
        public IView View { get; set; }

        public ICommand EngageCommand { get; set; }
        public ICommand RefuseCommand { get; set; }
    }
}
