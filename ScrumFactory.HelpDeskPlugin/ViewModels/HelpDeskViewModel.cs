using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Tasks.ViewModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ScrumFactory.Extensions;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Windows.Helpers.Extensions;



namespace ScrumFactory.HelpDeskPlugin.ViewModels {

    [Export]
    [Export(typeof(ITopMenuViewModel))]
    public class HelpDeskViewModel : BasePanelViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        [Import]
        private Services.IProjectsService projectsService { get; set; }
        
        [Import]
        private Services.IBacklogService backlogService { get; set; }

        [Import]
        private Services.ITasksService taskService { get; set; }

        [Import]
        private Services.IAuthorizationService authorizator { get; set; }
                
        [Import]
        private IBackgroundExecutor executor { get; set; }

        [Import]
        private IDialogService dialogs { get; set; }

        [Import]
        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        [Import]
        public IArtifactsListViewModel ArtifactListViewModel { get; set; }

        private ScrumFactory.Composition.IEventAggregator aggregator;

        private DelayAction delayFilter;

        private string searchFilterText;
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

        private ICollection<Project> projects;
        public ICollection<Project> Projects {
            get {
                return projects;
            }
            set {
                projects = value;
                OnPropertyChanged("Projects");
            }
        }

        private Project newTicketProject;
        public Project NewTicketProject {
            get {
                return newTicketProject;
            }
            set {
                newTicketProject = value;
                OnPropertyChanged("NewTicketProject");
            }
        }

        private string newTicketName;
        public string NewTicketName {
            get {
                return newTicketName;
            }
            set {
                newTicketName = value;
                OnPropertyChanged("NewTicketName");
            }
        }


     

    
        private void AddJob() {

            if (String.IsNullOrEmpty(NewTicketName))
                return;

            if (NewTicketProject == null)
                return;

            // create the new item
            BacklogItem newItem = new BacklogItem {
                BacklogItemUId = Guid.NewGuid().ToString(),
                ProjectUId = NewTicketProject.ProjectUId,
                Name = NewTicketName,
                Description = null,
                Status = (short)BacklogItemStatus.ITEM_REQUIRED,
                BusinessPriority = 0,
                OccurrenceConstraint = 1,
                SizeFactor = 1,
                CreateDate = DateTime.Now
            };

            

            // save its
            executor.StartBackgroundTask(
                () => {

                    // needs to get the project here to get its roles
                    Project project = projectsService.GetProject(newItem.ProjectUId);

                    newItem.Project = project;
                    newItem.SyncPlannedHoursAndRoles(1);

                    ICollection<BacklogItemGroup> groups = backlogService.GetBacklogItemGroups(project.ProjectUId);
                    BacklogItemGroup group = groups.FirstOrDefault(g => g.DefaultGroup == (short)DefaultItemGroups.DEV_GROUP);                    
                    if (group != null)
                        newItem.GroupUId = group.GroupUId;

                    backlogService.AddBacklogItem(newItem); 
                },
                () => {                    
                    BacklogItemViewModel vm = new BacklogItemViewModel();
                    vm.Init(backlogService, executor, aggregator, authorizator, newTicketProject, newItem);

                    ICollection<BacklogItemViewModel> tickets = ticketsSource.Source as ICollection<BacklogItemViewModel>;
                    tickets.Add(vm);

                    ((IEditableCollectionView)ticketsSource.View).EditItem(vm);
                    ((IEditableCollectionView)ticketsSource.View).CommitEdit();
                 
                    vm.NotifyAdded();

                    NewTicketProject = null;
                    NewTicketName = String.Empty;

                    
                });

        }

   

        private void ShowItem(BacklogItem item) {
            if (item==null)
                return;

            executor.StartBackgroundTask<Project>(
               () => { return projectsService.GetProject(item.ProjectUId); },
               p => {                   
                   item.Project = p;
                   item.SyncPlannedHoursAndRoles();
                   dialogs.SetBackTopMenu();            
                   aggregator.Publish<BacklogItem>(ScrumFactoryEvent.ShowItemDetail, item);                   
               });
                                  
            
        }

       
        private bool onlyMineProjects = true;
        public bool OnlyMineProjects {
            get {
                return onlyMineProjects;
            }
            set {
                onlyMineProjects = value;
                OnPropertyChanged("OnlyMineProjects");
                LoadData();
            }
        }


