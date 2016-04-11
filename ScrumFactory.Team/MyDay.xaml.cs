using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Team {

    /// <summary>
    /// Interaction logic for MyDay.xaml
    /// </summary>
    [Export]
    public partial class MyDay : UserControl, IView {

        private object model;
        
        public MyDay() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(ViewModel.MyDayViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = model;
            }
        }
    }
}
