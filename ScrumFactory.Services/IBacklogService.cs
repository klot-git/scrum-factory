using System;

namespace ScrumFactory.Services
{
    using System.ServiceModel;
    using System.Collections.Generic;

    public enum BacklogFiltersMode {
        PENDING,
        PLANNING,
        SELECTED,        
        ALL,     
        PRODUCT_BACKLOG,
        DAILY_MEETING
    }


    [ServiceContract()]
    public interface IBacklogService
    {

        ICollection<BacklogItem> GetCurrentBacklog(string projectUId, short filterMode, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime());

        [OperationContract()]
        ICollection<BacklogItem> GetBacklog(string projectUId, string planning = "current", short filterMode = 3, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime());     

        [OperationContract()]
        ICollection<ScrumFactory.BacklogItem> GetAllUnfinishedBacklogItems(bool onlyMine, string projectFilter);

        [OperationContract()]
        BacklogItem GetBacklogItem(string backlogItemUId);

        ICollection<BacklogItem> GetItemsBySize(string sizeUId, int depth);

        [OperationContract()]
        int AddBacklogItem(BacklogItem item);

        [OperationContract()]
        void UpdateBacklogItem(string backlogItemUId, BacklogItem item);

        [OperationContract()]
        void UpdateBacklogItemIgnoringHours(string backlogItemUId, BacklogItem item);

        [OperationContract()]
        void ChangeBacklogItemGroup(string backlogItemUId, string groupUId);

        [OperationContract()]
        void ChangeBacklogItemStatus(string backlogItemUId, short status);

        [OperationContract()]
        void ChangeBacklogItemIssueType(string backlogItemUId, short issueType);

        void UpdateItemStatusToWorking(Task task);
       
        [OperationContract()]
        BacklogItem[] MoveItem(string backlogItemUId, string targetBacklogItemUId);

        [OperationContract()]
        BacklogItem[] ChangeItemSprint(string backlogItemUId, int sprintNumber, bool lowPriority);

        [OperationContract()]
        void DeleteBacklogItem(string backlogItemUId);

        [OperationContract()]
        ICollection<BurndownLeftHoursByDay> GetBurndownHoursByDay(string projectUId, string planning);      

        [OperationContract()]
        ICollection<ItemSize> GetItemSizes();

        [OperationContract()]
        void AddItemSize(ItemSize size);

        [OperationContract()]
        void UpdateItemSize(string itemSizeUId, ItemSize size);

        [OperationContract()]
        void UpdateItemSizeOccurrenceContraint(string itemSizeUId, string constraint);

        [OperationContract()]
        void DeleteItemSize(string ItemSizeUId);

        [OperationContract()]
        void EqualizeSprints(string projectUId);

        [OperationContract()]
        ICollection<BacklogItemGroup> GetBacklogItemGroups(string projectUId);

        [OperationContract()]
        void UpdateBacklogItemGroup(string projectUId, BacklogItemGroup group);

        [OperationContract()]
        bool DeleteBacklogItemGroup(string groupUId);

        [OperationContract]
        Dictionary<string, decimal?> GetVelocityBySize(string sizeUId);

        [OperationContract]
        decimal GetProjectVelocityIndicator(string projectUId);

        [OperationContract]
        decimal GetVelocityIndicator();

        [OperationContract]
        void ShiftItemsAfter(string backlogitemUId, string planItemName, string deliveryItemName);

        [OperationContract]
        ICollection<BacklogItem> GetItemsTotalEffectiveHours(string projectUId);

    }
}
