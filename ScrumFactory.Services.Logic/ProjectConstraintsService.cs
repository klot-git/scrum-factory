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
    [Export(typeof(IProjectConstraintsService))]
    public class ProjectConstraintsService : IProjectConstraintsService {

        [Import]
        private Data.IProjectConstraintsRepository repository { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        
        [WebGet(UriTemplate = "Constraints/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<ProjectConstraint> GetProjectConstraints(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return repository.GetProjectConstraints(projectUId);
        }

        [WebInvoke(Method = "POST", UriTemplate = "Constraints/{projectUId}", RequestFormat = WebMessageFormat.Json)]
        public void AddProjectConstraint(string projectUId, ProjectConstraint constraint) {
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            repository.SaveProjectConstraint(constraint);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Constraints/{projectUId}", RequestFormat = WebMessageFormat.Json)]
        public void UpdateProjectConstraint(string projectUId, ProjectConstraint constraint) {
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
             repository.SaveProjectConstraint(constraint);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "Constraints/{projectUId}/{constraintUId}", RequestFormat = WebMessageFormat.Json)]
        public void RemoveProjectConstraint(string projectUId, string constraintUId) {
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            repository.DeleteProjectConstraint(constraintUId);
        }


        [WebGet(UriTemplate = "DefaultConstraints/", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<ProjectConstraint> GetDefaultContraints() {
            authorizationService.VerifyRequestAuthorizationToken();
            
            ProjectConstraint[] constraints = new ProjectConstraint[0];
            string path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "Templates\\default-constraints.xml";
            try {
                using (var f = new System.IO.FileStream(path, System.IO.FileMode.Open)) {
                    System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(f);
                    System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(ProjectConstraint[]));
                    constraints = (ProjectConstraint[])s.Deserialize(reader);
                }            
            } catch (Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
            }
            return constraints;
        }

        [WebGet(UriTemplate = "PointsFactor/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public double GetPointsFactor(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return repository.GetPointsFactor(projectUId);
        }


    }
}
