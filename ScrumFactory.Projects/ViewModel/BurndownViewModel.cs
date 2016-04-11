using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers;
using System.Linq;
using ScrumFactory.Composition.View;
using System.Windows.Data;


namespace ScrumFactory.Projects.ViewModel {


    
    [Export(typeof(BurndownViewModel))]
    public class BurndownViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IBacklogService backlogService;
        private IProjectsService projectsService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;

        private ICollection<BurndownLeftHoursByDay> actualHours = null;
        private ICollection<BurndownLeftHoursByDay> actualHoursAhead = null;
        private ICollection<BurndownLeftHoursByDay> plannedHours = null;

        private string deadlinePositionLabel;

        public Project Project { get; private set; }

        [ImportingConstructor()]
        public BurndownViewModel(
            [Import] IBacklogService backlogService,        
            [Import] IProjectsService projectsService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IAuthorizationService authorizator) {

            this.backlogService = backlogService;
            this.projectsService = projectsService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;
            this.authorizator = authorizator;

            NeedRefresh = false;

            OnLoadCommand = new DelegateCommand(() => {if (NeedRefresh) LoadData();});
            SetBaselineCommand = new DelegateCommand(_CanChangeBaseline, SetBaseline);
          
            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p=> {
                Project = p;
                SetPlannings();
                AskForRefresh();
            });

