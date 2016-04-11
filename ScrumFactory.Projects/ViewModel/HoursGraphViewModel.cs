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
using System.Linq;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Projects.ViewModel {


    
    [Export]
    public class HoursGraphViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IBacklogService backlogService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;

        public Project Project { get; private set; }

        [ImportingConstructor()]
        public HoursGraphViewModel(
            [Import] IBacklogService backlogService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator) {

            this.backlogService = backlogService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;

            NeedRefresh = false;

            OnLoadCommand = new DelegateCommand(() => {if (NeedRefresh) LoadData();});


            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, AskForRefresh);

            aggregator.Subscribe<Project>(ScrumFactoryEvent.BurndownShouldRefresh, AskForRefresh);
            
            aggregator.Subscribe<BacklogItem[]>(ScrumFactoryEvent.BacklogItemsChanged, b => { AskForRefresh(); });

            EffectiveHours = new List<HourColumn>();
            PlannedHours = new List<HourColumn>();

        }

        private ICollection<BacklogItem> items = null;
        private ICollection<BacklogItem> plannedItems = null;

        private double zoom = 1;
        public double Zoom {
            get {
                return zoom;
            }
            set {
                zoom = value;
                OnPropertyChanged("Zoom");
                OnPropertyChanged("ZoomLegend");
                Refresh();
            }
        }

        public string ZoomLegend {
            get {
                if (Zoom == 0)
                    return Properties.Resources.ZoomLevel_Project;
                if (Zoom == 1)
                    return Properties.Resources.ZoomLevel_Structures;
                if (Zoom == 2)
                    return Properties.Resources.ZoomLevel_Items;
                return Properties.Resources.ZoomLevel_Roles;
            }
        }

        private ICollection<HourColumn> effectiveHours;

        public ICollection<HourColumn> EffectiveHours {
            get {
                return effectiveHours;
            }
            set {
                effectiveHours = value;
                OnPropertyChanged("EffectiveHours");
                OnPropertyChanged("TotalEffectiveHours");
            }
        }

        private ICollection<HourColumn> plannedHours;

        public ICollection<HourColumn> PlannedHours {
            get {
                return plannedHours;
            }
            set {
                plannedHours = value;
                OnPropertyChanged("PlannedHours");
                OnPropertyChanged("TotalPlannedHours");
            }
        }

        public decimal TotalEffectiveHours {
            get {
                if (effectiveHours == null)
                    return 0;
                return effectiveHours.Sum(h => h.Hours);
            }
        }

        public decimal TotalPlannedHours {
            get {
                if (plannedHours == null)
                    return 0;
                return plannedHours.Sum(h => h.Hours);
            }
        }


        public bool IsVisible {
            get {
                return View.IsVisible;
            }            
        }

        private void AskForRefresh() {
            AskForRefresh(Project);
        }

        private void AskForRefresh(Project project) {
            Project = project;
            if (IsVisible)
                LoadData();
            else
                NeedRefresh = true;            
        }
      

        public void LoadData() {                        
            IsLoadingData = true;
            PlannedHours = new List<HourColumn>();
            EffectiveHours = new List<HourColumn>();
            items = null; plannedItems = null;
            executor.StartBackgroundTask(() => { items = backlogService.GetItemsTotalEffectiveHours(Project.ProjectUId); }, OnDataLoaded);
            executor.StartBackgroundTask(() => { plannedItems = backlogService.GetBacklog(Project.ProjectUId, "0");}, OnDataLoaded);
            
        }



        private void OnDataLoaded() {
            if (items == null || plannedItems == null)
                return;

            IsLoadingData = false;
            NeedRefresh = false;            
            // the planned items dont have groups associate with them, so linked
            foreach(BacklogItem item in plannedItems) {
                BacklogItem itemWithGroup = items.FirstOrDefault(i => i.GroupUId == item.GroupUId);
                if (itemWithGroup != null)
                    item.Group = itemWithGroup.Group;
            }
            Refresh();
        }

        private void Refresh() {
            if (items == null)
                return;
            if (Zoom == 0) {
                EffectiveHours = CreateProjectHours(items);
                PlannedHours = CreateProjectHours(plannedItems);
            }

            if (Zoom == 1) {
                EffectiveHours = CreateStructureHours(items);
                PlannedHours = CreateStructureHours(plannedItems);
            }

            if (Zoom == 2) {
                EffectiveHours = CreateItemsHours(items);
                PlannedHours = CreateItemsHours(plannedItems);                
            }

            if (Zoom == 3) {
                EffectiveHours = CreateRoleHours(items);
                PlannedHours = CreateRoleHours(plannedItems);
            }
        }

        private ICollection<HourColumn> CreateItemsHours(ICollection<BacklogItem> items) {            
            if (items == null)
                return null;
            List<HourColumn> temp = new List<HourColumn>();
            foreach (BacklogItem item in items.OrderBy(i => i.BacklogItemNumber)) {
                HourColumn column = new HourColumn() { Name = item.BacklogItemNumber.ToString() };

                if (item.PlannedHours != null) {
                    decimal? hours =  item.PlannedHours.Sum(h => h.Hours);
                    if(!hours.HasValue)
                        hours =0;
                    column.Hours = hours.Value;
                }

                temp.Add(column);
            }

            return temp;
        }

        private ICollection<HourColumn> CreateStructureHours(ICollection<BacklogItem> items) {            
            if (items == null)
                return null;
            List<HourColumn> temp = new List<HourColumn>();
            foreach (string groupUId in items.GroupBy(i => i.GroupUId).Select(g => g.Key)) {

                BacklogItem item = items.FirstOrDefault(i => i.GroupUId == groupUId);                
                if (item != null && item.Group!=null) {

                    HourColumn column = new HourColumn() { Name = item.Group.GroupName };

                    decimal? hours = items.Where(i => i.GroupUId == groupUId && i.PlannedHours != null).Sum(i => i.PlannedHours.Sum(h => h.Hours));
                    if (!hours.HasValue)
                        hours = 0;
                    column.Hours = hours.Value;

                    temp.Add(column);
                }

            }

            return temp;
        }

        private ICollection<HourColumn> CreateRoleHours(ICollection<BacklogItem> items) {
            if (items == null)
                return null;
            List<HourColumn> temp = new List<HourColumn>();
            foreach (Role role in Project.Roles) {
                
                HourColumn column = new HourColumn() { Name = role.RoleName };

                decimal? hours = items.Where(i=>i.PlannedHours!=null).Sum(i => i.PlannedHours.Where(h => h.RoleUId== role.RoleUId).Sum(h => h.Hours));
                if (!hours.HasValue)
                    hours = 0;
                column.Hours = hours.Value;

                temp.Add(column);
                
            }

            return temp;
        }

        private ICollection<HourColumn> CreateProjectHours(ICollection<BacklogItem> items) {            
            if (items == null)
                return null;
            List<HourColumn> temp = new List<HourColumn>();
            
            decimal? hours = items.Where(i => i.PlannedHours!=null).Sum(i => i.PlannedHours.Sum(h => h.Hours));
            if (!hours.HasValue)
                hours = 0;

            HourColumn column = new HourColumn() { Name = Project.ProjectName, Hours = hours.Value };

            temp.Add(column);

            return temp;
        }

        

       

        [Import(typeof(HoursGraph))]
        public IView View { get; set; }

     
        public ICommand OnLoadCommand { get; private set; }

       
    }



    public class HourColumn {
        public string Name { get; set; }
        public decimal Hours { get; set; }
        public string Tooltip { get; set; }
    }
}
