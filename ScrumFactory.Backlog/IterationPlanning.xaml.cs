using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Backlog {

    /// <summary>
    /// Iteration planing view.
    /// </summary>
    [Export(typeof(IterationPlanning))]
    public partial class IterationPlanning : UserControl, IView {

        private object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationPlanning"/> class.
        /// </summary>
        public IterationPlanning() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(IterationPlanningViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }

        /// <summary>
        /// To fix mouse scroll issue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            scroll.AddHandler(MouseWheelEvent, new RoutedEventHandler(MyMouseWheelH), true);
        }

        private void MyMouseWheelH(object sender, RoutedEventArgs e) {

            MouseWheelEventArgs eargs = (MouseWheelEventArgs)e;

            double x = (double)eargs.Delta;

            double y = scroll.VerticalOffset;

            scroll.ScrollToVerticalOffset(y - x);
        }

       
    }
}
