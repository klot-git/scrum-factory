namespace ScrumFactory.Services.Web
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    public class MefInstanceProvider : IInstanceProvider
    {
        private CompositionContainer container;
        private Type serviceType;

        public MefInstanceProvider(CompositionContainer container, Type serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
        {
         
            return this.container.GetExports(this.serviceType, null, null).SingleOrDefault().Value;            
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.container.GetExports(this.serviceType, null, null).SingleOrDefault().Value;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}