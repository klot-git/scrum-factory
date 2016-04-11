using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ScrumFactory.Windows.Helpers.Extensions
{
    public static class TextBoxExtension {

        #region IsFocused
        public static bool GetIsFocused(DependencyObject obj) {
            return (bool)obj.GetValue(IsFocusedProperty);
        }


        public static void SetIsFocused(DependencyObject obj, bool value) {
            obj.SetValue(IsFocusedProperty, value);
        }


        public static readonly DependencyProperty IsFocusedProperty =
                DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(TextBoxExtension), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));


        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            
            var uie = (FrameworkElement)d;
            if ((bool)e.NewValue) {
                
                System.Console.Out.WriteLine("****FOCUS*****");
                uie.Loaded += new RoutedEventHandler(uie_Loaded);                
                bool ok = uie.Focus(); // Don't care about false values.                      
            }
        }

        static void uie_Loaded(object sender, RoutedEventArgs e) {
            var uie = (FrameworkElement)sender;
            uie.Focus();
        }

        #endregion

        #region DisableOnLostFocus
        public static bool GetDisableOnLostFocus(DependencyObject obj) {
            return (bool)obj.GetValue(DisableOnLostFocusProperty);
        }


        public static void SetDisableOnLostFocus(DependencyObject obj, bool value) {
            obj.SetValue(DisableOnLostFocusProperty, value);
        }

        public static readonly DependencyProperty DisableOnLostFocusProperty =
                DependencyProperty.RegisterAttached("DisableOnLostFocus", typeof(bool), typeof(TextBoxExtension), new UIPropertyMetadata(false, OnDisableOnLostFocusPropertyChanged));

        private static void OnDisableOnLostFocusPropertyChanged(DependencyObject d,DependencyPropertyChangedEventArgs e) {
            var uie = (UIElement)d;
            uie.LostFocus -= new RoutedEventHandler(uie_LostFocus);

            if ((bool)e.NewValue)                 
                uie.LostFocus += new RoutedEventHandler(uie_LostFocus);
            
        }

        static void uie_LostFocus(object sender, RoutedEventArgs e) {
            var uie = (UIElement)sender;
            uie.IsEnabled = false;
        }
        #endregion

        #region AutoSelectOnFocus
        public static bool GetAutoSelectOnFocus(DependencyObject obj) {
            return (bool)obj.GetValue(AutoSelectOnFocusProperty);
        }


        public static void SetAutoSelectOnFocus(DependencyObject obj, bool value) {
            obj.SetValue(AutoSelectOnFocusProperty, value);
        }

        public static readonly DependencyProperty AutoSelectOnFocusProperty =
                DependencyProperty.RegisterAttached("AutoSelectOnFocus", typeof(bool), typeof(TextBoxExtension), new UIPropertyMetadata(false, OnAutoSelectOnFocusPropertyChanged));

        private static void OnAutoSelectOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uie = (UIElement)d;
            
            uie.GotFocus -= new RoutedEventHandler(uie_GotFocus);
            uie.PreviewMouseLeftButtonDown -= new System.Windows.Input.MouseButtonEventHandler(uie_PreviewMouseLeftButtonDown);

            if ((bool)e.NewValue) {
                uie.GotFocus += new RoutedEventHandler(uie_GotFocus);
                uie.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(uie_PreviewMouseLeftButtonDown);
            }
            
            
        }

        static void uie_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null)
                return;

            if (!textBox.IsFocused) {
                e.Handled = true;
                textBox.Focus();
            }
        }

        static void uie_GotFocus(object sender, RoutedEventArgs e) {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null)
                return;
            textBox.Select(0, textBox.Text.Length);
            
        }

        
        #endregion

        #region NumericJumper
        public static double GetNumericJumper(DependencyObject obj) {
            return (double)obj.GetValue(NumericJumperProperty);
        }


        public static void SetNumericJumper(DependencyObject obj, double value) {
            obj.SetValue(NumericJumperProperty, value);
        }

        public static readonly DependencyProperty NumericJumperProperty =
                DependencyProperty.RegisterAttached("NumericJumper", typeof(double), typeof(TextBoxExtension), new UIPropertyMetadata(0.0, NumericJumperPropertyChanged));

        private static void NumericJumperPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uie = (UIElement)d;

            uie.PreviewKeyDown -= new System.Windows.Input.KeyEventHandler(uie_PreviewKeyDown);
            
            if ((double)e.NewValue>0) {
                uie.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(uie_PreviewKeyDown);
            }


        }

        static void uie_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {

            if (e.Key != System.Windows.Input.Key.Up && e.Key != System.Windows.Input.Key.Down)
                return;

            System.Windows.Controls.TextBox t = sender as System.Windows.Controls.TextBox;
            if (t == null)
                return;
            double value;
            if (!Double.TryParse(t.Text, out value))
                return;

            double jumper = (double) t.GetValue(TextBoxExtension.NumericJumperProperty);

            if (e.Key == System.Windows.Input.Key.Up)
                value = value + jumper;
            if (e.Key == System.Windows.Input.Key.Down)
                value = value - jumper;

            t.Text = value.ToString();
            return;
        }

        #endregion

        #region BindOnLostFocus

        public static bool GetBindOnLostKeyboardFocus(DependencyObject obj) {
            return (bool)obj.GetValue(BindOnLostKeyboardFocusProperty);
        }


        public static void SetBindOnLostKeyboardFocus(DependencyObject obj, bool value) {
            obj.SetValue(BindOnLostKeyboardFocusProperty, value);
        }

        public static readonly DependencyProperty BindOnLostKeyboardFocusProperty =
            DependencyProperty.RegisterAttached("BindOnLostKeyboardFocus", typeof(bool), typeof(TextBoxExtension), new PropertyMetadata(default(bool), BindOnLostKeyboardFocusChanged));

        private static void BindOnLostKeyboardFocusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var control = o as UIElement;
            if (control != null) {
                if ((bool)e.NewValue) {
                    control.AddHandler(UIElement.LostKeyboardFocusEvent, new RoutedEventHandler(ControlLostKeyboardFocus));
                } else {
                    control.RemoveHandler(UIElement.LostKeyboardFocusEvent, new RoutedEventHandler(ControlLostKeyboardFocus));
                }
            }
        }

        private static void ControlLostKeyboardFocus(object sender, RoutedEventArgs e) {
            var control = (UIElement)sender;
            control.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));
        }

        #endregion


        public static string GetHtmlCopyContent(DependencyObject obj) {
            return (string)obj.GetValue(HtmlCopyContentProperty);
        }


        public static void SetHtmlCopyContent(DependencyObject obj, string value) {
            obj.SetValue(HtmlCopyContentProperty, value);
        }

        public static readonly DependencyProperty HtmlCopyContentProperty =
            DependencyProperty.RegisterAttached("HtmlCopyContent", typeof(string), typeof(TextBoxExtension), new PropertyMetadata(String.Empty, HtmlCopyContentChanged));

        private static void HtmlCopyContentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            DataObject.RemoveCopyingHandler(o, OnCopy);
            DataObject.AddCopyingHandler(o, OnCopy);            
        }

        private static void OnCopy(object sender, DataObjectCopyingEventArgs e) {
            
            var control = sender as System.Windows.Controls.TextBox;
            if (control == null)
                return;

            string html = GetHtmlCopyContent(control);
            html = HTMLClipboardHelper.GetHtmlDataString(html);

            e.DataObject.SetData(DataFormats.UnicodeText, control.Text.ToString(), true);
            e.DataObject.SetData(DataFormats.Text, control.Text.ToString(), true);
            e.DataObject.SetData(DataFormats.OemText, control.Text.ToString(), true);
            e.DataObject.SetData(DataFormats.Html, html, true);

            Clipboard.SetDataObject(e.DataObject, true);

            
                        
            e.Handled = true;
            e.CancelCommand();

        }


        
        
    } 

}
