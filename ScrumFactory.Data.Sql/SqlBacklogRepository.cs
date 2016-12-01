namespace ScrumFactory.Data.Sql
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Data.Objects;
    using System.Data.EntityClient;
    using System.Transactions;
    using System.Linq;

    [Export(typeof(IBacklogRepository))]
    public class SqlBacklogRepository : IBacklogRepository
    {
        private string connectionString;

        [ImportingConstructor()]
        public SqlBacklogRepository(
            [Import("ScrumFactoryEntitiesConnectionString")]
            string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets the entire backlog of a project from the database and its lasted planned hours.
        /// </summary>
        /// <param name="projectUId">The projectUId</param>
        /// <returns>A list of backlog items</returns>
        public ICollection<ScrumFactory.BacklogItem> GetCurrentBacklog(string projectUId, System.DateTime fromDate = new System.DateTime(), System.DateTime untilDate = new System.DateTime()) {
            using (var context = new ScrumFactoryEntities(this.connectionString))
            {
                
                // get all items from the backlog 
                IQueryable<BacklogItem> items =
                    context.BacklogItems.Where(i => i.ProjectUId == projectUId);

                items = FilterBacklog(items, fromDate, untilDate);
               
                // no selects the current planninh hours for the items
                var itemsWithHours = items 
                    .Select(
                        i => new {
                            BacklogItem = i,                            
                            PlannedHours = i.PlannedHours.Where(ih => ih.PlanningNumber == i.PlannedHours.Max(h => h.PlanningNumber))
                        });
                                
                return itemsWithHours.AsEnumerable().Select(i => i.BacklogItem).OrderBy(i=>i.SprintNumber).ThenBy(i=>i.BusinessPriority).ThenBy(i=>i.BacklogItemNumber).ToList<BacklogItem>();

            }
        }

        /// <summary>
        /// Gets the entire backlog of a project from the database and its all its planned hours.
        /// </summary>
        /// <param name="projectUId">The projectUId</param>
        /// <returns>A list of backlog items</returns>
        public ICollection<ScrumFactory.BacklogItem> GetBacklog(string projectUId, int? planningNumber = null, System.DateTime fromDate = new System.DateTime(), System.DateTime untilDate = new System.DateTime()) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // get all items from the backlog 
                IQueryable<BacklogItem> items =
                    context.BacklogItems.Where(i => i.ProjectUId == projectUId);

                items = FilterBacklog(items, fromDate, untilDate);
                
                // now selects the current planninh hours for the items
                if (planningNumber.HasValue) {
                    var itemsWithHours = items
                        .Select(
                            i => new {
                                BacklogItem = i,
                                PlannedHours = i.PlannedHours.Where(ih => ih.PlanningNumber == planningNumber)
                            });
                    return itemsWithHours.AsEnumerable().Select(i => i.BacklogItem).OrderBy(i => i.SprintNumber).ThenBy(i => i.BusinessPriority).ThenBy(i => i.BacklogItemNumber).ToList<BacklogItem>();
                } else {
                    return context.BacklogItems.Include("PlannedHours").Where(i => i.ProjectUId == projectUId).AsEnumerable().OrderBy(i => i.SprintNumber).ThenBy(i => i.BusinessPriority).ThenBy(i => i.BacklogItemNumber).ToList();
                }
            }
            
        }

        private IQueryable<BacklogItem> FilterBacklog(IQueryable<BacklogItem> items, System.DateTime fromDate, System.DateTime untilDate) {
            
            if (fromDate!=System.DateTime.MinValue)
                items = items.Where(i => i.FinishedAt >= fromDate || (i.FinishedAt==null && i.CreateDate  >= fromDate));
            if (untilDate != System.DateTime.MinValue)
                items = items.Where(i => i.FinishedAt <= untilDate || (i.FinishedAt == null && i.CreateDate <= untilDate));

            return items;
        }

        public ICollection<ScrumFactory.BacklogItem> GetBacklogOnlyItems(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.BacklogItems.Where(i => i.ProjectUId == projectUId).ToList();               
            }
        }

        public ICollection<ScrumFactory.BacklogItem> GetAllUnfinishedBacklogItems(string[] projectUIds) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // get all unfinished items from the backlog 
                IQueryable<BacklogItem> items =
                    context.BacklogItems.Where(i => i.Status < 2 && projectUIds.Contains(i.ProjectUId));

                // no selects the current planninh hours for the items
                var itemsWithHours = items
                    .Select(
                        i => new {
                            BacklogItem = i,
                            PlannedHours = i.PlannedHours.Where(ih => ih.PlanningNumber == i.PlannedHours.Max(h => h.PlanningNumber))
                        });

                return itemsWithHours.AsEnumerable().Select(i => i.BacklogItem).OrderBy(i => i.SprintNumber).ThenBy(i => i.BusinessPriority).ThenBy(i => i.BacklogItemNumber).ToList<BacklogItem>();

            }
        }


        /// <summary>
        /// Gets a backlog item from the database and its lasted planned hours.
        /// </summary>
        /// <param name="backlogItemUId">The backlogitemUid</param>
        /// <returns>The backlog item</returns>
        public ScrumFactory.BacklogItem GetBacklogItem(string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return GetBacklogItem(context, backlogItemUId);
            }
        }

        public ScrumFactory.BacklogItem GetBacklogItem(ScrumFactoryEntities context, string backlogItemUId) {            
            var itemWithHours =
                context.BacklogItems.Include("Group").Where(i => i.BacklogItemUId == backlogItemUId)
                .Select(
                    i => new {
                        BacklogItem = i,                        
                        Group = i.Group,
                        PlannedHours = i.PlannedHours.Where(ih => ih.PlanningNumber == i.PlannedHours.Max(h => h.PlanningNumber))
                    });

            return itemWithHours.AsEnumerable().Select(i => i.BacklogItem).SingleOrDefault<BacklogItem>();
        }

        public bool IsBacklogItemFirstPlan(Project project, string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
          
                int? minPlanNumber = context.PlannedHours.Where(p => p.BacklogItemUId == backlogItemUId && p.SprintNumber!=null).Min(p =>  (int?)p.PlanningNumber);
                if (minPlanNumber == null)
                    return true;

                if (project.CurrentPlanningNumber != (int)minPlanNumber)
                    return false;

                return true;
                
            }
        }
        
        public void DeleteBacklogItem(string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var item = context.BacklogItems.Include("PlannedHours").SingleOrDefault(b => b.BacklogItemUId == backlogItemUId);

                if (item != null) {
                    context.DeleteObject(item);
                    context.SaveChanges();
                }
            }
        }


        /// <summary>
        /// Tries to save the item and in case of unique key constraint violation,
        /// tries again four more times.
        /// </summary>
        /// <param name="action">The action of save the item</param>
        private void TryAgainSave(System.Action action) {
            int tries = 0;
            bool success = false;
            System.Data.SqlClient.SqlException lastException = null;
            while (tries < 5 && !success) {
                try {
                    action();
                    success = true;                    
                } catch (System.Data.UpdateException ex) {
                    lastException = ex.InnerException as System.Data.SqlClient.SqlException;
                    if (lastException == null)
                        throw ex;
                    if (lastException.Number != 2601 && lastException.Number != 2627)
                        throw ex;
                    tries++;
                }
            }
            // if after 4 times 
            if (!success)
                throw lastException;

        }
        

        /// <summary>
        /// Saves a backlog item and its hours.
        /// </summary>
        /// <param name="item">The item to be saved</param>
        public void SaveBacklogItem(BacklogItem item) {
            // I dont want to use a transaction to get a safe next backlog item number
            // because the SERIALIZABLE isolaton level will lock the whole table.
            // Better than that, i would rather try to update severl times until i got a valid number.
            TryAgainSave(() => { SaveBacklogItem(item, true); });            
        }

        /// <summary>
        /// Saves a backlog item, but ignores it hours.
        /// </summary>
        /// <param name="item">The item to be saved</param>
        public void SaveBacklogItemIgnoreHours(BacklogItem item) {
            // I dont want to use a transaction to get a safe next backlog item number
            // because the SERIALIZABLE isolaton level will lock the whole table.
            // Better than that, i would rather try to update severel times until i got a valid number.
            TryAgainSave(() => { SaveBacklogItem(item, false); });                
        }

        private void SaveBacklogItem(BacklogItem item, bool saveHours) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                SaveBacklogItem(context, item, saveHours);
                context.SaveChanges();
            }
        }


        private void SaveBacklogItem(ScrumFactoryEntities context, BacklogItem item, bool saveHours) {
            
                // dont want to add the item group again, so if the group is already at the database
                // remove it from the item
                BacklogItemGroup group = context.BacklogItemGroups.SingleOrDefault(g => g.GroupUId == item.GroupUId); 
                if (group != null)
                    item.Group = null;

                BacklogItem oldItem = GetBacklogItem(context, item.BacklogItemUId);

                // if is a new item insert it
                if (oldItem == null) {
                    int? lastNumber = context.BacklogItems.Where(b => b.ProjectUId == item.ProjectUId).Max(b => (int?)b.BacklogItemNumber);
                    if (lastNumber == null)
                        lastNumber = 0;
                    item.BacklogItemNumber = (int)lastNumber + 1;
                    context.BacklogItems.AddObject(item);                    
                }                    
                else {

                    // updates the item
                    context.AttachTo("BacklogItems", oldItem);
                    context.ApplyCurrentValues<BacklogItem>("BacklogItems", item);

                    if (saveHours) {

                        // detect the changes
                        var insertedHours = item.PlannedHours;
                        if (oldItem.PlannedHours != null)
                            insertedHours = item.PlannedHours.Where(p => !oldItem.PlannedHours.Any(o => (o.BacklogItemUId == p.BacklogItemUId && o.RoleUId == p.RoleUId && o.PlanningNumber == p.PlanningNumber))).ToList();

                        // ATTENTION HERE: DID NOT INCLUDE THE o.PlanningNumber == p.PlanningNumber BECAUSE I DONT WANT TO DELETE OLD PLANNINGS
                        var deletedHours = new List<PlannedHour>();
                        if (oldItem.PlannedHours != null)
                            deletedHours = oldItem.PlannedHours.Where(o => !item.PlannedHours.Any(p => o.BacklogItemUId == p.BacklogItemUId && o.RoleUId == p.RoleUId)).ToList();

                        var updatedHours = new List<PlannedHour>();
                        if (oldItem.PlannedHours != null)
                            updatedHours = item.PlannedHours.Where(p => oldItem.PlannedHours.Any(o => o.BacklogItemUId == p.BacklogItemUId && o.RoleUId == p.RoleUId && o.PlanningNumber == p.PlanningNumber)).ToList();

                        // insert, update and delete
                        foreach (PlannedHour p in updatedHours)
                            context.ApplyCurrentValues<PlannedHour>("PlannedHours", p);
                        foreach (PlannedHour p in insertedHours)
                            context.AddObject("PlannedHours", p);
                        foreach (PlannedHour p in deletedHours)
                            context.DeleteObject(p);

                    }


                }

        }

        public ICollection<ItemSize> GetItemSizes() {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ItemSizes.Include("SizeIdealHours").OrderBy(z => z.Name).ToList();
                
            }
        }

        public ItemSize GetItemSize(string itemSizeUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ItemSizes.Include("SizeIdealHours").SingleOrDefault(z => z.ItemSizeUId == itemSizeUId);
            }
        }

        public ItemSize GetItemSizeByConstraint(short constraint) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ItemSizes.Include("SizeIdealHours").FirstOrDefault(z => z.OccurrenceConstraint == constraint);
            }
        }

        public void SaveItemSize(ItemSize size) {

            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                ItemSize oldSize = GetItemSize(size.ItemSizeUId);

                if (oldSize == null) {
                    context.ItemSizes.AddObject(size);
                }
                else {
                    context.AttachTo("ItemSizes", oldSize);                                        
                    context.ApplyCurrentValues<ItemSize>("ItemSizes", size);

                    if (oldSize.SizeIdealHours == null)
                        oldSize.SizeIdealHours = new List<SizeIdealHour>();

                    if(size.SizeIdealHours==null)
                        oldSize.SizeIdealHours = new List<SizeIdealHour>();

                    var deletedHours = oldSize.SizeIdealHours.Where(o => !size.SizeIdealHours.Any(s => s.IdealHourUId == o.IdealHourUId));
                    var updatedHours = size.SizeIdealHours.Where(s => oldSize.SizeIdealHours.Any(o => o.IdealHourUId == s.IdealHourUId));
                    var insertedHours = size.SizeIdealHours.Where(s => !oldSize.SizeIdealHours.Any(o => o.IdealHourUId == s.IdealHourUId));

                    foreach (SizeIdealHour h in deletedHours)
                        context.SizeIdealHours.DeleteObject(h);

                    foreach (SizeIdealHour h in updatedHours)
                        context.ApplyCurrentValues<SizeIdealHour>("SizeIdealHours", h);

                    foreach (SizeIdealHour h in insertedHours)
                        context.SizeIdealHours.AddObject(h);

                    
                }

                context.SaveChanges();

            }

        }

        public bool IsItemSizeAlreadyPlanned(string itemSizeUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.BacklogItems.Any(i => i.ItemSizeUId == itemSizeUId);                
            }
        }

        public void DeleteItemSize(string itemSizeUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                ItemSize size = context.ItemSizes.Include("SizeIdealHours").SingleOrDefault(z => z.ItemSizeUId == itemSizeUId);                
                if (size == null)
                    return;
                //if (size.SizeIdealHours != null) {
                //    foreach (SizeIdealHour h in size.SizeIdealHours)
                //        context.SizeIdealHours.DeleteObject(h);
                //}
                context.ItemSizes.DeleteObject(size);
                
                context.SaveChanges();
            }
        }

        public void UpdateItemSizeOccurrenceContraint(string itemSizeUId, ItemOccurrenceContraints constraint) {
            
            // gets the item and set the constraint
            ItemSize newSize = GetItemSize(itemSizeUId);
            if (newSize == null)
                return;            
            
            // now seacrh for the olditem with the same constraint and clean it
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                ItemSize oldSize = context.ItemSizes.SingleOrDefault(z => z.OccurrenceConstraint == (int)constraint);
                if (oldSize != null) 
                    oldSize.OccurrenceConstraint = (int)ItemOccurrenceContraints.DEVELOPMENT_OCC;                    
                
                context.AttachTo("ItemSizes", newSize);
                newSize.OccurrenceConstraint = (int)constraint;
       
                context.SaveChanges();
            }
            
        }

        public BacklogItemGroup GetBacklogItemGroup(string groupUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.BacklogItemGroups.SingleOrDefault(g => g.GroupUId == groupUId);
            }
        }

        public void DeleteBacklogItemGroup(string groupUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var group = context.BacklogItemGroups.SingleOrDefault(g => g.GroupUId == groupUId);
                context.BacklogItemGroups.DeleteObject(group);
            }
        }

        public ICollection<BacklogItemGroup> GetBacklogItemGroups(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.BacklogItemGroups.Where(g => g.ProjectUId == projectUId).ToList();
            }
        }

        public void UpdateBacklogItemGroup(BacklogItemGroup group) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                BacklogItemGroup oldGroup = context.BacklogItemGroups.Where(g => g.GroupUId == group.GroupUId).SingleOrDefault();
                if (oldGroup == null) 
                    context.AddObject("BacklogItemGroups", group);
                else
                    context.ApplyCurrentValues<BacklogItemGroup>("BacklogItemGroups", group);

                context.SaveChanges();
                
            }
        }

        public ICollection<BacklogItem> GetDoneItemsBySize(string sizeUId, int depth) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<BacklogItem> items = context.BacklogItems
                    .Where(i => i.ItemSizeUId == sizeUId && i.SizeFactor>0 && i.Status == (short)BacklogItemStatus.ITEM_DONE)
                    .OrderByDescending(i=>i.FinishedAt);
                if (depth > 0)
                    items = items.Take(depth);
                return items.ToList();
            }
        }

        public ICollection<BacklogItem> GetDoneItemsByDepth(int depth) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<BacklogItem> items = context.BacklogItems
                    .Where(i => i.ItemSizeUId != null && i.SizeFactor > 0 && i.Status == (short)BacklogItemStatus.ITEM_DONE)
                    .OrderByDescending(i => i.FinishedAt);
                if (depth > 0)
                    items = items.Take(depth);
                return items.ToList();
            }
        }

        public decimal GetTotalPointsDone(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? pts = context.BacklogItems.Where(i => i.Status == (short)BacklogItemStatus.ITEM_DONE && i.ProjectUId == projectUId).Sum(i => i.Size);
                if (!pts.HasValue)
                    return 0;
                return pts.Value;
            }
        }


        public void UpdateBacklogItemArtifactCount(string backlogItemUId, int count) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var item = context.BacklogItems.SingleOrDefault(i => i.BacklogItemUId == backlogItemUId);
                if (item == null)
                    return;
                item.ArtifactCount = count;
                context.SaveChanges();
            }
        }
      
        
    }
}
