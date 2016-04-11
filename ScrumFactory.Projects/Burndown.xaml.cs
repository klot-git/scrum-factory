using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Projects.ViewModel;
using ScrumFactory.Composition.View;



namespace ScrumFactory.Projects {

    /// <summary>
    /// Interaction logic for Burndown.xaml
    /// </summary>
    [Export(typeof(Burndown))]
    public partial class Burndown : UserControl, IView  {

        private object model;

        public Burndown() {
            InitializeComponent();            
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>  
        [Import(typeof(BurndownViewModel))] 
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
