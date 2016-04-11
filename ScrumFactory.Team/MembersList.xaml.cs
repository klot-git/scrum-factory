using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Team.ViewModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {
    /// <summary>
    /// Interaction logic for MembersList.xaml
    /// </summary>
    [Export]
    public partial class MembersList : UserControl, IView {

        private object model;

        public MembersList() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(MembersListViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = model;
            }
        }
    }
}
