using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;


namespace ScrumFactory.Services.Clients {
    
    [Export(typeof(IAuthorizationService))]
    public class AuthorizationServiceClient : IAuthorizationService {

        public AuthorizationServiceClient() {
        }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        public IServerUrl ServerUrl { get; private set; }

        [Import("AuthorizationServiceUrl")]
        private string serviceUrl { get; set; }
        
        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", ServerUrl.Url) + relative);
        }

        public string ServerVersion {
            get {
                throw new NotSupportedException();
            }
        }

        private string ConvertEncode(string str) {
            System.Text.StringBuilder str2 = new System.Text.StringBuilder();

            for (int i = 0; i < str.Length; ++i) {
                switch (str[i]) {
                    case '#':
                    case '\\':
                    case '/':
                    case '@':
                    case '?':
                        str2.AppendFormat("%{0:X}", (int)str[i]);
                        break;
                    default:
                        str2.Append(str[i]);
                        break;
                }
            }
            return str2.ToString();
        }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("#", "@@@").Replace("\\", "@@").Trim();
        }

        public MemberProfile SignInMember(string providerName, string token, string memberUId = null) {
            
            var client = ClientHelper.GetClient();

            if (memberUId == null)
                memberUId = String.Empty;

            HttpResponseMessage response = client.Post(Url("ValidTokens/" + providerName + "/?memberUId=" + SafeMemberUId(memberUId)), new ObjectContent<string>(token, JsonMediaTypeFormatter.DefaultMediaType));

            if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
                throw new ScrumFactory.Exceptions.AuthorizationProviderNotSupportedException();

            ClientHelper.HandleHTTPErrorCode(response);            
            if(response.Content.ContentReadStream.Length==0)
                return null;

            MemberProfile member = response.Content.ReadAs<MemberProfile>();
            
            SignedMemberToken = token;
            SignedMemberProfile = member;

            return SignedMemberProfile;
            
        }

        public void SignOutMember(string providerName) {
            
            var client = ClientHelper.GetClient(this);
            HttpResponseMessage response = client.Delete(Url("ValidTokens/" + providerName + "/"));
            ClientHelper.HandleHTTPErrorCode(response);            
        
            SignedMemberToken = null;
            SignedMemberProfile = null;
        }

        public MemberProfile SignedMemberProfile { get; private set; }

        public string SignedMemberToken { get; private set; }


        public void VerifyUser(string memberUId) {
            if (SignedMemberProfile == null)
                throw new ScrumFactory.Exceptions.ForbittenException();

            if (memberUId != SignedMemberProfile.MemberUId)
                throw new ScrumFactory.Exceptions.ForbittenException();
        }

        public void VerifyFactoryOwner() {
            if( SignedMemberProfile==null)
                throw new ScrumFactory.Exceptions.ForbittenException();
            if(!SignedMemberProfile.IsFactoryOwner)
                throw new ScrumFactory.Exceptions.ForbittenException();
        }

        public void VerifyCanSeeProposalValues() {
            if (SignedMemberProfile == null)
                throw new ScrumFactory.Exceptions.ForbittenException();
            if (!SignedMemberProfile.CanSeeProposalValues)
                throw new ScrumFactory.Exceptions.ForbittenException();
        }

        public void VerifyRequestAuthorizationToken() {
            if (SignedMemberProfile == null)
                throw new ScrumFactory.Exceptions.ForbittenException();

        }


        public void VerifyPermissionAtProject(string projectUId, PermissionSets permission) {
            throw new NotSupportedException();
        }


        public void VerifyPermissionAtProject(string projectUId, PermissionSets[] permissions) {
            throw new NotSupportedException();
        }

        public void VerifyPermissionAtProjectOrFactoryOwner(string projectUId, PermissionSets[] permissions) {
            throw new NotSupportedException();
        }

        public void VerifyUserOrPermissionAtProject(string memberUId, string projectUId, PermissionSets permission) {
            throw new NotSupportedException();
        }

        public bool IsProjectScrumMaster(string projectUId) {
            throw new NotSupportedException();
        }
                        
    }
}
