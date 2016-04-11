using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(Double), typeof(Double))]
    public class HalfPointConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            double adjust = 0;

            if (value == null)
                return 0;

            if (parameter is double)
                adjust = (double)parameter;

            double dbValue = (double)value;

            dbValue = dbValue - adjust;
            dbValue = dbValue / 2;

            return new System.Windows.Point(dbValue, dbValue);
            
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
