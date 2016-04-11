﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(String), typeof(String))]
    public class ImageSourceConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return value;
            return String.Format(parameter.ToString(), value);
            
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
