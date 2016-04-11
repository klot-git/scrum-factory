using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ScrumFactory.Windows.Helpers {


    /// <summary>
    /// Thanks to: http://serialseb.blogspot.com/2007/01/attached-events-by-example-adding.html
    /// </summary>
    public static class ItemActivation {

        #region TheCommandToRun

        /// <summary>
        /// TheCommandToRun : The actual ICommand to run
        /// </summary>
        public static readonly DependencyProperty TheCommandToRunProperty =
            DependencyProperty.RegisterAttached("TheCommandToRun",
                typeof(ICommand),
                typeof(ItemActivation),
                new FrameworkPropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets the TheCommandToRun property.  
        /// </summary>
        public static ICommand GetTheCommandToRun(DependencyObject d) {
            return (ICommand)d.GetValue(TheCommandToRunProperty);
        }

        /// <summary>
        /// Sets the TheCommandToRun property.  
        /// </summary>
        public static void SetTheCommandToRun(DependencyObject d, ICommand value) {
            d.SetValue(TheCommandToRunProperty, value);
        }
        #endregion

        public enum ActivationMode {
            None,
            Mouse,
            Keyboard,
            Both,
            BothAndSingleClick
        }

        public static readonly RoutedEvent ItemActivateEvent =
            EventManager.RegisterRoutedEvent("ItemActivate",
                                            RoutingStrategy.Bubble,
                                            typeof(RoutedEventHandler),
                                            typeof(ItemActivation));


        public static void AddEventHandler(DependencyObject o, RoutedEventHandler handler) {
            ((UIElement)o).AddHandler(ItemActivation.ItemActivateEvent, handler);
        }

        public static void RemoveEventHandler(DependencyObject o, RoutedEventHandler handler) {
            ((UIElement)o).RemoveHandler(ItemActivation.ItemActivateEvent, handler);
        }

        public static ActivationMode GetActivationMode(DependencyObject obj) {
            return (ActivationMode)obj.GetValue(ActivationModeProperty);
        }
        public static void SetActivationMode(DependencyObject obj, ActivationMode value) {
            obj.SetValue(ActivationModeProperty, value);
        }
        public static readonly DependencyProperty ActivationModeProperty =
            DependencyProperty.RegisterAttached("ActivationMode",
                                                typeof(ActivationMode),
                                                typeof(ItemActivation),
                                                new FrameworkPropertyMetadata(ActivationMode.None,
                                                ItemActivation.HandleActivationModeChanged));


        private static MouseButtonEventHandler SelectorMouseDoubleClickHandler = new MouseButtonEventHandler(ItemActivation.HandleSelectorMouseDoubleClick);
        private static KeyEventHandler SelectorKeyDownHandler = new KeyEventHandler(ItemActivation.HandleSelectorKeyDown);

        private static void HandleActivationModeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            Selector selector = target as Selector;
            if (target == null) // if trying to attach to something else than a Selector, just ignore
                return;
            ActivationMode newActivation = (ActivationMode)e.NewValue;
            if (newActivation == ActivationMode.Mouse || newActivation == ActivationMode.Both || newActivation == ActivationMode.BothAndSingleClick) {
                selector.MouseDoubleClick += SelectorMouseDoubleClickHandler;
            }
            if (newActivation == ActivationMode.Keyboard || newActivation == ActivationMode.Both || newActivation == ActivationMode.BothAndSingleClick) {
                selector.KeyDown += SelectorKeyDownHandler;
            }
            if(newActivation == ActivationMode.None) {
                selector.KeyDown -= SelectorKeyDownHandler;
                selector.MouseDoubleClick -= SelectorMouseDoubleClickHandler;
            }

            if (newActivation == ActivationMode.BothAndSingleClick) {
                selector.MouseLeftButtonUp += SelectorMouseDoubleClickHandler;
            }
        }

        static void RaiseEvent(ItemsControl sender, DependencyObject originalSender) {
            
            if (sender == null || originalSender == null) return;

            DependencyObject container = ItemsControl.ContainerFromElement(sender, originalSender);
            // just in case, check if the double click doesn't come from somewhere else than something in a container
            if (container == null || container == DependencyProperty.UnsetValue) return;

            // found a container, now find the item.
            object activatedItem = sender.ItemContainerGenerator.ItemFromContainer(container);

            if (activatedItem != null && activatedItem != DependencyProperty.UnsetValue && !(activatedItem is System.Windows.Data.CollectionViewGroup)) {
                //sender.RaiseEvent(new ItemActivateEventArgs(ItemActivation.ItemActivateEvent, sender, activatedItem, ActivationMode.Mouse));
                ICommand command = (ICommand)sender.GetValue(ItemActivation.TheCommandToRunProperty);
                command.Execute(activatedItem);
            }
        }

        static void HandleSelectorMouseDoubleClick(object o, MouseButtonEventArgs e) {
            ItemsControl sender = o as ItemsControl;
            DependencyObject originalSender = e.OriginalSource as DependencyObject;
            RaiseEvent(sender, originalSender);
        }

        static void HandleSelectorKeyDown(object o, KeyEventArgs e) {

            if (e.Key == Key.Enter) {
                ItemsControl sender = o as ItemsControl;
                DependencyObject originalSender = e.OriginalSource as DependencyObject;
                RaiseEvent(sender, originalSender);
            }

        }
    }

    public class ItemActivateEventArgs : RoutedEventArgs {

        public object ActivatedItem { get; private set; }
        public ItemActivation.ActivationMode Mode { get; private set; }

        public ItemActivateEventArgs() : base() { }
        public ItemActivateEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        public ItemActivateEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
        public ItemActivateEventArgs(RoutedEvent routedEvent, object source, object activatedItem, ItemActivation.ActivationMode mode) : base(routedEvent, source) {
            ActivatedItem = activatedItem;
            Mode = mode;
        }
    }
}
