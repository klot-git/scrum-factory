namespace ScrumFactory.Data.Sql
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Data.Objects.SqlClient;
    
    [Export(typeof(IProjectRepository))]
    public class SqlProjectRepository : IProjectRepository {

        private string connectionString;

        [ImportingConstructor()]
        public SqlProjectRepository([Import("ScrumFactoryEntitiesConnectionString")] string connectionString) {
            this.connectionString = connectionString;
        }

        private IQueryable<ScrumFactory.Project> RestrictProjectsFromMember(IQueryable<ScrumFactory.Project> projects, string memberUId) {
            //if (string.IsNullOrEmpty(memberUId))
            //    return projects;
            if (string.IsNullOrEmpty(memberUId))
                return projects.Where(p => p.AnyoneCanJoin == true);
            return projects.Where(p => p.Memberships.Any(m => m.MemberUId == memberUId && m.IsActive==true));
        }

        public ICollection<ScrumFactory.Project> GetEngagedProjects(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                
                var projectsWithMemberships = context.Projects.
                    Where(p => p.Memberships.Any(m => m.MemberUId == memberUId && m.DayAllocation >0 && m.IsActive==true)).
                    Select(p => 
                        new { Project = p, Memberships = p.Memberships.Where(m => m.MemberUId == memberUId && m.DayAllocation > 0 && m.IsActive == true) });

                return projectsWithMemberships.AsEnumerable().Select(p => p.Project).ToList<Project>();
                
            }
        }

        private IQueryable<ScrumFactory.Project> RestrictProjectsByFilter(IQueryable<ScrumFactory.Project> projects, string filter) {
            if (string.IsNullOrEmpty(filter))
                return projects;

            string[] tags = filter.ToLower().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            projects = projects.Where(p => tags.All(t =>
                p.ProjectName.Contains(t) ||
                p.ClientName.Contains(t) ||
                SqlFunctions.StringConvert((double)p.ProjectNumber).Trim().Equals(t)));

            return projects;
        }

        public ICollection<ScrumFactory.Project> GetOpenProjects(string memberUId, string filter = null, int top = 0, int skip = 0) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<ScrumFactory.Project> projects = context.Projects.Where(p => 
                    p.Status == (short)ScrumFactory.ProjectStatus.PROJECT_STARTED
                    || p.Status == (short)ScrumFactory.ProjectStatus.PROPOSAL_APPROVED
                    || p.Status == (short)ScrumFactory.ProjectStatus.PROJECT_SUPPORT
                    || p.Status == (short)ScrumFactory.ProjectStatus.PROPOSAL_CREATION)
                    .Where(p => p.IsSuspended == false);

                projects = RestrictProjectsFromMember(projects, memberUId);

                projects = RestrictProjectsByFilter(projects, filter);
                
                projects = projects.OrderByDescending(p => p.ProjectNumber);
                if (top > 0) {
                    projects = projects.Skip(skip).Take(top);
                }

                // include only the memberUId memberships at the projects
                if (!string.IsNullOrEmpty(memberUId)) {
                    var projectsWithMemberships = projects.Select(p => new { Project = p, Memberships = p.Memberships.Where(m => m.MemberUId == memberUId && m.DayAllocation > 0 && m.IsActive == true) });
                    return projectsWithMemberships.AsEnumerable().Select(p => p.Project).ToList<Project>();
                }

                return projects.ToList();                
            }
        }

        public ICollection<ScrumFactory.Project> GetRunningProjects(string memberUId, short? pType = null) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<ScrumFactory.Project> projects = 
                    context.Projects.Where(p =>p.Status == (short)ScrumFactory.ProjectStatus.PROJECT_STARTED || p.Status == (short)ScrumFactory.ProjectStatus.PROJECT_SUPPORT)
                    .Where(p => p.IsSuspended == false);

                if(pType.HasValue)
                    projects = projects.Where(p => p.ProjectType == pType);

                projects = RestrictProjectsFromMember(projects, memberUId);

                // include only the memberUId memberships at the projects
                if (!string.IsNullOrEmpty(memberUId)) {
                    var projectsWithMemberships = projects.Select(p => new { Project = p, Memberships = p.Memberships.Where(m => m.MemberUId == memberUId && m.DayAllocation > 0 && m.IsActive == true) });
                    return projectsWithMemberships.AsEnumerable().Select(p => p.Project).ToList<Project>();
                }

                return projects.ToList();
            }
        }


        public ICollection<ScrumFactory.Project> GetClosedProjects(string memberUId, System.DateTime startDate, System.DateTime endDate, string filter = null, int top = 0, int skip = 0) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<ScrumFactory.Project> projects = context.Projects.Where(p =>
                    p.CreateDate >= startDate && p.CreateDate <= endDate &&
                    (p.Status == (short)ScrumFactory.ProjectStatus.PROJECT_DONE || p.Status == (short)ScrumFactory.ProjectStatus.PROPOSAL_REJECTED || p.IsSuspended==true));

                projects = RestrictProjectsFromMember(projects, memberUId);

                projects = RestrictProjectsByFilter(projects, filter);
                
                projects = projects.OrderByDescending(p => p.ProjectNumber);
                if (top > 0) {
                    projects = projects.Skip(skip).Take(top);
                }

                // include only the memberUId memberships at the projects
                if (!string.IsNullOrEmpty(memberUId)) {
                    var projectsWithMemberships = projects.Select(p => new { Project = p, Memberships = p.Memberships.Where(m => m.MemberUId == memberUId && m.DayAllocation > 0 && m.IsActive == true) });
                    return projectsWithMemberships.AsEnumerable().Select(p => p.Project).ToList<Project>();
                }

                return projects.ToList();                
            }
        }

        public ICollection<ScrumFactory.Project> GetAllProjects(string memberUId, System.DateTime startDate, System.DateTime endDate, string filter = null, int top = 0, int skip = 0) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<ScrumFactory.Project> projects = context.Projects.Where(p => p.CreateDate >= startDate && p.CreateDate <= endDate);
                
                projects = RestrictProjectsFromMember(projects, memberUId);

                projects = RestrictProjectsByFilter(projects, filter);

                projects = projects.OrderByDescending(p => p.ProjectNumber);
                if (top > 0) {
                    projects = projects.Skip(skip).Take(top);
                }

                // include only the memberUId memberships at the projects
                if (!string.IsNullOrEmpty(memberUId)) {
                    var projectsWithMemberships = projects.Select(p => new { Project = p, Memberships = p.Memberships.Where(m => m.MemberUId == memberUId && m.DayAllocation > 0 && m.IsActive == true) });
                    return projectsWithMemberships.AsEnumerable().Select(p => p.Project).ToList<Project>();
                }

                return projects.ToList();                
            }
        }

        public ICollection<ScrumFactory.Project> GetPendingEngagementProjects(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {                
                var projectsPend= context.Projects.Include("Roles")
                    .Where(p => p.Memberships.Any(m => m.MemberUId == memberUId && m.DayAllocation==null && m.IsActive==true)
                    && (p.Status!=(short)ProjectStatus.PROJECT_DONE && p.Status!=(short)ProjectStatus.PROPOSAL_REJECTED))                    
                    .Select(p2 => new { Project = p2, Memberships = p2.Memberships.Where(ms => ms.DayAllocation==null && ms.MemberUId==memberUId), Roles = p2.Roles });

                ICollection<Project> projects = projectsPend.AsEnumerable().Select(p => p.Project).ToList<Project>();

                return projects;
            }
        }

        public ScrumFactory.Project GetProject(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                context.ContextOptions.ProxyCreationEnabled = false;
                var project = context.Projects.Include("Roles").Include("Sprints").Include("Memberships").SingleOrDefault(p => p.ProjectUId == projectUId);
                project.FixRecursiveRelation();
                return project;
            }
        }

        public Project GetProjectByNumber(int projectNumber) {
           using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var project = context.Projects.Include("Roles").Include("Sprints").Include("Memberships").SingleOrDefault(p => p.ProjectNumber == projectNumber);
                project.FixRecursiveRelation();
                return project;
            }
        }

        public int InsertProject(ScrumFactory.Project project) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                context.Projects.AddObject(project);

                if(project.Memberships!=null)
                    foreach (ProjectMembership m in project.Memberships)
                        context.AddObject("ProjectMemberships", m);

                context.SaveChanges();
            }

            return project.ProjectNumber;
        }

        public void UpdateProject(ScrumFactory.Project project) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                var projectOld = GetProject(project.ProjectUId);
                context.AttachTo("Projects", projectOld);
                context.ApplyCurrentValues<Project>("Projects", project);

                if (projectOld.Memberships == null)
                    projectOld.Memberships = new List<ProjectMembership>();

                if (project.Memberships == null)
                    project.Memberships = new List<ProjectMembership>();
                
                var insertedMemberships = project.Memberships.Where(m => !projectOld.Memberships.Any(o => (o.MemberUId == m.MemberUId && o.RoleUId == m.RoleUId))).ToList();
                var updatedMemberships = project.Memberships.Where(m => projectOld.Memberships.Any(o => (o.MemberUId == m.MemberUId && o.RoleUId == m.RoleUId))).ToList();
                var deletedMemberships = projectOld.Memberships.Where(m => !project.Memberships.Any(o => (o.MemberUId == m.MemberUId && o.RoleUId == m.RoleUId))).ToList();

                foreach (ProjectMembership m in insertedMemberships)
                    context.AddObject("ProjectMemberships", m);
                foreach (ProjectMembership m in updatedMemberships)
                    context.ApplyCurrentValues<ProjectMembership>("ProjectMemberships", m);
                foreach (ProjectMembership m in deletedMemberships)
                    context.DeleteObject(m);


                context.SaveChanges();
            }
        }

        public ICollection<ScrumFactory.Role> GetProjectRoles(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Roles.Where(r => r.ProjectUId == projectUId).ToList();
            }
        }

        private Role GetProjectRole(string roleUId, bool includePlannedHours) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<Role> roles = context.Roles;
                if (includePlannedHours)
                    roles = roles.Include("PlannedHours");
                return roles.SingleOrDefault(r => r.RoleUId == roleUId);
            }
        }

        public Role GetProjectRole(string roleUId) {
            return GetProjectRole(roleUId, false);
        }

        public void SaveProjectRole(ScrumFactory.Role role) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                Role oldRole = GetProjectRole(role.RoleUId);

               

                if (oldRole == null) {
                    context.Roles.AddObject(role);
                } else {
                    context.AttachTo("Roles", oldRole);
                    context.ApplyCurrentValues<Role>("Roles", role);
                }
                context.SaveChanges();
            }
        }
                
        public bool CanDeleteProjectRole(string projectUId, string roleUId) {

            // can delete if there is any member with this role
            Project project = GetProject(projectUId);
            if(project.Memberships.Any(ms => ms.RoleUId==roleUId))
                return false;

            // can delete if is the only scrum master role
            if (project.Memberships.Count(ms => ms.Role.PermissionSet==(short)PermissionSets.SCRUM_MASTER && ms.RoleUId != roleUId)==0)
                return false;

            // can delete if there is any hour planned for this role
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? plannedHoursForThisRole = context.PlannedHours.Where(h => h.RoleUId == roleUId).Sum(h => h.Hours);
                if (plannedHoursForThisRole == null || plannedHoursForThisRole == 0)
                    return true;                
            }
            return false;
        }

        public void DeleteProjectRole(string roleUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                Role oldRole = GetProjectRole(roleUId, true);
                context.AttachTo("Roles", oldRole);
                context.DeleteObject(oldRole);                
                context.SaveChanges();                                           
            }
        }

        public ICollection<Sprint> GetSprints(string projectId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Sprints.Where(s => s.ProjectUId == projectId).ToList();
            }
        }

        public Sprint GetSprint(string sprintUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Sprints.Where(s => s.SprintUId == sprintUId).SingleOrDefault();
            }
        }

        public void SaveSprint(Sprint sprint) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                Sprint oldSprint = GetSprint(sprint.SprintUId);

                // if is a new item insert it
                if (oldSprint == null) {
                    context.Sprints.AddObject(sprint);

                    // updates the item
                } else {
                    context.AttachTo("Sprints", oldSprint);
                    context.ApplyCurrentValues<Sprint>("Sprints", sprint);
                }

                context.SaveChanges();

            }
        }

        public void DeleteSprint(string sprintUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                Sprint oldSprint = GetSprint(sprintUId);
                context.AttachTo("Sprints", oldSprint);
                context.DeleteObject(oldSprint);
                context.SaveChanges();
            }
        }


        public ICollection<string> GetClientNames() {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return (ICollection<string>)context.Projects.Select(p => p.ClientName).Distinct().ToList();
            }
        }

        public ICollection<string> GetPlatforms() {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return (ICollection<string>)context.Projects.Select(p => p.Platform).Distinct().ToList();
            }
        }

        public Project GetLastSimilarProject(Project project, bool onlyWithProposals = false) {

            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // first tries to get the last project from same client
                var projectQuery = context.Projects
                    .Include("Roles").Include("Sprints").Include("Memberships")
                    .Where(p => p.ClientName == project.ClientName && p.ProjectUId != project.ProjectUId);

                if(onlyWithProposals)
                    projectQuery = projectQuery.Where(p => context.RoleHourCosts.Any(o => o.ProjectUId == p.ProjectUId && o.Price>0));

                Project similar = projectQuery.OrderByDescending(p => p.CreateDate).FirstOrDefault();

                similar.FixRecursiveRelation();

                if (similar != null)
                    return similar;

                // id there is no project from this client before, gets the last one of any client made by me
                projectQuery = context.Projects
                    .Include("Roles").Include("Sprints").Include("Memberships")
                    .Where(p => p.ProjectUId!=project.ProjectUId && p.CreateBy==project.CreateBy);

                if (onlyWithProposals)
                    projectQuery = projectQuery.Where(p => context.RoleHourCosts.Any(o => o.ProjectUId == p.ProjectUId && o.Price > 0));

                similar = projectQuery.OrderByDescending(p => p.CreateDate).FirstOrDefault();

                similar.FixRecursiveRelation();

                if (similar != null)
                    return similar;

                // id there is no project from this client before, gets the last one of any client
                projectQuery = context.Projects
                    .Include("Roles").Include("Sprints").Include("Memberships")
                    .Where(p => p.ProjectUId != project.ProjectUId);

                if (onlyWithProposals)
                    projectQuery = projectQuery.Where(p => context.RoleHourCosts.Any(o => o.ProjectUId == p.ProjectUId && o.Price > 0));

                similar = projectQuery.OrderByDescending(p => p.CreateDate).FirstOrDefault();

                similar.FixRecursiveRelation();

                return similar;
            }
        }

        public ICollection<Risk> GetProjectRisks(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Risks.Where(r => r.ProjectUId == projectUId).ToList();
            }
        }

        public Risk GetRisk(string riskUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Risks.SingleOrDefault(r => r.RiskUId == riskUId);
            }
        }

        public void SaveRisk(Risk risk) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                Risk oldRisk = GetRisk(risk.RiskUId);

                // if is a new item insert it
                if (oldRisk == null) {
                    context.Risks.AddObject(risk);

                    // updates the item
                }
                else {
                    context.AttachTo("Risks", oldRisk);
                    context.ApplyCurrentValues<Risk>("Risks", risk);
                }

                context.SaveChanges();

            }
        }

        public ICollection<ProjectEvent> GetUserProjectEvents(string memberUId) {
            List<ProjectEvent> events = new List<ProjectEvent>();
            
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                System.DateTime limitDate = System.DateTime.Today.AddDays(4);

                ICollection<Project> projects = context.Projects.Include("Sprints").Where(p =>
                    (p.Status == (short) ProjectStatus.PROJECT_STARTED || p.Status == (short) ProjectStatus.PROPOSAL_APPROVED)
                    && p.ProjectType != (short) ProjectTypes.SUPPORT_PROJECT
                    && p.Memberships.Any(ms => ms.MemberUId == memberUId && ms.IsActive==true)
                    && p.Sprints.Any(s =>
                        (s.StartDate < limitDate && s.StartDate >= System.DateTime.Today) ||    // sprints que vão começar
                        (s.EndDate < limitDate && s.EndDate >= System.DateTime.Today)))         // sprints que vão acabar
                        .ToList();

                
                foreach (Project p in projects) {
                    ProjectEvent e = new ProjectEvent() { ProjectUId = p.ProjectUId, ProjectName = p.ProjectName, ProjectNumber = p.ProjectNumber, EventType = (short)ProjectEventTypes.SPRINT_END };
                    Sprint sprintThatWillStart = p.Sprints.OrderBy(s => s.SprintNumber).FirstOrDefault(s => s.StartDate < limitDate && s.StartDate >= System.DateTime.Today);
                    Sprint sprintThatWillEnd = p.Sprints.OrderBy(s => s.SprintNumber).FirstOrDefault(s => s.EndDate < limitDate && s.EndDate >= System.DateTime.Today);
                    
                    if (sprintThatWillEnd != null) {
                        if(sprintThatWillEnd==p.LastSprint)
                            e.EventType = (short)ProjectEventTypes.PROJECT_END;
                        e.When = sprintThatWillEnd.EndDate;
                        e.SprintNumber = sprintThatWillEnd.SprintNumber;
                    }

                    if(sprintThatWillStart!=null) {
                        e.When = sprintThatWillStart.StartDate;
                        e.SprintNumber = sprintThatWillStart.SprintNumber;
                        if(e.SprintNumber==1)
                            e.EventType = (short)ProjectEventTypes.PROJECT_START;
                    }

                    events.Add(e);
                   
                }
                
            }

            return events;
        }

        public int GetMembershipCountOf(string projectUId, PermissionSets permissionSet) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ProjectMemberships.Count(ms => ms.Role.PermissionSet == (short)permissionSet && ms.ProjectUId == projectUId);
            }
        }


        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets permission) {
            PermissionSets[] ps = new PermissionSets[1];
            ps[0] = permission;
            return MemberHasPermissionAtProject(memberUId, projectUId, ps);
        }

        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets[] permissions) {

            short[] permissionsS = new short[permissions.Length];
            for (int i = 0; i < permissions.Length; i++)
                permissionsS[i] = (short)permissions[i];

            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // does not make sense
                //if (context.Projects.Where(p => p.ProjectUId == projectUId && p.CreateBy == memberUId).Count() > 0)
                //    return true;

                if (context.ProjectMemberships.Where(
                        pmr => pmr.MemberUId == memberUId
                        && pmr.ProjectUId == projectUId
                        && pmr.IsActive == true
                        && permissionsS.Contains(pmr.Role.PermissionSet)).Count() > 0)
                    return true;
            }

            return false;
        }


        public int GetMemberDayAllocation(string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                int? allocation = context.ProjectMemberships.Where(m => m.MemberUId == memberUId && m.IsActive).Sum(m => m.DayAllocation);
                if (!allocation.HasValue)
                    allocation = 0;
                return allocation.Value;
            }
        }

        public string[] GetSimilarCodeRepositories(string projectName, string clientName) {            
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                string[] tags = projectName.Split(' ').Where(t => t.Length > 3).ToArray();                    
                return context.Projects
                    .Where(p => p.ClientName == clientName 
                        && p.CodeRepositoryPath != null && p.CodeRepositoryPath != ""
                        && tags.Any(t => p.ProjectName.Contains(t)))
                    .OrderBy(p => p.CodeRepositoryPath)
                    .Select(p => p.CodeRepositoryPath).Distinct().Take(3).ToArray();                    
            }
        }
       
    }
}
