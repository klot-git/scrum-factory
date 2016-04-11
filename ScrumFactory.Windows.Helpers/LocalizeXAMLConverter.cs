using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(string), typeof(object))]
    public class LocalizeXAMLConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            object loc = null;

            if (value == null)
                return null;

            string xamlString = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + value + "</TextBlock>";

            try {
                loc = System.Windows.Markup.XamlReader.Parse(xamlString);
            } catch (Exception) { }

            return loc;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
