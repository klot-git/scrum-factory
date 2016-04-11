using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using System.Windows.Input;
using System.Linq;
using ScrumFactory.Extensions;

namespace ScrumFactory.Proposals.ViewModel {

    [Export]    
    public class ProposalViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private Proposal oldProposal;
        private Proposal proposal;

        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IDialogService dialogs;

        private IProposalsService proposalsService;
        private IProjectsService projectsService;

        private decimal itemsPrice;
        private decimal fixedCostPrice;

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }

        [Import]
        private IProjectConstraintsService constraintsService { get; set; }

        [Import]
        public IArtifactsListViewModel ArtifactListViewModel { get; set; }

        [Import]
        public ICalendarService calendar { get; set; }
       

        [ImportMany]
        private IEnumerable<IPluginCommand> allPluginMenuItems { get; set; }

        public IEnumerable<IPluginCommand> PluginMenuItems {
            get {
                if (allPluginMenuItems == null)
                    return null;
                return allPluginMenuItems.Where(m => m.ContainerViewModelClassName.Equals(this.GetType().ToString())).OrderBy(m => m.DisplayOrder).ToList();
            }
        }

        public bool UseDifferentCurrency {
            get {
                if (Proposal == null || Proposal.CurrencySymbol==null)
                    return false;
                return Proposal.CurrencySymbol.ToLower() != DefaultCurrencySymbol.ToLower();
            }
            set {
                if (!value)
                    Proposal.CurrencySymbol = DefaultCurrencySymbol;                    
                else
                    Proposal.CurrencySymbol = String.Empty;
                OnPropertyChanged("UseDifferentCurrency");
            }
        }

        public decimal CurrencyRate {
            get {
                if (Proposal == null || !Proposal.CurrencyRate.HasValue)
                    return 1;
                return Proposal.CurrencyRate.Value;
            }
            set {
                Proposal.CurrencyRate = value;
                OnPropertyChanged("CurrencyRate");
            }
        }

        public string DefaultCurrencySymbol {
            get {
                return System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
            }
        }


        private ICollection<ProposalClause> clauses;

        public ICollection<ProposalClause> Clauses {
            get {
                return clauses;
            }
            set {
                clauses = value;
                OnPropertyChanged("Clauses");
            }
        }

        private ICollection<ProposalFixedCost> fixedCosts;

        public ICollection<ProposalFixedCost> FixedCosts {
            get {
                return fixedCosts;
            }
            set {
                fixedCosts = value;
                OnPropertyChanged("FixedCosts");
            }
        }

        private bool isFirstTabSelected;
        public bool IsFirstTabSelected {
            get {
                return isFirstTabSelected;
            }
            set {
                isFirstTabSelected = value;
                OnPropertyChanged("IsFirstTabSelected");
            }
        }

        private System.Windows.Data.CollectionViewSource itemsViewSource;

        private ICollection<BacklogItemGroup> groups;

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }


        [Import]
        private ReportHelper.Report ReportService { get; set; }

        private Project project;
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
                OnPropertyChanged("Project");
            }
        }
        
        [ImportingConstructor]
        public ProposalViewModel(
            [Import]IBackgroundExecutor executor,
            [Import]IEventAggregator aggregator,
            [Import]IDialogService dialogs,
            [Import]IProposalsService proposalsService,
            [Import]IProjectsService projectsService) {

                this.executor = executor;
                this.aggregator = aggregator;
                this.dialogs = dialogs;
                this.proposalsService = proposalsService;
            this.projectsService = projectsService;

                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
                

                itemsViewSource = new System.Windows.Data.CollectionViewSource();
                itemsViewSource.SortDescriptions.Add(new SortDescription("ItemGroup.DefaultGroup", ListSortDirection.Ascending));
                itemsViewSource.SortDescriptions.Add(new SortDescription("ItemGroup.GroupName", ListSortDirection.Ascending));
                itemsViewSource.SortDescriptions.Add(new SortDescription("Item.Name", ListSortDirection.Ascending));

                CloseWindowCommand = new DelegateCommand(CloseWindow);
                RefreshProposalPriceCommand = new DelegateCommand(CanEdit, RefreshProposalPrice);

                AddClauseCommand = new DelegateCommand(CanEdit, AddClause);
                DeleteClauseCommand = new DelegateCommand<ProposalClause>(CanEdit, DeleteClause);
                MoveClauseDownCommand = new DelegateCommand<ProposalClause>(CanEdit, MoveClauseDown);
                MoveClauseUpCommand = new DelegateCommand<ProposalClause>(CanEdit, MoveClauseUp);

                AddCostCommand = new DelegateCommand(CanEdit, AddFixedCost);
                DeleteCostCommand = new DelegateCommand<ProposalFixedCost>(CanEdit, DeleteCost);
            

                ApproveCommand = new DelegateCommand(CanEdit, Approve);
                RejectCommand = new DelegateCommand<short>(CanEdit, Reject);

                ShowProposalReportCommand = new DelegateCommand(ShowProposalReport);

                SyncProposalDatesCommand = new DelegateCommand(SyncProposalDates);
        }

        private void SyncProposalDates() {
            if (Project == null || Project.Sprints == null || Project.Sprints.Count == 0)
                return;
            EstimatedStartDate  = Project.FirstSprint.StartDate.Date;
            EstimatedEndDate = Project.LastSprint.EndDate.Date;
        }

        public DateTime EstimatedStartDate {
            get {
                if (Proposal == null)
                    return DateTime.Today;
                return Proposal.EstimatedStartDate;
            }
            set {                
                Proposal.EstimatedStartDate = value;
                OnPropertyChanged("EstimatedStartDate");
            }
        }

        public DateTime EstimatedEndDate {
            get {
                if (Proposal == null)
                    return DateTime.Today;
                return Proposal.EstimatedEndDate;
            }
            set {
                Proposal.EstimatedEndDate = value;
                OnPropertyChanged("EstimatedEndDate");
            }
        }

        private string[] templates;
        public string[] Templates {
            get {
                return templates;
            }
            set {
                templates = value;
                OnPropertyChanged("Templates");
            }
        }
            
         private void LoadTemplates() {
            if (Templates != null)
                return;
            executor.StartBackgroundTask<string[]>(
                () => { return proposalsService.GetProposalTemplates(); },
                t => { Templates = t.OrderBy(s => s).ToArray(); });

        }

        private void AddFixedCost() {
            ProposalFixedCost cost = new ProposalFixedCost();
            cost.ProposalFixedCostUId = Guid.NewGuid().ToString();
            cost.RepassToClient = true;
            cost.ProposalUId = Proposal.ProposalUId;
            cost.CostDescription = Properties.Resources.New_cost;            
            FixedCosts.Add(cost);
            Proposal.FixedCosts.Add(cost);
            FixedCostPrice = Proposal.CalcFixedCosts();
        }

        private void DeleteCost(ProposalFixedCost cost) {
            FixedCosts.Remove(cost);
            Proposal.FixedCosts.Remove(cost);
        }

        private void AddClause() {
            ProposalClause clause = new ProposalClause();
            clause.ProposalUId = Proposal.ProposalUId;
            clause.ClauseName = Properties.Resources.New_clause;
            if (Clauses.Count == 0)
                clause.ClauseOrder = 1;
            else
                clause.ClauseOrder = Clauses.Max(c => c.ClauseOrder) + 1;
            Clauses.Add(clause);
            Proposal.Clauses.Add(clause);
        }

        private void DeleteClause(ProposalClause clause) {
            Clauses.Remove(clause);
            Proposal.Clauses.Remove(clause);
        }

        private void MoveClauseDown(ProposalClause clause) {
            // gets the clause Idx
            ObservableCollection<ProposalClause> orderedClauses = Clauses as ObservableCollection<ProposalClause>;
            int clauseIdx = orderedClauses.IndexOf(clause);
            if (clauseIdx + 1 == Clauses.Count)
                return;
            
            // get next clause, and moves up its order
            ProposalClause nextClause = orderedClauses.ElementAt(clauseIdx + 1);
            nextClause.ClauseOrder--;
            clause.ClauseOrder++;

            // change its order at the list
            orderedClauses.Remove(clause);
            orderedClauses.Insert(clauseIdx + 1, clause);
        }

        private void MoveClauseUp(ProposalClause clause) {
            // gets the clause Idx
            ObservableCollection<ProposalClause> orderedClauses = Clauses as ObservableCollection<ProposalClause>;
            int clauseIdx = orderedClauses.IndexOf(clause);
            if (clauseIdx == 0)
                return;

            // get next clause, and moves up its order
            ProposalClause previousClause = orderedClauses.ElementAt(clauseIdx - 1);
            previousClause.ClauseOrder++;
            clause.ClauseOrder--;

            // change its order at the list
            orderedClauses.Remove(clause);
            orderedClauses.Insert(clauseIdx - 1, clause);
        }


        private void OnViewProjectDetails(Project project) {
            this.Project = project;
        }

        public RoleHourCost[] HourCosts { get; set; }

        public bool IsProposalWaiting {
            get {
                if (Proposal == null)
                    return false;
                return Proposal.ProposalStatus == (short)ProposalStatus.PROPOSAL_WAITING;
            }
        }

        private bool CanEdit() {
            if (IsLoadingData)
                return false;
            return IsProposalWaiting;
        }

        

        private void Approve() {

            if (IsLoadingData)
                return;

            System.Windows.MessageBoxResult r =
            dialogs.ShowMessageBox(Properties.Resources.Approve_proposal, Properties.Resources.Approve_proposal_tip, System.Windows.MessageBoxButton.YesNo, "/Images/Dialogs/approve.png");
            if (r == System.Windows.MessageBoxResult.No)
                return;

            IsLoadingData = true;

            ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();

            //string reportXAML = ReportService.CreateReportXAML(serverUrl.Url, CreateReportConfig());

            var items = itemsViewSource.Source as ICollection<ProposalItemViewModel>;

            executor.StartBackgroundTask(
                () => {
                    string reportXAML = ReportService.CreateReportXAML(serverUrl.Url, CreateReportConfig(items));
                    proposalsService.UpdateProposal(Proposal.ProjectUId, Proposal); 
                    proposalsService.ApproveProposal(Proposal.ProjectUId, Proposal.ProposalUId, reportXAML);
                    return projectsService.GetProject(proposal.ProjectUId);
                },
                p => {
                    Proposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_APPROVED;
                    RefreshPermissions();
                    aggregator.Publish<Project>(ScrumFactoryEvent.ProjectStatusChanged, p);
                    IsLoadingData = false;
                    ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
                    ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();
                    //CloseWindow(false);
                });
        }

        private void Reject(short reason) {

            if (IsLoadingData)
                return;


            System.Windows.MessageBoxResult r =
            dialogs.ShowMessageBox(Properties.Resources.Reject_proposal, Properties.Resources.Reject_proposal_tip, System.Windows.MessageBoxButton.YesNo, "/Images/Dialogs/reject.png");
            if (r == System.Windows.MessageBoxResult.No)
                return;


            IsLoadingData = true;

            ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();

            var items = itemsViewSource.Source as ICollection<ProposalItemViewModel>;
            

            executor.StartBackgroundTask(
                () => {
                    string reportXAML = ReportService.CreateReportXAML(serverUrl.Url, CreateReportConfig(items));
                    proposalsService.UpdateProposal(Proposal.ProjectUId, Proposal); 
                    proposalsService.RejectProposal(Proposal.ProjectUId, Proposal.ProposalUId, reason.ToString(), reportXAML);
                    return projectsService.GetProject(proposal.ProjectUId);
                },
                p => {
                    Proposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_REJECTED;
                    RefreshPermissions();
                    aggregator.Publish<Project>(ScrumFactoryEvent.ProjectStatusChanged, p);
                    IsLoadingData = false;
                    ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
                    ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();
                    //CloseWindow(false);
                });
        }

        private void RefreshPermissions() {
            OnPropertyChanged("IsProposalWaiting");
            ((DelegateCommand)AddClauseCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<ProposalClause>)DeleteClauseCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<ProposalClause>)MoveClauseDownCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<ProposalClause>)MoveClauseUpCommand).NotifyCanExecuteChanged();

            ((DelegateCommand)AddCostCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<ProposalFixedCost>)DeleteCostCommand).NotifyCanExecuteChanged();


            ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();

        }

        public void OnGroupsLoaded(ICollection<BacklogItemGroup> groups) {
            this.groups = groups;
        }

        private ReportHelper.ReportConfig CreateReportConfig(ICollection<ProposalItemViewModel> items) {

            ReportHelper.ReportConfig reportConfig = new ReportHelper.ReportConfig("ProposalReport", Proposal.TemplateName, Proposal.ProposalName);
            
            // if is an approved or rejected proposal, uses the xaml saved at the database 
            if (Proposal.ProposalStatus != (short)ProposalStatus.PROPOSAL_WAITING) {
                try {
                    ProposalDocument document = proposalsService.GetProposalDocument(Proposal.ProjectUId, Proposal.ProposalUId);
                    reportConfig.StaticXAMLReport = document.ProposalXAML;
                }
                catch (Exception) {
                    dialogs.ShowAlertMessage(Properties.Resources.Error_reading_proposal, Properties.Resources.An_error_occured_when_reading_this_proposal, null);
                    return null;
                }
                return reportConfig;
            }

            // add risks
            ICollection<Risk> risks = projectsService.GetProjectRisks(Project.ProjectUId);
            reportConfig.ReportObjects.Add(risks);

            // calcs the work days
            int dayCount = calendar.CalcWorkDayCount(Proposal.EstimatedStartDate, Proposal.EstimatedEndDate);
            reportConfig.ReportVars.Add("workDaysCount", dayCount.ToString());

            // constraints
            ICollection<ProjectConstraint> constraints = constraintsService.GetProjectConstraints(Project.ProjectUId);

            if (Proposal.CurrencyRate == null)
                Proposal.CurrencyRate = 1;

            reportConfig.ReportObjects.Add(Project);
            reportConfig.ReportObjects.Add(Proposal);
            reportConfig.ReportObjects.Add(HourCosts);            
            reportConfig.ReportObjects.Add(groups);
            reportConfig.ReportObjects.Add(allItems);
            reportConfig.ReportObjects.Add(constraints);

            // creates proposal items with price
            List<ProposalItemWithPrice> itemsWithValue = new List<ProposalItemWithPrice>();
            //ICollection<ProposalItemViewModel> items = itemsViewSource.Source as ICollection<ProposalItemViewModel>;
            foreach (ProposalItemViewModel item in items.Where(i => i.IsAtProposal == true))
                itemsWithValue.Add(new ProposalItemWithPrice(Proposal.ProposalUId, item.Item, item.ItemPrice));
            reportConfig.ReportObjects.Add(itemsWithValue);

            return reportConfig;
        }

        private void ShowProposalReport() {
            ReportHelper.ReportConfig report = CreateReportConfig(itemsViewSource.Source as ICollection<ProposalItemViewModel>);
            if (report == null)
                return;
            aggregator.Publish<ReportHelper.ReportConfig>(ScrumFactoryEvent.ShowReport, report);
        }

        private ICollection<BacklogItem> allItems;

        public void SetProjectItems(ICollection<BacklogItem> items, ICollection<BacklogItemGroup> groups) {

            this.allItems = items;
            this.groups = groups; 

            // sets the proposal item reference
            foreach (ProposalItem pItem in Proposal.Items)
                pItem.Item = items.SingleOrDefault(i => i.BacklogItemUId == pItem.BacklogItemUId);

            // create proposal items view models
            ICollection<ProposalItemViewModel> proposalItems = new List<ProposalItemViewModel>();
            foreach (BacklogItem item in items) 
                proposalItems.Add(new ProposalItemViewModel(this, item, groups.SingleOrDefault(g => g.GroupUId==item.GroupUId)));

            itemsViewSource.GroupDescriptions.Clear();
            itemsViewSource.Source = proposalItems;
            
            itemsViewSource.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("ItemGroup.GroupName"));

            Discount = Proposal.Discount;
            UseCalcPrice = Proposal.UseCalcPrice;
            
            // calcs proposal price
            if (Proposal.ProposalStatus == (short) ProposalStatus.PROPOSAL_WAITING) {
                ItemsPrice = Proposal.CalcItemsPrice(HourCosts);
                FixedCostPrice = Proposal.CalcFixedCosts();
            } else {
                TotalPrice = Proposal.TotalValue;
            }
            
                
            OnPropertyChanged("ProjectItems");
        }

        public ICollectionView ProjectItems {
            get {
                return itemsViewSource.View;
            }            
        }

        public decimal ItemsPrice {
            get {
                return itemsPrice;
            }
            set {
                itemsPrice = value;
                OnPropertyChanged("ItemsPrice");
                TotalPrice = Proposal.CalcTotalPrice(HourCosts);
            }
        }

        public decimal FixedCostPrice {
            get {
                return fixedCostPrice;
            }
            set {
                fixedCostPrice = value;
                OnPropertyChanged("FixedCostPrice");
                if (Proposal.ProposalStatus == (short)ProposalStatus.PROPOSAL_WAITING)
                    TotalPrice = Proposal.CalcTotalPrice(HourCosts);
            }
        }

        public decimal Discount {
            get {
                if (Proposal == null)
                    return 0;
                return Proposal.Discount;
            }
            set {
                Proposal.Discount = value;
                OnPropertyChanged("Discount");
                if(Proposal.ProposalStatus==(short)ProposalStatus.PROPOSAL_WAITING)
                    TotalPrice = Proposal.CalcTotalPrice(HourCosts);
            }
        }

        public decimal TotalPrice {
            get {
                if (Proposal == null)
                    return 0;
                return Proposal.TotalValue;
            }
            set {
                Proposal.TotalValue = value;     
                OnPropertyChanged("TotalPrice");                
            }
        }

        

        public bool UseCalcPrice {
            get {
                if (Proposal == null)
                    return false;
                return proposal.UseCalcPrice;
            }
            set {
                Proposal.UseCalcPrice = value;
                if (Proposal.UseCalcPrice)
                    Discount = Proposal.Discount;                
                OnPropertyChanged("UseCalcPrice");
            }
        }

        private void RefreshProposalPrice() {
            ItemsPrice = Proposal.CalcItemsPrice(HourCosts);
            FixedCostPrice = Proposal.CalcFixedCosts();
            ICollection<ProposalItemViewModel> items = itemsViewSource.Source as ICollection<ProposalItemViewModel>;
            foreach(ProposalItemViewModel pItem in items)
                pItem.RefreshUI();

        }

        private void SaveProposal(Action afterSave) {
            executor.StartBackgroundTask(
                () => { proposalsService.UpdateProposal(Proposal.ProjectUId, Proposal); },
                () => { 
                    if(afterSave!=null)
                    afterSave.Invoke(); 
                });
        }

   
        [Import(typeof(ProposalDetail))]
        public IView View { get; set; }

        public void Show() {
            IsFirstTabSelected = true;
            LoadTemplates();
            Show(projectContainer.Value);

            ArtifactListViewModel.ChangeContext(ArtifactContexts.PROPOSAL_ARTIFACT, Proposal.ProposalUId);
        }
        
        public string PanelName {
            get {
                if (Proposal == null || String.IsNullOrEmpty(Proposal.ProposalName))
                    return Properties.Resources.Proposal;
                if (Proposal.ProposalName.Length < 20)
                    return Proposal.ProposalName;
                else
                    return Proposal.ProposalName.Substring(0, 20) + "...";
            }
        }

        public Proposal Proposal {
            get {
                return proposal;
            }
            set {                
                proposal = value;
                oldProposal = proposal.Clone();

                if (proposal.Clauses == null)
                    proposal.Clauses = new List<ProposalClause>();
                Clauses = new ObservableCollection<ProposalClause>(proposal.Clauses.OrderBy(c => c.ClauseOrder));

                if (proposal.FixedCosts == null)
                    proposal.FixedCosts = new List<ProposalFixedCost>();
                FixedCosts = new ObservableCollection<ProposalFixedCost>(proposal.FixedCosts.OrderBy(c => c.CostDescription));


                OnPropertyChanged("Proposal");
                OnPropertyChanged("PanelName");
                OnPropertyChanged("IsProposalWaiting");
                OnPropertyChanged("EstimatedStartDate");
                OnPropertyChanged("EstimatedEndDate");
                OnPropertyChanged("UseDifferentCurrency");
                OnPropertyChanged("CurrencyRate");

                ((DelegateCommand)AddClauseCommand).NotifyCanExecuteChanged();
                ((DelegateCommand)ApproveCommand).NotifyCanExecuteChanged();
                ((DelegateCommand<short>)RejectCommand).NotifyCanExecuteChanged();
                
            }
        }

        private void CloseWindow() {
            CloseWindow(true);
        }

        /// <summary>
        /// Closes the window, invoke the OnCloseAction and publishes the CloseWindow event.
        /// </summary>
        private void CloseWindow(bool andSave) {

            // refreshs pricebefore saving
            if(Proposal.ProposalStatus==(short)ProposalStatus.PROPOSAL_WAITING)
                RefreshProposalPrice();

            // if proposal has changed save it
            if (Proposal.ProposalStatus == (short)ProposalStatus.PROPOSAL_WAITING && andSave && !oldProposal.IsTheSame(Proposal))
                SaveProposal(
                    () => {
                        // closes only after save, because otherwise the proposal list may load old values
                        Close();
                    });
            else
                Close();

                        
        }


        public ICommand ApproveCommand { get; set; }
        public ICommand RejectCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand RefreshProposalPriceCommand { get; set; }
        public ICommand ShowProposalReportCommand { get; set; }

        public ICommand SyncProposalDatesCommand { get; set; }
     
        public ICommand AddClauseCommand { get; set; }
        public ICommand DeleteClauseCommand { get; set; }
        public ICommand MoveClauseUpCommand { get; set; }
        public ICommand MoveClauseDownCommand { get; set; }

        public ICommand AddCostCommand { get; set; }
        public ICommand DeleteCostCommand { get; set; }

        
       
    }
}
