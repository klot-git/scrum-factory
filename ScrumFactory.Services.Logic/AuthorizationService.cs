using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Collections.Concurrent;



namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(IAuthorizationService))]
    public class AuthorizationService : IAuthorizationService {

        private static readonly ConcurrentDictionary<string, AuthorizationInfo> authorizedTokens = new ConcurrentDictionary<string, AuthorizationInfo>();

        [Import(typeof(Data.IAuthorizationRepository))]
        private Data.IAuthorizationRepository authorizationRepository { get; set; }

        [Import(typeof(ITeamService))]
        private ITeamService teamService { get; set; }

        [Import(typeof(IProjectsService))]
        private IProjectsService projectsService { get; set; }

        [ImportMany(typeof(Services.AuthProviders.IOAuthServerTokenValidator))]
        private IEnumerable<Services.AuthProviders.IOAuthServerTokenValidator> authorizationProviders { get; set; }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("@@@", "#").Replace("@@", "\\").Trim();
        }

        public string ServerVersion {
            get {
                return "3.2";
            }
        }

 
        
        [WebInvoke(Method = "POST", UriTemplate = "ValidTokens/{providerName}/?memberUId={memberUId}", RequestFormat = WebMessageFormat.Json)]
        public MemberProfile SignInMember(string providerName, string token, string memberUId = null) {

            // if the token is not authenticated yet, validate it
            AuthorizationInfo authInfo = null;
            MemberProfile memberInfo = null;
            authorizedTokens.TryGetValue(token, out authInfo);

            if (authInfo == null) {
                authInfo = authorizationRepository.GetAuthorizationInfo(token);
            }
            
            // this will thrown an exception if the token is not valid
            if (authInfo == null) {
                memberInfo = CreateAuthorizationForToken(providerName, token);

                // GitHub may not provide the user email, so set it here
                if (memberInfo.MemberUId == null)
                    memberInfo.MemberUId = SafeMemberUId(memberUId);

                authorizedTokens.TryGetValue(token, out authInfo);
            }
                        
            // token was not valid
            if (authInfo == null)
                throw new WebFaultException<string>("Could not validate token", System.Net.HttpStatusCode.Unauthorized);

         
            // gets the user, or create anew one if was first time
            MemberProfile member = GetOrCreateMember(authInfo.MemberUId, memberInfo);
            if (member == null)
                return null;
     

            return member;
        }
        
    

        public void VerifyRequestAuthorizationToken() {
            
            string token = SignedMemberToken;

            if (!IsAuthorizationTokenValid(token))
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

        }

        private void GetTokenFromRequest(out string providerName, out string token) {
            string auth = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if (auth == null)
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            string[] parts = auth.Split('=');
            if (parts.Length < 2)
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            providerName = parts[0].Replace("auth","").Trim();
            token = auth.Replace(providerName + " auth=", "").Trim();

        }


        [WebInvoke(Method = "DELETE", UriTemplate = "ValidTokens/{providerName}/", RequestFormat = WebMessageFormat.Json)]
        public void SignOutMember(string providerName) {         
            AuthorizationInfo info = null;
            authorizedTokens.TryRemove(SignedMemberToken, out info);                     
        }



    

        public MemberProfile SignedMemberProfile {
            get {
                string token = SignedMemberToken;
                if (!IsAuthorizationTokenValid(token))
                    throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

                AuthorizationInfo info = null;
                authorizedTokens.TryGetValue(SignedMemberToken, out info);
                if(info==null)
                    throw new WebFaultException<string>("Could not find auth token info", System.Net.HttpStatusCode.BadRequest);

                return teamService.GetMember(info.MemberUId);
            }
        }

        public string SignedMemberToken {
            get {
                string provider;
                string token;
                GetTokenFromRequest(out provider, out token);
                return token;
            }
        }

        private Services.AuthProviders.IOAuthServerTokenValidator GetAuthorizationProvider(string providerName) {
            return authorizationProviders.SingleOrDefault(p => p.ProviderName == providerName);
        }

        private bool CanNewMemberSeeProposals {
            get {
                var canThey = System.Configuration.ConfigurationManager.AppSettings["CanNewMemberSeeProposals"];
                if (canThey == null)
                    return false;
                bool canTheyB;
                bool.TryParse(canThey, out canTheyB);
                return canTheyB;
            }
        }

        private string DefaultCompanyName {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["DefaultCompanyName"];
            }
        }

        private string[] TrustedDomains {
            get {
                string domainStr = System.Configuration.ConfigurationManager.AppSettings["TrustedDomains"];
                if (String.IsNullOrEmpty(domainStr))
                    return new string[0];
                string[] domains = domainStr.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < domains.Length; i++ ) {
                    domains[i] = domains[i].Trim();
                    if (domains[i].StartsWith("@"))
                        domains[i] = domains[i].Substring(1);
                }
                return domains;
            }
        }

        private bool IsMemberAtTrustedDomain(string memberUId) {
            
            // if is allowed any domain
            if (TrustedDomains.Contains("*"))
                return true;

            // if its not an email, its not a trusted domain
            int idx = memberUId.LastIndexOf('@');
            if (idx < 0)
                return false;

            string memberDomain = memberUId.Substring(idx + 1).ToLower();
            return TrustedDomains.Contains(memberDomain);
        }

        private MemberProfile GetOrCreateMember(string memberUId, MemberProfile memberInfo) {

            if (String.IsNullOrEmpty(memberUId))
                throw new WebFaultException<string>("Could not get or create member. MemberUId is null", HttpStatusCode.BadRequest);
                

            MemberProfile member = teamService.GetMember(memberUId);

            // is the member does not exist, create it
            if (member == null) {

                if(!IsMemberAtTrustedDomain(memberUId))
                    throw new WebFaultException<string>("This server does not allow self-sign up from not trusted domains", HttpStatusCode.BadRequest);

                string name = memberUId;
                string email = null;
                
                if (memberInfo != null) {
                    email = memberInfo.EmailAccount;
                    if(memberInfo.FullName!=null) name = memberInfo.FullName;
                }

                member = new MemberProfile();
                member.MemberUId = memberUId;
                member.FullName = name;
                member.EmailAccount = email;
                member.IsFactoryOwner = false;
                member.CanSeeProposalValues = CanNewMemberSeeProposals;
                member.CompanyName = DefaultCompanyName;
                member.IsActive = true;
                member.AuthorizationProvider = "-DEPRECATED-";                
                teamService.CreateMember(member);
            }
           
            return member;
                
        }

        private MemberProfile CreateAuthorizationForToken(string providerName, string token) {

            // no provider to validate the token
            Services.AuthProviders.IOAuthServerTokenValidator provider = GetAuthorizationProvider(providerName);
            if (provider == null)
                throw new WebFaultException<string>("Provider not informed", System.Net.HttpStatusCode.BadRequest);

            // not a valid token
            if (!provider.ValidateAccessToken(token))
                throw new WebFaultException<string>("Token not Valid", System.Net.HttpStatusCode.Unauthorized);

            // so the token is valid, at it the valid tokens
            AuthorizationInfo info = new AuthorizationInfo();
            info.ProviderName = providerName;
            info.MemberUId = provider.MemberInfo.MemberUId;
            info.Token = token.Trim().Replace("\n", "");
            info.IssueDate = DateTime.Now;

            authorizedTokens.TryAdd(info.Token, info);

            // Dont wanna save google/live tokens
            //if (info.ProviderName == "Windows Authentication")
            authorizationRepository.SaveAuthorizationInfo(info);

            return provider.MemberInfo;
        }

        
        public void VerifyUser(string memberUId) {

            if (!authorizedTokens.ContainsKey(SignedMemberToken))
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            if(SignedMemberProfile.MemberUId!=memberUId)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        public void VerifyUserOrPermissionAtProject(string memberUId, string projectUId, PermissionSets permission) {
            if (!projectsService.MemberHasPermissionAtProject(SignedMemberProfile.MemberUId, projectUId, new PermissionSets[] {  permission })
                && SignedMemberProfile.MemberUId != memberUId)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        public void VerifyPermissionAtProject(string projectUId, PermissionSets permission) {
            PermissionSets[] ps = new PermissionSets[1];
            ps[0] = permission;
            VerifyPermissionAtProject(projectUId, ps);
        }
        
        public void VerifyPermissionAtProject(string projectUId, PermissionSets[] permissions) {

            string token = SignedMemberToken;

            AuthorizationInfo authInfo = null;
            authorizedTokens.TryGetValue(token, out authInfo);

            if (authInfo==null)
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            string memberUId = authInfo.MemberUId;
            
            bool hasPermission = projectsService.MemberHasPermissionAtProject(memberUId, projectUId, permissions);
            if(!hasPermission)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        public void VerifyPermissionAtProjectOrFactoryOwner(string projectUId, PermissionSets[] permissions) {

            if (!authorizedTokens.ContainsKey(SignedMemberToken))
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            if (SignedMemberProfile.IsFactoryOwner)
                return;

            VerifyPermissionAtProject(projectUId, permissions);

        }

        public void VerifyFactoryOwner() {
            
            if (!authorizedTokens.ContainsKey(SignedMemberToken))
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            if(!SignedMemberProfile.IsFactoryOwner)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        public void VerifyCanSeeProposalValues() {

            if (!authorizedTokens.ContainsKey(SignedMemberToken))
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);

            if (!SignedMemberProfile.CanSeeProposalValues)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        public bool IsProjectScrumMaster(string projectUId) {
            return projectsService.MemberHasPermissionAtProject(
                SignedMemberProfile.MemberUId,
                projectUId,
                new PermissionSets[] { PermissionSets.SCRUM_MASTER });
        }

        private bool IsAuthorizationTokenValid(string token) {

            // tries to get from memory
            if (authorizedTokens.ContainsKey(token))
                return true;

            // if not, looks at repository
            AuthorizationInfo info = authorizationRepository.GetAuthorizationInfo(token);
            if (info != null) {                
                authorizedTokens.TryAdd(token, info);                
                return true;
            }

            return false;
        }


        public IServerUrl ServerUrl {
            get {
                throw new NotImplementedException();
            }
        }

    }

    

    
}
