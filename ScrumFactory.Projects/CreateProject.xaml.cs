using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Projects.ViewModel;

namespace ScrumFactory.Projects {


    /// <summary>
    /// Create Project view.
    /// </summary>
    [Export(typeof(CreateProject))]
    public partial class CreateProject : UserControl, ScrumFactory.Composition.View.IView {

        object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProject"/> class.
        /// </summary>        
        public CreateProject() {            
            this.InitializeComponent();            
        }        

        #region ICreateProjectView Members

       

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(CreateProjectViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }

        #endregion
    }
}
