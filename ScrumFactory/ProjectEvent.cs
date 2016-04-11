using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ScrumFactory {

    public enum ProjectEventTypes : short {
        PROJECT_START,
        SPRINT_END,
        PROJECT_END
    }

    [DataContract]
    public class ProjectEvent {

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public int ProjectNumber { get; set; }

        [DataMember]
        public short EventType { get; set; }

        [DataMember]
        public int SprintNumber { get; set; }

        [DataMember]
        public System.DateTime When { get; set; }


        public int DaysLeft {
            get {
                return (int)Math.Floor(When.Date.Subtract(System.DateTime.Today).TotalDays);
            }
        }
    }
}
