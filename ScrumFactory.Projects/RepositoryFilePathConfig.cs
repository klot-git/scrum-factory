using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Projects {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "RepositoryFilePath")]
    public class RepositoryFilePathConfig : IConfigValue {
        public object Value {
            get {
                return Properties.Settings.Default.RepositoryFilePath;
            }
            set {
                Properties.Settings.Default.RepositoryFilePath = (string)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
