using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Text;

namespace ScrumFactory.Windows.Helpers.Converters {

        [ValueConversion(typeof(object), typeof(short))]
    public class EnumToShortConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {            
            return Enum.Parse(parameter as System.Type, value.ToString(), true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (short)Enum.Parse(parameter as System.Type, value.ToString(), true);            
        }
    } 

}
