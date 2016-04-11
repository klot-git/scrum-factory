using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ScrumFactory.Windows.Helpers.Extensions {
    public static class GestureExtension {

        public static bool GetHandleManipulationBoundary(DependencyObject obj) {
            return (bool)obj.GetValue(HandleManipulationBoundaryProperty);
        }


        public static void SetHandleManipulationBoundary(DependencyObject obj, bool value) {
            obj.SetValue(HandleManipulationBoundaryProperty, value);
        }


        public static readonly DependencyProperty HandleManipulationBoundaryProperty =
                DependencyProperty.RegisterAttached("HandleManipulationBoundary", typeof(bool), typeof(GestureExtension), new UIPropertyMetadata(false, OnHandleManipulationBoundaryChanged));

        private static void OnHandleManipulationBoundaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var view = (UIElement)d;
            view.ManipulationBoundaryFeedback += new EventHandler<System.Windows.Input.ManipulationBoundaryFeedbackEventArgs>(view_ManipulationBoundaryFeedback);
        }

        static void view_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e) {
            e.Handled = GetHandleManipulationBoundary(sender as UIElement);
            
        }

    }
}
