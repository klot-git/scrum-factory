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
using ScrumFactory.Windows.Helpers.Extensions;
using System.Windows.Data;

namespace ScrumFactory.Team.ViewModel {

    
    public class MemberDetailBase : BasePanelViewModel {

        protected IEventAggregator aggregator;
        protected ITasksService tasksService;
        protected IProjectsService projectsService;
        protected ITeamService teamService;
        
        protected IBackgroundExecutor executor;
        protected IAuthorizationService authorizator;

        private MemberProfile memberProfile;

        private ICollection<Task> memberTasks;        
        private ICollection<Project> memberEngagedProjects;
        private ICollection<MemberViewModel> memberWorkingWithMembers;

        private ICollection<MemberProfile> teamMembers;

        [Import]
        private IDialogService dialogs { get; set; }

        [Import]
        private IUserTasksSelectorViewModel taskSelector { get; set; }

        public MemberDetailBase(
            IEventAggregator aggregator,
            ITasksService tasksService,
            IProjectsService projectsService,
            ITeamService teamService,
            IBackgroundExecutor executor,
            IAuthorizationService authorizator) {

                this.aggregator = aggregator;                
                this.executor = executor;
                this.authorizator = authorizator;
                this.tasksService = tasksService;
                this.teamService = teamService;
                this.projectsService = projectsService;
    
                ShowTaskDetailCommand = new DelegateCommand<Task>(ShowTaskDetail);
                ShowProjectDetailCommand = new DelegateCommand<string>(ShowProjectDetail);
                CloseTaskCommand = new DelegateCommand<Task>(CloseTask);

                TrackTaskCommand = new DelegateCommand<Task>(TrackTask);
            
        }

        public string TrackingTaskUId {
            get {
                return taskSelector.TrackingTaskUId;
            }
        }

        public void TrackTask(Task task) {
            taskSelector.StartTrack(task);
            OnPropertyChanged("TrackingTaskUId");
        }
       

        public int MemberTotalEngagement {
            get {
                if (MemberEngagedProjects == null)
                    return 0;
                int? total = MemberEngagedProjects.Where(p => p.Memberships!=null).Sum(p => p.Memberships.Sum(m => m.DayAllocation));
                if (total.HasValue)
                    return total.Value;

                return 0;
            }
        }

        public ICollection<Task> MemberTasks {
            get {
                return memberTasks;
            }
            set {
                memberTasks = value;
                OnPropertyChanged("MemberTasks");
            }
        }

        public ICollection<MemberProfile> TeamMembers {
            get {
                return teamMembers;
            }
            set {
                teamMembers = value;
                OnPropertyChanged("TeamMembers");
                OnPropertyChanged("TeamTaskCount");
            }
        }

        public ICollection<MemberViewModel> MemberWorkingWithMembers {
            get {
                return memberWorkingWithMembers;
            }
            set {
                memberWorkingWithMembers = value;
                OnPropertyChanged("MemberWorkingWithMembers");       
            }
        }

        public ICollection<Project> MemberEngagedProjects {
            get {
                return memberEngagedProjects;
            }
            set {
                memberEngagedProjects = value;
                OnPropertyChanged("MemberEngagedProjects");
                OnPropertyChanged("MemberTotalEngagement");
            }
        }

        [Import]
        private IServerUrl ServerUrl { get; set; }

        public decimal MemberPlannedEffort {
            get {
                if (MemberTasks == null || MemberTasks.Count == 0)
                    return 0;
                return MemberTasks.Sum(t => t.PlannedHours - t.EffectiveHours);
            }
        }

        protected void LoadMemberPerfomance() {
            IsLoadingData = true;
            executor.StartBackgroundTask<MemberProfile>(() => {
                return teamService.GetMember(MemberProfile.MemberUId);
            },
            m => {
                MemberProfile.Performance = m.Performance;
                OnPropertyChanged("MemberProfile");
            });
        }

        protected void LoadMemberWorkingWithMembers() {
            IsLoadingData = true;
            MemberWorkingWithMembers.ClearAndDispose();
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => { return teamService.GetMembers(null, 0, null, true, MemberProfile.MemberUId, 0); },
                members => {
                    List<MemberViewModel> temp = new List<MemberViewModel>();
                    foreach(MemberProfile m in members.OrderBy(m => m.FullName))
                        temp.Add(new MemberViewModel(m, ServerUrl, authorizator));
                    MemberWorkingWithMembers = temp;
                    IsLoadingData = false;
                });
        }

       

        protected void LoadMemberTasks() {
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Task>>(
                () => { return tasksService.GetUserTasks(MemberProfile.MemberUId, true, false); },
                tasks => {
                    MemberTasks = new List<Task>(tasks.OrderByDescending(t => t.Priority).ThenByDescending(t => t.TaskType));
                    OnPropertyChanged("MemberPlannedEffort");
                    IsLoadingData = false;
                });
        }



        protected void LoadMemberEngagedProjects(Action afterLoad = null) {
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Project>>(
                () => { return projectsService.GetProjects(null, null, "ENGAGED_PROJECTS", MemberProfile.MemberUId); },
                projects => {
                    MemberEngagedProjects = projects;
                    if (afterLoad != null)
                        afterLoad();
                    IsLoadingData = false;
                });
        }

        protected void LoadMemberTeamTasks() {
            
            IsLoadingData = true;

            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => { return teamService.GetTeamMembers(MemberProfile.TeamCode, true, false, 25, 5); },
                members => {
                    foreach (MemberProfile m in members)
                        m.IsSignedMember = (authorizator.SignedMemberProfile.MemberUId == m.MemberUId);
                    TeamMembers = new ObservableCollection<MemberProfile>(members.OrderByDescending(m=> m.IsSignedMember).ThenBy(m => m.FullName));
                    IsLoadingData = false;
                });
        }

        public int TeamTaskCount {
            get {
                if (TeamMembers == null)
                    return 0;
                return TeamMembers.Sum(m => m.OpenTasks.Count);
            }
        }

        
        public MemberProfile MemberProfile {
            get {
                return memberProfile;
            }
            protected set {
                memberProfile = value;
                OnPropertyChanged("MemberProfile");
            }
        }

        private void ShowTaskDetail(Task task) {
            dialogs.SetBackTopMenu();   
            aggregator.Publish<Task>(ScrumFactoryEvent.ShowTaskDetail, task);
        }

        private void CloseTask(Task task) {
            aggregator.Publish<Task>(ScrumFactoryEvent.ShowFinishTaskDialog, task);
        }

        private void ShowProjectDetail(string projectUId) {
            executor.StartBackgroundTask<Project>(
                () => { return projectsService.GetProject(projectUId); },
                p => {
                    aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);
                        
                });
        }

            
        public ICommand ShowTaskDetailCommand { get; set; }
        public ICommand ShowProjectDetailCommand { get; set; }
        public ICommand CloseTaskCommand { get; set; }

        public ICommand TrackTaskCommand { get; set; }


    }
}
