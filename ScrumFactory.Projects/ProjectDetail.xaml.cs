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

namespace ScrumFactory.Projects {

    /// <summary>
    /// Interaction logic for ProjectDetail.xaml
    /// </summary>
    [Export]
    public partial class ProjectDetail : UserControl, IView {

        private object model;

        public ProjectDetail() {
            InitializeComponent();
        }

        [Import(typeof(ViewModel.ProjectViewModel))]
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
