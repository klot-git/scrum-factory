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
    [Export(typeof(IProjectsService))]    
    public class ProjectsService : IProjectsService {

        

        [Import]
        private Data.IProjectRepository projectsRepository { get; set; }

        [Import]
        private IBacklogService_ServerSide backlogService { get; set; }

        [Import]
        private ITasksService_ServerSide tasksService { get; set; }

        [Import]
        private IProposalsService_ServerSide proposalsService { get; set; }


        [Import]
        private IMailerService mailer { get; set; }

        [Import]
        private ICalendarService calendar { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }


        private string CodeRepPath {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["CodeRepPath"];
            }
        }

        private string DocRepPath {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["DocRepPath"];
            }
        }

        private string DefaultCompanyName {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["DefaultCompanyName"];
            }
        }

        [WebGet(UriTemplate = "ProjectTotalEffectiveHoursByMember/{projectUId}/?memberUId={memberUId}", ResponseFormat = WebMessageFormat.Json)]        
        public IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string memberUId, string projectUId) {
            return tasksService.GetProjectTotalEffectiveHoursByMember(memberUId, projectUId);
        }

        [WebGet(UriTemplate = "Projects/?startDate={startDate}&endDate={endDate}&filter={filter}&memberUId={memberUId}&top={top}&skip={skip}&tagFilter={tagFilter}", ResponseFormat = WebMessageFormat.Json)]        
        public ICollection<Project> GetProjects(string startDate, string endDate, string filter, string memberUId, string tagFilter = null, int top = 0, int skip = 0) {

            authorizationService.VerifyRequestAuthorizationToken();

            ICollection<Project> projects = null;
                        
            DateTime startDateDt;
            DateTime endDateDt;

            if(!System.DateTime.TryParse(startDate, out startDateDt))            
                startDateDt = DateTime.MinValue;

            if (!System.DateTime.TryParse(endDate, out endDateDt))
                endDateDt = DateTime.MaxValue;

            if (memberUId == "me")
                memberUId = authorizationService.SignedMemberProfile.MemberUId;                                    

            if (filter.Equals("CLOSED_PROJECTS")) 
                projects = this.projectsRepository.GetClosedProjects(memberUId, startDateDt, endDateDt, tagFilter, top, skip);

            if (filter.Equals("OPEN_PROJECTS"))
                projects = this.projectsRepository.GetOpenProjects(memberUId, tagFilter, top, skip);

            if (filter.Equals("RUNNING_PROJECTS"))
                projects = this.projectsRepository.GetRunningProjects(memberUId, null);

            if (filter.Equals("HELPDESK_PROJECTS"))
                projects = this.projectsRepository.GetRunningProjects(memberUId, 30);

            if (filter.Equals("ENGAGED_PROJECTS"))
                projects = this.projectsRepository.GetEngagedProjects(memberUId);

            if (projects == null)
                projects = this.projectsRepository.GetAllProjects(memberUId, startDateDt, endDateDt, tagFilter, top, skip);

            foreach (var p in projects) p.FixRecursiveRelation();
            return projects;
                        
        }


        [WebGet(UriTemplate = "PendingEngagementProjects/", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Project> GetPendingEngagementProjects() {
            authorizationService.VerifyRequestAuthorizationToken();
            var projects = projectsRepository.GetPendingEngagementProjects(authorizationService.SignedMemberProfile.MemberUId);
            foreach (var p in projects) p.FixRecursiveRelation();
            return projects;
        }
                
        [WebGet(UriTemplate = "Projects/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public Project GetProject(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return GetProject_skipAuth(projectUId);
        }

        private Project GetProject_skipAuth(string projectUId) {
            
            int number;
            int.TryParse(projectUId, out number);

            if (number > 0)
                return projectsRepository.GetProjectByNumber(number);

            return projectsRepository.GetProject(projectUId);

        }

        [WebInvoke(Method="PUT", UriTemplate = "Projects/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateProject(string projectUId, Project project) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            projectsRepository.UpdateProject(project);            
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Status/?option={option}", ResponseFormat = WebMessageFormat.Json)]
        public Project ChangeProjectStatus(string projectUId, string option, short status) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            Project project = null;

            if (status == (short)ProjectStatus.PROJECT_STARTED)
                project = StartProject(projectUId, option.Equals("MOVE_SPRINT_DATE"));

            if (status == (short)ProjectStatus.PROJECT_DONE)
                project = CloseProject(projectUId, option.Equals("CLOSE_ITEMS"));

            if (status == (short)ProjectStatus.PROPOSAL_APPROVED || status == (short)ProjectStatus.PROPOSAL_REJECTED) {
                project = GetProject(projectUId);
                project.Status = status;
                UpdateProject(project.ProjectUId, project);
            }

            if (status == (short)ProjectStatus.PROJECT_SUPPORT) {
                project = StartProject(projectUId, option.Equals("MOVE_SPRINT_DATE"));
                project.Status = status;
                UpdateProject(project.ProjectUId, project);
            }


            return project;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Baseline", ResponseFormat = WebMessageFormat.Json)]
        public void SetProjectBaseline(string projectUId, int? baseline) {
            Project project = projectsRepository.GetProject(projectUId);
            if (project == null)
                throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
            project.Baseline = baseline;
            projectsRepository.UpdateProject(project);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/IsSuspended", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeProjectIsSuspended(string projectUId, bool isSuspended) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            Project project = GetProject(projectUId);
            project.IsSuspended = isSuspended;
            UpdateProject(project.ProjectUId, project);

            return;
        }


        private Project CloseProject(string projectUId, bool closeItems) {

            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
                        
            Project project = GetProject(projectUId);
            project.EndDate = System.DateTime.Now;
            project.Status = (short)ProjectStatus.PROJECT_DONE;

            // releases the resources
            if(project.Memberships!=null)
                foreach (ProjectMembership pm in project.Memberships)
                    pm.DayAllocation = 0;
            
            // closes the project
            UpdateProject(projectUId, project);

            // closes the itens
            if (closeItems) {
                BacklogItem[] items = backlogService.GetCurrentBacklog(projectUId, (short)BacklogFiltersMode.PENDING).ToArray();
                foreach (BacklogItem item in items) {
                    item.Status = (short)BacklogItemStatus.ITEM_DONE;
                    backlogService.UpdateBacklogItemIgnoringHours(item.BacklogItemUId, item);
                }                
            }

            // send email at other thread, so all the database query stuff does not lock this return
            //string serverUrl = Helper.ReportTemplate.ServerUrl;
            //System.Threading.ThreadStart sendMail = delegate { SendCloseProjectEmail(project, serverUrl); };
            //new System.Threading.Thread(sendMail).Start();
            SendCloseProjectEmail(project, Helper.ReportTemplate.ServerUrl);

            project.FixRecursiveRelation();
                        
            return project;

        }
        
        private Project StartProject(string projectUId, bool moveDate) {

            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            Project project = GetProject(projectUId);
            project.StartDate = System.DateTime.Now;
            project.Status = (short)ProjectStatus.PROJECT_STARTED;
            project.FirstSprint.StartDate = (DateTime)project.StartDate;

            UpdateProject(projectUId, project);

            if (moveDate) {
                project.FirstSprint.StartDate = (DateTime)project.StartDate.Value.Date;
                project.Sprints = UpdateSprint(projectUId, project.FirstSprint.SprintUId, project.FirstSprint).ToList();
            }

            // send email at other thread, so all the database query stuff does not lock this return            
            SendStartProjectEmail(project, Helper.ReportTemplate.ServerUrl);

            project.FixRecursiveRelation();

            return project;
        }

        private void SendStartProjectEmail(Project project, string serverUrl) {

            try {
                // get members and attach to the project
                mailer.AttachProjectMembers(project);

                // create body from the template
                ReportHelper.Report reports = new ReportHelper.Report();
                ReportHelper.ReportConfig reportConfig = new ReportHelper.ReportConfig("EmailNotifications", "project_started", "");
                reportConfig.ReportObjects.Add(project);
                reportConfig.ReportVars.Add("LastSprintUId", project.LastSprint.SprintUId);

                string body = reports.CreateReportXAML(serverUrl, reportConfig);

                // subject
                string subject = project.ProjectName + " STARTED !!";

                // send it to all project members
                bool send = mailer.SendEmail(project, subject, body);
                if (!send)
                    ScrumFactory.Services.Logic.Helper.Log.LogMessage("Start project email was not send.");
            }
            catch (System.Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
            }
        }

        private void SendCloseProjectEmail(Project project, string serverUrl) {

            try {
                // get members and attach to the project
                mailer.AttachProjectMembers(project);

                // get vel indicator
                decimal velIndicator = backlogService.GetProjectVelocityIndicator_skipAuth(project.ProjectUId);
                decimal budgetIndicator = proposalsService.GetBudgetIndicator_skipAuth(project.ProjectUId);
                decimal qualityIndicator = tasksService.GetReworkIndicator_skipAuth(project.ProjectUId);
                
                // create body from the template
                ReportHelper.Report reports = new ReportHelper.Report();
                ReportHelper.ReportConfig reportConfig = new ReportHelper.ReportConfig("EmailNotifications", "project_closed", "");
                reportConfig.ReportObjects.Add(project);
                reportConfig.ReportVars.Add("VelIndicator", velIndicator.ToString("#0"));
                reportConfig.ReportVars.Add("BudgetIndicator", budgetIndicator.ToString("#0"));
                reportConfig.ReportVars.Add("QualityIndicator", qualityIndicator.ToString("#0"));

                string body = reports.CreateReportXAML(serverUrl, reportConfig);

                // subject
                string subject = project.ProjectName + " CLOSED !!";

                // send it to all project members
                bool send = mailer.SendEmail(project, subject, body);
                if (!send)
                    ScrumFactory.Services.Logic.Helper.Log.LogMessage("Close project email was not send.");
            }
            catch (System.Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
            }
        }

        private void SendInviteMemberEmail(ProjectMembership membership, string serverUrl) {

            try {

               
                Project project = GetProject_skipAuth(membership.ProjectUId);

                // get members and attach to the project
                mailer.AttachProjectMembers(project);

                MemberProfile member = project.Memberships.FirstOrDefault(ms => ms.MemberUId == membership.MemberUId).Member;

                if (member.IsContactMember)
                    return;
                                
                // create body from the template
                ReportHelper.Report reports = new ReportHelper.Report();
                ReportHelper.ReportConfig reportConfig = new ReportHelper.ReportConfig("EmailNotifications", "member_invited", "");
                reportConfig.ReportObjects.Add(project);
                reportConfig.ReportVars.Add("RoleName", membership.Role.RoleName);

                string body = reports.CreateReportXAML(serverUrl, reportConfig);

                // subject
                string subject = "Join project invitation";

                // send it to all project members
                bool send = mailer.SendEmail(member.EmailAccount, subject, body);
                if (!send)
                    ScrumFactory.Services.Logic.Helper.Log.LogMessage("Invite project email was not send.");
            }
            catch (System.Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
            }
        }



        [WebInvoke(Method = "POST", UriTemplate = "Projects/?useLastProjectAsModel={useLastProjectAsModel}", ResponseFormat = WebMessageFormat.Json)]
        public int CreateProject(Project project, bool useLastProjectAsModel) {
     
            authorizationService.VerifyRequestAuthorizationToken();

            // creates the project
            project.CreateDate = System.DateTime.Now;
            project.CreateBy = authorizationService.SignedMemberProfile.MemberUId;
            project.Status = (short)ProjectStatus.PROPOSAL_CREATION;

            string formatedProjectName = FormatFolderString(project.ProjectName);
            string formatedClientName = FormatFolderString(project.ClientName);

            try {
                if (!String.IsNullOrEmpty(DocRepPath) && String.IsNullOrEmpty(project.DocRepositoryPath))
                    project.DocRepositoryPath = String.Format(DocRepPath, formatedClientName, formatedProjectName);
            }
            catch (Exception) { }

            // if using other project as model, copy roles from that
            if (useLastProjectAsModel) {
                Project similarProject = projectsRepository.GetLastSimilarProject(project);
                CopyProjectRoles(project, similarProject);
            }
            
            // checks if the project has at least one SM role
            Role SMRole = project.Roles.Where(r => r.PermissionSet == (short)PermissionSets.SCRUM_MASTER).FirstOrDefault();
            if (SMRole == null)
                throw new WebFaultException<String>("BRE_NEW_PROJECT_WITHOUT_SM_ROLE", System.Net.HttpStatusCode.BadRequest);

            // adds me as owner
            ProjectMembership ownerMember = new ProjectMembership();
            ownerMember.MemberUId = authorizationService.SignedMemberProfile.MemberUId;
            ownerMember.ProjectUId = project.ProjectUId;
            ownerMember.RoleUId = SMRole.RoleUId;
            ownerMember.DayAllocation = 0;
            ownerMember.IsActive = true;
            
            project.Memberships = new List<ProjectMembership>();
            project.Memberships.Add(ownerMember);

            // saves it
            int projectNumber = projectsRepository.InsertProject(project);

            // creates project folder
            CreateProjectFolder(project);

            // run create project hook
            RunCreateHook(project);

            return projectNumber;
        }

        private void CopyProjectRoles(Project project, Project similarProject) {
            if (similarProject == null)
                return;
            
            project.Roles.Clear();

            foreach (Role r in similarProject.Roles) {
                Role newRole = new Role() { 
                    RoleUId = Guid.NewGuid().ToString(),
                    ProjectUId = project.ProjectUId,
                    RoleName = r.RoleName,
                    RoleDescription = r.RoleDescription,
                    RoleShortName = r.RoleShortName,
                    PermissionSet = r.PermissionSet
                };
                project.Roles.Add(newRole);
            }
        }

        [WebGet(UriTemplate = "Projects/{projectUId}/Roles", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Role> GetProjectRoles(string projectUId) {

            authorizationService.VerifyRequestAuthorizationToken();

            return projectsRepository.GetProjectRoles(projectUId);
        }

        [WebInvoke(Method = "POST", UriTemplate = "Projects/{projectUId}/Roles", ResponseFormat = WebMessageFormat.Json)]
        public void CreateProjectRole(string projectUId, Role role) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            projectsRepository.SaveProjectRole(role);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Roles/{roleUId}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateProjectRole(string projectUId, string roleUId, Role role) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            // verifies if the role change may affect scrum master
            Project project = GetProject(projectUId);
            if (role.PermissionSet!=(short)PermissionSets.SCRUM_MASTER && !project.Memberships.Any(m => m.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER && m.RoleUId != roleUId)) {
                throw new WebFaultException<String>("BRE_SCRUM_MASTER_ROLE_IS_NECESSARY", System.Net.HttpStatusCode.BadRequest);
            }

            // if the saved role is default, set other ones to false
            if (role.IsDefaultRole) {             
                if (project.Roles != null) {
                    foreach (Role pr in project.Roles.Where(r => r.RoleUId != roleUId)) {
                        pr.IsDefaultRole = false;
                        projectsRepository.SaveProjectRole(pr);
                    }
                }
            }

            projectsRepository.SaveProjectRole(role);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "Projects/{projectUId}/Roles/{roleUId}", ResponseFormat = WebMessageFormat.Json)]
        public void DeleteProjectRole(string projectUId, string roleUId) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            using (TransactionScope scope = new TransactionScope()) {
                try {

                    // can delete if there is any member with this role
                    Project project = GetProject(projectUId);
                    if (project.Memberships.Any(ms => ms.RoleUId == roleUId))
                        throw new WebFaultException<String>("BRE_ROLE_ALREADY_USED", System.Net.HttpStatusCode.BadRequest);

                    // can delete if is the only scrum master role
                    if (project.Memberships.Count(ms => ms.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER && ms.RoleUId != roleUId) == 0)
                        throw new WebFaultException<String>("BRE_SCRUM_MASTER_ROLE_IS_NECESSARY", System.Net.HttpStatusCode.BadRequest);
                    
                    // make sure the role is not planned
                    if (!projectsRepository.CanDeleteProjectRole(projectUId, roleUId))
                        throw new WebFaultException<String>("BRE_ROLE_ALREADY_PLANNED", System.Net.HttpStatusCode.BadRequest);

                    // delete it
                    projectsRepository.DeleteProjectRole(roleUId);

                    scope.Complete();

                } finally { scope.Dispose(); }
            }
                
        }

        [WebInvoke(Method = "POST", UriTemplate = "Projects/{projectUId}/Sprints?addDefaultItems={addDefaultItems}&planItemName={planItemName}&planGroupName={planGroupName}&deliveryItemName={deliveryItemName}&deliveryGroupName={deliveryGroupName}", RequestFormat = WebMessageFormat.Json)]
        public ICollection<BacklogItem> AddSprint(string projectUId, Sprint sprint, bool addDefaultItems = false, string planItemName = null, string planGroupName = null, string deliveryItemName = null, string deliveryGroupName = null) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            
            projectsRepository.SaveSprint(sprint);

            if (addDefaultItems) {
                Project project = GetProject(projectUId);
                return backlogService.AddSprintDefaultItems(project, sprint.SprintNumber, planItemName, planGroupName, deliveryItemName, deliveryGroupName);                
            }

            return null;
        }
                
        
        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Sprints/{sprintUId}", RequestFormat = WebMessageFormat.Json)]
        public ICollection<Sprint> UpdateSprint(string projectUId, string sprintUId, Sprint sprint) {

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            ICollection<Sprint> sprints = GetSprints(projectUId);
            PushSprintDates(sprint.SprintNumber, sprint.StartDate, sprint.EndDate, sprints);

            foreach (Sprint s in sprints)
                projectsRepository.SaveSprint(s);

            return sprints;
        }


        private void PushSprintDates(int sprintNumber, System.DateTime newStartDate, System.DateTime newEndDate, ICollection<Sprint> sprints) {

            // make sure sprints are ordered by number
            Sprint[] orderedSprint = sprints.OrderBy(s => s.SprintNumber).ToArray();

            // finds the sprint
            Sprint sprint = orderedSprint.SingleOrDefault(s => s.SprintNumber == sprintNumber);
            if (sprint == null)
                return;

            if (sprintNumber >= 2) {
                Sprint previous = orderedSprint[sprintNumber - 2];
                if (newStartDate <= previous.StartDate.AddDays(1))
                    throw new WebFaultException<String>("BRE_SPRINT_START_DATE_NOT_ALLOWED", System.Net.HttpStatusCode.BadRequest);                    
            }

            if (newEndDate < sprint.StartDate)
                throw new WebFaultException<String>("BRE_SPRINT_END_DATE_NOT_ALLOWED", System.Net.HttpStatusCode.BadRequest);


            // get the number of days to push the sprints
            int pushDays = 0;
            int startDatePushDays = (sprintNumber == 1) ? newStartDate.Subtract(sprint.StartDate).Days : 0;

            // if is the first sprint and the start date was changed, it override the end date change
            if (startDatePushDays != 0) {
                newEndDate = newEndDate.AddDays(startDatePushDays);
                pushDays = startDatePushDays;
            } else {
                pushDays = newEndDate.Subtract(sprint.EndDate).Days;
            }

            if (newStartDate > newEndDate)
                throw new WebFaultException<String>("BRE_SPRINT_START_DATE_NOT_ALLOWED", System.Net.HttpStatusCode.BadRequest);

            // update the sprint start date
            sprint.StartDate = newStartDate;
            sprint.EndDate = newEndDate;   
            
            // pushs the following sprints, if this one is not the last
            if (sprintNumber <= orderedSprint.Length && pushDays != 0) {

                for (int i = sprintNumber; i < orderedSprint.Length; i++) {
                    int days = calendar.CalcWorkDayCount(orderedSprint[i].StartDate, orderedSprint[i].EndDate);
                    orderedSprint[i].StartDate = calendar.AddWorkDays(orderedSprint[i - 1].EndDate, 1); 
                    orderedSprint[i].EndDate = calendar.AddWorkDays(orderedSprint[i].StartDate, days);
                }
            }
                                    
            // moves the previus sprint end date one day before this one start date
            if (sprintNumber > 1)
                orderedSprint[sprintNumber - 2].EndDate = calendar.SubWorkDays(newStartDate, 1);

            

        }

        [WebGet(UriTemplate = "Projects/{projectUId}/Sprints", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Sprint> GetSprints(string projectUId) {

            // verify permission set            
            authorizationService.VerifyRequestAuthorizationToken();

            return projectsRepository.GetSprints(projectUId).OrderBy(s => s.SprintNumber).ToList();
        }

            
      

        [WebInvoke(Method = "DELETE", UriTemplate = "Projects/{projectUId}/Sprints/{sprintUId}", ResponseFormat = WebMessageFormat.Json)]
        public void RemoveSprint(string projectUId, string sprintUId) {

            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            Project project = GetProject(projectUId);
            Sprint sprintToBeDeleted = project.Sprints.Where(s => s.SprintUId == sprintUId).SingleOrDefault();

            // cannot delete the first sprint
            if (sprintToBeDeleted.SprintNumber <= project.CurrentPlanningNumber)
                throw new WebFaultException<string>("BRE_CAN_NOT_REMOVE_CURRENT_SPRINT", System.Net.HttpStatusCode.BadRequest);

            using (TransactionScope scope = new TransactionScope()) {

                // move sprint itens to previous sprint
                ICollection<BacklogItem> items = backlogService.GetBacklog(projectUId).Where(b => b.SprintNumber >= sprintToBeDeleted.SprintNumber).ToList();
                foreach (BacklogItem item in items)
                    backlogService.ChangeItemSprint(item.BacklogItemUId, (int)item.SprintNumber - 1, true);                

                // deletes the last sprint
                projectsRepository.DeleteSprint(project.LastSprint.SprintUId);

                scope.Complete();
            }

        }


        [WebGet(UriTemplate = "Clients", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<string> GetClientNames() {
            authorizationService.VerifyRequestAuthorizationToken();
            return projectsRepository.GetClientNames();
        }

        [WebGet(UriTemplate = "Platforms", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<string> GetPlatforms() {
            authorizationService.VerifyRequestAuthorizationToken();
            return projectsRepository.GetPlatforms();
        }

        [WebGet(UriTemplate = "SimilarProjects/{projectUId}/?onlyWithProposals={onlyWithProposals}", ResponseFormat = WebMessageFormat.Json)]
        public Project GetLastSimilarProject(string projectUId, bool onlyWithProposals = false) {
            authorizationService.VerifyRequestAuthorizationToken();
            Project p = GetProject(projectUId);
            return projectsRepository.GetLastSimilarProject(p, onlyWithProposals);
        }

        [WebGet(UriTemplate = "Projects/{projectUId}/Risks", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Risk> GetProjectRisks(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return projectsRepository.GetProjectRisks(projectUId);
        }

        [WebInvoke(Method = "POST", UriTemplate = "Projects/{projectUId}/Risks", RequestFormat = WebMessageFormat.Json)]
        public void AddRisk(string projectUId, Risk risk) {
            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            risk.CreateDate = DateTime.Now;            
            projectsRepository.SaveRisk(risk);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Risks/{riskUId}", RequestFormat = WebMessageFormat.Json)]
        public void UpdateRisk(string projectUId, string riskUId, Risk risk) {
            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            risk.UpdatedAt = DateTime.Now;
            projectsRepository.SaveRisk(risk);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Risks/{riskUId}/Impact", RequestFormat = WebMessageFormat.Json)]
        public void ChangeRiskImpact(string projectUId, string riskUId, short impact) {
            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            Risk risk = projectsRepository.GetRisk(riskUId);
            risk.Impact = impact;
            risk.UpdatedAt = DateTime.Now;
            projectsRepository.SaveRisk(risk);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Risks/{riskUId}/Probability", RequestFormat = WebMessageFormat.Json)]
        public void ChangeRiskProbability(string projectUId, string riskUId, short probability) {
            // verify permission set            
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            Risk risk = projectsRepository.GetRisk(riskUId);
            risk.Probability = probability;
            risk.UpdatedAt = DateTime.Now;
            projectsRepository.SaveRisk(risk);
        }

        [WebGet(UriTemplate="UserProjectEvents", ResponseFormat=WebMessageFormat.Json)]
        public ICollection<ProjectEvent> GetUserProjectEvents() {
            authorizationService.VerifyRequestAuthorizationToken();
            return projectsRepository.GetUserProjectEvents(authorizationService.SignedMemberProfile.MemberUId);
        }

        private void RunCreateHook(Project project) {

            if (DefaultCompanyName == "_LOCAL_SERVER_")
                return;

            if (System.Web.HttpContext.Current == null)
                return;

            string formatedProjectName = FormatFolderString(project.ProjectName);
            string formatedClientName = FormatFolderString(project.ClientName);

            string hookPath = System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\Hooks");
            string args = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\"\" ", project.ClientName, project.ProjectName, formatedClientName, formatedProjectName, project.CodeRepositoryPath);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("CreateProject.bat", args);

            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;
            psi.WorkingDirectory = hookPath;
            
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;            
            try {
                p.Start();
            }
            catch (Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
                return;
            }
           
            //if (p.ExitCode != 0)
                //LogError(null, "CreateProject.Bat exit with error " + p.ExitCode);

        }

        private string FormatFolderString(string text) {
            if (text == null)
                return string.Empty;

            // remove accents
            System.Text.RegularExpressions.Regex nonSpacingMarkRegex = new System.Text.RegularExpressions.Regex(@"\p{Mn}", System.Text.RegularExpressions.RegexOptions.Compiled);
            var normalizedText = text.Normalize(System.Text.NormalizationForm.FormD);
            normalizedText = nonSpacingMarkRegex.Replace(normalizedText, string.Empty);

            // replace spaces for _
            normalizedText = normalizedText.Replace(" ", "_");

            normalizedText = normalizedText.Replace("\"", "");
            normalizedText = normalizedText.Replace("'", "");
            normalizedText = normalizedText.Replace("\\", "");
            normalizedText = normalizedText.Replace("/", "");

            return normalizedText;
        }

        [WebInvoke(Method = "POST", UriTemplate = "Projects/{projectUId}/Memberships", ResponseFormat = WebMessageFormat.Json)]
        public void AddProjectMembership(string projectUId, ProjectMembership membership) {

            authorizationService.VerifyRequestAuthorizationToken();

            // gets the role
            Role role = projectsRepository.GetProjectRole(membership.RoleUId);
            bool isNotTeamRole = role.PermissionSet != (short)PermissionSets.TEAM;

            bool isScrumMaster = MemberHasPermissionAtProject(authorizationService.SignedMemberProfile.MemberUId, projectUId, new PermissionSets[] {  PermissionSets.SCRUM_MASTER });
            bool isFactoryOwner = authorizationService.SignedMemberProfile.IsFactoryOwner;
            
            // If not a team role, you need to be SM or FACTORY OWNER
            if(isNotTeamRole && !isScrumMaster && !isFactoryOwner)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);

            // if is a Team role any one can join
            SetMemberWorkload(membership.MemberUId, membership.ProjectUId, membership.RoleUId, membership.DayAllocation);

            // if its myself, dont need to send an email
            if (membership.MemberUId == authorizationService.SignedMemberProfile.MemberUId)
                return;

            // send email at other thread, so all the database query stuff does not lock this return
            //string serverUrl = Helper.ReportTemplate.ServerUrl;
            //System.Threading.ThreadStart sendMail = delegate { SendInviteMemberEmail(membership, serverUrl); };
            //new System.Threading.Thread(sendMail).Start();
            SendInviteMemberEmail(membership, Helper.ReportTemplate.ServerUrl);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Projects/{projectUId}/Memberships/{memberUId}/Roles/{roleUId}/DayAllocation", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateProjectMembershipAllocation(string projectUId, string memberUId, string roleUId, int dayAllocation) {
            authorizationService.VerifyUserOrPermissionAtProject(memberUId, projectUId, PermissionSets.SCRUM_MASTER);
            SetMemberWorkload(SafeMemberUId(memberUId), projectUId, roleUId, dayAllocation);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "Projects/{projectUId}/Memberships/{memberUId}/Roles/{roleUId}", ResponseFormat = WebMessageFormat.Json)]
        public void RemoveProjectMembership(string projectUId, string memberUId, string roleUId) {

            authorizationService.VerifyUserOrPermissionAtProject(memberUId, projectUId, PermissionSets.SCRUM_MASTER);

            Project project = GetProject(projectUId);
            if (project == null)
                throw new WebFaultException<string>("Project not found", System.Net.HttpStatusCode.NotFound);

            ProjectMembership membership = project.Memberships.Where(ms => ms.MemberUId == memberUId && ms.RoleUId == roleUId).SingleOrDefault();
            if (membership == null)
                throw new WebFaultException<string>("Project membership not found", System.Net.HttpStatusCode.NotFound);

            // if is a SCRUM MASTER membership, checks if is not the only one
            if (membership.Role.PermissionSet == (short)PermissionSets.SCRUM_MASTER
                && projectsRepository.GetMembershipCountOf(membership.ProjectUId, PermissionSets.SCRUM_MASTER) == 1)
                throw new WebFaultException<string>("BRE_CAN_NOT_REMOVE_LAST_SCRUM_MASTER", System.Net.HttpStatusCode.BadRequest);

            // if the member has tasks already, inactive it only
            if (tasksService.DoesMemberHasAnyTaskAtProject(membership.ProjectUId, memberUId)) {
                membership.IsActive = false;
                membership.InactiveSince = DateTime.Now;
            }
            // otherwise, delete it
            else
                project.Memberships.Remove(membership);

            projectsRepository.UpdateProject(project);
        }



        private void SetMemberWorkload(string memberUId, string projectUId, string roleUId, int? newAllocation) {

            Project project = GetProject(projectUId);
            if (project ==null)
                throw new WebFaultException<string>("Project not found", System.Net.HttpStatusCode.NotFound);

            int? deltaAllocation = 0;

            ProjectMembership membership = project.Memberships.Where(ms => ms.MemberUId == memberUId && ms.RoleUId == roleUId).SingleOrDefault();
            if (membership == null) {
                membership = new ProjectMembership() { ProjectUId = projectUId, MemberUId = SafeMemberUId(memberUId), RoleUId = roleUId };
                if (project.Memberships == null)
                    project.Memberships = new List<ProjectMembership>();
                project.Memberships.Add(membership);

                // if is new membership, then the delta is the new allocation
                deltaAllocation = newAllocation;
            }
            else {
                // if is a update at a membership, then the delta is the difference (new one - old one)
                if (!membership.DayAllocation.HasValue)
                    deltaAllocation = newAllocation;
                else
                    deltaAllocation = newAllocation - membership.DayAllocation;
            }

            if (!membership.IsActive)
                membership.IsActive = true;

            int actualAllocation = projectsRepository.GetMemberDayAllocation(memberUId);

            if (actualAllocation + deltaAllocation > 4)
                throw new WebFaultException<String>("BRE_INVALID_MEMBER_WORKLOAD", System.Net.HttpStatusCode.BadRequest);

            membership.DayAllocation = newAllocation;
            
            projectsRepository.UpdateProject(project);
        }


        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets[] permissions) {
            return projectsRepository.MemberHasPermissionAtProject(memberUId, projectUId, permissions);
        }

        public bool MemberHasPermissionAtProject(string memberUId, string projectUId, PermissionSets permission) {
            return projectsRepository.MemberHasPermissionAtProject(memberUId, projectUId, permission);
        }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("@@@", "#").Replace("@@", "\\").Trim();
        }

        [WebGet(UriTemplate = "SimilarCodeRepositories/?projectName={projectName}&clientName={clientName}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<string> GetSimilarCodeRepositories(string projectName, string clientName) {
            authorizationService.VerifyRequestAuthorizationToken();
            return projectsRepository.GetSimilarCodeRepositories(projectName, clientName);
        }


        [WebInvoke(Method = "POST", UriTemplate = "Projects/{projectUId}/CodeRepositoryPath/", ResponseFormat = WebMessageFormat.Json)]
        public void SetCodeRepositoryPath(string projectUId, string svnTrunk) {

            if (svnTrunk == null)
                throw new WebFaultException<string>("Invalid SVN path", System.Net.HttpStatusCode.BadRequest);

            Project project = GetProject(projectUId);

            project.CodeRepositoryPath = svnTrunk;
            projectsRepository.UpdateProject(project);

        }

        [WebGet(UriTemplate = "SVNSettings/", ResponseFormat = WebMessageFormat.Json)]
        public SVNSettings GetSVNSettings() {
            return SVNSettings.Read();
        }

        private void CreateProjectFolder(Project project) {
            if (String.IsNullOrEmpty(project.DocRepositoryPath))
                return;

            System.Threading.Tasks.Task.Factory.StartNew(() => {
                try {
                    // creates the folder
                    string path = project.DocRepositoryPath;
                    System.IO.Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
                }
            });

        }
    }
}
