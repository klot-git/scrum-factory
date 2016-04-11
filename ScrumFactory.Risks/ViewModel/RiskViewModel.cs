using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Services;
using ScrumFactory.Composition;
using ScrumFactory.Extensions;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel.Composition;

namespace ScrumFactory.Risks.ViewModel {

    [Export]
    public class RiskViewModel : BaseEditableObjectViewModel, IViewModel {

        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IProjectsService projectsService;

        private Risk oldRisk;
        private Risk risk;

        private IDialogService dialogs;

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }

        [ImportingConstructor]
        public RiskViewModel(
            [Import] IBackgroundExecutor executor,
            [Import] IEventAggregator aggregator,
            [Import] IDialogService dialogs,
            [Import] IProjectsService projectsService) {

            this.executor = executor;
            this.aggregator = aggregator;
            this.projectsService = projectsService;
            this.dialogs = dialogs;

            CloseWindowCommand = new DelegateCommand(CloseWindow);
   
        }

        public RiskViewModel(
            IBackgroundExecutor executor,
            IProjectsService projectsService,
            Risk risk) {

                this.executor = executor;
                this.projectsService = projectsService;
                Risk = risk;

                ChangeRiskImpactCommand = new DelegateCommand(ChangeRiskImpact);
                ChangeRiskProbabilityCommand = new DelegateCommand(ChangeRiskProbability);
        }

         ~RiskViewModel() {
            System.Console.WriteLine("***< risk died here");
        }

        public Risk Risk {
            get {
                return risk;
            }
            set {
                risk = value;
                OnPropertyChanged("Risk");
                OnPropertyChanged("PanelName");
            }
        }

        public string PanelName {
            get {
                if (Risk == null || String.IsNullOrEmpty(Risk.RiskDescription))
                    return Properties.Resources.Risk;
                if(Risk.RiskDescription.Length<20)
                    return Risk.RiskDescription;
                else
                    return Risk.RiskDescription.Substring(0, 20) + "...";
            }
        }

        public void Show(Risk risk) {
            Risk = risk;
            oldRisk = risk.Clone();
            Show(projectContainer.Value);            
        }


        private void ChangeRiskImpact() {
            executor.StartBackgroundTask(
                () => { projectsService.ChangeRiskImpact(Risk.ProjectUId, Risk.RiskUId, Risk.Impact); },
                () => { });                
        }

        private void ChangeRiskProbability() {
            executor.StartBackgroundTask(
                () => { projectsService.ChangeRiskProbability(Risk.ProjectUId, Risk.RiskUId, Risk.Probability); },
                () => { });
        }

        /// <summary>
        /// Closes the window, invoke the OnCloseAction and publishes the CloseWindow event.
        /// </summary>
        private void CloseWindow() {
            if (!risk.IsTheSame(oldRisk))
                SaveAndClose();
            else
                Close();
        }

        private void SaveAndClose() {
            Risk.UpdatedAt = DateTime.Now;
            executor.StartBackgroundTask(
                () => { projectsService.UpdateRisk(Risk.ProjectUId, Risk.RiskUId, Risk); },
                () => {
                    Close();
                }
            );
        }

        protected override void OnDispose() {

            if(aggregator!=null)
                aggregator.UnSubscribeAll(this);

            ChangeRiskImpactCommand = null; OnPropertyChanged("ChangeRiskImpactCommand");
            ChangeRiskProbabilityCommand = null; OnPropertyChanged("ChangeRiskProbabilityCommand");
            CloseWindowCommand = null; OnPropertyChanged("CloseDetailCommand");        
        }

        [Import(typeof(RiskDetail))]
        public ScrumFactory.Composition.View.IView View { get; set; }

        public ICommand ChangeRiskImpactCommand { get; set; }
        public ICommand ChangeRiskProbabilityCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
                
    }
}
