using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(String), typeof(String))]
    public class LocalizeConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            System.Reflection.Assembly assembly;
            if (parameter != null)
                assembly = System.Reflection.Assembly.GetAssembly(Type.GetType(parameter.ToString()));
            else
                assembly = System.Reflection.Assembly.GetEntryAssembly();
            System.Resources.ResourceManager r = new System.Resources.ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);

            if (r == null)
                return value;

            if (value == null)
                return null;

            object loc = r.GetObject(value.ToString());
            if (loc == null)
                return value.ToString();
            return loc.ToString();
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
