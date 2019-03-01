using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(ITasksService))]
    public class TasksServiceClient : ITasksService {

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("TasksServiceUrl")]
        private string serviceUrl { get; set; }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        private readonly object TASK_UPDATE_LOCK = new object();

        private string SafeMemberUId(string memberUId) {
            if (memberUId == null)
                return null;
            return memberUId.Replace("#", "@@@").Replace("\\", "@@").Trim();
        }


        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        public ICollection<Task> GetProjectTasks(string projectUId, System.DateTime fromDate, System.DateTime toDate, bool dailyTasksOnly, string reviewTagUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("ProjectTasks/" + projectUId + "/?fromDate=" + fromDate.ToString("yyyy-MM-dd") + "&toDate=" + toDate.ToString("yyyy-MM-dd") + "&dailyTasksOnly=" + dailyTasksOnly + "&reviewTagUId=" + reviewTagUId) );
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Task>>();
        }

        public ICollection<Task> GetItemTasks(string backlogitemUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("ItemTasks/" + backlogitemUId));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Task>>();
        }

        public ICollection<Task> GetUserTasks(string memberUId, bool onlyOpen = true, bool includeUnassigned = false) {
            var client = ClientHelper.GetClient(authorizator);
            Uri u = Url("UserTasks/" + SafeMemberUId(memberUId) + "?onlyOpen=" + onlyOpen + "&includeUnassigned=" + includeUnassigned);
             HttpResponseMessage response = client.Get(u);
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Task>>();
        }

        public ICollection<Task> GetUserOwnedTasks(string memberUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("UserOwnedTasks/" + SafeMemberUId(memberUId)));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Task>>();
        }

        public ICollection<TaskTag> GetTaskTags(string projectUId, string filter = "ALL") {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("TaskTags/" + projectUId) + "/?filter=" + filter);
            // old server versions does not have thsi feature
            ClientHelper.HandleHTTPErrorCode(response, true);
            if (!response.IsSuccessStatusCode)
                return new TaskTag[0];
            return response.Content.ReadAs<ICollection<TaskTag>>();
        }

        public TaskTag AddTaskTag(string projectUId, TaskTag newtag) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("TaskTags/" + projectUId), new ObjectContent<TaskTag>(newtag, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response, true);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                throw new ScrumFactory.Exceptions.OldServerException();
            }
            return response.Content.ReadAs<TaskTag>();
        }

        public void UpdateTaskTag(string projectUId, string tagUId, TaskTag tag) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("TaskTags/" + projectUId + "/" + tagUId), new ObjectContent<TaskTag>(tag, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void RemoveTaskTag(string projectUId, string tagUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("TaskTags/" + projectUId + "/" + tagUId));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public ICollection<Task> GetTeamTasks(string teamCode, bool onlyOpen, bool excludeMe) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("TeamTasks/" + teamCode + "/?onlyOpen=" + onlyOpen + "&excludeMe=" + excludeMe));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Task>>();
        }

        public Task GetTask(string taskUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Tasks/" + taskUId));
            ClientHelper.HandleHTTPErrorCode(response);            
            return response.Content.ReadAs<Task>();
        }

        

        public TaskDetail GetTaskDetail(string taskUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Tasks/" + taskUId + "/TaskDetail"));
            ClientHelper.HandleHTTPErrorCode(response);
            if (response.Content.ContentReadStream.Length == 0)
                return null;
            return response.Content.ReadAs<TaskDetail>();
        }

        public ICollection<BacklogItemEffectiveHours> GetItemTasksEffectiveHoursByProject(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("TaskEffectiveHours/?projectUId=" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItemEffectiveHours>>();
        }

        public int CreateTask(Task task) {
            var client = ClientHelper.GetClient(authorizator);            
            HttpResponseMessage response = client.Post(Url("Tasks/"), new ObjectContent<Task>(task, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            task.TaskNumber = response.Content.ReadAs<int>();
            return task.TaskNumber;
        }

        public void SaveTask(Task task, bool replanItem) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = ClientHelper.SafePut<Task>(TASK_UPDATE_LOCK, client, Url("Tasks/?replanItem=" + replanItem), task);                
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ChangeTaskAssigneeUId(string taskUId, string taskAssigneeUId, bool replanItem) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = ClientHelper.SafePut<string>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/TaskAssigneeUId/?replanItem=" + replanItem), taskAssigneeUId);                
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ChangeTaskName(string taskUId, string taskName) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = ClientHelper.SafePut<string>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/TaskName"), taskName);                
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeTaskType(string taskUId, short type) {
            throw new NotImplementedException();
        }

        public void ChangeTaskItem(string taskUId, string backlogItemUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = ClientHelper.SafePut<string>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/BacklogItemUId"), backlogItemUId);                
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public System.DateTime ChangeTaskStatus(string taskUId, decimal addingHours, short status) {            
            var client = ClientHelper.GetClient(authorizator);
            string decimaStr = addingHours.ToString(System.Globalization.CultureInfo.InvariantCulture);
            HttpResponseMessage response = ClientHelper.SafePut<short>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/Status/?addingHours=" + decimaStr), status);
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<System.DateTime>();
        }

        public void ChangeTaskPlannedHours(string taskUId, decimal plannedHours, bool replanItem) {            
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = ClientHelper.SafePut<decimal>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/PlannedHours/?replanItem=" + replanItem), plannedHours);            
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeTaskEffectiveHours(string taskUId, decimal effectiveHours, bool append) {
            var client = ClientHelper.GetClient(authorizator);                                                            
            HttpResponseMessage response = ClientHelper.SafePut<decimal>(TASK_UPDATE_LOCK, client, Url("Tasks/" + taskUId + "/EffectiveHours/?append=" + append), effectiveHours);            
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public void ChangeTaskTrackStatus(string taskUId, bool finishAlso, bool isTracking) {            
            var client = ClientHelper.GetClient(authorizator);                                                
            string finishAlsoStr = finishAlso.ToString(System.Globalization.CultureInfo.InvariantCulture);
            HttpResponseMessage response = client.Post(Url("Tasks/" + taskUId + "/IsTracking/?finishAlso="+ finishAlsoStr), new ObjectContent<bool>(isTracking, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }

        public decimal GetReworkIndicator(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("ReworkIndicator/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<decimal>();
        }

        public decimal GetProjectTotalEffectiveHours(string projectUId) {
            throw new NotImplementedException();
        }

        public decimal GetProjectTasksPrice(RoleHourCost[] costs) {
            throw new NotImplementedException();
        }

        public decimal GetTotalEffectiveHoursByItem(string backlogitemUId) {
            throw new NotImplementedException();
        }

        public bool DoesBacklogItemHasTasks(string backlogItemUId) {
            throw new NotImplementedException();
        }

        public bool DoesMemberHasAnyTaskAtProject(string projectUId, string memberUId) {
            throw new NotImplementedException();
        }

        public IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string projectUId, string memberUId) {
            var client = ClientHelper.GetClient(authorizator);            
            HttpResponseMessage response = client.Get(Url("ProjectTotalEffectiveHoursByMember/" + projectUId + "/?memberUId=" + SafeMemberUId(memberUId)));
            
            // workaround - for old server versions
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<IDictionary<string, decimal>>();
        }

        public MemberPerformance GetMemberPerformance(string memberUId)
        {
            throw new NotImplementedException();
        }
    }
}
