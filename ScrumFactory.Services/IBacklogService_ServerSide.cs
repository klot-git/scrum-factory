using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services {

    public interface IBacklogService_ServerSide : IBacklogService {

        decimal GetProjectVelocityIndicator_skipAuth(string projectUId);

        void AddPlannedHoursToItem(string backlogItemUId, string roleUId, decimal hoursToAdd);

        ICollection<BacklogItem> AddSprintDefaultItems(Project project, int sprintNumber, string planItemName, string planGroupName, string deliveryItemName, string deliveryGroupName);

        void UpdateBacklogItemArtifactCount(string backlogItemUId, int count);
    }
}
