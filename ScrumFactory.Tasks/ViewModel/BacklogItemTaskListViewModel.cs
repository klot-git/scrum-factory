using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Tasks.ViewModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ScrumFactory.Extensions;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Windows.Helpers.Extensions;



namespace ScrumFactory.Tasks.ViewModels {

    [Export]
    [Export(typeof(IBacklogItemTaskListViewModel))]
    public class BacklogItemTaskListViewModel : BasePanelViewModel, IBacklogItemTaskListViewModel, INotifyPropertyChanged {

     
        [Import]
        private Services.ITasksService taskService { get; set; }

        [Import]
        private Services.IAuthorizationService authorizator { get; set; }
                
        [Import]
        private IBackgroundExecutor executor { get; set; }

        [Import]
        private IDialogService dialogs { get; set; }

        private ScrumFactory.Composition.IEventAggregator aggregator;

        private string newTaskName;
        public string NewTaskName {
            get {
                return newTaskName;
            }
            set {
                newTaskName = value;
                OnPropertyChanged("NewTaskName");
            }
        }
        
        public bool IsFocusAtNewTask {
            get {
                return isAddingTask;
            }            
        }

        private bool isAddingTask;
        public bool IsAddingTask {
            get {
                return isAddingTask;
            }
            set {
                isAddingTask = value;
                OnPropertyChanged("IsAddingTask");
                OnPropertyChanged("IsFocusAtNewTask");
            }
        }


        public Action OnTaskChanged { get; set; }

        private ICollection<Task> tasks;
        public ICollection<Task> Tasks {
            get {
                return tasks;
            }
            private set {
                tasks = value;
                OnPropertyChanged("Tasks");
            }
        }

        public decimal TotalEffectiveHours { get; private set; }

        public bool CanAddTask() {
            if (Item == null || Item == null || Item.Project == null || Item.Project.Roles == null)
                return false;

            return Item.Project.Memberships.Any(m => m.MemberUId == authorizator.SignedMemberProfile.MemberUId);
        }


        private bool IsAddingTaskForMe { get; set; }
      
        private void AddTask(Boolean forMe) {
            IsAddingTask = true;
            IsAddingTaskForMe = forMe;
            NewTaskName = "";
        }

        private void ConfirmAddTask() {

            Task task = new Task();
            task.TaskUId = Guid.NewGuid().ToString();
            task.Status = (short)TaskStatus.REQUIRED_TASK;
            task.PlannedHours = 0.0M;
            task.EffectiveHours = 0.0M;
            task.TaskName = NewTaskName;
            task.TaskType = 1;
            task.ProjectUId = Item.ProjectUId;
            task.BacklogItemUId = Item.BacklogItemUId;
            task.TaskOwnerUId = authorizator.SignedMemberProfile.MemberUId;
            if(IsAddingTaskForMe)
                task.TaskAssigneeUId = authorizator.SignedMemberProfile.MemberUId;
            task.CreatedAt = DateTime.Now;
            task.IsAccounting = true;

            executor.StartBackgroundTask<int>(() => {
                return taskService.CreateTask(task);
            },
            taskNumber => {
                task.TaskNumber = taskNumber;
                ((ObservableCollection<Task>)Tasks).Insert(0, task);
                IsAddingTaskForMe = false;
                IsAddingTask = false;
            });
        }

        private void ShowRepositoryLog(int taskNumber) {                      
            string filter = "#" + Item.Project.ProjectNumber + "." + Item.BacklogItemNumber + "." + taskNumber + "#";
            aggregator.Publish<string>(ScrumFactoryEvent.ShowCodeRepositoryLog, filter);
        }

        private void CancelAddTask() {            
            IsAddingTask = false;
        }

        private void ShowTask(Task task) {
             
            aggregator.Publish<Task>(ScrumFactoryEvent.ShowTaskDetail, task);
        }

        private BacklogItem item { get; set; }
        public BacklogItem Item {
            get {
                return item;
            }
            set {
                
                item = value;
                if (item == null)
                    return;

                ((DelegateCommand<string>)(AddTaskCommand)).NotifyCanExecuteChanged();
                                
                LoadTasks(true);
                    
                OnPropertyChanged("Item");
                IsAddingTask = false;                
            }
        }



        [ImportingConstructor]
        public BacklogItemTaskListViewModel([Import] ScrumFactory.Composition.IEventAggregator aggregator) {

            this.aggregator = aggregator;

            RefreshCommand = new ScrumFactory.Composition.DelegateCommand(LoadTasks);

            ShowTaskCommand = new ScrumFactory.Composition.DelegateCommand<Task>(ShowTask);

            AddTaskCommand = new ScrumFactory.Composition.DelegateCommand<string>(CanAddTask, s => { AddTask(s == "True"); });
            ConfirmAddTaskCommand = new ScrumFactory.Composition.DelegateCommand(ConfirmAddTask);
            CancelAddTaskCommand = new ScrumFactory.Composition.DelegateCommand(CancelAddTask);

            ShowRepositoryLogCommand = new ScrumFactory.Composition.DelegateCommand<int>(ShowRepositoryLog);

            NeedRefresh = true;
        }

        private void LoadTasks() {
            LoadTasks(false);
        }

        private void LoadTasks(bool deferBind) {
            Tasks = new ObservableCollection<Task>();

            if (Item == null)                
                return;
            
            executor.StartBackgroundTask<ICollection<Task>>(() => {
                ICollection<Task> tasks = taskService.GetItemTasks(Item.BacklogItemUId);
                if(deferBind)
                    System.Threading.Thread.Sleep(300);
                return tasks;
            },
            tasks => {
                Tasks = new ObservableCollection<Task>(tasks.OrderByDescending(t => t.CreatedAt));
                TotalEffectiveHours = tasks.Sum(t => t.EffectiveHours);
                if (OnTaskChanged != null)
                    OnTaskChanged.Invoke();
            });            
        }

        public decimal GetTotalEffectiveHoursForSprint(Sprint sprint) {
            if (sprint == null)
                return TotalEffectiveHours;
            return Tasks.Where(t => t.CreatedAt >= sprint.StartDate && t.CreatedAt <= sprint.EndDate).Sum(t => t.EffectiveHours);
        }

        [Import(typeof(ItemTaskList))]
        public Composition.View.IView View {
            get;
            set;
        }


        public ICommand RefreshCommand { get; set; }
    
        public ICommand AddTaskCommand { get; set; }
        public ICommand ConfirmAddTaskCommand { get; set; }
        public ICommand CancelAddTaskCommand { get; set; }

        public ICommand ShowTaskCommand { get; set; }


        public ICommand ShowRepositoryLogCommand { get; set; }
        
    }
}
