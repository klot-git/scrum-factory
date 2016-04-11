using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Backlog {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "UsePoints")]
    public class UsePointsConfig : IConfigValue {

        public object Value {
            get {
                return Properties.Settings.Default.UsePoints;
            }
            set {
                Properties.Settings.Default.UsePoints = (bool)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
