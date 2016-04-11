using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;
using System.Linq;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(IProjectConstraintsService))]
    public class ProjectConstraintsServiceClient : IProjectConstraintsService {

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("ProjectConstraintsServiceUrl")]
        private string serviceUrl { get; set; }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        public ICollection<ProjectConstraint> GetProjectConstraints(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Constraints/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);            
            return response.Content.ReadAs<ICollection<ProjectConstraint>>();
        }

        public ICollection<ProjectConstraint> GetDefaultContraints() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("DefaultConstraints/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<ProjectConstraint>>();
        }

        public double GetPointsFactor(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("PointsFactor/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<double>();
        }

        public void AddProjectConstraint(string projectUId, ProjectConstraint constraint) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Constraints/" + projectUId), new ObjectContent<ProjectConstraint>(constraint, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void UpdateProjectConstraint(string projectUId, ProjectConstraint constraint) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Constraints/" + projectUId), new ObjectContent<ProjectConstraint>(constraint, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void RemoveProjectConstraint(string projectUId, string constraintUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("Constraints/" + projectUId + "/" + constraintUId));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        
    }
}
