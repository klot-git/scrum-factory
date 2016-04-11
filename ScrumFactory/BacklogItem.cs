using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace ScrumFactory {

    

    public enum BacklogItemStatus : short {
        ITEM_REQUIRED,
        ITEM_WORKING,
        ITEM_DONE,
        ITEM_CANCELED
    }

    public enum ItemOccurrenceContraints : short {
        PLANNING_OCC,
        DEVELOPMENT_OCC,
        DELIVERY_OCC
    }

    public enum IssueTypes : short {
        NORMAL_ITEM,
        ISSUE_ITEM,
        IMPROVEMENT_ITEM,
        CRITICAL_ITEM,
    }

    [DataContract]
    [Serializable] // for clipboard reasons
    public class BacklogItem {

        private short status;

        ~BacklogItem() {
            System.Console.Out.WriteLine("---> item died here");
        }

        [DataMember]
        public string BacklogItemUId { get; set; }

        [DataMember]
        public int BacklogItemNumber { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int BusinessPriority { get; set; }
        
        [DataMember]
        public System.DateTime CreateDate { get; set; }

        [DataMember]
        public System.DateTime? DeliveryDate { get; set; }

        [DataMember]
        public short IssueType { get; set; }

        [DataMember]
        public short? CancelReason { get; set; }

        [DataMember]
        public List<PlannedHour> PlannedHours {
            get;
            set;
        }

        [DataMember]
        public short Status {
            get {
                return status;
            }
            set {
                status = value;
                if (status == (short) BacklogItemStatus.ITEM_WORKING) {
                    if(StartedAt==null)
                        StartedAt = System.DateTime.Now;
                    FinishedAt = null;
                }
                if (status == (short)BacklogItemStatus.ITEM_DONE || status == (short)BacklogItemStatus.ITEM_CANCELED) {
                    if(FinishedAt==null)
                        FinishedAt = System.DateTime.Now;                    
                }
                if (status == (short)BacklogItemStatus.ITEM_REQUIRED) {
                    StartedAt = null;
                    FinishedAt = null;
                }
            }
        }

        [DataMember]
        public System.DateTime? StartedAt { get; set; }

        [DataMember]
        public System.DateTime? FinishedAt { get; set; }

        [DataMember]
        public string ItemSizeUId { get; set; }

        [DataMember]
        public int? Size { get; set; }

        [DataMember]
        public short OccurrenceConstraint { get; set; }

        [NonSerialized]
        private Project project;

        [XmlIgnore]
        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
            }
        }

        
        [DataMember]
        public string GroupUId { get; set; }
        
        [DataMember]
        public BacklogItemGroup Group { get; set; }

        [DataMember]
        public int SizeFactor { get; set; }

        [DataMember]
        public int? ArtifactCount { get; set; }

        public bool IsFinished {
            get {
                return (FinishedAt != null);
            }
        }


        public bool IsTheSame(BacklogItem item) {
            return
                this.BacklogItemNumber == item.BacklogItemNumber &&
                this.BacklogItemUId == item.BacklogItemUId &&
                this.BusinessPriority == item.BusinessPriority &&
                this.CreateDate == item.CreateDate &&
                this.Name == item.Name &&
                this.Description == item.Description &&                
                this.FinishedAt == item.FinishedAt &&
                this.ItemSizeUId == item.ItemSizeUId &&
                this.ProjectUId == item.ProjectUId &&
                this.IssueType == item.IssueType &&
                this.Size == item.Size &&
                this.StartedAt == item.StartedAt &&
                this.Status == item.Status &&
                this.DeliveryDate == item.DeliveryDate &&
                this.CancelReason == item.CancelReason &&
                this.GroupUId == item.GroupUId &&                
                this.OccurrenceConstraint == item.OccurrenceConstraint &&
                this.ItemSizeUId == item.ItemSizeUId &&
                this.SizeFactor == item.SizeFactor &&
                this.HasTheSameHours(item);                
        }

        public bool HasTheSameHours(BacklogItem item) {
            if (item == null)
                return false;
            if (item.PlannedHours == null && PlannedHours != null)
                return false;
            if (item.PlannedHours == null && PlannedHours == null)
                return true;
            return PlannedHours.All(h => item.PlannedHours.Any(hh => hh.RoleUId == h.RoleUId && h.Hours == hh.Hours));
            
        }

        /// <summary>
        /// Gets a value indicating whether this instance can edit hours.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can edit hours; otherwise, <c>false</c>.
        /// </value>
        public bool CanEditHours {
            get {
                // now, allows user to chage hours of support projects, even those odl itens
                if (Project.ProjectType == (short) ProjectTypes.SUPPORT_PROJECT)
                    return true;
                return (SprintNumber >= Project.CurrentPlanningNumber) && !IsFinished;
            }
        }



        /// <summary>
        /// Gets the planned hours for ths item at a given planning.
        /// </summary>
        /// <remarks>If there is no hours planned at the planning number, returns the last planning.</remarks>
        /// <param name="planningNumber">The planning number.</param>
        /// <returns>The planned hours</returns>
        public List<PlannedHour> GetPlannedHoursAtPlanning(int planningNumber) {
            if (PlannedHours == null || PlannedHours.Count==0)
                return null;

            if (planningNumber < 0)
                planningNumber = 0;
            
            // this is wrong
            //if(planningNumber >= PlanningNumber)
            //    return PlannedHours.Where(p => p.PlanningNumber == PlanningNumber).ToList();

            // gets the last planning minor then planningNumber
            ICollection<PlannedHour> previousPlans = PlannedHours.Where(h => h.PlanningNumber <= planningNumber).ToArray();
            if (previousPlans == null || previousPlans.Count == 0)
                return null;
            int lastPlan  = previousPlans.Max(h => h.PlanningNumber);

            return PlannedHours.Where(p => p.PlanningNumber == lastPlan).ToList();
        }

        public List<PlannedHour> GetValidPlannedHours() {
            if (PlannedHours == null || PlannedHours.Count == 0)
                return null;

            // gets the last planning minor then planningNumber
            return PlannedHours.Where(h=>
                h.SprintNumber >= h.PlanningNumber &&
                !PlannedHours.Any(h2 => h2!=h && h2.PlanningNumber > h.PlanningNumber && h2.PlanningNumber <= h.SprintNumber)).ToList();
            
        }

        public List<PlannedHour> ValidPlannedHours {
            get;
            set;
        }

        public int? FirstSprintWorked {
            get;
            set;
        }

        public int? LastSprintWorked {
            get;
            set;
        }

        public int? OrderSprintWorked {
            get;
            set;
        }


        


   
        /// <summary>
        /// Gets the total hours for this item at a given planning.
        /// </summary>
        /// <param name="planningNumber">The planning number.</param>
        /// <returns>The sum of hours at the planning</returns>
        public decimal GetTotalHoursAtPlanning(int planningNumber) {
            List<PlannedHour> hours = GetPlannedHoursAtPlanning(planningNumber);
            if (hours == null)
                return 0;
            decimal? total = hours.Sum(p => p.Hours);
            if (total == null)
                return 0;
            return (decimal)total;            
        }




        /// <summary>
        /// Gets the current planned hours for this item.
        /// </summary>
        /// <value>The current planned hours.</value>
        public List<PlannedHour> CurrentPlannedHours {
            get {
              
                return GetPlannedHoursAtPlanning(PlanningNumber);
            }
        }


        /// <summary>
        /// Gets the total amount of hours current planned for this item.
        /// </summary>
        public decimal CurrentTotalHours {            
            get {
                if (CurrentPlannedHours == null)
                    return 0;      
                decimal? total = CurrentPlannedHours.Sum(p =>p.Hours);
                if (total == null)
                    return 0;
                return (decimal) total;                
            }
        }

      

        /// <summary>
        /// Gets the current planning number of this item.
        /// </summary>
        public int PlanningNumber {
            get {
                if (PlannedHours == null || PlannedHours.Count == 0)
                    return 0;
                return (int) PlannedHours.Max(h => h.PlanningNumber);
            }
        }


        /// <summary>
        /// Gets or sets the sprint number.
        /// </summary>
        /// <value>The sprint number.</value>
        public int? SprintNumber {
            get {
                if (CurrentPlannedHours == null || CurrentPlannedHours.Count==0)
                    return null;

                return CurrentPlannedHours[0].SprintNumber;
                
                                
                //return PlannedHours[0].SprintNumber;
            }
            set {
                if (PlannedHours == null) {
                    SyncPlannedHoursAndRoles(value);
                    return;
                }

                foreach (PlannedHour h in PlannedHours)
                    h.SprintNumber = value;
            }
        }


        public void SyncPlannedHoursAndRoles() {
            SyncPlannedHoursAndRoles(SprintNumber);
        }

        /// <summary>
        /// Sync the planned hours collection of the item with the roles of the project.
        /// </summary>        
        public void SyncPlannedHoursAndRoles(int? sprintNumber) {
            
            if (Project == null)
                throw new System.Exception("Can not change a item sprint that is not linked to a project");

            if (Project.Roles == null)
                throw new System.Exception("Can not change a item sprint that is not linked to project roles");
                        
            if (PlannedHours == null)
                PlannedHours = new List<PlannedHour>();

            int maxItemPlan = Project.CurrentPlanningNumber;
            if (PlannedHours.Count > 0)
                maxItemPlan = PlannedHours.Max(p => p.PlanningNumber);

            foreach (Role r in Project.Roles) {
                PlannedHour hour = PlannedHours.SingleOrDefault(h => h.RoleUId == r.RoleUId);                
                if (hour == null) {
                    PlannedHours.Add(new PlannedHour() {
                        Role = r,
                        RoleUId = r.RoleUId,
                        PlanningNumber = maxItemPlan,                                                
                        SprintNumber = sprintNumber,
                        BacklogItemUId = this.BacklogItemUId,
                        Hours = 0
                    });
                } else {
                    hour.Role = r;
                }
            }

            PlannedHour[] deletedHours = PlannedHours.Where(h => !Project.Roles.Any(r => r.RoleUId == h.RoleUId)).ToArray();
            for (int i = deletedHours.Length - 1; i >= 0; i--)
                PlannedHours.Remove(deletedHours[i]);
   
        }

    }

    [DataContract]
    [Serializable] // for clipboard reasons    
    public class PlannedHour {
     
        [DataMember]
        public string RoleUId { get; set; }

        [DataMember]
        public string BacklogItemUId { get; set; }

        [DataMember]
        public int PlanningNumber { get; set; }

        [DataMember]
        public int? SprintNumber { get; set; }

        [DataMember]
        public decimal? Hours { get; set; }

        public Role Role { get; set; }
    }

    [DataContract]
    public class ItemSize {

        [DataMember]
        public string ItemSizeUId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int Size { get; set; }


        [DataMember]
        public int OccurrenceConstraint { get; set; }

        [DataMember]
        public List<SizeIdealHour> SizeIdealHours { get; set; }

        public bool IsPlanningItem {
            get {
                return OccurrenceConstraint == (int)ItemOccurrenceContraints.PLANNING_OCC;
            }
        }

        public bool IsDeliveryItem {
            get {
                return OccurrenceConstraint == (int)ItemOccurrenceContraints.DELIVERY_OCC;
            }
        }

        

        public bool IsIdealHoursTheSame(ItemSize other) {
            if (other.SizeIdealHours == null && SizeIdealHours == null)
                return true;
            if (other.SizeIdealHours == null && SizeIdealHours != null)
                return false;
            if (other.SizeIdealHours != null && SizeIdealHours == null)
                return false;
            if (other.SizeIdealHours.Count != SizeIdealHours.Count)
                return false;

            var myHours = SizeIdealHours.OrderBy(i => i.RoleShortName).ToArray();
            var otherHours = other.SizeIdealHours.OrderBy(i => i.RoleShortName).ToArray();
            for (int i = 0; i < myHours.Length; i++) {
                if(myHours[i].Hours!=otherHours[i].Hours)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether this ItemSize is the same that other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// 	<c>true</c> if is the same the specified other; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTheSame(ItemSize other) {
            return other.Description == Description
                && other.Name == Name
                && other.IsActive == IsActive
                && other.Size == Size                                
                && IsIdealHoursTheSame(other)
                && other.OccurrenceConstraint == OccurrenceConstraint;
        }

    }

    [DataContract]
    public class SizeIdealHour {

        [DataMember]
        public string IdealHourUId { get; set; }
        
        [DataMember]
        public string ItemSizeUId { get; set; }

        [DataMember]
        public string RoleShortName { get; set; }

        [DataMember]
        public decimal? Hours { get; set; }
    }

   
}
