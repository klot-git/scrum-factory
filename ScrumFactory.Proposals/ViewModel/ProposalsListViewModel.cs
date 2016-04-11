using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using ScrumFactory.Windows.Helpers.Extensions;
using System.Windows.Input;
using System.Linq;

namespace ScrumFactory.Proposals.ViewModel {

    [Export]
    [Export(typeof(IProjectTabViewModel))]
    public class ProposalsListViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {

        private IDialogService dialogs;
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;


        private IAuthorizationService authorizator;
        private IProposalsService proposalsService;
        private IBacklogService backlogService;

        private ICollection<Proposal> proposals;

        private ICollection<BacklogItemGroup> groups;
        private ICollection<BacklogItem> projectItems;
        private RoleHourCost[] costs;


        private Project project;

        private bool isAddingProposal = false;

        [ImportingConstructor]
        public ProposalsListViewModel(
            [Import]IBackgroundExecutor executor,
            [Import]IEventAggregator eventAggregator,            
            [Import]IDialogService dialogs,
            [Import]IAuthorizationService authorizator,
            [Import]IBacklogService backlogService,
            [Import]IProposalsService proposalsService) {

                this.executor = executor;
                this.dialogs = dialogs;
                this.aggregator = eventAggregator;

                this.authorizator = authorizator;
                this.proposalsService = proposalsService;
                this.backlogService = backlogService;

                this.aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
                
                this.aggregator.Subscribe(ScrumFactoryEvent.RoleHourCostsChanged, LoadProposals);
                this.aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

                this.aggregator.Subscribe(ScrumFactoryEvent.ApplicationWhentBackground, () => { ShowValues = false; });

                OnLoadCommand = new DelegateCommand(CanSeeProposals, () => {
                    ShowValues = false;
                    if (NeedRefresh) LoadProposals(); 
                });
                AddProposalCommand = new DelegateCommand(CanSeeProposals, AddProposal);
                ShowDetailCommand = new DelegateCommand<Proposal>(CanSeeProposals, ShowDetail);
                ShowHourCostsCommand = new DelegateCommand(CanSeeProposals, ShowHourCosts);
        }

        

        [Import]
        private ProposalViewModel ProposalViewModel { get; set; }

        [Import]
        private HourCostsViewModel HourCostsViewModel { get; set; }

        private bool IsAddingProposal {
            get {
                return isAddingProposal;
            }
            set {
                isAddingProposal = value;
                IsLoadingData = value;
            }
        }

