using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Text;

namespace ScrumFactory.Projects {

    
    public class ProjectGroupSize {

        public static int ScreenSize { get; set; }

        public static int TemplateMinSize { get; set; }

        public static int MaxColumns {
            get {
                return  ScreenSize / TemplateMinSize;
            }
        }

        public static int GetGroupSizeinPixels(int count) {
            if (count > MaxColumns || count==0)
                return ScreenSize;

            return (ScreenSize / count) -20;
        }
        

    }
}
