using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Artifacts {
    public class CanEnlargeConverter : IValueConverter {
        

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Artifact artifact = value as Artifact;
            
            if (artifact == null)
                return false;

            string ext = ArtifactImageConverter.GetExtension(artifact.ArtifactPath);
            if (ArtifactImageConverter.IsImage(ext))
                return true;

            

            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }


    }
}
