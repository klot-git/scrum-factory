using System.ComponentModel.Composition;
using ScrumFactory.Tasks.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Tasks {

    /// <summary>
    /// Interaction logic for TaskDetail.xaml
    /// </summary>
    [Export(typeof(TaskDetail))]
    public partial class TaskDetail : UserControl, IView {

        private object model;

        public TaskDetail() {
            InitializeComponent();
        }

        [Import(typeof(TaskViewModel))]
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
