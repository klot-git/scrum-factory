using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ScrumFactory.Services;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;

namespace ScrumFactory.Backlog.ViewModel {

    
    public class SprintViewModel : BaseEditableObjectViewModel, INotifyPropertyChanged, IEditableObjectViewModel {
                       
        private CollectionViewSource backlogViewSource;

        private IProjectsService projectsService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;

        private ICalendarService calendar;

        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SprintViewModel"/> class.
        /// </summary>
        /// <param name="sprint">The sprint.</param>
        /// <param name="backlog">The backlog.</param>
        public SprintViewModel(
            IProjectsService projectsService,
            IBackgroundExecutor backgroundExecutor,
            IEventAggregator eventAggregator,
            Sprint sprint,
            ICollection<BacklogItemViewModel> backlog,
            ScrumFactory.Composition.Configuration sfConfig,
            ICalendarService calendar) {

            this.projectsService = projectsService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;

            this.calendar = calendar;

            SFConfig = sfConfig;
        
            Sprint = sprint;            
            backlogViewSource = new CollectionViewSource();
            backlogViewSource.Source = backlog;
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.OccurrenceConstraint", ListSortDirection.Ascending));
            backlogViewSource.SortDescriptions.Add(new SortDescription("Item.BusinessPriority", ListSortDirection.Ascending));
            backlogViewSource.Filter += new FilterEventHandler(backlogViewSource_Filter);

        
            
        }

        

        protected override void OnDispose() {
            aggregator.UnSubscribeAll(this);
        }

        ~SprintViewModel() {
            System.Console.Out.WriteLine("***< sprint died here");
        }

        /// <summary>
        /// Handles the Filter event of the backlogViewSource control.
        /// Only backlog items wich hours planned to this sprint, and not canceled are accepted.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Data.FilterEventArgs"/> instance containing the event data.</param>
        private void backlogViewSource_Filter(object sender, FilterEventArgs e) {
            BacklogItemViewModel itemVM = e.Item as BacklogItemViewModel;
            e.Accepted = false;
            
            if (itemVM == null || itemVM.Item==null)
                return;

            if (Sprint != null) {
                if (itemVM.Item.SprintNumber.Equals(Sprint.SprintNumber) && itemVM.Item.Status != (short)BacklogItemStatus.ITEM_CANCELED)
                    e.Accepted = true;
            }
            else {
                // dont want to show plan and delivery itens here
                if (itemVM.Item.OccurrenceConstraint != (short)ItemOccurrenceContraints.DEVELOPMENT_OCC) {
                    e.Accepted = false;
                    return;
                }
                if (itemVM.Item.SprintNumber == null || itemVM.IsPlanningLate)
                    e.Accepted = true;
            }

        }
        
        #region ISprintViewModel Members

        /// <summary>
        /// Gets the sprint.
        /// </summary>
        /// <value>The sprint.</value>
        public Sprint Sprint { get; private set; }

        /// <summary>
        /// Gets the planned items.
        /// </summary>
        /// <value>
        /// The planned items as a collection view of BacklogItemDetailViewModels.
        /// </value>
        public ICollectionView PlannedItems {
            get {
                return backlogViewSource.View;
            }
        }


        public System.DateTime SprintMinStartDate {
            get {
                if (Sprint == null || Sprint.Project == null || Sprint.Project.Sprints==null)
                    return System.DateTime.MinValue;

                if(Sprint.SprintNumber==1)
                    return System.DateTime.MinValue;

                Sprint previous = Sprint.Project.Sprints.SingleOrDefault(s => s.SprintNumber == Sprint.SprintNumber - 1);
                if(previous==null)
                    return System.DateTime.MinValue;
                return previous.StartDate.AddDays(2);                
            }
        }

        /// <summary>
        /// Gets or sets the sprint end date.
        /// </summary>
        /// <value>The sprint end date.</value>
        public System.DateTime SprintEndDate {
            get {
                return Sprint.EndDate;
            }
            set {
                Sprint.EndDate = value;
                OnPropertyChanged("SprintEndDate");                
                OnPropertyChanged("Sprint");

                executor.StartBackgroundTask<ICollection<Sprint>>(
                    () => { return projectsService.UpdateSprint(Sprint.ProjectUId, Sprint.SprintUId, Sprint); },
                    sprints => { aggregator.Publish<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, sprints); });
            }
        }

