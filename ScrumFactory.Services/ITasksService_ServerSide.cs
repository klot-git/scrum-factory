using System.ServiceModel;
using System.Collections.Generic;

namespace ScrumFactory.Services {

    public interface ITasksService_ServerSide : ITasksService {

        decimal GetReworkIndicator_skipAuth(string projectUId);
        decimal GetProjectTotalEffectiveHours_skipAuth(string projectUId);

        ICollection<Task> GetUsersTasks(string[] memberUIds, bool onlyOpen, bool includeUnassigned);

        void UpdateTaskArtifactCount(string taskUId, int count);

        PlannedHour[] GetTotalEffectiveHoursByItem(string backlogitemUId, ICollection<Role> roles);

        ICollection<TodayMemberPlannedHours> GetTodayMemberPlannedHours(string[] membersUIds);

        MemberPerformance GetMemberPerformance(string memberUId);

    }
}
