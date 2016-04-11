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
using ScrumFactory.Composition.View;
using System.ComponentModel.Composition;

namespace ScrumFactory.Tasks {
    
    /// <summary>
    /// Interaction logic for FinishTaskDialog.xaml
    /// </summary>
    [Export]
    public partial class FinishTaskDialog : UserControl, IView {

        private object model;

        public FinishTaskDialog() {
            InitializeComponent();
        }

        [Import(typeof(ViewModel.FinishTaskDialogViewModel))]
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
