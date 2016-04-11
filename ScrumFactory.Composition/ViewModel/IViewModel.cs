using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Composition.ViewModel {

    public interface IViewModel {
        IView View { get; set; }
    }
}
