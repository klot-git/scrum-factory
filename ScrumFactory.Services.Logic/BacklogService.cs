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
    [Export(typeof(IBacklogService))]
    [Export(typeof(IBacklogService_ServerSide))]
    public partial class BacklogService : IBacklogService, IBacklogService_ServerSide {

        [Import]
        private Data.IBacklogRepository backlogRepository { get; set; }
        
        [Import]
        private IProjectsService projectsService { get; set; }

        [Import]
        private IProposalsService proposalsService { get; set; }


        [Import]
        private ITasksService_ServerSide tasksService { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        [Import]
        private IArtifactsService artifactService { get; set; }

        [Import]
        private IMailerService mailer { get; set; }

        private int VelocityCalcDepth {
            get {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings["VelocityCalcDepth"]);
            }
        }


        [WebGet(UriTemplate = "UnfinishedBacklogItems/?onlyMine={onlyMine}&projectFilter={projectFilter}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<ScrumFactory.BacklogItem> GetAllUnfinishedBacklogItems(bool onlyMine, string projectFilter) {
            authorizationService.VerifyRequestAuthorizationToken();

            string memberUId = null;
            if (onlyMine)
                memberUId = authorizationService.SignedMemberProfile.MemberUId;
            var myProjects = projectsService.GetProjects(null, null, projectFilter, memberUId);

            var myProjectUids = myProjects.Select(p => p.ProjectUId).ToArray();

            return backlogRepository.GetAllUnfinishedBacklogItems(myProjectUids);
        }

        [WebGet(UriTemplate = "MyUnfinishedBacklogItemsCount/?projectFilter={projectFilter}", ResponseFormat = WebMessageFormat.Json)]
        public int GetAllUnfinishedBacklogItemsCountForUser(string projectFilter)
        {
            var items = GetAllUnfinishedBacklogItems(true, projectFilter);
            if (items == null)
                return 0;
            return items.Count;            
        }

        public ICollection<BacklogItem> GetCurrentBacklog(string projectUId, short filterMode, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime()) {
            
            return GetBacklog(projectUId, "current", filterMode, fromDate, untilDate);
        }
   
        [WebGet(UriTemplate = "Backlogs/{projectUId}/?planning={planning}&filterMode={filterMode}&fromDate={fromDate}&untilDate={untilDate}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<BacklogItem> GetBacklog(string projectUId, string planning = "current", short filterMode = 3, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime()) {


            System.Diagnostics.Debug.WriteLine("***< get backlog");

            authorizationService.VerifyRequestAuthorizationToken();

            // gets the project
            Project project = projectsService.GetProject(projectUId);

            // if filtermode is SELECTED and there is no current sprint, returns nothing
            if (filterMode == (short)BacklogFiltersMode.SELECTED && project.CurrentSprint == null)
                return new List<BacklogItem>();

            // get items
            ICollection<BacklogItem> items = new List<BacklogItem>();
            if (String.IsNullOrEmpty(planning))
                items = backlogRepository.GetBacklog(projectUId, null, fromDate, untilDate);
            else if (planning.ToLower() == "current")
                items = backlogRepository.GetCurrentBacklog(projectUId, fromDate, untilDate);            
            else
                items = backlogRepository.GetBacklog(projectUId, Int32.Parse(planning), fromDate, untilDate);
            
            
            // if only pending items matther
            if (filterMode == (short)BacklogFiltersMode.PENDING)
                items = items.Where(i => i.Status != (short)BacklogItemStatus.ITEM_DONE && i.Status != (short)BacklogItemStatus.ITEM_CANCELED).ToList();

            // if only current sprint matther
            if (filterMode == (short)BacklogFiltersMode.SELECTED)
                items = items.Where(i => 
                    i.SprintNumber == project.CurrentSprint.SprintNumber
                    || (i.SprintNumber < project.CurrentSprint.SprintNumber && i.Status != (short)BacklogItemStatus.ITEM_DONE && i.Status != (short)BacklogItemStatus.ITEM_CANCELED)
                    ).ToList();

            // if only planned items matther
            if (filterMode == (short)BacklogFiltersMode.PLANNING)
                items = items.Where(i => 
                    i.SprintNumber >= project.CurrentValidSprint.SprintNumber
                    || (i.Status != (short)BacklogItemStatus.ITEM_DONE && i.Status != (short)BacklogItemStatus.ITEM_CANCELED)).ToList();

            // if only planned items matther
            if (filterMode == (short)BacklogFiltersMode.DAILY_MEETING) {
                items = items.Where(i => i.SprintNumber == project.CurrentValidSprint.SprintNumber && i.Status!=(short)BacklogItemStatus.ITEM_CANCELED).ToList();
                items = IncludeBacklogItemsThatHasTasks(project.ProjectUId, items);
            }

            // if only itens not planned matthers
            if (filterMode == (short)BacklogFiltersMode.PRODUCT_BACKLOG) {
                items = items.Where(i => i.SprintNumber == null).ToList();                
            }
            
            return items;
        }

        private ICollection<BacklogItem> IncludeBacklogItemsThatHasTasks(string projectUId, ICollection<BacklogItem> items) {

            ICollection<Task> dailyTasks = tasksService.GetProjectTasks(projectUId, DateTime.MinValue, DateTime.MinValue, true, null);
            List<Task> outsideTasks = dailyTasks.Where(t => !items.Any(i => i.BacklogItemUId==t.BacklogItemUId)).ToList();
            foreach (Task task in outsideTasks) {
                BacklogItem item = GetBacklogItem(task.BacklogItemUId);
                if(!items.Any(i => i.BacklogItemUId==item.BacklogItemUId))
                    items.Add(item);
            }
            return items;
            
         
        }
                

      

        [WebGet(UriTemplate = "BacklogItems/{backlogItemUId}", ResponseFormat = WebMessageFormat.Json)]
        public BacklogItem GetBacklogItem(string backlogItemUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return this.backlogRepository.GetBacklogItem(backlogItemUId);
        }


        public ICollection<BacklogItem> GetItemsBySize(string sizeUId, int lastProjects) {
            authorizationService.VerifyRequestAuthorizationToken();
            return backlogRepository.GetDoneItemsBySize(sizeUId, lastProjects);
        }

        private void VerifyPermissionForCreateEditItem(BacklogItem item, Project project = null) {

            PermissionSets[] permissions = new PermissionSets[] { PermissionSets.SCRUM_MASTER, PermissionSets.PRODUCT_OWNER };

            if (project == null)
                project = projectsService.GetProject(item.ProjectUId);

            if (project.IsTicketProject) 
                permissions = new PermissionSets[] { PermissionSets.SCRUM_MASTER, PermissionSets.PRODUCT_OWNER, PermissionSets.TEAM };
                

            authorizationService.VerifyPermissionAtProject(item.ProjectUId, permissions);

            if (!project.IsTicketProject && !authorizationService.IsProjectScrumMaster(item.ProjectUId) && item.SprintNumber != null)
                throw new WebFaultException<String>("Product owners can not plan items", System.Net.HttpStatusCode.BadRequest);
        }

        private bool ShouldCreateTicketFolders {
            get {
                return Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["ShouldCreateTicketFolders"]);
            }
        }

        private string TicketEmailSubject {
            get {
                var str = System.Configuration.ConfigurationManager.AppSettings["TicketEmailSubject"];
                if (String.IsNullOrEmpty(str))
                {
                    return "Incident #{0}";
                }
                return str;
            }
        }

        [WebInvoke(Method = "POST", UriTemplate = "BacklogItems", RequestFormat = WebMessageFormat.Json)]
        public int AddBacklogItem(BacklogItem item) {

            Project project = projectsService.GetProject(item.ProjectUId);

            VerifyPermissionForCreateEditItem(item, project);

            item.CreatedBy = authorizationService.SignedMemberProfile.MemberUId;
            
            backlogRepository.SaveBacklogItem(item);

            if (project.ProjectType == (short)ProjectTypes.TICKET_PROJECT)
            {                
                try
                {
                    SendTicketEmail(item, project);
                }
                catch (Exception ex)
                {
                    ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
                }
            }

            return item.BacklogItemNumber;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}", RequestFormat = WebMessageFormat.Json)]
        public void UpdateBacklogItem(string backlogItemUId, BacklogItem item) {

            Project project = projectsService.GetProject(item.ProjectUId);

            VerifyPermissionForCreateEditItem(item, project);
            
            UpdateBacklogItem(backlogItemUId, item, project);
        }

        public void UpdateBacklogItem(string backlogItemUId, BacklogItem item, Project project) {

            // updates the planning number            
            foreach (PlannedHour h in item.PlannedHours)
            {
                if (h.PlanningNumber < project.CurrentPlanningNumber)
                {
                    h.PlanningNumber = project.CurrentPlanningNumber;
                }
            }

            backlogRepository.SaveBacklogItem(item);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}/GroupUId", RequestFormat = WebMessageFormat.Json)]
        public void ChangeBacklogItemGroup(string backlogItemUId, string groupUId) {
            BacklogItem item = GetBacklogItem(backlogItemUId);

            Project project = projectsService.GetProject(item.ProjectUId);
            VerifyPermissionForCreateEditItem(item, project);

            item.GroupUId = groupUId;
            backlogRepository.SaveBacklogItemIgnoreHours(item);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}/Status", RequestFormat = WebMessageFormat.Json)]
        public void ChangeBacklogItemStatus(string backlogItemUId, short status) {
            BacklogItem item = GetBacklogItem(backlogItemUId);

            Project project = projectsService.GetProject(item.ProjectUId);
            VerifyPermissionForCreateEditItem(item, project);

            item.Status = status;
            backlogRepository.SaveBacklogItemIgnoreHours(item);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}/IssueType", RequestFormat = WebMessageFormat.Json)]
        public void ChangeBacklogItemIssueType(string backlogItemUId, short issueType) {
            BacklogItem item = GetBacklogItem(backlogItemUId);

            Project project = projectsService.GetProject(item.ProjectUId);
            VerifyPermissionForCreateEditItem(item, project);

            item.IssueType = issueType;
            backlogRepository.SaveBacklogItemIgnoreHours(item);
        }

        public void UpdateItemStatusToWorking(Task task) {
            BacklogItem item = GetBacklogItem(task.BacklogItemUId);
            if (item.Status != (short)BacklogItemStatus.ITEM_REQUIRED || task.EffectiveHours == 0)
                return;
            item.Status = (short)BacklogItemStatus.ITEM_WORKING;
            backlogRepository.SaveBacklogItemIgnoreHours(item);            
        }

        public void UpdateBacklogItemArtifactCount(string backlogItemUId, int count) {
            backlogRepository.UpdateBacklogItemArtifactCount(backlogItemUId, count);
        }


        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}/?IgnoreHours=true", RequestFormat = WebMessageFormat.Json)]
        public void UpdateBacklogItemIgnoringHours(string backlogItemUId, BacklogItem item) {
            VerifyPermissionForCreateEditItem(item);
            backlogRepository.SaveBacklogItemIgnoreHours(item);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "BacklogItems/{backlogItemUId}", ResponseFormat = WebMessageFormat.Json)]
        public void DeleteBacklogItem(string backlogItemUId) {

            System.Diagnostics.Debug.WriteLine("***< try to delete item");

            BacklogItem item = GetBacklogItem(backlogItemUId);

            if(item==null)
                throw new WebFaultException<String>("Item not found", System.Net.HttpStatusCode.NotFound);
            
            authorizationService.VerifyPermissionAtProject(item.ProjectUId, PermissionSets.SCRUM_MASTER);

            Project project = projectsService.GetProject(item.ProjectUId);

            if (!backlogRepository.IsBacklogItemFirstPlan(project, backlogItemUId))
                throw new WebFaultException<String>("BRE_CAN_DELETE_ALREADY_PLANNED_ITEM", System.Net.HttpStatusCode.BadRequest);

            if(tasksService.DoesBacklogItemHasTasks(backlogItemUId))
                throw new WebFaultException<String>("BRE_CAN_DELETE_ITEM_WITH_TASKS", System.Net.HttpStatusCode.BadRequest);

            if (proposalsService.IsItemAtAnyProposal(backlogItemUId))
                throw new WebFaultException<String>("BRE_CAN_DELETE_ITEM_AT_PROPOSAL", System.Net.HttpStatusCode.BadRequest);


            backlogRepository.DeleteBacklogItem(backlogItemUId);
            System.Diagnostics.Debug.WriteLine("***< item deleted");

        }

        [WebInvoke(Method = "DELETE", UriTemplate = "Groups/{groupUId}", ResponseFormat = WebMessageFormat.Json)]        
        public bool DeleteBacklogItemGroup(string groupUId) {

            var group = backlogRepository.GetBacklogItemGroup(groupUId);
            if (group == null)
                return true;

            authorizationService.VerifyPermissionAtProject(group.ProjectUId, PermissionSets.SCRUM_MASTER);
            
            var removed = backlogRepository.DeleteBacklogItemGroup(groupUId);
            return removed;
        }

        [WebInvoke(Method = "POST", UriTemplate = "BacklogItems/{backlogItemUId}/SprintNumber?lowPriority={lowPriority}", RequestFormat = WebMessageFormat.Json)]
        public BacklogItem[] ChangeItemSprint(string backlogItemUId, int sprintNumber, bool lowPriority) {

            // get item
            BacklogItem item = GetBacklogItem(backlogItemUId);

            // verify permission
            authorizationService.VerifyPermissionAtProject(item.ProjectUId, PermissionSets.SCRUM_MASTER);

            ICollection<BacklogItem> backlog = GetCurrentBacklog(item.ProjectUId, (short)BacklogFiltersMode.ALL);

            Project project = projectsService.GetProject(item.ProjectUId);

            return ChangeItemSprint(item, backlog, project, sprintNumber, lowPriority);
        }

        private BacklogItem[] ChangeItemSprint(BacklogItem item, ICollection<BacklogItem> backlog, Project project, int sprintNumber, bool lowPriority) {

            // if is moving back to the product backlog
            if (sprintNumber < 0) {
                item.SprintNumber = null;
                UpdateBacklogItem(item.BacklogItemUId, item, project);
                return new BacklogItem[] { item };
            }

            // if moving to a previous sprint or is low priority
            // insert it at the end of the sprint
            if (lowPriority || item.SprintNumber > sprintNumber) {
                BacklogItem targetItem = GetLastSprintDevelopmentItem(backlog, sprintNumber);
                int priority = 1;
                if (targetItem != null)
                    priority = targetItem.BusinessPriority + 1;
                item.SprintNumber = sprintNumber;
                item.BusinessPriority = priority;
                UpdateBacklogItem(item.BacklogItemUId, item, project);
                return new BacklogItem[] { item };
            }

            // if moving to a sprint ahead, insert it at the begining
            if (item.SprintNumber == null || item.SprintNumber < sprintNumber) {
                BacklogItem targetItem = GetFirstSprintDevelopmentItem(backlog, sprintNumber);
                // if the sprint has no items
                if (targetItem == null) {
                    item.SprintNumber = sprintNumber;
                    item.BusinessPriority = 1;
                    UpdateBacklogItem(item.BacklogItemUId, item, project);
                    return new BacklogItem[] { item };
                }
                List<BacklogItem> sprintItems = backlog.Where(i => i.SprintNumber == sprintNumber).ToList();
                return MoveItem(item, targetItem, sprintItems, project);
                
            }

            return null;
        }

        [WebInvoke(Method = "POST", UriTemplate = "BacklogItems/{backlogItemUId}/BusinessPriority/_moveTo", RequestFormat = WebMessageFormat.Json)]
        public BacklogItem[] MoveItem(string backlogItemUId, string targetBacklogItemUId) {

            // gets the itens
            BacklogItem item = GetBacklogItem(backlogItemUId);
            BacklogItem targetItem = GetBacklogItem(targetBacklogItemUId);

            // move them
            return MoveItem(item, targetItem);

        }

        private int CalcsNewPriority(List<BacklogItem> items, int targetIdx) {

            if(items[targetIdx].OccurrenceConstraint == 1)
                return items[targetIdx].BusinessPriority;
                
            while (targetIdx>=0 && items[targetIdx].OccurrenceConstraint == 2) {
                targetIdx--;
            }
            int priority = 10;
            if(targetIdx>=0)
                priority = items[targetIdx].BusinessPriority + 10;
            return priority;
        }

        /// <summary>
        /// Moves the item to the target item position - 1
        /// </summary>
        /// <param name="item"></param>
        /// <param name="targetItem"></param>
        /// <returns></returns>
        private BacklogItem[] MoveItem(BacklogItem item, BacklogItem targetItem) {

            // verify permission
            authorizationService.VerifyPermissionAtProject(targetItem.ProjectUId, PermissionSets.SCRUM_MASTER);

            if (targetItem.OccurrenceConstraint == 0) // can not move to before a plan item
                return null;

            // get the sprint itens
            List<BacklogItem> sprintItems = GetCurrentBacklog(targetItem.ProjectUId, (short)BacklogFiltersMode.ALL)
                .Where(b => b.SprintNumber == targetItem.SprintNumber)
                .OrderBy(b => b.OccurrenceConstraint).ThenBy(b => b.BusinessPriority).ToList();

            Project project = projectsService.GetProject(item.ProjectUId);

            return MoveItem(item, targetItem, sprintItems, project);
        }

        private BacklogItem[] MoveItem(BacklogItem item, BacklogItem targetItem, List<BacklogItem> sprintItems, Project project) {

            int itemIndex = sprintItems.IndexOf(sprintItems.Where(b => b.BacklogItemUId == item.BacklogItemUId).SingleOrDefault());
            int targetIndex = sprintItems.IndexOf(sprintItems.Where(b => b.BacklogItemUId == targetItem.BacklogItemUId).SingleOrDefault());

            if (itemIndex == targetIndex)
                return null;

            // The item sprint may change, so link it to its project before do that
            // -> item.SprintNumber requires a project
            item.Project = project;

            // keep tracking of the changing items
            List<BacklogItem> changedItems = new List<BacklogItem>();
            changedItems.Add(item);

            // item was at other sprint
            if (itemIndex == -1) {

                // update item priority and resets the delivery date
                item.BusinessPriority = CalcsNewPriority(sprintItems, targetIndex);
                item.SprintNumber = targetItem.SprintNumber;
                item.DeliveryDate = null;
                UpdateBacklogItem(item.BacklogItemUId, item, project);


                for (int i = targetIndex; i < sprintItems.Count - 1; i++) {
                    if (sprintItems[i].OccurrenceConstraint == 1) {
                        sprintItems[i].BusinessPriority = sprintItems[i].BusinessPriority + 10;
                        UpdateBacklogItem(sprintItems[i].BacklogItemUId, sprintItems[i], project);
                        changedItems.Add(sprintItems[i]);
                    }
                }

                //BacklogItem last = sprintItems.Last();
                //last.BusinessPriority = last.BusinessPriority + 1;
                //UpdateBacklogItem(last.BacklogItemUId, last, project);
                //changedItems.Add(last);
            }

            // item at the same sprint and moving an item up
            else if (itemIndex > targetIndex) {

                // update item priority
                item.BusinessPriority = targetItem.BusinessPriority;
                item.SprintNumber = targetItem.SprintNumber;
                UpdateBacklogItem(item.BacklogItemUId, item, project);


                for (int i = targetIndex; i < itemIndex; i++) {                   
                        sprintItems[i].BusinessPriority = sprintItems[i + 1].BusinessPriority;   
                        UpdateBacklogItem(sprintItems[i].BacklogItemUId, sprintItems[i], project);
                        changedItems.Add(sprintItems[i]);
                   
                }
            }

            // item at the same sprint and moving an item down
            else if (itemIndex < targetIndex) {

                int realTargetIndex = targetIndex - 1;
                int itemPriority = sprintItems[realTargetIndex].BusinessPriority;

                for (int i = realTargetIndex; i >= itemIndex + 1; i--) {
                    sprintItems[i].BusinessPriority = sprintItems[i - 1].BusinessPriority;
                    UpdateBacklogItem(sprintItems[i].BacklogItemUId, sprintItems[i], project);
                    changedItems.Add(sprintItems[i]);
                }

                // update item priority
                item.BusinessPriority = itemPriority;
                item.SprintNumber = targetItem.SprintNumber;
                UpdateBacklogItem(item.BacklogItemUId, item, project);


            }

            return changedItems.ToArray();
        }

        private BacklogItem GetFirstSprintDevelopmentItem(ICollection<BacklogItem> backlog, int sprintNumber) {            
            return backlog
                .Where(b => b.SprintNumber == sprintNumber && b.OccurrenceConstraint == (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                .OrderBy(b => b.BusinessPriority).FirstOrDefault();
        }

        private BacklogItem GetLastSprintDevelopmentItem(ICollection<BacklogItem> backlog, int sprintNumber) {            
            return backlog
                .Where(b => b.SprintNumber == sprintNumber && b.OccurrenceConstraint == (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                .OrderBy(b => b.BusinessPriority).LastOrDefault();
        }
     

        [WebGet(UriTemplate = "ItemSizes/", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<ItemSize> GetItemSizes() {
            authorizationService.VerifyRequestAuthorizationToken();
            return backlogRepository.GetItemSizes();
        }

        [WebInvoke(Method = "PUT", UriTemplate = "ItemSizes/{itemSizeUId}", RequestFormat = WebMessageFormat.Json)]
        public void UpdateItemSize(string itemSizeUId, ItemSize size) {
            authorizationService.VerifyFactoryOwner();
            backlogRepository.SaveItemSize(size);
        }

        [WebInvoke(Method = "POST", UriTemplate = "ItemSizes", RequestFormat = WebMessageFormat.Json)]
        public void AddItemSize(ItemSize size) {
            authorizationService.VerifyFactoryOwner();
            backlogRepository.SaveItemSize(size);
        }

        [WebInvoke(Method = "DELETE", UriTemplate = "ItemSizes/{itemSizeUId}", ResponseFormat = WebMessageFormat.Json)]
        public void DeleteItemSize(string itemSizeUId) {
            authorizationService.VerifyFactoryOwner();
            ItemSize size = backlogRepository.GetItemSize(itemSizeUId);

            if(size.IsDeliveryItem || size.IsPlanningItem)
                throw new WebFaultException<String>("BRE_PLAN_ITEM_SIZE_CAN_NOT_BE_DELETED", System.Net.HttpStatusCode.BadRequest);

            if (backlogRepository.IsItemSizeAlreadyPlanned(itemSizeUId))
                throw new WebFaultException<String>("BRE_ITEM_SIZE_ALREADY_USED", System.Net.HttpStatusCode.BadRequest);

            backlogRepository.DeleteItemSize(itemSizeUId);
        }   

        [WebInvoke(Method = "PUT", UriTemplate = "ItemSizes/{itemSizeUId}/OccurrenceContraint/{constraint}", RequestFormat = WebMessageFormat.Json)] 
        public void UpdateItemSizeOccurrenceContraint(string itemSizeUId, string constraint) {
            authorizationService.VerifyFactoryOwner();
            ItemOccurrenceContraints c = (ItemOccurrenceContraints) Enum.Parse(typeof(ItemOccurrenceContraints), constraint);
            backlogRepository.UpdateItemSizeOccurrenceContraint(itemSizeUId, c);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "EqualizedBacklogs/", ResponseFormat = WebMessageFormat.Json)]
        public void EqualizeSprints(string projectUId) {

            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);

            // gets the project
            Project project = projectsService.GetProject(projectUId);

            // gets the backlog
            ICollection<BacklogItem> items = GetBacklog(projectUId);
            if (items == null)
                return;

            // itens do be equalized
            ICollection<BacklogItem> toDoItems = items.Where(b => b.Status == (short)BacklogItemStatus.ITEM_REQUIRED || b.Status == (short)BacklogItemStatus.ITEM_WORKING).ToList();
            toDoItems = toDoItems.OrderBy(b => b.SprintNumber).ThenBy(b => b.BusinessPriority).ToList();

            // calcs the amount of hours per sprint
            decimal toDoTotalHours = toDoItems.Sum(b => b.CurrentTotalHours);
            int remainingSprints = project.Sprints.Count - project.CurrentValidSprint.SprintNumber + 1;
            decimal sprintLimit = toDoTotalHours / remainingSprints;

            // equalize items
            int sprintNumber = project.CurrentValidSprint.SprintNumber;
            decimal hours = toDoItems.Where(b => b.SprintNumber == sprintNumber && b.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC).Sum(b => b.CurrentTotalHours);
            foreach (BacklogItem item in toDoItems.Where(b => b.OccurrenceConstraint == (int)ItemOccurrenceContraints.DEVELOPMENT_OCC)) {

                // decide when ever the item should be at this or at the next sprint
                decimal hoursWithThisItem = hours + item.CurrentTotalHours;
                decimal devInThisSprint = Math.Abs(hoursWithThisItem - sprintLimit);
                decimal devInNextSprint = Math.Abs(hours - sprintLimit);

                if (devInThisSprint > devInNextSprint) {
                    if (sprintNumber < project.Sprints.Count) {
                        sprintNumber++;
                        hours = toDoItems.Where(b => b.SprintNumber == sprintNumber && b.OccurrenceConstraint != (int)ItemOccurrenceContraints.DEVELOPMENT_OCC).Sum(b => b.CurrentTotalHours);
                    }
                }

                item.SprintNumber = sprintNumber;
                hours = hours + item.CurrentTotalHours;

            }

            

            // now save everything
            using (TransactionScope scope = new TransactionScope()) {
                foreach (BacklogItem item in toDoItems)
                    UpdateBacklogItem(item.BacklogItemUId, item, project);
                
                scope.Complete();
            }
                

        }
        
        [WebGet(UriTemplate = "Groups/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<BacklogItemGroup> GetBacklogItemGroups(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return backlogRepository.GetBacklogItemGroups(projectUId);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "Groups/{projectUId}", RequestFormat = WebMessageFormat.Json)]
        public void UpdateBacklogItemGroup(string projectUId, BacklogItemGroup group) {
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            backlogRepository.UpdateBacklogItemGroup(group);
        }


        [WebGet(UriTemplate = "ItemVelocity/{sizeUId}", ResponseFormat = WebMessageFormat.Json)]
        [AspNetCacheProfile("CacheForOneDay")]
        public Dictionary<string, decimal?> GetVelocityBySize(string sizeUId) {
            Dictionary<string, decimal?> effortByRole = new Dictionary<string, decimal?>();

            // first get all items of this size
            ICollection<BacklogItem> items = GetItemsBySize(sizeUId, VelocityCalcDepth);

            string projectUId = String.Empty;
            ICollection<Role> projectRoles = null;
            int? totalSize = 0;
            foreach (BacklogItem item in items.OrderBy(i => i.ProjectUId)) {

                // get the project roles
                if (projectUId != item.ProjectUId) {
                    projectUId = item.ProjectUId;
                    projectRoles = projectsService.GetProjectRoles(projectUId);
                }

                // get the item tasks
                ICollection<Task> tasks = tasksService.GetItemTasks(item.BacklogItemUId);

                // adds the item size
                totalSize = totalSize + item.Size;

                if (tasks.Count > 0) {
                                        
                    // adds the hours to the relative role
                    foreach (Role r in projectRoles) {
                        decimal hours = tasks.Where(t => t.RoleUId == r.RoleUId).Sum(t => t.EffectiveHours);
                        string roleShortName = r.RoleShortName.ToLower();
                        if (effortByRole.ContainsKey(roleShortName))
                            effortByRole[roleShortName] = effortByRole[roleShortName] + hours;
                        else
                            effortByRole[roleShortName] = hours;
                    }

                }
                
            }

            // finally divide the size for each role spent hours
            Dictionary<string, decimal?> velocityByRole = new Dictionary<string, decimal?>();
            foreach (string shortName in effortByRole.Keys) {
                if (effortByRole[shortName] == 0)
                    velocityByRole[shortName] = 0;
                else
                    velocityByRole[shortName] = totalSize / effortByRole[shortName];
            }

            return velocityByRole;
        }

    

        [WebGet(UriTemplate = "VelocityIndicator/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public decimal GetProjectVelocityIndicator(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return GetProjectVelocityIndicator_skipAuth(projectUId);
        }

        public decimal GetProjectVelocityIndicator_skipAuth(string projectUId) {

            decimal hours = tasksService.GetProjectTotalEffectiveHours_skipAuth(projectUId);
            if (hours == 0)
                return 0;

            decimal ptsDone = backlogRepository.GetTotalPointsDone(projectUId);
            return ptsDone / hours;
        }

        [WebGet(UriTemplate = "VelocityIndicator/", ResponseFormat = WebMessageFormat.Json)]
        [AspNetCacheProfile("CacheForOneDay")]
        public decimal GetVelocityIndicator() {
            authorizationService.VerifyRequestAuthorizationToken();
            int size = 0;
            decimal hours = 0;
            ICollection<BacklogItem> items = backlogRepository.GetDoneItemsByDepth(VelocityCalcDepth);
            foreach (BacklogItem i in items.Where(i => i.Size!=null && i.SizeFactor!=0)) {
                size = size + (int)i.Size;
                hours = hours + tasksService.GetTotalEffectiveHoursByItem(i.BacklogItemUId);
            }

            if (hours == 0)
                return 0;

            return size / hours;
        }

        [WebInvoke(Method = "PUT", UriTemplate = "BacklogItems/{backlogItemUId}/_shiftAfter/?planItemName={planItemName}&deliveryItemName={deliveryItemName}", RequestFormat = WebMessageFormat.Json)]
        public void ShiftItemsAfter(string backlogitemUId, string planItemName, string deliveryItemName) {

            // find my item
            BacklogItem myItem = GetBacklogItem(backlogitemUId);
            if (myItem == null)
                return;

            // verify permission set            
            authorizationService.VerifyPermissionAtProject(myItem.ProjectUId, PermissionSets.SCRUM_MASTER);

            // gets the project
            Project project = projectsService.GetProject(myItem.ProjectUId);
            Sprint lastSprint = project.LastSprint;
            if (lastSprint == null)
                return;

            // if there is no next sprint, creates a new one
            if (myItem.SprintNumber == lastSprint.SprintNumber) {                
                Sprint newSprint = new Sprint() {
                    SprintUId = Guid.NewGuid().ToString(),
                    SprintNumber = project.NextSprintNumber,
                    StartDate = lastSprint.EndDate.AddDays(1),
                    EndDate = lastSprint.EndDate.AddDays(16),
                    ProjectUId = project.ProjectUId
                };
                projectsService.AddSprint(project.ProjectUId, newSprint, true, planItemName, null, deliveryItemName, null);
            }

            // get all backlogitems
            ICollection<BacklogItem> backlog = GetCurrentBacklog(myItem.ProjectUId, (short)BacklogFiltersMode.ALL);                       

            // select only the items of my sprint and after me
            backlog = backlog.Where(i =>
                i.SprintNumber==myItem.SprintNumber &&
                i.BusinessPriority > myItem.BusinessPriority &&
                i.OccurrenceConstraint == (short)ItemOccurrenceContraints.DEVELOPMENT_OCC)
                .OrderBy(i => i.BusinessPriority).ToList();

            

            using (TransactionScope scope = new TransactionScope()) {

                // lets move each item to next sprint                
                foreach (BacklogItem item in backlog)
                    ChangeItemSprint(item, backlog, project, (int)item.SprintNumber + 1, true);

                scope.Complete();
            }
        }


        public void AddPlannedHoursToItem(string backlogItemUId, string roleUId, decimal hoursToAdd) {
            
            BacklogItem item = GetBacklogItem(backlogItemUId);
            if (item == null)
                throw new WebFaultException<string>("Backlog item not found to replan hours", System.Net.HttpStatusCode.NotFound);

            Project project = projectsService.GetProject(item.ProjectUId);

            // if is not a scrum master ignores it and return
            if (!project.HasPermission(authorizationService.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER))
                return;

            PlannedHour hours = item.CurrentPlannedHours.SingleOrDefault(h => h.RoleUId == roleUId);
            if (hours == null) {                
                hours = new PlannedHour() { RoleUId = roleUId, BacklogItemUId = backlogItemUId, Hours = 0, PlanningNumber = project.CurrentPlanningNumber };
                item.PlannedHours.Add(hours);
            }
            
            hours.Hours += hoursToAdd;
            if (hours.Hours < 0)
                hours.Hours = 0;

            backlogRepository.SaveBacklogItem(item);
        }

        public ICollection<BacklogItem> AddSprintDefaultItems(Project project, int sprintNumber, string planItemName, string planGroupName, string deliveryItemName, string deliveryGroupName) {

            var planItem = PrepareDefaultItem(project, planItemName, planGroupName, sprintNumber, DefaultItemGroups.PLAN_GROUP, ItemOccurrenceContraints.PLANNING_OCC);
            AddBacklogItem(planItem);

            var deliveryItem = PrepareDefaultItem(project, deliveryItemName, deliveryGroupName, sprintNumber, DefaultItemGroups.DELIVERY_GROUP, ItemOccurrenceContraints.DELIVERY_OCC);
            AddBacklogItem(deliveryItem);

            return new BacklogItem[2] { planItem, deliveryItem };
                        
        }

        private BacklogItem PrepareDefaultItem(Project project, string name, string groupName, int sprintNumber, DefaultItemGroups defaultGroup, ItemOccurrenceContraints constraint) {

            // create the new item
            BacklogItem item = new BacklogItem {
                BacklogItemUId = Guid.NewGuid().ToString(),
                ProjectUId = project.ProjectUId,
                Name = name,
                Description = null,
                Status = (short)BacklogItemStatus.ITEM_REQUIRED,
                BusinessPriority = 1,
                OccurrenceConstraint = (short)constraint,
                SizeFactor = 0,
                CreateDate = DateTime.Now
            };
            item.Project = project;
            item.SyncPlannedHoursAndRoles(sprintNumber);

            // assign group
            ICollection<BacklogItemGroup> groups = GetBacklogItemGroups(project.ProjectUId);
            var group = groups.FirstOrDefault(g => g.DefaultGroup == (short)defaultGroup);
            if (group == null)
                group = CreateDefaultGroup(project.ProjectUId, defaultGroup, groupName);
            item.Group = group;
            item.GroupUId = group.GroupUId;


            // assign size
            ItemSize size = backlogRepository.GetItemSizeByConstraint((short)constraint);
            if (size != null) {
                item.ItemSizeUId = size.ItemSizeUId;
                item.SizeFactor = 1;
                item.Size = size.Size * item.SizeFactor;

                // tries to set the ideal hour values
                foreach(var h in size.SizeIdealHours)
                {
                    var ph = item.PlannedHours.SingleOrDefault(p => p.Role.RoleShortName.ToLower() == h.RoleShortName.ToLower());
                    if (ph != null)
                        ph.Hours = h.Hours;
                }
            }

            return item;

        }

        private BacklogItemGroup CreateDefaultGroup(string projectUId, DefaultItemGroups defaultGroup, string name) {

            if (name == null)
                name = defaultGroup.ToString();

            BacklogItemGroup group =
                new BacklogItemGroup() {
                    GroupColor = "WhiteSmoke",
                    GroupName = name,
                    GroupUId = Guid.NewGuid().ToString(),
                    ProjectUId = projectUId,
                    DefaultGroup = (short)defaultGroup
                };
            if (defaultGroup == DefaultItemGroups.PLAN_GROUP)
                group.GroupColor = "Khaki";
            if (defaultGroup == DefaultItemGroups.DELIVERY_GROUP)
                group.GroupColor = "Crimson";

            return group;
        }

        [WebGet(UriTemplate = "BacklogsWithEffectiveHours/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<BacklogItem> GetItemsTotalEffectiveHours(string projectUId) {            
            ICollection<Role> roles = projectsService.GetProjectRoles(projectUId);
            ICollection<BacklogItem> items = backlogRepository.GetBacklogOnlyItems(projectUId);
            ICollection<BacklogItemGroup> groups = backlogRepository.GetBacklogItemGroups(projectUId);

            foreach (BacklogItem item in items) {
                PlannedHour[] hours = tasksService.GetTotalEffectiveHoursByItem(item.BacklogItemUId, roles);
                item.PlannedHours = new List<PlannedHour>(hours);
                item.Group = groups.SingleOrDefault(g => g.GroupUId == item.GroupUId);
            }
            return items;
            
        }

        private void SendTicketEmail(BacklogItem ticket, Project project) {

            try {
                // get members and attach to the project
                mailer.AttachProjectMembers(project);

                // create body from the template
                ReportHelper.Report reports = new ReportHelper.Report();
                ReportHelper.ReportConfig reportConfig = new ReportHelper.ReportConfig("EmailNotifications", "ticket_created", Helper.ReportTemplate.ServerUrl);
                reportConfig.ReportObjects.Add(project);
                reportConfig.ReportObjects.Add(ticket);
                
                string body = reports.CreateReportXAML(Helper.ReportTemplate.ServerUrl, reportConfig);

                // subject
                string subject = String.Format(TicketEmailSubject, project.ProjectNumber + "." + ticket.BacklogItemNumber);

                if(!String.IsNullOrEmpty(ticket.ExternalId))
                {
                    subject = String.Format(TicketEmailSubject, ticket.ExternalId);
                }

                // send it to all project members
                bool send = mailer.SendEmail(project, subject, body);
                if (!send)
                    ScrumFactory.Services.Logic.Helper.Log.LogMessage("Ticket email was not send.");
            } catch (System.Exception ex) {
                ScrumFactory.Services.Logic.Helper.Log.LogError(ex);
            }
        }

        [WebInvoke(Method = "POST", UriTemplate = "PokerCards/{backlogItemUId}", RequestFormat = WebMessageFormat.Json)]
        public ICollection<PokerCard> VotePokerCard(string backlogItemUId, PokerCard card)
        {
            card.MemberUId = authorizationService.SignedMemberProfile.MemberUId;            
            card.BacklogItemUId = backlogItemUId;
            backlogRepository.SavePokerCard(card);
            return backlogRepository.GetPokerCards(card.BacklogItemUId);
        }

        [WebGet(UriTemplate = "PokerCards/{backlogItemUId}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<PokerCard> GetPokerCards(string backlogItemUId)
        {
            return backlogRepository.GetPokerCards(backlogItemUId);
        }

    }
}
