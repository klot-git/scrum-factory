using System.ComponentModel.Composition;
using System.Windows;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows {

    public partial class MessageBox : Window, IDialogView {

        private object model;

        public MessageBox() {
            InitializeComponent();                
        }

        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }

        public new void Show() {
            if (IsVisible)
                return;
            Owner = Application.Current.MainWindow;
            ShowDialog();
        }

        public new void Close() {
            contentView.Content = null;
            base.Close();
        }

     

        
    }
}
