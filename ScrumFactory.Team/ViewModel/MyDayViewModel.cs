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
using System.Windows.Data;
using System.Windows.Input;

namespace ScrumFactory.Team.ViewModel {
    
    [Export]
    [Export(typeof(ITopMenuViewModel))]
    public class MyDayViewModel : MemberDetailBase, ITopMenuViewModel, INotifyPropertyChanged {

        private System.Windows.Data.CollectionViewSource eventsViewSource;
    

        public List<ProjectInfo> RecentProjects { get; private set; }

        [Import]
        private IDialogService dialogs { get; set; }

        [ImportingConstructor]
        public MyDayViewModel(
            [Import] IEventAggregator aggregator,
            [Import] ITasksService tasksService,
            [Import] IProjectsService projectsService,
            [Import] ITeamService teamService,
            [Import] IBackgroundExecutor executor,
            [Import] IAuthorizationService authorizator) : base(aggregator, tasksService, projectsService, teamService, executor, authorizator)  {

            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

            aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskAdded, t => { UpdateTasks(); });
            aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskAssigneeChanged, t => { UpdateTasks(); });
            aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskChanged, t => { UpdateTasks(); });
            

            aggregator.Subscribe<ICollection<ProjectInfo>>(ScrumFactoryEvent.RecentProjectChanged, prjs => {
                List<ProjectInfo> prjs2 = new List<ProjectInfo>(prjs);
                if (MemberEngagedProjects != null)                 
                    prjs2.RemoveAll(p => MemberEngagedProjects.Any(ep => ep.ProjectUId == p.ProjectUId));                
                RecentProjects = prjs2.Take(8).ToList();
                OnPropertyChanged("RecentProjects");
            });
            
            OnLoadCommand = new DelegateCommand(OnLoad);
            RefreshCommand = new DelegateCommand(Load);
            ShowMemberDetailCommand = new DelegateCommand<MemberProfile>(ShowMemberDetail);
            CreateNewProjectCommand = new DelegateCommand(CreateNewProject);

            eventsViewSource = new System.Windows.Data.CollectionViewSource();
         
        }

        [Import]
        public IPendingMembershipsListViewModel PendingMembershipsListViewModel { get; set; }


        private void CreateNewProject() {
            aggregator.Publish(ScrumFactoryEvent.CreateNewProject);
        }

        public ICollectionView Events {
            get {
                return eventsViewSource.View;
            }
        } 

        private void ShowMemberDetail(MemberProfile member) {
            if (member == null || member.MemberUId==null)
                return;
            dialogs.SetBackTopMenu();
            aggregator.Publish<MemberProfile>(ScrumFactoryEvent.ShowMemberDetail, member);            
        }

        private void OnSignedMemberChanged(MemberProfile member) {
            NeedRefresh = true;
            MemberProfile = member;
            Converters.NewTrophyConverter.SignedMember = member;
            OnLoad();
        }



        private void UpdateTasks() {

            NeedRefresh = true;
            if (View.IsVisible)
                LoadMemberTeamTasks();
        }

        private void OnLoad() {            
            IsVisible = (authorizator != null && authorizator.SignedMemberProfile != null);            
            if(NeedRefresh && IsVisible)
                Load();
         
        }

        //private System.DateTime lastRefreshDate;
        
        //public override bool NeedRefresh {
        //    get {
                
        //        return System.DateTime.Now.Subtract(lastRefreshDate).TotalSeconds > 10;
        //    }
        //    set {
        //        if (value == false)
        //            lastRefreshDate = System.DateTime.Now;
        //        base.NeedRefresh = value;
        //    }
        //}

      

        private void Load() {

            //LoadMemberTasks();
            LoadMemberTeamTasks();
            LoadMemberEngagedProjects(() => {
                if (RecentProjects != null) {
                    RecentProjects.RemoveAll(p => MemberEngagedProjects.Any(ep => ep.ProjectUId == p.ProjectUId));
                    OnPropertyChanged("RecentProjects");
                }
            });

            // not using this anymore
            //LoadMemberWorkingWithMembers();

            LoadEvents();

            NeedRefresh = false;
        }

        

        private void LoadEvents() {
            IsLoadingData = true;

            executor.StartBackgroundTask<ICollection<ProjectEvent>>(
                () => { return projectsService.GetUserProjectEvents(); },
                events => {                    
                    
                    eventsViewSource.GroupDescriptions.Clear();
                    eventsViewSource.Source = events.Where(t=> t.When <= System.DateTime.Today.AddDays(7)).OrderBy(t => t.When).ToArray(); ;
                    eventsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("When"));
                    

                    OnPropertyChanged("Events");
                    IsLoadingData = false;
                });
        }


        public string PanelName {
            get { return Properties.Resources.My_day; }
        }

        public int PanelDisplayOrder {
            get { return 0; }
        }

        private bool isVisible;
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
                return true;
            }
        }
      

        [Import(typeof(MyDay))]
        public IView View { get; set; }


        public ICommand RefreshCommand { get; set; }
        public ICommand OnLoadCommand { get; set; }
        public ICommand ShowMemberDetailCommand { get; set; }

        public ICommand CreateNewProjectCommand { get; set; }


        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Normal; }
        }

        public string ImageUrl {
            get { return "\\Images\\TopMenu\\home.png"; }
        }
    }
}
