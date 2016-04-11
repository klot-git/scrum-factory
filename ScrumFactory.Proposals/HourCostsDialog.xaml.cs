using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using ScrumFactory.Proposals.ViewModel;
using ScrumFactory.Composition.View;



namespace ScrumFactory.Proposals {
    /// <summary>
    /// Interaction logic for HourCostsDialog.xaml
    /// </summary>
    [Export]
    public partial class HourCostsDialog : UserControl, IView {

        public HourCostsDialog() {
            InitializeComponent();
        }

        private object model;

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>     
        [Import(typeof(HourCostsViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }


    }
}
