using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory {
    public class CalendarDay {

        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public string HolidayDescription { get; set; }

        public bool IsHoliday(DateTime day) {
            return Day == day.Day && Month == day.Month && (Year == day.Year || Year == 0);
        }
    }
}
