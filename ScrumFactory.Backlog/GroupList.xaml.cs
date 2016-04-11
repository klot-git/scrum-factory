using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Backlog {

    /// <summary>
    /// Interaction logic for GroupList.xaml
    /// </summary>
    [Export]
    public partial class GroupList : UserControl, IView {


        private object model;

        public GroupList() {
            InitializeComponent();
        }


        #region IView Members

        [Import(typeof(ViewModel.GroupListViewModel))]
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
