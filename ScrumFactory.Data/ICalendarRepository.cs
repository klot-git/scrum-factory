using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Data {

    public interface ICalendarRepository {

        ICollection<CalendarDay> GetHolidays(int fromYear);
    }
}