        private bool CanSeeProposals() {            
            if (Project==null || authorizator==null || authorizator.SignedMemberProfile == null)
                return false;
            return 
                (authorizator.SignedMemberProfile.IsFactoryOwner ||
                Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                && authorizator.SignedMemberProfile.CanSeeProposalValues;            
        }

        private void OnSignedMemberChanged(MemberProfile m) {
            OnPropertyChanged("CanSeeProposals");
        }

        public Project Project {
            get {
                return project;
            }
            set {
                project = value;

                IsVisible = (project != null);

                costs.ClearAndDispose();
                projectItems.ClearAndDispose();
                groups.ClearAndDispose();
                OnPropertyChanged("Project");
            }
        }

        #region Commands Actions


        private void ShowHourCosts() {
            if (!CanSeeProposals())
                return;
            LoadCosts();
            HourCostsViewModel.HourCosts = costs;
            HourCostsViewModel.Show(Project);
        }

        private void LoadCosts() {
            //if (costs != null)
            //    return;
            costs = proposalsService.GetHourCosts(Project.ProjectUId);
            foreach (RoleHourCost cost in costs)
                cost.Role = Project.Roles.SingleOrDefault(r => r.RoleUId == cost.RoleUId);
        }

        private void ShowDetail(Proposal proposal) {

            if (!CanSeeProposals())
                return;

            
            executor.StartBackgroundTask<Proposal>(
                () => {
                    LoadCosts();
                    LoadProjectItems();
                    LoadProjectItemsGroups();

                    return proposalsService.GetProjectProposal(proposal.ProjectUId, proposal.ProposalUId);
                },
                p => {                    
                    ProposalViewModel.HourCosts = costs;
                    ProposalViewModel.Proposal = p;                    
                    ProposalViewModel.SetProjectItems(projectItems, groups);                    
                    ProposalViewModel.Show();
                });

            
        }

        private void AddProposal() {

            if (IsAddingProposal)
                return;

            if (!CanSeeProposals())
                return;

            if (Project.Sprints == null)
                return;

            IsAddingProposal = true;

            Proposal newProposal = new Proposal();
            newProposal.ProjectUId = Project.ProjectUId;
            newProposal.ProposalUId = Guid.NewGuid().ToString();
            newProposal.Description = Project.Description;
            newProposal.ProposalName = Properties.Resources.Proposal;
            newProposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_WAITING;
            newProposal.CreateDate = DateTime.Now;
            newProposal.CurrencySymbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
            newProposal.UseCalcPrice = true;

          
            newProposal.EstimatedStartDate = Project.FirstSprint.StartDate;
            newProposal.EstimatedEndDate = Project.LastSprint.EndDate;

            
            executor.StartBackgroundTask<Proposal>(
                () => {
                    if (projectItems == null)
                        LoadProjectItems();
                    return proposalsService.AddProposal(newProposal.ProjectUId, newProposal); },
                p => {
                    p.SetBacklogItems(projectItems);                    
                    Proposals.Add(p);
                    IsAddingProposal = false;                    
                    ShowDetail(p);
                });

        }

        
        private void LoadProposals() {

            if (!CanSeeProposals()) {
                dialogs.ShowAlertMessage(Properties.Resources.Can_not_see_proposal_title, Properties.Resources.Can_not_see_proposal_message, null);
                return;
            }
            
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Proposal>>(
                () => { return proposalsService.GetProjectProposals(Project.ProjectUId); },
                ps => {
                    ShowDetailCommand = null;
                    if(Proposals!=null)
                        Proposals.Clear();
                    Proposals = new ObservableCollection<Proposal>(ps.OrderBy(p => p.ProposalStatus).ThenBy(p =>p.ApprovalDate));
                    ShowDetailCommand = new DelegateCommand<Proposal>(CanSeeProposals, ShowDetail);
                    IsLoadingData = false;
                });

        }

        private void LoadProjectItems() {            
            projectItems = backlogService.GetCurrentBacklog(Project.ProjectUId, (short) BacklogFiltersMode.ALL);
        }

        
        private void LoadProjectItemsGroups() {
            //if (groups != null)
            //    return;
            groups = backlogService.GetBacklogItemGroups(Project.ProjectUId); ;
            foreach (BacklogItem i in projectItems)
                i.Group = groups.SingleOrDefault(u => u.GroupUId == i.GroupUId);
        }

        #endregion

        private void AskForRefresh() {
            if (View != null && View.IsVisible) {
                LoadProposals();
            }
            else
                NeedRefresh = true;
        }

        private void OnViewProjectDetails(Project project) {
            Project = project;
            if (Proposals != null)
                Proposals.Clear();

            if (AddProposalCommand != null)
                ((DelegateCommand)AddProposalCommand).NotifyCanExecuteChanged();

            if (ShowHourCostsCommand != null)
                ((DelegateCommand)ShowHourCostsCommand).NotifyCanExecuteChanged();

            if (ShowDetailCommand != null)
                ((DelegateCommand<Proposal>)ShowDetailCommand).NotifyCanExecuteChanged();



            AskForRefresh();
        }


        [Import(typeof(ProposalsList))]
        public IView View { get; set; }

        private bool showValues;
        public bool ShowValues {
            get {
                return showValues;
            }
            set {
                showValues = value;
                OnPropertyChanged("ShowValues");
            }
        }

        public string PanelName {
            get { return Properties.Resources.Proposals; }
        }

        public int PanelDisplayOrder {
            get { return 700; }
        }

        public ICollection<Proposal> Proposals {
            get {
                return proposals;
            }
            set {
                proposals = value;
                OnPropertyChanged("Proposals");
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

        public ICommand OnLoadCommand { get; set; }
        public ICommand AddProposalCommand { get; set; }
        public ICommand ShowDetailCommand { get; set; }
        public ICommand ShowHourCostsCommand { get; set; }
    }
}
