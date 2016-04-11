using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using ScrumFactory.Composition.View;
using ScrumFactory.Windows.Helpers.Extensions;
using ScrumFactory.ReportHelper;

namespace ScrumFactory.Backlog.ViewModel {

    /// <summary>
    /// Iteration Planning View Model.
    /// </summary>
    [Export(typeof(IterationPlanningViewModel))]
    [Export(typeof(IProjectTabViewModel))]
    public class IterationPlanningViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {

        private IBacklogService backlogService;
        private IProjectsService projectsService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;

        private IDialogService dialogs;

        [Import]
        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        [Import]
        private ICalendarService calendar { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationPlanningViewModel"/> class.
        /// </summary>
        /// <param name="backlogService">The backlog service.</param>
        /// <param name="projectsService">The iteration service.</param>
        /// <param name="backgroundExecutor">The background executor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor()]
        public IterationPlanningViewModel(
            [Import] IBacklogService backlogService,
            [Import] IProjectsService projectsService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IDialogService dialogService,
            [Import] IEventAggregator eventAggregator,
            [Import] IAuthorizationService authorizator) {

            this.projectsService = projectsService;
            this.backlogService = backlogService;
            this.executor = backgroundExecutor;
            this.dialogs = dialogService;
            this.aggregator = eventAggregator;
            this.authorizator = authorizator;

            SprintPlans = new ObservableCollection<SprintViewModel>();

            aggregator.Subscribe<string>(ScrumFactoryEvent.ConfigChanged, OnConfigChanged);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
            aggregator.Subscribe<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, OnSprintsDateChanged);

            aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, UpdateChangedItems);

            aggregator.Subscribe<string>(ScrumFactoryEvent.BacklogItemDeleted, OnBacklogItemDeleted);

            aggregator.Subscribe <ICollection<BacklogItemGroup>>(ScrumFactoryEvent.BacklogItemGroupsLoaded, OnBacklogItemsGroupLoaded);

            LoadDataCommand = new DelegateCommand(() => { if (NeedRefresh) LoadData(); });
            
            CreateNewSprintCommand = new DelegateCommand(CanEditSprints, CreateNewSprint);
            EqualizeSprintsCommand = new DelegateCommand(CanEditSprints, EqualizeSprints);
            RemoveSprintCommand = new DelegateCommand<SprintViewModel>(CanEditSprints, RemoveSprint);

            MoveItemToCommand = new DelegateCommand<Windows.Helpers.DragDrop.DropCommandParameter>(MoveItemTo);

            ShowDeliveryReportCommand = new DelegateCommand(ShowDeliveryReport);
            ShowSprintStackReportCommand = new DelegateCommand(ShowSprintStackReport);
            ShowFullDeliveryReportCommand = new DelegateCommand(ShowFullDeliveryReport);

            ShiftItemsCommand = new DelegateCommand<BacklogItem>(CanEditSprints, ShiftItemsAfter);

            ShowDetailCommand = new DelegateCommand<BacklogItemViewModel>(ShowDetail);

            

        }

        private bool calcZoom = false;

        private double maxZoomFactor = 3;
        public double MaxZoomFactor {
            get {
                return maxZoomFactor;
            }
            set {
                maxZoomFactor = value;
                zoomFactor = value;
                OnPropertyChanged("ZoomFactor");
                OnPropertyChanged("MaxZoomFactor");
            }
        }

        private double zoomFactor = 3;
        public double ZoomFactor {
            get {
                return zoomFactor;
            }
            set {
                zoomFactor = value;
                foreach (BacklogItemViewModel item in Items)
                    item.PixelZoomFactor = zoomFactor;
                OnPropertyChanged("ZoomFactor");
            }
        }

        private bool showAllSprints;
        public bool ShowAllSprints {
            get {
                return showAllSprints;
            }
            set {
                showAllSprints = value;
                OnPropertyChanged("ShowAllSprints");
                UpdateSprintPlans();
            }
        }

        [ImportMany]
        private IEnumerable<IPluginCommand> allPluginMenuItems { get; set; }

