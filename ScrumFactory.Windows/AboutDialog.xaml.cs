using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows {
    
    [Export]
    public partial class AboutDialog : UserControl, IView  {

        public AboutDialog() {
            InitializeComponent();
        }

        private object model;

        [Import(typeof(ViewModel.AboutViewModel))]
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
