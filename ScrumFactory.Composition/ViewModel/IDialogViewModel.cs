using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ScrumFactory.Composition.ViewModel;
using System.Windows.Input;
using System.Linq;

namespace ScrumFactory.Composition.ViewModel {

    /// <summary>
    /// View Model interface for windows views.
    /// </summary>
    public interface IDialogViewModel {

        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        View.IDialogView View { get; set; }
      
        /// <summary>
        /// Shows this instance.
        /// </summary>
        void Show();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the move window command.
        /// </summary>
        /// <value>The move window command.</value>
        ICommand MoveWindowCommand { get; }

        /// <summary>
        /// Gets the close window command.
        /// </summary>
        /// <value>The close window command.</value>
        ICommand CloseWindowCommand { get; }

        /// <summary>
        /// Gets the minimize window command.
        /// </summary>
        /// <value>The minimize window command.</value>
        ICommand MinimizeWindowCommand { get; }

        /// <summary>
        /// Gets the maximize window command.
        /// </summary>
        /// <value>The maximize window command.</value>
        ICommand MaximizeWindowCommand { get; }

    }

    
}
