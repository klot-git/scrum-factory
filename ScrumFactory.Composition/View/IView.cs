using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.View {
    public interface IView {

        object Model { get; set; }

        bool IsVisible { get;  }
    }
}
