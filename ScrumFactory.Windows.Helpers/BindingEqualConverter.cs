using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;


namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(int), typeof(int))]
    public class BindingEqualConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (values.Length < 1)
                return -2;

            int? a = values[0] as int?;
            int? b = values[1] as int?;

            if (!a.HasValue && !b.HasValue)
                return 0;

            if (!a.HasValue && b.HasValue)
                return -1;
            if (a.HasValue && !b.HasValue)
                return 1;

            if (a < b)
                return -1;                        
            if (a > b)
                return 1;
            

            return 0;


        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
