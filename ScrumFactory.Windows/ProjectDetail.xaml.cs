using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows {

    /// <summary>
    /// Interaction logic for ProjectDetail.xaml
    /// </summary>
    [Export]
    public partial class ProjectDetail : UserControl, IView {

        private object model;

        public ProjectDetail() {
            InitializeComponent();
        }

        [Import(typeof(ViewModel.ProjectDetailViewModel))]
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
