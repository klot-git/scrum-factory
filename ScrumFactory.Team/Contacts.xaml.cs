using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for Contacts.xaml
    /// </summary>
    [Export]
    public partial class Contacts : UserControl, IView {

        public Contacts() {
            InitializeComponent();
        }

        private object model;

        [Import(typeof(ContactListViewModel))]
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
