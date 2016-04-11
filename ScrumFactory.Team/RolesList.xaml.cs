using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for RolesList.xaml
    /// </summary>
    [Export]
    public partial class RolesList : UserControl, IView {


        private object model;

        public RolesList() {
            InitializeComponent();
            Loaded += new System.Windows.RoutedEventHandler(RolesList_Loaded);
        }

        void RolesList_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            newRoleMenuItem.Focus();
        }

        #region IRolesListView Members

        [Import(typeof(RolesListViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }

        #endregion
    }
}
