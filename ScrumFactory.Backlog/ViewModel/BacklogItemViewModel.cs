using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Windows.Input;
using System.Linq;
using System;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;


namespace ScrumFactory.Backlog.ViewModel {

    /// <summary>
    /// Backlog item Detail View Model.
    /// </summary> 
    [Export]
    public class BacklogItemViewModel : BaseEditableObjectViewModel, IEditableObjectViewModel, INotifyPropertyChanged, IViewModel {

         private BacklogItem item;
         private BacklogItem oldItem;
         
         private IBacklogService backlogService;
         private IAuthorizationService authorizator;

         private IBackgroundExecutor executor;
         private IEventAggregator aggregator;

         private IDialogService dialogs;

         [Import]
         private Lazy<BacklogViewModel> backlog { get; set; }

   
         public IBacklogItemTaskListViewModel TaskListViewModel { get; set; }

         [Import]
         public IArtifactsListViewModel ArtifactListViewModel { get; set; }

         [Import]
         private Services.ITasksService taskService { get; set; }

         private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        [Import]
        IProjectConstraintsService constraintsService;
     

         private BacklogViewModel Backlog {
             get {
                 if (backlog == null)
                     return null;
                 return backlog.Value;
             }
         }

        

        private bool isInEditMode = false;
        public bool IsInEditMode {
            get {                
                return isInEditMode;
            }
            set {
                isInEditMode = value;
                OnPropertyChanged("IsInEditMode");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacklogItemViewModel"/> class.
        /// </summary>
        /// <param name="backlogService">The backlog service.</param>
        /// <param name="backgroundExecutor">The background executor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="item">The item.</param>
        [ImportingConstructor]
        public BacklogItemViewModel(
            [Import] IBacklogService backlogService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IAuthorizationService authorizator,
            [Import] IDialogService dialogService,
            [Import] ScrumFactory.Composition.Configuration sfconfig,
            [Import] IBacklogItemTaskListViewModel taskListViewModel) :
            this(backlogService, backgroundExecutor, eventAggregator, authorizator, null, null, sfconfig) {

                this.dialogs = dialogService;

                aggregator.Subscribe<string>(ScrumFactoryEvent.ConfigChanged, OnConfigChanged);

                aggregator.Subscribe(ScrumFactoryEvent.ActiveSizesChanged, () => { OnPropertyChanged("Sizes");  });

                //aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, UpdateChangedItems);

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p => { adjustPointsChanged = true; });
                aggregator.Subscribe(ScrumFactoryEvent.ProjectAdjustPointsChanged, () => { adjustPointsChanged = true; });

                FinishItemCommand = new DelegateCommand(CanEdit, FinishBacklogItem);

                SaveCommand = new DelegateCommand(CanEdit, SaveBacklogItem);            
                CloseWindowCommand = new DelegateCommand(CloseWindow);

                ShowSuggestedHoursCommand = new DelegateCommand(ShowSuggestedHours);

                ApplySuggestedHoursCommand = new DelegateCommand<string>(ApplySuggestedHours);

                NavigateToNextItemCommand = new DelegateCommand(CanNavigate, () => { NavigateToItem(1); });
                NavigateToPreviousItemCommand = new DelegateCommand(CanNavigate, () => { NavigateToItem(-1); });

                TaskListViewModel = taskListViewModel;
                TaskListViewModel.OnTaskChanged = () => {                    
                    EffectiveHours = TaskListViewModel.GetTotalEffectiveHoursForSprint(Sprint);
                };
        }


        public short ItemOccurrence {
            get {
                if (Item == null)
                    return 0;
                return Item.OccurrenceConstraint;
            }
            set {
                Item.OccurrenceConstraint = value;                
                OnPropertyChanged("ItemOccurrence");
            }
        }

        public Project Project {
            get {
                if (Item == null)
                    return null;
                return Item.Project;
            }
        }

        public BacklogItemGroup Group {
            get {
                if (item == null)
                    return null;
                if (item.Group == null) 
                    LoadBacklogItemGroup();
                return item.Group;
            }
            set {
                if (item == null)
                    return;
                item.Group = value;
                OnPropertyChanged("Group");
            }
        }

        private bool isLoadingGroup = false;
        private void LoadBacklogItemGroup() {
            if (isLoadingGroup)
                return;
            isLoadingGroup = true;
            executor.StartBackgroundTask<ICollection<BacklogItemGroup>>(
                () => {
                    return backlogService.GetBacklogItemGroups(Item.Project.ProjectUId);
                },
                groups => {
                    Group = groups.SingleOrDefault(g => g.GroupUId == Item.GroupUId);
                    isLoadingGroup = false;
                });
        }

        private void OnConfigChanged(string config) {
            if (config == "UsePoints") {
                OnPropertyChanged("UsePoints");
                OnPropertyChanged("ConfigSize");
                OnPropertyChanged("ItemPixelHeight");
                
                
            }
        }

        public bool UsePoints {
            get {
                if (SFConfig == null)
                    return false;
                return SFConfig.GetBoolValue("UsePoints");
            }
        }

        private void AssignGroup() {
            if (Item == null || Backlog==null)
                return;
            Backlog.AssignBacklogItemGroup(this);
        }

        private bool isSelected;
        public override bool IsSelected {
            get {
                return isSelected;
            }
            set {
                isSelected = value;
                aggregator.Publish<BacklogItem>(ScrumFactoryEvent.BacklogItemSelectedChanged, Item);
                OnPropertyChanged("IsSelected");
            }
        }

 

        public override string ToString() {
            if (Item == null)
                return String.Empty;

            string text = Item.BacklogItemNumber + "\t" + Item.Name;
            if (PlannedHours != null) {
                foreach (PlannedHour p in PlannedHours)
                    text = text + "\t" + p.Hours;
            }

            return text;
        }

        public BacklogItemViewModel() { }
        
        public BacklogItemViewModel(
            IBacklogService backlogService,
            IBackgroundExecutor backgroundExecutor,
            IEventAggregator eventAggregator,            
            IAuthorizationService authorizator,
            Project project,            
            BacklogItem item,
            ScrumFactory.Composition.Configuration sfConfig) {


                Init(backlogService, backgroundExecutor, eventAggregator, authorizator, project, item);

                SFConfig = sfConfig;

                ShowRepositoryLogCommand = new DelegateCommand(ShowRepositoryLog);        
                

        }

        public bool IgnoreChangeCommands { get; set; }

        public void Init(
            IBacklogService backlogService,
            IBackgroundExecutor backgroundExecutor,
            IEventAggregator eventAggregator,
            IAuthorizationService authorizator,
            Project project,
            BacklogItem item) {

            this.backlogService = backlogService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;
            this.authorizator = authorizator;

            if (item != null) {
                this.Item = item;
                this.Item.Project = project;
                if(Item.Project.Roles!=null)
                    this.Item.SyncPlannedHoursAndRoles();
            }
            
            ChangeGroupCommand = new DelegateCommand(CanEdit, ChangeGroup);
            ChangeStatusCommand = new DelegateCommand(CanEdit, ChangeStatus);

            MoveItemToNextSprintCommand = new DelegateCommand(CanMoveItemToNextSprint, MoveItemToNextSprint);
            MoveItemToPreviousSprintCommand = new DelegateCommand(CanMoveItemToPreviousSprint, MoveItemToPreviousSprint);

            MoveItemToCurrentSprintCommand = new DelegateCommand(CanMoveItemToCurrentSprint, MoveItemToCurrentSprint);
            MoveItemToLastSprintCommand = new DelegateCommand(CanMoveItemToNextSprint, MoveItemToLastSprint);

            MoveItemToProductBacklogCommand = new DelegateCommand(CanMoveItemToProductBacklog, () => { ChangeSprint(-1, false); });

            ShowTasksCommand = new DelegateCommand(ShowTasks);

        }

        private void ShowTasks() {
            aggregator.Publish<BacklogItem>(ScrumFactoryEvent.ShowTasksForItem, Item);
        }

        private void ShowRepositoryLog() {
            string filter = "#" + Item.Project.ProjectNumber + "." + Item.BacklogItemNumber + ".";
            aggregator.Publish<string>(ScrumFactoryEvent.ShowCodeRepositoryLog, filter );
        }

        private decimal effectiveHours;
        public decimal EffectiveHours {
            get {
                return effectiveHours;
            }
            set {
                effectiveHours = value;
                OnPropertyChanged("EffectiveHours");
                OnPropertyChanged("HoursLeft");                
                OnPropertyChanged("PctHoursLeft");
                OnPropertyChanged("IsHoursLeftOverLimit");
            }
        }


        public bool IsDevelopmentItem {
            get {
                if (Item == null)
                    return true;
                return Item.OccurrenceConstraint == (int)ItemOccurrenceContraints.DEVELOPMENT_OCC;
            }
        }

        public void ShowDetail(IChildWindow parentViewModel, BacklogItem item) {

            Item = item;            
            SuggestedHours = null;
            
            ((DelegateCommand)NavigateToNextItemCommand).NotifyCanExecuteChanged();
            ((DelegateCommand)NavigateToPreviousItemCommand).NotifyCanExecuteChanged();

            if (!View.IsVisible)
                Show(parentViewModel);

            oldItem = Item.Clone();
            SetIdealHours();
            SetIsInEditMode();

         
            ArtifactListViewModel.ChangeContext(ArtifactContexts.BACKLOGITEM_ARTIFACT, Item.BacklogItemUId, RefreshBacklogItemArtifactCount);

            TaskListViewModel.Item = this.Item;
            
        }

        private void SetIsInEditMode() {
            if (Item == null)
                return;
            if (Item.Project == null)
                return;
            if (Item.Project.Status == (short)ProjectStatus.PROPOSAL_CREATION && CanEdit())
                IsInEditMode = true;
            else
                IsInEditMode = false;
            

            

        }

        public bool HasArtifact {
            get {
                if (Item == null || Item.ArtifactCount == null)
                    return false;
                return Item.ArtifactCount > 0;
            }
        }

        private void RefreshBacklogItemArtifactCount(int count) {            
            int? oldCount = Item.ArtifactCount;
            if (!oldCount.HasValue)
                oldCount = 0;

            Item.ArtifactCount = count;

            if ((oldCount == 0) != (count == 0))              
                aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, new BacklogItem[] { this.Item });                        
        }


