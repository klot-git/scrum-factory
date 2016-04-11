using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Projects {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "RepositoryUrl")]
    public class RepositoryUrlConfig : IConfigValue {
        public object Value {
            get {
                return Properties.Settings.Default.RepositoryUrl;
            }
            set {
                Properties.Settings.Default.RepositoryUrl = (string)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
