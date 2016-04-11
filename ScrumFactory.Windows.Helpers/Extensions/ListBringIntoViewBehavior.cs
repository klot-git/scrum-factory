using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Specialized;

namespace ScrumFactory.Windows.Helpers.Extensions {
    /// <summary>
    /// ListBoxItem Behavior class
    /// </summary>
    public static class ListBringIntoViewBehavior {

        #region IsBroughtIntoViewWhenSelected

        /// <summary>
        /// Gets the IsBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listBoxItem"></param>
        /// <returns></returns>
        public static bool GetBringIntoViewNewItems(ListBoxItem listBoxItem) {
            return (bool)listBoxItem.GetValue(BringIntoViewNewItemsProperty);
        }

        /// <summary>
        /// Sets the IsBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listBoxItem"></param>
        /// <param name="value"></param>
        public static void SetBringIntoViewNewItems(ListBoxItem listBoxItem, bool value) {
            listBoxItem.SetValue(BringIntoViewNewItemsProperty, value);
        }

        /// <summary>
        /// Determins if the ListBoxItem is bought into view when enabled
        /// </summary>
        public static readonly DependencyProperty BringIntoViewNewItemsProperty =
            DependencyProperty.RegisterAttached(
            "BringIntoViewNewItems",
            typeof(bool),
            typeof(ListBringIntoViewBehavior),
            new UIPropertyMetadata(false, OnBringIntoViewNewItemsChanged));

        /// <summary>
        /// Action to take when item is brought into view
        /// </summary>
        /// <param name="depObj"></param>
        /// <param name="e"></param>
        static void OnBringIntoViewNewItemsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {

            ListBoxItem listBoxItem = depObj as ListBoxItem;
            if (listBoxItem == null)
                return;

            if (e.NewValue is bool == false)
                return;

            var model = listBoxItem.DataContext as ScrumFactory.Composition.ViewModel.IEditableObjectViewModel;
            if (model == null)
                return;
            if(model.JustHasBeenInserted)
                listBoxItem.BringIntoView();
        }


        #endregion // IsBroughtIntoViewWhenSelected
    }
}
