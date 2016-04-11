using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Team {
    
    [Export]
    public partial class OwnersList : UserControl, IView {

        private object model;

        public OwnersList() {
            InitializeComponent();
        }

        [Import(typeof(OwnersListViewModel))]
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
