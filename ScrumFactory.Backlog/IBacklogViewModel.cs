namespace ScrumFactory.Backlog
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public interface IBacklogViewModel
    {
        ObservableCollection<BacklogItem> Items
        {
            get;
        }

        ICommand SaveBacklogCommand
        {
            get;
        }

        ICommand DeleteBacklogItemCommand
        {
            get;
        }

        ICommand AddBacklogItemCommand
        {
            get;
        }
    }
}