        [ImportingConstructor]
        public HelpDeskViewModel([Import] ScrumFactory.Composition.IEventAggregator aggregator) {

            this.aggregator = aggregator;

            aggregator.Subscribe<string>(ScrumFactoryEvent.ConfigChanged, c => {
                if (c == "TicketProjectsEnabled")
                    OnPropertyChanged("PanelPlacement");
        
            });

            OnLoadCommand = new ScrumFactory.Composition.DelegateCommand(() => { if (NeedRefresh) LoadData(); });

            RefreshCommand = new ScrumFactory.Composition.DelegateCommand(LoadData);

          
            ShowItemCommand = new ScrumFactory.Composition.DelegateCommand<BacklogItem>(ShowItem);

            AddJobCommand = new ScrumFactory.Composition.DelegateCommand(AddJob);
       
  

            delayFilter = new DelayAction(800, new DelayAction.ActionDelegate(() => { ticketsSource.View.Refresh(); }));

            ticketsSource = new System.Windows.Data.CollectionViewSource();
            ticketsSource.Filter += new System.Windows.Data.FilterEventHandler(ticketsSource_Filter);

            NeedRefresh = true;
        }

        void ticketsSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {

            e.Accepted = true;

            if (String.IsNullOrEmpty(SearchFilterText))
                return;

            BacklogItemViewModel vm = e.Item as BacklogItemViewModel;
            if (vm == null)
                return;

            string[] tags = SearchFilterText.NormalizeD().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tags.All(t => 
                vm.Item.Project.ProjectName.NormalizeD().Contains(t) ||
                vm.Item.Project.ClientName.NormalizeD().Contains(t) ||
                vm.Item.Name.NormalizeD().Contains(t) ||
                t.Equals(vm.Item.BacklogItemNumber.ToString())))
                e.Accepted = true;
            else
                e.Accepted = false;            
        }

        private void LoadData() {            
            LoadProjects(LoadBacklogItems);
            NeedRefresh = false;
        }

        private void LoadBacklogItems() {
            
            executor.StartBackgroundTask<ICollection<BacklogItem>>(() => {
                return backlogService.GetAllUnfinishedBacklogItems(OnlyMineProjects, "HELPDESK_PROJECTS");
            },
            items => {
                ObservableCollection<BacklogItemViewModel> vms = new ObservableCollection<BacklogItemViewModel>();
                foreach (BacklogItem i in items.OrderBy(i => i.DeliveryDate).OrderBy(i => i.IssueType).ThenBy(i => i.CreateDate).ThenBy(i => i.BusinessPriority)) {
                    Project project = projects.SingleOrDefault(p => p.ProjectUId == i.ProjectUId);
                    vms.Add(new BacklogItemViewModel(backlogService, executor, aggregator, authorizator, project, i, SFConfig));
                }                
                ticketsSource.Source = vms;
                OnPropertyChanged("Tickets");

                IsLoadingData = false;
            });
        }

        private void LoadProjects(Action after) {
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Project>>(() => {
                return projectsService.GetProjects(null, null, "HELPDESK_PROJECTS", OnlyMineProjects?authorizator.SignedMemberProfile.MemberUId:null);
            },
            prjs => {
                Projects = prjs;
                after.Invoke();
            });
        }

    
      

        private System.Windows.Data.CollectionViewSource ticketsSource;
                
        public ICollectionView Tickets {
            get {
                return ticketsSource.View;
            }        
        }
                
        public string ImageUrl {
            get { return "pack://application:,,,/ScrumFactory.HelpDeskPlugin;component/Images/help_desk.png"; }
        }

        public int PanelDisplayOrder {
            get { return 150; }
        }

        public string PanelName {
            get { return Properties.Resources.Help_desk; }
        }



        public Composition.ViewModel.PanelPlacements PanelPlacement {
            get {
                if (SFConfig.GetBoolValue("TicketProjectsEnabled"))
                    return PanelPlacements.Normal;
                return PanelPlacements.Hidden;
            }
            set {
                IConfigValue config = SFConfig.Get("TicketProjectsEnabled");
                if (config == null)
                    return;
                if(value!=PanelPlacements.Hidden)
                    config.Value = true;
                else
                    config.Value = false;
            }
        }

        

        [Import(typeof(HelpDesk))]
        public Composition.View.IView View {
            get;
            set;
        }


        public ICommand OnLoadCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        

        public ICommand AddJobCommand { get; set; }

  


        public ICommand ShowItemCommand { get; set; }
        
    }
}
