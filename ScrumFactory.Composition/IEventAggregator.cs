namespace ScrumFactory.Composition
{
    using System;

    public enum ScrumFactoryEvent : int {
        NotAuthorizedServerExceptionThrowed,
        ViewProjectDetails,                       
        BacklogItemsChanged,        
        BacklogItemDeleted,
        BacklogItemSelectedChanged,
        BacklogItemGroupsLoaded,
        BacklogReplannedByTask,
        BurndownShouldRefresh,
        ProjectStatusChanged,
        ProjectDetailsChanged,
        ProjectRolesChanged,
        ProjectRoleChanged,        
        ProjectMembershipRemoved,
        ProjectMembersChanged,        
        RoleHourCostsChanged,        
        SprintsDateChanged,  
        SprintAdded,
        SprintsShifted,      
        ProjectCreated,
        ActiveSizesChanged,  
        ShowFullScreen,
        ShowLogin,
        ShowProfile,
        ShowProjectTab,
        ShowProjectTeam,        
        ShowItemDetail,
        ShowReport,        
        ShowFinishTaskDialog,
        ShowTasksForItem,
        ShowTaskDetail,
        ShowJoinDialog,
        ShowOwnersList,
        ShowCodeRepositoryLog,        
        ShowUserTasksSelector,        
        ShowMemberDetail,
        ShellStarted,        
        Signing,
        SignedMemberChanged,        
        TaskTrackChanged,
        TasksLoaded,
        TaskAdded,
        TaskChanged,
        TaskAssigneeChanged,
        TaskSelectedChanged,
        TaskReplanItemChanged,
        ServerArgOnInit,
        ProjectArgOnInit,
        TaskArgOnInit,
        ApplicationWhentBackground,
        ApplicationWhentForeground,
        RecentProjectChanged,
        ConfigChanged,
        ProjectAdjustPointsChanged,
        ShowCodeRepository,
        ShowDocRepository,
        CreateNewProject
    }

    public interface IEventAggregator
    {

        void UnSubscribeAll(object obj);
        void Subscribe(string eventName, Action action);
        void Subscribe(string eventName, Action action, int priority);
        void Subscribe<TData>(string eventName, Action<TData> action, int priority);
        void Subscribe<TData>(string eventName, Action<TData> action);
        void Publish<TData>(string eventName, TData data);
        void Publish(string eventName);

        
        void Subscribe(ScrumFactoryEvent eventName, Action action);
        void Subscribe(ScrumFactoryEvent eventName, Action action, int priority);
        void Subscribe<TData>(ScrumFactoryEvent e, Action<TData> action, int priority);
        void Subscribe<TData>(ScrumFactoryEvent eventName, Action<TData> action);
        void Publish<TData>(ScrumFactoryEvent eventName, TData data);
        void Publish(ScrumFactoryEvent eventName);

        
    }

   


}
