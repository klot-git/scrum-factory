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

namespace ScrumFactory.FactoryServerConfiguration {

    /// <summary>
    /// Interaction logic for ServerConfiguration.xaml
    /// </summary>
    [Export]
    public partial class ServerConfiguration : UserControl, ScrumFactory.Composition.View.IView {

        private object model;

        public ServerConfiguration() {
            InitializeComponent();
        }

        [Import(typeof(ViewModel.ServerConfigurationViewModel))]
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
