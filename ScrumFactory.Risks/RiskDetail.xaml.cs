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
using System.ComponentModel.Composition;

namespace ScrumFactory.Risks {

    /// <summary>
    /// Interaction logic for RiskDetail.xaml
    /// </summary>
    [Export]
    public partial class RiskDetail : UserControl, ScrumFactory.Composition.View.IView {

        private object model;

        public RiskDetail() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>    
        [Import(typeof(ViewModel.RiskViewModel))]
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
