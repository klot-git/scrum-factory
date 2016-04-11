using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ScrumFactory.Composition.ViewModel {

    public interface IPluginCommand {

        ICommand OnClickCommand { get; }

        string MenuItemHeader { get; }
        string MenuItemIconUrl { get; }
        bool IsCheckeable { get; }

        bool IsChecked { get; set;  }

        string ContainerViewModelClassName { get; }

        int DisplayOrder { get; }

    }
}
