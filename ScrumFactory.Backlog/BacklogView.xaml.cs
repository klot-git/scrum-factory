namespace ScrumFactory.Backlog
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    [Export(typeof(IBacklogView))]
    public partial class BacklogView : UserControl, IBacklogView
    {
        private IBacklogViewModel model;

        public BacklogView()
        {
            this.InitializeComponent();
        }

        [Import()]
        public IBacklogViewModel Model
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
    }
}
