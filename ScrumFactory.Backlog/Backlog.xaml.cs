using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Backlog {

    /// <summary>
    /// Backlog View.
    /// </summary>
    [Export(typeof(Backlog))]
    public partial class Backlog : UserControl, IView {
        
        private object model;

        /// <summary>
        /// Initializes a new instance of the <see cref="Backlog"/> class.
        /// </summary>
        public Backlog() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Import(typeof(BacklogViewModel))]
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
