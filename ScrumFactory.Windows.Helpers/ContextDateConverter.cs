using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ScrumFactory.Windows.Helpers;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class ContextDateConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return "?";
            DateTime? date = (DateTime?)value;
            if (date == null)
                return "?";

            bool showCountDays = false;
            bool.TryParse(parameter as string, out showCountDays);
            

            if (date.Value.Date == System.DateTime.Today)
                return Properties.Resources.today;

            int days = System.DateTime.Today.Subtract(date.Value.Date).Days;

            if (days==1)
                return Properties.Resources.yesterday;

            if (days <= 0 && days >= -1)
                return Properties.Resources.tomorrow;

            if (days <=0 && days >=-7)
                return String.Format(Properties.Resources.in_N_days, -days);

            if (showCountDays)
                return String.Format(Properties.Resources.in_N_days, Math.Abs(days));
                
            return String.Format("{0:dd MMM}", date.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