        /// <summary>
        /// Gets or sets the sprint start date.
        /// </summary>
        /// <value>The sprint start date.</value>
        public System.DateTime SprintStartDate {
            get {
                return Sprint.StartDate;
            }
            set {
                Sprint.StartDate = value;
                OnPropertyChanged("SprintStartDate");
                OnPropertyChanged("Sprint");

                executor.StartBackgroundTask<ICollection<Sprint>>(
                    () => { return projectsService.UpdateSprint(Sprint.ProjectUId, Sprint.SprintUId, Sprint); },
                    sprints => { aggregator.Publish<ICollection<Sprint>>(ScrumFactoryEvent.SprintsDateChanged, sprints); });
            }
        }

        /// <summary>
        /// Gets the total hours of this Sprint.
        /// It includes all items planned at the Sprint, but the CANCELED ones.
        /// </summary>
        /// <value>The total hours.</value>
        public decimal TotalHours {
            get {
                if (PlannedItems == null)
                    return 0;

                ICollection<BacklogItemViewModel> items = PlannedItems.SourceCollection as ICollection<BacklogItemViewModel>;

                if (Sprint == null) {
                    return items.Where(i => i.Item.SprintNumber==null && i.Item.Status != (short)BacklogItemStatus.ITEM_CANCELED).Sum(i => i.Item.CurrentTotalHours);
                }

                return items.Where(i => i.Item.SprintNumber.Equals(Sprint.SprintNumber) && i.Item.Status != (short)BacklogItemStatus.ITEM_CANCELED).Sum(i => i.Item.CurrentTotalHours);
            }
        }

        public bool UsePoints {
            get {
                if (SFConfig == null)
                    return false;
                return SFConfig.GetBoolValue("UsePoints");
            }
        }

        public decimal ConfigTotal {
            get {
                if (!UsePoints)
                    return TotalHours;

                ICollection<BacklogItemViewModel> items = PlannedItems.SourceCollection as ICollection<BacklogItemViewModel>;
                return (decimal) items.Where(i => i.Item.SprintNumber.Equals(Sprint.SprintNumber) && i.Item.Status != (short)BacklogItemStatus.ITEM_CANCELED && i.Item.Size.HasValue).Sum(i => i.Item.Size);
            }
        }

        /// <summary>
        /// Refreshes the corresponding View of the View Model.
        /// </summary>
        public void RefreshUI() {
            PlannedItems.Refresh();

            OnPropertyChanged("ConfigTotal");
            OnPropertyChanged("TotalHours");
            OnPropertyChanged("NumberOfMembersNeededForDoIt");
            OnPropertyChanged("NumberOfMembersNeededForDoItInPixels");
            OnPropertyChanged("WorkDayCount");
            OnPropertyChanged("WorkDaysLeft");
        }

        /// <summary>
        /// Gets the number of members needed to finish the Sprint until
        /// its end date.
        /// </summary>
        /// <value>The number of members needed to finish it.</value>
        public double NumberOfMembersNeededForDoIt {
            get {
                return (double)TotalHours / WorkDayCount / 6;
            }
        }

        /// <summary>
        /// Gets the number of members needed to finish the Sprint until
        /// its end date, converted in Pixels to display the team member icons.
        /// </summary>
        /// <value>The number of members needed to finish it converted in pixels.</value>
        public double NumberOfMembersNeededForDoItInPixels {
            get {
                return NumberOfMembersNeededForDoIt * 16;
            }
        }

        public int WorkDayCount {
            get {
                return calendar.CalcWorkDayCount(Sprint.StartDate, Sprint.EndDate);
            }
        }

        public int WorkDaysLeft {
            get {
                return calendar.CalcWorkDayCount(System.DateTime.Today, Sprint.EndDate);
            }
        }

        #endregion
     
     
    }
}
