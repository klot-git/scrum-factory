using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;


namespace ScrumFactory.Projects {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "RepositoryLogCommand")]
    public class RepositoryLogCommandConfig : IConfigValue {

        public object Value {
            get {
                return Properties.Settings.Default.RepositoryLogExternalCommand;
            }
            set {
                Properties.Settings.Default.RepositoryLogExternalCommand = (string)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
