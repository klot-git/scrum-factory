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
using ScrumFactory.Composition.View;

namespace ScrumFactory.HelpDeskPlugin {

    /// <summary>
    /// Interaction logic for HelpDesk.xaml
    /// </summary>
    [Export]
    public partial class HelpDesk : UserControl, IView {

        private object model;

        public HelpDesk() {
            InitializeComponent();
        }


        [Import(typeof(ViewModels.HelpDeskViewModel))]
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
