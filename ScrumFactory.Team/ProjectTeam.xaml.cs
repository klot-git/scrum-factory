using System.ComponentModel.Composition;
using ScrumFactory.Team.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team {
    /// <summary>
    /// Interaction logic for ProjectTeam.xaml
    /// </summary>
    [Export]    
    public partial class ProjectTeam : UserControl, IView {

        private  object model;
        
        public ProjectTeam() {
            InitializeComponent();
        }

        [Import(typeof(ProjectTeamViewModel))]
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
