using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace ScrumFactory.Windows.Helpers.Extensions {

    public static class ListSelectionBehavior {

        public static readonly DependencyProperty ClickSelectionProperty =
            DependencyProperty.RegisterAttached("ClickSelection",
                                                typeof(bool),
                                                typeof(ListSelectionBehavior),
                                                new UIPropertyMetadata(false, OnClickSelectionChanged));

        public static bool GetClickSelection(DependencyObject obj) {
            return (bool)obj.GetValue(ClickSelectionProperty);
        }

        public static void SetClickSelection(DependencyObject obj, bool value) {
            obj.SetValue(ClickSelectionProperty, value);
        }

        private static void OnClickSelectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e) {
            ListBox listBox = dpo as ListBox;
            if (listBox != null) {
                if ((bool)e.NewValue == true) {
                    listBox.SelectionMode = SelectionMode.Multiple;
                    listBox.SelectionChanged += OnSelectionChanged;
                }
                else {
                    listBox.SelectionChanged -= OnSelectionChanged;
                }
            }
        }
        static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                ListBox listBox = sender as ListBox;
                var valid = e.AddedItems[0];
                foreach (var item in new ArrayList(listBox.SelectedItems)) {
                    if (item != valid) {
                        listBox.SelectedItems.Remove(item);
                    }
                }
            }
        }
    }
}
