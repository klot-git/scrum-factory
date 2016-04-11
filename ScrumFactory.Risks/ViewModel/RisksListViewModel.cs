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

namespace ScrumFactory.Risks.ViewModel {

    [Export]
    [Export(typeof(IProjectTabViewModel))]
    public class RisksListViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {

        private IDialogService dialogs;
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;


        private IAuthorizationService authorizator;
        private IProjectsService projectsService;


        private Project project;
        private ICollection<Risk> risks;
        private ICollection<RiskViewModel> filteredRisks;
        private string newRiskDescription;

        private bool showMitigatedRisks = false;

        private bool userCanEdit;

        [Import]
        private RiskViewModel riskDetail { get; set; }

        [ImportingConstructor]
        public RisksListViewModel(
            [Import]IBackgroundExecutor executor,
            [Import]IEventAggregator eventAggregator,            
            [Import]IDialogService dialogs,
            [Import]IAuthorizationService authorizator,
            [Import]IProjectsService projectsService) {

                this.executor = executor;
                this.dialogs = dialogs;
                this.aggregator = eventAggregator;

                this.authorizator = authorizator;
                this.projectsService = projectsService;

                this.aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);
                this.aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

                OnLoadCommand = new DelegateCommand(() => { if (NeedRefresh) LoadRisks(); });
                AddRiskCommand = new DelegateCommand(CanAddRisk, AddRisk);
                ShowDetailCommand = new DelegateCommand<RiskViewModel>(ShowDetail);
                
        }

        public bool ShowMitigatedRisks {
            get {
                return showMitigatedRisks;
            }
            set {
                showMitigatedRisks = value;
                ApplyFilterRisk();
                OnPropertyChanged("ShowMitigatedRisks");
            }
        }

        public string NewRiskDescription {
            get {
                return newRiskDescription;
            }
            set {
                newRiskDescription = value;
                ((DelegateCommand)AddRiskCommand).NotifyCanExecuteChanged();
                OnPropertyChanged("NewRiskDescription");
            }
        }
                
        public bool UserCanEdit {
            get {
                return userCanEdit;
            }
            set {
                userCanEdit = value;
                OnPropertyChanged("UserCanEdit");
            }
        }

        #region Commands Actions

        private bool CanAddRisk() {
            if (Project == null)
                return false;
            if(authorizator.SignedMemberProfile==null)
                return false;
            return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }

        private void AddRisk() {
            Risk newRisk = new Risk();
            newRisk.RiskUId = Guid.NewGuid().ToString();
            newRisk.ProjectUId = project.ProjectUId;
            newRisk.CreateDate = DateTime.Now;
            newRisk.UpdatedAt = DateTime.Now;
            newRisk.IsPrivate = false;
            newRisk.RiskDescription = NewRiskDescription;
            newRisk.Impact = (short)RiskImpacts.LOW_IMPACT_RISK;
            newRisk.Probability = (short)RiskProbabilities.LOW_PROBABILITY_RISK;
            

            executor.StartBackgroundTask(
                () => { projectsService.AddRisk(newRisk.ProjectUId, newRisk);  },
                () => {
                    risks.Add(newRisk);
                    RiskViewModel riskVM = new RiskViewModel(executor, projectsService, newRisk);                    
                    FilteredRisks.Add(riskVM);
                    riskVM.NotifyAdded();
                    NewRiskDescription = null;
                });

        }

        private void AskForRefresh() {
            if (View != null && View.IsVisible) {
                LoadRisks();
            }
            else
                NeedRefresh = true;
        }
        
        private void LoadRisks() {        
            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Risk>>(
                () => { return projectsService.GetProjectRisks(project.ProjectUId); },
                risks => {
                    IsLoadingData = false;
                    NeedRefresh = false;

                    ShowDetailCommand = null;
                    this.risks.ClearAndDispose();
                    this.risks = new List<Risk>(risks);
                    ShowDetailCommand = new DelegateCommand<RiskViewModel>(ShowDetail);
                    ApplyFilterRisk();
                });
        }

        #endregion

        public Project Project {
            get {
                return project;
            }
            set {
                this.project = value;

                IsVisible = (project != null);

                if (project != null && authorizator != null)
                    UserCanEdit = project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
                else
                    UserCanEdit = false;
                FilteredRisks.ClearAndDispose();
                NeedRefresh = true;
                OnPropertyChanged("Project");
            }
        }

        private void OnViewProjectDetails(Project project) {
            Project = project;
            AskForRefresh();
        }

        private void OnSignedMemberChanged(MemberProfile m) {
            if (project == null || authorizator == null || authorizator.SignedMemberProfile == null)
                UserCanEdit = false;            
            else
                UserCanEdit = project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
        }

        private void ShowDetail(RiskViewModel risk) {
            riskDetail.Show(risk.Risk);
        }


        [Import(typeof(RisksList))]
        public IView View { get; set; }

        public string PanelName {
            get { return Properties.Resources.Risks; }
        }

        public int PanelDisplayOrder {
            get { return 600; }
        }


        private void ApplyFilterRisk() {            
            
            ICollection<Risk> filteredRisks;
            if (!ShowMitigatedRisks)
                filteredRisks = risks.Where(r => r.Probability != (short)RiskProbabilities.NONE_PROBABILITY_RISK).OrderByDescending(r => r.Impact).ToArray();
            else
                filteredRisks = risks.OrderByDescending(r => r.Impact).ToArray();

            ICollection<RiskViewModel> filteredVM = new ObservableCollection<RiskViewModel>();
            foreach (Risk risk in filteredRisks)
                filteredVM.Add(new RiskViewModel(executor, projectsService, risk));

            FilteredRisks = filteredVM;
        }

        public ICollection<RiskViewModel> FilteredRisks {
            get {
                return filteredRisks;
            }
            set {
                filteredRisks.ClearAndDispose();
                filteredRisks = value;
                OnPropertyChanged("FilteredRisks");
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
        public ICommand AddRiskCommand { get; set; }
        public ICommand ShowDetailCommand { get; set; }
        
    }
}
