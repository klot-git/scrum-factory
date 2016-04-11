using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;

namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(ITeamService_ServerSide))]
    [Export(typeof(ITeamService))]
    public class TeamService : ITeamService_ServerSide {


        [Import]
        private Data.ITeamRepository teamRepository { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }


        [Import]
        private ITasksService_ServerSide tasksService { get; set; }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("@@@", "#").Replace("@@", "\\").Trim();
        }

        [WebGet(UriTemplate = "Members/{memberUId}", ResponseFormat = WebMessageFormat.Json)]
        public MemberProfile GetMember(string memberUId) {
            MemberProfile member = teamRepository.GetMember(SafeMemberUId(memberUId));
            if (member != null)
                member.Performance = tasksService.GetMemberPerformance(SafeMemberUId(memberUId));
            return member;
        }

        
        public MemberAvatar GetMemberAvatar(string memberUId) {
            return teamRepository.GetMemberAvatar(SafeMemberUId(memberUId));
        }

        [WebInvoke(Method = "POST", UriTemplate = "Members/Contacts", ResponseFormat = WebMessageFormat.Json)]
        public void CreateContactMember(MemberProfile member) {

            authorizationService.VerifyRequestAuthorizationToken();

            if (!member.IsContactMember)
                throw new WebFaultException<string>("Only contact members can be created.", System.Net.HttpStatusCode.BadRequest);

            member.CreateBy = authorizationService.SignedMemberProfile.MemberUId;
            member.IsActive = true;
            
            teamRepository.SaveMember(member);            
        }
        
        public void CreateMember(MemberProfile member) {
            member.CreateBy = member.MemberUId; // only I can create Myself

            member.IsActive = true;
           
            // if there is no owner this will be the one
            int owners = teamRepository.GetOwnersMembersCount();
            if (owners == 0) {
                member.IsFactoryOwner = true;
                member.CanSeeProposalValues = true;
            }

            teamRepository.SaveMember(member);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Members/{memberUId}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateMember(string memberUId, MemberProfile member) {
            MemberProfile oldMember = GetMember(memberUId);

            if (oldMember.IsContactMember)
                authorizationService.VerifyUser(SafeMemberUId(oldMember.CreateBy));
            else
                authorizationService.VerifyUser(SafeMemberUId(memberUId));

            teamRepository.SaveMember(member);            
        }
        
        [WebInvoke(Method = "PUT", UriTemplate = "Members/{memberUId}/Avatar", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateMemberAvatar(string memberUId, MemberAvatar avatar) {
            authorizationService.VerifyUser(SafeMemberUId(memberUId));                
            teamRepository.SaveMemberAvatar(avatar);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "Members/{memberUId}/Avatar", ResponseFormat = WebMessageFormat.Json)]
        public void RemoveMemberAvatar(string memberUId) {
            authorizationService.VerifyUser(SafeMemberUId(memberUId));
            teamRepository.RemoveMemberAvatar(memberUId);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Members/{memberUId}/IsActive", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateMemberIsActive(string memberUId, bool isActive) {
            authorizationService.VerifyFactoryOwner();
            MemberProfile member = GetMember(memberUId);
            member.IsActive = isActive;
            teamRepository.SaveMember(member);
        }

        

        private bool RestrictProjectMembers {
            get {
                return Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["RestrictProjectMembers"]);
            }
        }

        [WebGet(UriTemplate = "TeamMembers/{teamCode}/?includeTasks={includeTasks}&excludeMe={excludeMe}&top={top}&topTasks={topTasks}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<ScrumFactory.MemberProfile> GetTeamMembers(string teamCode, bool includeTasks, bool excludeMe, int top = 25, int topTasks = 10) {
            authorizationService.VerifyRequestAuthorizationToken();

            ICollection<MemberProfile> members = null;
            
            if (!String.IsNullOrEmpty(teamCode))
                members = teamRepository.GetTeamMembers(teamCode);

            if (members == null)
                return new List<MemberProfile>();

            // if should include members open tasks
            if (includeTasks) {

                if (members.Count == 0) {
                    MemberProfile me = teamRepository.GetMember(authorizationService.SignedMemberProfile.MemberUId);
                    members.Add(me);
                }

                string[] membersUIds = members.Select(m => m.MemberUId).ToArray();
                ICollection<Task> tasks = tasksService.GetUsersTasks(membersUIds, true, true);

                foreach (MemberProfile member in members) {
                    member.OpenTasks = tasks.Where(t => t.TaskAssigneeUId == member.MemberUId).ToList();
                    member.PlannedHoursForToday = member.OpenTasks.Sum(t => t.PlannedHours - t.EffectiveHours);
                    member.OpenTasks = member.OpenTasks.Take(topTasks).ToList();
                }

                members = members.OrderByDescending(m => m.PlannedHoursForToday).ThenBy(m => m.FullName).Take(top).ToList();

                // include unassigned tasks crated by this members
                var unassignedTasks = tasks.Where(t => t.TaskAssigneeUId == null).ToArray();
                if (unassignedTasks.Length > 0) {
                    MemberProfile nullMember = new MemberProfile() { MemberUId = null, IsActive = true };
                    nullMember.OpenTasks = unassignedTasks.Take(topTasks).ToList();
                    nullMember.PlannedHoursForToday = unassignedTasks.Sum(t => t.PlannedHours - t.EffectiveHours);
                    members.Add(nullMember);
                }
                
                
            }

            if (excludeMe)
                members = members.Where(m => m.MemberUId != authorizationService.SignedMemberProfile.MemberUId).ToList();

            return members;
        }


        [WebGet(UriTemplate = "Members/?filter={filter}&availability={availability}&clientName={clientName}&activeOnly={activeOnly}&workingWith={workingWith}&top={top}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<MemberProfile> GetMembers(string filter, int availability, string clientName, bool activeOnly, string workingWith, int top) {
            
            authorizationService.VerifyRequestAuthorizationToken();

            // if the server is set to restrict project members, restrict them
            List<string> companies = new List<string>();
            if (RestrictProjectMembers) {
                companies.Add(authorizationService.SignedMemberProfile.CompanyName);
                if(!string.IsNullOrEmpty(clientName))
                    companies.Add(clientName);
            }

            ICollection<MemberProfile> members = teamRepository.GetAllMembers(filter, availability, companies.ToArray(), activeOnly, workingWith, top);


            string[] ids = members.Select(m => m.MemberUId).ToArray();
            ICollection<TodayMemberPlannedHours> hours = tasksService.GetTodayMemberPlannedHours(ids);
            AssignTodayPlannedHoursToMembers(members, hours);
           
            
            return members;
        }


        [WebGet(UriTemplate = "Members/Contacts/{clientName}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<MemberProfile> GetContacts(string clientName) {
            authorizationService.VerifyRequestAuthorizationToken();            
            return teamRepository.GetAllMembers(null, 0, new string[] { clientName }, true, null, 0);
        }

        [WebGet(UriTemplate = "ProjectMembers/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<MemberProfile> GetProjectMembers(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            ICollection<MemberProfile> members = GetProjectMembers_skipAuth(projectUId);
            string[] memberUIds = members.Select(m => m.MemberUId).ToArray();

            ICollection<TodayMemberPlannedHours> hours = tasksService.GetTodayMemberPlannedHours(memberUIds);
            AssignTodayPlannedHoursToMembers(members, hours);
            return members;
        }

        public ICollection<MemberProfile> GetProjectMembers_skipAuth(string projectUId) {
            return teamRepository.GetProjectMembers(projectUId);                        
        }

        private void AssignTodayPlannedHoursToMembers(ICollection<MemberProfile> members, ICollection<TodayMemberPlannedHours> hours) {
            foreach (MemberProfile m in members) {
                TodayMemberPlannedHours hour = hours.SingleOrDefault(h => h.TaskAssigneeUId == m.MemberUId);
                if (hour != null)
                    m.PlannedHoursForToday = hour.PlannedHours;
                if (m.PlannedHoursForToday == null)
                    m.PlannedHoursForToday = 0;
            }
        }
   
        [WebGet(UriTemplate = "OwnersMembers", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<MemberProfile> GetOwnersMembers() {
            authorizationService.VerifyFactoryOwner();
            return teamRepository.GetOwnersMembers();
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Members/{memberUId}/IsFactoryOnwer", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeMemberIsFactoryOwner(string memberUId, bool isOwner) {
            authorizationService.VerifyFactoryOwner();

            // at least one factory owner shuld exist
            if (isOwner == false) {
                int membersCount = teamRepository.GetOwnersMembersCount();
                if (membersCount <= 1)
                    throw new WebFaultException<string>("BRE_CAN_NOT_REMOVE_LAST_FACTORY_OWNER", System.Net.HttpStatusCode.BadRequest);
            }
            
            MemberProfile member = teamRepository.GetMember(SafeMemberUId(memberUId));
            member.IsFactoryOwner = isOwner;
            teamRepository.SaveMember(member);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Members/{memberUId}/CanSeeProposalValues", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeMemberCanSeeProposals(string memberUId, bool canSee) {
            authorizationService.VerifyFactoryOwner();
            MemberProfile member = teamRepository.GetMember(SafeMemberUId(memberUId));
            member.CanSeeProposalValues = canSee;
            teamRepository.SaveMember(member);
        }
    }
}
