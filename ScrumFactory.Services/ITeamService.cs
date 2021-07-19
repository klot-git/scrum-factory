namespace ScrumFactory.Services {

    using System.ServiceModel;
    using System.Collections.Generic;

    [ServiceContract()]
    public interface ITeamService {

        [OperationContract]
        MemberProfile GetMember(string memberUId);

        
        MemberAvatar GetMemberAvatar(string memberUId);

        
        void CreateMember(MemberProfile member);

        [OperationContract]
        void CreateContactMember(MemberProfile member);

        [OperationContract]
        void UpdateMember(string memberUId, MemberProfile member);

        [OperationContract]
        void UpdateMemberAvatar(string memberUId, MemberAvatar avatar);

        [OperationContract]
        void RemoveMemberAvatar(string memberUId);

        [OperationContract]
        void UpdateMemberIsActive(string memberUId, bool isActive);

        [OperationContract]
        ICollection<MemberProfile> GetMembers(string filter, int availability, string clientName, bool activeOnly, string workingWith, int top, bool includeProjects);

        [OperationContract]
        ICollection<ScrumFactory.MemberProfile> GetTeamMembers(string teamCode, bool includeTasks, bool excludeMe, int top = 25, int topTasks = 10);

        [OperationContract]
        ICollection<MemberProfile> GetContacts(string clientName);

        [OperationContract]
        ICollection<MemberProfile> GetProjectMembers(string projectUId);

        

        [OperationContract]
        ICollection<MemberProfile> GetOwnersMembers();

        [OperationContract]
        void ChangeMemberIsFactoryOwner(string memberUId, bool isOwner);

        [OperationContract]
        void ChangeMemberCanSeeProposals(string memberUId, bool canSee);


        
    }
}
