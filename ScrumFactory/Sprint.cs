using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ScrumFactory {

    [DataContract]
    public class Sprint {

        
        private System.DateTime endDate;

        [DataMember]
        public string SprintUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public int SprintNumber { get; set; }

        [DataMember]
        public System.DateTime StartDate { get; set; }

        [DataMember]
        public System.DateTime EndDate {
            set {
                if (value < StartDate)
                    throw new System.Exception("Sprint end date should be after sprint start.");
                endDate = value;
            }
            get {
                return endDate;
            }
        }
        
        [XmlIgnore]
        public Project Project { get; set; }

     
        public bool IsOver {
            get {
                return System.DateTime.Today > EndDate;
            }
        }

        public bool IsCurrent {
            get {
                return System.DateTime.Today >= StartDate && System.DateTime.Today <= EndDate;
            }
        }

        public int DaysLeft {
            get {
                var today = System.DateTime.Today;
                
                if (today > EndDate.Date.AddDays(1))
                    return 0;
                
                if (today < StartDate.Date)
                    today = StartDate.Date;

                int d = (int) EndDate.Subtract(today).TotalDays + 1;
                if (d < 0)
                    d = 0;
                return d;
            }
        }

        public int TotalDays {
            get {
                return (int) EndDate.Date.Subtract(StartDate.Date).TotalDays;
            }
        }

        public System.DateTime[] Days {
            get {
                List<System.DateTime> days = new List<System.DateTime>();
                if (EndDate < StartDate)
                    return days.ToArray();
                var day = this.StartDate;

                if (EndDate.Subtract(day).TotalDays > 30)
                    day = EndDate.AddDays(-30);

                while (day <= EndDate) {
                    days.Add(new System.DateTime(day.Year, day.Month, day.Day));
                    day = day.AddDays(1);
                }
                return days.ToArray();
            }
        }

        
    }
}
