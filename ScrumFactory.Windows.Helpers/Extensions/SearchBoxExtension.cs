using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ScrumFactory.Windows.Helpers.Extensions {
    public static class SearchBoxExtension {

        #region SearchBox
        public static System.Windows.Controls.TextBox GetSearchBox(DependencyObject obj) {
            return (System.Windows.Controls.TextBox)obj.GetValue(SearchBoxProperty);
        }


        public static void SetSearchBox(DependencyObject obj, System.Windows.Controls.TextBox value) {
            obj.SetValue(SearchBoxProperty, value);
        }


        public static readonly DependencyProperty SearchBoxProperty =
                DependencyProperty.RegisterAttached("SearchBox", typeof(UIElement), typeof(SearchBoxExtension), new UIPropertyMetadata(null, OnSearchBoxPropertyChanged));
        
        private static void OnSearchBoxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            
            var view = (FrameworkElement)d;

       

            view.Focusable = true;

            //var searchBox = GetSearchBox(view);
            //System.Windows.Input.FocusManager.SetIsFocusScope(view, true);
            //System.Windows.Input.FocusManager.SetFocusedElement(view, searchBox);
            

            view.MouseUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseUp);

            
            view.KeyUp += new System.Windows.Input.KeyEventHandler(view_KeyUp);

            view.TextInput += new System.Windows.Input.TextCompositionEventHandler(view_TextInput);

            view.KeyDown += new System.Windows.Input.KeyEventHandler(view_KeyDown);

            view.Loaded += new RoutedEventHandler(view_Loaded);

        }

        static void view_Loaded(object sender, RoutedEventArgs e) {
            bool a = ((FrameworkElement)sender).Focus();
        }


        static void view_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            //if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl)) {
            //    UIElement panel = sender as UIElement;
            //    if (panel == null)
            //        return;

            //    var searchBox = GetSearchBox(panel);                
            //    if (!searchBox.IsFocused)
            //        return;

            //    panel.Focus();
            //}
        }
        
        static void view_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {

            //if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            //    return;    

            var searchBox = GetSearchBox(sender as UIElement);
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
            if (focusedElement == searchBox)
                return;

            string text = e.Text;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^[0-9A-Za-z ]+$");
            if (String.IsNullOrEmpty(text) || !regex.IsMatch(text)) {
                return;
            }

            searchBox.Opacity = 1;

            searchBox.Text = searchBox.Text + text;
            searchBox.CaretIndex = searchBox.Text.Length;            
            searchBox.Focus();
            
        }

        static void view_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {

            UIElement panel = sender as UIElement;
            if (panel==null)
               return;
            
            var searchBox = GetSearchBox(panel);
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;

            // se não está no campo de busca e foi um CTRl-V, sai daqui para dexiar o Pastle ocorrer
            //if (searchBox.Opacity != 1 && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            //    return;
            
            if (e.Key == System.Windows.Input.Key.F3 && focusedElement != searchBox) {
                searchBox.Opacity = 1;
                searchBox.Focus();
                return;
            }

            if (searchBox.Text.Length ==0 || e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Tab) {
                if(searchBox.Opacity!=0)
                    searchBox.Opacity = 0;
                if(focusedElement==searchBox)
                    panel.Focus();
                return;
            }

            if (e.Key == System.Windows.Input.Key.Escape) {
                searchBox.Opacity = 0;
                searchBox.Text = String.Empty;
                panel.Focus();
                return;
            }

            if (searchBox.Text.Length > 0)
                searchBox.Opacity = 1;
        
        }

        static void view_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            UIElement panel = sender as UIElement;

            // if the mouse click source can get focus, get out of here
            // UGLEY CODE HERE
            UIElement source = e.OriginalSource as UIElement;
            if (e.OriginalSource.GetType().Name == "TextBoxView")
                return;
            

            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;            
            var s = focusedElement as DependencyObject;
            if (s != null) {
                bool? autoFocus = s.GetValue(TextBoxExtension.AutoSelectOnFocusProperty) as bool?;
                if (autoFocus.HasValue && autoFocus.Value)
                    return;
            }
            if (panel == null)
                return;

            panel.Focus();    
            
        
        }


        #endregion

      

    } 

}
