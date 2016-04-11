using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Windows.Helpers.Converters {

    enum SprintDay : int {
        NOT_IN_SPRINT,
        FIRST_SPRINT_DAY,
        IN_SPRINT,
        LAST_SPRINT_DAY
    }


    [ValueConversion(typeof(DateTime), typeof(int))]
    public class SprintCalendarDayConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (values == null || values.Length != 2)
                return (int)SprintDay.NOT_IN_SPRINT;

            if (values[0] == null || !(values[0] is Sprint))
                return (int)SprintDay.NOT_IN_SPRINT;

            if (values[1] == null || !(values[1] is DateTime))
                return (int)SprintDay.NOT_IN_SPRINT;

            Sprint sprint = (Sprint)values[0];
            DateTime day = (DateTime)values[1];

            if (day.Date < sprint.StartDate.Date)
                return (int)SprintDay.NOT_IN_SPRINT;

            if (day.Date > sprint.EndDate.Date)
                return (int)SprintDay.NOT_IN_SPRINT;

            if (sprint.StartDate.Date.Equals(day.Date))
                return (int)SprintDay.FIRST_SPRINT_DAY;

            if (sprint.EndDate.Date.Equals(day.Date))
                return (int)SprintDay.LAST_SPRINT_DAY;

            return (int)SprintDay.IN_SPRINT;

        }

        public object[] ConvertBack(object values, Type[] targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }


    }
}
