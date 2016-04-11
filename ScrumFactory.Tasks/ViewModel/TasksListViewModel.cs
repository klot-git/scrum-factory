using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Windows.Helpers.Extensions;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using System.Linq;
using System.Windows.Data;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;

namespace ScrumFactory.Tasks.ViewModel {

    public enum ListModes : int {
        POST_IT_MODE,
        LIST_MODE
    }

    [Export(typeof(IProjectTabViewModel))]
    [Export(typeof(TasksListViewModel))]
    public class TasksListViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {
        
        private string searchFilterText;
        private DelayAction delayFilter;
        private System.Windows.Data.CollectionViewSource tasksViewSource;

        private BacklogItem selectedBacklogItem = null;
        
        private ICollection<BacklogItem> backlogItems;
        private System.Windows.Data.CollectionViewSource backlogViewSource;



        private Project project;
        
        private ICollection<AssigneeViewModel> projectMembers;
        
        private ListModes listMode;
        private string newTaskName;

        private short newTaskType;

        private ITasksService tasksService;
        private IBacklogService backlogService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;
        private IDialogService dialogs;

        private DelayAction clockUpdate;

        private System.Media.SoundPlayer player;

        
        [ImportingConstructor()]
        public TasksListViewModel(
            [Import] ITasksService tasksService,
            [Import] IBacklogService backlogService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogService,
            [Import] IAuthorizationService authorizationService) {

                this.tasksService = tasksService;
                this.backlogService = backlogService;
                this.executor = backgroundExecutor;
                this.aggregator = eventAggregator;
                this.dialogs = dialogService;
                this.authorizator = authorizationService;

            tasksViewSource = new System.Windows.Data.CollectionViewSource();

            tasksViewSource.SortDescriptions.Add(new SortDescription("Task.Priority", ListSortDirection.Descending));
            tasksViewSource.SortDescriptions.Add(new SortDescription("Task.TaskType", ListSortDirection.Descending));
            tasksViewSource.SortDescriptions.Add(new SortDescription("Task.TaskNumber", ListSortDirection.Descending));

            tasksViewSource.Filter += new System.Windows.Data.FilterEventHandler(tasksViewSource_Filter);
            delayFilter = new DelayAction(500, new DelayAction.ActionDelegate(Refresh));

            backlogViewSource = new System.Windows.Data.CollectionViewSource();
            backlogViewSource.SortDescriptions.Add(new SortDescription("SprintNumber", ListSortDirection.Ascending));
            backlogViewSource.SortDescriptions.Add(new SortDescription("OccurrenceConstraint", ListSortDirection.Ascending));
            backlogViewSource.SortDescriptions.Add(new SortDescription("BusinessPriority", ListSortDirection.Ascending));
            backlogViewSource.SortDescriptions.Add(new SortDescription("BacklogItemNumber", ListSortDirection.Ascending));
            
            clockUpdate = new DelayAction(1000, new DelayAction.ActionDelegate(UpdateClock), false);

            player = new System.Media.SoundPlayer();
            player.Stream = Properties.Resources.whistles;
            player.Load();


            ListMode = ListModes.POST_IT_MODE;

            NewTaskType = (short)TaskTypes.DEVELOPMENT_TASK;

            OnLoadCommand = new DelegateCommand(() => { if (NeedRefresh) LoadData(); });

            ShowDetailWindowCommand = new DelegateCommand<TaskViewModel>(ShowDetail);

            CreateTaskCommand = new DelegateCommand(CanCreateTask, CreateTask);
            CreateEmptyTaskCommand = new DelegateCommand(CanCreateEmptyTask, CreateTask);
            
            ChangeListModeCommand = new DelegateCommand(ChangeListMode);

            MoveTaskCommand = new DelegateCommand<Windows.Helpers.DragDrop.DropCommandParameter>(MoveTask);

            ChangeTaskItemCommand = new DelegateCommand<Windows.Helpers.DragDrop.DropCommandParameter>(ChangeTaskItem);

            ShowJoinDialogCommand = new DelegateCommand(CanJoinProject, ShowJoinDialog);

            StartStopClockCommand = new DelegateCommand(StartStopClock);

            SetNewTaskTypeCommand = new DelegateCommand<Int16>(t => { NewTaskType = t; });

            CopyTasksCommand = new DelegateCommand(CopyTasksToClipboard);
            PasteTasksCommand = new DelegateCommand(PasteTasks);
            SelectAllTasksCommand = new DelegateCommand(SelectAllTasks);

            CloseItemCommand = new DelegateCommand<BacklogItem>(CloseItem);

            AddNewTagCommand = new DelegateCommand(CanAddTags, AddNewTag);
            RemoveTagCommand = new DelegateCommand<TaskTag>(CanAddTags, RemoveTag);
            UpdateTagCommand = new DelegateCommand<TaskTag>(CanAddTags, UpdateTag);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p => {                
                Project = p;
                StopClock();
                OnPropertyChanged("CurrentSprintLabel");
            });
         
            aggregator.Subscribe<ICollection<MemberProfile>>(ScrumFactoryEvent.ProjectMembersChanged, OnMembersChanged);

