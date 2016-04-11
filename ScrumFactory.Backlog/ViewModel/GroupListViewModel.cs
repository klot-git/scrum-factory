using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel;
using ScrumFactory;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers.Extensions;


namespace ScrumFactory.Backlog.ViewModel {


    [Export]
    public class GroupListViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IBacklogService backlogService;
        private IAuthorizationService authorizator;

        private Project project;

        private ICollection<BacklogItemGroup> groups;

        private BacklogItemGroup selectedGroup;
        
        [ImportingConstructor]
        public GroupListViewModel(
            [Import] IBackgroundExecutor executor,
            [Import] IEventAggregator aggregator,
            [Import] IBacklogService backlogService,
            [Import] IAuthorizationService authorizator) {

                this.executor = executor;
                this.aggregator = aggregator;
                this.backlogService = backlogService;
                this.authorizator = authorizator;

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);

                AddGroupCommand = new DelegateCommand(CanEditGroups, AddGroup);
                UpdateGroupCommand = new DelegateCommand<BacklogItemGroup>(CanEditGroups, UpdateGroup);

        }

        private bool CanEditGroups() {
            if (project == null || authorizator==null || authorizator.SignedMemberProfile==null)
                return false;
            return project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }
        
        private void OnViewProjectDetails(Project project) {
            this.project = project;
            NeedRefresh = true;
            ((DelegateCommand)AddGroupCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<BacklogItemGroup>)UpdateGroupCommand).NotifyCanExecuteChanged();
            // DO NOT CLEAR GROUPS HERE BECAUSE, COMBOBOX ITEMSSOURCE MAY BE BINDED 
            // TO IT, AND IT MAY NULL THE GROUPID
            //Groups = null;            
        }

        public void LoadGroups() {
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<BacklogItemGroup>>(
                () => {
                    return backlogService.GetBacklogItemGroups(project.ProjectUId);
                },
                groups => {
                    if (Groups != null)
                        Groups.ClearAndDispose();
                    
                    Groups = new ObservableCollection<BacklogItemGroup>(groups.OrderBy(g => g.DefaultGroup).ThenBy(g => g.GroupName));
                    aggregator.Publish<ICollection<BacklogItemGroup>>(ScrumFactoryEvent.BacklogItemGroupsLoaded, groups);
                    NeedRefresh = false;
                    IsLoadingData = false;
                });
        }

        private void UpdateGroup(BacklogItemGroup group) {
            if (!CanEditGroups())
                return;
            executor.StartBackgroundTask(
                () => { backlogService.UpdateBacklogItemGroup(group.ProjectUId, group); },
                () => { });
        }

        private void AddGroup() {
            if (!CanEditGroups())
                return;
            BacklogItemGroup newGroup = new BacklogItemGroup() { GroupUId = Guid.NewGuid().ToString(), ProjectUId = project.ProjectUId,  GroupName = Properties.Resources.New_struct, GroupColor = "Transparent", DefaultGroup = 1 };
            executor.StartBackgroundTask(
                () => { backlogService.UpdateBacklogItemGroup(newGroup.ProjectUId, newGroup); },
                () => { Groups.Add(newGroup); });
        }

        public BacklogItemGroup PlanGroup {
            get {
                LoadGroupsIfNotLoaded();
                BacklogItemGroup group = Groups.Where(g => g.DefaultGroup == (short)DefaultItemGroups.PLAN_GROUP).FirstOrDefault();
                if (group == null)
                    group = CreateDefaultGroup(DefaultItemGroups.PLAN_GROUP, Properties.Resources.PLAN_REVIEW_GROUP);
                return group;
            }
        }

        public void LoadGroupsIfNotLoadedAsync() {
            if (NeedRefresh)
                LoadGroups();
        }

        public void LoadGroupsIfNotLoaded() {
            if (NeedRefresh) {
                ICollection<BacklogItemGroup> gs = backlogService.GetBacklogItemGroups(project.ProjectUId);
                if (Groups != null)
                    Groups.ClearAndDispose();
                Groups = new ObservableCollection<BacklogItemGroup>(gs);
                aggregator.Publish<ICollection<BacklogItemGroup>>(ScrumFactoryEvent.BacklogItemGroupsLoaded, gs);
                NeedRefresh = false;
            }
        }

        public BacklogItemGroup DeliveryGroup {
            get {
                LoadGroupsIfNotLoaded();
                BacklogItemGroup group = Groups.Where(g => g.DefaultGroup == (short)DefaultItemGroups.DELIVERY_GROUP).FirstOrDefault();
                if (group == null)
                    group = CreateDefaultGroup(DefaultItemGroups.DELIVERY_GROUP, Properties.Resources.DELIVERY_GROUP);
                return group;
            }
        }

        public BacklogItemGroup DevelopmentGroup {
            get {
                BacklogItemGroup group = Groups.Where(g => g.DefaultGroup == (short)DefaultItemGroups.DEV_GROUP).FirstOrDefault();
                if (group == null)
                    group = CreateDefaultGroup(DefaultItemGroups.DEV_GROUP, Properties.Resources.DEV_GROUP);
                return group;
            }
        }
                
        private BacklogItemGroup CreateDefaultGroup(DefaultItemGroups defaultGroup, string name) {            
            BacklogItemGroup group =
                new BacklogItemGroup() {
                GroupColor = "WhiteSmoke",
                GroupName = name,
                GroupUId = Guid.NewGuid().ToString(),
                ProjectUId = this.project.ProjectUId,
                DefaultGroup = (short)defaultGroup
            };
            if (defaultGroup == DefaultItemGroups.PLAN_GROUP)
                group.GroupColor = "Khaki";
            if (defaultGroup == DefaultItemGroups.DELIVERY_GROUP)
                group.GroupColor = "Crimson";
            
            Groups.Add(group);
            return group;
        }
        public ICollection<BacklogItemGroup> Groups {
            get {
                return groups;
            }
            set {
                groups = value;
                OnPropertyChanged("Groups");
            }
        }

        public BacklogItemGroup SelectedGroup {
            get {
                return selectedGroup;
            }
            set {
                selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }

        public ICommand AddGroupCommand { get; set; }
        public ICommand UpdateGroupCommand { get; set; }

        #region IViewModel Members

        [Import(typeof(GroupList))]
        public Composition.View.IView View { get; set; }

        #endregion
    }


}
