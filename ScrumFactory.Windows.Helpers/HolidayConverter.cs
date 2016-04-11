using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Windows.Helpers.Converters {




    [ValueConversion(typeof(DateTime), typeof(String))]
    public class HolidayConverter : IValueConverter {

        public static ICollection<CalendarDay> Holidays { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (Holidays == null)
                return null;

            DateTime day = (DateTime)value;

            CalendarDay holiday = Holidays.SingleOrDefault(h => h.IsHoliday(day));

            if (holiday == null)
                return null;
            return holiday.HolidayDescription;

        }

        public object ConvertBack(object values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }


    }
}
