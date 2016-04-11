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
using System.Diagnostics;

namespace ScrumFactory.Windows.ViewModel {

    [Export]
    [Export(typeof(ITopMenuViewModel))]
    public class WhatIsNewViewModel : ITopMenuViewModel, IViewModel {
        
        [Import(typeof(WhatIsNew))]
        public IView View { get; set; }

        [Import]
        private IDialogService dialogs { get; set; }


        public WhatIsNewViewModel(){
            ShowOnStartCommand = new DelegateCommand<bool>(SetShowOnStart);
            CloseWindowCommand = new DelegateCommand(CloseWindow);
        }

        private void CloseWindow() {
            dialogs.GoToFirstTopMenu();
        }

        private void SetShowOnStart(bool showOnStart){
            Properties.Settings.Default.ShowOnStart = showOnStart;
            Properties.Settings.Default.Save();
        }

        public ICommand ShowOnStartCommand { get; set; }

        public string PanelName {
            get { return Properties.Resources.What_is_new; }
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

        public ICommand CloseWindowCommand { get; set; }
    }
}
