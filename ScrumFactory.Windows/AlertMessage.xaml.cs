using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows {
    /// <summary>
    /// Interaction logic for AlertMessage.xaml
    /// </summary>
    public partial class AlertMessage : UserControl, IDialogView {

   
   
        public static readonly RoutedEvent TurnedVisibleEvent = EventManager.RegisterRoutedEvent("TurnedVisible", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AlertMessage));
        public event RoutedEventHandler TurnedVisible {
            add { AddHandler(TurnedVisibleEvent, value); }
            remove { RemoveHandler(TurnedVisibleEvent, value); }
        }

        public AlertMessage() {
            InitializeComponent();
            DataContext = null;
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(AlertMessage_DataContextChanged);
            this.Visibility = Visibility.Collapsed;            
        }

        void AlertMessage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            // very ugly, CHANGE THIS
            ((ScrumFactory.Composition.ViewModel.IDialogViewModel)DataContext).View = (ScrumFactory.Composition.View.IDialogView)this;
        }

        public object Model {
            get {
                return DataContext;
            }
            set {                
                DataContext = value;                
                // very ugly, CHANGE THIS
                ((ScrumFactory.Composition.ViewModel.IDialogViewModel)value).View = (ScrumFactory.Composition.View.IDialogView)this;
            }
        }

        public void Show() {            
            closeButton.Focus();
            this.border.Margin = new Thickness(0, 0, 0, -this.ActualHeight);
            this.Visibility = Visibility.Visible;
            this.UpdateLayout();
            RoutedEventArgs args = new RoutedEventArgs(TurnedVisibleEvent);
            RaiseEvent(args);

        }

        public void Close() {            
            this.border.Margin = new Thickness(0, 0, 0, -this.ActualHeight);
            this.Visibility = Visibility.Collapsed;            
        }

        public void DragMove() {
            throw new NotSupportedException();
        }

        public WindowState WindowState { get; set; }

    }
}