        public IEnumerable<IPluginCommand> PluginMenuItems {
            get {
                if (allPluginMenuItems == null)
                    return null;
                return allPluginMenuItems.Where(m => m.ContainerViewModelClassName.Equals(this.GetType().ToString())).OrderBy(m => m.DisplayOrder).ToList();
            }
        }

        private void OnConfigChanged(string config) {
            if (config == "UsePoints") {
                OnPropertyChanged("UsePoints");
                OnPropertyChanged("ConfigUnit");
            }
        }

        private void OnBacklogItemDeleted(string backlogItemUId) {            
            if (Items == null)
                return;
            BacklogItemViewModel item = items.Where(i => i.Item.BacklogItemUId == backlogItemUId).SingleOrDefault();
            if (item == null)
                return;
            int? sprintNumber = item.Item.SprintNumber;
            Items.Remove(item);

            SprintViewModel sprintAffected = SprintPlans.Where(s => s.Sprint.SprintNumber == sprintNumber).SingleOrDefault();
            if (sprintAffected != null)
                sprintAffected.RefreshUI();
            
        }

        private void ShowDetail(BacklogItemViewModel itemVM) {
            aggregator.Publish<BacklogItem>(ScrumFactoryEvent.ShowItemDetail, itemVM.Item);            
        }

        private void ShiftItemsAfter(BacklogItem item) {

            if (item == null)
                return;



            string planItemName = String.Format(Properties.Resources.Plan_sprint_N, item.SprintNumber + 1);
            string deliveryItemName = String.Format(Properties.Resources.Delivery_sprint_N, item.SprintNumber + 1);
            
         
            IsLoadingData = true;
            executor.StartBackgroundTask(
                () => {
                    backlogService.ShiftItemsAfter(item.BacklogItemUId, planItemName, deliveryItemName);
                    return projectsService.GetProject(Project.ProjectUId);
                },
                // ugly, ugly
                p => {                                        
                    OnViewProjectDetails(p);                    
                    AskForRefresh();    
                    IsLoadingData = false;
                    aggregator.Publish(ScrumFactoryEvent.SprintsShifted);
                });
        }

 

