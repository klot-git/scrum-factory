using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {
    public interface IEditableObjectViewModel {

        void NotifyAdded();
        bool JustHasBeenInserted { get; }

        bool IsSelected { get; set; }
            
    }
}
