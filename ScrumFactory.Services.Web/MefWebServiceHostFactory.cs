namespace ScrumFactory.Services.Web
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    
    
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;


    public class MefWebServiceHostFactory : WebServiceHostFactory
    {
        private CompositionContainer container;

        public MefWebServiceHostFactory(CompositionContainer container)
        {
            this.container = container;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            // This returns a tuple
            var exports = container.Catalog.GetExports(new ImportDefinition(
                e => MatchesContract(e, serviceType), null, ImportCardinality.ExactlyOne, true, false)).SingleOrDefault();

            // exports.Item1.ToString() returns the name of the exporting type
            var implementationTypeName = exports.Item1.ToString();

            // look into all loaded assemblies for the implementation type
            var implementationType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.FullName == implementationTypeName).SingleOrDefault();

            var host = base.CreateServiceHost(
                implementationType,
                baseAddresses);

            host.Description.Behaviors.Add(new MefBehavior(this.container, implementationType, serviceType));

            ServiceThrottlingBehavior throt = new ServiceThrottlingBehavior();
            int maxConcurrentCalls;
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["WCF_MaxConcurrentCalls"], out maxConcurrentCalls))
                throt.MaxConcurrentCalls = maxConcurrentCalls;

            
            
            host.Description.Behaviors.Add(throt);
           

            return host;
        }

        private bool MatchesContract(ExportDefinition e, Type serviceType)
        {
            return e.Metadata["ExportTypeIdentity"].ToString() == serviceType.FullName;
        }
    }
}