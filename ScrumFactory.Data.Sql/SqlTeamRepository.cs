namespace ScrumFactory.Data.Sql {
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Data.Objects;
    using System.Data.EntityClient;
    using System.Transactions;
    using System.Linq;

    [Export(typeof(ITeamRepository))]
    public class SqlTeamRepository : ITeamRepository {
        private string connectionString;

        [ImportingConstructor()]
        public SqlTeamRepository(
            [Import("ScrumFactoryEntitiesConnectionString")]
            string connectionString) {
            this.connectionString = connectionString;
        }

        public ICollection<ScrumFactory.MemberProfile> GetAllMembers(string filter, int availability, string[] companies, bool activeOnly, string workingWithUId, int top) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // members and membeeships with allocation
                var filteredMembers = context.MembersProfile.Select(m =>
                    new { MemberProfile = m, Memberships = m.Memberships.Where(ms => ms.DayAllocation > 0) });

                if (activeOnly)
                    filteredMembers = filteredMembers.Where(m => m.MemberProfile.IsActive == true);

                if (companies!=null && companies.Length > 0)
                    filteredMembers = filteredMembers.Where(m => companies.Contains(m.MemberProfile.CompanyName));

                if (!string.IsNullOrEmpty(workingWithUId))
                    filteredMembers = filteredMembers.Where(m =>
                        m.Memberships.Any(ms => context.ProjectMemberships.Any(mms => mms.MemberUId == workingWithUId && mms.ProjectUId == ms.ProjectUId && mms.IsActive))
                        && m.MemberProfile.MemberUId!=workingWithUId);
                        

                if (!string.IsNullOrWhiteSpace(filter)) {
                    string[] tags = filter.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    filteredMembers = filteredMembers.Where(
                        m => tags.All(t =>
                            m.MemberProfile.TeamCode.StartsWith(t)
                            || m.MemberProfile.FullName.Contains(t)
                            || m.MemberProfile.EmailAccount.Contains(t)
                            || m.MemberProfile.Skills.Contains(t)));
                }
                
                if(availability>0)
                    filteredMembers = filteredMembers.Where(
                        m => m.Memberships.Sum(ms => ms.DayAllocation) <= 4 - availability
                        || m.Memberships.Sum(ms => ms.DayAllocation) == null);

                if (top > 0)
                    filteredMembers = filteredMembers.Take(top);

                return filteredMembers.AsEnumerable().Select(m => m.MemberProfile).ToList<MemberProfile>();
                  
            }
        }


        public ICollection<ScrumFactory.MemberProfile> GetTeamMembers(string teamCode) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.MembersProfile.Where(m => m.IsActive == true && m.TeamCode.ToLower() == teamCode.ToLower()).ToList();
            }
        }

        public MemberProfile GetMember(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // need to get the memberships here in order to calc the DAYOCCUPATION property of the member
                var memberWithAllocation = context.MembersProfile
                    .Where(m => m.MemberUId == memberUId)
                    .Select(m2 => new { MemberProfile = m2, Memberships = m2.Memberships.Where(ms => ms.DayAllocation > 0) });

                MemberProfile member = memberWithAllocation.AsEnumerable().Select(m => m.MemberProfile).SingleOrDefault<MemberProfile>();

                return member;
            }
        }

        public MemberAvatar GetMemberAvatar(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.MembersAvatar.Where(ma => ma.MemberUId == memberUId).SingleOrDefault();
            }
        }

        public void SaveMemberAvatar(MemberAvatar avatar) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                MemberProfile oldMember = GetMember(avatar.MemberUId);

                MemberAvatar oldAvatar = GetMemberAvatar(avatar.MemberUId);

                if (oldMember == null)
                    throw new System.Exception("member not found");

                if (oldAvatar == null) {
                    context.MembersAvatar.AddObject(avatar);
                } else {
                    context.AttachTo("MembersAvatar", oldAvatar);
                    context.ApplyCurrentValues<MemberAvatar>("MembersAvatar", avatar);
                }

                context.SaveChanges();

            }
        }

        public void RemoveMemberAvatar(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                MemberAvatar avatar = context.MembersAvatar.SingleOrDefault(a => a.MemberUId == memberUId);
                if (avatar != null) {
                    context.MembersAvatar.DeleteObject(avatar);
                    context.SaveChanges();
                }
            }
        }

        public void SaveMember(MemberProfile newMember) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                MemberProfile oldMember = GetMember(newMember.MemberUId);

                if (oldMember == null)
                    context.MembersProfile.AddObject(newMember);
                else {
                    context.AttachTo("MemberProfiles", oldMember);
                    context.ApplyCurrentValues<MemberProfile>("MemberProfiles", newMember);
                }

                context.SaveChanges();
             
            }
        }

        /// <summary>
        /// Gets the project members for a given project.
        /// The Memberships collection of each member is also filled
        /// with the corresponding role(s) of the member at the project.
        /// </summary>
        /// <param name="projectUId">The project Uid.</param>
        /// <returns>The project members</returns>
        public ICollection<MemberProfile> GetProjectMembers(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.MembersProfile.Where(m => m.Memberships.Any(ms => ms.ProjectUId == projectUId)).ToList();              
            }
        }

        public void SaveProjectMembership(ProjectMembership membership) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                ProjectMembership existMembership = context.ProjectMemberships.SingleOrDefault(m => m.MemberUId == membership.MemberUId && m.ProjectUId == membership.ProjectUId && m.RoleUId == membership.RoleUId);
                if (existMembership == null) {
                    context.ProjectMemberships.AddObject(membership);
                } else {
                    context.AttachTo("ProjectMemberships", existMembership);
                    context.ApplyCurrentValues<ProjectMembership>("ProjectMemberships", membership);
                }
                context.SaveChanges();
            }
        }


        public ICollection<MemberProfile> GetOwnersMembers() {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.MembersProfile.Where(m => m.IsFactoryOwner == true || m.CanSeeProposalValues == true).ToList();
            }
        }

        public int GetOwnersMembersCount() {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.MembersProfile.Where(m => m.IsFactoryOwner == true).Count();
            }
        }


    }
}
