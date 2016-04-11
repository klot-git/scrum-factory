using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Projects.ViewModel;
using ScrumFactory.Composition.View;
using System.Linq;

namespace ScrumFactory.Projects {

    /// <summary>
    /// The Projects List View.
    /// </summary>
    [Export(typeof(ProjectsList))]
    public partial class ProjectsList : UserControl, IView {
        private object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsList"/> class.
        /// </summary>        
        public ProjectsList()
        {
            this.InitializeComponent();

            Projects.ProjectGroupSize.ScreenSize = ((int) System.Windows.SystemParameters.PrimaryScreenWidth) - 50;
            Projects.ProjectGroupSize.TemplateMinSize = 320;
        }

       

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(ProjectsListViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {            
                this.model = value;
                this.DataContext = model;
            }
        }
      
    }
}
