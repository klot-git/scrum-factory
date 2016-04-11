using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(Double), typeof(Double))]
    public class HalfValueConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            double adjust = 0;

            if (value == null)
                return 0;

            if(parameter!=null)
                double.TryParse(parameter.ToString(), out adjust);

            double dbValue = (double)value;

            dbValue = dbValue - adjust;

            return (double) (dbValue / 2);
            
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
