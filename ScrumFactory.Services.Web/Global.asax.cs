namespace ScrumFactory.Services.Web
{
    using System.ComponentModel.Composition.Extensions;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;
    using System.ServiceModel.Activation;
    using System.Web.Routing;

    public class Application : System.Web.HttpApplication {

        public static CompositionContainer CompositionContainer { get; private set; }

        protected void Application_Start() {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(Server.MapPath("~/Bin"), "ScrumFactory.*.dll"));
            try {
                catalog.Catalogs.Add(new DirectoryCatalog(Server.MapPath("~/Bin-Plugins"), "*.dll"));
            }
            catch (System.Exception) { }
           
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new ConfigurationCatalog());
            catalog.Catalogs.Add(new ConnectionStringCatalog());

            CompositionContainer = new CompositionContainer(catalog, true, null);


            RouteTable.Routes.Add(new ServiceRoute("ProjectsService", new MefWebServiceHostFactory(CompositionContainer), typeof(IProjectsService)));
            RouteTable.Routes.Add(new ServiceRoute("BacklogService", new MefWebServiceHostFactory(CompositionContainer), typeof(IBacklogService)));
            RouteTable.Routes.Add(new ServiceRoute("TeamService", new MefWebServiceHostFactory(CompositionContainer), typeof(ITeamService)));
            RouteTable.Routes.Add(new ServiceRoute("TasksService", new MefWebServiceHostFactory(CompositionContainer), typeof(ITasksService)));
            RouteTable.Routes.Add(new ServiceRoute("ProposalsService", new MefWebServiceHostFactory(CompositionContainer), typeof(IProposalsService)));
            RouteTable.Routes.Add(new ServiceRoute("AuthorizationService", new MefWebServiceHostFactory(CompositionContainer), typeof(IAuthorizationService)));

            RouteTable.Routes.Add(new ServiceRoute("ArtifactsService", new MefWebServiceHostFactory(CompositionContainer), typeof(IArtifactsService)));

            RouteTable.Routes.Add(new ServiceRoute("CalendarService", new MefWebServiceHostFactory(CompositionContainer), typeof(ICalendarService)));
            
            RouteTable.Routes.Add(new ServiceRoute("ProjectConstraintsService", new MefWebServiceHostFactory(CompositionContainer), typeof(IProjectConstraintsService)));

            RouteTable.Routes.Add(new ServiceRoute("FactoryServerService", new MefWebServiceHostFactory(CompositionContainer), typeof(IFactoryServerService)));

            RouteTable.Routes.Add(new ServiceRoute("ReportService", new MefWebServiceHostFactory(CompositionContainer), typeof(IReportService)));

            RouteTable.Routes.MapPageRoute("SFOpenProject", "{projectNumber}", "~/open-project.aspx", false, new RouteValueDictionary(), new RouteValueDictionary() { { "projectNumber", "[0-9]+" }});
            RouteTable.Routes.MapPageRoute("SFOpenTask", "tasks/{taskNumber}", "~/open-task.aspx", false, new RouteValueDictionary(), new RouteValueDictionary() { { "taskNumber", "[0-9]+" } });

            ImportServerPlugins();


        }

        private void ImportServerPlugins() {
           
            System.Collections.Generic.IEnumerable<IScrumFactoryPluginService> plugins = CompositionContainer.GetExportedValues<IScrumFactoryPluginService>();
            foreach (IScrumFactoryPluginService plugin in plugins)
                RouteTable.Routes.Add(new ServiceRoute(plugin.ServiceName, new MefWebServiceHostFactory(CompositionContainer), plugin.GetType()));            

            
        }
      
        

    }
}