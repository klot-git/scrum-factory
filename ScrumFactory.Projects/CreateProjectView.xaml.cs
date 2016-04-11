namespace ScrumFactory.ProjectsList
{
    using System.ComponentModel.Composition;
    using System.Windows;

    [Export(typeof(ICreateProjectView))]
    public partial class CreateProjectView : Window, ICreateProjectView
    {
        private ICreateProjectViewModel model;

        public CreateProjectView()
        {
            this.InitializeComponent();

            this.Closing += new System.ComponentModel.CancelEventHandler(this.CreateProjectViewClosing);
        }

        [Import(typeof(ICreateProjectViewModel))]
        public ICreateProjectViewModel Model
        {
            get
            {
                return this.model;
            }

            set
            {
                this.model = value;
                this.DataContext = value;
            }
        }

        void ICreateProjectView.Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
        }

        void ICreateProjectView.Hide()
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CreateProjectViewClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
