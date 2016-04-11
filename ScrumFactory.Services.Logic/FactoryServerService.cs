using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;

namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(IFactoryServerService))]
    public class FactoryServerService : IFactoryServerService {

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        [WebGet(UriTemplate = "Configuration", ResponseFormat = WebMessageFormat.Json)]        
        public ServerConfiguration GetConfiguration() {
            ServerConfiguration config = ReadFromConfig();
            return config;
        }

        [WebInvoke(Method = "POST", UriTemplate = "Configuration", RequestFormat = WebMessageFormat.Json)]
        public void UpdateConfiguration(ServerConfiguration config) {

            authorizationService.VerifyFactoryOwner();
            
        }

        private ServerConfiguration ReadFromConfig() {
            ServerConfiguration config = new ServerConfiguration();
            config.ScrumFactorySenderEmail = System.Configuration.ConfigurationManager.AppSettings["ScrumFactorySenderEmail"];
            config.ScrumFactorySenderName = System.Configuration.ConfigurationManager.AppSettings["ScrumFactorySenderName"];
            config.DefaultCompanyName = System.Configuration.ConfigurationManager.AppSettings["DefaultCompanyName"];
            config.TrustedEmailDomains = System.Configuration.ConfigurationManager.AppSettings["TrustedEmailDomains"];

            return config;
        }
    }
}
