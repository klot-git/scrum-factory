using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using ScrumFactory.Team.ViewModel;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for JoinDialog.xaml
    /// </summary>
    [Export]
    public partial class JoinDialog : UserControl, IView {

        public JoinDialog() {
            InitializeComponent();
        }

        private object model;

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>     
        [Import(typeof(JoinViewModel))]
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
