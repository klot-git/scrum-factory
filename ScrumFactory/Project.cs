using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;


namespace ScrumFactory {

    public enum ProjectStatus : short {        
        PROPOSAL_CREATION,        
        PROPOSAL_APPROVED,
        PROPOSAL_REJECTED,
        PROJECT_STARTED,
        PROJECT_DONE,   
        PROJECT_SUPPORT             
    }

    public enum ProjectTypes : short {
        NORMAL_PROJECT = 0,
        SUPPORT_PROJECT = 20,
        TICKET_PROJECT = 30
    }

    /// <summary>
    /// POCO object for the project.
    /// </summary>
    [DataContract]
    public class Project {

    
        /// <summary>
        /// Gets or sets the project Unique id.
        /// </summary>
        /// <value>The project Unique id.</value>
        [DataMember]
        public string ProjectUId { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the project number.
        /// </summary>
        /// <value>The project number.</value>
        [DataMember]
        public int ProjectNumber { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public short Status { get; set; }

        [DataMember]
        public System.DateTime CreateDate { get; set; }

        [DataMember]
        public String CreateBy { get; set; }

        [DataMember]
        public String Platform { get; set; }


        [DataMember]
        public DateTime? StartDate { get; set; }


        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public string CodeRepositoryPath { get; set; }

        [DataMember]
        public string DocRepositoryPath { get; set; }

        [DataMember]
        public List<Role> Roles { get; set; }

        [DataMember]
        public bool AnyoneCanJoin { get; set; }

        [DataMember]
        public short ProjectType { get; set; }

        [DataMember]
        public List<Sprint> Sprints { get; set; }

        [DataMember]
        public int? Baseline { get; set; }

        // one day i will implement it
        private ProjectOptions projectOptions;
        public ProjectOptions ProjectOptions {
            get {
                if (projectOptions == null)
                    projectOptions = new ProjectOptions();
                return projectOptions;
            }
            set {
                projectOptions = value;
            }
        }

        [DataMember]
        public bool IsSuspended { get; set; }

        [DataMember]
        public List<ProjectMembership> Memberships { get; set; }


        public int TotalDayAllocation {
            get {
                if (Memberships == null)
                    return 0;
                int? al = Memberships.Sum(m => m.DayAllocation);
                if (al != null)
                    return al.Value;
                return 0;

            }
        }
                
        /// <summary>
        /// Gets the current sprint.
        /// If there is no current sprint, returns nulls.
        /// </summary>
        public Sprint CurrentSprint {
            get {
                if (Sprints == null || Sprints.Count == 0)
                    return null;

                if (!IsRunning)
                    return null;
                
                return SprintForDate(DateTime.Today);                
            }
        }

        public Sprint SprintForDate(DateTime date) {
            Sprint sprint = null;

            // order them by number
            var sprintArray = Sprints.OrderBy(i => i.SprintNumber).ToArray();

            for (int i = 0; i < sprintArray.Length; i++) {
                                
                var start = sprintArray[i].StartDate.Date;
                var end = sprintArray[i].EndDate.Date;

                // if there is a sprint after that, uses the next sprint startDate as end
                if (i + 1 < sprintArray.Length)
                    end = sprintArray[i + 1].StartDate.Date.AddDays(-1);

                if (date >= start && date <= end) {
                    sprint = sprintArray[i];
                    break;
                }
            }
                
            return sprint;         
        }

        public bool IsRunning {
            get {
                return (Status == (short)ProjectStatus.PROJECT_STARTED || Status == (short)ProjectStatus.PROJECT_SUPPORT);
            }
        }

        /// <summary>
        /// Gets a current valid Sprint.
        /// If the first sprint has not yet started return the first.
        /// If the last sprint is over, return the last.
        /// </summary>
        public Sprint CurrentValidSprint {
            get {

                if (Sprints == null || Sprints.Count == 0)
                    return null;

                if (CurrentSprint != null)     
                    return CurrentSprint;

                if (Status == (short)ProjectStatus.PROJECT_DONE)
                    return LastSprint;

                // if over, return last
                if (LastSprint.EndDate < System.DateTime.Today)
                    return LastSprint;

                // not stared yet, return first
                if (FirstSprint.StartDate.Date > System.DateTime.Today)
                    return FirstSprint;

                
                return FirstSprint;

            }
        }

        public bool HasPermission(string memberUId, PermissionSets permission) {
            if (Memberships == null)
                return false;
            return Memberships.Any(m => m.MemberUId == memberUId && m.IsActive==true && m.Role.PermissionSet == (short)permission);
        }
        

        /// <summary>
        /// Gets the current planning number.
        /// If the project has not started yet, returns zero (=proposal plan).        
        /// </summary>
        public int CurrentPlanningNumber {
            get {
                if (!IsRunning)
                    return 0;
                return CurrentValidSprint.SprintNumber;
            }
        }

        /// <summary>
        /// Gets the next sprint number.
        /// </summary>
        public int NextSprintNumber {
            get {
                if (Sprints == null)
                    throw new System.Exception("Project Sprints has not been loaded yet");
                return Sprints.Count + 1;
            }
        }
        
        public Sprint FirstSprint {
            get {
                if (Sprints == null || Sprints.Count == 0)
                    return null;
                return Sprints.SingleOrDefault(s => s.SprintNumber == 1);
            }
        }

        public Sprint LastSprint {
            get {
                if (Sprints == null || Sprints.Count == 0)
                    return null;

                return Sprints.SingleOrDefault(s => s.SprintNumber == Sprints.Max(s1 => s1.SprintNumber));
            }
        }

        public Role DefaultRole {
            get {
                if (Roles == null)
                    return null;
                return Roles.FirstOrDefault(r => r.IsDefaultRole == true);
            }
        }
       
        public bool IsTheSame(Project p) {
            if (p == null)
                return false;
            return 
                this.ProjectName == p.ProjectName &&
                this.ProjectNumber == p.ProjectNumber &&
                this.ClientName == p.ClientName &&
                this.CodeRepositoryPath == p.CodeRepositoryPath &&
                this.DocRepositoryPath == p.DocRepositoryPath &&
                this.Description == p.Description &&
                this.ProjectType == p.ProjectType &&
                this.AnyoneCanJoin == p.AnyoneCanJoin &&
                this.IsSuspended == p.IsSuspended;
        }


        ~Project() {
            System.Console.Out.WriteLine("---> project died here");
        }

        public bool IsTicketProject {
            get {
                return ProjectType == (short)ProjectTypes.TICKET_PROJECT;
            }
        }

        public override string ToString() {
            return ClientName + "\t" + ProjectName + " (" + ProjectNumber + ")\t" + Enum.GetName(typeof(ProjectStatus), Status) + "\t" + (StartDate.HasValue? StartDate.Value.Date.ToShortDateString(): "") + "\t" + (EndDate.HasValue?EndDate.Value.Date.ToShortDateString():"");
        }

        public string ToHTMLString(string serverUrl) {
            return "<td>" + ClientName + "</td><td>" + ProjectName + " (<a href=\"" + serverUrl + "/" + ProjectNumber + "\">" + ProjectNumber + "</a>)</td><td>" + Enum.GetName(typeof(ProjectStatus), Status) + "</td><td>" + (StartDate.HasValue ? StartDate.Value.Date.ToShortDateString() : "") + "</td><td>" + (EndDate.HasValue ? EndDate.Value.Date.ToShortDateString() : "" + "</td>");
        }

        public DateTime? EstimatedEndDate {
            get {
                if (LastSprint == null)
                {
                    return null;
                }
                return LastSprint.EndDate;
            }
        }

        public void FixRecursiveRelation()
        {
            foreach (var m in Memberships)
            {
                m.Project = null;
            }
        }


    }

    


    public class ProjectOptions {

        public bool AddPlanAndDeliveryItemsToSprint { get; set; }
        public bool AutoAddSprints { get; set; }
        public int SprintLimitHours { get; set; }
        public int SprintLimitDays { get; set; }

        public ProjectOptions() {
            AddPlanAndDeliveryItemsToSprint = true;
            AutoAddSprints = false;
            SprintLimitHours = 160;
            SprintLimitDays = 15;

        }

    }

}