        public bool IsPlanningLate {
            get {

                if (Item == null)
                    return false;

                // cant be late if is done
                if (Item.IsFinished)
                    return false;

                // cant be late if it has not started
                if (Item.Project!=null && Item.Project.Status != (short) ProjectStatus.PROJECT_STARTED)
                    return false;

                // no sprint assigned, so is late
                if (Sprint == null)
                    return true;

                // if sprint is over
                if (Sprint.EndDate.Date < System.DateTime.Today)
                    return true;

                // delivery date gone, is late
                if (Item.DeliveryDate.HasValue && Item.DeliveryDate.Value.Date < System.DateTime.Today)
                    return true;

                return false;
            }
        }


        public bool UserCannotEdit {
            get {
                if (Item == null)
                    return false;
                return !Item.Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        public bool UserCannotEditName {
            get {
                return UserCannotEdit && !Item.Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.PRODUCT_OWNER);
            }
        }

        public bool UserCannotEditHours {
            get {
                if (UserCannotEdit)
                    return true;
                return !Item.CanEditHours;
            }
        }

        private bool CanNavigate() {
            return (Backlog != null && Backlog.FilteredBacklog!=null);
        }

        private void NavigateToItem(int offset) {

            if (!CanNavigate())
                return;

            int position = Backlog.FilteredBacklog.CurrentPosition;

            if (ItemHasChanged)
                SaveBacklogItem(() => { Backlog.NavigateTo(position + offset); });
            else
                Backlog.NavigateBy(offset);
        }
        
        /// <summary>
        /// Closes the window, invoke the OnCloseAction and publishes the CloseWindow event.
        /// </summary>
        private void CloseWindow() {

            dialogs.CloseAlertMessage();

            if ((!UserCannotEdit || !UserCannotEditName || !UserCannotEditHours) && ItemHasChanged)
                SaveBacklogItem();

            Close();

            dialogs.GoBackSelectedTopMenu();

            if (OnCloseAction != null)
                OnCloseAction.Invoke(this);                  

        }

        public string PlannedHoursToolTip {
            get {
                string tooltip = "";
                foreach (PlannedHour h in Item.PlannedHours)
                    tooltip = tooltip + h.Hours + " " + h.Role.RoleShortName + "; ";
                return tooltip;
            }
        }

        /// <summary>
        /// Saves the item, detecting if the planned hours were changed or not.
        /// </summary>
        private void SaveBacklogItem() {
            SaveBacklogItem(null);
        }

        private void SaveBacklogItem(Action afterSave) {

            executor.StartBackgroundTask(
                () => {
                    // if the planned hours was not changed, dont update it
                    if (!Item.HasTheSameHours(oldItem))
                        backlogService.UpdateBacklogItem(Item.BacklogItemUId, Item);
                    else
                        backlogService.UpdateBacklogItemIgnoringHours(Item.BacklogItemUId, Item);
                },
                () => {
                    RefreshUI(); // need this to refresh item in the list

                    aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, new BacklogItem[] { this.Item });

                    // clean the original values
                    oldItem = item.Clone();
                    oldItem.Project = item.Project;
                    oldItem.SyncPlannedHoursAndRoles();

                    //oldItem = null;

                    if(afterSave!=null)
                        afterSave.Invoke();


                    
                });

        }

