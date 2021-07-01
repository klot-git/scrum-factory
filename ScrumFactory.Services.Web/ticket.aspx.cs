using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ScrumFactory.Services.Web
{
    public partial class ticket : System.Web.UI.Page
    {
        ScrumFactory.BacklogItem Item { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Item = BacklogService.GetBacklogItem("ee");
        }

        private IBacklogService backlogService = null;
        private IBacklogService BacklogService {
            get {
                if (backlogService != null)
                    return backlogService;
                try
                {
                    backlogService = ScrumFactory.Services.Web.Application.CompositionContainer.GetExportedValue<IBacklogService>();
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    foreach (Exception e in ex.LoaderExceptions)
                        Response.Write(e.Message);
                    Response.End();
                }

                return backlogService;
            }
        }
    }
}