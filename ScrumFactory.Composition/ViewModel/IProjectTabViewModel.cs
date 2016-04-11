using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ScrumFactory.Composition.ViewModel {

   
    /// <summary>
    /// Basic interface for a generic panel.
    /// </summary>
    public interface IProjectTabViewModel : IViewModel {

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        string PanelName { get; }
        
        /// <summary>
        /// Gets the panel display order when it is displayed at the view.
        /// </summary>
        /// <value>The panel display order.</value>
        int PanelDisplayOrder { get; }


        bool IsVisible { get; }

        bool IsEspecialTab { get; }
    }
}
