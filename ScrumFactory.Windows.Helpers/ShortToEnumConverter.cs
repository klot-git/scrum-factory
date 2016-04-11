using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Text;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(short), typeof(string))]
    public class ShortToEnumConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return null;
            return Enum.GetName(parameter as System.Type, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return null;
            return (short)Enum.Parse(parameter as System.Type, value.ToString(), true);            
        }
    } 

}
