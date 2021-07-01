using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Web {


    public partial class _default : System.Web.UI.Page {

        private IAuthorizationService authorizator = null;
        private IAuthorizationService Authorizator {
            get {
                if (authorizator != null)
                    return authorizator;
                try {
                    authorizator = ScrumFactory.Services.Web.Application.CompositionContainer.GetExportedValue<IAuthorizationService>();
                }
                catch (System.Reflection.ReflectionTypeLoadException ex) {
                    foreach (Exception e in ex.LoaderExceptions)
                        Response.Write(e.Message);
                    Response.End();
                }
                
                return authorizator;
            }
        }

        public bool IsPublicHub {
            get {
                return Request.Url.Host.ToLower().StartsWith("scrum-factory.com") || Request.Url.Host.ToLower().StartsWith("www.scrum-factory.com");
            }
        }
        
        public string SFClientServerVersion {
            get {
                if (Authorizator == null)
                    return "?";
                return Authorizator.ServerVersion;
            }
        }

        public string SFClientServer {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["SFClientServer"];
            }
        }

        public string DefaultCompanyName {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["DefaultCompanyName"];
            }
        }

        public string SFServer {
            get {
                return Request.Url.AbsoluteUri.Replace("/default.aspx","");
            }
        }

        protected void Page_Load(object sender, EventArgs e) {
            if (!IsInstalledOk())
                Response.Redirect("~/Setup/default.aspx");
        }

        private bool IsInstalledOk() {

            bool connOk = false;

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ScrumFactoryEntitiesConnectionString"].ConnectionString;
            
            System.Text.RegularExpressions.Regex connRegex = new System.Text.RegularExpressions.Regex(@"provider\sconnection\sstring=""(?<conn>[^""]+)");
            string pureConnStr = connRegex.Match(connStr).Groups["conn"].ToString();

            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(pureConnStr);
            try {
                conn.Open();
                conn.Close();
                connOk = true;
            }
            catch (Exception ex) {}

            return connOk;
        }
    }
}