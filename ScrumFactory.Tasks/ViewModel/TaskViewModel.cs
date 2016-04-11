using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using System.Linq;
using System.Windows.Data;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;


namespace ScrumFactory.Tasks.ViewModel  {

    [Export]
    public class TaskViewModel : BaseEditableObjectViewModel, IViewModel, INotifyPropertyChanged {

        private IBackgroundExecutor executor;
        private ITasksService tasksService;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;

        private IDialogService dialogs;

        private ICollection<Role> projectRoles;
        private ICollection<AssigneeViewModel> projectMembers;
       
        private Task task;
        private Task oldTask;

        
        private string panelName;

        private Project project;
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
                OnPropertyChanged("Project");
            }
        }

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }

        [Import]
        public IArtifactsListViewModel ArtifactListViewModel { get; set; }
       

        [Import]
        private IBacklogService backlogService { get; set;}

        [Import]
        private IProjectsService projectsService { get; set;}

        [ImportMany]
        private IEnumerable<IPluginCommand> allPluginMenuItems { get; set; }

        public IEnumerable<IPluginCommand> PluginMenuItems {
            get {
                if (allPluginMenuItems == null)
                    return null;
                return allPluginMenuItems.Where(m => m.ContainerViewModelClassName.Equals(this.GetType().ToString())).OrderBy(m => m.DisplayOrder).ToList();
            }
        }
        
        public FinishTaskDialogViewModel FinishDialog { get; set; }

        private bool postItPlannedHoursTextBoxFocus;

        public bool PostItPlannedHoursTextBoxFocus {
            get { return postItPlannedHoursTextBoxFocus; }
            set {                 
                postItPlannedHoursTextBoxFocus = value;
                OnPropertyChanged("PostItPlannedHoursTextBoxFocus");
            }
        }

        private bool postItTaskNameTextBoxFocus;

        public bool PostItTaskNameTextBoxFocus {
            get { return postItTaskNameTextBoxFocus; }
            set {
                postItTaskNameTextBoxFocus = value;
                OnPropertyChanged("PostItTaskNameTextBoxFocus");
            }
        }

        private UserTasksSelectorViewModel selectorViewModel;

        private int? taskNumberToShowOnInit;

        [ImportingConstructor]
        public TaskViewModel(
            [Import] IBackgroundExecutor executor,
            [Import] ITasksService tasksService,
            [Import] IEventAggregator aggregator,
            [Import] IDialogService dialogs,
            [Import] IAuthorizationService authorizationService)
            : this(executor, tasksService, aggregator, dialogs, authorizationService, null, null) {

                aggregator.Subscribe<Task>(ScrumFactoryEvent.ShowTaskDetail, ShowDetail, 1);

                aggregator.Subscribe<int>(ScrumFactoryEvent.TaskArgOnInit, OnTaskArgOnInit);

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, ShowDetailOnInit);
                
                CloseWindowCommand = new DelegateCommand(CloseWindow);

                // if its on detail screen, need to save other tasks changes also
                FinishTaskCommand = new DelegateCommand(CanFinishTask, () => { ChangeTaskStatusAndClose((short)TaskStatus.DONE_TASK); });


        }
   
        /// <summary>
        /// Constructor for lifeless viewmodels.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="tasksService"></param>
        /// <param name="aggregator"></param>
        /// <param name="authorizationService"></param>
        /// <param name="task"></param>
        public TaskViewModel(
            IBackgroundExecutor executor,
            ITasksService tasksService,
            IEventAggregator aggregator,
            IDialogService dialogs,
            IAuthorizationService authorizationService,
            Task task,
            UserTasksSelectorViewModel selector)  {

            if(task!=null)
                Task = task;            
            
            this.executor = executor;
            this.tasksService = tasksService;
            this.aggregator = aggregator;
            this.authorizator = authorizationService;
            this.selectorViewModel = selector;
            this.dialogs = dialogs;
            
            aggregator.Subscribe<ICollection<MemberProfile>>(ScrumFactoryEvent.ProjectMembersChanged, OnMembersChanged);
            aggregator.Subscribe<string>(ScrumFactoryEvent.TaskTrackChanged, OnTaskTrackChanged);

            aggregator.Subscribe(ScrumFactoryEvent.ApplicationWhentBackground, OnApplicationWhentBackground);

            aggregator.Subscribe<bool>(ScrumFactoryEvent.TaskReplanItemChanged, r => { ReplanItemWhenChanged = r; });
            
            MoveTaskToCommand = new DelegateCommand<short>(MoveTaskTo);
            MoveTaskLeftCommand = new DelegateCommand(CanMoveTaskLeft, MoveTaskLeft);
            MoveTaskRightCommand = new DelegateCommand(CanMoveTaskRight, MoveTaskRight);
            CancelTaskCommand = new DelegateCommand(CanCancelTask, CancelTask);
            FinishTaskCommand = new DelegateCommand(CanFinishTask, FinishTask);

            AssignTaskToMeCommand = new DelegateCommand(AssignTaskToMe);

            CopyTaskTrackIdCommand = new DelegateCommand(CopyTaskTrackId);

            ShowDetailCommand = new DelegateCommand(Show);

            ShowFinishDialogCommand = new DelegateCommand(ShowFinishDialog);
            
            ChangeTaskNameCommand = new DelegateCommand(ChangeTaskName);

            ChangePlannedHoursCommand = new DelegateCommand(ChangePlannedHours);
            ChangeEffectiveHoursCommand = new DelegateCommand(ChangeEffectiveHours);

            ChangeTaskAssigneeCommand = new DelegateCommand(ChangeTaskAssigneeUId);

            ChangeTaskItemCommand = new DelegateCommand<string>(ChangeTaskItem);

            ShowRepositoryLogCommand = new DelegateCommand(ShowRepositoryLog);

            TrackTaskCommand = new DelegateCommand(TrackTask);

            ResetTaskAssigneeCommand = new DelegateCommand(ResetTaskAssignee);

        }

        private ICollection<TaskTag> tags;
        public ICollection<TaskTag> Tags {
            get {
                return tags;
            }
            set {
                tags = new List<TaskTag>();
                tags.Add(new TaskTag() { Name = Properties.Resources.None, TagUId = null });
                if(value!=null)
                    ((List<TaskTag>)tags).AddRange(value.OrderByDescending(t=> t.CreatedAt));                
                OnPropertyChanged("Tags");
            }
        }

        public string TagUId {
            get {
                if (task == null)
                    return null;
                return task.TagUId;
            }
            set {
                if (task == null)
                    return;
                task.TagUId = value;
                OnPropertyChanged("TagUId");
            }
        }

        public void ResetTaskAssignee() {
            TaskAssigneeUId = null;
        }

        public bool CanTrackTask {
            get {
                if (authorizator == null || authorizator.SignedMemberProfile==null || Task==null)
                    return false;
                if (Task.Status == (short)TaskStatus.DONE_TASK)
                    return false;
                return (Task.TaskAssigneeUId == authorizator.SignedMemberProfile.MemberUId);
            }
        }

        public bool HasArtifact {
            get {
                if (Task == null || Task.ArtifactCount == null)
                    return false;
                return Task.ArtifactCount > 0;
            }
        }

        public string BacklogItemAndGroupName {
            get {                
                if (BacklogItem == null)
                    return null;
                if(BacklogItem.Group==null)
                    return BacklogItem.Name + " [" + BacklogItem.BacklogItemNumber + "]";

                return BacklogItem.Group.GroupName + " > " + BacklogItem.Name;
            }
        }
        
        private bool isSelected;
        public override bool IsSelected {
            get {
                return isSelected;
            }
            set {
                isSelected = value;
                aggregator.Publish<Task>(ScrumFactoryEvent.TaskSelectedChanged, Task);
                OnPropertyChanged("IsSelected");
            }
        }

        private void ShowRepositoryLog() {
            int backlogItemNumber;
            if (BacklogItem == null)
                backlogItemNumber = Task.TaskInfo.BacklogItemNumber;
            else
                backlogItemNumber = BacklogItem.BacklogItemNumber;
            string filter = "#" + Project.ProjectNumber + "." + backlogItemNumber + "." + Task.TaskNumber + "#";
            aggregator.Publish<string>(ScrumFactoryEvent.ShowCodeRepositoryLog, filter);
        }

        public override string ToString() {
            if (Task == null)
                return String.Empty;
            string status = Enum.GetName(typeof(TaskStatus), Task.Status);
            string taskRole = String.Empty;
            if (TaskRole != null)
                taskRole = TaskRole.RoleShortName;
            return status + "\t" + Task.TaskNumber + "\t" + Task.TaskName + "\t" + Task.TaskAssigneeUId + "\t" + Task.PlannedHours + "\t" + Task.EffectiveHours + "\t" + taskRole;
        }

        public string ToHTMLString() {
            if (Task == null)
                return String.Empty;
            string status = Enum.GetName(typeof(TaskStatus), Task.Status);
            string taskRole = String.Empty;
            if (TaskRole != null)
                taskRole = TaskRole.RoleShortName;
            return "<td>" + status + "</td><td><a href=\"" + GetHTMLLink(authorizator.ServerUrl.Url, Task) + "\">" + Task.TaskNumber + "</a></td><td>" + Task.TaskName + "</td><td>" + Task.TaskAssigneeUId + "</td><td>" + Task.PlannedHours + "</td><td>" + Task.EffectiveHours + "</td><td>" + taskRole + "</td>";
        }

        public bool IgnoreChangeCommands { get; set; }

        private void OnTaskArgOnInit(int taskNumber) {

            taskNumberToShowOnInit = taskNumber;

            // if not logged yet, wait for login
            if (authorizator.SignedMemberProfile == null)
                return;

            ShowDetailOnInit(authorizator.SignedMemberProfile);

            
        }

        private void OnApplicationWhentBackground() {
            if(TaskHasChanged)
                SaveTask();
        }
   
        private void AssignTaskToMe() {
            TaskAssigneeUId = authorizator.SignedMemberProfile.MemberUId;
            ChangeTaskAssigneeUId();
            RefreshUI();
            
        }

        public decimal TaskEffetiveHours {
            get {
                if (Task == null)
                    return 0;
                return Task.EffectiveHours;
            }
            set {
                Task.EffectiveHours = value;
                if (Task.EffectiveHours > 0 && Task.Status == (short)TaskStatus.REQUIRED_TASK)
                    Task_Status = (short)TaskStatus.WORKING_ON_TASK;
                OnPropertyChanged("TaskEffetiveHours");
            }
        }

        public Role TaskRole {
            get {                
                if (ProjectRoles == null)
                    return null;
                return ProjectRoles.SingleOrDefault(r => r.RoleUId == Task.RoleUId);
            }
            set {
                if (isLoadingRoles)
                    return;
                if (value == null)
                    Task.RoleUId = null;
                else
                    Task.RoleUId = value.RoleUId;
                OnPropertyChanged("TaskRole");
         
                
            }
        }


        
        public ICollection<Role> ProjectRoles {
            get {
                return projectRoles;
            }
            set {
                isLoadingRoles = true;
                projectRoles = value;
                OnPropertyChanged("ProjectRoles");
                isLoadingRoles = false;
            }
        }

        private BacklogItem backlogItem;
        public BacklogItem BacklogItem {
            get {
                return backlogItem;
            }
            set {
                backlogItem = value;                
                OnPropertyChanged("BacklogItem");
                OnPropertyChanged("BacklogItemAndGroupName");                
                SetPanelName();
            }
        }

        
        private ICollection<MemberProfile> members;

        private void OnMembersChanged(ICollection<MemberProfile> members) {
            this.members = members;
            UpdateMembersInfo();            
        }

        private void UpdateMembersInfo() {
            if (ProjectMembers == null || members==null)
                return;
            foreach (AssigneeViewModel assignee in ProjectMembers)
                assignee.Member = members.FirstOrDefault(m => m.MemberUId == assignee.MemberUId);            

        }       


        private void SetPanelName() {
            if (BacklogItem != null)
                PanelName = Properties.Resources.Task + " " + Task.TaskNumber;
        }

    

        public void ShowDetailOnInit(MemberProfile member) {

            if (member == null)
                return;

            if (taskNumberToShowOnInit == null)
                return;

            executor.StartBackgroundTask<Task>(
                () => { return tasksService.GetTask(taskNumberToShowOnInit.ToString()); },
                t => {
                    taskNumberToShowOnInit = null;
                    ShowDetail(t);
                });
        }

        public void ShowDetail(Task task) {
            ShowDetail(task, null, null);
        }

        private bool isLoadingRoles = false;
        
        public void ShowDetail(Task task, Project project, ICollection<TaskTag> tags) {

            Task = task;
            oldTask = Task.Clone();
            this.Project = project;
                        
            // no project, get it SYNC                       
            if (project == null) {
                this.Project = projectsService.GetProject(task.ProjectUId);                
                //aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, this.project);
            }

            ProjectRoles = this.Project.Roles;
            ProjectMembers = AssigneeViewModel.CreateAssigneeCollection(this.Project.Memberships);

            // just to make sure tha lines above did not mess my roleUId value
            Task.RoleUId = oldTask.RoleUId;
          
            // no tags, get tags async
            if (tags == null) {
                executor.StartBackgroundTask<ICollection<TaskTag>>(
                    () => { return tasksService.GetTaskTags(Project.ProjectUId); },
                    ts => {
                        Tags = ts;
                        TagUId = oldTask.TagUId;
                    });
            } else {
                this.Tags = tags;
            }


            // no detail , get it async
            if (Task.TaskDetail == null) {
                executor.StartBackgroundTask<ScrumFactory.TaskDetail>(
                    () => { return tasksService.GetTaskDetail(Task.TaskUId); },
                    d => {
                        if (d != null) {
                            TaskDetail = d.Detail;
                            oldTask.TaskDetail = d;
                        }
                        else {
                            TaskDetail = null;
                            oldTask.TaskDetail = null;
                        }
                    });
            }
            else
                TaskDetail = Task.TaskDetail.Detail;

            // no backlog item, get it async            
            executor.StartBackgroundTask<BacklogItem>(
                () => { return backlogService.GetBacklogItem(task.BacklogItemUId); },
                i => { BacklogItem = i; });                           

            Show();
        }

        public string TaskDetail {
            get {
                if (Task==null || Task.TaskDetail == null)
                    return null;
                return Task.TaskDetail.Detail;
            }
            set {

                if (value == null) {
                    Task.TaskDetail = null;
                    OnPropertyChanged("TaskDetail");
                    return;
                }
                
                if(Task.TaskDetail==null)
                    Task.TaskDetail = new ScrumFactory.TaskDetail() { TaskUId = Task.TaskUId };
                Task.TaskDetail.Detail = value;
                OnPropertyChanged("TaskDetail");
            }
        }

        public void Show() {                
            SetPanelName();
            RefreshUI();

            ArtifactListViewModel.ChangeContext(ArtifactContexts.TASK_ARTIFACT, Task.TaskUId, RefreshTaskArtifactCount);

            Show(projectContainer.Value);
        }

        private void RefreshTaskArtifactCount(int count) {
            int? oldCount = Task.ArtifactCount;
            if (!oldCount.HasValue)
                oldCount = 0;

            Task.ArtifactCount = count;
            
            if ((oldCount == 0) != (count == 0))
                aggregator.Publish<Task>(ScrumFactoryEvent.TaskChanged, Task);
        }

        /// <summary>
        /// Closes the window, invoke the OnCloseAction and publishes the CloseWindow event.
        /// </summary>
        private void CloseWindow() {
            if (TaskHasChanged)
                SaveTask();
            oldTask = null;
            Close();

            dialogs.GoBackSelectedTopMenu();
          
            
        }

        private void SaveTask() {

            bool assigneeHasChanged = (oldTask != null && oldTask.TaskAssigneeUId != task.TaskAssigneeUId);

            if (!CanEditThisTask && !assigneeHasChanged)
                return;

            executor.StartBackgroundTask(
                () => { tasksService.SaveTask(Task, ReplanItemWhenChanged); },
                () => {
                    
                    // if task assignee changed, tells everyone
                    if (assigneeHasChanged)
                        aggregator.Publish<Task>(ScrumFactoryEvent.TaskAssigneeChanged, Task);

                    aggregator.Publish<Task>(ScrumFactoryEvent.TaskChanged, Task);
                    aggregator.Publish(ScrumFactoryEvent.BacklogReplannedByTask);
                });

        }

        public Task Task {
            get {
                return task;
            }
            set {                
                task = value;

                TaskDetail = null;

                oldPlannedHours = task.PlannedHours;
                oldEffectiveHours = task.EffectiveHours;
                
                if (task!=null && task.Project != null) {
                    this.Project = task.Project;
                    ProjectRoles = Task.Project.Roles; 
                }

              
                
                OnPropertyChanged("Task");
                OnPropertyChanged("TaskEffetiveHours");
                OnPropertyChanged("TaskPostItStatus");
                OnPropertyChanged("IsFinished");
                OnPropertyChanged("IsCanceled");
                OnPropertyChanged("TaskStartDate");
                OnPropertyChanged("TaskEndDate");
                OnPropertyChanged("FormatedEndDate");
                OnPropertyChanged("FormatedStartDate");
                OnPropertyChanged("TaskMoveLeftToolTip");
                OnPropertyChanged("TaskMoveRightToolTip");
                OnPropertyChanged("CanChangeAssignee");
                OnPropertyChanged("CanEditThisTask");
                OnPropertyChanged("CanNotEditThisTask");
                OnPropertyChanged("TagUId");
            }
        }

        private decimal oldPlannedHours;
        private decimal oldEffectiveHours;

        private bool TaskHasChanged {
            get {
                if (Task == null)
                    return false;

                // if is a list task
                if (oldTask == null) {
                    return oldPlannedHours != Task.PlannedHours || oldEffectiveHours != Task.EffectiveHours;
                }

                // if is a detail
                return !Task.IsTheSame(oldTask);
            }
        }


        public short Task_Status {
            get {
                if (Task == null)
                    return 0;
                return Task.Status;
            }
            set {
                Task.Status = value;
                RefreshUI();
            }
        }

        public DateTime? TaskStartDate {
            get {
                if (Task == null)
                    return null;
                return Task.StartDate;
            }
            set {
                Task.StartDate = value;
            }
        }

        public DateTime? TaskEndDate {
            get {
                if (Task == null)
                    return null;
                return Task.EndDate;
            }
            set {
                Task.EndDate = value;
            }
        }

        
        public ICollection<AssigneeViewModel> ProjectMembers {
            get {
                return projectMembers;
            }
            set {
                isLoadingRoles = true;
                projectMembers = value;
                UpdateMembersInfo();
                string assigneeUId = Task.TaskAssigneeUId;
                OnPropertyChanged("ProjectMembers");
                TaskAssigneeUId = assigneeUId;
                isLoadingRoles = false;
            }
        }


        public bool IsCanceled {
            get {
                return Task.Status == (short)TaskStatus.CANCELED_TASK;
            }
        }

        public bool IsFinished {
            get {
                return Task.Status == (short)TaskStatus.DONE_TASK;
            }
            set {
                if(value)
                    ChangeTaskStatus((short)TaskStatus.DONE_TASK);
                else
                    ChangeTaskStatus((short)TaskStatus.WORKING_ON_TASK);
            }
        }


        public string TaskAssigneeUId {
            get {
                if (Task == null)
                    return null;
                return Task.TaskAssigneeUId;
            }
            set {
                Task.TaskAssigneeUId = value;
                OnPropertyChanged("TaskAssigneeUId");
                OnPropertyChanged("CanTrackTask");
                
                if(!isLoadingRoles)
                    SetTaskRoleAccordingAssignee();
            }
        }

        public AssigneeViewModel TaskAssignee {
            get {
                if (ProjectMembers == null || Task == null)
                    return null;
                return ProjectMembers.SingleOrDefault(t => t.MemberUId == Task.TaskAssigneeUId);
            }       
        }

        public void SetTaskRoleAccordingAssignee() {
            if (Task == null || Project == null || Project.Memberships==null || Task.TaskAssigneeUId==null || Project.Roles==null)
                return;
            ProjectMembership membership = Project.Memberships.FirstOrDefault(ms => ms.MemberUId == Task.TaskAssigneeUId && ms.IsActive==true);
            if (membership == null)
                return;
            TaskRole = Project.Roles.SingleOrDefault(r => r.RoleUId == membership.RoleUId);
            if(TaskRole!=null)
                Task.RoleUId = TaskRole.RoleUId;
        }


        public bool CaNotEditThisTask {
            get {
                return !CanEditThisTask;
            }
        }

        public bool CanEditThisTask {
            get {
                if (Task == null || Project == null || authorizator.SignedMemberProfile==null)
                    return false;
                return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER)
                    || Task.TaskAssigneeUId==authorizator.SignedMemberProfile.MemberUId;
            }
        }

        public bool CanChangeAssignee {
            get {
                if (Task == null || Project == null || authorizator.SignedMemberProfile == null)
                    return false;
                return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        private bool CanMoveTaskLeft() {
            if (Task == null)
                return false;
            return TaskPostItStatus != (short)TaskStatus.REQUIRED_TASK && CanEditThisTask;
        }

        private bool CanMoveTaskRight() {
            if (Task == null)
                return false;
            return TaskPostItStatus != (short)TaskStatus.DONE_TASK && CanEditThisTask;            
        }

        private bool CanFinishTask() {
            if (Task == null)
                return false;
            return Task.Status != (short)TaskStatus.DONE_TASK && Task.Status != (short)TaskStatus.CANCELED_TASK && CanEditThisTask;
        }

        private bool CanCancelTask() {
            if (Task == null)
                return false;
            return Task.Status != (short)TaskStatus.CANCELED_TASK && CanEditThisTask;
        }


        private void CancelTask() {
            ChangeTaskStatus((short)TaskStatus.CANCELED_TASK);            
        }

        private void FinishTask() {
            if (isDisposed)
                return;
            ChangeTaskStatus((short)TaskStatus.DONE_TASK);
            Close();
        }

        private void MoveTaskTo(short status) {
            ChangeTaskStatus(status);
        }

        private void MoveTaskLeft() {
            if (Task.Status == (short)TaskStatus.CANCELED_TASK)
                ChangeTaskStatus((short)TaskStatus.WORKING_ON_TASK);
            else
                ChangeTaskStatus((short)(Task.Status - 1));
        }

        private void MoveTaskRight() {
            ChangeTaskStatus((short)(Task.Status + 1));
        }

        private void ChangeTaskStatusAndClose(short status) {

            if (isDisposed || IgnoreChangeCommands)
                return;

            if (IsTracking && (status == (short)TaskStatus.DONE_TASK || status == (short)TaskStatus.CANCELED_TASK))
                selectorViewModel.StopTaskTrack();

            Task.Status = status;

            CloseWindow();

        }

        private void ChangeTaskStatus(short status) {

            if (isDisposed || IgnoreChangeCommands)
                return;

            if (IsTracking && (status == (short)TaskStatus.DONE_TASK || status == (short)TaskStatus.CANCELED_TASK))
                selectorViewModel.StopTaskTrack();
                        
            executor.StartBackgroundTask(
                () => { return tasksService.ChangeTaskStatus(Task.TaskUId, 0, status); },
                date => {
                    Task.Status = status;
                    Task.AdjustDateWithStatus(date);                    
                    RefreshUI();
                    aggregator.Publish<Task>(ScrumFactoryEvent.TaskChanged, Task);                    
                });
        }

        private void ChangeTaskAssigneeUId() {
            if (isDisposed || IgnoreChangeCommands)
                return;
            executor.StartBackgroundTask(
                () => { tasksService.ChangeTaskAssigneeUId(Task.TaskUId, Task.TaskAssigneeUId, ReplanItemWhenChanged); },
                () => {
                    aggregator.Publish<Task>(ScrumFactoryEvent.TaskAssigneeChanged, Task);
                    if (ReplanItemWhenChanged)
                        aggregator.Publish(ScrumFactoryEvent.BacklogReplannedByTask);
                });
        }

        private void ChangeTaskName() {
            if (isDisposed || IgnoreChangeCommands)
                return;
            executor.StartBackgroundTask(
                () => { tasksService.ChangeTaskName(Task.TaskUId, Task.TaskName); },
                () => {});
        }

        private void ChangeTaskItem(string backlogItemUId) {
            if (isDisposed || IgnoreChangeCommands)
                return;
            executor.StartBackgroundTask(
             () => { tasksService.ChangeTaskItem(Task.TaskUId, backlogItemUId); },
             () => {
                 Task.BacklogItemUId = backlogItemUId;
             });
        }

        private void ChangePlannedHours() {
            if (isDisposed || IgnoreChangeCommands || oldPlannedHours == Task.PlannedHours)
                return;            
            executor.StartBackgroundTask(
                () => {                    
                        tasksService.ChangeTaskPlannedHours(Task.TaskUId, Task.PlannedHours, ReplanItemWhenChanged);                        
                        },
                () => {
                    oldPlannedHours = task.PlannedHours;
                    if (ReplanItemWhenChanged)
                        aggregator.Publish(ScrumFactoryEvent.BacklogReplannedByTask);
                });
        }

        private void ChangeEffectiveHours() {
            if (isDisposed || IgnoreChangeCommands || oldEffectiveHours==Task.EffectiveHours)
                return;
            executor.StartBackgroundTask(
                () => { tasksService.ChangeTaskEffectiveHours(Task.TaskUId, Task.EffectiveHours, false); },
                () => {
                    oldEffectiveHours = task.EffectiveHours;
                });
        }

        public bool ReplanItemWhenChanged { get; set; }

        public void RefreshUI() {
            OnPropertyChanged("HasArtifact");
            OnPropertyChanged("Task");
            OnPropertyChanged("Task_Status");
            OnPropertyChanged("TaskRole");
            OnPropertyChanged("TaskAssigneeUId");
            OnPropertyChanged("ProjectMembers");
            OnPropertyChanged("ProjectRoles");
            OnPropertyChanged("TaskPostItStatus");
            OnPropertyChanged("IsFinished");
            OnPropertyChanged("IsCanceled");
            OnPropertyChanged("TaskStartDate");
            OnPropertyChanged("TaskEndDate");
            OnPropertyChanged("FormatedEndDate");
            OnPropertyChanged("FormatedStartDate");
            OnPropertyChanged("FormatedEllipsedHours");
            OnPropertyChanged("TaskMoveLeftToolTip");
            OnPropertyChanged("TaskMoveRightToolTip");
            OnPropertyChanged("CanChangeAssignee");
            OnPropertyChanged("CanEditThisTask");
            OnPropertyChanged("CanNotEditThisTask");
            
            
            if(CancelTaskCommand!=null)
                ((DelegateCommand)CancelTaskCommand).NotifyCanExecuteChanged();
            if(FinishTaskCommand!=null)
                ((DelegateCommand)FinishTaskCommand).NotifyCanExecuteChanged();            
        }

        public short TaskPostItStatus {
            get {
                if (Task.Status == (short)TaskStatus.CANCELED_TASK)
                    return (short)TaskStatus.DONE_TASK;
                return Task.Status;
            }
        }


        public string TaskMoveLeftToolTip {
            get {
                if (Task.Status == (short)TaskStatus.WORKING_ON_TASK)
                    return Properties.Resources.Set_this_task_as_planned;
                return Properties.Resources.Set_this_task_as_working_on;
            }
        }

        public string TaskMoveRightToolTip {
            get {
                if (Task.Status == (short)TaskStatus.WORKING_ON_TASK)
                    return Properties.Resources.Finish_this_task;
                return Properties.Resources.Set_this_task_as_working_on;
            }
        }


        public ICommand ChangeTaskNameCommand { get; set; }
        public ICommand ChangeTaskAssigneeCommand { get; set; }
        public ICommand ChangePlannedHoursCommand { get; set; }
        public ICommand ChangeEffectiveHoursCommand { get; set; }

        public ICommand ChangeTaskItemCommand { get; set; }

        public ICommand MoveTaskToCommand { get; set; }
        public ICommand MoveTaskLeftCommand { get; set; }
        public ICommand MoveTaskRightCommand { get; set; }
        public ICommand CancelTaskCommand { get; set; }

        public ICommand FinishTaskCommand { get; set; }
        
        public ICommand CloseWindowCommand { get; set; }

        public ICommand ShowDetailCommand { get; set; }

        public ICommand ShowFinishDialogCommand { get; set; }
        
        public ICommand CopyTaskTrackIdCommand { get; set; }

        public ICommand AssignTaskToMeCommand { get; set; }

        public ICommand ShowRepositoryLogCommand { get; set; }

        public ICommand TrackTaskCommand { get; set; }

        public ICommand ResetTaskAssigneeCommand { get; set; }


        /// <summary>
        /// Make sure to unbind all commands.
        /// </summary>
        protected override void OnDispose() {

            aggregator.UnSubscribeAll(this);
            
            ChangeTaskNameCommand = null; OnPropertyChanged("ChangeTaskNameCommand");
            ChangeTaskAssigneeCommand = null; OnPropertyChanged("ChangeTaskAssigneeCommand");
            ChangePlannedHoursCommand = null; OnPropertyChanged("ChangePlannedHoursCommand");
            ChangeEffectiveHoursCommand = null; OnPropertyChanged("ChangeEffectiveHoursCommand");
            ChangeTaskItemCommand = null; OnPropertyChanged("ChangeTaskItemCommand");
            MoveTaskToCommand = null; OnPropertyChanged("MoveTaskToCommand");
            MoveTaskLeftCommand = null; OnPropertyChanged("MoveTaskLeftCommand");
            MoveTaskRightCommand = null; OnPropertyChanged("MoveTaskRightCommand");
            CancelTaskCommand = null; OnPropertyChanged("CancelTaskCommand");
            FinishTaskCommand = null; OnPropertyChanged("FinishTaskCommand");
            CloseWindowCommand = null; OnPropertyChanged("CloseWindowCommand");
            ShowDetailCommand = null; OnPropertyChanged("ShowDetailCommand");

            AssignTaskToMeCommand = null; OnPropertyChanged("AssignTaskToMeCommand");

            ShowFinishDialogCommand = null; OnPropertyChanged("ShowFinishDialogCommand");

            CopyTaskTrackIdCommand = null; OnPropertyChanged("CopyTaskTrackIdCommand");

            ShowRepositoryLogCommand = null; OnPropertyChanged("ShowRepositoryLogCommand");

            TrackTaskCommand = null; OnPropertyChanged("TrackTaskCommand");

            ResetTaskAssigneeCommand = null; OnPropertyChanged("ResetTaskAssigneeCommand");
            
        }



        public bool IsTracking {
            get {
                if (selectorViewModel == null || Task==null)
                    return false;
                return (selectorViewModel.TrackingTaskUId==Task.TaskUId);
            }            
        }

        private void OnTaskTrackChanged(string trackingUId) {
            if (Task == null)
                return;
            OnPropertyChanged("IsTracking");
            OnPropertyChanged("FormatedEllipsedHours");
            
        }

        /// <summary>
        /// Gets the formated ellipsed hours.
        /// </summary>
        /// <value>The formated ellipsed hours.</value>
        public string FormatedEllipsedHours {
            get {
                if (selectorViewModel==null || selectorViewModel.TrackingTaskUId==null)
                    return String.Empty;
                return selectorViewModel.FormatedEllipsedHours;
            }
        }

        public string GetTaskTrackId() {

            int projectNumber;
            int backlogItemNumber;

            // if has a taskInfo (called from taskSelector)
            if (Task.TaskInfo != null) {
                projectNumber = Task.TaskInfo.ProjectNumber;
                backlogItemNumber = Task.TaskInfo.BacklogItemNumber;
            }
                // if has a backlogitem (called from the detail)
            else {
                projectNumber = Project.ProjectNumber;
                backlogItemNumber = BacklogItem.BacklogItemNumber;
            }

            return Task.GetTaskTrackId(projectNumber, backlogItemNumber);
        }

        private static string GetHTMLLink(string serverUrl, Task task) {
            return serverUrl + "/tasks/" + task.TaskNumber;
        }

        private void CopyTaskTrackId() {

            string trackId = GetTaskTrackId();            
            TaskViewModel.CopyTaskTrackId(this.Task, trackId, authorizator.ServerUrl.Url);
        }

        public static void CopyTaskTrackId(Task task, string trackId, string serverUrl) {
            string text = task.TaskName + " " + trackId;
            string link = GetHTMLLink(serverUrl, task);
            string html = task.TaskName + " <a href=\"" + link + "\">" + trackId + "</a>";
            html = HTMLClipboardHelper.GetHtmlDataString(html);

            System.Windows.DataObject data = new System.Windows.DataObject();
            data.SetData(System.Windows.DataFormats.UnicodeText, text, true);
            data.SetData(System.Windows.DataFormats.Text, text, true);
            data.SetData(System.Windows.DataFormats.OemText, text, true);
            data.SetData(System.Windows.DataFormats.Html, html, true);

            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data, true);
        }


        private void TrackTask() {
            if (selectorViewModel == null)
                return;
            if (IsTracking)
                selectorViewModel.StopTaskTrack();
            else                
                selectorViewModel.StartTrack(Task);
        }

        /// <summary>
        /// Shows the finish dialog.
        /// </summary>
        /// <param name="taskVM">The task VM.</param>
        private void ShowFinishDialog() {
            if (FinishDialog == null)
                return;
            // if im closing the tracking task, stop to track it
            if (IsTracking)
                FinishDialog.AddingHours = (decimal) selectorViewModel.TrackEllipsedTime.TotalHours;
            else
                FinishDialog.AddingHours = 0.0M;
            
            aggregator.Publish<Task>(ScrumFactoryEvent.ShowFinishTaskDialog, this.Task);

            if (FinishDialog.TaskWasFinished && selectorViewModel.TrackingTaskUId==this.Task.TaskUId)
                selectorViewModel.ResetTaskTrack();
        }

        ~TaskViewModel() {
            if(Task!=null)
                Console.WriteLine("***< task died here " + Task.TaskName);
            else
                Console.WriteLine("***< task died here");
        }

        #region IPanelViewModel Members

        public string PanelName {
            get {
                return panelName;
            }
            set {
                panelName = value;
                OnPropertyChanged("PanelName");
            }
            
        }

        public int PanelDisplayOrder {
            get { return 100; }
        }

    

        #endregion

        #region IViewModel Members

        [Import(typeof(TaskDetail))]
        public IView View { get; set; }


        #endregion
    }
}
