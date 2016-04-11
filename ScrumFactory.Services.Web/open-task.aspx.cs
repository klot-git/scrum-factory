using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ScrumFactory.Services.Web {
    public partial class open_task : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            string clientServer = System.Configuration.ConfigurationManager.AppSettings["SFClientServer"];
            string server = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;
            Response.Redirect(clientServer + "/SFClient2012/ScrumFactory.application?server=" + server + "&taskNumber=" + Page.RouteData.Values["tasktNumber"], true);
        }
    }
}