        private void ChangeGroup() {
            if (isDisposed || IgnoreChangeCommands || Item.GroupUId==null)
                return;

            // sometimes the Change event is called when the groups are loaded
            // in order to aboid a FORBITTEN execpetion when the user is nort the SM, verifies it first
            if (!Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                return;

            executor.StartBackgroundTask(
                () => {
                    backlogService.ChangeBacklogItemGroup(Item.BacklogItemUId, Item.GroupUId);
                },
                () => { });         
        }

        private void ChangeStatus() {
            if (isDisposed || IgnoreChangeCommands)
                return;
            executor.StartBackgroundTask(
                () => {
                    backlogService.ChangeBacklogItemStatus(Item.BacklogItemUId, Item.Status);
                },
                () => {
                    aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, new BacklogItem[] { this.Item });
                });
        }

        private void FinishBacklogItem() {
            if (isDisposed)
                return;
            Item.Status = (short)BacklogItemStatus.ITEM_DONE;
            ChangeStatus();
        }

        private bool ItemHasChanged {
            get {
                return !item.IsTheSame(oldItem);
            }
        }

        
       

        
        [Import(typeof(BacklogItemDetail))]
        public IView View { get; set; }


        #region IPanelViewModel Members

        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get {
                return 100;
            }
        }

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                if (Item == null)
                    return Properties.Resources.Backlog_item + "?";
                return Properties.Resources.Backlog_item + " " + Item.BacklogItemNumber;
            }
        }

       


        
        #endregion

        #region IBacklogItemViewModel Members

        private void ShowSuggestedHours() {
            if(ItemSize==null) {
                dialogs.ShowAlertMessage(Properties.Resources.Could_not_suggest_hours, Properties.Resources.You_need_to_provide_the_item_size, null);
                return;
            }
            CalcSuggestedHours();
        }

        /// <summary>
        /// Gets the sizes.
        /// </summary>
        /// <value>The sizes.</value>
        public ICollection<SizeViewModel> Sizes {
            get {
                if (Backlog == null)
                    return null;
                return Backlog.ActiveSizes;
            }            
        }

        /// <summary>
        /// Refreshes the corresponding View Model view.
        /// </summary>
        public void RefreshUI() {
            
            OnPropertyChanged("HasArtifact");
            OnPropertyChanged("Item");
            OnPropertyChanged("PanelName");
            OnPropertyChanged("ItemPixelHeight");            
            OnPropertyChanged("PlannedHours");
            OnPropertyChanged("ShowPlanningNumberLabel");
            OnPropertyChanged("ItemSprintLabel");
            OnPropertyChanged("ItemSizeUId");
            OnPropertyChanged("ItemSize");
            OnPropertyChanged("ConfigSize");
          
            OnPropertyChanged("ItemStatus");
            OnPropertyChanged("ItemSizeString");

            OnPropertyChanged("ItemOccurrence");

            OnPropertyChanged("Project");

            OnPropertyChanged("TrackId");

            OnPropertyChanged("IsPlanningLate");

            OnPropertyChanged("PlannedHoursToolTip");

            OnPropertyChanged("SizeFactor");
            OnPropertyChanged("UserCannotEdit");
            OnPropertyChanged("UserCannotEditName");
            OnPropertyChanged("UserCannotEditHours");
            
            
            OnPropertyChanged("IsAtCurrentSprint");
            OnPropertyChanged("IsDevelopmentItem");

            OnPropertyChanged("Group");

            
            OnPropertyChanged("DeliveryDateForEdit");
            OnPropertyChanged("UseDeliveryDate");
            OnPropertyChanged("DeliveryDate");

            OnPropertyChanged("IsTickectProject");
            

            if(MoveItemToCurrentSprintCommand!=null)
                ((DelegateCommand)MoveItemToCurrentSprintCommand).NotifyCanExecuteChanged();

            if(MoveItemToNextSprintCommand!=null)
                ((DelegateCommand)MoveItemToNextSprintCommand).NotifyCanExecuteChanged();

            if (MoveItemToPreviousSprintCommand != null)
                ((DelegateCommand)MoveItemToPreviousSprintCommand).NotifyCanExecuteChanged();

        }

        /// <summary>
        /// Gets the planned hours for this item.
        /// </summary>
        /// <value>The planned hours.</value>
        public ICollection<PlannedHour> PlannedHours {
            get {
                // no item, no candy
                if (item == null || item.PlannedHours==null)
                    return null;
                // item has no hours planned, create it with zero
                //if (item.PlannedHours == null || item.PlannedHours.Count == 0)
                //    item.SyncPlannedHoursAndRoles(item.Project.CurrentValidSprint.SprintNumber);
                
                if(item.PlannedHours.Any(h => h.Role==null))
                    return item.PlannedHours;
               
                return item.PlannedHours.OrderBy(p => p.Role.PermissionSet).ThenBy(p => p.Role.RoleName).ThenBy(p => p.Role.RoleShortName).ToList();
            }
        }

        private ICollection<PlannedHour> suggestedHours;
        public ICollection<PlannedHour> SuggestedHours {
            get {
                if (suggestedHours == null)
                    return null;
                return suggestedHours.OrderBy(p => p.Role.PermissionSet).ThenBy(p => p.Role.RoleName).ThenBy(p => p.Role.RoleShortName).ToList();
            }
            set {
                suggestedHours = value;
                OnPropertyChanged("SuggestedHours");
            }
        }

        private ICollection<PlannedHour> idealHours;
        public ICollection<PlannedHour> IdealHours {
            get {
                if (idealHours == null)
                    return null;
                return idealHours.OrderBy(p => p.Role.PermissionSet).ThenBy(p => p.Role.RoleName).ThenBy(p => p.Role.RoleShortName).ToList();                
            }            
        }

        private void SetIdealHours() {
            idealHours = new List<PlannedHour>();            
            if (ItemSize != null && ItemSize.ItemSize != null && SuggestedHours!=null) {
                foreach (PlannedHour h in SuggestedHours) {
                    PlannedHour i = new PlannedHour() { Role = h.Role, Hours = 0 };
                    if (ItemSize.ItemSize.SizeIdealHours != null) {
                        SizeIdealHour ideal = ItemSize.ItemSize.SizeIdealHours.SingleOrDefault(ih => ih.RoleShortName.ToLower() == h.Role.RoleShortName.ToLower());
                        if (ideal != null)
                            i.Hours = ideal.Hours * SizeFactor;
                    }
                    idealHours.Add(i);
                }
            }
            OnPropertyChanged("IdealHours");
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public BacklogItem Item {
            get { return item; }
            set {
                //ItemSize = null;
                item = value;                                
                RefreshUI();
                AssignGroup();
            }
        }

        public decimal HoursLeft {
            get {                
                return Item.CurrentTotalHours - EffectiveHours;
            }
        }


        public string TrackId {
            get {
                if (Item==null || Item.Project == null)
                    return "";
                return Item.Project.ProjectNumber + "." + Item.BacklogItemNumber;
            }
        }

        public decimal PctHoursLeft {
            get {
                if (Item.CurrentTotalHours == 0 && HoursLeft==0)
                    return 1;
                if (Item.CurrentTotalHours == 0 && HoursLeft < 0)
                    return 0;
                decimal pct = HoursLeft / Item.CurrentTotalHours;
                if (pct < 0)
                    pct = 0;
                return pct;
            }
        }

        public short IsHoursLeftOverLimit {
            get {
                if (PctHoursLeft < (decimal)0.2)
                    return 2;
                if (PctHoursLeft < (decimal)0.5)
                    return 1;
                return 0;
            }
        }


        /// <summary>
        /// Gets or sets the item status.
        /// </summary>
        /// <value>The item status.</value>
        public short ItemStatus {
            get {
                if (item == null)
                    return 0;
                return item.Status;
            }
            set {
                item.Status = value;
                OnPropertyChanged("Item");                
            }
        }
        
        public SizeViewModel ItemSize {
            get {
                if (Sizes == null)
                    return null;
                return Sizes.SingleOrDefault(z => z.ItemSize.ItemSizeUId == Item.ItemSizeUId);
            }
            set {
                if (SizeFactor == 0 && value!=null)
                    SizeFactor = 1;
                CalcSize(value);
                CalcSuggestedHours();

                // set occurance constraint
                if (value != null)
                    Item.OccurrenceConstraint = (short)value.OccurrenceConstraint;
                else
                    Item.OccurrenceConstraint = (short) ItemOccurrenceContraints.DEVELOPMENT_OCC;

            }
        }

        private void CalcSize() {
            CalcSize(ItemSize);
        }

        private void CalcSize(SizeViewModel sizeVM) {
            if (Item == null)
                return;

         

            if (sizeVM != null) {
                Item.ItemSizeUId = sizeVM.ItemSize.ItemSizeUId;
                Item.Size = sizeVM.ItemSize.Size * Item.SizeFactor;
            }
            else {
                Item.Size = null;
                Item.ItemSizeUId = null;
            }

            OnPropertyChanged("ItemSize");
            OnPropertyChanged("ItemSizeString");
            OnPropertyChanged("ConfigSize");
           
        }

        public int SizeFactor {
            get {
                if (Item == null)
                    return 0;
                return Item.SizeFactor;
            }
            set {
                Item.SizeFactor = value;
                CalcSize();
                CalcSuggestedHours();
                OnPropertyChanged("SizeFactor");
            }
        }

        private bool adjustPointsChanged = true;
        private decimal adjustPointsFactor = 1;
        private void GetAdjustPointsFactor() {
            if (adjustPointsChanged)
                adjustPointsFactor = (decimal) constraintsService.GetPointsFactor(Project.ProjectUId);
            adjustPointsChanged = false;            
        }

        private void CalcSuggestedHours() {

            if (Item == null)
                return;
            
            if (Item.ItemSizeUId == null)
                return;

            executor.StartBackgroundTask<Dictionary<string, decimal?>>(
                () => {
                    GetAdjustPointsFactor();
                    return backlogService.GetVelocityBySize(Item.ItemSizeUId);
                },
                vel => {                                       
                    List<PlannedHour> suggestedHours = new List<PlannedHour>();
                    foreach (Role r in Item.Project.Roles) {
                        string shortName = r.RoleShortName.ToLower();
                        decimal hours = 0;
                        if (vel.ContainsKey(shortName)) {
                            decimal? ptsHrs = vel[shortName];
                           if (!ptsHrs.HasValue || ptsHrs == 0)
                               hours = 0;
                           else
                               hours = (decimal)ItemSize.ItemSize.Size / ptsHrs.Value;

                        }

                        suggestedHours.Add(new PlannedHour() { Hours = hours * SizeFactor * adjustPointsFactor, Role = r });                        
                    }

                    SuggestedHours = suggestedHours;
                    SetIdealHours();
                });            
        }

        public string ItemSizeString {
            get {
                if (Item == null)
                    return null;
                if (Item.Size == null)
                    return "?";
                return Item.Size.ToString() + " " + Properties.Resources.pts;
            }            
        }

        /// <summary>
        /// Gets the item sprint label.
        /// It could be the localized string for "Next Sprint", "Current Sprint", etc...
        /// </summary>
        /// <value>The item sprint label.</value>
        public string ItemSprintLabel {
            get {
                if (Item == null || Item.Project == null)
                    return "";

                if (Item.SprintNumber == null)
                    return Properties.Resources.plan;

                return string.Format(Properties.Resources.sprint_N, Item.SprintNumber);

                //Sprint currentSprint = Item.Project.CurrentSprint;

                //if (currentSprint == null || !Item.Project.IsRunning)
                //    return string.Format(Properties.Resources.sprint_N, Item.SprintNumber);

                //if (currentSprint.SprintNumber == Item.SprintNumber)
                //    return Properties.Resources.current_sprint;

                //if (currentSprint.SprintNumber + 1 == Item.SprintNumber)
                //    return Properties.Resources.next_sprint;

                //if (currentSprint.SprintNumber - 1 == Item.SprintNumber)
                //    return Properties.Resources.previous_sprint;

                //return string.Format(Properties.Resources.sprint_N, Item.SprintNumber);
            }
        }

        public bool IsAtCurrentSprint {
            get {
                if (Item == null || Item.SprintNumber == null || Item.Project == null)
                    return false;

                Sprint currentSprint = Item.Project.CurrentSprint;

                if (currentSprint == null || !Item.Project.IsRunning)
                    return false;

                if (currentSprint.SprintNumber == Item.SprintNumber)
                    return true;

                return false;

            }
        }

        /// <summary>
        /// Changes the item sprint.
        /// </summary>
        /// <param name="sprintNumber">The new sprint number.</param>
        /// <param name="lowPriority">if set to <c>true</c> the item is placed at the end of the sprint.</param>
        public void ChangeSprint(int sprintNumber, bool lowPriority) {
           
            executor.StartBackgroundTask<BacklogItem[]>(
                () => { return backlogService.ChangeItemSprint(item.BacklogItemUId, sprintNumber, lowPriority); },
                changedItems => {
                    Item.SprintNumber = sprintNumber;  // POR QUE ESTAVA null?  ANTES?
                    RefreshUI();
                    aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, changedItems);                                   
                });
        }

        /// <summary>
        /// Moves the item to another item position.
        /// </summary>        
        /// <param name="targetItem">The target item.</param>
        public void MoveItemTo(BacklogItem targetItem) {

            executor.StartBackgroundTask<BacklogItem[]>(
                () => { return backlogService.MoveItem(item.BacklogItemUId, targetItem.BacklogItemUId); },
                changedItems => {
                    if (changedItems == null)
                        return;
                    BacklogItem updatedItem = changedItems.SingleOrDefault(i => i.BacklogItemUId == Item.BacklogItemUId);
                    if (updatedItem != null)
                        Item.SprintNumber = updatedItem.SprintNumber;
                    aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, changedItems);                                                            
                });
        }

        private bool CanEdit() {
            return !UserCannotEdit;
        }

        private void MoveItemToNextSprint() {            
            int sprintNumber;
            if (Item.SprintNumber == null)
                sprintNumber = Item.Project.CurrentValidSprint.SprintNumber + 1;
            else
                sprintNumber = (int)Item.SprintNumber + 1;

            if (sprintNumber > Item.Project.LastSprint.SprintNumber)
                sprintNumber = Item.Project.LastSprint.SprintNumber;

            ChangeSprint(sprintNumber, false);
        }

        private void MoveItemToPreviousSprint() {
            if (Item.SprintNumber == null || Item.SprintNumber==1)
                return;
            ChangeSprint((int)Item.SprintNumber -1, false);
        }

        private void MoveItemToLastSprint() {            
            ChangeSprint(Item.Project.LastSprint.SprintNumber, false);            
        }

        private void MoveItemToCurrentSprint() {
            ChangeSprint(Item.Project.CurrentValidSprint.SprintNumber, false);
        }

        public void UpdateChangedItems(BacklogItem[] changedItems) {
            if (isDisposed)
                return;
            if (changedItems == null || Item == null)
                return;
            BacklogItem changedItem = changedItems.SingleOrDefault(i => i.BacklogItemUId == Item.BacklogItemUId);
            if(changedItem==null)
                return;

            changedItem.Project = Item.Project;
            changedItem.SyncPlannedHoursAndRoles();
            Item = changedItem;
            
        }


        private bool CanMoveItemToCurrentSprint() {
            if (Item == null)
                return false;
            if (Item.Project == null || Item.Project.Sprints == null || Item.Project.IsTicketProject)
                return false;
            if (UserCannotEdit)
                return false;
            if (Item.Status == (short)BacklogItemStatus.ITEM_DONE || Item.Status == (short)BacklogItemStatus.ITEM_CANCELED)
                return false;
            if (Item.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                return false;
            if (Item.Project.CurrentSprint == null)
                return false;
            return !Item.SprintNumber.Equals(Item.Project.CurrentSprint.SprintNumber);
        }

        
        private bool CanMoveItemToNextSprint() {            
                if (Item.Project == null || Item.Project.Sprints == null || Item.Project.IsTicketProject)
                    return false;
                if (UserCannotEdit)
                    return false;
                if (Item.Status == (short)BacklogItemStatus.ITEM_DONE || Item.Status == (short)BacklogItemStatus.ITEM_CANCELED)
                    return false;
                if (Item.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                    return false;
                return !Item.SprintNumber.Equals(Item.Project.LastSprint.SprintNumber);
        }

        private bool CanMoveItemToProductBacklog() {
            if (Item.Project == null || Item.Project.Sprints == null || Item.Project.IsTicketProject)
                return false;
            if (UserCannotEdit)
                return false;
            if (Item.Status == (short)BacklogItemStatus.ITEM_DONE || Item.Status == (short)BacklogItemStatus.ITEM_CANCELED)
                return false;
            if (Item.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                return false;
            return true;
        }

        
        private bool CanMoveItemToPreviousSprint() {

            if (Item.Project == null || Item.Project.Sprints == null || Item.Project.IsTicketProject)
                    return false;
                if (UserCannotEdit)
                    return false;
                if (Item.Status == (short)BacklogItemStatus.ITEM_DONE || Item.Status == (short)BacklogItemStatus.ITEM_CANCELED)
                    return false;
                if (Item.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                    return false;
                return Item.SprintNumber > Item.Project.CurrentValidSprint.SprintNumber;
            
        }


        public decimal ConfigSize {
            get {
                if (UsePoints)
                    return (decimal)(Item.Size.HasValue?Item.Size:0);
                else
                    return Item.CurrentTotalHours;
            }
        }

        private double pixelZoomFactor = 3;
        public double PixelZoomFactor {
            get {
                return pixelZoomFactor;
            }
            set {
                pixelZoomFactor = value;
                OnPropertyChanged("ItemPixelHeight");
            }
        }

        public double ItemPixelHeight {
            get {
                double height;
                if (UsePoints)
                    height = (double)(Item.Size.HasValue ? Item.Size : 0) * PixelZoomFactor;
                else
                    height = (double)Item.CurrentTotalHours * PixelZoomFactor;

                if (height < 16)
                    height = 16;

                return height;
            }
        }

        public bool IsTickectProject {
            get {
                if (Item == null || Item.Project==null)
                    return false;
                return Item.Project.IsTicketProject;
            }
        }

        public bool UseDeliveryDate {
            get {
                if (Item == null)
                    return false;
                return (Item.DeliveryDate != null);
            }
            set {
                if (!value) 
                    DeliveryDate = null;
                else 
                    DeliveryDate = SprintEndDate;
                OnPropertyChanged("UseDeliveryDate");
                OnPropertyChanged("DeliveryDate");
                OnPropertyChanged("DeliveryDateForEdit");                                

            }
        }

      

        public System.DateTime? DeliveryDateForEdit {
            get {
                if (Item == null)
                    return null;
                return Item.DeliveryDate;
            }
            set {
                Item.DeliveryDate = value;
                OnPropertyChanged("UseDeliveryDate");
                OnPropertyChanged("DeliveryDate");
                OnPropertyChanged("DeliveryDateForEdit");                                
            }
        }

        public System.DateTime? DeliveryDate {
            get {
                if (Item == null)
                    return null;
                if (Item.DeliveryDate != null)
                    return Item.DeliveryDate;

                if (Item.Project == null)
                    return null;
                
                if (Item.Project.Sprints == null)
                    return null;

                if (Item.Project.IsTicketProject)
                    return null;

                return SprintEndDate;
            }
            set {
                Item.DeliveryDate = value;
                OnPropertyChanged("DeliveryDate");
                OnPropertyChanged("DeliveryDateForEdit");
                
            }
        }

        private DateTime? SprintEndDate {
            get {         
                if (Sprint == null)
                    return null;
                return Sprint.EndDate;
            }
        }

        public Sprint Sprint {
            get {
                if (Item==null || Item.Project == null || Item.Project.Sprints==null)
                    return null;
                return Item.Project.Sprints.SingleOrDefault(s => s.SprintNumber == Item.SprintNumber);                
            }
        }

        private void ApplySuggestedHours(string type) {
            if (type == "A")
                ApplySuggestedHours(SuggestedHours);
            if(type=="I")
                ApplySuggestedHours(IdealHours);
        }

        private void ApplySuggestedHours(ICollection<PlannedHour> hours) {
            foreach (PlannedHour h in Item.PlannedHours) {
                var suggested = hours.SingleOrDefault(s => s.Role.RoleShortName.ToLower() == h.Role.RoleShortName.ToLower());
                if (suggested != null)
                    h.Hours = suggested.Hours;
            }
            OnPropertyChanged("PlannedHours");
        }

        /// <summary>
        /// Gets or sets the on close action that is executed after a backlog item has been edited
        /// at the detail window.
        /// </summary>
        /// <value>The on close action.</value>
        public System.Action<BacklogItemViewModel> OnCloseAction { get; set; }

        /// <summary>
        /// Gets the close window command.
        /// </summary>
        /// <value>The close window command.</value>
        public ICommand CloseWindowCommand { get; private set; }


        public ICommand SaveCommand { get; set; }

        public ICommand ChangeGroupCommand { get; set; }
        public ICommand ChangeStatusCommand { get; set; }

        public ICommand FinishItemCommand { get; set; }


        /// <summary>
        /// Gets the move item to next sprint command.
        /// </summary>
        /// <value>The move item to next sprint command.</value>
        public ICommand MoveItemToNextSprintCommand { private set; get; }

        /// <summary>
        /// Gets the move item to previous sprint command.
        /// </summary>
        /// <value>The move item to previous sprint command.</value>
        public ICommand MoveItemToPreviousSprintCommand { private set; get; }

        /// <summary>
        /// Gets the move item to last sprint command.
        /// </summary>
        /// <value>The move item to last sprint command.</value>
        public ICommand MoveItemToLastSprintCommand { private set; get; }

        /// <summary>
        /// Gets the move item to last sprint command.
        /// </summary>
        /// <value>The move item to last sprint command.</value>
        public ICommand MoveItemToCurrentSprintCommand { private set; get; }

        public ICommand MoveItemToProductBacklogCommand { get; set; }

        /// <summary>
        /// Gets or sets the navigate to next item command.
        /// </summary>
        /// <value>The navigate to next item command.</value>
        public ICommand NavigateToNextItemCommand { get; set; }

        


        /// <summary>
        /// Gets or sets the navigate to previous item command.
        /// </summary>
        /// <value>The navigate to previous item command.</value>
        public ICommand NavigateToPreviousItemCommand { get; set; }

        public ICommand ShowTasksCommand { get; set; }

        public ICommand ShowRepositoryLogCommand { get; set; }

        public ICommand ShowSuggestedHoursCommand { get; set; }

        public ICommand ApplySuggestedHoursCommand { get; set; }

        /// <summary>
        /// Make sure to unbind all commands.
        /// </summary>
        protected override void OnDispose() {

            aggregator.UnSubscribeAll(this);

            FinishItemCommand = null; OnPropertyChanged("FinishItemCommand");
            SaveCommand = null; OnPropertyChanged("SaveCommand");
            ChangeGroupCommand = null; OnPropertyChanged("ChangeGroupCommand");
            CloseWindowCommand = null; OnPropertyChanged("CloseWindowCommand");
            MoveItemToNextSprintCommand = null; OnPropertyChanged("MoveItemToNextSprintCommand");
            MoveItemToPreviousSprintCommand = null; OnPropertyChanged("MoveItemToPreviousSprintCommand");
            MoveItemToLastSprintCommand = null; OnPropertyChanged("MoveItemToLastSprintCommand");
            MoveItemToProductBacklogCommand = null; OnPropertyChanged("MoveItemToProductBacklogCommand");
            NavigateToNextItemCommand = null; OnPropertyChanged("NavigateToNextItemCommand");
            NavigateToPreviousItemCommand = null; OnPropertyChanged("NavigateToPreviousItemCommand");            
            ChangeStatusCommand = null; OnPropertyChanged("ChangeStatusCommand");
            MoveItemToCurrentSprintCommand = null; OnPropertyChanged("MoveItemToCurrentSprintCommand");
            ShowTasksCommand = null; OnPropertyChanged("ShowTasksCommand");
            ShowRepositoryLogCommand = null; OnPropertyChanged("ShowRepositoryLogCommand");
            
        }

        ~BacklogItemViewModel() {
            System.Console.WriteLine("***< item died here");
        }
        
        #endregion
    }
}
