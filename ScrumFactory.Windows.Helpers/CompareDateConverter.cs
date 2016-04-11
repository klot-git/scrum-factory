using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ScrumFactory.Windows.Helpers;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(DateTime), typeof(int))]
    public class CompareDateConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            DateTime? date1 = value as DateTime?;
            if (date1 == null)
                date1 = DateTime.Today;

            DateTime? date2 = parameter as DateTime?;
            if (date2 == null)
                date2 = DateTime.Today;

            if (date1 == date2)
                return 0;

            if (date1 < date2)
                return -1;

            return 1;
          
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
