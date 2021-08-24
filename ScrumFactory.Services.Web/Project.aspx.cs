using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ScrumFactory.Services.Web
{
    public partial class Project : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string ProjectNumber {
            get {
                return Request["number"];
            }
        }

        public string DefaultCompanyName {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["DefaultCompanyName"];
            }
        }
    }
}