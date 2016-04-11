using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System.Windows.Input;
using System.Linq;
using System;
using ScrumFactory.Composition.View;
using System.Deployment.Application;

namespace ScrumFactory.Windows.ViewModel {

    [Export]
    public class AboutViewModel : IViewModel {

        public AboutViewModel() {
            Version = "DEV";
            if (ApplicationDeployment.IsNetworkDeployed)
                Version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

            ShowFactorySiteCommand = new DelegateCommand(ShowFactorySite);
        }

        public string Version { get; private set; }

        [Import(typeof(AboutDialog))]
        public IView View { get; set; }

        private void ShowFactorySite() {
            System.Diagnostics.Process.Start("http://www.scrum-factory.com");
        }


        public ICommand ShowFactorySiteCommand { get; set; }
    }
}
