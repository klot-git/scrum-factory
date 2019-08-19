﻿#pragma checksum "..\..\Backlog.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6E6335823F2373BF5A0E3115206CAF37F5F1B0E8"
//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------

using ScrumFactory;
using ScrumFactory.Backlog.Properties;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Windows.Helpers.Converters;
using ScrumFactory.Windows.Helpers.DragDrop;
using ScrumFactory.Windows.Helpers.Extensions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.KExtensions;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ScrumFactory.Backlog {
    
    
    /// <summary>
    /// Backlog
    /// </summary>
    public partial class Backlog : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ScrumFactory.Backlog.Backlog thisView;
        
        #line default
        #line hidden
        
        
        #line 169 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem addButton;
        
        #line default
        #line hidden
        
        
        #line 197 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox itemStatusFilterComboBox;
        
        #line default
        #line hidden
        
        
        #line 243 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem showValuesButton;
        
        #line default
        #line hidden
        
        
        #line 276 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid newItemPanel;
        
        #line default
        #line hidden
        
        
        #line 282 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox addTextBox;
        
        #line default
        #line hidden
        
        
        #line 305 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContentPresenter groupListView;
        
        #line default
        #line hidden
        
        
        #line 364 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView listview;
        
        #line default
        #line hidden
        
        
        #line 733 "..\..\Backlog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox searchTextBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ScrumFactory.Backlog;component/backlog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Backlog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.thisView = ((ScrumFactory.Backlog.Backlog)(target));
            return;
            case 2:
            this.addButton = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 3:
            this.itemStatusFilterComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.showValuesButton = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 5:
            this.newItemPanel = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.addTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.groupListView = ((System.Windows.Controls.ContentPresenter)(target));
            return;
            case 8:
            this.listview = ((System.Windows.Controls.ListView)(target));
            return;
            case 9:
            this.searchTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

