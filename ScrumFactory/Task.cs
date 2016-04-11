using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;


namespace ScrumFactory {

    public enum TaskTypes : short {
        // the order is important, cuz sets its priority
        IMPROVEMENT_TASK,        
        DEVELOPMENT_TASK,
        TEST_TASK,
        IMPEDIMENT_TASK,
        BUG_TASK,        
        AFTER_DEPLOY_BUG_TASK
    }

    public enum TaskStatus : short {
        REQUIRED_TASK,
        WORKING_ON_TASK,
        DONE_TASK,
        CANCELED_TASK
    }

    public enum TaskPriorities : short {
        NORMAL_PRIORITY,
        MEDIUM_PRIORITY,
        HIGH_PRIORITY,
        URGENT_PRIORITY
    }

    [DataContract]
    [Serializable] // for clipboard reasons
    public class Task {


        [DataMember]
        public string TaskUId { get; set; }

        [DataMember]
        public int TaskNumber { get; set; }

        [DataMember]
        public string TaskName { get; set; }

        [DataMember]
        public string BacklogItemUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public short Priority { get; set; }

        [DataMember]
        public System.DateTime CreatedAt { get; set; }

        [DataMember]
        private short status;
        public short Status {
            get {
                return status;
            }
            set {
                status = value;
                AdjustDateWithStatus(System.DateTime.Now);
            }
        }

        [DataMember]
        public string TaskAssigneeUId { get; set; }

        [DataMember]
        public string TaskOwnerUId { get; set; }

        [DataMember]
        public string RoleUId { get; set; }

        [DataMember]
        public string Description { get; set; }


        [DataMember]
        public decimal PlannedHours { get; set; }

        [DataMember]
        public decimal EffectiveHours { get; set; }

        [DataMember]
        public System.DateTime? EndDate { get; set; }

        [DataMember]
        public System.DateTime? StartDate { get; set; }

        [DataMember]
        public short TaskType { get; set; }

        [DataMember]
        public bool IsAccounting { get; set; }
       
        [DataMember]
        public TaskDetail TaskDetail { get; set; }

        [DataMember]
        public int? ArtifactCount { get; set; }

        [DataMember]
        public string TagUId { get; set; }

        public string GetTaskTrackId() {
            if (TaskInfo == null)
                return null;
            return GetTaskTrackId(TaskInfo.ProjectNumber, TaskInfo.BacklogItemNumber);
        }

        public string GetTaskTrackId(int projectNumber, int backlogItemNumber) {
            return "#" + projectNumber + "." + backlogItemNumber + "." + TaskNumber + "#";
        }



        public int DaysOpened {
            get {
                return (int) System.Math.Floor(System.DateTime.Today.Subtract(CreatedAt.Date).TotalDays);
            }
        }


        public void AdjustDateWithStatus(System.DateTime now) {
            if (Status == (short)TaskStatus.DONE_TASK || Status == (short)TaskStatus.CANCELED_TASK)
                EndDate = now;

            if (Status == (short)TaskStatus.WORKING_ON_TASK) {
                StartDate = now;
                EndDate = null;
            }

            if (Status == (short)TaskStatus.REQUIRED_TASK) {
                StartDate = null;
                EndDate = null;
            }

            if (Status == (short)TaskStatus.CANCELED_TASK)
                IsAccounting = false;
            else
                IsAccounting = true;
        }

        [DataMember]
        public TaskInfo TaskInfo { get; set; }

        [NonSerialized]
        private Project project;

        public Project Project {
            get {
                return project;
            }
            set {
                project = value;
            }
        }

        public decimal HoursLeft {
            get {
                return PlannedHours - EffectiveHours;
            }
        }
       

        public bool IsTheSame(Task other) {
            if (other == null)
                return false;
            return
                   this.TaskName == other.TaskName
                && this.TaskNumber == other.TaskNumber
                && this.TaskUId == other.TaskUId
                && this.BacklogItemUId == other.BacklogItemUId
                && this.CreatedAt == other.CreatedAt
                && this.StartDate == other.StartDate
                && this.EndDate == other.EndDate
                && this.EffectiveHours == other.EffectiveHours
                && this.PlannedHours == other.PlannedHours                
                && this.TaskAssigneeUId == other.TaskAssigneeUId
                && this.RoleUId == other.RoleUId
                && this.TaskType == other.TaskType
                && this.Status == other.Status                
                && this.ProjectUId == other.ProjectUId
                && this.IsAccounting == other.IsAccounting
                && this.Priority == other.Priority
                && this.TagUId == other.TagUId
                && this.IsTheSameTaskDetail(other);
        }

        private bool IsTheSameTaskDetail(Task other) {

            if (this.TaskDetail == null) {
                if (other.TaskDetail == null)
                    return true;
                else
                    return string.IsNullOrEmpty(other.TaskDetail.Detail);
            }
            // IF this.TaskDetail is not null
            else  {
                if (other.TaskDetail == null) {
                    if (string.IsNullOrEmpty(this.TaskDetail.Detail))
                        return true;
                    else
                        return false;
                }
                else
                    return other.TaskDetail.Detail == this.TaskDetail.Detail;
            }
        }
    }


    [DataContract]
    [Serializable]  // for clipboard reasons
    public class TaskDetail {

        [DataMember]
        public string TaskUId { get; set; }

        [DataMember]
        public string Detail { get; set; }
    }



    [DataContract]
    [Serializable]  // for clipboard reasons
    public class TaskInfo {

        [DataMember]
        public string TaskUId { get; set; }

        [DataMember]
        public int ProjectNumber { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public int BacklogItemNumber { get; set; }

        [DataMember]
        public string BacklogItemName { get; set; }
    }

    [DataContract]
    public class BacklogItemEffectiveHours {

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string BacklogItemUId { get; set; }

        [DataMember]
        public int SprintNumber { get; set; }

        [DataMember]
        public decimal EffectiveHours { get; set; }
    }


    [DataContract]
    public class TodayMemberPlannedHours {

        [DataMember]
        public string TaskAssigneeUId { get; set; }

        [DataMember]
        public decimal PlannedHours { get; set; }
    }

}
