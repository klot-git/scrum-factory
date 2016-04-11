using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Backlog {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "SprintLength")]
    public class SprintLengthConfig : IConfigValue {

        public object Value {
            get {
                return Properties.Settings.Default.SprintLength;
            }
            set {
                Properties.Settings.Default.SprintLength = (int)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
