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


namespace ScrumFactory.Tasks.ViewModel {

    [Export]
    public class FinishTaskDialogViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private ITasksService tasksService;
        private IDialogService dialogs;

        private Task task;

        [Import]
        private IServerUrl ServerUrl { get; set; }

        [ImportingConstructor]
        public FinishTaskDialogViewModel(
            [Import]IBackgroundExecutor executor,
            [Import]IEventAggregator aggregator,
            [Import]ITasksService tasksService,
            [Import]IDialogService dialogs) {

            this.executor = executor;
            this.aggregator = aggregator;
            this.tasksService = tasksService;
            this.dialogs = dialogs;

            aggregator.Subscribe<Task>(ScrumFactoryEvent.ShowFinishTaskDialog, Show);
          

            AddingHours = 0.0M;

            FinishTaskCommand = new DelegateCommand(FinishTask);        
            CancelCommand = new DelegateCommand(() => { dialog.Close(); });
            CopyTaskTrackIdCommand = new DelegateCommand(CopyTaskTrackId);
            
            
        }

       

        private decimal addingHours;

        public decimal AddingHours {
            get {
                return addingHours;
            }
            set {
                addingHours = value;
                OnPropertyChanged("AddingHours");                
            }
        }

        public Task Task {
            get {
                return task;
            }
            set {
                task = value;                
                OnPropertyChanged("Task");
                OnPropertyChanged("FormatedEllipsedHours");
            }
        }

        public bool TaskWasFinished { get; private set; }

        private IDialogViewModel dialog;
        private void Show(Task task) {
            Task = task;
            TaskWasFinished = false;
            dialog = dialogs.NewDialog(Properties.Resources.Finish_this_task, View);
            dialog.Show();            
        }

        private void FinishTask() {            
            executor.StartBackgroundTask(
                () => { return tasksService.ChangeTaskStatus(Task.TaskUId, AddingHours, (short)TaskStatus.DONE_TASK); },
                dateDone => {
                    Task.Status = (short)TaskStatus.DONE_TASK;
                    task.EndDate = dateDone;
                    TaskWasFinished = true;
                    dialog.Close();
                    aggregator.Publish<Task>(ScrumFactoryEvent.TaskChanged, Task);                    
                });
        }

        private void CopyTaskTrackId() {
            TaskViewModel.CopyTaskTrackId(Task, Task.GetTaskTrackId(), ServerUrl.Url);          
        }

        public string FormatedEllipsedHours {
            get {
                if (Task == null)
                    return "";
                DateTime now = DateTime.Now;                
                decimal min = Task.EffectiveHours * 60;
                if (min < 1)
                    return (min*60).ToString("0") + " " + Properties.Resources.secs;

                if (min < 60)
                    return min.ToString("0") + " " + Properties.Resources.mins;

                return Task.EffectiveHours.ToString("#.#") + " " + Properties.Resources.hrs;
            }
        }

        public ICommand FinishTaskCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand CopyTaskTrackIdCommand { get; set; }


        #region IViewModel Members

        [Import(typeof(FinishTaskDialog))]
        public Composition.View.IView View { get; set; }

        #endregion
    }
}