        /// <summary>
        /// Moves the item to another item position or sprint according the drag and drop parameters.
        /// </summary>
        /// <param name="p">The drag and drop parameters.</param>
        private void MoveItemTo(ScrumFactory.Windows.Helpers.DragDrop.DropCommandParameter p) {
            
            BacklogItemViewModel item = p.Item as BacklogItemViewModel;            
            BacklogItemViewModel targetItem = p.DropTargetItem as BacklogItemViewModel;
            int? sprintNumber = p.DropTargetListTag as int?;

            // move to the product backlog
            if (sprintNumber == null)
                sprintNumber = -1;
            
            // no item no move
            if (item==null)
                return;

            // if is a plan or delivery item, cant move it
            if (item.Item.OccurrenceConstraint != (short)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                return;

            // if there is no target item, just move to the end of the sprint
            if (targetItem == null && sprintNumber != null) {
                item.ChangeSprint((int)sprintNumber, true);
                return;
            }

            // else move to the other position item
            item.MoveItemTo(targetItem.Item);
         
        }

        public void UpdateChangedItems(BacklogItem[] changedItems) {
            AskForRefresh();
        }

        private void PrepareReport(ReportConfig reportConfig) {
            ICollection<BacklogItem> backlog = backlogService.GetBacklog(Project.ProjectUId, null, (short)ScrumFactory.Services.BacklogFiltersMode.ALL);

            ICollection<BacklogItemGroup> groups = backlogService.GetBacklogItemGroups(Project.ProjectUId);

            foreach (BacklogItem item in backlog) {
                item.ValidPlannedHours = item.GetValidPlannedHours();

                int? firstSprint = item.ValidPlannedHours.Min(h => h.SprintNumber);
                item.FirstSprintWorked = firstSprint.HasValue ? firstSprint.Value : Project.LastSprint.SprintNumber;

                int? lastSprint = item.ValidPlannedHours.Max(h => h.SprintNumber);
                item.LastSprintWorked = lastSprint.HasValue ? lastSprint.Value : Project.LastSprint.SprintNumber;

                if (item.FirstSprintWorked < Project.CurrentValidSprint.SprintNumber)
                    item.OrderSprintWorked = item.LastSprintWorked;
                else
                    item.OrderSprintWorked = item.FirstSprintWorked;

                item.Group = groups.SingleOrDefault(g => g.GroupUId == item.GroupUId);
            }

            reportConfig.ReportObjects.Add(Project);
            reportConfig.ReportObjects.Add(backlog);
            reportConfig.ReportObjects.Add(groups);

            if (Project.CurrentValidSprint != null)
                reportConfig.ReportVars.Add("ProjectCurrentSprintNumber", Project.CurrentValidSprint.SprintNumber.ToString());

        }
        
        private void ShowDeliveryReport() {
            ReportConfig reportConfig = new ReportConfig("DeliveryReport", "DeliveryReport", Properties.Resources.Schedule_report);
            PrepareReport(reportConfig);            
            aggregator.Publish<ReportConfig>(ScrumFactoryEvent.ShowReport, reportConfig);
        }

        private void ShowFullDeliveryReport() {
            ReportConfig reportConfig = new ReportConfig("DeliveryReport", "FullDeliveryReport", Properties.Resources.Schedule_report);
            PrepareReport(reportConfig);            
            aggregator.Publish<ReportConfig>(ScrumFactoryEvent.ShowReport, reportConfig);
        }

        private void ShowSprintStackReport() {
            ReportConfig reportConfig = new ReportConfig("DeliveryReport", "SprintStackReport", Properties.Resources.Schedule_report);
            PrepareReport(reportConfig);
            aggregator.Publish<ReportConfig>(ScrumFactoryEvent.ShowReport, reportConfig);
        }

        private void EqualizeSprints() {

            System.Windows.MessageBoxResult d = dialogs.ShowMessageBox(
                Properties.Resources.Equalize,
                Properties.Resources.Equalise_tooltip,
                System.Windows.MessageBoxButton.OKCancel,
                "/Images/Dialogs/Equalize.png");

            if (d != System.Windows.MessageBoxResult.OK)
                return;

            IsLoadingData = true;
            executor.StartBackgroundTask(
                () => { 
                    backlogService.EqualizeSprints(Project.ProjectUId);
                    return projectsService.GetProject(Project.ProjectUId);
                },
                p => {
                    OnViewProjectDetails(p);                    
                    AskForRefresh();
                    IsLoadingData = false;
                    aggregator.Publish(ScrumFactoryEvent.SprintsShifted);
                });
        }

        private void OnViewProjectDetails(Project project) {
            Project = project;
            if (Project == null)
                return;
            if (Project.Sprints == null || Project.Sprints.Count == 0)
                CreateNewSprint();
        }

        private bool userCannotEdit;
        public bool UserCannotEdit {
            get {
                return userCannotEdit;
            }
            set {
                userCannotEdit = value;
                OnPropertyChanged("UserCannotEdit");
            }
        }

        private bool CanEditSprints() {
            if (Project == null || Project.IsTicketProject)
                return false;
            return !UserCannotEdit;
        }

        private void OnSprintsDateChanged(ICollection<Sprint> sprints) {
            // need to fix the references, i did not figure out how to do this in a better way
            foreach (Sprint s in sprints)
                s.Project = Project;

            Project.Sprints = new List<Sprint>(sprints);
            UpdateSprintsViewModels();
        }

     
        public void UpdateSprintsViewModels() {

            // sets project sprints
            if (Project.Sprints == null)
                Project.Sprints = new List<Sprint>();

            // can not live without sprints
            if (Project.Sprints == null || Project.Sprints.Count == 0) {
                if(SprintPlans!=null)
                    SprintPlans.ClearAndDispose();
                SprintPlans = new ObservableCollection<SprintViewModel>();
            }
            
            // update the sprint plans
            UpdateSprintPlans();
        }

        public int SprintLength {
            get {
                int len = SFConfig.GetIntValue("SprintLength");
                if (len == 0)
                    len = 10;
                return len;
            }         
        }

        /// <summary>
        /// Creates the new sprint and saves it.
        /// </summary>
        public void CreateNewSprint() {

            if (!Project.Memberships.Any(m => m.MemberUId == authorizator.SignedMemberProfile.MemberUId && m.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER))
                return;
            
            if (IsLoadingData)
                return;

            IsLoadingData = true;

            if (Project.Sprints == null)
                Project.Sprints = new List<Sprint>();

            DateTime startDate = DateTime.Today;
            if (Project.Sprints.Count > 0)
                startDate = calendar.AddWorkDays(Project.LastSprint.EndDate, 1);
                //startDate = Project.LastSprint.EndDate.AddDays(1);
            
            DateTime endDate = calendar.AddWorkDays(startDate, SprintLength-1);
            //DateTime endDate = startDate.AddDays(15);
            if (Project.IsTicketProject)
                endDate = startDate.AddYears(10);

            Sprint newSprint = new Sprint() {
                SprintUId = Guid.NewGuid().ToString(),
                SprintNumber = Project.NextSprintNumber,
                StartDate = startDate,
                EndDate = endDate,
                ProjectUId = Project.ProjectUId,                                
                Project = Project
            };

            string planItemName = String.Format(Properties.Resources.Plan_sprint_N, newSprint.SprintNumber);            
            string deliveryItemName = String.Format(Properties.Resources.Delivery_sprint_N, newSprint.SprintNumber);
            
            executor.StartBackgroundTask<ICollection<BacklogItem>>(
                () => {
                    ICollection<BacklogItem> items = null;
                    if(!Project.IsTicketProject)
                        items = projectsService.AddSprint(Project.ProjectUId, newSprint, true, planItemName, Properties.Resources.PLAN_REVIEW_GROUP, deliveryItemName, Properties.Resources.DELIVERY_GROUP);                     
                    else
                        items = projectsService.AddSprint(Project.ProjectUId, newSprint);                     
                    return items;
                },
                items => { AfterAddSprint(newSprint, items); });            
        }

        

        private void AfterAddSprint(Sprint newSprint, ICollection<BacklogItem> defaultItems) {

            Backlog.GroupList.LoadGroupsIfNotLoaded();

            Project.Sprints.Add(newSprint);

            ViewModel.SprintViewModel sprintVM = new ViewModel.SprintViewModel(
                projectsService,
                executor,
                aggregator,
                newSprint,
                Items,
                SFConfig,
                calendar);

            if (defaultItems != null) {
                foreach (BacklogItem item in defaultItems) {
                    BacklogItemViewModel vm = new BacklogItemViewModel(backlogService, executor, aggregator, authorizator, Project, item, SFConfig);
              
                    Items.Add(vm);
                    vm.NotifyAdded();
                }
            }

            SprintPlans.Add(sprintVM);
            sprintVM.RefreshUI();
            sprintVM.NotifyAdded();

            aggregator.Publish<Sprint>(ScrumFactoryEvent.SprintAdded, newSprint);
         
            IsLoadingData = false;
        }      

        /// <summary>
        /// Updates the Sprint plans.
        /// </summary>
        private void UpdateSprintPlans() {
                        
            if(Project==null || Project.Sprints==null || Items==null)
                return;

            // if has no sprints, create one
            if (Project.Sprints.Count == 0)
                CreateNewSprint();
            
            // create a new sprintplan collection
            if (SprintPlans != null)
                SprintPlans.ClearAndDispose();
            SprintPlans = new ObservableCollection<SprintViewModel>();

            int sprintNumber = 1;
            if (!ShowAllSprints)
                sprintNumber = Project.CurrentValidSprint.SprintNumber;
            foreach (Sprint s in Project.Sprints.Where(s => s.SprintNumber >= sprintNumber).OrderBy(s => s.SprintNumber)) {
                SprintViewModel sp = new SprintViewModel(projectsService, executor, aggregator, s, Items, SFConfig, calendar);                 
                SprintPlans.Add(sp);                
            }

            OnPropertyChanged("SprintPlans");

            LateItemsSprint = new SprintViewModel(projectsService, executor, aggregator, null, Items, SFConfig, calendar);

            //NotFinishedItems = Items.Where(
            //    vm => (vm.Item.Status != (short)BacklogItemStatus.ITEM_DONE
            //        && vm.Item.Status != (short)BacklogItemStatus.ITEM_CANCELED
            //        && vm.Item.SprintNumber < Project.CurrentValidSprint.SprintNumber) || vm.Item.SprintNumber==null).ToList();

            //OnPropertyChanged("NotFinishedItems");            
        }

        SprintViewModel lateItemsSprint;
        public SprintViewModel LateItemsSprint {
            get {
                return lateItemsSprint;
            }
            set {
                lateItemsSprint = value;
                OnPropertyChanged("LateItemsSprint");
            }
        }

        private void RemoveSprint(SprintViewModel sprintVM) {
            executor.StartBackgroundTask(
                () => { 
                    projectsService.RemoveSprint(sprintVM.Sprint.ProjectUId, sprintVM.Sprint.SprintUId);
                    return projectsService.GetProject(Project.ProjectUId);
                },                
                p => {
                    SprintPlans.Remove(sprintVM);
                    sprintVM.Dispose();
                    // ugly, ugly
                    aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);
                });
        }

