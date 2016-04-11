using System.ServiceModel;
using System.Collections.Generic;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface ITasksService {

        [OperationContract]
        ICollection<Task> GetProjectTasks(string projectUId, System.DateTime fromDate, System.DateTime toDate, bool dailyTasksOnly, string reviewTagUId);

        [OperationContract]
        ICollection<Task> GetItemTasks(string backlogItemUId);

        [OperationContract]
        ICollection<Task> GetUserTasks(string memberUId, bool onlyOpen= true, bool includeUnassigned = false);

        [OperationContract]
        ICollection<Task> GetUserOwnedTasks(string memberUId);

        [OperationContract]
        ICollection<TaskTag> GetTaskTags(string projectUId, string filter = "ALL");

        [OperationContract]
        TaskTag AddTaskTag(string projectUId, TaskTag newtag);

        [OperationContract]
        void UpdateTaskTag(string projectUId, string tagUId, TaskTag tag);

        [OperationContract]
        void RemoveTaskTag(string projectUId, string tagUId);
      

        [OperationContract]
        ICollection<BacklogItemEffectiveHours> GetItemTasksEffectiveHoursByProject(string projectUId);

     
        [OperationContract]
        int CreateTask(Task task);

        [OperationContract]
        void SaveTask(Task task, bool replanItem);

        [OperationContract]
        Task GetTask(string taskUId);

        [OperationContract]
        TaskDetail GetTaskDetail(string taskUId);

        [OperationContract]
        System.DateTime ChangeTaskStatus(string taskUId, decimal addingHours, short status);

        [OperationContract]
        void ChangeTaskAssigneeUId(string taskUId, string taskAssigneeUId, bool replanItem);

        [OperationContract]
        void ChangeTaskName(string taskUId, string taskName);

        [OperationContract]
        void ChangeTaskItem(string taskUId, string backlogItemUId);

        [OperationContract]
        void ChangeTaskPlannedHours(string taskUId, decimal hours, bool replanItem);

        [OperationContract]
        void ChangeTaskEffectiveHours(string taskUId, decimal hours, bool append);

        [OperationContract]
        void ChangeTaskTrackStatus(string taskUId, bool finishAlso, bool isTracking);

        [OperationContract]
        decimal GetReworkIndicator(string projectUId);

        bool DoesBacklogItemHasTasks(string backlogItemUId);

        bool DoesMemberHasAnyTaskAtProject(string projectUId, string memberUId);

        decimal GetProjectTotalEffectiveHours(string projectUId);

        decimal GetProjectTasksPrice(RoleHourCost[] costs);

        decimal GetTotalEffectiveHoursByItem(string backlogitemUId);

        [OperationContract]
        IDictionary<string, decimal> GetProjectTotalEffectiveHoursByMember(string projectUId, string memberUId);

    }
}
