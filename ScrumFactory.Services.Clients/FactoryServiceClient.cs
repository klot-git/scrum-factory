using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(IFactoryServerService))]
    public class FactoryServiceClient : IFactoryServerService {

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("FactoryServerServiceUrl")]
        private string serviceUrl { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }


        public ServerConfiguration GetConfiguration() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Configuration"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ServerConfiguration>();                        
        }

        public void UpdateConfiguration(ServerConfiguration config) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Configuration"), new ObjectContent<ServerConfiguration>(config, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            return;             
        }
    }
}