        [Import]
        private Lazy<BacklogViewModel> backlog { get; set; }

        private BacklogViewModel Backlog {
            get {
                if (backlog == null)
                    return null;
                return backlog.Value;
            }
        }
        public bool UsePoints {
            get {
                if (SFConfig == null)
                    return false;
                return SFConfig.GetBoolValue("UsePoints");
            }
        }


        public string ConfigUnit {
            get {
                if (UsePoints)
                    return Properties.Resources.pts;
                else
                    return Properties.Resources.hrs;
            }
        }
        
        #region IIterationPlanningViewModel Members

        /// <summary>
        /// Gets the sprint plans.
        /// </summary>
        /// <value>The sprint plans.</value>
        public ObservableCollection<SprintViewModel> SprintPlans { set; get; }

        private Project project;

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
                SprintPlans.ClearAndDispose();
                calcZoom = true;
                AskForRefresh();

                if (project == null || authorizator == null || authorizator.SignedMemberProfile == null)
                    UserCannotEdit = true;
                else
                    UserCannotEdit = !project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);

                NeedRefresh = true;

                (CreateNewSprintCommand as DelegateCommand).NotifyCanExecuteChanged();
                (ShiftItemsCommand as DelegateCommand<BacklogItem>).NotifyCanExecuteChanged();
                (EqualizeSprintsCommand as DelegateCommand).NotifyCanExecuteChanged();

                OnPropertyChanged("Project");
         
            }
        }

        private void AskForRefresh() {
            if (View != null && View.IsVisible) {
                LoadData();
            }
            else
                NeedRefresh = true;
        }             

        private void OnBacklogItemsGroupLoaded(ICollection<BacklogItemGroup> groups) {
            AssignGroups();
        }

        private void AssignGroups() {            
            foreach (BacklogItemViewModel itemVm in Items)
                Backlog.AssignBacklogItemGroup(itemVm);                            
        }

        private void LoadData() {
            IsLoadingData = true;
            NeedRefresh = false;

            if (Backlog != null && Backlog.GroupList != null)
                Backlog.GroupList.LoadGroupsIfNotLoadedAsync();


            executor.StartBackgroundTask<ICollection<BacklogItem>>(
                () => {
                    if (Project == null)
                        return null;

                    return this.backlogService.GetCurrentBacklog(Project.ProjectUId, (short)Services.BacklogFiltersMode.PENDING);
                },
                items => {
                    List<BacklogItemViewModel> temp = new List<ViewModel.BacklogItemViewModel>();
                    foreach (BacklogItem i in items) {
                        var itemVm = new BacklogItemViewModel(backlogService, executor, aggregator, authorizator, Project, i, SFConfig);            
                        temp.Add(itemVm);
                    }

                    Items = temp;
                    if (calcZoom) {
                        MaxZoomFactor = CalcMaxZoomFactor();
                        ZoomFactor = CalcFitZoomFactor();
                        calcZoom = false;
                    }
                    
                    AssignGroups();

                    // load data is over
                    IsLoadingData = false;
                });         
        }

        private double CalcFitZoomFactor() {
            var itemsWithHours = Items.Where(i => i.Item.CurrentTotalHours > 1 &&
                i.Item.OccurrenceConstraint != (short)ItemOccurrenceContraints.PLANNING_OCC &&
                i.Item.OccurrenceConstraint != (short)ItemOccurrenceContraints.DELIVERY_OCC).ToArray();

            double minZoom = _CalcMinZoomFactor(itemsWithHours);
            double maxZoom = _CalcMaxZoomFactor();
            
            if (maxZoom > MaxZoomFactor)
                maxZoom = MaxZoomFactor;

            if (maxZoom > minZoom && maxZoom>0)
                return maxZoom;

            return minZoom;

        }

        private double CalcMaxZoomFactor() {
            var itemsWithHours = Items.Where(i => i.Item.CurrentTotalHours > 0).ToArray();
            
            double minZoom = _CalcMinZoomFactor(itemsWithHours);
            double maxZoom = _CalcMaxZoomFactor();

            if (maxZoom > minZoom && maxZoom > 0)
                return maxZoom;

            return minZoom;
        }

        private double _CalcMinZoomFactor(BacklogItemViewModel[] itemsWithHours) {            
            if (itemsWithHours == null || itemsWithHours.Length == 0)
                return 3;
            decimal? minHours = itemsWithHours.Min(i => i.Item.CurrentTotalHours);
            if (!minHours.HasValue)
                return 3;
            return (double)(14 / minHours.Value);
        }

        private double _CalcMaxZoomFactor() {
            decimal? max = SprintPlans.Max(p => p.TotalHours);

            if (!max.HasValue) {
                max = LateItemsSprint.TotalHours;
            } else {
                max = LateItemsSprint.TotalHours > max ? LateItemsSprint.TotalHours : max.Value;
            }

            double maxZoom = 0;
            if (max.HasValue && max.Value > 0) {
                maxZoom = (double)(400 / max.Value);
            }
            return maxZoom;
        }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        [Import(typeof(IterationPlanning))]
        public IView View { get; set; }

        /// <summary>
        /// Gets the not finished items.
        /// </summary>
        /// <value>The not finished items.</value>
        public ICollection<BacklogItemViewModel> NotFinishedItems { get; private set; }
        private ICollection<BacklogItemViewModel> items = new List<BacklogItemViewModel>();

        public ICollection<BacklogItemViewModel> Items {
            get {
                return items;
            }
            set {
                items.ClearAndDispose();
                items = value;                
                UpdateSprintsViewModels();
            }
        }

        /// <summary>
        /// Gets the create new sprint command.
        /// </summary>
        /// <value>The create new sprint command.</value>
        public ICommand CreateNewSprintCommand { private set; get; }


        /// <summary>
        /// Gets or sets the equalize sprints command.
        /// </summary>
        /// <value>The equalize sprints command.</value>
        public ICommand EqualizeSprintsCommand { get; set; }


        /// <summary>
        /// Gets or sets the delete sprint command.
        /// </summary>
        /// <value>The delete sprint command.</value>
        public ICommand RemoveSprintCommand { private set; get; }

        /// <summary>
        /// Gets or sets the show delivery report command.
        /// </summary>
        /// <value>The show delivery report command.</value>
        public ICommand ShowDeliveryReportCommand { get; set; }

        /// <summary>
        /// Gets or sets the move item to command.
        /// </summary>
        /// <value>The move item to command.</value>
        public ICommand MoveItemToCommand { private set; get; }


        public ICommand ShiftItemsCommand { get; set; }

        public ICommand LoadDataCommand { get; set; }

        public ICommand ShowDetailCommand { get; set; }

        public ICommand ShowSprintStackReportCommand { get; set; }

        public ICommand ShowFullDeliveryReportCommand { get; set; }

        #endregion

        #region IPanelViewModel Members

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.Sprints;
            }
        }

        

        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        public int PanelDisplayOrder {
            get {
                return 300;
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
