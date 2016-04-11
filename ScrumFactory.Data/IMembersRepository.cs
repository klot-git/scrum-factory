namespace ScrumFactory.Data
{
    using System.Collections.Generic;

    public interface IMembersRepository
    {
        void RemoveProjectMember(string projectId, string memberId);

        void SaveProjectMember(ProjectMember member);

        ICollection<ProjectMember> GetMembersInProject(string projectId);

        ICollection<ProjectRole> GetAllRoles();
    }
}
