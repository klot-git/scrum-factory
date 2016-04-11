using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {
    public interface IProjectContainer : IViewModel, IChildWindow {

        IProjectTabViewModel SelectedProjectTab { get; set; }
    }
}
