using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace ScrumFactory.Team.ViewModel {

    public class ProjectMembershipViewModel : BaseEditableObjectViewModel, INotifyPropertyChanged {

        private IBackgroundExecutor executor;        
        private IProjectsService projectsService;
        private IAuthorizationService authorizator;

        public ProjectMembershipViewModel() { }
        
        public ProjectMembershipViewModel(
            IBackgroundExecutor executor,
            IProjectsService projectsService,
            IAuthorizationService authorizatorService,
            ProjectMembership membership,
            MemberProfile member) {

                Init(executor, projectsService, authorizatorService, membership, member);

        }

        public void Init(
            IBackgroundExecutor executor,
            IProjectsService projectsService,
            IAuthorizationService authorizatorService,
            ProjectMembership membership,
            MemberProfile member) {

            this.executor = executor;
            this.projectsService = projectsService;
            this.authorizator = authorizatorService;

            ProjectMembership = membership;
            Member = member;


            ChangeDayAllocationCommand = new DelegateCommand(ChangeDayAllocation);
            SendEmailCommand = new DelegateCommand(SendEmail);
        }

        ~ProjectMembershipViewModel() {
            System.Console.Out.WriteLine("***< member died here");
        }
        
        public ProjectMembership ProjectMembership { get; private set; }
        public MemberProfile Member { get; private set; }


      
        public decimal TotalHoursInThisProject { get; set; }


        public short SortPriority {
            get {
                if (ProjectMembership == null || ProjectMembership.Role==null)
                    return 5;
                if (ProjectMembership.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER)
                    return 1;
                if (ProjectMembership.Role.PermissionSet == (short)PermissionSets.TEAM)
                    return 2;
                if (ProjectMembership.Role.PermissionSet == (short)PermissionSets.PRODUCT_OWNER)
                    return 3;
                
                return 4;
            }
        }

        public bool UserCanChangeAllocation {
            get {
                if (Member == null || authorizator.SignedMemberProfile==null)
                    return false;
                return Member.MemberUId == authorizator.SignedMemberProfile.MemberUId;
            }
        }

        private void ChangeDayAllocation() {
            
            executor.StartBackgroundTask(
                () => { projectsService.UpdateProjectMembershipAllocation(ProjectMembership.ProjectUId, ProjectMembership.MemberUId, ProjectMembership.RoleUId, (int)ProjectMembership.DayAllocation); },
                () => { });
        
                           
        }

        private void SendEmail() {
            Process.Start(new ProcessStartInfo("mailto:" + Member.EmailAccount));
        }


        protected override void OnDispose() {

            //ProjectMembership = null; OnPropertyChanged("ProjectMembership");
            ChangeDayAllocationCommand = null; OnPropertyChanged("ChangeDayAllocationCommand");
            SendEmailCommand = null; OnPropertyChanged("SendEmailCommand");

        }

        public void RefreshUI() {
            OnPropertyChanged("Role");
            OnPropertyChanged("MemberProfile");
            OnPropertyChanged("ProjectMembership");
            OnPropertyChanged("DayAllocation");
        }

        public ICommand ChangeDayAllocationCommand { get; set; }
        public ICommand SendEmailCommand { get; set; }

    }
}
