namespace ScrumFactory.ProjectsList
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    [Export(typeof(IProjectsListView))]
    public partial class ProjectsListView : UserControl, IProjectsListView
    {
        private IProjectsListViewModel model;

        [ImportingConstructor()]
        public ProjectsListView()
        {
            this.InitializeComponent();
        }

        public event System.EventHandler Shown;

        [Import()]
        public IProjectsListViewModel Model
        {
            get
            {
                return this.model;
            }

            set
            {
                this.model = value;
                this.DataContext = this.model;
            }
        }

        private void ProjectView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Shown != null)
            {
                this.Shown(this, EventArgs.Empty);
            }
        }
    }
}
