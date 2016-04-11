namespace ScrumFactory.Data.Sql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;

    [Export(typeof(IMembersRepository))]
    public class SqlMembersRepository : IMembersRepository
    {
        private string connectionString;

        [ImportingConstructor()]
        public SqlMembersRepository(
            [Import("ScrumFactoryEntitiesConnectionString")]
            string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void RemoveProjectMember(string projectId, string memberId)
        {
            throw new NotImplementedException();
        }

        public void SaveProjectMember(ScrumFactory.ProjectMember member)
        {
            using (var context = new Sql.ScrumFactoryEntities(this.connectionString))
            {
                var memberEntity = context.ProjectMembers.Where(m => m.ProjectId == member.ProjectId && m.MemberId == member.MemberId).SingleOrDefault();

                if (memberEntity == null)
                {
                    memberEntity = new ProjectMember { MemberId = member.MemberId, ProjectId = member.ProjectId, RoleId = member.RoleId };
                    context.AddToProjectMembers(memberEntity);
                }

                memberEntity.RoleId = member.RoleId;

                context.SaveChanges();
            }
        }

        public ICollection<ScrumFactory.ProjectMember> GetMembersInProject(string projectId)
        {
            using (var context = new Sql.ScrumFactoryEntities(this.connectionString))
            {
                return context.ProjectMembers.Where(m => m.ProjectId == projectId).ToList().Select(p => p.ToModel()).ToList();
            }
        }

        public ICollection<ScrumFactory.ProjectRole> GetAllRoles()
        {
            throw new NotImplementedException();
        }
    }
}
