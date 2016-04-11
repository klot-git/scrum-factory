using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Projects.ViewModel;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Projects {

    /// <summary>
    /// Interaction logic for HoursGraph.xaml
    /// </summary>
    [Export]
    public partial class HoursGraph : UserControl,IView {
        public HoursGraph() {
            InitializeComponent();
        }

          private object model;

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>  
        [Import(typeof(HoursGraphViewModel))] 
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }


        public bool IsReadOnly {
            set {
                if (value)
                    controls.Visibility = System.Windows.Visibility.Collapsed;
                else
                    controls.Visibility = System.Windows.Visibility.Visible;
            }
            get {
                return controls.Visibility == System.Windows.Visibility.Collapsed;
            }
        }
    }
}
