using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for PendingMemberships.xaml
    /// </summary>
    [Export]
    public partial class PendingMemberships : UserControl, IView {

        private object model;

        public PendingMemberships() {
            InitializeComponent();
        }

        [Import(typeof(PendingMembershipsListViewModel))]
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
