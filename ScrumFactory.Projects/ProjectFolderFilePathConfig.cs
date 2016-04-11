using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;

namespace ScrumFactory.Projects {

    [Export(typeof(IConfigValue))]
    [ExportMetadata("Name", "ProjectFolderFilePath")]
    public class ProjectFolderFilePathConfig : IConfigValue {
        public object Value {
            get {
                return Properties.Settings.Default.ProjectFolderFilePath;
            }
            set {
                Properties.Settings.Default.ProjectFolderFilePath = (string)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
