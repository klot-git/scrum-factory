using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Services;
using System.Windows.Input;
using System.Linq;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;

namespace ScrumFactory.Backlog.ViewModel {

    [Export]    
    public class SizeListViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private const string SINGLE_POINT_UID = "__SINGLE_POINT_SIZE_________________";

        private IBacklogService backlogService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;

        private System.Windows.Data.CollectionViewSource sizesViewSource;

        private DelayAction delayFilter;

        private ICollection<SizeViewModel> sizes;
        private SizeViewModel selectedSize;
        private ItemSize oldSelectedSize;

        private string searchFilterText;

        private IDialogService dialogs;
            
        [ImportingConstructor]
        public SizeListViewModel(
            [Import] IBacklogService backlogService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogs,
            [Import] IAuthorizationService authorizator) {

            this.backlogService = backlogService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;
            this.authorizator = authorizator;
            this.dialogs = dialogs;

            AddNewSizeCommand = new DelegateCommand(CanEditItemSizes, AddNewSize);
            CloseWindowCommand = new DelegateCommand(CloseWindow);
            DeleteSizeCommand = new DelegateCommand<SizeViewModel>(CanEditItemSizes, DeleteSize);
            SetAsPlanningItemCommand = new DelegateCommand<SizeViewModel>(CanEditItemSizes, size => SetItemOccurrenceContraint(size, ItemOccurrenceContraints.PLANNING_OCC));
            SetAsDeliveryItemCommand = new DelegateCommand<SizeViewModel>(CanEditItemSizes, size => SetItemOccurrenceContraint(size, ItemOccurrenceContraints.DELIVERY_OCC));

            sizesViewSource = new System.Windows.Data.CollectionViewSource();
            sizesViewSource.Filter += new System.Windows.Data.FilterEventHandler(sizesViewSource_Filter);
            delayFilter = new DelayAction(1200, new DelayAction.ActionDelegate(() => { FilteredSizes.Refresh(); }));

            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged,
                m => {
                    RefreshCommands();
                    if (m == null) return;
                    if (Sizes!=null) return;
                    LoadItemSizes();
                });


            
        }

        private void RefreshCommands() {
            ((DelegateCommand)AddNewSizeCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<SizeViewModel>)DeleteSizeCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<SizeViewModel>)SetAsDeliveryItemCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<SizeViewModel>)SetAsPlanningItemCommand).NotifyCanExecuteChanged();
         
        }

        private void SetItemOccurrenceContraint(SizeViewModel sizeVM, ItemOccurrenceContraints constraint) {
            
            executor.StartBackgroundTask(
                () => { backlogService.UpdateItemSizeOccurrenceContraint(sizeVM.ItemSize.ItemSizeUId, constraint.ToString()); },
                () => {
                    // remove the previous planning/delivery selection
                    if(constraint==ItemOccurrenceContraints.PLANNING_OCC)
                        PlanningSize.OccurrenceConstraint = (int) ItemOccurrenceContraints.DEVELOPMENT_OCC;
                    if (constraint == ItemOccurrenceContraints.DELIVERY_OCC)
                        DeliverySize.OccurrenceConstraint = (int)ItemOccurrenceContraints.DEVELOPMENT_OCC;

                    // mark the new one
                    sizeVM.OccurrenceConstraint = (int)constraint;
                });
        }


        
        private void DeleteSize(SizeViewModel sizeVM) {            
            executor.StartBackgroundTask(
                () => { backlogService.DeleteItemSize(sizeVM.ItemSize.ItemSizeUId); },
                () => { OnItemSizeDeleted(sizeVM); });
        }

        private void OnItemSizeDeleted(SizeViewModel deletedSize) {                           
            Sizes.Remove(deletedSize);            
            aggregator.Publish(ScrumFactoryEvent.ActiveSizesChanged);            
        }

        private void sizesViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {
            
            if (string.IsNullOrEmpty(SearchFilterText)) {
                e.Accepted = true;
                return;
            }

            e.Accepted = false;

            SizeViewModel sizeVM = e.Item as SizeViewModel;
            if (sizeVM == null)
                return;

            e.Accepted = sizeVM.ItemSize.Name.ToUpper().Contains(SearchFilterText.ToUpper())
                || sizeVM.ItemSize.Size.ToString().Equals(SearchFilterText);

        }

        private void LoadItemSizes() {
            executor.StartBackgroundTask<ICollection<ItemSize>>(
                () => {
                    IsLoadingData = true;
                    return backlogService.GetItemSizes();
                },
                sizes => {                    
                    Sizes = new ObservableCollection<SizeViewModel>();
                    foreach (ItemSize size in sizes) {
                        if (size.SizeIdealHours == null || size.SizeIdealHours.Count == 0)
                            AddDefaultIdealHours(size);
                        size.SizeIdealHours = size.SizeIdealHours.OrderByDescending(h => h.RoleShortName).ToList();
                        Sizes.Add(new SizeViewModel(size));                        
                    }


                    sizesViewSource.Source = Sizes;
                    if (sizesViewSource.SortDescriptions.Count == 0) {
                        sizesViewSource.SortDescriptions.Add(new SortDescription("ItemSize.Name", ListSortDirection.Ascending));
                        sizesViewSource.SortDescriptions.Add(new SortDescription("ItemSize.Size", ListSortDirection.Ascending));                        
                    }               
                    
                    IsLoadingData = false;
                    OnPropertyChanged("FilteredSizes");
                    aggregator.Publish(ScrumFactoryEvent.ActiveSizesChanged);            
                });
        }

     

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void CloseWindow() {
            if (HasSizeChanged)
                SaveOldSize();
            Close();
            
        }

        [Import(typeof(SizeList))]
        public IView View { get; set; }

        private void AddDefaultIdealHours(ItemSize size) {
            size.SizeIdealHours = new List<SizeIdealHour>() {
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = Properties.Resources.SM_ROLE_SN, ItemSizeUId = size.ItemSizeUId },
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = Properties.Resources.TEAM_ROLE_SN, ItemSizeUId = size.ItemSizeUId },
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = Properties.Resources.PO_ROLE_SN, ItemSizeUId = size.ItemSizeUId },
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = "", ItemSizeUId = size.ItemSizeUId },
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = "", ItemSizeUId = size.ItemSizeUId },
                new SizeIdealHour() { Hours = 0, IdealHourUId = System.Guid.NewGuid().ToString(), RoleShortName = "", ItemSizeUId = size.ItemSizeUId }
            };
        }

        private void AddNewSize() {
            ItemSize newSize = new ItemSize() { ItemSizeUId = System.Guid.NewGuid().ToString(), Size = 0, Name = Properties.Resources.New_size, IsActive = true, Description = string.Empty, OccurrenceConstraint = (int) ItemOccurrenceContraints.DEVELOPMENT_OCC };

            AddDefaultIdealHours(newSize);

            executor.StartBackgroundTask(
                () => { backlogService.AddItemSize(newSize);},
                () => {
                    SizeViewModel sizeVm = new SizeViewModel(newSize);                    
                    Sizes.Add(sizeVm);
                    SelectedSize = sizeVm;
                    sizeVm.NotifyAdded();
                });
            
            aggregator.Publish(ScrumFactoryEvent.ActiveSizesChanged);            
        }


 
        private SizeViewModel CreateSinglePointSize() {

            ItemSize singleSize = new ItemSize();
            singleSize.ItemSizeUId = SINGLE_POINT_UID;
            singleSize.Name = Properties.Resources.One_unit_point;
            singleSize.Description = Properties.Resources.One_unit_point;
            singleSize.Size = 1;
            singleSize.IsActive = true;
            singleSize.OccurrenceConstraint = 1;

            backlogService.AddItemSize(singleSize);

            SizeViewModel sizeVm = new SizeViewModel(singleSize);
            Sizes.Add(sizeVm);
            SelectedSize = sizeVm;
            sizeVm.NotifyAdded();            
            
            aggregator.Publish(ScrumFactoryEvent.ActiveSizesChanged);

            return sizeVm;
            
        }


        private bool HasSizeChanged {
            get {
                if (oldSelectedSize == null)
                    return false;
                SizeViewModel actualSizeVM = Sizes.SingleOrDefault(z => z.ItemSize.ItemSizeUId == oldSelectedSize.ItemSizeUId);
                if (actualSizeVM == null)
                    return false;

                return !actualSizeVM.ItemSize.IsTheSame(oldSelectedSize);
            }
        }

        private void SaveOldSize() {
            if (oldSelectedSize == null)
                return;
            SizeViewModel actualSizeVM = Sizes.SingleOrDefault(z => z.ItemSize.ItemSizeUId == oldSelectedSize.ItemSizeUId);
            if (actualSizeVM == null)
                return;
            
            //aggregator.Publish<Role>(ScrumFactoryEvent.ProjectRoleChanged, actualRole);

            executor.StartBackgroundTask(() => {
                backlogService.UpdateItemSize(actualSizeVM.ItemSize.ItemSizeUId, actualSizeVM.ItemSize);                
            }, () => {
                aggregator.Publish(ScrumFactoryEvent.ActiveSizesChanged);
            });
        }

        #region IItemSizeListViewModel Members

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show(IChildWindow parentWindow) {
            if (Sizes == null)
                LoadItemSizes();
            base.Show(parentWindow);          
        }

        public ICollectionView FilteredSizes {
            get {
                return sizesViewSource.View;
            }
        }

        public ICollection<SizeViewModel> Sizes {
            get {
                //if (sizes == null && authorizator.SignedMemberProfile!=null)
                //    LoadItemSizes();
                return sizes;
            }
            set {
                sizes = value;
                OnPropertyChanged("Sizes");
            }
        }

        public SizeViewModel SingleSize {
            get {
                if (sizes == null)
                    return null;
                var size = sizes.SingleOrDefault(z => z.ItemSize.ItemSizeUId == SINGLE_POINT_UID);
                if (size == null)
                    size = CreateSinglePointSize();
                return size;
            }
        }

        /// <summary>
        /// Gets the size of the default planning item.
        /// </summary>
        /// <value>The size of the default planning item.</value>
        public SizeViewModel PlanningSize {
            get {
                if (sizes == null)
                    return null;
                return sizes.SingleOrDefault(z => z.ItemSize.OccurrenceConstraint == (int)ItemOccurrenceContraints.PLANNING_OCC);                                
            }
        }

        /// <summary>
        /// Gets the size of the default delivery item.
        /// </summary>
        /// <value>The size of the default delivery item.</value>
        public SizeViewModel DeliverySize {
            get {
                if (sizes == null)
                    return null;
                return sizes.SingleOrDefault(z => z.ItemSize.OccurrenceConstraint == (int)ItemOccurrenceContraints.DELIVERY_OCC);                
            }
        }

        public SizeViewModel SelectedSize {
            get {
                return selectedSize;
            }
            set {

                if (HasSizeChanged)
                    SaveOldSize();

                if (value != null && value.ItemSize != null)
                    oldSelectedSize = value.ItemSize.Clone();
                else
                    oldSelectedSize = null;

                selectedSize = value;
                OnPropertyChanged("SelectedSize");
            }
        }

        public string SearchFilterText {
            get {
                return searchFilterText;
            }
            set {
                searchFilterText = value;
                delayFilter.StartAction();
                OnPropertyChanged("SearchFilterText");                
            }

        }

        private bool CanEditItemSizes() {
            if (authorizator.SignedMemberProfile == null)
                return false;
            return authorizator.SignedMemberProfile.IsFactoryOwner;
        }

        public ICommand AddNewSizeCommand { get; set; }
        public ICommand DeleteSizeCommand { get; set;}
        public ICommand CloseWindowCommand { get; set; }   

        public ICommand SetAsPlanningItemCommand { get; set; }
        public ICommand SetAsDeliveryItemCommand { get; set; }
        
        #endregion

        #region IPanelViewModel Members

        public string PanelName {
            get { return Properties.Resources.Item_sizes; }
        }

       
        public int PanelDisplayOrder {
            get { return 0; }
        }

     

        #endregion
    }
}
