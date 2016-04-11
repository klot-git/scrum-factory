using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition;

namespace ScrumFactory.Composition.View {

    public interface IDialogView : IView {

        void Show();

        void Close();

        void DragMove();

        System.Windows.WindowState WindowState { get; set; }
    }
}
