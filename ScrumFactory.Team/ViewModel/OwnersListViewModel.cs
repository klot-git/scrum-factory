using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers.Extensions;

namespace ScrumFactory.Team.ViewModel {
    
    [Export]
    [Export(typeof(ITopMenuViewModel))]
    public class OwnersListViewModel : BasePanelViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        private ITeamService teamServices;
        private IBackgroundExecutor executor;
        private IAuthorizationService authorizator;

        private ICollection<MemberViewModel> owners;

        private ICollection<MemberViewModel> members;

        private IDialogService dialogs;

        [ImportingConstructor]
        public OwnersListViewModel(
            [Import]IEventAggregator eventAggregator,            
            [Import]ITeamService teamServices,
            [Import]IBackgroundExecutor backgroundExecutor,
            [Import] IDialogService dialogs,
            [Import]IAuthorizationService authorizationService) {

            this.aggregator = eventAggregator;

            this.teamServices = teamServices;
            this.executor = backgroundExecutor;
            this.authorizator = authorizationService;
            this.dialogs = dialogs;

            aggregator.Subscribe(ScrumFactoryEvent.ShowOwnersList, Show);

            CloseWindowCommand = new DelegateCommand(CloseWindow);
            OnLoadCommand = new DelegateCommand(LoadOwners);

            ChangeFactoryOwnerCommand = new DelegateCommand<string>(ChangeFactoryOwner);
            ChangeCanSeeProposalsCommand = new DelegateCommand<string>(ChangeCanSeeProposals);

            AddNewMemberCommand = new DelegateCommand(AddNewMember);

            RefreshMemberFilter = LoadMembers;
        }

        public void LoadMembers(string filter) {
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                 () => { return teamServices.GetMembers(filter, 0, null, true, null, 9); }, OnMembersLoaded);

        }

        private void OnMembersLoaded(ICollection<MemberProfile> members) {
            ObservableCollection<MemberViewModel> membersVM = new ObservableCollection<MemberViewModel>();
            if (members != null)
                foreach (MemberProfile m in members)
                    membersVM.Add(new ViewModel.MemberViewModel(m, ServerUrl, authorizator));
            if (Members != null)
                Members.ClearAndDispose();
            Members = membersVM;
        }

        private void Show() {
            dialogs.SelectTopMenu(this);
        }

        private void CloseWindow() {
            dialogs.GoBackSelectedTopMenu();
        }

        private void LoadOwners() {
            IsLoadingData = true;
            Owners.ClearAndDispose();
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => { return teamServices.GetOwnersMembers(); }, 
                owners => {
                    IsLoadingData = false;
                    ObservableCollection<MemberViewModel> temp = new ObservableCollection<MemberViewModel>();
                    foreach (MemberProfile m in owners.OrderBy(m => m.FullName).ToList())
                        temp.Add(new MemberViewModel(m, ServerUrl, authorizator));
                    Owners = temp;
                });
            
        }

        private void ChangeFactoryOwner(string memberUId) {
            MemberViewModel memberVM = Owners.SingleOrDefault(m => m.MemberProfile.MemberUId == memberUId);
            if (memberVM == null)
                return;
            executor.StartBackgroundTask(
                () => { teamServices.ChangeMemberIsFactoryOwner(memberUId, memberVM.MemberProfile.IsFactoryOwner); },
                () => { });
        }

        private void ChangeCanSeeProposals(string memberUId) {
            MemberViewModel memberVM = Owners.SingleOrDefault(m => m.MemberProfile.MemberUId == memberUId);
            if (memberVM == null)
                return;
            executor.StartBackgroundTask(
                () => { teamServices.ChangeMemberCanSeeProposals(memberUId, memberVM.MemberProfile.CanSeeProposalValues); },
                () => { });
        }

        private void AddNewMember() {
            if (NewMember == null)
                return;
            Owners.Add(NewMember);
            NewMember = null;
        }

        private MemberViewModel newMember = null;
        public MemberViewModel NewMember {
            get {
                return newMember;
            }
            set {
                newMember = value;
                OnPropertyChanged("NewMember");
                ((DelegateCommand)AddNewMemberCommand).NotifyCanExecuteChanged();
            }
        }

        public ICollection<MemberViewModel> Owners {
            get {
                return owners;
            }
            set {
                owners = value;
                OnPropertyChanged("Owners");
            }
        }

        public ICollection<MemberViewModel> Members {
            get {
                return members;
            }
            set {
                members = value;
                OnPropertyChanged("Members");
            }
        }

        public System.Action<string> RefreshMemberFilter { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl ServerUrl { get; set; }

        [Import(typeof(OwnersList))]
        public IView View { get; set; }

        public ICommand CloseWindowCommand { get; set; }
        public ICommand OnLoadCommand { get; set; }
        public ICommand ChangeFactoryOwnerCommand { get; set; }
        public ICommand ChangeCanSeeProposalsCommand { get; set; }

        public ICommand AddNewMemberCommand { get; set; }

        public string PanelName {
            get { return Properties.Resources.Factory_owners; }
        }

        public int PanelDisplayOrder {
            get { return int.MaxValue; }
        }

        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Hidden; }
        }

        public string ImageUrl {
            get { return null; }
        }
    }
}
