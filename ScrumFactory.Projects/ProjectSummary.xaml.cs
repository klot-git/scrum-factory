using System.ComponentModel.Composition;
using ScrumFactory.Projects.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Projects {

    /// <summary>
    /// The Project Summary View.
    /// </summary>
    [Export(typeof(ProjectSummary))]
    public partial class ProjectSummary : UserControl, IView  {

        private object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSummary"/> class.
        /// </summary>        
        public ProjectSummary() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(ProjectSummaryViewModel))]
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
