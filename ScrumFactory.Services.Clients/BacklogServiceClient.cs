using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;

namespace ScrumFactory.Services.Clients {


    [Export(typeof(IBacklogService))]
    public class BacklogServiceClient : IBacklogService {

        public BacklogServiceClient() { }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("BacklogServiceUrl")]
        private string serviceUrl { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        public ICollection<BacklogItem> GetAllUnfinishedBacklogItems(bool onlyMine, string projectFilter) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("UnfinishedBacklogItems/?onlyMine=" + onlyMine + "&projectFilter=" + projectFilter));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItem>>();
        }

        public ICollection<BacklogItem> GetCurrentBacklog(string projectUId, short filterMode, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime()) {
            var client = ClientHelper.GetClient(authorizator);
            string ps = "";
            if (fromDate!=DateTime.MinValue)
                ps = ps + "&fromDate=" + fromDate.ToString("yyyy-MM-dd");
            if (untilDate != DateTime.MinValue)
                ps = ps + "&untilDate=" + untilDate.ToString("yyyy-MM-dd");
            HttpResponseMessage response = client.Get(Url("Backlogs/" + projectUId + "/?planning=current&filterMode=" + filterMode + ps));
            ClientHelper.HandleHTTPErrorCode(response);           
            return response.Content.ReadAs<ICollection<BacklogItem>>();            
        }

        public ICollection<BacklogItem> GetBacklog(string projectUId, string planning = "current", short filterMode = 3, DateTime fromDate = new DateTime(), DateTime untilDate = new DateTime()) {
            var client = ClientHelper.GetClient(authorizator);
            string ps = "";
            if (fromDate != DateTime.MinValue)
                ps = ps + "&fromDate=" + fromDate.ToString("yyyy-MM-dd");
            if (untilDate != DateTime.MinValue)
                ps = ps + "&untilDate=" + untilDate.ToString("yyyy-MM-dd");
            HttpResponseMessage response = client.Get(Url("Backlogs/" + projectUId + "/?planning=" + planning + "&filterMode=" + filterMode + ps));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItem>>();
        }

        public BacklogItem GetBacklogItem(string backlogItemUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("BacklogItems/" + backlogItemUId));
            ClientHelper.HandleHTTPErrorCode(response);           
            return response.Content.ReadAs<BacklogItem>();
        }

        public ICollection<BacklogItem> GetItemsBySize(string sizeUId, int depth) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a backlog item.
        /// If the item planned hours is old, it also replan the hours for the current planning number.
        /// </summary>
        /// <param name="item">The iem to be saved</param>
        public int AddBacklogItem(BacklogItem item) {

            if (item.Project == null)
                throw new Exception("Can not save a backlogitem without a Project property");

            var client = ClientHelper.GetClient(authorizator); 
            HttpResponseMessage response = client.Post(Url("BacklogItems"), new ObjectContent<BacklogItem>(item, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            item.BacklogItemNumber = response.Content.ReadAs<int>();
            return item.BacklogItemNumber;
        }

        public void ChangeBacklogItemGroup(string backlogItemUId, string groupUId) {            
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("BacklogItems/" + backlogItemUId + "/GroupUId"), new ObjectContent<string>(groupUId, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ChangeBacklogItemStatus(string backlogItemUId, short status) {
            var client = ClientHelper.GetClient(authorizator);            
            HttpResponseMessage response = client.Put(Url("BacklogItems/" + backlogItemUId + "/Status"), new ObjectContent<short>(status, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ChangeBacklogItemIssueType(string backlogItemUId, short issueType) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a backlog item.
        /// If the item planned hours is old, it also replan the hours for the current planning number.
        /// </summary>
        /// <param name="item">The iem to be saved</param>
        public void UpdateBacklogItem(string backlogItemUId, BacklogItem item) {

            if (item.Project == null)
                throw new Exception("Can not save a backlogitem without a null Project property");

            // if the item planning is old, add planned hours for the current planning
            int currentPlanningNumber = item.Project.CurrentPlanningNumber;            
            if (currentPlanningNumber > item.PlanningNumber) {

                List<PlannedHour> replannedHours = new List<PlannedHour>();
                foreach (PlannedHour h in item.PlannedHours)
                    replannedHours.Add(new PlannedHour() {
                        BacklogItemUId = h.BacklogItemUId,
                        PlanningNumber = currentPlanningNumber,
                        SprintNumber = h.SprintNumber,
                        RoleUId = h.RoleUId,
                        Role = h.Role,
                        Hours = h.Hours
                    });

                item.PlannedHours = replannedHours;
            }
            
            var client = ClientHelper.GetClient(authorizator);            
            HttpResponseMessage response = client.Put(Url("BacklogItems/" + item.BacklogItemUId), new ObjectContent<BacklogItem>(item, JsonValueMediaTypeFormatter.DefaultMediaType));            
            ClientHelper.HandleHTTPErrorCode(response);           
        }

        /// <summary>
        /// Saves a backlog item, ignoring its hours.
        /// </summary>
        /// <param name="item">The iem to be saved</param>
        public void UpdateBacklogItemIgnoringHours(string backlogItemUId, BacklogItem item) {
            var client = ClientHelper.GetClient(authorizator);                        
            HttpResponseMessage response = client.Put(Url("BacklogItems/" + item.BacklogItemUId + "/?IgnoreHours=true"), new ObjectContent<BacklogItem>(item, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);           
        }

       

        public BacklogItem[] MoveItem(string backlogItemUId, string targetBacklogItemUId) {
            var client = ClientHelper.GetClient(authorizator);                        
            HttpResponseMessage response = client.Post(Url("BacklogItems/" + backlogItemUId + "/BusinessPriority/_moveTo"), new ObjectContent<string>(targetBacklogItemUId, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            if (response.Content.ContentReadStream.Length == 0)
                return null;
            return response.Content.ReadAs<BacklogItem[]>();
        }

        public BacklogItem[] ChangeItemSprint(string backlogItemUId, int sprintNumber, bool lowPriority) {
            var client = ClientHelper.GetClient(authorizator);                        
            HttpResponseMessage response = client.Post(Url("BacklogItems/" + backlogItemUId + "/SprintNumber?lowPriority=" + lowPriority), new ObjectContent<int>(sprintNumber, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            if (response.Content.ContentReadStream.Length == 0)
                return null;
            return response.Content.ReadAs<BacklogItem[]>();
        }

        public void DeleteBacklogItem(string backlogItemUId) {
            var client = ClientHelper.GetClient(authorizator);                        
            HttpResponseMessage response = client.Delete(Url("BacklogItems/" + backlogItemUId));
            ClientHelper.HandleHTTPErrorCode(response);           
        }


        public ICollection<BurndownLeftHoursByDay> GetBurndownHoursByDay(string projectUId, string planning) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("BurndownHoursByDay/" + projectUId + "?planning=" + planning));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BurndownLeftHoursByDay>>();
        }

        
        public ICollection<ItemSize> GetItemSizes() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("ItemSizes/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<ItemSize>>();
        }


        public void AddItemSize(ItemSize size) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Post(Url("ItemSizes"), new ObjectContent<ItemSize>(size, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);           
        }

        public void UpdateItemSize(string itemSizeUId, ItemSize size) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("ItemSizes/" + size.ItemSizeUId), new ObjectContent<ItemSize>(size, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);           
        }

        public void UpdateItemSizeOccurrenceContraint(string itemSizeUId, string constraint) {
            var client = ClientHelper.GetClient(authorizator);

            // TO DO : this is wronh should PUT contraint as content

            HttpResponseMessage response = client.Put(Url("ItemSizes/" + itemSizeUId + "/OccurrenceContraint/" + constraint), null);
            ClientHelper.HandleHTTPErrorCode(response);           
        }

        public void DeleteItemSize(string itemSizeUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Delete(Url("ItemSizes/" + itemSizeUId));
            ClientHelper.HandleHTTPErrorCode(response);           
        }


        public void EqualizeSprints(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("EqualizedBacklogs/"), new ObjectContent<string>(projectUId, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);           
            
        }

        public void ShiftItemsAfter(string backlogitemUId, string planItemName, string deliveryItemName) {
            var client = ClientHelper.GetClient(authorizator);
            string param = String.Format("?planItemName={0}&deliveryItemName={1}", planItemName, deliveryItemName);
            HttpResponseMessage response = client.Put(Url("BacklogItems/" + backlogitemUId + "/_shiftAfter/" + param), new ObjectContent<string>(backlogitemUId, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
        }


        public ICollection<BacklogItemGroup> GetBacklogItemGroups(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Groups/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItemGroup>>();
        }

        public void UpdateBacklogItemGroup(string projectUId, BacklogItemGroup group) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Put(Url("Groups/" + projectUId), new ObjectContent<BacklogItemGroup>(group, JsonValueMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);           
        }

        public Dictionary<string, decimal?> GetVelocityBySize(string sizeUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("ItemVelocity/" + sizeUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<Dictionary<string, decimal?>>();
        }

        public decimal GetProjectVelocityIndicator(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("VelocityIndicator/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<decimal>();
        }

        public decimal GetVelocityIndicator() {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("VelocityIndicator/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<decimal>();
        }

        public ICollection<BacklogItem> GetItemsTotalEffectiveHours(string projectUId) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("BacklogsWithEffectiveHours/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<BacklogItem>>();
        }
    

        public void UpdateItemStatusToWorking(Task task) {
            throw new NotSupportedException();
        }

        public void DeleteBacklogItemGroup(string groupUId) {
            throw new NotImplementedException();
        }





    }
}
