using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface ICalendarService {

        [OperationContract]
        ICollection<CalendarDay> GetHolidays(int fromYear);

        ICollection<CalendarDay> Holidays { get; }
        void LoadHolidays();

        int CalcWorkDayCount(DateTime start, DateTime end);
        bool IsWorkDay(DateTime day);

        DateTime AddWorkDays(DateTime date, int days);
        DateTime SubWorkDays(DateTime date, int days);
    }
}
