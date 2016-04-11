using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ScrumFactory.Windows.Helpers {


    /// <summary>
    /// Panel layout for the default application panels.
    /// </summary>
    public class PanelLayout : ContentControl {

        static PanelLayout() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PanelLayout), new FrameworkPropertyMetadata(typeof(PanelLayout)));
        }

        
      
        /// <summary>
        /// Gets or sets the title's toolbar.
        /// </summary>
        /// <value>The title.</value>
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PanelLayout), new UIPropertyMetadata());

     
        /// <summary>
        /// Gets or sets the panel's toolbar.
        /// </summary>
        /// <value>The toolbar.</value>
        public object Toolbar {
            get { return (object)GetValue(ToolbarProperty); }
            set { SetValue(ToolbarProperty, value); }
        }
        public static readonly DependencyProperty ToolbarProperty = DependencyProperty.Register("Toolbar", typeof(object), typeof(PanelLayout), new UIPropertyMetadata());

        public object SubContent {
            get { return (object)GetValue(SubContentProperty); }
            set { SetValue(SubContentProperty, value); }
        }
        public static readonly DependencyProperty SubContentProperty = DependencyProperty.Register("SubContent", typeof(object), typeof(PanelLayout), new UIPropertyMetadata());

        public bool ShowCurrentProjectName {
            get { return (bool)GetValue(ShowCurrentProjectNameProperty); }
            set { SetValue(ShowCurrentProjectNameProperty, value); }
        }
        public static readonly DependencyProperty ShowCurrentProjectNameProperty = DependencyProperty.Register("ShowCurrentProjectName", typeof(bool), typeof(PanelLayout), new UIPropertyMetadata(true));
       
        

    }
}
