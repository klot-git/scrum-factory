using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScrumFactory.Projects.ViewModel;
using System.ComponentModel.Composition;

namespace ScrumFactory.Projects {

    [Export]
    /// <summary>
    /// Interaction logic for CreateRepository.xaml
    /// </summary>
    public partial class CreateRepository : UserControl, ScrumFactory.Composition.View.IView {


        object model;

        public CreateRepository() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(CreateRepositoryViewModel))]
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
