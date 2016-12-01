namespace ScrumFactory.Data
{
    using System.Collections.Generic;

    public interface IBacklogRepository
    {
        ICollection<BacklogItem> GetCurrentBacklog(string projectId, System.DateTime fromDate = new System.DateTime(), System.DateTime untilDate = new System.DateTime());

        ICollection<BacklogItem> GetBacklog(string projectId, int? planningNumber = null, System.DateTime fromDate = new System.DateTime(), System.DateTime untilDate = new System.DateTime());

        ICollection<ScrumFactory.BacklogItem> GetBacklogOnlyItems(string projectUId);

        ICollection<ScrumFactory.BacklogItem> GetAllUnfinishedBacklogItems(string[] projectUIds);

        ScrumFactory.BacklogItem GetBacklogItem(string backlogItemId);

        ICollection<ItemSize> GetItemSizes();

        ItemSize GetItemSize(string itemSizeUId);

        ItemSize GetItemSizeByConstraint(short constraint);

        void SaveItemSize(ItemSize size);

        void DeleteItemSize(string itemSizeUId);

        void UpdateItemSizeOccurrenceContraint(string itemSizeUId, ItemOccurrenceContraints constraint);

        void SaveBacklogItem(BacklogItem item);

        void SaveBacklogItemIgnoreHours(BacklogItem item);

        void DeleteBacklogItem(string backlogItemId);

        bool IsBacklogItemFirstPlan(Project project, string backlogItemUId);

        bool IsItemSizeAlreadyPlanned(string itemSizeUId);

        ICollection<BacklogItemGroup> GetBacklogItemGroups(string projectUId);

        void UpdateBacklogItemGroup(BacklogItemGroup group);

        BacklogItemGroup GetBacklogItemGroup(string groupUId);

        bool DeleteBacklogItemGroup(string groupUId);

        ICollection<BacklogItem> GetDoneItemsBySize(string sizeUId, int lasMonths);

        decimal GetTotalPointsDone(string projectUId);

        ICollection<BacklogItem> GetDoneItemsByDepth(int depth);

        void UpdateBacklogItemArtifactCount(string backlogItemUId, int count);

    }
}
