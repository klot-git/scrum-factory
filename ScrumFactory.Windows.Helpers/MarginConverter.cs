using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Windows.Helpers.Converters {
    public class MarginConverter : IValueConverter {
        

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double sliderValue = (double) value;
            if(parameter==null)
                return new System.Windows.Thickness(0, 0, 0, -sliderValue);

            if(parameter.ToString().ToUpper().Equals("BOTTOM_NEG"))
                return new System.Windows.Thickness(0, 0, 0, -sliderValue);
            if (parameter.ToString().ToUpper().Equals("TOP_NEG"))
                return new System.Windows.Thickness(0, -sliderValue, 0, 0);

            return new System.Windows.Thickness(0, 0, 0, -sliderValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }


    }
}
