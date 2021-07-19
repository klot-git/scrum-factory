using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services {
    public class SVNSettings {

        public string SVN_FilePath { get; set; }
        public string SVN_Url { get; set; }
        public int SVN_Version { get; set; }

        public static SVNSettings Read() {
            SVNSettings s = new SVNSettings();
            s.SVN_FilePath = GetSVN_FilePath();
            s.SVN_Url = GetSVN_Url();
            s.SVN_Version = GetSVN_Version();
            return s;
        }

        public static string GetSVN_FilePath() {            
            string p = System.Configuration.ConfigurationManager.AppSettings["SVN_FilePath"];
            if (String.IsNullOrEmpty(p))
                return String.Empty;
            if (!p.EndsWith("\\"))
                p = p + "\\";
            return p;            
        }

        public static string GetSVN_Url() {            
            string p = System.Configuration.ConfigurationManager.AppSettings["SVN_Url"];
            if (String.IsNullOrEmpty(p))
                return String.Empty;
            if (!p.EndsWith("/"))
                p = p + "/";
            return p;
            
        }

        public static int GetSVN_Version() {            
            string v = System.Configuration.ConfigurationManager.AppSettings["SVN_Version"];
            if (String.IsNullOrEmpty(v))
                return 0;
            int i = 0;
            int.TryParse(v, out i);
            return i;            
        }
    }
}
