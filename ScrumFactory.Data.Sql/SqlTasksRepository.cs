namespace ScrumFactory.Data.Sql {
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Data.Objects;
    using System.Data.EntityClient;
    using System.Transactions;
    using System.Linq;

    [Export(typeof(ITasksRepository))]
    public class SqlTasksRepository : ITasksRepository {
        private string connectionString;

        [ImportingConstructor()]
        public SqlTasksRepository(
            [Import("ScrumFactoryEntitiesConnectionString")]
            string connectionString) {
            this.connectionString = connectionString;
        }

        public ICollection<Task> GetProjectTasks(string projectUId, System.DateTime fromDate, System.DateTime toDate, bool dailyTasksOnly, string reviewTagUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                
                IQueryable<Task> tasks = context.Tasks.Include("TaskInfo").Where(t => t.ProjectUId == projectUId);

                if (!dailyTasksOnly && !fromDate.Equals(System.DateTime.MinValue))
                    tasks = tasks.Where(t => t.CreatedAt >= fromDate);

                if (!dailyTasksOnly && !toDate.Equals(System.DateTime.MinValue))
                    tasks = tasks.Where(t => t.CreatedAt <= toDate);

                if (dailyTasksOnly) {
                    System.DateTime lastDay = System.DateTime.Today.AddDays(-1);
                    if (lastDay.DayOfWeek == System.DayOfWeek.Saturday)
                        lastDay = lastDay.AddDays(-1);
                    if (lastDay.DayOfWeek == System.DayOfWeek.Sunday)
                        lastDay = lastDay.AddDays(-2);
                    if (lastDay.DayOfWeek == System.DayOfWeek.Monday)
                        lastDay = lastDay.AddDays(-3);

                    tasks = tasks.Where(t => t.Status <= (short)TaskStatus.WORKING_ON_TASK || t.EndDate >= lastDay);
                }

                if (!string.IsNullOrEmpty(reviewTagUId))
                    tasks = tasks.Where(t => t.TagUId == reviewTagUId);

                return tasks.ToList();
            }
        }

        public ICollection<Task> GetItemTasks(string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var tasks = context.Tasks.Where(t => t.BacklogItemUId == backlogItemUId);                
                return tasks.ToList();
            }
        }

        public ICollection<Task> GetUsersTasks(string[] membersUIds, bool onlyOpen, bool includeUnassigned, string[] projectUIds = null) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var tasks = context.Tasks.Include("TaskInfo");

                if (includeUnassigned && projectUIds==null) {
                    tasks = tasks.Where(t => membersUIds.Contains(t.TaskAssigneeUId) || (t.TaskAssigneeUId == null && membersUIds.Contains(t.TaskOwnerUId)));
                } 
                else if (includeUnassigned && projectUIds != null) {
                    tasks = tasks.Where(t => membersUIds.Contains(t.TaskAssigneeUId) || (t.TaskAssigneeUId == null && projectUIds.Contains(t.ProjectUId)));
                }
                else {
                    tasks = tasks.Where(t => membersUIds.Contains(t.TaskAssigneeUId));
                }

                if (onlyOpen)
                    tasks = tasks.Where(t => t.Status <2);
                return tasks.ToList();
            }
        }

        public ICollection<Task> GetUserTasks(string taskAssigneeUId, bool onlyOpen, bool includeUnassigned, string[] projectUIds = null) {
            return GetUsersTasks(new string[] { taskAssigneeUId }, onlyOpen, includeUnassigned, projectUIds);
        }

        public MemberPerformance GetMemberPerformance(string memberUId) {
            MemberPerformance memberPerformance = new ScrumFactory.MemberPerformance() { MemberUId = memberUId };
            System.DateTime today = System.DateTime.Today;
            int thisYear = today.Year;
            int thisMonth = today.Month;
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                IQueryable<Task> doneTasks = context.Tasks.Where(t => t.TaskAssigneeUId == memberUId && t.Status == 2);
                IQueryable<Task> doneTasksThisMonth = doneTasks.Where(t=> t.EndDate!=null && t.EndDate.Value.Year==thisYear && t.EndDate.Value.Month==thisMonth);

                memberPerformance.TasksDone = doneTasks.Count();

                decimal? hours = doneTasks.Sum(t => (decimal?)t.EffectiveHours);
                memberPerformance.TotalWorkedHours = hours.HasValue ? hours.Value : 0;

                memberPerformance.BugsResolved = doneTasksThisMonth.Count(t => t.TaskType == 4);
                memberPerformance.ImprovimentsDone = doneTasksThisMonth.Count(t => t.TaskType == 0);
                
                memberPerformance.TasksDoneBeforePlanned = doneTasksThisMonth.Count(t => t.EffectiveHours < t.PlannedHours);
                
                decimal? monthHours = doneTasksThisMonth.Sum(t => (decimal?)t.EffectiveHours);
                memberPerformance.MonthWorkedHours = monthHours.HasValue ? monthHours.Value : 0;
            }
                
            return memberPerformance;
        }

        public ICollection<Task> GetUserOwnedTasks(string taskOwnerUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var ownedtasks = context.Tasks.Include("TaskInfo").Where(t => t.TaskOwnerUId == taskOwnerUId && (t.TaskAssigneeUId != taskOwnerUId || t.TaskAssigneeUId==null) && t.Status != (short)TaskStatus.DONE_TASK && t.Status != (short)TaskStatus.CANCELED_TASK);
                ownedtasks = ownedtasks.Where(t => context.Projects.Any(p => p.ProjectUId == t.ProjectUId && (p.Status == 3 || p.Status == 5))); // only running projects
                return ownedtasks.ToList();
            }
        }

        public void SaveTaskTag(TaskTag tag) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var oldTag = context.TaskTags.SingleOrDefault(t => t.TagUId == tag.TagUId);
                if (oldTag == null) {
                    context.TaskTags.AddObject(tag);
                } else {
                    context.AttachTo("TaskTags", oldTag);
                    context.ApplyCurrentValues<TaskTag>("TaskTags", tag);
                }

                context.SaveChanges();
            }
        }

        public void DeleteTaskTag(string tagUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                
                // first un-mark tasks
                var tasks = context.Tasks.Where(t => t.TagUId == tagUId);
                foreach (Task task in tasks)
                    task.TagUId = null;

                // now deletes the tag
                var oldTag = context.TaskTags.SingleOrDefault(t => t.TagUId == tagUId);
                context.TaskTags.DeleteObject(oldTag);

                context.SaveChanges();
            }
        }
        
        public ICollection<TaskTag> GetTaskTags(string projectUId, string filter = "ALL") {

            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var ts = context.TaskTags.Where(t => t.ProjectUId == projectUId)
                    .Select(t => new { Tag = t, OpenTasksCount = context.Tasks.Count(ta => ta.TagUId==t.TagUId && (ta.Status == (short)TaskStatus.REQUIRED_TASK || ta.Status == (short)TaskStatus.WORKING_ON_TASK))}).AsEnumerable();

                List<TaskTag> tags = new List<TaskTag>();
                foreach (var t in ts) {
                    t.Tag.OpenTasksCount = t.OpenTasksCount;
                    tags.Add(t.Tag);
                }
                return tags;
            }
        }

        public ICollection<BacklogItemEffectiveHours> GetItemTasksEffectiveHoursByProject(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.BacklogItemEffectiveHours.Where(t => t.ProjectUId == projectUId).ToList();
            }
        }

        public decimal GetTotalEffectiveHoursByItem(string backlogitemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? hrs = context.Tasks.Where(t => t.BacklogItemUId == backlogitemUId).Sum(t => (decimal?) t.EffectiveHours);
                if (!hrs.HasValue)
                    return 0;
                return hrs.Value;
            }
        }

    

        public ICollection<TodayMemberPlannedHours> GetTodayMemberPlannedHoursByUIds(string[] membersUIds) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                var hours = context.Tasks.Where(t => t.Status < 2 && membersUIds.Contains(t.TaskAssigneeUId))
                    .GroupBy(t => t.TaskAssigneeUId)
                    .Select(g => new { TaskAssigneeUId = g.Key, PlannedHours = g.Sum(t => t.PlannedHours - t.EffectiveHours) });

                return hours.AsEnumerable().Select(h => new TodayMemberPlannedHours() { TaskAssigneeUId = h.TaskAssigneeUId, PlannedHours = h.PlannedHours }).ToList();
                
                //return context.TodayMemberPlannedHours.Where(m => membersUIds.Contains(m.TaskAssigneeUId)).ToList();
            }
        }

        public Task GetTask(string taskUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Tasks.Include("TaskDetail").Where(t => t.TaskUId == taskUId).SingleOrDefault();
            }
        }

        public Task GetTaskByNumber(int taskNumber) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Tasks.Include("TaskDetail").Where(t => t.TaskNumber == taskNumber).SingleOrDefault();
            }
        }

        public TaskDetail GetTaskDetail(string taskUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.TaskDetails.Where(t => t.TaskUId == taskUId).SingleOrDefault();
            }
        }


        public decimal GetTotalEffectiveHoursByRole(string roleUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? hours = context.Tasks.Where(t => t.RoleUId == roleUId).Sum(t => (decimal?) t.EffectiveHours);
                if (!hours.HasValue)
                    return 0;
                return hours.Value;
            }
        }

        public void SaveTask(Task task) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                Task oldTask = GetTask(task.TaskUId);

                if (oldTask == null) {
                    context.Tasks.AddObject(task);
                    if(task.TaskDetail!=null)
                        context.TaskDetails.AddObject(task.TaskDetail);
                } else {

                    // apply current values, but preserve create at date
                    System.DateTime createAt = oldTask.CreatedAt;
                    string owner = oldTask.TaskOwnerUId;

                    context.AttachTo("Tasks", oldTask);
                    context.ApplyCurrentValues<Task>("Tasks", task);
                    oldTask.CreatedAt = createAt;
                    oldTask.TaskOwnerUId = owner;

                    if (task.TaskDetail != null && oldTask.TaskDetail==null) {
                        context.TaskDetails.AddObject(task.TaskDetail);
                    }
                    else
                    if (task.TaskDetail != null && oldTask.TaskDetail != null) {
                        context.AttachTo("TaskDetails", oldTask.TaskDetail);
                        context.ApplyCurrentValues<TaskDetail>("TaskDetails", task.TaskDetail);
                    }
                }

                context.SaveChanges();
             
            }
        }

        public decimal GetTotalEffectiveHoursByProject(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? hours = context.Tasks.Where(t => t.ProjectUId == projectUId).Sum(t => (decimal?)t.EffectiveHours);
                if (!hours.HasValue)
                    return 0;
                return hours.Value;
            }
        }

        public decimal GetTotalBugHoursByProject(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                decimal? hours = context.Tasks.Where(t => t.ProjectUId == projectUId && (t.TaskType==(short)TaskTypes.BUG_TASK || t.TaskType==(short)TaskTypes.AFTER_DEPLOY_BUG_TASK)).Sum(t => (decimal?)t.EffectiveHours);
                if (!hours.HasValue)
                    return 0;
                return hours.Value;
            }
        }

        public bool DoesBacklogItemHasTasks(string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Tasks.Any(t => t.BacklogItemUId == backlogItemUId);
            }
        }

        public bool DoesMemberHasAnyTaskAtProject(string projectUId, string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Tasks.Any(t => t.ProjectUId ==projectUId && t.TaskAssigneeUId==memberUId);
            }
        }

        public void UpdateTaskArtifactCount(string taskUId, int count) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                var task = context.Tasks.SingleOrDefault(t => t.TaskUId == taskUId);
                if (task == null)
                    return;
                task.ArtifactCount = count;
                context.SaveChanges();
            }
        }

        public PlannedHour[] GetTotalEffectiveHoursByItem(string backlogitemUId, ICollection<Role> roles) {
            List<PlannedHour> hours = new List<PlannedHour>();
            using (var context = new ScrumFactoryEntities(this.connectionString)) {            
                foreach (Role role in roles) {
                    decimal? hour = context.Tasks.Where(t => t.BacklogItemUId == backlogitemUId && t.RoleUId==role.RoleUId).Sum(t => (decimal?)t.EffectiveHours);
                    hours.Add(new PlannedHour() { BacklogItemUId = backlogitemUId, RoleUId = role.RoleUId, Hours = hour });
                }
                decimal? hourOfNullRole = context.Tasks.Where(t => t.BacklogItemUId == backlogitemUId && t.RoleUId == null).Sum(t => (decimal?)t.EffectiveHours);
                hours.Add(new PlannedHour() { BacklogItemUId = backlogitemUId, RoleUId = null, Hours = hourOfNullRole });
            }
            return hours.ToArray();

        }


        public IDictionary<string, decimal> GetTotalEffectiveHoursByProjectAndMember(string projectUId, string memberUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                IQueryable<Task> tasks = context.Tasks.Where(t => t.ProjectUId == projectUId);

                if (memberUId != null)
                    tasks = tasks.Where(t => t.TaskAssigneeUId == memberUId);

                var memberHours = tasks.GroupBy(t => t.TaskAssigneeUId).Select(group => new { memberUId = group.Key, Hours = (decimal?)group.Sum(t => t.EffectiveHours) });

                Dictionary<string, decimal> hours = new Dictionary<string, decimal>();
                foreach (var mh in memberHours) {                    
                    hours.Add(mh.memberUId!=null?mh.memberUId:string.Empty, mh.Hours.HasValue?mh.Hours.Value:0);
                }

                return hours;
                
            }
        }
    }
}
