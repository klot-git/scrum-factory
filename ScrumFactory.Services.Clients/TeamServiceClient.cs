using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(ITeamService))]
    public class TeamServiceClient : ITeamService {

        
        public TeamServiceClient() { }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("TeamServiceUrl")]
        private string serviceUrl { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("#", "@@@").Replace("\\", "@@").Trim();
        }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizationService { get; set; }


        public MemberProfile GetMember(string memberUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("Members/" + memberUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<MemberProfile>();
        }

        public MemberProfile GetMember(string providerName, string user) {
            throw new NotSupportedException();
        }

        public MemberAvatar GetMemberAvatar(string memberUId) {
            throw new NotSupportedException();
        }

        public void CreateMember(MemberProfile member) {
            throw new NotSupportedException();
        }

        public void CreateContactMember(MemberProfile member) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Post(Url("Members/Contacts"), new ObjectContent<MemberProfile>(member, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }


        public void UpdateMember(string memberUId, MemberProfile member) {
            var client = ClientHelper.GetClient(authorizationService); 
            HttpResponseMessage response = client.Put(Url("Members/" + SafeMemberUId(memberUId)), new ObjectContent<MemberProfile>(member, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

       

        public void UpdateMemberAvatar(string memberUId, MemberAvatar avatar) {
            var client = ClientHelper.GetClient(authorizationService); 
            HttpResponseMessage response = client.Put(Url("Members/" + SafeMemberUId(memberUId) + "/Avatar"), new ObjectContent<MemberAvatar>(avatar, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void RemoveMemberAvatar(string memberUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Delete(Url("Members/" + SafeMemberUId(memberUId) + "/Avatar"));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void UpdateMemberIsActive(string memberUId, bool isActive) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Put(Url("Members/" + SafeMemberUId(memberUId) + "/IsActive"), new ObjectContent<bool>(isActive, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public ICollection<MemberProfile> GetMembers(string filter, int availability, string clientName, bool activeOnly, string workingWith, int top) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("Members/?filter=" + filter + "&availability=" + availability + "&clientName=" + clientName + "&activeOnly=" + activeOnly + "&workingWith=" + workingWith + "&top=" + top));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<MemberProfile>>();
        }

        public ICollection<ScrumFactory.MemberProfile> GetTeamMembers(string teamCode, bool includeTasks, bool excludeMe, int top = 25, int topTasks = 10) {
            var client = ClientHelper.GetClient(authorizationService);
            if (string.IsNullOrEmpty(teamCode))
                teamCode = "__NONE__";
            HttpResponseMessage response = client.Get(Url("TeamMembers/" + teamCode + "/?includeTasks=" + includeTasks + "&excludeMe=" + excludeMe + "&top=" + top + "&topTasks=" + topTasks));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<MemberProfile>>();
        }

        public ICollection<MemberProfile> GetContacts(string clientName) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("Members/Contacts/" + clientName));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<MemberProfile>>();
        }

        public ICollection<MemberProfile> GetProjectMembers(string projectUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("ProjectMembers/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<MemberProfile>>();
        }

     

        public ICollection<MemberProfile> GetOwnersMembers() {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("OwnersMembers"));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<MemberProfile>>();
        }

        public void ChangeMemberIsFactoryOwner(string memberUId, bool isOwner) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Put(Url("Members/" + SafeMemberUId(memberUId) + "/IsFactoryOnwer"), new ObjectContent<bool>(isOwner, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeMemberCanSeeProposals(string memberUId, bool canSee) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Put(Url("Members/" + SafeMemberUId(memberUId) + "/CanSeeProposalValues"), new ObjectContent<bool>(canSee, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }


    }
}
