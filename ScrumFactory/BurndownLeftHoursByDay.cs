using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory {

    public enum LeftHoursMetrics {
        PLANNING,        
        LEFT_HOURS,
        LEFT_HOURS_AHEAD
    }

    public class BurndownLeftHoursByDay {

     

        public System.DateTime Date { get; set; }
        public decimal TotalHours { get; set; }

        public int SprintNumber { get; set; }

        public bool IsToday {
            get {
                return Date.Date.Equals(DateTime.Today);
            }
        }

        public bool IsLastSprint { get; set; }

        public LeftHoursMetrics LeftHoursMetric { get; set; }
    }
}
