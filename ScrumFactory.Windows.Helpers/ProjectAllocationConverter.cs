using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(Project), typeof(int))]
    public class ProjectAllocationConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            Project project = value as Project;
            if (project == null || project.Memberships == null || project.Memberships.Count == 0)
                return 0;

            return project.Memberships.Where(m => m.IsActive).Sum(m => m.DayAllocation);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }


    }
}
