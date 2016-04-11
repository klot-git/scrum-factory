using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Artifacts {
    public class ArtifactImageConverter : IValueConverter {
        

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Artifact artifact = value as Artifact;
            
            if (artifact == null)
                return null;

            string ext = GetExtension(artifact.ArtifactPath);
            if (IsImage(ext))
                return artifact.ArtifactPath;

            if (ext == "docx" || ext == "doc" || ext == "dot" || ext == "docm"|| ext == "dotx" || ext == "dotm")
                return "/Images/ArtifactIcons/word.png";

            if (ext == "xls" || ext == "xlt" || ext == "xlm" || ext == "xlsx" || ext == "xlsm" || ext == "xltx" || ext == "xltm")
                return "/Images/ArtifactIcons/excel.png";

            if (ext == "ppt" || ext == "pot" || ext == "pps" || ext == "pptx" || ext == "pptm" || ext == "potx" || ext == "potm" || ext == "ppam" || ext == "ppsx" || ext == "ppsm" || ext == "sldx" || ext == "sldm")
                return "/Images/ArtifactIcons/powerpoint.png";

            if (ext == "vsd")
                return "/Images/ArtifactIcons/visio.png";

            if (ext == "mpp")
                return "/Images/ArtifactIcons/project.png";



            return "/Images/ArtifactIcons/no-icon.png";

        }

        public static string GetExtension(string file) {
            if (file == null)
                return null;
            int idx = file.LastIndexOf('.');
            if (idx < 0 || idx==file.Length-1)
                return null;
            return file.Substring(idx+1).ToLower();
        }

        public static bool IsImage(string ext) {            
            if (ext == null)
                return false;
            if (ext == "png" || ext == "bmp" || ext == "jpg" || ext == "jpeg" || ext == "tiff" || ext == "gif")
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }


    }
}