            aggregator.Subscribe<ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, OnRolesChanged);
            aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskChanged, OnTaskChanged);

            aggregator.Subscribe(ScrumFactoryEvent.ApplicationWhentForeground, OnAppForeground);

            aggregator.Subscribe<BacklogItem>(ScrumFactoryEvent.ShowTasksForItem, ShowTasksForItem);

            aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, UpdateChangedItems);
            aggregator.Subscribe(ScrumFactoryEvent.SprintsShifted, AskForRefresh);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ProjectStatusChanged, SetReplanItemWhenChanged);


            aggregator.Subscribe<Task>(ScrumFactoryEvent.TaskSelectedChanged, OnTaskSelectedChanged);
            


        }

        public bool CanNotAddTags {
            get {
                if (authorizator.SignedMemberProfile == null)
                    return true;
                return !Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        private bool CanAddTags() {
            return !CanNotAddTags;
        }

        public TaskTag filterTag;
        public TaskTag FilterTag {
            get {
                return filterTag;
            }
            set {
                filterTag = value;                                
                OnPropertyChanged("FilterTag");
                LoadTasks();
            }
        }

        private void UpdateTag(TaskTag tag) {
            executor.StartBackgroundTask(() => { tasksService.UpdateTaskTag(Project.ProjectUId, tag.TagUId, tag); }, () => { });            
        }

        private void RemoveTag(TaskTag tag) {
            System.Windows.MessageBoxResult confirm = dialogs.ShowMessageBox(Properties.Resources.Remove_review_tag, String.Format(Properties.Resources.Confirm_remove_review_tag_N, tag.Name), System.Windows.MessageBoxButton.YesNo);
            if (confirm == System.Windows.MessageBoxResult.No)
                return;
            executor.StartBackgroundTask(() => { tasksService.RemoveTaskTag(Project.ProjectUId, tag.TagUId); },
            () => {
                allTaskTags.Remove(tag);
                ((ObservableCollection<TaskTag>)TaskTags).Remove(tag);
                foreach (var vm in Tasks.Where(t => t.Task.TagUId==tag.TagUId))
                    vm.Task.TagUId = null;
            });            
        }

        private void AddNewTag() {
            var newTag = new TaskTag() { ProjectUId = Project.ProjectUId, TagUId = Guid.NewGuid().ToString() };
            newTag.Name = GetNewTagName();
            executor.StartBackgroundTask<TaskTag>(() => { return tasksService.AddTaskTag(Project.ProjectUId, newTag); },
            t => { 
                ((ObservableCollection<TaskTag>)TaskTags).Insert(0, t);
                allTaskTags.Add(t);
                FilterTag = t;
            });
        }

        private string GetNewTagName() {
            if (Project == null)
                return Properties.Resources.Review_tag;
            string name = String.Format(Properties.Resources.TagName_N, Project.CurrentValidSprint.SprintNumber);
            int idx = 1;
            while (TaskTags.Any(t => t.Name.ToLower() == name.ToLower())) {
                name = String.Format(Properties.Resources.TagName_N, Project.CurrentValidSprint.SprintNumber) + "." + idx;
                idx++;
            }                
            return name;

        }
        private ICollection<TaskTag> allTaskTags;
        private ICollection<TaskTag> taskTags;
        public ICollection<TaskTag> TaskTags {
            get {
                return taskTags;
            }
            set {
                taskTags = value;
                OnPropertyChanged("TaskTags");
            }
        }

        private void LoadTaskTags() {
            if (Project == null)
                return;
            executor.StartBackgroundTask<ICollection<TaskTag>>(
                () => { return tasksService.GetTaskTags(Project.ProjectUId); },
                tags => {
                    
                    allTaskTags = new List<TaskTag>(tags);

                    if (TaskTags != null)
                        TaskTags.ClearAndDispose();
                    
                    // show only tags with open tasks, or created 5 days ago
                    DateTime limitDate = System.DateTime.Today.AddDays(-5);
                    TaskTags = new ObservableCollection<TaskTag>(allTaskTags.Where(t => t.OpenTasksCount > 0 || t.CreatedAt >= limitDate).OrderByDescending(t => t.OpenTasksCount).ThenByDescending(t => t.CreatedAt));
                });
        }

        private void CloseItem(BacklogItem item) {
           
            executor.StartBackgroundTask(() => {
                backlogService.ChangeBacklogItemStatus(item.BacklogItemUId, (short) BacklogItemStatus.ITEM_DONE);
            },
            () => {
                ((IEditableCollectionView)backlogViewSource.View).EditItem(item);
                item.Status = (short)BacklogItemStatus.ITEM_DONE;
                ((IEditableCollectionView)backlogViewSource.View).CommitEdit();
                aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, new BacklogItem[] { item });
                
            });
        }

        private void SelectAllTasks() {
            if (Tasks == null)
                return;
            foreach (TaskViewModel task in Tasks)
                task.IsSelected = !task.IsSelected;
        }

        private void PasteTasks() {

            // not a member, cant paste
            if (!IsSignedMemberAtProject) {
                dialogs.ShowAlertMessage(Properties.Resources.Could_not_paste_tasks, Properties.Resources.You_need_to_be_a_project_member_to_paste_tasks, null);
                return;
            }

            if (SelectedBacklogItem == null) {
                dialogs.ShowAlertMessage(Properties.Resources.Could_not_paste_tasks, Properties.Resources.Please_select_a_backlog_item_to_create_the_new_task, null);
                return;
            }

            // gets task data
            System.Windows.DataObject data = System.Windows.Clipboard.GetDataObject() as System.Windows.DataObject;
            if (data == null)
                return;
                        
            // pasting task objects
            ICollection<Task> clipTasks = data.GetData("sf_tasks") as ICollection<Task>;
            if (clipTasks != null) {
                PasteObjectTasks(clipTasks);
                return;
            }
            
            // pasting text
            if (data.ContainsText()) {
                PasteTextTasks(data.GetText());
                return;
            }
            
        }

        private bool IsSignedMemberScrumMaster {
            get {
                if (Project == null || authorizator == null || authorizator.SignedMemberProfile == null)
                    return false;                
               return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        private void PasteObjectTasks(ICollection<Task> clipTasks) {

            // if there are other people tasks and im not a scrum master, cant paste
            bool isThereTaskNotMine = clipTasks.Any(t => (t.TaskAssigneeUId!=null && t.TaskAssigneeUId != authorizator.SignedMemberProfile.MemberUId));
            if (isThereTaskNotMine && !IsSignedMemberScrumMaster) {
                dialogs.ShowAlertMessage(Properties.Resources.Could_not_paste_tasks, Properties.Resources.You_can_only_paste_your_own_tasks, null);
                return;
            }

            // ask before paste
            bool doPaste = ShowBeforePasteDialog(clipTasks.Count);
            if (!doPaste)
                return;

            // paste
            foreach (Task t in clipTasks) {

                Task newtask = new Task();
                newtask.TaskUId = Guid.NewGuid().ToString();
                newtask.TaskName = t.TaskName;
                newtask.TaskType = t.TaskType;
                newtask.PlannedHours = t.PlannedHours;
                newtask.EffectiveHours = t.EffectiveHours;                
                newtask.Status = t.Status;
                newtask.StartDate = t.StartDate;
                newtask.EndDate = t.EndDate;
                newtask.CreatedAt = System.DateTime.Now;
                newtask.Priority = t.Priority;
                newtask.IsAccounting = t.IsAccounting;
                newtask.BacklogItemUId = SelectedBacklogItem.BacklogItemUId;
                
                if(Project.Roles.Any(r => r.RoleUId == t.RoleUId))
                    newtask.RoleUId = t.RoleUId;

                if (ProjectMembers.Any(m => m.MemberUId == t.TaskAssigneeUId))
                    newtask.TaskAssigneeUId = t.TaskAssigneeUId;

                CreateTask(newtask);
            }

        }

        private void PasteTextTasks(string text) {

            if (String.IsNullOrEmpty(text))
                return;

            string[] taskNames = text.Split(new char[] { '\n' });

            // ask before paste
            bool doPaste = ShowBeforePasteDialog(taskNames.Length);
            if (!doPaste)
                return;
            
            foreach (string taskName in taskNames) {

                Task newtask = new Task();
                newtask.TaskUId = Guid.NewGuid().ToString();
                newtask.TaskName = taskName;
                newtask.TaskType = (short)TaskTypes.DEVELOPMENT_TASK;
                newtask.Status = (short)TaskStatus.REQUIRED_TASK;
                newtask.CreatedAt = System.DateTime.Now;
                newtask.IsAccounting = true;
                newtask.BacklogItemUId = SelectedBacklogItem.BacklogItemUId;

                if (!IsSignedMemberScrumMaster) {
                    newtask.TaskAssigneeUId = authorizator.SignedMemberProfile.MemberUId;
                }

                CreateTask(newtask);

            }
        }

        private bool ShowBeforePasteDialog(int n) {
            if (SelectedBacklogItem == null) // no item, no paste
                return false;
            if (n <= 1) // less than two, no need to ask
                return true;            
            System.Windows.MessageBoxResult d = dialogs.ShowMessageBox(Properties.Resources.Pasting_tasks,
                String.Format(Properties.Resources.Pasting_tasks_prompt, n, SelectedBacklogItem.BacklogItemNumber),
                System.Windows.MessageBoxButton.YesNo);

            return d==System.Windows.MessageBoxResult.Yes;
        }

        private void CopyTasksToClipboard() {

            if (SelectedTasksCount==0)
                return;

            ICollection<TaskViewModel> tasks = SelectedTasks;

            List<Task> clipTasks = new List<Task>();

            string textData = string.Empty;
            string htmlData = "<table>";
            foreach (TaskViewModel t in tasks) {
                textData = textData + t.ToString() + System.Environment.NewLine;
                htmlData = htmlData + "<tr>" + t.ToHTMLString() + "</tr>";
                clipTasks.Add(t.Task.Clone());
            }

            htmlData = htmlData + "</table>";
            htmlData = HTMLClipboardHelper.GetHtmlDataString(htmlData);

            System.Windows.DataObject data = new System.Windows.DataObject();
            data.SetData(System.Windows.DataFormats.UnicodeText, textData, true);
            data.SetData(System.Windows.DataFormats.Text, textData, true);
            data.SetData(System.Windows.DataFormats.OemText, textData, true);
            data.SetData(System.Windows.DataFormats.Html, htmlData, true);
            data.SetData("sf_tasks", clipTasks);

            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data);

           
        }

        private void OnTaskSelectedChanged(Task task) {
            OnPropertyChanged("SelectedTasksCount");
            OnPropertyChanged("SelectedTasksPlannedHours");
            OnPropertyChanged("SelectedTasksEffectiveHours");
        }

        private ICollection<TaskViewModel> SelectedTasks {
            get {
                if (Tasks == null)
                    return null;
                return Tasks.Where(t => t.IsSelected).ToArray();
            }
        }

        public int SelectedTasksCount {
            get {
                if (SelectedTasks == null)
                    return 0;
                return SelectedTasks.Count;
            }
        }

        public decimal SelectedTasksPlannedHours {
            get {
                if (SelectedTasksCount == 0)
                    return 0;
                return SelectedTasks.Sum(t => t.Task.PlannedHours);
            }
        }

        public decimal SelectedTasksEffectiveHours {
            get {
                if (SelectedTasksCount == 0)
                    return 0;
                return SelectedTasks.Sum(t => t.Task.EffectiveHours);
            }
        }

      

        private DateTime startClockTime = DateTime.MinValue;

        private void StartStopClock() {
            if (startClockTime == DateTime.MinValue) {
                startClockTime = DateTime.Now;
                PlayIsClockOverSound();
                clockUpdate.StartAction();
            }
            else 
                StopClock();
        }

        private void StopClock() {
            clockUpdate.Stop();
            startClockTime = DateTime.MinValue;
            OnPropertyChanged("ClockTime");
            SetIsClockOver();
        }

        private void UpdateClock() {
            OnPropertyChanged("ClockTime");
            SetIsClockOver();
        }

        private bool isClockTimeOver;
        public bool IsClockTimeOver {
            get {
                return isClockTimeOver;
            }
            set {
                bool old = isClockTimeOver;
                isClockTimeOver = value;
                if (old != isClockTimeOver) {
                    OnPropertyChanged("IsClockTimeOver");
                    if(isClockTimeOver)
                        PlayIsClockOverSound();
                }
            }
        }

        private void PlayIsClockOverSound() {                        
            player.Play();
        }

        private void SetIsClockOver() {

            if (startClockTime == DateTime.MinValue) {
                IsClockTimeOver = false;
                return;
            }

            if (DateTime.Now.Subtract(startClockTime).TotalMinutes > 15) 
                IsClockTimeOver = true;
            else
                IsClockTimeOver = false;
        }

        public string ClockTime {
            get {
                if (startClockTime == DateTime.MinValue)
                    return "00:00";
                TimeSpan time = DateTime.Now.Subtract(startClockTime);
                return time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
            }
        }

        private bool replanItemWhenChanged = false;
        public bool ReplanItemWhenChanged {
            get {
                return replanItemWhenChanged;
            }
            set {
                replanItemWhenChanged = value;                
                OnPropertyChanged("ReplanItemWhenChanged");
                aggregator.Publish<bool>(ScrumFactoryEvent.TaskReplanItemChanged, replanItemWhenChanged);
            }
        }

        private bool canReplanItemWhenChanged = false;
        public bool CanReplanItemWhenChanged {
            get {
                return canReplanItemWhenChanged;
            }
            set {
                canReplanItemWhenChanged = value;
                OnPropertyChanged("CanReplanItemWhenChanged");                
            }
        }

        private bool showAddTaskToolTip;
        public bool ShowAddTaskToolTip {
            get {
                return showAddTaskToolTip;
            }
            set {
                showAddTaskToolTip = value;
                OnPropertyChanged("ShowAddTaskToolTip");
            }
        }

        private void SetShowAddTaskToolTip() {

            if (ListMode == ListModes.LIST_MODE) {
                ShowAddTaskToolTip = false;
                return;
            }

            if (Tasks == null) {
                ShowAddTaskToolTip = true;
                return;
            }

            if (Tasks.Where(t => t.Task.Status == (short)TaskStatus.REQUIRED_TASK).Count() == 0)
                ShowAddTaskToolTip = true;
            else
                ShowAddTaskToolTip = false;
        }


        public void UpdateChangedItems(BacklogItem[] changedItems) {
            AskForRefresh();
        }

        private void OnAppForeground() {
            if (Project == null)
                return;
            AskForRefresh();
        }

        private void ShowTasksForItem(BacklogItem item) {
            ListMode = ListModes.LIST_MODE;
            SelectedBacklogItem = item;
            LoadItems();
            aggregator.Publish<IProjectTabViewModel>(ScrumFactoryEvent.ShowProjectTab, this);
        }

        [Import]
        private UserTasksSelectorViewModel taskSelector { get; set; }

        private void ShowDetail(TaskViewModel task) {            
            DetailViewModel.ShowDetail(task.Task, Project, allTaskTags);
        }

        private void ShowJoinDialog() {
            aggregator.Publish(ScrumFactoryEvent.ShowJoinDialog);
        }

        public ICollection<AssigneeViewModel> ProjectMembers {
            get {
                return projectMembers;
            }
            set {
                projectMembers = value;
                OnPropertyChanged("ProjectMembers");
                OnPropertyChanged("SignedMemberUId");
                OnPropertyChanged("SignedMemberImageUrl");
            }
        }


       
        #region Subscribers





        /// <summary>
        /// Called when task status changed.
        /// </summary>
        /// <param name="task">The task.</param>
        private void OnTaskChanged(Task task) {

            if (Tasks == null)
                return;
            
            // if is a post it view, should change the column of the post it
            TaskViewModel taskVM = Tasks.Where(t => t.Task.TaskUId == task.TaskUId).SingleOrDefault();

            if (taskVM == null)
                return;

            if (taskVM.Task != task) {
                LoadTasks();
                return;
            }

            taskVM.IgnoreChangeCommands = true;
            ((IEditableCollectionView)tasksViewSource.View).EditItem(taskVM);
            taskVM.Task = task;
            ((IEditableCollectionView)tasksViewSource.View).CommitEdit();
            taskVM.RefreshUI();
            taskVM.IgnoreChangeCommands = false;

            SetShowAddTaskToolTip();
            
        }


        /// <summary>
        /// Called when members changed.
        /// Updates each task´s team member combobox items source.
        /// </summary>
        /// <param name="members">The new collection of members.</param>
        private void OnMembersChanged(ICollection<MemberProfile> members) {

            // update members info
            foreach (AssigneeViewModel assignee in ProjectMembers) 
                assignee.Member = members.FirstOrDefault(m => m!=null && m.MemberUId == assignee.MemberUId);

            // add new memberships
            foreach(ProjectMembership newMS in Project.Memberships.Where(ms => !ProjectMembers.Any(pm => pm.MemberUId==ms.MemberUId)))
                ProjectMembers.Add(new AssigneeViewModel(newMS));

            // remove the deleted ones
            for (int i = ProjectMembers.Count - 1; i >= 0; i--) {
                AssigneeViewModel assignee = ProjectMembers.ElementAt(i);
                if (!members.Any(m => m!=null && m.MemberUId == assignee.MemberUId))
                    ProjectMembers.Remove(assignee);
            }

            
            OnPropertyChanged("ProjectMembers");
            OnPropertyChanged("IsSignedMemberAtProject");
            ((DelegateCommand)ShowJoinDialogCommand).NotifyCanExecuteChanged();            
        }

        private void OnRolesChanged(ICollection<Role> roles) {            
            if (Tasks == null)
                return;

            foreach (TaskViewModel task in Tasks)
                task.ProjectRoles = roles;
        }

       

        #endregion

        #region Properties

        private bool showAllItems;
        public bool ShowAllItems {
            get {
                return showAllItems;
            }
            set {
                showAllItems = value;
                LoadItems();
                OnPropertyChanged("ShowAllItems");

            }
        }

        /// <summary>
        /// Gets or sets the new name of the task.
        /// </summary>
        /// <value>The new name of the task.</value>
        public string NewTaskName {
            get {
                return newTaskName;
            }
            set {
                newTaskName = value;                
                OnPropertyChanged("NewTaskName");
                ((DelegateCommand)CreateTaskCommand).NotifyCanExecuteChanged();
            }
        }

        public short NewTaskType {
            get {
                return newTaskType;
            }
            set {
                newTaskType = value;
                OnPropertyChanged("NewTaskType");
            }
        }

        /// <summary>
        /// Gets or sets the list mode.
        /// </summary>
        /// <value>The list mode.</value>
        public ListModes ListMode {
            get {
                return listMode;
            }
            set {
                listMode = value;
                if (View != null && View.IsVisible)
                    LoadTasks();
                OnPropertyChanged("ListMode");
            }
        }

        /// <summary>
        /// Gets or sets the selected backlog item.
        /// </summary>
        /// <value>The selected backlog item.</value>
        public BacklogItem SelectedBacklogItem {
            get {
                return selectedBacklogItem;
            }
            set {
                selectedBacklogItem = value;
                OnPropertyChanged("SelectedBacklogItem");
                if (ListMode == ListModes.POST_IT_MODE)
                    Refresh();
                else
                    if(!isLoadingItems) LoadTasks();
            }
        }

        /// <summary>
        /// Gets the grouped backlog items.
        /// </summary>
        /// <value>The grouped backlog items.</value>
        public ICollectionView GroupedBacklogItems {
            get {
                return backlogViewSource.View;
            }
        }

        /// <summary>
        /// Gets or sets the backlog items.
        /// </summary>
        /// <value>The backlog items.</value>
        public ICollection<BacklogItem> BacklogItems {
            get {
                return backlogItems;
            }
            set {
                backlogItems = value;

                // update the collection view
                backlogViewSource.Source = backlogItems;
                backlogViewSource.GroupDescriptions.Clear();
                backlogViewSource.GroupDescriptions.Add(new PropertyGroupDescription("SprintNumber"));                    
                
                // notify change
                OnPropertyChanged("GroupedBacklogItems");            
                OnPropertyChanged("BacklogItems");
            }
        }

        
        public ICollectionView FilteredTasksPostIt {
            get {
                if (ListMode == ListModes.POST_IT_MODE)
                    return tasksViewSource.View;
                else
                    return null;
            }            
        }


        public ICollectionView FilteredTasksList {
            get {
                if (ListMode == ListModes.LIST_MODE)
                    return tasksViewSource.View;
                else
                    return null;
            }            
        }

        /// <summary>
        /// Gets or sets the search filter text.
        /// </summary>
        /// <value>The search filter text.</value>
        public string SearchFilterText {
            get {
                return this.searchFilterText;
            }
            set {
                this.searchFilterText = value;
                OnPropertyChanged("SearchFilterText");
                delayFilter.StartAction();
            }
        }

        [Import]
        private TaskViewModel DetailViewModel { get; set; }

        public ICollection<TaskViewModel> Tasks { get; set; }


        #endregion

        public string SignedMemberUId {
            get {
                if (authorizator.SignedMemberProfile == null)
                    return null;
                return authorizator.SignedMemberProfile.MemberUId;
            }
        }

        public string SignedMemberImageUrl {
            get {
                if (authorizator.SignedMemberProfile == null)
                    return null;
                if (ProjectMembers == null)
                    return null;
                return ProjectMembers.Where(m => m.MemberUId == authorizator.SignedMemberProfile.MemberUId).Select(m => m.MemberAvatarUrl).FirstOrDefault();
            }
        }

        private void ChangeTaskItem(ScrumFactory.Windows.Helpers.DragDrop.DropCommandParameter p) {
            
            // no task, no change
            TaskViewModel taskVM = p.Item as TaskViewModel;
            if (taskVM == null)
                return;

            // no item, no change
            BacklogItem item = p.DropTargetItem as BacklogItem;
            if (item == null)
                return;

            taskVM.ChangeTaskItemCommand.Execute(item.BacklogItemUId);
        }

        private void MoveTask(ScrumFactory.Windows.Helpers.DragDrop.DropCommandParameter p) {

            // no group, no move
            if (p==null || p.Group == null)
                return;
            
            // no task, no move
            TaskViewModel taskVM = p.Item as TaskViewModel;
            if (taskVM == null)
                return;

            short newStatus = (short) p.Group.Name;
            taskVM.MoveTaskToCommand.Execute(newStatus);
            
        }

         
        private void ChangeListMode() {

            if (ListMode == ListModes.LIST_MODE)
                ListMode = ListModes.POST_IT_MODE;
            else
                ListMode = ListModes.LIST_MODE;

            SetShowAddTaskToolTip();
        }


        /// <summary>
        /// Sets the NeedRefresh to true, so when the panel gets visible it loads the tasks again
        /// from the given project.
        /// </summary>
        /// <param name="project">The project to load tasks from.</param>
        private void AskForRefresh() {
            if (View != null && View.IsVisible) {                
                LoadData();
            } else
                NeedRefresh = true;
        }             

        public void Refresh() {

            if (ListMode == ListModes.POST_IT_MODE && FilteredTasksPostIt != null) {
                FilteredTasksPostIt.Refresh();
            }

            if (ListMode == ListModes.LIST_MODE && FilteredTasksList != null)
                FilteredTasksList.Refresh();
        }

        public bool IsSignedMemberAtProject {
            get {
                if (Project == null || Project.Memberships==null || Project.Memberships.All(ms => ms.Member==null))
                    return false;
                return Project.Memberships.Any(ms => ms.Member!=null && ms.Member.MemberUId == authorizator.SignedMemberProfile.MemberUId && ms.IsActive==true);
            }
        }

        private bool CanJoinProject() {
            if (!Project.AnyoneCanJoin)
                return false;
            return !IsSignedMemberAtProject;
        }

        
        private bool CanCreateTask() {
            return IsSignedMemberAtProject && !String.IsNullOrEmpty(NewTaskName);
        }

        private bool CanCreateEmptyTask() {
            return IsSignedMemberAtProject;
        }

        private void CreateTask(Task task) {

            if (!IsSignedMemberAtProject) {
                dialogs.ShowAlertMessage(Properties.Resources.Can_not_create_task, Properties.Resources.Please_join_the_project_to_create_the_new_task, null);
                return;
            }

            if (SelectedBacklogItem == null) {
                dialogs.ShowAlertMessage(Properties.Resources.Can_not_create_task, Properties.Resources.Please_select_a_backlog_item_to_create_the_new_task, null);
                return;
            }

            dialogs.CloseAlertMessage();

            task.ProjectUId = Project.ProjectUId;
            task.Project = Project;
            task.CreatedAt = DateTime.Now;

            // if im not a Scrum Master can only create tasks for myself
            if (!Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                task.TaskAssigneeUId = authorizator.SignedMemberProfile.MemberUId;

            TaskViewModel taskVM = new TaskViewModel(executor, tasksService, aggregator, dialogs, authorizator, task, taskSelector);
            taskVM.SetTaskRoleAccordingAssignee();
            taskVM.BacklogItem = BacklogItems.SingleOrDefault(i => i.BacklogItemUId == task.BacklogItemUId);
            taskVM.ReplanItemWhenChanged = ReplanItemWhenChanged;

            executor.StartBackgroundTask<int>(
                () => { return tasksService.CreateTask(task); },
                taskNumber => {                                        
                    Tasks.Add(taskVM);
                    ((IEditableCollectionView)tasksViewSource.View).EditItem(taskVM);
                    ((IEditableCollectionView)tasksViewSource.View).CommitEdit();
                    NewTaskName = null;
                    taskVM.NotifyAdded();
                    if (String.IsNullOrEmpty(task.TaskName))
                        taskVM.PostItTaskNameTextBoxFocus = true;
                    //else
                    //taskVM.PostItPlannedHoursTextBoxFocus = true;

                    SetShowAddTaskToolTip();

                    aggregator.Publish<Task>(ScrumFactoryEvent.TaskAdded, task);

                });
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        private void CreateTask() {

            if (SelectedBacklogItem == null) {
                dialogs.ShowAlertMessage(Properties.Resources.Can_not_create_task, Properties.Resources.Please_select_a_backlog_item_to_create_the_new_task, null);
                return;
            }

            // task created using the mouse click
            string taskName = NewTaskName;
            if (taskName == null)
                taskName = String.Empty;
            
            Task task = new Task();
            task.TaskUId = Guid.NewGuid().ToString();
            task.Status = (short)TaskStatus.REQUIRED_TASK;
            task.PlannedHours = 0.0M;
            task.EffectiveHours = 0.0M;
            task.TaskName = taskName;
            task.TaskType = NewTaskType;
            task.ProjectUId = Project.ProjectUId;
            task.BacklogItemUId = SelectedBacklogItem.BacklogItemUId;
            task.TaskOwnerUId = authorizator.SignedMemberProfile.MemberUId;
            task.IsAccounting = true;

            if (FilterTag != null)
                task.TagUId = FilterTag.TagUId;
            
            if (Project.DefaultRole != null)
                task.RoleUId = Project.DefaultRole.RoleUId;

            CreateTask(task);
            
        }

        
        

        public Project Project {
            get {
                return project;
            }
            set {
                SetReplanItemWhenChanged(value);
                project = value;

                IsVisible = (project != null);

                isLoadingItems = true;
                SelectedBacklogItem = null;
                isLoadingItems = false;
                ClearTasks();

                if (project == null)                    
                    ProjectMembers = null;
                else                            
                    ProjectMembers = AssigneeViewModel.CreateAssigneeCollection(project.Memberships);                    
                
                AskForRefresh();

                ((DelegateCommand)AddNewTagCommand).NotifyCanExecuteChanged();
                ((DelegateCommand<TaskTag>)RemoveTagCommand).NotifyCanExecuteChanged();
                ((DelegateCommand<TaskTag>)UpdateTagCommand).NotifyCanExecuteChanged();
                OnPropertyChanged("CanNotAddTags");

                OnPropertyChanged("Project");
            }
        }

        private void SetReplanItemWhenChanged(Project newProject) {
            
            // no new project
            if (newProject == null) {
                ReplanItemWhenChanged = false;
                return;
            }

            if (authorizator==null || authorizator.SignedMemberProfile == null) {
                ReplanItemWhenChanged = false;
                return;
            }

            // new project is the same
            if (project != null && project.ProjectUId == newProject.ProjectUId && project.Status == newProject.Status)
                return;

            // sets whenever user can change plan item
            CanReplanItemWhenChanged = newProject.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);

            if (!CanReplanItemWhenChanged) {
                ReplanItemWhenChanged = false;
                return;
            }

            if (newProject.Status == (short)ProjectStatus.PROPOSAL_CREATION)
                ReplanItemWhenChanged = true;
            else
                ReplanItemWhenChanged = false;
        }

        /// <summary>
        /// Make sure to call dispose in every task to unbind commands.
        /// </summary>
        private void ClearTasks() {
            if(Tasks==null) {
                Tasks = new List<TaskViewModel>();
                return;
            }
            ShowDetailWindowCommand = null;
            Tasks.ClearAndDispose();
            OnTaskSelectedChanged(null);

            ShowDetailWindowCommand = new DelegateCommand<TaskViewModel>(ShowDetail);
                        
        }

        private bool isLoadingItems = false;
        private void LoadData() {
            LoadTasks();
            LoadItems();
            LoadTaskTags();
        }

        private void LoadItems() {
            isLoadingItems = true;
            string oldSelectedItemUId = null;
            if (SelectedBacklogItem != null)
                oldSelectedItemUId = SelectedBacklogItem.BacklogItemUId;

            executor.StartBackgroundTask<ICollection<BacklogItem>>(
                () => {
                    if (ShowAllItems == false) {
                        if(!Project.IsTicketProject)
                            return backlogService.GetCurrentBacklog(Project.ProjectUId, (short)BacklogFiltersMode.DAILY_MEETING);
                        else
                            return backlogService.GetCurrentBacklog(Project.ProjectUId, (short)BacklogFiltersMode.PENDING);
                    } else
                        return backlogService.GetCurrentBacklog(Project.ProjectUId, (short)BacklogFiltersMode.ALL);
                },
                items => {
                    
                    BacklogItems = items;
                    if (oldSelectedItemUId != null)
                        SelectedBacklogItem = BacklogItems.SingleOrDefault(i => i.BacklogItemUId == oldSelectedItemUId);

                    isLoadingItems = false;
                });
        }

        private void LoadTasks() {
            executor.StartBackgroundTask<ICollection<Task>>(
                () => {
                    IsLoadingData = true;
                    NeedRefresh = false;


                    System.DateTime from = System.DateTime.MinValue;
                    System.DateTime to = System.DateTime.MinValue;

                    if (FilterTag != null)
                        return tasksService.GetProjectTasks(Project.ProjectUId, from, to, false, FilterTag.TagUId);


                    if (ListMode == ListModes.LIST_MODE) {
                        if (SelectedBacklogItem == null)
                            return null;
                        return tasksService.GetItemTasks(SelectedBacklogItem.BacklogItemUId);
                    }

                    return tasksService.GetProjectTasks(Project.ProjectUId, from, to, true, null);
                },
                OnTasksLoaded);

        }

        private void GroupAndSortTaskSourceView() {

            using(tasksViewSource.DeferRefresh()) {

                if (ListMode == ListModes.POST_IT_MODE) {

                    tasksViewSource.GroupDescriptions.Clear();
                    tasksViewSource.GroupDescriptions.Add(new PropertyGroupDescription("TaskPostItStatus"));
                    tasksViewSource.GroupDescriptions[0].GroupNames.Add((short)TaskStatus.REQUIRED_TASK);
                    tasksViewSource.GroupDescriptions[0].GroupNames.Add((short)TaskStatus.WORKING_ON_TASK);
                    tasksViewSource.GroupDescriptions[0].GroupNames.Add((short)TaskStatus.DONE_TASK);
                    
                }

                if (ListMode == ListModes.LIST_MODE) 
                    tasksViewSource.GroupDescriptions.Clear();
                
            }

        }

        
        /// <summary>
        /// After the service returns all the tasks, create a observable collection of 
        /// tasks view models and set it as source for the collection view source.
        /// </summary>
        /// <param name="tasks"></param>
        private void OnTasksLoaded(ICollection<Task> tasks) {
            
            ClearTasks();

            if (tasks != null) {
                foreach (Task task in tasks) {                    
                    task.Project = Project;    
                    TaskViewModel taskVM = new TaskViewModel(executor, tasksService, aggregator, dialogs, authorizator, task, taskSelector);
                    taskVM.ReplanItemWhenChanged = ReplanItemWhenChanged;
                    Tasks.Add(taskVM);
                }
            }
            
            // sets the filtered view source                                                 
            tasksViewSource.Source = Tasks;

            GroupAndSortTaskSourceView();

            OnPropertyChanged("FilteredTasksPostIt");
            OnPropertyChanged("FilteredTasksList");

            SetShowAddTaskToolTip();

            // notify other modules
            aggregator.Publish<ICollection<Task>>(ScrumFactoryEvent.TasksLoaded, tasks);

            // load data is over
            IsLoadingData = false;

        }

        private void tasksViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {            
            TaskViewModel taskVM = e.Item as TaskViewModel;

            if (taskVM == null) {
                e.Accepted =false;
                return;
            }

            bool itemAccepted = SelectedBacklogItem==null || taskVM.Task.BacklogItemUId.Equals(SelectedBacklogItem.BacklogItemUId);

            bool textAccepted = true;

            if (SearchFilterText != null) {
                string[] tags = SearchFilterText.NormalizeD().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                textAccepted = tags.All(
                    t => taskVM.Task.TaskName.NormalizeD().Contains(t)
                        || taskVM.Task.TaskNumber.ToString()==t
                        || (MemberNameForTask(taskVM).NormalizeD().Contains(t)));
            }

            bool tagAccepted = true;
            if (FilterTag != null)
                tagAccepted = (taskVM.Task.TagUId!=null && taskVM.Task.TagUId.ToLower() == FilterTag.TagUId.ToLower());

            e.Accepted = textAccepted && itemAccepted && tagAccepted;
        }

        private string MemberNameForTask(TaskViewModel taskVM) {
            if (ProjectMembers == null)
                return string.Empty;
            AssigneeViewModel member = ProjectMembers.Where(m => m.MemberUId == taskVM.TaskAssigneeUId).SingleOrDefault();
            if (member == null)
                return string.Empty;
            return member.FullName;
        }

        public string CurrentSprintLabel {
            get {
                if (Project == null)
                    return null;

                if (Project.Sprints == null || Project.Sprints.Count == 0 || Project.IsTicketProject)
                    return null;

                if (!Project.IsRunning || Project.CurrentSprint == null)
                    return null;

                int daysLeft = Project.CurrentSprint.EndDate.Date.Subtract(System.DateTime.Today).Days + 1;
                return string.Format(Properties.Resources.N_days_left, daysLeft);
            }
                
        }

        public ICommand CreateTaskCommand { get; set; }
        public ICommand CreateEmptyTaskCommand { get; set; }
        
        public ICommand ChangeListModeCommand { get; set; }
        public ICommand ShowDetailWindowCommand { get; set; }

        public ICommand MoveTaskCommand { get; set; }
        public ICommand ChangeTaskItemCommand { get; set; }

        public ICommand OnLoadCommand { get; set; }

        public ICommand ShowJoinDialogCommand { get; set; }

        public ICommand StartStopClockCommand { get; set; }

        public ICommand CopyTasksCommand { get; set; }

        public ICommand PasteTasksCommand { get; set; }

        public ICommand SelectAllTasksCommand { get; set; }

        public ICommand SetNewTaskTypeCommand { get; set; }

        public ICommand CloseItemCommand { get; set; }

        public ICommand AddNewTagCommand { get; set; }
        public ICommand RemoveTagCommand { get; set; }
        public ICommand UpdateTagCommand { get; set; }

        #region IPanelViewModel Members

        public string PanelName {
            get { return Properties.Resources.Tasks; }
        }

        public int PanelDisplayOrder {
            get { return 400; }
        }

        private bool isVisible = false;
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
                return false;
            }
        }

        #endregion

        #region IViewModel Members

        [Import(typeof(TasksList))]
        public IView View { get; set; }

        #endregion
    }
}
