using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services.Logic.Helper {

    public class ReportTemplate {

        public ReportTemplate() {
            appPath = ServerUrl;
        }

        public static string ServerUrl {
            get {
                if (System.Web.HttpContext.Current == null)
                    throw new System.Exception("HTTPCONTEXT not found. could not find template.");

                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;

                string url = String.Format("{0}://{1}:{2}{3}", request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath);
                
                return url;
            }
        }

        private string appPath;

        private string TemplatePath {
            get {
                if (System.Web.HttpContext.Current == null)
                    throw new System.Exception("HTTPCONTEXT not found. could not find template.");
                return System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\ReportTemplates\\";
            }
        }

        public string[] GetTemplateList(string templateGroup) {
            string[] templates = System.IO.Directory.EnumerateFiles(TemplatePath + templateGroup).ToArray();
            for (int i = 0; i < templates.Length; i++) {                
                templates[i] = templates[i].Substring(templates[i].LastIndexOf("\\")+1);
                templates[i] = templates[i].Replace(".xslt", "");
            }
            return templates;
        }
    }
}
