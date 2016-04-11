using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {
    public interface IChildWindow {

        ICollection<object> OpenedWindows { get; }

        void Show(IChildWindow parent);

        void Close();

        void ShowChildWindow(IViewModel viewModel);

        void CloseChildWindow(IViewModel viewModel);
    }
}
