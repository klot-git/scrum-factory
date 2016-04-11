using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;
using System.Linq;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(ICalendarService))]
    public class CalendarServiceClient : ICalendarService {

        public CalendarServiceClient() { }

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("CalendarServiceUrl")]
        private string serviceUrl { get; set; }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizator { get; set; }

        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        public ICollection<CalendarDay> Holidays { get; private set; }

        public void LoadHolidays() {
            Holidays = GetHolidays(DateTime.Today.Year - 1);
            if (Holidays == null)
                Holidays = new CalendarDay[0];
        }

        public ICollection<CalendarDay> GetHolidays(int fromYear) {
            var client = ClientHelper.GetClient(authorizator);
            HttpResponseMessage response = client.Get(Url("Holidays/?fromYear=" + fromYear));
            ClientHelper.HandleHTTPErrorCode(response, true);
            if (!response.IsSuccessStatusCode)
                return null;
            return response.Content.ReadAs<ICollection<CalendarDay>>();
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

    }
}
