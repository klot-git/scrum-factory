namespace ScrumFactory.Data {
    using System.Collections.Generic;

    public interface ITasksRepository {

        ICollection<Task> GetItemTasks(string backlogItemUId);

        ICollection<Task> GetUserTasks(string taskAssigneeUId, bool onlyOpen, bool includeUnassigned, string[] projectUIds = null);

        MemberPerformance GetMemberPerformance(string memberUId);

        ICollection<Task> GetUserOwnedTasks(string taskOwnerUId);

        ICollection<Task> GetUsersTasks(string[] membersUIds, bool onlyOpen, bool includeUnassigned, string[] projectUIds = null);

        ICollection<Task> GetProjectTasks(string projectUId, System.DateTime fromDate, System.DateTime toDate, bool dailyTasksOnly, string reviewTagUId);

        ICollection<BacklogItemEffectiveHours> GetItemTasksEffectiveHoursByProject(string projectUId);

        ICollection<TodayMemberPlannedHours> GetTodayMemberPlannedHoursByUIds(string[] membersUIds);

        Task GetTask(string taskUId);

        Task GetTaskByNumber(int taskNumber);

        TaskDetail GetTaskDetail(string taskUId);

        void SaveTask(Task task);

        decimal GetTotalEffectiveHoursByRole(string roleUId);

        decimal GetTotalEffectiveHoursByProject(string projectUId);

        decimal GetTotalBugHoursByProject(string projectUId);

        decimal GetTotalEffectiveHoursByItem(string backlogitemUId);

        bool DoesBacklogItemHasTasks(string backlogItemUId);

        bool DoesMemberHasAnyTaskAtProject(string projectUId, string memberUId);

        void UpdateTaskArtifactCount(string taskUId, int count);

        PlannedHour[] GetTotalEffectiveHoursByItem(string backlogitemUId, ICollection<Role> roles);

        IDictionary<string, decimal> GetTotalEffectiveHoursByProjectAndMember(string projectUId, string memberUId);

        void SaveTaskTag(TaskTag tag);
        void DeleteTaskTag(string tagUId);
        ICollection<TaskTag> GetTaskTags(string projectUId, string filter = "ALL");

    }
}
