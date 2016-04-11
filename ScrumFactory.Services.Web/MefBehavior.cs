namespace ScrumFactory.Services.Web
{
    using System.ComponentModel.Composition.Hosting;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class MefBehavior : IServiceBehavior
    {
        private CompositionContainer container;
        private System.Type implementationType;
        private System.Type serviceType;

        public MefBehavior(CompositionContainer container, System.Type implementationType, System.Type serviceType)
        {
            this.container = container;
            this.implementationType = implementationType;
            this.serviceType = serviceType;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ErrorHandler onError = new ErrorHandler();

            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                channelDispatcher.ErrorHandlers.Add(onError);

                if (channelDispatcher != null)
                {
                    foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                    {                        
                        endpointDispatcher.DispatchRuntime.InstanceProvider = new MefInstanceProvider(this.container, this.serviceType);                        
                        
                    }
                }
            }


            //foreach (ServiceEndpoint e in serviceDescription.Endpoints)
            //    e.Binding = new NetTcpBinding();
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}