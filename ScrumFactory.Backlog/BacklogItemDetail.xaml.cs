using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Composition.View;


using System.Windows.Input;

namespace ScrumFactory.Backlog {
    
    /// <summary>
    /// Interaction logic for BacklogItemDetailView.xaml
    /// </summary>
    [Export(typeof(BacklogItemDetail))]
    public partial class BacklogItemDetail : UserControl, IView {

        private object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacklogItemDetail"/> class.
        /// </summary>
        public BacklogItemDetail() {            
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(BacklogItemDetail_Loaded);
            this.PreviewKeyDown += new KeyEventHandler(BacklogItemDetail_PreviewKeyDown);
        }

        // NOT VERY MVVM, but the only way to avoid textbox to salwwo my key bindings
        void BacklogItemDetail_PreviewKeyDown(object sender, KeyEventArgs e) {
            foreach (InputBinding inputBinding in this.InputBindings) {
                KeyGesture keyGesture = inputBinding.Gesture as KeyGesture;
                if (keyGesture != null && keyGesture.Key == e.Key && keyGesture.Modifiers == Keyboard.Modifiers) {
                    if (inputBinding.Command != null) {
                        inputBinding.Command.Execute(0);
                        e.Handled = true;
                    }
                }
            }
        }

        void BacklogItemDetail_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            itemName.Focus(); // noy much MVVM, but at least is simple            

        }


        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>    
        [Import(typeof(ViewModel.BacklogItemViewModel))]
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
