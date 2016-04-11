namespace ScrumFactory.Services
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract()]
    public interface IProjectsService {

        IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string memberUId, string projectUId);

        
        [OperationContract]
        ICollection<Project> GetProjects(string startDate, string endDate, string filter, string memberUId, string tagFilter = null, int top = 0, int skip = 0);

        [OperationContract]
        ICollection<Project> GetPendingEngagementProjects();

        [OperationContract]
        Project GetProject(string projectUId);

        [OperationContract]
        ICollection<Role> GetProjectRoles(string projectUId);

        [OperationContract]
        void UpdateProjectRole(string projectUId, string roleUId, Role role);

        [OperationContract]
        void DeleteProjectRole(string projectUId, string roleUId);

        [OperationContract]
        void CreateProjectRole(string projectUId, Role role);

        [OperationContract]
        int CreateProject(Project project, bool useLastProjectAsModel);

        [OperationContract]
        void UpdateProject(string projectUId, Project project);


        [OperationContract]
        Project ChangeProjectStatus(string projectUId, string option, short status);

        [OperationContract]
        ICollection<BacklogItem> AddSprint(string projectUId, Sprint sprint, bool addDefaultItems = false, string planItemName = null, string planGroupName = null, string deliveryItemName = null, string deliveryGroupName = null);

        [OperationContract]
        ICollection<Sprint> UpdateSprint(string projectUId, string sprintUId, Sprint sprint);

        [OperationContract]
        ICollection<Sprint> GetSprints(string projectUId);

        [OperationContract]
        void RemoveSprint(string projectUId, string sprintUId);

        
        [OperationContract]
        ICollection<string> GetClientNames();

        [OperationContract]
        ICollection<string> GetPlatforms();

        [OperationContract]
        Project GetLastSimilarProject(string projectUId, bool onlyWithProposals);

        [OperationContract]
        ICollection<Risk> GetProjectRisks(string projectUId);

        [OperationContract]
        void AddRisk(string projectUId, Risk risk);

        [OperationContract]
        void UpdateRisk(string projectUId, string riskUId, Risk risk);

        [OperationContract]
        void ChangeRiskImpact(string projectUId, string riskUId, short impact);

        [OperationContract]
        void ChangeRiskProbability(string projectUId, string riskUId, short probability);

        [OperationContract]
        void ChangeProjectIsSuspended(string projectUId, bool isSuspended);

        [OperationContract]
        ICollection<ProjectEvent> GetUserProjectEvents();

        [OperationContract]
        void AddProjectMembership(string projectUId, ProjectMembership membership);

        [OperationContract]
        void UpdateProjectMembershipAllocation(string projectUId, string memberUId, string roleUId, int dayAllocation);

        [OperationContract]
        void RemoveProjectMembership(string projectUId, string memberUId, string roleUId);

        bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets[] permissions);

        bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets permission);

        [OperationContract]
        ICollection<string> GetSimilarCodeRepositories(string projectName, string clientName);

        [OperationContract]
        void SetCodeRepositoryPath(string projectUId, string svnTrunk);

        [OperationContract]
        SVNSettings GetSVNSettings();

        [OperationContract]
        void SetProjectBaseline(string projectUId, int? baseline);
    }
}
