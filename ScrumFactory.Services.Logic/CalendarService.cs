using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;

namespace ScrumFactory.Services.Logic {


    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(ICalendarService))]
    public class CalendarService : ICalendarService {

        [Import]
        private Data.ICalendarRepository calendarRepository { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        [WebGet(UriTemplate = "Holidays/?fromYear={fromYear}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<CalendarDay> GetHolidays(int fromYear) {
            authorizationService.VerifyRequestAuthorizationToken();
            return calendarRepository.GetHolidays(fromYear);
        }


        public ICollection<CalendarDay> Holidays {
            get;
            private set;
        }

        public void LoadHolidays() {
            Holidays = GetHolidays(DateTime.Today.Year-1);
            if (Holidays == null)
                Holidays = new CalendarDay[0];
        }

        /// <summary>
        /// Define the number of days ripping of the saturdays and sundays.
        /// </summary>
        /// <returns>The number of work days</returns>
        public int CalcWorkDayCount(DateTime start, DateTime end) {
            DateTime day = start.Date;
            int dayCount = 0;
            while (day <= end.Date) {
                if (IsWorkDay(day))
                    dayCount++;
                day = day.AddDays(1);
            }
            return dayCount;

        }

        /// <summary>
        /// Normal human beings do not work on weekends.
        /// Verify if the day is a work day.
        /// </summary>
        /// <param name="day"></param>
        /// <returns>True if the day is not a weekend</returns>
        public bool IsWorkDay(DateTime day) {
            if (Holidays == null)
                LoadHolidays();
            return (!DayOfWeek.Saturday.Equals(day.DayOfWeek) &&
                    !DayOfWeek.Sunday.Equals(day.DayOfWeek) &&
                    !Holidays.Any(h => h.IsHoliday(day)));
        }

        public DateTime AddWorkDays(DateTime date, int days) {
            int added = 0;
            DateTime day = date;

            while (added < days) {
                day = day.AddDays(1);
                if (IsWorkDay(day))
                    added++;
                

            }
            return day;
        }

        public DateTime SubWorkDays(DateTime date, int days) {
            int added = 0;
            DateTime day = date;

            while (added < days) {
                day = day.AddDays(-1);
                if (IsWorkDay(day))
                    added++;
            }

            return day;
        }
    }
}
