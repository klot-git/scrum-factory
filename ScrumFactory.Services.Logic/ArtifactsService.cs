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
    [Export(typeof(IArtifactsService))]    
    public class ArtifactsService : IArtifactsService {

        [Import]
        private Data.IArtifactRepository artifactRepository { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        [Import]
        private ITasksService_ServerSide taskServices { get; set; }

        [Import]
        private IBacklogService_ServerSide backlogServices { get; set; }

        [WebGet(UriTemplate = "Artifacts/{contextUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Artifact> GetArtifacts(string contextUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return artifactRepository.GetArtifacts(contextUId);            
        }

        [WebInvoke(Method = "POST", UriTemplate = "Artifacts/", ResponseFormat = WebMessageFormat.Json)]
        public int AddArtifact(Artifact artifact) {
            VerifyPermision(artifact);
            artifactRepository.SaveArtifact(artifact);
            int count = UpdateCounters(artifact);
            return count;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Artifacts/{contextUId}/{artifactUId}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateArtifact(string contextUId, string artifactUId, Artifact artifact) {
            if (artifactUId != artifact.ArtifactUId || contextUId != artifact.ContextUId)
                throw new WebFaultException(System.Net.HttpStatusCode.BadRequest);            
            VerifyPermision(artifact);         
            artifactRepository.SaveArtifact(artifact);
        }
        
        [WebInvoke(Method = "DELETE", UriTemplate = "Artifacts/{contextUId}/{artifactUId}", ResponseFormat = WebMessageFormat.Json)]
        public int RemoveArtifact(string contextUId, string artifactUId) {

            Artifact artifact = artifactRepository.GetArtifact(artifactUId);

            if (artifact == null)
                throw new WebFaultException(System.Net.HttpStatusCode.NotFound);

            VerifyPermision(artifact);

            artifactRepository.RemoveArtifact(artifactUId);
            int count = UpdateCounters(artifact);
            return count;
        }


        private void VerifyPermision(Artifact artifact) {
            // if is a task artifact, then task owner can add
            if (artifact.ArtifactContext == (short)ArtifactContexts.TASK_ARTIFACT) {
                Task task = taskServices.GetTask(artifact.ContextUId);
                if (task == null)
                    throw new WebFaultException(System.Net.HttpStatusCode.BadRequest);
                authorizationService.VerifyUserOrPermissionAtProject(task.TaskOwnerUId, artifact.ProjectUId, PermissionSets.SCRUM_MASTER);
            } else
                authorizationService.VerifyPermissionAtProject(artifact.ProjectUId, PermissionSets.SCRUM_MASTER);

        }

        private int UpdateCounters(Artifact artifact) {

            int count = artifactRepository.GetArtifactContextCount(artifact.ContextUId);
            
            if (artifact.ArtifactContext == (short)ArtifactContexts.BACKLOGITEM_ARTIFACT)
                backlogServices.UpdateBacklogItemArtifactCount(artifact.ContextUId, count);

            if (artifact.ArtifactContext == (short)ArtifactContexts.TASK_ARTIFACT)
                taskServices.UpdateTaskArtifactCount(artifact.ContextUId, count);
                
            return count;
        }
        
    }
}
