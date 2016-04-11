using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Services;
using System.Linq;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;
using System.Windows.Data;
using System.Windows.Input;
using ScrumFactory.Windows.Helpers.Extensions;

namespace ScrumFactory.Team.ViewModel {

    [Export]
    [Export(typeof(ITopMenuViewModel))]   
    public class MembersListViewModel : BasePanelViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        private string clientName;
      
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private IDialogService dialogs;
        private IProjectsService projectService;
        private ITeamService teamService;
        private IAuthorizationService authorizator;
        
        private System.Windows.Data.CollectionViewSource membersViewSource;

        private DelayAction delayFilter;

        [ImportingConstructor]
        public MembersListViewModel(
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IAuthorizationService authorizator,
            [Import] IDialogService dialogService,
            [Import] ITeamService teamService,
            [Import] IProjectsService projectService) {

                this.aggregator = eventAggregator;
                this.executor = backgroundExecutor;
                this.authorizator = authorizator;
                this.dialogs = dialogService;

                this.teamService = teamService;
                this.projectService = projectService;

                membersViewSource = new System.Windows.Data.CollectionViewSource();
                membersViewSource.SortDescriptions.Add(new SortDescription("MemberProfile.FullName", ListSortDirection.Ascending));

                membersViewSource.Filter += new FilterEventHandler(membersViewSource_Filter);
                delayFilter = new DelayAction(500, new DelayAction.ActionDelegate(() => { if (FilteredMembers != null) FilteredMembers.Refresh(); }));            

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.ShowMemberDetail, 
                    member => {
                        dialogs.SelectTopMenu(this);
                        ShowDetail(member);
                    });

                OnLoadCommand = new DelegateCommand(() => { if (NeedRefresh) LoadMembers(); });
                RefreshCommand = new DelegateCommand(LoadMembers);
                ShowDetailWindowCommand = new DelegateCommand<MemberViewModel>(ShowDetail);

                AddNewMemberCommand = new DelegateCommand(CanAddMember, AddMember);

                OnlyActiveMembers = true;

                NeedRefresh = true;
       
        }

        public bool CanAddMember() {            
            if (authorizator == null || authorizator.SignedMemberProfile == null)
                return false;
            return authorizator.SignedMemberProfile.IsFactoryOwner;            
        }

        [Import]
        private IServerUrl ServerUrl { get; set; }

        [Import]
        private MemberDetailViewModel MemberDetailViewModel { get; set; }

        [Import]
        private MemberViewModel NewMemberViewModel { get; set; }

        private int searchMemberAvailability;
        public int SearchMemberAvailability {
            get {
                return searchMemberAvailability;
            }
            set {
                searchMemberAvailability = value;
                OnPropertyChanged("SearchMemberAvailability");
                LoadMembers();
            }
        }

        private void ShowDetail(MemberViewModel member) {            
            MemberDetailViewModel.Show(this, member.MemberProfile);
        }

        private void ShowDetail(MemberProfile member) {
            MemberDetailViewModel.Show(this, member);
        }

        private void AddMember() {
            
        }

        private void LoadMembers() {

            if (authorizator == null || authorizator.SignedMemberProfile == null)
                return;

            IsLoadingData = true;
            
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => {
                    return teamService.GetMembers(null, SearchMemberAvailability, ClientName, OnlyActiveMembers, null, 0);
                }, OnMembersLoaded);
        }

        private void OnMembersLoaded(ICollection<MemberProfile> members) {

            NeedRefresh = false;

            ICollection<MemberViewModel> oldMembers = membersViewSource.Source as ICollection<MemberViewModel>;
            if (oldMembers != null)
                oldMembers.ClearAndDispose();

            List<MemberViewModel> newMembers = new List<MemberViewModel>();
            foreach (MemberProfile m in members) 
                newMembers.Add(new MemberViewModel(m, ServerUrl, authorizator));
            
            membersViewSource.Source = newMembers;

            IsLoadingData = false;
            OnPropertyChanged("FilteredMembers");
        }

        private bool onlyActiveMembers;
        public bool OnlyActiveMembers {
            get {
                return onlyActiveMembers;
            }
            set {
                onlyActiveMembers = value;
                OnPropertyChanged("OnlyActiveMembers");
                LoadMembers();
            }
        }

        public ICollectionView FilteredMembers {
            get {
                return membersViewSource.View;
            }
        }

        public string filter;
        public string Filter {
            get {
                return filter;
            }
            set {
                filter = value;
                OnPropertyChanged("Filter");
                delayFilter.StartAction();
            }
        }

        void membersViewSource_Filter(object sender, FilterEventArgs e) {
            MemberViewModel member = e.Item as MemberViewModel;
            if (member == null) {
                e.Accepted = false;
                return;
            }
            e.Accepted = member.FilterTest(Filter);
        }

        private void OnLoad() {            
            if(membersViewSource.Source==null)
                LoadMembers();
        }

        private void OnSignedMemberChanged(MemberProfile member) {
            if (member == null) {
                membersViewSource.Source = null;
                ClientName = string.Empty;
                return;
            }

            ((DelegateCommand)AddNewMemberCommand).NotifyCanExecuteChanged();

            ClientName = member.CompanyName;
        }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>The name of the client.</value>
        public string ClientName {
            get {
                return clientName;
            }
            set {
                clientName = value;
                OnPropertyChanged("ClientName");                
            }
        }

       

        public string PanelName {
            get { return Properties.Resources.People; }
        }

        public int PanelDisplayOrder {
            get { return 200; }
        }

        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Normal; }
        }

        public string ImageUrl {
            get { return "\\Images\\Toolbar\\TeamMember.png"; }
        }

        [Import(typeof(MembersList))]
        public IView View { get; set; }


        public ICommand OnLoadCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ShowDetailWindowCommand { get; set; }

        public ICommand AddNewMemberCommand { get; set; }
    }
}
