using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using ScrumFactory.Login.ViewModel;
using ScrumFactory.Composition.View;




namespace ScrumFactory.Login {

    
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>            
    [Export]
    public partial class Login : UserControl, IView {

        public Login() {
            InitializeComponent();
        
        }

        
        
        private object model;

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>     
        [Import(typeof(LoginViewModel))]
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
