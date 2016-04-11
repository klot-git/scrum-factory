using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for MemberProfile.xaml
    /// </summary>
    [Export(typeof(MyProfile))]
    public partial class MyProfile : UserControl, IView {


        private object model;

       public MyProfile() {
            InitializeComponent();
        }

       [Import(typeof(MemberViewModel))]
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