            aggregator.Subscribe<Project>(ScrumFactoryEvent.BurndownShouldRefresh, AskForRefresh);
            aggregator.Subscribe<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, o => { AskForRefresh(); });
            aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, b => { AskForRefresh(); });

        }

        private bool _CanChangeBaseline() {
            return CanChangeBaseline;
        }

        public bool CanChangeBaseline {
            get {
                if (authorizator == null || authorizator.SignedMemberProfile == null || Project == null)
                    return false;
                return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        public bool IsComparePlanningBaseline {
            get {
                if (Project == null)
                    return false;
                return Project.Baseline == ComparePlanning;
            }
        }

        public int? Baseline {
            get {
                if (Project == null)
                    return null;
                return Project.Baseline;
            }
        }

        private void SetBaseline() {

            int? baseline = null;
            if (Project.Baseline != ComparePlanning)                
                baseline = ComparePlanning;

            executor.StartBackgroundTask(() => {
                projectsService.SetProjectBaseline(Project.ProjectUId, baseline);
            },
            ()=> {
                Project.Baseline = baseline;
                OnPropertyChanged("Baseline");
                OnPropertyChanged("IsComparePlanningBaseline");
            });

            

            
        }

        private int[] plannings;
        public int[] Plannings {
            get {
                return plannings;
            }
            set {
                plannings = value;
                OnPropertyChanged("Plannings");
            }
        }

        private void SetPlannings() {

            if (Project == null)
                return;

            SetDefaultComparePlanning();

            List<int> ps = new List<int>();
            for (int i = Project.CurrentPlanningNumber; i >= 0; i--)
                ps.Add(i);
            Plannings = ps.ToArray();
        }

        private void SetDefaultComparePlanning() {

            if (Project == null)
                return;

            if (Project.Baseline.HasValue) {
                ComparePlanning = Project.Baseline.Value;
                return;
            }

            if (Project.CurrentPlanningNumber < 1)
                ComparePlanning = 0;
            else
                ComparePlanning = 1;
        }

        private int comparePlanning;
        public int ComparePlanning {
            get {
                return comparePlanning;
            }
            set {
                comparePlanning = value;
                if(IsVisible)
                    LoadData();                
                OnPropertyChanged("ComparePlanning");
                OnPropertyChanged("IsComparePlanningBaseline");                
            }
        }

        public bool IsVisible {
            get {
                return View.IsVisible;
            }            
        }

        private void AskForRefresh() {
            AskForRefresh(Project);
        }

        private void AskForRefresh(Project project) {
            Project = project;                        

            if (IsVisible) {
                LoadData();                
            } else
                NeedRefresh = true;            
        }
      

        public void LoadData() {

            

            OnPropertyChanged("Baseline");
            OnPropertyChanged("IsComparePlanningBaseline");
            OnPropertyChanged("CanChangeBaseline");

            ((DelegateCommand)SetBaselineCommand).NotifyCanExecuteChanged();

            executor.StartBackgroundTask<ICollection<BurndownLeftHoursByDay>>(LoadCurveData, OnDataLoaded);
        }

        private ICollection<BurndownLeftHoursByDay> LoadCurveData() {
            if (IsLoadingData)
                return null;
            IsLoadingData = true;
            if (Project == null)
                return new List<BurndownLeftHoursByDay>();
            
            return backlogService.GetBurndownHoursByDay(Project.ProjectUId, ComparePlanning.ToString());
            
        }

        private void OnDataLoaded(ICollection<BurndownLeftHoursByDay> leftHoursByDay) {
            
            IsLoadingData = false;
            NeedRefresh = false;
            if (leftHoursByDay == null) return;


            ActualHours = new ObservableCollection<BurndownLeftHoursByDay>(leftHoursByDay.Where(h => h.LeftHoursMetric==LeftHoursMetrics.LEFT_HOURS).ToArray());
            ActualHoursAhead = new ObservableCollection<BurndownLeftHoursByDay>(leftHoursByDay.Where(h => h.LeftHoursMetric == LeftHoursMetrics.LEFT_HOURS_AHEAD).ToArray());            
            PlannedHours = new ObservableCollection<BurndownLeftHoursByDay>(leftHoursByDay.Where(h => h.LeftHoursMetric == LeftHoursMetrics.PLANNING).ToArray());

            OnPropertyChanged("WalkedPct");
            OnPropertyChanged("DeadlinePosition");
            OnPropertyChanged("IsBacklogPlanned");

        }

        public bool IsBacklogPlanned {
            get {
                if (PlannedHours == null)
                    return true;
                if(PlannedHours.Count == 0)
                    return false;
                decimal max = PlannedHours.Max(p => p.TotalHours);
                return (max > 1);
            }
        }

        

        #region IBurndownViewModel Members

        public decimal WalkedPct {
            get {
                if (PlannedHours == null || PlannedHours.Count == 0)
                    return -100;
                if (ActualHours == null || ActualHours.Count == 0)
                    return 0;

                decimal? plannedTotalHours = 0;

                plannedTotalHours = PlannedHours.Where(h => h.SprintNumber == 1).Max(h => h.TotalHours);

                if (!plannedTotalHours.HasValue || plannedTotalHours==0)
                    return -100;


                BurndownLeftHoursByDay actual = ActualHours.FirstOrDefault(d => d.Date.Equals(DateTime.Today));
                if (actual == null)
                    actual = ActualHours.Last();
                if (actual == null)
                    return 0;                

                if (actual.TotalHours == 0)
                    return 100;


                decimal done = plannedTotalHours.Value - actual.TotalHours;
                decimal walked = done / plannedTotalHours.Value * 100;


                return walked;
            }
        }


        private bool deadlineAhead;

        public decimal DeadlinePosition {
            get {
                if (PlannedHours == null || PlannedHours.Count == 0)
                    return 0;
                if (ActualHours == null || ActualHours.Count == 0)
                    return 0;

                BurndownLeftHoursByDay planned = PlannedHours.FirstOrDefault(d => d.Date.Equals(DateTime.Today));
                if (planned == null)
                    planned = plannedHours.Last();
                BurndownLeftHoursByDay actual = ActualHours.FirstOrDefault(d => d.Date.Equals(DateTime.Today));
                if (actual == null)
                    actual = ActualHours.Last();
                if (actual == null || planned == null)
                    return 0;
                
                decimal position = planned.TotalHours - actual.TotalHours;

                if (position < 0) {
                    deadlineAhead = false;
                    DeadlinePositionLabel = Properties.Resources.hrs_behind;
                }
                else {
                    deadlineAhead = true;
                    DeadlinePositionLabel = Properties.Resources.hrs_ahead;
                }
                
                return Math.Abs(position);
            }
        }

        public string DeadlinePositionLabel {
            get {
                return deadlinePositionLabel;
            }
            private set {
                deadlinePositionLabel = value;
                OnPropertyChanged("DeadlinePositionLabel");
                OnPropertyChanged("DeadlinePositionStatus");
            }
        }

        public int DeadlinePositionStatus {
            get {                
                if (!deadlineAhead)
                    return (int)IndicatorStatus.OVER;
                return (int)IndicatorStatus.NORMAL;
            }
        }

        [Import(typeof(Burndown))]
        public IView View { get; set; }

     
     
        public ICollection<BurndownLeftHoursByDay> PlannedHours {
            get {
                return plannedHours;
            }
            set {
                plannedHours = value;
                OnPropertyChanged("PlannedHours");
            }
        }

        public ICollection<BurndownLeftHoursByDay> ActualHours {
            get {
                return actualHours;
            }
            set {
                actualHours = value;
                OnPropertyChanged("ActualHours");
            }
        }

        public ICollection<BurndownLeftHoursByDay> ActualHoursAhead {
            get {
                return actualHoursAhead;
            }
            set {
                actualHoursAhead = value;
                OnPropertyChanged("ActualHoursAhead");
            }
        }

        public ICommand OnLoadCommand { get; private set; }

        public ICommand SetBaselineCommand { get; private set; }

        #endregion
    }


    
}
