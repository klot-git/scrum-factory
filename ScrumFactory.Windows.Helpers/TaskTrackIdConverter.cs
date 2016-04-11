using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;


namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(Task), typeof(string))]
    public class TaskTrackIdConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Task task = value as Task;
            if (task == null)
                return String.Empty;

            return task.TaskInfo.ProjectNumber + "." + task.TaskInfo.BacklogItemNumber + "." + task.TaskNumber;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
