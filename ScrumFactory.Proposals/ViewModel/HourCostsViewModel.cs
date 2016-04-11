using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Configuration;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Proposals.ViewModel {

    [Export]    
    public class HourCostsViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged  {

        private IAuthorizationService authorizator;
        private IEventAggregator aggregator;        
        private IDialogService dialogs;
        private IBackgroundExecutor executor;

        private IProposalsService proposalsService;

        private RoleHourCost[] hourCosts;

        private IDialogViewModel window;

        private Project project;

        [ImportingConstructor]
        public HourCostsViewModel(
            [Import] IEventAggregator aggregator,            
            [Import] IBackgroundExecutor executor,
            [Import] IAuthorizationService authorizator,
            [Import] IDialogService dialogService,
            [Import] IProposalsService proposalsService) {

            this.aggregator = aggregator;
            this.executor = executor;
            this.authorizator = authorizator;
            this.dialogs = dialogService;

            this.proposalsService = proposalsService;
            this.ChangeCostsCommand = new DelegateCommand(ChangeCosts);
            this.CancelCommand = new DelegateCommand(Cancel);

        }
    
        private void ChangeCosts() {
            executor.StartBackgroundTask(
                () => { proposalsService.UpdateHourCosts(project.ProjectUId, HourCosts); },
                () => {
                    aggregator.Publish(ScrumFactoryEvent.RoleHourCostsChanged);
                    window.Close();
                });
            
        }

        private void Cancel() {
            window.Close();
        }

        public void Show(Project project) {
            this.project = project;
            window = dialogs.NewDialog(Properties.Resources.Hour_costs, View);
            window.Show();
        }
            
            

        public RoleHourCost[] HourCosts {
            get {
                return hourCosts;
            }
            set {
                hourCosts = value;
                OnPropertyChanged("HourCosts");
            }
        }



        [Import(typeof(HourCostsDialog))]
        public IView View { get; set; }


        public ICommand ChangeCostsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        
    }

}
