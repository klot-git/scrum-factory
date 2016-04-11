namespace ScrumFactory.Backlog
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Windows.Input;
    using ScrumFactory.Composition;
    using ScrumFactory.Services;

    [Export(typeof(IBacklogViewModel))]
    [Export(typeof(IPanelViewModel))]
    public class BacklogViewModel : BasePanelViewModel, IBacklogViewModel, IPanelViewModel, INotifyPropertyChanged
    {
        private IBacklogService backlogService;
        private IBackgroundExecutor backgroundExecutor;
        private IEventAggregator eventAggregator;
        private string projectId;
        private string newItemDescription;

        [ImportingConstructor()]
        public BacklogViewModel(
            [Import] IBacklogService backlogService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator)
        {
            this.backlogService = backlogService;
            this.backgroundExecutor = backgroundExecutor;
            this.eventAggregator = eventAggregator;

            this.Items = new ObservableCollection<BacklogItem>();

            this.eventAggregator.Subscribe<Project>(
                "ViewProjectDetails",
                p =>
                {
                    this.projectId = p.Id;

                    this.backgroundExecutor.StartBackgroundTask<ICollection<BacklogItem>>(
                        () =>
                        {
                            return this.backlogService.GetBacklog(p.Id);
                        },
                        i =>
                        {
                            this.Items.Clear();

                            foreach (var item in i)
                            {
                                this.Items.Add(item);
                            }
                        });
                });

            this.SaveBacklogCommand = new DelegateCommand(() =>
            {
                this.backgroundExecutor.StartBackgroundTask(
                    () =>
                    {
                        this.backlogService.SaveBacklog(this.projectId, this.Items);
                    },
                    () => { });
            });

            this.DeleteBacklogItemCommand = new DelegateCommand<BacklogItem>(item =>
            {
                this.Items.Remove(item);
            });

            this.AddBacklogItemCommand = new DelegateCommand(() =>
            {
                this.Items.Add(
                    new BacklogItem
                    {
                        Description = this.NewItemDescription,
                        ProjectId = this.projectId
                    });
            });
        }

        public ObservableCollection<BacklogItem> Items
        {
            get;
            private set;
        }

        public ICommand SaveBacklogCommand
        {
            get;
            private set;
        }

        public string PanelName
        {
            get
            {
                return "Backlog";
            }
        }

        [Import(typeof(IBacklogView))]
        public IBacklogView View
        {
            get;
            set;
        }

        object IPanelViewModel.View
        {
            get
            {
                return this.View;
            }
        }

        public ICommand DeleteBacklogItemCommand
        {
            get;
            private set;
        }

        public ICommand AddBacklogItemCommand
        {
            get;
            private set;
        }

        public string NewItemDescription
        {
            get
            {
                return this.newItemDescription;
            }

            set
            {
                this.newItemDescription = value;
                this.RaisePropertyChanged("NewItemDescription");
            }
        }
    }
}
