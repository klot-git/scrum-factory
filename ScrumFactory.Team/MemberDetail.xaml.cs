using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {
    /// <summary>
    /// Interaction logic for MemberDetail.xaml
    /// </summary>
    [Export]
    public partial class MemberDetail : UserControl, IView {

        private object model;

        public MemberDetail() {
            InitializeComponent();
        }

        [Import(typeof(MemberDetailViewModel))]
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
