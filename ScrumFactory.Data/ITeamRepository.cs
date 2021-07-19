namespace ScrumFactory.Data {
    using System.Collections.Generic;

    public interface ITeamRepository {

        ICollection<MemberProfile> GetAllMembers(string filter, int availability, string[] companies, bool activeOnly, string workingWithUId, int top, bool includeProjects);

        ICollection<ScrumFactory.MemberProfile> GetTeamMembers(string teamCode);

        MemberProfile GetMember(string memberUId);


        MemberAvatar GetMemberAvatar(string memberUId);

        ICollection<MemberProfile> GetProjectMembers(string projectUId);

        void SaveMember(MemberProfile newMember);

        void SaveMemberAvatar(MemberAvatar avatar);

        void RemoveMemberAvatar(string memberUId);
        
        ICollection<MemberProfile> GetOwnersMembers();

        int GetOwnersMembersCount();

        ICollection<ProjectMembership> GetActiveProjectsFromUsers(string[] memberUIds);

    }
}
