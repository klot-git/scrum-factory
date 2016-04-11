using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ScrumFactory;

namespace ScrumFactory.Windows.Helpers.Converters {

    public class TaskAssigneeCollectionConverter : IValueConverter {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            CollectionViewSource view = new CollectionViewSource();
            view.Source = value;
            view.GroupDescriptions.Add(new PropertyGroupDescription("Membership.IsActive"));
            view.View.MoveCurrentToPosition(-1); // NEED THIS OTHERWSIE DE FISRT ASSIGNEE IS SELECTED
            return view.View;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
