namespace ScrumFactory.Data
{
    using System.Collections.Generic;

    public interface IProjectRepository
    {


        ICollection<Project> GetAllProjects(string memberUId, System.DateTime startDate, System.DateTime endDate, string filter = null, int top = 0, int skip = 0);

        ICollection<Project> GetClosedProjects(string memberUId, System.DateTime startDate, System.DateTime endDate, string filter = null, int top = 0, int skip = 0);

        ICollection<Project> GetOpenProjects(string memberUId, string filter = null, int top = 0, int skip = 0);

        ICollection<ScrumFactory.Project> GetRunningProjects(string memberUId, short? pType = null);

        ICollection<ScrumFactory.Project> GetEngagedProjects(string memberUId);

        ICollection<ScrumFactory.Project> GetPendingEngagementProjects(string memberUId);

        Project GetProject(string projectUId);
        
        Project GetProjectByNumber(int projectNumber);

        int InsertProject(Project project);

        void UpdateProject(Project project);

        ICollection<Role> GetProjectRoles(string projectUId);

        Role GetProjectRole(string roleUId);

        bool CanDeleteProjectRole(string projectUId, string roleUId);

        void DeleteProjectRole(string roleUId);

        void SaveProjectRole(Role role);

        ICollection<Sprint> GetSprints(string projectId);

        Sprint GetSprint(string sprintUId);

        void SaveSprint(Sprint sprint);

        void DeleteSprint(string sprintUId);

        ICollection<string> GetClientNames();

        ICollection<string> GetPlatforms();

        Project GetLastSimilarProject(Project project, bool onlyWithProposals = false);

        ICollection<Risk> GetProjectRisks(string projectUId);

        Risk GetRisk(string riskUId);

        void SaveRisk(Risk risk);

        ICollection<ProjectEvent> GetUserProjectEvents(string memberUId);

        int GetMembershipCountOf(string projectUId, PermissionSets permissionSet);

        bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets permission);

        bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets[] permissions);

        int GetMemberDayAllocation(string memberUId);

        string[] GetSimilarCodeRepositories(string projectName, string clientName);

    }
}
