using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {

    /// <summary>
    /// Location of the panel at the application.
    /// </summary>
    public enum PanelPlacements {        
        Normal,
        Hidden,
        Project
    }


    public interface ITopMenuViewModel : IViewModel {

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

        /// <summary>
        /// Gets the panel placement.
        /// </summary>
        /// <value>The panel placement.</value>
        PanelPlacements PanelPlacement { get; }

        string ImageUrl { get; }
    }
}
