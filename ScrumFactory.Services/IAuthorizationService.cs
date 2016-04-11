using System;
using System.Collections.Generic;
using System.ServiceModel;
namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IAuthorizationService {

        [OperationContract]
        MemberProfile SignInMember(string providerName, string token, string memberUId = null);

        [OperationContract]
        void SignOutMember(string providerName);
             
        void VerifyRequestAuthorizationToken();
                
        MemberProfile SignedMemberProfile { get; }
                
        string SignedMemberToken { get; }

        bool IsProjectScrumMaster(string projectUId);

        void VerifyUser(string memberUId);

        void VerifyPermissionAtProject(string projectUId, PermissionSets permission);

        void VerifyPermissionAtProject(string projectUId, PermissionSets[] permissions);

        void VerifyPermissionAtProjectOrFactoryOwner(string projectUId, PermissionSets[] permissions);

        void VerifyFactoryOwner();

        void VerifyCanSeeProposalValues();

        void VerifyUserOrPermissionAtProject(string memberUId, string projectUId, PermissionSets permission);

        string ServerVersion { get; }

        IServerUrl ServerUrl { get; }

     
    }
}
