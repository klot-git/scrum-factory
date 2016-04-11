using System.ComponentModel.Composition;
using ScrumFactory.Tasks.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Tasks {
    
    /// <summary>
    /// Interaction logic for TasksList.xaml
    /// </summary>
    [Export(typeof(TasksList))]
    public partial class TasksList : UserControl, IView {

        private object model;

        public TasksList() {
            InitializeComponent();
        }

        [Import(typeof(TasksListViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }

        //public void HideList(int mode) {

        //    if (mode == 0) {
        //        grid.Children.Remove(normalList);
        //        if (!grid.Children.Contains(postItList))
        //            grid.Children.Add(postItList);
        //    } else {
        //        grid.Children.Remove(postItList);
        //        if (!grid.Children.Contains(normalList))
        //            grid.Children.Add(normalList);
        //    }
        

        //}
    }
}
