using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(IArtifactsService))]
    public class ArtifactsServiceClient : IArtifactsService {

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("ArtifactsServiceUrl")]
        private string serviceUrl { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        public ICollection<Artifact> GetArtifacts(string contextUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Artifacts/" + contextUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Artifact>>();
        }

        public int  AddArtifact(Artifact artifact) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Artifacts/"), new ObjectContent<Artifact>(artifact, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<int>();
        }

        public void UpdateArtifact(string contextUId, string artifactUId, Artifact artifact) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Artifacts/" + artifact.ContextUId + "/" + artifact.ArtifactUId), new ObjectContent<Artifact>(artifact, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public int RemoveArtifact(string contextUId, string artifactUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("Artifacts/" + contextUId + "/" + artifactUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<int>();
        }
    }
}
