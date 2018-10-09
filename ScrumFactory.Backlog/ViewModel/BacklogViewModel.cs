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
using ScrumFactory.Windows.Helpers.Extensions;
using System.Linq;
using System.Windows.Data;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;

namespace ScrumFactory.Backlog.ViewModel {

    public enum BacklogStatusFilter : short {
        TODO_BACKLOG,
        PLANNING_BACKLOG,
        SELECTED_BACKLOG,
        ALL_BACKLOG,
        PRODUCT_BACKLOG
    }

    /// <summary>
    /// The Backlog View Model.
    /// </summary>    
    [Export]
    [Export(typeof(IProjectTabViewModel))]
    public class BacklogViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {

        private BacklogStatusFilter statusFilter;
        private string searchFilterText;

        private string newItemName;
        private System.Windows.Data.CollectionViewSource backlogViewSource;
        private DelayAction delayFilter;
    
        private IBacklogService backlogService;
        private ITasksService tasksService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;
        private IDialogService dialogs;

        [Import]
        private Lazy<IProjectContainer> projectContainer { get; set; }

        [Import]
        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        [Import]
        private IProposalsService proposalsService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacklogViewModel"/> class.
        /// </summary>
        /// <param name="backlogService">The backlog service.</param>
        /// <param name="backgroundExecutor">The background executor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor()]
        public BacklogViewModel(
            [Import] IBacklogService backlogService,
            [Import] ITasksService tasksService,
            [Import] IBackgroundExecutor backgroundExecutor,            
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogService,
            [Import] IAuthorizationService authorizationService) {

            this.backlogService = backlogService;
            this.tasksService = tasksService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;
            this.dialogs = dialogService;
            this.authorizator = authorizationService;

            aggregator.Subscribe<string>(ScrumFactoryEvent.ConfigChanged, OnConfigChanged);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails, 10);
            
            aggregator.Subscribe <ICollection<Role>>(ScrumFactoryEvent.ProjectRolesChanged, SyncPlannedHours);

            aggregator.Subscribe<Sprint>(ScrumFactoryEvent.SprintAdded, s => { AskForRefresh(); });
            aggregator.Subscribe<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, s => { AskForRefresh(); });
            aggregator.Subscribe(ScrumFactoryEvent.SprintsShifted, AskForRefresh);

            aggregator.Subscribe(ScrumFactoryEvent.BacklogReplannedByTask, AskForRefresh);

            aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, UpdateChangedItems);

            


            aggregator.Subscribe<BacklogItem>(ScrumFactoryEvent.ShowItemDetail, ShowDetailWindow);

            aggregator.Subscribe<BacklogItem>(ScrumFactoryEvent.BacklogItemSelectedChanged, OnItemsSelectedChanged);

            LoadDataCommand = new DelegateCommand(() => { if (NeedRefresh) LoadData(); });
            ShowDetailWindowCommand = new DelegateCommand<BacklogItemViewModel>(ShowDetailWindow);
            CreateNewBacklogItemCommand = new DelegateCommand(CreateNewBacklogItem);
            
            DeleteBacklogItemCommand = new DelegateCommand<BacklogItemViewModel>(CanRemoveBacklogItem, RemoveBacklogItem);
            
            EditItemSizesCommand = new DelegateCommand(CanEditItemSizes, EditItemSizes);
            PlanAllLateItemsCommand = new DelegateCommand(CanPlanAllLateItems, PlanAllLateItems);

            MoveItemToCommand = new DelegateCommand<Windows.Helpers.DragDrop.DropCommandParameter>(MoveItemTo);

            CopyItemsCommand = new DelegateCommand(CopyItemsToClipboard);
            PasteItemsCommand = new DelegateCommand(PasteItems);
            SelectAllItemsCommand = new DelegateCommand(SelectAllItems);


            FilterByMonthCommand = new DelegateCommand<DateTime>(FilterByMonth);
            
            backlogViewSource = new System.Windows.Data.CollectionViewSource();            
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.SprintNumber", ListSortDirection.Ascending));            
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.OccurrenceConstraint", ListSortDirection.Ascending));            
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.BusinessPriority", ListSortDirection.Ascending));
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.BacklogItemNumber", ListSortDirection.Ascending));
            
            backlogViewSource.Filter += new System.Windows.Data.FilterEventHandler(backlogViewSource_Filter);
            delayFilter = new DelayAction(800, new DelayAction.ActionDelegate(() => { FilteredBacklog.Refresh(); }));

            // just to show group list at screens with 1024 or more pixels width
            OnPropertyChanged("ShowGroupList");

        }

        public bool CanSeeProposals {
            get {
                if (Project == null || authorizator == null || authorizator.SignedMemberProfile == null)
                    return false;
                return
                    (authorizator.SignedMemberProfile.IsFactoryOwner ||
                    Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                    && authorizator.SignedMemberProfile.CanSeeProposalValues;
            }
        }

        string backlogPriceStr = "";
        public string BacklogPriceStr {
            get {
                return backlogPriceStr;
            }
            set {
                backlogPriceStr = value;
                OnPropertyChanged("BacklogPriceStr");
            }
        }

        private bool showValues;
        public bool ShowValues {
            get {
                if (!CanSeeProposals)
                    return false;
                return showValues;
            }
            set {
                showValues = value;
                if (showValues)
                    CalcBacklogPrice();
                else
                    BacklogPriceStr = "";
                OnPropertyChanged("ShowValues");
            }
        }

        private void CalcBacklogPrice() {
            Proposal proposal = new Proposal();
            proposal.UseCalcPrice = true;
            
            BacklogPriceStr = Properties.Resources.Calculating;
            executor.StartBackgroundTask<RoleHourCost[]>(() => {
                return proposalsService.GetHourCosts(Project.ProjectUId);                
            },
            costs => {
                proposal.Items = new List<ProposalItem>();

                var items = SelectedItems;
                if (items.Count == 0)
                    items = this.Items;
                foreach (var itemVM in items)
                    proposal.Items.Add(new ProposalItem() { ProposalUId = proposal.ProposalUId, BacklogItemUId = itemVM.Item.BacklogItemUId, Item = itemVM.Item });

                // calcs total value
                proposal.TotalValue = proposal.CalcTotalPrice(costs);
                BacklogPriceStr = proposal.TotalValue.ToString("c"); ;
            });


        }

        private void OnConfigChanged(string config) {
            if (config == "UsePoints") {
                OnPropertyChanged("UsePoints");
                OnPropertyChanged("PointsColumnWidth");
                OnPropertyChanged("HoursColumnWidth");
            }
        }

        public bool UsePoints {
            get {
                if (SFConfig == null)
                    return false;
                return SFConfig.GetBoolValue("UsePoints");
            }
        }

        public int PointsColumnWidth {
            get {
                return UsePoints ? 150 : 0;
            }
        }

        public int HoursColumnWidth {
            get {
                return !UsePoints ? 150 : 0;
            }
        }

        public bool ShowGroupList {
            get {
                return System.Windows.SystemParameters.PrimaryScreenWidth > 1024;                
            }
        }

        private void SelectAllItems() {
            if (Items == null)
                return;
            foreach (BacklogItemViewModel item in Items)
                item.IsSelected = !item.IsSelected;
        }

        private void PasteItems() {

            // not a scrum member, cant paste
            if (!CanCreateNewBacklogItem) {
                dialogs.ShowAlertMessage(Properties.Resources.Could_not_paste_items, Properties.Resources.You_need_to_be_a_scrum_master_to_paste_items, null);
                return;
            }

            // gets iten data
            System.Windows.DataObject data = System.Windows.Clipboard.GetDataObject() as System.Windows.DataObject;
            if (data == null)
                return;

            // pasting item objects
            ICollection<BacklogItem> clipItems = data.GetData("sf_items") as ICollection<BacklogItem>;
            if (clipItems != null) {
                try {
                    PasteObjectItems(clipItems);
                }
                catch (Exception) { }
                return;
            }

            // pasting text
            if (data.ContainsText()) {
                try {
                    PasteTextItems(data.GetText());
                }
                catch (Exception) { }
                return;
            }

        }

        private void PasteTextItems(string text) {

            if (String.IsNullOrEmpty(text))
                return;

            string[] items = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 0)
                return;

            List<BacklogItem> clipItems = new List<BacklogItem>();            
            foreach (string item in items) {
                
                string[] fields = item.Replace("\r","").Split(new char[] { '\t' }, StringSplitOptions.None);
                
                BacklogItem newItem = new BacklogItem() {
                    Name = fields[0],                    
                    BacklogItemUId = Guid.NewGuid().ToString(),                                             
                    Status = (short)BacklogItemStatus.ITEM_REQUIRED,
                    OccurrenceConstraint = (int)ItemOccurrenceContraints.DEVELOPMENT_OCC,
                    SizeFactor = 1,                
                    CreateDate = DateTime.Now
                };

                newItem.PlannedHours = new List<PlannedHour>();
                for (int i = 2; i < fields.Length; i++) {
                    decimal hours;
                    decimal.TryParse(fields[i], out hours);
                    newItem.PlannedHours.Add(new PlannedHour() { BacklogItemUId = newItem.BacklogItemUId, Hours = hours });
                }
              
                clipItems.Add(newItem);   
            }

            PasteObjectItems(clipItems);

        }

        private void PasteObjectItems(ICollection<BacklogItem> clipItems) {

            // ask before paste
            bool doPaste = ShowBeforePasteDialog(clipItems.Count);
            if (!doPaste)
                return;

            // paste
            foreach (BacklogItem i in clipItems) {
                BacklogItem newItem = DuplicateBacklogItem(i);

                SetItemGroupAndSize(newItem);

                i.BusinessPriority = NextBusinessPriority;

                CreateNewBacklogItem(newItem);
            }

        }

        private bool ShowBeforePasteDialog(int n) {            
            if (n <= 1) // less than two, no need to ask
                return true;
            System.Windows.MessageBoxResult d = dialogs.ShowMessageBox(Properties.Resources.Pasting_items,
                String.Format(Properties.Resources.Pasting_items_prompt, n),
                System.Windows.MessageBoxButton.YesNo);

            return d == System.Windows.MessageBoxResult.Yes;
        }


        private void CopyItemsToClipboard() {

            if (SelectedItemsCount == 0)
                return;

            ICollection<BacklogItemViewModel> items = SelectedItems;

            List<BacklogItem> clipItems = new List<BacklogItem>();

            string textData = string.Empty;
            foreach (BacklogItemViewModel i in items) {                
                // need to ensure the order for text copy and paste
                i.Item.PlannedHours = i.Item.PlannedHours.OrderBy(p => p.Role.PermissionSet).ThenBy(p => p.Role.RoleName).ThenBy(p => p.Role.RoleShortName).ToList();
                textData = textData + i.ToString() + System.Environment.NewLine;
                clipItems.Add(i.Item.Clone());
            }

            System.Windows.DataObject data = new System.Windows.DataObject();
            data.SetText(textData, System.Windows.TextDataFormat.Text);
            data.SetData("sf_items", clipItems);
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data);


        }

        private ICollection<BacklogItemViewModel> SelectedItems {
            get {
                if (Items == null)
                    return null;
                return Items.Where(i => i.IsSelected).ToArray();
            }
        }

        public int SelectedItemsCount {
            get {
                if (SelectedItems == null)
                    return 0;
                return SelectedItems.Count;
            }
        }

        public decimal SelectedItemsPlannedHours {
            get {
                if (SelectedItemsCount == 0)
                    return 0;
                return SelectedItems.Sum(i => i.Item.CurrentTotalHours);
            }
        }

        public decimal SelectedItemsEffectiveHours {
            get {
                if (SelectedItemsCount == 0)
                    return 0;
                return SelectedItems.Sum(i => i.EffectiveHours);
            }
        }

        private void OnItemsSelectedChanged(BacklogItem item) {
            OnPropertyChanged("SelectedItemsCount");
            OnPropertyChanged("SelectedItemsPlannedHours");
            OnPropertyChanged("SelectedItemsEffectiveHours");
        }

        private bool showIssueOnly;

        public bool ShowIssueOnly {
            get {
                return showIssueOnly;
            }
            set {
                showIssueOnly = value;
                OnPropertyChanged("ShowIssueOnly");
                FilteredBacklog.Refresh();
            }
        }

        public void Refresh() {               
            FilteredBacklog.Refresh();
            ((DelegateCommand)PlanAllLateItemsCommand).NotifyCanExecuteChanged();
            aggregator.Publish<Project>(ScrumFactoryEvent.BurndownShouldRefresh, Project);         
        }

        [Import]
        public GroupListViewModel GroupList { get; set; }
            

        public void UpdateChangedItems(BacklogItem[] changedItems) {
            if (changedItems == null || Items==null)
                return;
            foreach (BacklogItem changedItem in changedItems) {
                BacklogItemViewModel itemVM = Items.SingleOrDefault(b => b.Item.BacklogItemUId == changedItem.BacklogItemUId);                
                if (itemVM != null) {                    
                    itemVM.Item.SprintNumber = changedItem.SprintNumber;
                    itemVM.Item.BusinessPriority = changedItem.BusinessPriority;
                    itemVM.Item.Status = changedItem.Status;
                    if (changedItem.ArtifactCount != null)
                        itemVM.Item.ArtifactCount = changedItem.ArtifactCount;
                    //itemVM.IgnoreChangeCommands = true;
                    //itemVM.RefreshUI();
                    //itemVM.IgnoreChangeCommands = false;
                }            
            }

            if (changedItems.Length == 1) {
                BacklogItemViewModel firstItem = Items.SingleOrDefault(b => b.Item.BacklogItemUId == changedItems[0].BacklogItemUId);
                ((IEditableCollectionView)backlogViewSource.View).EditItem(firstItem);
                ((IEditableCollectionView)backlogViewSource.View).CommitEdit();
            }
            else
                backlogViewSource.View.Refresh();

            OnPropertyChanged("ItemsLeftTotalHours");
            OnPropertyChanged("ItemsLeftTotalSize");
            OnPropertyChanged("RequiredVelocity");  

            
        }

        /// <summary>
        /// Moves the item to another item position or sprint according the drag and drop parameters.
        /// </summary>
        /// <param name="p">The drag and drop parameters.</param>
        private void MoveItemTo(ScrumFactory.Windows.Helpers.DragDrop.DropCommandParameter p) {

            BacklogItemViewModel item = p.Item as BacklogItemViewModel;
            BacklogItemViewModel targetItem = p.DropTargetItem as BacklogItemViewModel;
            
            // no item no move
            if (item == null)
                return;

            // if is a plan or delivery item, cant move it
            if (item.Item.OccurrenceConstraint != (short)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                return;

            // if there is no target item, just move to the end of the backlog
            if (targetItem == null) {
                //item.ChangeSprint(Project.Sprints.Count, true);
                return;
            }
            
            // else move to the other position item
            item.MoveItemTo(targetItem.Item);

        }

                
        [Import]        
        public IterationPlanningViewModel IterationPlanningViewModel { get; set; }

        [Import]
        private SizeListViewModel ItemSizeListViewModel { get; set; }

        [Import]
        private BacklogItemViewModel itemDetail { get; set; }

        private bool CanPlanAllLateItems() {
            if (UserCannotEdit)
                return false;
            if (Items == null)
                return false;
            return Items.Any(i => i.IsPlanningLate);
        }

        private void PlanAllLateItems() {

            System.Windows.MessageBoxResult d = dialogs.ShowMessageBox(
               Properties.Resources.Plan_items,
               Properties.Resources.Plan_items_tooltip,
               System.Windows.MessageBoxButton.OKCancel,
               "/Images/Dialogs/PlanItems.png");

            if (d != System.Windows.MessageBoxResult.OK)
                return;

            foreach (BacklogItemViewModel itemVM in Items.Where(i => i.IsPlanningLate))
                itemVM.MoveItemToCurrentSprintCommand.Execute(null);
        }



        public void NavigateBy(int offset) {
            if (FilteredBacklog == null)
                return;
            
            // cycle id reach the end of the list
            if (offset > 0) {
                if (!FilteredBacklog.MoveCurrentToNext())
                    FilteredBacklog.MoveCurrentToFirst();
            }
            else {
                if (!FilteredBacklog.MoveCurrentToPrevious())
                    FilteredBacklog.MoveCurrentToLast();
            }

            BacklogItemViewModel nextItem = FilteredBacklog.CurrentItem as BacklogItemViewModel;
            itemDetail.ShowDetail(projectContainer.Value, nextItem.Item);
        }

        public void NavigateTo(int position) {
            if (FilteredBacklog == null)
                return;

            // cycle id reach the end of the list
            if (position < 0)
                FilteredBacklog.MoveCurrentToLast();
            else if (position >= Items.Count)
                FilteredBacklog.MoveCurrentToFirst();
            else
                FilteredBacklog.MoveCurrentToPosition(position);

            if(FilteredBacklog.CurrentItem==null)
                FilteredBacklog.MoveCurrentToFirst();

            BacklogItemViewModel nextItem = FilteredBacklog.CurrentItem as BacklogItemViewModel;
            itemDetail.ShowDetail(projectContainer.Value, nextItem.Item);
        }


        /// <summary>
        /// Shows the detail window.
        /// </summary>
        private void ShowDetailWindow(BacklogItemViewModel itemVM) {
            if (itemVM.Item == null)
                return;
            FilteredBacklog.MoveCurrentTo(itemVM);
            ShowDetailWindow(itemVM.Item);                
        }

        private void ShowDetailWindow(BacklogItem item) {            
            itemDetail.ShowDetail(projectContainer.Value, item);                
        }

        private void EditItemSizes() {
            ItemSizeListViewModel.Show(this.projectContainer.Value);
        }

        private void SyncPlannedHours(ICollection<Role> roles) {
            if (Project == null || Items==null)
                return;
            foreach (BacklogItemViewModel item in Items)
                // CHECK THIS !!! ITS SMELLING SHIT
                item.Item.SyncPlannedHoursAndRoles();
        }

        private bool CanEditItemSizes() {
            if (authorizator.SignedMemberProfile == null)
                return false;
            return authorizator.SignedMemberProfile.IsFactoryOwner;
        }

        private bool CanRemoveBacklogItem() {
            return !UserCannotEdit;
        }

        /// <summary>
        /// Gets the items total hours.
        /// </summary>
        /// <value>The items total hours.</value>
        public decimal ItemsLeftTotalHours {
            get {
                if (Project == null || Items == null || Items.Count == 0)
                    return 0;
                return Items.Where(i=>!i.Item.IsFinished).Sum(i => i.Item.CurrentTotalHours);
            }
        }


        /// <summary>
        /// Gets the total size of the items left.
        /// </summary>
        /// <value>The total size of the items left.</value>
        public int ItemsLeftTotalSize {
            get {
                 if (Project == null || Items == null || Items.Count == 0)
                        return 0;
                 int? total = Items.Where(i => !i.Item.IsFinished).Sum(i => i.Item.Size);
                 if (total == null)
                     return 0;
                 return (int)total;
            }
        }


        /// <summary>
        /// Gets the required velocity.
        /// </summary>
        /// <value>The required velocity.</value>
        public decimal RequiredVelocity {
            get {
                if (ItemsLeftTotalHours == 0)
                    return 0;
                return ItemsLeftTotalSize / ItemsLeftTotalHours;
            }
        }

        
        /// <summary>
        /// Handles the Filter event of the backlogViewSource control.
        /// Only items that matches the SearchFilter and the StatusFilter are accepted.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Data.FilterEventArgs"/> instance containing the event data.</param>
        private void backlogViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {
            
            BacklogItemViewModel vm = e.Item as BacklogItemViewModel;
            if (vm == null)
                return;

            bool groupFilter = true;
            if (GroupList!=null && GroupList.SelectedGroup != null)
                groupFilter = vm.Item.GroupUId == GroupList.SelectedGroup.GroupUId;

            bool textSearchFilter = true;
            if (!String.IsNullOrEmpty(SearchFilterText)) {
                string[] tags = SearchFilterText.NormalizeD().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (tags.All(t => 
                    vm.Item.Name.NormalizeD().Contains(t) 
                    || t.Equals(vm.Item.BacklogItemNumber.ToString())
                    || (vm.Item.ExternalId != null && vm.Item.ExternalId.NormalizeD().StartsWith(t))))
                    textSearchFilter = true;
                else
                    textSearchFilter = false;
            }

            bool bug = true;
            if (ShowIssueOnly && vm.Item.IssueType != (short)IssueTypes.ISSUE_ITEM)
                bug = false;

            e.Accepted = textSearchFilter && groupFilter && bug;

        }

        private void LoadData() {            
            LoadBacklogItems();
            if(GroupList!=null)
                GroupList.LoadGroupsIfNotLoadedAsync();            
        }

        private void LoadEffectiveHours() {
            executor.StartBackgroundTask<ICollection<BacklogItemEffectiveHours>>(
                () => { return tasksService.GetItemTasksEffectiveHoursByProject(Project.ProjectUId); },
                hrs => { 
                    foreach(BacklogItemViewModel item in Items) {
                        BacklogItemEffectiveHours taskHours = hrs.SingleOrDefault(h => h.BacklogItemUId==item.Item.BacklogItemUId && h.SprintNumber==item.Item.SprintNumber);
                        if (taskHours != null)
                            item.EffectiveHours = taskHours.EffectiveHours;
                    }                        
                });
        }

        private void OnViewProjectDetails(Project project) {            
            Project = project;

            if (Project == null)
                return;

            if (Project.IsTicketProject) {
                statusFilter = BacklogStatusFilter.TODO_BACKLOG;
            } else {
                statusFilter = BacklogStatusFilter.PLANNING_BACKLOG;
                selectedMonthFilter = DateTime.MinValue;
            }

            OnPropertyChanged("StatusFilter");
            OnPropertyChanged("CanSeeProposals");

        }

        private DateTime selectedMonthFilter;
        public DateTime SelectedMonthFilter {
            get {
                return selectedMonthFilter;
            }
            set {
                selectedMonthFilter = value;
                OnPropertyChanged("SelectedMonthFilter");
            }
        }

        private void FilterByMonth(DateTime month) {
            SelectedMonthFilter = month;
            if(SelectedMonthFilter!=DateTime.MinValue)
                StatusFilter = BacklogStatusFilter.ALL_BACKLOG;
            else
                StatusFilter = BacklogStatusFilter.TODO_BACKLOG;
            LoadBacklogItems();
        }

        private DateTime[] monthFilters;
        public DateTime[] MonthFilters {
            get {
                return monthFilters;
            }
            private set {
                monthFilters = value;
                OnPropertyChanged("MonthFilters");
            }
        }

        private void CreateMonthFilters() {
            List<DateTime> filters = new List<DateTime>();

            if (Project == null) {
                MonthFilters = filters.ToArray();
                return;
            }
            
            DateTime today = DateTime.Today;
            var day = new DateTime(today.Year, today.Month, 1);
            var createDate = new DateTime(Project.CreateDate.Year, Project.CreateDate.Month, 1);

            while (day >= createDate && filters.Count<12) {
                filters.Add(day);
                day = day.AddMonths(-1);
            }

            filters.Add(DateTime.MinValue);

            MonthFilters = filters.ToArray();

            SelectedMonthFilter = filters.Last();
        }


        /// <summary>
        /// Loads the backlog items from the service.
        /// </summary>
        /// <param name="project">The project.</param>
        private void LoadBacklogItems() {

            IsLoadingData = true;
            NeedRefresh = false;

            Items = new List<BacklogItemViewModel>();

            if (GroupList != null) {
                GroupList.PropertyChanged -= new PropertyChangedEventHandler(GroupList_PropertyChanged);
                GroupList.PropertyChanged += new PropertyChangedEventHandler(GroupList_PropertyChanged);
            }

            executor.StartBackgroundTask<ICollection<BacklogItem>>(
                () => {
                    if (Project == null)
                        return null;

                    if (Project.IsTicketProject) {
                        DateTime fromDate = SelectedMonthFilter;
                        DateTime untilDate = DateTime.MinValue;
                        if (fromDate != DateTime.MinValue)
                            untilDate = fromDate.AddMonths(1);
                        return this.backlogService.GetCurrentBacklog(Project.ProjectUId, (short)StatusFilter, fromDate, untilDate);
                    }

                    return this.backlogService.GetCurrentBacklog(Project.ProjectUId, (short)StatusFilter);

                },
                OnBacklogItemsLoaded);
        }

        void GroupList_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedGroup")
                FilteredBacklog.Refresh();
        }

        /// <summary>
        /// Called when backlog items loaded.
        /// Creates the Items View Model collection and raises publishes the BacklogItemsLoaded event.
        /// </summary>
        /// <param name="items">The items.</param>
        private void OnBacklogItemsLoaded(ICollection<BacklogItem> items) {

            

            if (items == null)
                return;

            List<BacklogItemViewModel> temp = new List<ViewModel.BacklogItemViewModel>();
            foreach (BacklogItem i in items) {
                var itemVm = new BacklogItemViewModel(backlogService, executor, aggregator, authorizator, Project, i, SFConfig);

                itemVm.IgnoreChangeCommands = true; // avoid to trigger ChangeGroup command

                // this should be done at the server - just quick fix
                // NEW SERVER VERSION DOESNT NEED THIS
                if (i != null && i.ItemSizeUId != null && i.Size == null) {
                    var size = ActiveSizes.SingleOrDefault(z => z.ItemSize.ItemSizeUId == i.ItemSizeUId);
                    if (size != null)
                        i.Size = size.ItemSize.Size * i.SizeFactor;
                }
                //------------------------------------------------

                temp.Add(itemVm);
            }

            
            Items = temp;
            foreach (BacklogItemViewModel item in Items)
                item.IgnoreChangeCommands = false; // now you can change group

            LoadEffectiveHours();

            // load data is over
            IsLoadingData = false;
        }

        public void AssignBacklogItemGroup(BacklogItemViewModel item) {
            if (GroupList == null || GroupList.Groups == null || item==null || item.Item == null)
                return;            
            item.Group = GroupList.Groups.Where(g => g.GroupUId == item.Item.GroupUId).SingleOrDefault();
        }




        /// <summary>
        /// Gets the active sizes.
        /// </summary>
        /// <value>The active sizes.</value>
        public ICollection<SizeViewModel> ActiveSizes {
            get {
                if (ItemSizeListViewModel.Sizes == null)
                    return null;
                return ItemSizeListViewModel.Sizes.Where(z => z.ItemSize.IsActive).ToArray();
            }
        }

        /// <summary>
        /// Removes the backlog item form the Model, from the View Model collection and from the
        /// services.
        /// </summary>
        /// <param name="itemViewModel">The item view model to be removed.</param>
        private void RemoveBacklogItem(BacklogItemViewModel itemViewModel) {

            string shortName = itemViewModel.Item.Name;
            if (shortName == null)
                shortName = String.Empty;
            if (shortName.Length > 15)
                shortName = shortName.Substring(0, 15) + "...";
            string confirmMessage = String.Format(Properties.Resources.Are_you_sure_remove_item, shortName);

            System.Windows.MessageBoxResult ok = dialogs.ShowMessageBox(Properties.Resources.Remove_backlog_item, confirmMessage, System.Windows.MessageBoxButton.YesNo);
            if (ok == System.Windows.MessageBoxResult.No)
                return;

            string backlogitemUId = itemViewModel.Item.BacklogItemUId;

            executor.StartBackgroundTask(
                    () => {
                        backlogService.DeleteBacklogItem(itemViewModel.Item.BacklogItemUId);                                                
                    },
                    () => {
                        ((IEditableCollectionView)backlogViewSource.View).Remove(itemViewModel);                                                                         
                        itemViewModel.Dispose();
                        OnPropertyChanged("ItemsLeftTotalHours");
                        OnPropertyChanged("ItemsLeftTotalSize");
                        OnPropertyChanged("RequiredVelocity");
                        ((DelegateCommand)PlanAllLateItemsCommand).NotifyCanExecuteChanged();
                        aggregator.Publish<string>(ScrumFactoryEvent.BacklogItemDeleted, backlogitemUId);
                    });
        }

        /// <summary>
        /// Determines whether this instance can create new backlog item.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can create new backlog item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanCreateNewBacklogItem {
            get {
                return !UserCannotEdit;
            }
        }

        

        private int NextBusinessPriority {
            get {
                if (Project == null || Items == null || Items.Count == 0)
                    return 1;
                return Items.Max(i => i.Item.BusinessPriority) + 10;
            }
        }

        private void CreateNewBacklogItem() {
            if (String.IsNullOrEmpty(newItemName))
                return;

            BacklogItem newItem = PrepareNewBacklogItem(this.NewItemName, ItemOccurrenceContraints.DEVELOPMENT_OCC, null);

            CreateNewBacklogItem(newItem);
        }

        /// <summary>
        /// Creates the new backlog item and saves it.
        /// </summary>
        private void CreateNewBacklogItem(BacklogItem newItem) {
            
            // save its
            executor.StartBackgroundTask(
                () => { backlogService.AddBacklogItem(newItem); },
                () => {
                    NewItemName = String.Empty;
                    BacklogItemViewModel vm = new BacklogItemViewModel();
                    vm.Init(backlogService, executor, aggregator, authorizator, Project, newItem);

                    // TO THIS STRANGE CHECK 'CUZ WHEN LIST IS EMPTY COMMITNEW THROWS STRANGE EXCEPTION
                    if (FilteredBacklog.IsEmpty || Items.Count == 0) {
                        Items.Add(vm);
                        backlogViewSource.View.Refresh();
                    }
                    else {
                        ((System.Windows.Data.ListCollectionView)backlogViewSource.View).AddNewItem(vm);                    
                        ((System.Windows.Data.ListCollectionView)backlogViewSource.View).CommitNew();                            
                    }
                    vm.NotifyAdded();
                    aggregator.Publish<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, new BacklogItem[] { newItem });
                });

        }

        public int? NewBacklogItemSprintNumber {
            get {
                if (Project.Sprints == null || Project.Sprints.Count == 0)
                    return 1;

                if (Items == null)
                    throw new System.Exception("Project Backlog has not been loaded yet");

                if (Project.ProjectType == (short)ProjectTypes.TICKET_PROJECT)
                    return 1;

                // if project already closed
                if (Project.Status >= (short)ProjectStatus.PROJECT_STARTED)
                    return null;

                foreach (Sprint sprint in Project.Sprints.OrderBy(s => s.SprintNumber)) {
                    if (!sprint.IsOver || !Project.IsRunning) {
                        decimal hours = Items.Where(i => i.Item.SprintNumber == sprint.SprintNumber).Sum(i => i.Item.CurrentTotalHours);
                        if (hours < Project.ProjectOptions.SprintLimitHours)
                            return sprint.SprintNumber;
                    }
                }

                return Project.Sprints.Count + 1;
            }
        }

        /// <summary>
        /// Make sure that the default groups are at the group list.
        /// <remarks>
        /// Th group collcetion does not allow insert at different threads, so this
        /// silly code should be called before change thread.
        /// </remarks>
        /// </summary>
        internal void PrepareGroups() {
            if (GroupList == null)
                return;
            BacklogItemGroup g = GroupList.DeliveryGroup;
            g = GroupList.PlanGroup;
            g = GroupList.DevelopmentGroup;
        }

        internal BacklogItem DuplicateBacklogItem(BacklogItem fromItem) {
            int? sprintNumber = GetSprintNumberForInsert();

            // create item
            BacklogItem newItem = new BacklogItem {
                BacklogItemUId = Guid.NewGuid().ToString(),
                ProjectUId = Project.ProjectUId,
                Project = Project,
                Name = fromItem.Name,
                Description = fromItem.Description,
                Status = fromItem.Status,
                BusinessPriority = NextBusinessPriority,
                OccurrenceConstraint = fromItem.OccurrenceConstraint,
                SizeFactor = fromItem.SizeFactor,
                Size = fromItem.Size,
                ItemSizeUId = fromItem.ItemSizeUId,
                CancelReason = fromItem.CancelReason,
                DeliveryDate = fromItem.DeliveryDate,
                StartedAt = fromItem.StartedAt,
                FinishedAt = fromItem.FinishedAt,
                IssueType = fromItem.IssueType,                
                CreateDate = DateTime.Now
            };

            // copy planned hours
            if (fromItem.ProjectUId == Project.ProjectUId)
                DuplicateBacklogItemHoursSameProject(newItem, fromItem, sprintNumber);
            else
                DuplicateBacklogItemHours(newItem, fromItem, sprintNumber);
            

            newItem.SyncPlannedHoursAndRoles(sprintNumber);

            return newItem;
        }

        internal void DuplicateBacklogItemHoursSameProject(BacklogItem newItem, BacklogItem fromItem, int? sprintNumber) {
            newItem.PlannedHours = new List<PlannedHour>();
            foreach (PlannedHour h in fromItem.CurrentPlannedHours) {                 
                if (Project.Roles.Any(r => r.RoleUId == h.RoleUId)) {
                    PlannedHour hour = new PlannedHour { BacklogItemUId = newItem.BacklogItemUId, Hours = h.Hours, SprintNumber = sprintNumber, PlanningNumber = Project.CurrentPlanningNumber, RoleUId = h.RoleUId };                    
                    newItem.PlannedHours.Add(hour);
                }                
            }
        }

        internal void DuplicateBacklogItemHours(BacklogItem newItem, BacklogItem fromItem, int? sprintNumber) {
            newItem.PlannedHours = new List<PlannedHour>();            
            Role[] orderedRoles = Project.Roles.OrderBy(r => r.PermissionSet).ThenBy(r => r.RoleName).ThenBy(r => r.RoleShortName).ToArray();
            for(int i=0; i<orderedRoles.Length && i<fromItem.PlannedHours.Count; i++) {
                PlannedHour h = fromItem.PlannedHours[i];
                PlannedHour hour = new PlannedHour { BacklogItemUId = newItem.BacklogItemUId, Hours = h.Hours, SprintNumber = sprintNumber, PlanningNumber = Project.CurrentPlanningNumber, RoleUId = orderedRoles[i].RoleUId };
                newItem.PlannedHours.Add(hour);
            }
            
        }

        private int? GetSprintNumberForInsert() {
            int? sprintNumber = NewBacklogItemSprintNumber;

            // if the sprint number is higher than the project sprints
            if (sprintNumber > Project.Sprints.Count) {

                // add a new sprint if allowed at project option, otherwise, use the last sprint
                if (Project.ProjectOptions.AutoAddSprints)
                    IterationPlanningViewModel.CreateNewSprint();
                else
                    sprintNumber = Project.Sprints.Count;
            }
            return sprintNumber;
        }

        internal BacklogItem PrepareNewBacklogItem(string name, ItemOccurrenceContraints occurrenceConstraint, int? sprintNumber) {

            // if no sprint number was informed, tries to fit it at the avaiable sprints
            if (sprintNumber == null) 
                sprintNumber = GetSprintNumberForInsert();

            // create the new item
            BacklogItem newItem = new BacklogItem {
                BacklogItemUId = Guid.NewGuid().ToString(),                
                ProjectUId = Project.ProjectUId,
                Project = Project,
                Name = name,
                Description = null,
                Status = (short)BacklogItemStatus.ITEM_REQUIRED,
                BusinessPriority = NextBusinessPriority,
                OccurrenceConstraint = (short)occurrenceConstraint,
                SizeFactor = 0,                
                CreateDate = DateTime.Now
            };
           
            SetItemGroupAndSize(newItem);

            // if is a SCRUM MASTER plan it at the sprint
            // if is a PRODUCT OWNER than sprintNumber will be null
            if(Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                newItem.SyncPlannedHoursAndRoles(sprintNumber);


            return newItem;
        }


        private void SetItemGroupAndSize(BacklogItem item) {

            // find out the size of the new item
            if (ItemSizeListViewModel.PlanningSize == null)
                throw new Exception("Size list was not loaded yet");

            ItemSize size = null;

            if (item.OccurrenceConstraint == (int)ItemOccurrenceContraints.DEVELOPMENT_OCC) {
                if (GroupList.SelectedGroup != null)
                    item.Group = GroupList.SelectedGroup;
                else
                    item.Group = GroupList.DevelopmentGroup;
            }

            if (item.OccurrenceConstraint == (int)ItemOccurrenceContraints.PLANNING_OCC) {
                size = ItemSizeListViewModel.PlanningSize.ItemSize;
                item.Group = GroupList.PlanGroup;
            }

            if (item.OccurrenceConstraint == (int)ItemOccurrenceContraints.DELIVERY_OCC) {
                size = ItemSizeListViewModel.DeliverySize.ItemSize;
                item.Group = GroupList.DeliveryGroup;
            }

            // SINGLE SIZE MODE
            if (size==null) {
                size = ItemSizeListViewModel.SingleSize.ItemSize;
            }

            if (size != null) {
                item.Size = size.Size * item.SizeFactor;
                item.ItemSizeUId = size.ItemSizeUId;
            }

            item.GroupUId = item.Group.GroupUId;

        }

        private Project project;
               
        #region IBacklogViewModel Members

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>The project.</value>
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;

                IsVisible = (project != null);

                Items.ClearAndDispose();
                if (GroupList != null) {
                    GroupList.NeedRefresh = true;
                    GroupList.Groups.ClearAndDispose();
                }
                AskForRefresh();

                if (project==null || authorizator == null || authorizator.SignedMemberProfile == null)
                    UserCannotEdit = true;
                else
                    UserCannotEdit = ! (project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER) || project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.PRODUCT_OWNER));

                (CreateNewBacklogItemCommand as DelegateCommand).NotifyCanExecuteChanged();
                (EditItemSizesCommand as DelegateCommand).NotifyCanExecuteChanged();
                (DeleteBacklogItemCommand as DelegateCommand<BacklogItemViewModel>).NotifyCanExecuteChanged();

                OnPropertyChanged("Project");

                CreateMonthFilters();
            
            }
        }

        private void AskForRefresh() {
            if (View != null && View.IsVisible) {
                LoadData();
            }
            else
                NeedRefresh = true;
        }             

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        [Import(typeof(Backlog))]
        public IView View { get; set; }


        /// <summary>
        /// Gets or sets the status filter.
        /// </summary>
        /// <value>The status filter.</value>
        public BacklogStatusFilter StatusFilter {
            get {
                return this.statusFilter;
            }
            set {
                this.statusFilter = value;
                LoadBacklogItems();
                OnPropertyChanged("StatusFilter");
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

        /// <summary>
        /// Gets the new name of the item to be created.
        /// </summary>
        /// <value>The new name of the item.</value>
        public string NewItemName {
            get {
                return this.newItemName;
            }
            set {
                this.newItemName = value;
                ((DelegateCommand)CreateNewBacklogItemCommand).NotifyCanExecuteChanged();
                OnPropertyChanged("NewItemName");
            }
        }

        
        /// <summary>
        /// Gets the filtered backlog.
        /// </summary>
        /// <value>The filtered backlog.</value>
        public ICollectionView FilteredBacklog {
            get {               
                return backlogViewSource.View;
            }
        }

        private bool userCannotEdit;
        public bool UserCannotEdit {
            get {
                return userCannotEdit;
            }
            set {
                userCannotEdit = value;
                OnPropertyChanged("UserCannotEdit");                
                OnPropertyChanged("CanCreateNewBacklogItem");
            }
        }

        private ICollection<BacklogItemViewModel> items;
        
        /// <summary>
        /// Gets the backlog items.
        /// </summary>
        /// <value>The collection of View Model of each item.</value>        
        public ICollection<BacklogItemViewModel> Items {
            get {
                return items;
            }
            set {

                // free memory
                DeleteBacklogItemCommand = null; // NEED THIS BECAUSE COMMAND BIND HOLDS A REFERENCE AN LEAKS
                ShowDetailWindowCommand = null;
                items.ClearAndDispose();
                backlogViewSource.GroupDescriptions.Clear();
                DeleteBacklogItemCommand = new DelegateCommand<BacklogItemViewModel>(CanRemoveBacklogItem, RemoveBacklogItem);
                ShowDetailWindowCommand = new DelegateCommand<BacklogItemViewModel>(ShowDetailWindow);
                
                items = value;

                // sets the filtered view source                         
                backlogViewSource.Source = items;
                backlogViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Item.SprintNumber"));

                // raises the change property and the event
                OnPropertyChanged("FilteredBacklog");
                OnPropertyChanged("ItemsLeftTotalHours");
                OnPropertyChanged("ItemsLeftTotalSize");
                OnPropertyChanged("RequiredVelocity");
                
                ((DelegateCommand)PlanAllLateItemsCommand).NotifyCanExecuteChanged();

            }
        }


        /// <summary>
        /// Gets the delete backlog item command.
        /// </summary>
        /// <value>The delete backlog item command.</value>
        public ICommand DeleteBacklogItemCommand { get; private set; }

        /// <summary>
        /// Gets the create new backlog item command.
        /// </summary>
        /// <value>The create new backlog item command.</value>
        public ICommand CreateNewBacklogItemCommand { get; private set; }

        public ICommand LoadDataCommand { get; set; }


        /// <summary>
        /// Gets the edit item sizes command.
        /// </summary>
        /// <value>The edit item sizes command.</value>
        public ICommand EditItemSizesCommand { get; set; }

        /// <summary>
        /// Gets the show detail window command.
        /// </summary>
        /// <value>The show detail window command.</value>
        public ICommand ShowDetailWindowCommand { get; private set; }


        /// <summary>
        /// Gets or sets the plan all late items command.
        /// </summary>
        /// <value>The plan all late items command.</value>
        public ICommand PlanAllLateItemsCommand { get; set; }

        /// <summary>
        /// Gets or sets the move item to command.
        /// </summary>
        /// <value>The move item to command.</value>
        public ICommand MoveItemToCommand { private set; get; }


        public ICommand CopyItemsCommand { get; set; }

        public ICommand PasteItemsCommand { get; set; }

        public ICommand SelectAllItemsCommand { get; set; }

        public ICommand FilterByMonthCommand { get; set; }

        #endregion

        #region IPanelViewModel Members

    

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.ResourceManager.GetString("Backlog");
            }
        }


        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get {
                return 200;
            }
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


        
    }
}
