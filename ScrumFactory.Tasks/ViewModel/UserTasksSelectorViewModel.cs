using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers.Extensions;
using System.Windows.Data;

namespace ScrumFactory.Tasks.ViewModel {

    [Export]
    [Export(typeof(IUserTasksSelectorViewModel))]
    public class UserTasksSelectorViewModel : BasePanelViewModel, IUserTasksSelectorViewModel, IDialogViewModel, IStartsWithApp, INotifyPropertyChanged {

        [Import]
        private IProjectsService projectsService { get; set; }
        
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private ITasksService tasksService;
        private IAuthorizationService authorizator;
        private IDialogService dialogs;

        private ICollection<TaskViewModel> tasks;

        private System.Windows.Data.CollectionViewSource tasksViewSource;

        private System.Windows.Data.CollectionViewSource notMineTasksViewSource;

        public string TrackingTaskUId { get; private set; }

        private string trackingTaskInfo { get; set; }

        private System.DateTime lastLoadTime;

        private System.Windows.Threading.DispatcherTimer timeKeeper = new System.Windows.Threading.DispatcherTimer();

        [Import]
        private FinishTaskDialogViewModel FinishDialog { get; set; }
        
        [ImportingConstructor]
        public UserTasksSelectorViewModel(
            [Import]IBackgroundExecutor executor,
            [Import]IEventAggregator aggregator,
            [Import]ITasksService tasksService,
            [Import] IDialogService dialogs,
            [Import]IAuthorizationService authorizator) {

                this.executor = executor;
                this.aggregator = aggregator;
                this.tasksService = tasksService;
                this.dialogs = dialogs;
                
                this.authorizator = authorizator;

                tasksViewSource = new System.Windows.Data.CollectionViewSource();
                notMineTasksViewSource = new System.Windows.Data.CollectionViewSource();
                
                TrackingTaskInfo = null;

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, m => { OnPropertyChanged("SignedMemberUId"); });

                aggregator.Subscribe(ScrumFactoryEvent.ApplicationWhentForeground, () => { LoadTasks(true); });

                aggregator.Subscribe(ScrumFactoryEvent.ShowUserTasksSelector, Show);

                aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskAssigneeChanged, OnTaskChanged);
                aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskChanged, OnTaskChanged);
        
                ShowTaskDetailCommand = new DelegateCommand<TaskViewModel>(ShowTaskDetail);

                TrackTaskCommand = new DelegateCommand<TaskViewModel>(TrackTask);

        

               timeKeeper.Tick += new EventHandler(timeKeeper_Tick);
        }

        public string SignedMemberUId {
            get {
                if (authorizator == null || authorizator.SignedMemberProfile==null)
                    return null;
                return authorizator.SignedMemberProfile.MemberUId;
            }
        }

        public ICollectionView FilteredTasks {
            get {
                return tasksViewSource.View;            
            }
        }

        public ICollectionView NotMineFilteredTasks {
            get {
                return notMineTasksViewSource.View;
            }
        }

        private void  timeKeeper_Tick(object sender, EventArgs e) {
            dialogs.ShowMessageBox(Properties.Resources.Time_is_up, 
                String.Format(Properties.Resources.The_planned_effor_to_finish_task_T_has_ran_out, TrackingTaskInfo),
                "/Images/Dialogs/clock.png");
            timeKeeper.Stop();
        }

        [Import]
        private TaskViewModel detailViewModel { get; set; }

        private void OnTaskChanged(Task task) {
            if (tasks == null)
                return;
            if (!tasks.Any(t => t.Task.TaskUId == task.TaskUId))
                return;
            if(View.IsVisible)
                LoadTasks();
        }
        
        public string TrackingTaskInfo {
            get {
                if (String.IsNullOrEmpty(trackingTaskInfo))
                    return String.Empty;

                return trackingTaskInfo + " (" + FormatedEllipsedHours + ")";
            }
            set {
                trackingTaskInfo = value;
                OnPropertyChanged("TrackingTaskInfo");
            }
        }

                
        private DateTime? startTrackingTime = null;


        private void ShowTaskDetail(TaskViewModel task) {
            dialogs.SetBackTopMenu();
            aggregator.Publish<Task>(ScrumFactoryEvent.ShowTaskDetail, task.Task);
            //View.Close();
            //detailViewModel.ShowDetail(task.Task);                                                    
        }

        public void StopTaskTrack() {
            StopTaskTrack(true);
        }

        private void StopTaskTrack(bool publish) {
            decimal hours = (decimal) TrackEllipsedTime.TotalHours;
            string id = TrackingTaskUId;            
            executor.StartBackgroundTask(
                () => {                    
                    tasksService.ChangeTaskEffectiveHours(id, hours, true);
                },
                () => {});
            ResetTaskTrack();
        }

        public void ResetTaskTrack() {            
            startTrackingTime = null;
            TrackingTaskUId = null;
            TrackingTaskInfo = null;
            timeKeeper.Stop();
            aggregator.Publish<string>(ScrumFactoryEvent.TaskTrackChanged, TrackingTaskUId);
        }




        public void StartTrack(Task task) {
            
            if (TrackingTaskUId != null)
                StopTaskTrack(false);

            TrackingTaskUId = task.TaskUId;
            startTrackingTime = DateTime.Now;
            TrackingTaskInfo = FormatTaskInfo(task.TaskName);
            aggregator.Publish<string>(ScrumFactoryEvent.TaskTrackChanged, TrackingTaskUId);    

            DateTime now = DateTime.Now;
            decimal remainingHours = task.PlannedHours - task.EffectiveHours;
            if (remainingHours > 0) {
                timeKeeper.Interval = now.AddHours((double)remainingHours).Subtract(now);
                timeKeeper.Start();
            }
        }

        private string FormatTaskInfo(string name) {            
            if (name.Length > 60)
                name = name.Substring(0, 56) + "...";
            return String.Format(Properties.Resources.Working_at_T, name);
        }

        /// <summary>
        /// Gets the ellipsed hours since the current tracking task was started.
        /// </summary>
        /// <value>The ellipsed hours.</value>
        public TimeSpan TrackEllipsedTime {
            get {
                if (!startTrackingTime.HasValue)
                    return new TimeSpan();
                return DateTime.Now.Subtract(startTrackingTime.Value);
            }
        }

        public string FormatedEllipsedHours {
            get {                
                DateTime now = DateTime.Now;
                double min = TrackEllipsedTime.TotalMinutes;
                if (min < 1)
                    return TrackEllipsedTime.TotalSeconds.ToString("0") + " " + Properties.Resources.secs;
                if (min < 60)
                    return TrackEllipsedTime.TotalMinutes.ToString("0") + " " + Properties.Resources.mins;
                return TrackEllipsedTime.TotalHours.ToString("0,#") + " " + Properties.Resources.hrs;
            }
        }


        /// <summary>
        /// Loads the tasks.
        /// </summary>
        private void LoadTasks(bool goFullIfNoTasks = false) {

            if (authorizator.SignedMemberProfile == null)
                return;

            IsLoadingData = true;
  
            executor.StartBackgroundTask<ICollection<Task>>(
                () => { return tasksService.GetUserTasks(authorizator.SignedMemberProfile.MemberUId, true, true); },
                ts => {
                    List<TaskViewModel> tempList = new List<TaskViewModel>();
                    foreach (Task t in ts) {
                        TaskViewModel vm = new TaskViewModel(executor, tasksService, aggregator, dialogs, authorizator, t, this);
                        vm.FinishDialog = this.FinishDialog;
                        tempList.Add(vm);                        
                    }
                    ClearTasks();
                    tasks = new ObservableCollection<TaskViewModel>(tempList.OrderByDescending(t => t.IsTracking).ThenByDescending(t => t.Task.Priority).ThenByDescending(t => t.Task.TaskType));

                    tasksViewSource.Source = tasks.Where(t => t.TaskAssigneeUId!=null);
                    tasksViewSource.GroupDescriptions.Clear();
                    tasksViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Task.TaskInfo.ProjectName"));

                    notMineTasksViewSource.Source = tasks.Where(t => t.TaskAssigneeUId == null);
                    notMineTasksViewSource.GroupDescriptions.Clear();
                    notMineTasksViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Task.TaskInfo.ProjectName"));                    

                    OnPropertyChanged("FilteredTasks");
                    OnPropertyChanged("NotMineFilteredTasks");
                    OnPropertyChanged("TrackingTaskInfo");

                    IsLoadingData = false;
                    lastLoadTime = DateTime.Now;

                    if (tasks.Count == 0 && goFullIfNoTasks)
                        aggregator.Publish(ScrumFactoryEvent.ShowFullScreen);

                });
        }

        /// <summary>
        /// Make sure to call dispose in every task to unbind commands.
        /// </summary>
        private void ClearTasks() {
            ShowTaskDetailCommand = null;
            tasks.ClearAndDispose();
            tasks = new List<TaskViewModel>();

            // need to this to release the taskview model from memory
            // 'cuz the TaskViewModel are binded to this command passing themselves as parameters
            // it create a reference and GC never collect the viewmodels..
            ShowTaskDetailCommand = new DelegateCommand<TaskViewModel>(ShowTaskDetail);
            
        }

        /// <summary>
        /// Start or stop to track a given task.
        /// </summary>
        /// <param name="taskVM">The task VM.</param>
        private void TrackTask(TaskViewModel task) {         
            if (task.IsTracking)
                StopTaskTrack();
            else
                StartTrack(task.Task);
        }


        public ICommand ShowTaskDetailCommand { get; set; }

        public ICommand TrackTaskCommand { get; set; }

        #region IDialogViewModel Members
        
        [Import(typeof(TaskSelector))]
        public Composition.View.IDialogView View { get; set; }

        public void Show() {
            
            if (authorizator.SignedMemberProfile == null)
                return;

            // just to save server requests, load only after each 5 seconds
            if (DateTime.Now.Subtract(lastLoadTime).TotalSeconds > 5)
                LoadTasks();

            //View.Show();
        }

        public void Close() {
            View.Close();
        }



        public System.Windows.Input.ICommand CloseWindowCommand { get; set; }


        public System.Windows.Input.ICommand MoveWindowCommand {
            get { throw new NotSupportedException(); }
        }
        
        public System.Windows.Input.ICommand MinimizeWindowCommand {
            get { throw new NotSupportedException(); }
        }

        public System.Windows.Input.ICommand MaximizeWindowCommand {
            get { throw new NotSupportedException(); }
        }

        #endregion
    }
}
