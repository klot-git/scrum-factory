using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.ProjectConstraints.ViewModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.ProjectConstraints {

    /// <summary>
    /// Interaction logic for ConstraintList.xaml
    /// </summary>
    [Export]
    public partial class ConstraintList : UserControl, IView {

        private object model;

        public ConstraintList() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(ContraintListViewModel))]
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
