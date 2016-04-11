using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;
using System.Collections.Concurrent;

namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(ITasksService))]
    [Export(typeof(ITasksService_ServerSide))]
    public class TasksService : ITasksService_ServerSide, ITasksService {

        [Import(typeof(Data.ITasksRepository))]
        private Data.ITasksRepository tasksRepository { get; set; }

        
        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizationService { get; set; }

     

        [Import]
        private IProjectsService projectsService { get; set; }

        [Import]
        private IBacklogService_ServerSide backlogService { get; set; }

        [Import]
        private ITeamService teamService { get; set; }

        private string SafeMemberUId(string memberUId) {
            return memberUId.Replace("@@@", "#").Replace("@@", "\\").Trim();
        }

        public static readonly ConcurrentDictionary<string, DateTime> TaskTrack = new ConcurrentDictionary<string, DateTime>();

        [WebGet(UriTemplate = "ProjectTasks/{projectUId}/?fromDate={fromDate}&toDate={toDate}&dailyTasksOnly={dailyTasksOnly}&reviewTagUId={reviewTagUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Task> GetProjectTasks(string projectUId, System.DateTime fromDate, System.DateTime toDate, bool dailyTasksOnly, string reviewTagUId) {

            authorizationService.VerifyRequestAuthorizationToken();

            return tasksRepository.GetProjectTasks(projectUId, fromDate, toDate, dailyTasksOnly, reviewTagUId);
        }

        [WebGet(UriTemplate = "ItemTasks/{backlogItemUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Task> GetItemTasks(string backlogItemUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetItemTasks(backlogItemUId);
        }

        // CANT HAVE THE /? HER FOR ERALY VERSIONS COMPATIBILITY
        [WebGet(UriTemplate = "UserTasks/{memberUId}?onlyOpen={onlyOpen}&includeUnassigned={includeUnassigned}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Task> GetUserTasks(string memberUId, bool onlyOpen = true, bool includeUnassigned = false) {
            authorizationService.VerifyRequestAuthorizationToken();

            string[] userProjectUIds = null;
            if (includeUnassigned) {
                var userProjects = projectsService.GetProjects(null, null, "RUNNING_PROJECTS", memberUId);
                userProjectUIds = userProjects.Select(p => p.ProjectUId).ToArray();
            }

            return tasksRepository.GetUserTasks(memberUId, onlyOpen, includeUnassigned, userProjectUIds);
        }


        [WebGet(UriTemplate = "MemberPerformance/{memberUId}", ResponseFormat = WebMessageFormat.Json)]
        public MemberPerformance GetMemberPerformance(string memberUId) {
            // no authenticaton here, cuz it is used at sign in method            
            return tasksRepository.GetMemberPerformance(SafeMemberUId(memberUId));
        }

        public ICollection<Task> GetUsersTasks(string[] memberUIds, bool onlyOpen, bool includeUnassigned) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetUsersTasks(memberUIds, onlyOpen, includeUnassigned);
        }

        [WebGet(UriTemplate = "UserOwnedTasks/{memberUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Task> GetUserOwnedTasks(string memberUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetUserOwnedTasks(memberUId);
        }

        [WebGet(UriTemplate = "TaskTags/{projectUId}/?filter={filter}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<TaskTag> GetTaskTags(string projectUId, string filter = "ALL") {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetTaskTags(projectUId, filter);
        }

        [WebInvoke(Method="POST", UriTemplate = "TaskTags/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public TaskTag AddTaskTag(string projectUId, TaskTag newtag) {
            authorizationService.VerifyPermissionAtProject(projectUId, new PermissionSets[] { PermissionSets.TEAM, PermissionSets.SCRUM_MASTER });
            newtag.CreatedAt = DateTime.Now;
            tasksRepository.SaveTaskTag(newtag);
            return newtag;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "TaskTags/{projectUId}/{tagUId}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateTaskTag(string projectUId, string tagUId, TaskTag tag) {
            authorizationService.VerifyPermissionAtProject(projectUId, new PermissionSets[] { PermissionSets.TEAM, PermissionSets.SCRUM_MASTER });
            tasksRepository.SaveTaskTag(tag);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "TaskTags/{projectUId}/{tagUId}", ResponseFormat = WebMessageFormat.Json)]
        public void RemoveTaskTag(string projectUId, string tagUId) {
            authorizationService.VerifyPermissionAtProject(projectUId, new PermissionSets[] { PermissionSets.TEAM, PermissionSets.SCRUM_MASTER });
            tasksRepository.DeleteTaskTag(tagUId);
        }

        [WebGet(UriTemplate = "TaskEffectiveHours/?projectUId={projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<BacklogItemEffectiveHours> GetItemTasksEffectiveHoursByProject(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetItemTasksEffectiveHoursByProject(projectUId);
        }

        public decimal GetTotalEffectiveHoursByItem(string backlogitemUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetTotalEffectiveHoursByItem(backlogitemUId);
        }

     

        public ICollection<TodayMemberPlannedHours> GetTodayMemberPlannedHours(string[] membersUIds) {

            return tasksRepository.GetTodayMemberPlannedHoursByUIds(membersUIds);
        }

        [WebInvoke(Method="POST", UriTemplate = "Tasks/", ResponseFormat = WebMessageFormat.Json)]        
        public int CreateTask(Task task) {

            authorizationService.VerifyRequestAuthorizationToken();

            // if not a SCRUM MASTER can not create tasks for others
            if (!authorizationService.IsProjectScrumMaster(task.ProjectUId) && task.TaskAssigneeUId != authorizationService.SignedMemberProfile.MemberUId)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);

            Project project = projectsService.GetProject(task.ProjectUId);
            BacklogItem item = backlogService.GetBacklogItem(task.BacklogItemUId);
            
            //task.Status = (short)TaskStatus.REQUIRED_TASK; // no with paste, we can create task with other status
            task.CreatedAt = System.DateTime.Now;
            task.TaskOwnerUId = authorizationService.SignedMemberProfile.MemberUId;

            tasksRepository.SaveTask(task);
            
            return task.TaskNumber;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/?replanItem={replanItem}", ResponseFormat = WebMessageFormat.Json)]        
        public void SaveTask(Task task, bool replanItem) {

            Task oldTask = GetTask(task.TaskUId);

            VerifyIfCanEditTask(oldTask);

            Project project = projectsService.GetProject(oldTask.ProjectUId);                        
            if (task.RoleUId == null)
                SetRoleAccordingAssignee(task, project, false);

            tasksRepository.SaveTask(task);

            if (replanItem)
                ReplanBacklogItem(task, oldTask.RoleUId, oldTask.PlannedHours);

            // now changes the item status to working
            backlogService.UpdateItemStatusToWorking(task);
            
        }


        private void ReplanBacklogItem(Task task, string oldRoleUId, decimal oldHours) {
            
            decimal delta = task.PlannedHours - oldHours;

            // if role was changed, remove hours from old role
            if (oldRoleUId  != task.RoleUId && !String.IsNullOrEmpty(oldRoleUId)) {
                backlogService.AddPlannedHoursToItem(task.BacklogItemUId, oldRoleUId, -oldHours);
                delta = task.PlannedHours; 
            }

            if (delta != 0 && !String.IsNullOrEmpty(task.RoleUId))
                backlogService.AddPlannedHoursToItem(task.BacklogItemUId, task.RoleUId, delta);
        }

        [WebGet(UriTemplate = "Tasks/{taskUId}", ResponseFormat = WebMessageFormat.Json)]
        public Task GetTask(string taskUId) {
            authorizationService.VerifyRequestAuthorizationToken();

            int number;
            int.TryParse(taskUId, out number);

            if (number > 0)
                return tasksRepository.GetTaskByNumber(number);

            return tasksRepository.GetTask(taskUId);
        }


        [WebGet(UriTemplate = "Tasks/{taskUId}/TaskDetail", ResponseFormat = WebMessageFormat.Json)]
        public TaskDetail GetTaskDetail(string taskUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetTaskDetail(taskUId);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/Status/?addingHours={addingHours}", ResponseFormat = WebMessageFormat.Json)]        
        public DateTime ChangeTaskStatus(string taskUId, decimal addingHours, short status) {

            Task task = GetTask(taskUId);
            VerifyIfCanEditTask(task);
            
            System.DateTime now = System.DateTime.Now;
            task.Status = status;

            task.AdjustDateWithStatus(now);
            task.EffectiveHours = task.EffectiveHours + addingHours;
            tasksRepository.SaveTask(task);

            // now changes the item status to working
            backlogService.UpdateItemStatusToWorking(task);

            return now;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/TaskAssigneeUId/?replanItem={replanItem}", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskAssigneeUId(string taskUId, string taskAssigneeUId, bool replanItem) {

            Task task = GetTask(taskUId);

            if (task.TaskAssigneeUId == taskAssigneeUId)
                return;

            Project project = projectsService.GetProject(task.ProjectUId);

            VerifyIfCanChangeAssignee(task, taskAssigneeUId, project);
            
            task.TaskAssigneeUId = taskAssigneeUId;
            SetRoleAccordingAssignee(task, project, replanItem);


            tasksRepository.SaveTask(task);
        }

        private void SetRoleAccordingAssignee(Task task, Project project, bool replanItem) {
            
            ProjectMembership membership = project.Memberships.Where(ms => ms.MemberUId == task.TaskAssigneeUId && ms.IsActive==true).FirstOrDefault();
            if (membership == null)
                return;
            string oldRoleUId = task.RoleUId;
            task.RoleUId = membership.RoleUId;

            if (replanItem)
                ReplanBacklogItem(task, oldRoleUId, task.PlannedHours);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/TaskName", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskName(string taskUId, string taskName) {

            Task task = GetTask(taskUId);
            VerifyIfCanEditTask(task);

            task.TaskName = taskName;
            tasksRepository.SaveTask(task);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/BacklogItemUId", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskItem(string taskUId, string backlogItemUId) {

            Task task = GetTask(taskUId);
            VerifyIfCanEditTask(task);

            task.BacklogItemUId = backlogItemUId;
            tasksRepository.SaveTask(task);
        }


        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/PlannedHours/?replanItem={replanItem}", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskPlannedHours(string taskUId, decimal plannedHours, bool replanItem) {

            Task task = GetTask(taskUId);
            VerifyIfCanEditTask(task);

            decimal oldHours = task.PlannedHours;

            task.PlannedHours = plannedHours;
            tasksRepository.SaveTask(task);

            if(replanItem)
                ReplanBacklogItem(task, task.RoleUId, oldHours);
                
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Tasks/{taskUId}/EffectiveHours/?append={append}", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskEffectiveHours(string taskUId, decimal effectiveHours, bool append) {

            Task task = GetTask(taskUId);
            VerifyIfCanEditTask(task);

            if(append)
                task.EffectiveHours = task.EffectiveHours + effectiveHours;
            else
                task.EffectiveHours = effectiveHours;

            if (task.EffectiveHours > 0 && task.Status == (short)TaskStatus.REQUIRED_TASK) {
                task.Status = (short)TaskStatus.WORKING_ON_TASK;
                task.AdjustDateWithStatus(System.DateTime.Now);
            }

            tasksRepository.SaveTask(task);

            // now changes the item status to working
            backlogService.UpdateItemStatusToWorking(task);
        }


        

        private void VerifyIfCanEditTask(Task task) {

            authorizationService.VerifyRequestAuthorizationToken();

            if (!authorizationService.IsProjectScrumMaster(task.ProjectUId) && task.TaskAssigneeUId != authorizationService.SignedMemberProfile.MemberUId)
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        private void VerifyIfCanChangeAssignee(Task task, string newAssignee, Project project) {

            authorizationService.VerifyRequestAuthorizationToken();

            // if is the Scrum Master, ok
            if (authorizationService.IsProjectScrumMaster(task.ProjectUId))
                return;

            // is the task is not assigned yet and im at the project, ok
            if (task.TaskAssigneeUId == null && project.Memberships.Any(m => m.MemberUId==authorizationService.SignedMemberProfile.MemberUId))
                return;

            // if the task is mine, and i´m releasing it, ok
            if (task.TaskAssigneeUId == authorizationService.SignedMemberProfile.MemberUId && newAssignee == null)
                return;

            // else, youc cant
            throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
        }

        [WebInvoke(Method = "POST", UriTemplate = "Tasks/{taskUId}/IsTracking/?finishAlso={finishAlso}", ResponseFormat = WebMessageFormat.Json)]
        public void ChangeTaskTrackStatus(string taskUId, bool finishAlso, bool isTracking) {
            
            // if its is a start track, just put it at the dictionary
            if (isTracking) {
                TaskTrack[taskUId] = DateTime.Now;
                return;
            }

            // if is stoping to track, calcs the time
            DateTime iniTime = TaskTrack[taskUId];
            decimal addingHours = (decimal) DateTime.Now.Subtract(iniTime).TotalHours;

            // removes from track
            DateTime dateRemoved;
            TaskTrack.TryRemove(taskUId, out dateRemoved);
             
            // if should finish also, finish it and add the hours
            if (finishAlso) {
                ChangeTaskStatus(taskUId, addingHours, (short)TaskStatus.DONE_TASK);                
                return;
            }

            // gets the task and adds the hours
            Task task = GetTask(taskUId);            
            ChangeTaskEffectiveHours(taskUId, addingHours, true);                            
            
        }

        public decimal GetProjectTasksPrice(RoleHourCost[] costs) {
            decimal price = 0;
            foreach (RoleHourCost roleCost in costs) {
                decimal hours = tasksRepository.GetTotalEffectiveHoursByRole(roleCost.RoleUId);
                price = price + hours * roleCost.Price;
            }
            return price;
        }

        public decimal GetReworkIndicator_skipAuth(string projectUId) {
            decimal total = tasksRepository.GetTotalEffectiveHoursByProject(projectUId);
            if (total == 0)
                return 0;

            decimal bugs = tasksRepository.GetTotalBugHoursByProject(projectUId);

            return bugs / total * 100;
        }

        [WebGet(UriTemplate = "ReworkIndicator/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public decimal GetReworkIndicator(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return GetReworkIndicator_skipAuth(projectUId);
            
        }

        [WebGet(UriTemplate = "ProjectTotalEffectiveHours/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public decimal GetProjectTotalEffectiveHours(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return GetProjectTotalEffectiveHours_skipAuth(projectUId);
        }

        public decimal GetProjectTotalEffectiveHours_skipAuth(string projectUId) {
            return tasksRepository.GetTotalEffectiveHoursByProject(projectUId);            
        }

        public bool DoesBacklogItemHasTasks(string backlogItemUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.DoesBacklogItemHasTasks(backlogItemUId);
        }

        public bool DoesMemberHasAnyTaskAtProject(string projectUId, string memberUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.DoesMemberHasAnyTaskAtProject(projectUId, memberUId);
        }

        public void UpdateTaskArtifactCount(string taskUId, int count) {
            tasksRepository.UpdateTaskArtifactCount(taskUId, count);
        }


        public PlannedHour[] GetTotalEffectiveHoursByItem(string backlogitemUId, ICollection<Role> roles) {
            authorizationService.VerifyRequestAuthorizationToken();
            return tasksRepository.GetTotalEffectiveHoursByItem(backlogitemUId, roles);
        }

        [WebGet(UriTemplate = "ProjectTotalEffectiveHoursByMember/{projectUId}/?memberUId={memberUId}",BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        public IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string projectUId, string memberUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            if (String.IsNullOrEmpty(memberUId))
                memberUId = null;
            IDictionary<string, decimal> hoursByMember = tasksRepository.GetTotalEffectiveHoursByProjectAndMember(projectUId, memberUId);            
            return hoursByMember;
        }
    }
}
