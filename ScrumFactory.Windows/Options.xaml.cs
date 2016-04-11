using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows {

    
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    [Export]
    public partial class Options : UserControl, IView {

        private object model;

        public Options() {
            InitializeComponent();
        }

        [Import(typeof(ViewModel.OptionsViewModel))]
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
