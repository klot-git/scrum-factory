﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScrumFactory.Windows.Helpers {
    /// <summary>
    /// ListViewColumnStretch
    /// </summary>
    public class ListViewColumnsExtension : DependencyObject {


        #region Dependancy Property Infrastructure

        /// <summary>
        /// IsStretched Dependancy property which can be attached to gridview columns.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.RegisterAttached("Stretch",
            typeof(bool),
            typeof(ListViewColumnsExtension),
            new UIPropertyMetadata(true, null, OnCoerceStretch));


        /// <summary>
        /// Gets the stretch.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static bool GetStretch(DependencyObject obj) {
            return (bool)obj.GetValue(StretchProperty);
        }

        /// <summary>
        /// Sets the stretch.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetStretch(DependencyObject obj, bool value) {
            obj.SetValue(StretchProperty, value);
        }

        /// <summary>
        /// Called when [coerce stretch].
        /// </summary>
        /// <remarks>If this callback seems unfamilar then please read
        /// the great blog post by Paul jackson found here. 
        /// http://compilewith.net/2007/08/wpf-dependency-properties.html</remarks>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object OnCoerceStretch(DependencyObject source, object value) {
            ListView lv = (source as ListView);

            //Ensure we dont have an invalid dependancy object of type ListView.
            if (lv == null)
                throw new ArgumentException("This property may only be used on ListViews");

            //Setup our event handlers for this list view.
            lv.Loaded += new RoutedEventHandler(lv_Loaded);
            lv.SizeChanged += new SizeChangedEventHandler(lv_SizeChanged);
            return value;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SizeChanged event of the lv control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private static void lv_SizeChanged(object sender, SizeChangedEventArgs e) {
            ListView lv = (sender as ListView);
            if (lv.IsLoaded) {
                //Set our initial widths.
                SetColumnWidths(lv);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the lv control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private static void lv_Loaded(object sender, RoutedEventArgs e) {
            ListView lv = (sender as ListView);
            //Set our initial widths.
            SetColumnWidths(lv);
        }
        #endregion

        #region Private Members

        /// <summary>
        /// Sets the column widths.
        /// </summary>
        private static void SetColumnWidths(ListView listView) {


            //Pull the stretch columns fromt the tag property.

            List<GridViewColumn> columns = (listView.Tag as List<GridViewColumn>);            
            double specifiedWidth = 0;
            GridView gridView = listView.View as GridView;
            if (gridView == null)
                return;
            
            if (columns == null) {
                //Instance if its our first run.
                columns = new List<GridViewColumn>();
                // Get all columns with no width having been set.
                foreach (GridViewColumn column in gridView.Columns) {

                    if (!(column.Width >= 0))
                        columns.Add(column);
                    else specifiedWidth += column.ActualWidth;
                }
            } else {
                // Get all columns with no width having been set.
                foreach (GridViewColumn column in gridView.Columns)
                    if (!columns.Contains(column))
                        specifiedWidth += column.ActualWidth;
            }
            
            ScrollViewer scrollViewer = FindChildControl<ScrollViewer>(listView);
            if (scrollViewer != null) {
                double scrollBarWidth = scrollViewer.ScrollableHeight > 0 ? SystemParameters.VerticalScrollBarWidth : 0;
                double newWidth = (listView.ActualWidth - scrollBarWidth - specifiedWidth) / columns.Count;

                // Allocate remaining space equally.
                foreach (GridViewColumn column in columns)
                    if (newWidth >= 10) column.Width = newWidth - 10;
            }

            //Store the columns in the TAG property for later use. 
            listView.Tag = columns;
            
        }


        static T FindChildControl<T>(DependencyObject outerDepObj) where T : DependencyObject {
            T child = null;

            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(outerDepObj); index++) {
                DependencyObject depObj = VisualTreeHelper.GetChild(outerDepObj, index);

                if ((child = depObj as T) != null) {
                    break;
                } else if (VisualTreeHelper.GetChildrenCount(depObj) > 0) {
                    child = FindChildControl<T>(depObj);
                }
            }

            return child;
        }



        #endregion

    }
}
