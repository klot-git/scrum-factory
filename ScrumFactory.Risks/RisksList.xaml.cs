using System;
using System.ComponentModel.Composition;
using ScrumFactory.Risks.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Risks {

    /// <summary>
    /// Interaction logic for RisksList.xaml
    /// </summary>
    [Export]
    public partial class RisksList : UserControl, IView {


        private object model;

        public RisksList() {
            InitializeComponent();
        }

        [Import(typeof(RisksListViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }
    }
}
