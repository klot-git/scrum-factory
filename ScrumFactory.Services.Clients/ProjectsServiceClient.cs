using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {    

    [Export(typeof(IProjectsService))]
    public class ProjectsServiceClient : IProjectsService {
       
        public ProjectsServiceClient() { }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("ProjectsServiceUrl")]
        private string serviceUrl { get; set; }

        public IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string memberUId, string projectUId) {
            TasksServiceClient tService = new TasksServiceClient();
            return tService.GetProjectTotalEffectiveHoursByMember(projectUId, memberUId);
        }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("#", "@@@").Replace("\\", "@@").Trim();
        }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        public ICollection<Project> GetProjects(string startDate, string endDate, string filter, string memberUId, string tagFilter = null, int top = 0, int skip = 0) {            

            if (startDate == null)
                startDate = String.Empty;
            if (endDate == null)
                endDate = string.Empty;

            var client = ClientHelper.GetClient(authorizator);
            
            HttpResponseMessage response = client.Get(Url("Projects/?startDate=" + startDate + "&endDate=" + endDate + "&filter=" + filter + "&memberUId=" + memberUId + "&tagFilter=" + tagFilter + "&top=" + top + "&skip=" + skip));
            
            ClientHelper.HandleHTTPErrorCode(response);           
            return response.Content.ReadAs<ICollection<Project>>();                        
        }

        public ICollection<Project> GetPendingEngagementProjects() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("PendingEngagementProjects/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Project>>();                        
        }

        public Project GetProject(string projectUId) {

            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Projects/" + projectUId));

            ClientHelper.HandleHTTPErrorCode(response);

            Project p = response.Content.ReadAs<Project>();

            // need to fix the references, i did not figure out how to do this in a better way
            if (p != null) {
                foreach (Sprint s in p.Sprints)
                    s.Project = p;
            }
            return p;
        }

        public ICollection<Role> GetProjectRoles(string projectUId) {

            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Projects/" + projectUId.Trim() + "/Roles"));

            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Role>>();
        }

        public void CreateProjectRole(string projectUId, Role role) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Projects/" + role.ProjectUId.Trim() + "/Roles"), new ObjectContent<Role>(role, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void UpdateProjectRole(string projectUId, string roleUId, Role role) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + role.ProjectUId.Trim() + "/Roles/" + roleUId.Trim()), new ObjectContent<Role>(role, JsonMediaTypeFormatter.DefaultMediaType));                        
            ClientHelper.HandleHTTPErrorCode(response);
        }


        public void DeleteProjectRole(string projectUId, string roleUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("Projects/" + projectUId.Trim() + "/Roles/" + roleUId));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public int CreateProject(Project project, bool useLastProjectAsModel) {      
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Projects/?useLastProjectAsModel=" + useLastProjectAsModel.ToString()), new ObjectContent<Project>(project, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<int>();
        }


        public void UpdateProject(string projectUId, Project project) {

            // clones the project and get rid off the itens and sprint
            // just to save bandwidth
            Project onlyProject = project.Clone();            
            onlyProject.Sprints = null;

            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId.Trim()), new ObjectContent<Project>(onlyProject, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public Project ChangeProjectStatus(string projectUId, string option, short status) {

            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Status/?option=" + option), new ObjectContent<short>(status, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
            Project p = response.Content.ReadAs<Project>();

            // need to fix the references, i did not figure out how to do this in a better way
            foreach (Sprint s in p.Sprints)
                s.Project = p;

            return p;
        }

        
        

        
        public ICollection<BacklogItem> AddSprint(string projectUId, Sprint sprint, bool addDefaultItems = false, string planItemName = null, string planGroupName = null, string deliveryItemName = null, string deliveryGroupName = null) {
            var client = ClientHelper.GetClient(authorizator);
            string param = String.Format("?addDefaultItems={0}&planItemName={1}&planGroupName={2}&deliveryItemName={3}&deliveryGroupName={4}", addDefaultItems, planItemName, planGroupName, deliveryItemName, deliveryGroupName);
            HttpResponseMessage response = client.Post(Url("Projects/" + projectUId.Trim() + "/Sprints" + param), new ObjectContent<Sprint>(sprint, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItem>>();
        }
                
        public ICollection<Sprint> UpdateSprint(string projectUId, string sprintUId, Sprint sprint) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId.Trim() + "/Sprints/" + sprintUId.Trim()), new ObjectContent<Sprint>(sprint, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Sprint>>();
        }

        public ICollection<Sprint> GetSprints(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Projects/" + projectUId.Trim() + "/Sprints"));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Sprint>>();

        }

        public void RemoveSprint(string projectUId, string sprintUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("Projects/" + projectUId.Trim() + "/Sprints/" + sprintUId));            
            ClientHelper.HandleHTTPErrorCode(response);         
        }

        public ICollection<string> GetClientNames() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Clients"));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<string>>();
        }

        public ICollection<string> GetPlatforms() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Platforms"));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<string>>();
        }

        public Project GetLastSimilarProject(string projectUId, bool onlyWithProposals = false) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("SimilarProjects/" + projectUId + "/?onlyWithProposals=" + onlyWithProposals));
            ClientHelper.HandleHTTPErrorCode(response);            
            return response.Content.ReadAs<Project>();
        }

        public ICollection<Risk> GetProjectRisks(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Projects/" + projectUId + "/Risks"));
            ClientHelper.HandleHTTPErrorCode(response);            
            return response.Content.ReadAs<ICollection<Risk>>();
        }

        public void AddRisk(string projectUId, Risk risk) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Projects/" + projectUId.Trim() + "/Risks"), new ObjectContent<Risk>(risk, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void UpdateRisk(string projectUId, string riskUId, Risk risk) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId.Trim() + "/Risks/" + riskUId.Trim()), new ObjectContent<Risk>(risk, JsonMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeRiskImpact(string projectUId, string riskUId, short impact) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Risks/" + riskUId + "/Impact"), new ObjectContent<short>(impact, JsonMediaTypeFormatter.DefaultMediaType));                        
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ChangeRiskProbability(string projectUId, string riskUId, short probability) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Risks/" + riskUId + "/Probability"), new ObjectContent<short>(probability, JsonMediaTypeFormatter.DefaultMediaType));                        
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeProjectIsSuspended(string projectUId, bool isSuspended) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/IsSuspended"), new ObjectContent<bool>(isSuspended, JsonMediaTypeFormatter.DefaultMediaType));                        
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public ICollection<ProjectEvent> GetUserProjectEvents() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("UserProjectEvents"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<ProjectEvent>>();
        
        }

        public void AddProjectMembership(string projectUId, ProjectMembership membership) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Projects/" + projectUId + "/Memberships"), new ObjectContent<ProjectMembership>(membership, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void UpdateProjectMembershipAllocation(string projectUId, string memberUId, string roleUId, int dayAllocation) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Memberships/" + memberUId + "/Roles/" +roleUId + "/DayAllocation"), new ObjectContent<int>(dayAllocation, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void RemoveProjectMembership(string projectUId, string memberUId, string roleUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("Projects/" + projectUId + "/Memberships/" + memberUId + "/Roles/" + roleUId));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        

        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets[] permissions) {
            throw new NotSupportedException();
        }

        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets permission) {
            throw new NotSupportedException();
        }

        public ICollection<string> GetSimilarCodeRepositories(string projectName, string clientName) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("SimilarCodeRepositories/?projectName=" + projectName + "&clientName=" + clientName));
            ClientHelper.HandleHTTPErrorCode(response, true);
            if (!response.IsSuccessStatusCode)
                return new List<string>();
            return response.Content.ReadAs<ICollection<string>>();
        }


        public void SetCodeRepositoryPath(string projectUId, string svnUrl) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("Projects/" + projectUId.Trim() + "/CodeRepositoryPath/"), new ObjectContent<string>(svnUrl, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public SVNSettings GetSVNSettings() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("SVNSettings/"));
            ClientHelper.HandleHTTPErrorCode(response, true);            
            return response.Content.ReadAs<SVNSettings>();
        }

        public void SetProjectBaseline(string projectUId, int? baseline) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Baseline"), new ObjectContent<int?>(baseline, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        
        //public void ChangeProjectMembershipIsActive(string projectUId, string memberUId, string roleUId, bool isActive) {
        //    var client = ServiceClient.GetClient(authorizator);
        //    HttpResponseMessage response = client.Put(Url("Projects/" + projectUId + "/Memberships/" + memberUId + "/Roles/" + roleUId + "/IsActive"), new ObjectContent<bool>(isActive, JsonMediaTypeFormatter.DefaultMediaType));
        //    ServiceClient.HandleHTTPErrorCode(response);
        //}

    }
}
