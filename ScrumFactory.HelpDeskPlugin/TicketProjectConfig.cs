using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.HelpDeskPlugin {
    
    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "TicketProjectsEnabled")]
    public class TicketProjectConfig : IConfigValue {

        public object Value {
            get {                                
                return Properties.Settings.Default.TicketProjectsEnabled;
            }
            set {
                Properties.Settings.Default.TicketProjectsEnabled = (bool) value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
