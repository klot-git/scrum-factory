using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Projects {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "RepositoryVersion")]
    public class RepositoryVersionConfig : IConfigValue {
        public object Value {
            get {
                return Properties.Settings.Default.RepositoryVersion;
            }
            set {
                Properties.Settings.Default.RepositoryVersion = (string)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
