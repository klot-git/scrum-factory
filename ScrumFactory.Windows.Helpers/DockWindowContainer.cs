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
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Collections.Specialized;



namespace ScrumFactory.Windows.Helpers {

   public enum WindowActions {
       OPEN,
       CLOSE
   };

    /// <summary>
    /// Interaction logic for DockWindowContainer.xaml
    /// </summary>
    public class DockWindowContainer : ContentControl {

     
        public static readonly DependencyProperty ChildWindowsProperty =
            DependencyProperty.Register("ChildWindows", typeof(ObservableCollection<object>), typeof(DockWindowContainer), new FrameworkPropertyMetadata(null, OnChildWindowsChanged));
        
        #region Constructor

        static DockWindowContainer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockWindowContainer), new FrameworkPropertyMetadata(typeof(DockWindowContainer)));  
          
        }


        public DockWindowContainer() {
            ScrollDuration = 250;
        }

        #endregion

        #region Fields
        
        private Stack<FrameworkElement> childWindows = new Stack<FrameworkElement>();        
        private bool mediatorRegistered = false;

        private FrameworkElement removingWindow = null;
        private FrameworkElement newWindow = null;

        private Stack<WindowAction> actionsStack = new Stack<WindowAction>();         

        #endregion

        #region Properties

        /// <summary>
        /// Gets and Sets the Child Dock Windows.
        /// </summary>
        public ObservableCollection<FrameworkElement> ChildWindows {
            get {
                return GetValue(ChildWindowsProperty) as ObservableCollection<FrameworkElement>;                
            }
            set { SetValue(ChildWindowsProperty, value); }
        }

      

        
        /// <summary>
        /// Gets and sets the scroll duration in milliseconds.
        /// </summary>
        public double ScrollDuration { get; set; }
        
        /// <summary>
        /// Gets the scrollviewer mediator.
        /// </summary>
        private ScrollViewerOffsetMediator ScrollViewerMediator {
            get {
                ScrollViewerOffsetMediator s = this.Template.FindName("Mediator_PART", this) as ScrollViewerOffsetMediator;
                if (s == null)
                    throw new Exception("DockWindowContainer ControlTemlate should define a Mediator_PART.");
                return s;
            }
        }

        /// <summary>
        /// Gets the windows scroll viewer.
        /// </summary>
        private ScrollViewer WindowScrollViewer {
            get {
                ScrollViewer s = this.Template.FindName("windowScrollViewer_PART", this) as ScrollViewer;
                if (s == null)
                    throw new Exception("DockWindowContainer ControlTemlate should define a windowScrollViewer_PART.");
                return s;
            }
        }


        /// <summary>
        /// Gets the content grid.
        /// </summary>
        private Grid ContentGrid {
            get {
                Grid g = this.Template.FindName("contentGrid_PART", this) as Grid;
                if (g == null)
                    throw new Exception("DockWindowContainer ControlTemlate should define a contentGrid_PART.");
                return g;
            }
        }

        /// <summary>
        /// Gets the content grid.
        /// </summary>
        private FrameworkElement MainContent {
            get {
                this.ApplyTemplate();
                FrameworkElement c = this.Template.FindName("mainContent_PART", this) as FrameworkElement;
                if (c == null)
                    throw new Exception("DockWindowContainer ControlTemlate should define a mainContent_PART.");
                return c;
            }
        }

        /// <summary>
        /// Gets the width of each new window.
        /// </summary>
        private double NewWindowWidth {
            get {
                return this.ActualWidth;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new storyboad for the scroll animation.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private Storyboard CreateStoryBoard(double offset) {

            offset = offset/1;

            if (!mediatorRegistered) {
                this.RegisterName(ScrollViewerMediator.Name, ScrollViewerMediator);
                mediatorRegistered = true;
            }

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();

            
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollDuration));

          
            Storyboard.SetTargetName(myDoubleAnimation, ScrollViewerMediator.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(ScrollViewerOffsetMediator.HorizontalOffsetProperty));
            Storyboard.SetDesiredFrameRate(myDoubleAnimation, 15);
            
            Storyboard myStoryboard = new Storyboard();

            myStoryboard.DecelerationRatio = 0.8;
            
            myStoryboard.Children.Add(myDoubleAnimation);

            if (offset >= 0) {
                //WindowScrollViewer.ScrollToHorizontalOffset(0);
                myDoubleAnimation.From = 0;
                myDoubleAnimation.To = offset;  
                myStoryboard.Completed += new EventHandler(AfterOpenWindow);
            } else {
                //WindowScrollViewer.ScrollToHorizontalOffset(-offset);
                myDoubleAnimation.From = -offset;
                myDoubleAnimation.By = offset;  
                myStoryboard.Completed += new EventHandler(AfterCloseWindow);
            }

            ScrollViewerMediator.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

            return myStoryboard;
        }
        
        /// <summary>
        /// Opens a new windos at thios dock window container.
        /// </summary>
        /// <param name="window"></param>
        public void OpenNewWindow(FrameworkElement window) {

            if (window==null)
                return;

            // adds action on the stack
            if (removingWindow != null) {
                actionsStack.Push(new WindowAction() { Action = WindowActions.OPEN, Window = window });
                return;
            }

            if (Application.Current.MainWindow != null && !Application.Current.MainWindow.IsActive) {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }

            // if the new window is the same as last one, return
            if (childWindows!=null && childWindows.Count > 0 && childWindows.Peek() == window)
                return;

            // to open a new window we need to remove the last one
            if (childWindows.Count > 0) {
                removingWindow = childWindows.Peek();                
            }
            else
                removingWindow = MainContent;
            
            // fixes the last column width
            ContentGrid.ColumnDefinitions.Last<ColumnDefinition>().Width = new GridLength(NewWindowWidth, GridUnitType.Pixel);

            // creates a new column
            ColumnDefinition newColumn = new ColumnDefinition();
            newColumn.Width = new GridLength(NewWindowWidth, GridUnitType.Pixel);
            ContentGrid.ColumnDefinitions.Add(newColumn);

            // adds the view to this column
            window.SetValue(Grid.ColumnProperty, ContentGrid.ColumnDefinitions.Count - 1);
            ContentGrid.Children.Add(window);

            childWindows.Push(window);

            newWindow = window;

            Storyboard myStoryboard = CreateStoryBoard(NewWindowWidth);
            myStoryboard.Begin(this, this.Template);
            
        }
        private Image lastWindowImage;
        private void ScreenToImage() {
            lastWindowImage = new Image();
            RenderTargetBitmap r = new RenderTargetBitmap((int)removingWindow.ActualWidth, (int)removingWindow.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            r.Render(removingWindow);
            lastWindowImage.Source = r;

            
            int? removingWindowColumn = removingWindow.GetValue(Grid.ColumnProperty) as int?;
            lastWindowImage.SetValue(Grid.ColumnProperty, removingWindowColumn);
            ContentGrid.Children.Remove(removingWindow);
            ContentGrid.Children.Add(lastWindowImage);
        }

        /// <summary>
        /// When the OUT scroll animation ends, adjust the grid columns.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AfterOpenWindow(object sender, EventArgs e) {
            _AfterOpenWindow();
        }

        private void _AfterOpenWindow() {
            

            ScrollViewerMediator.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            if (removingWindow == null)             
                return;

            
            if (ContentGrid.ColumnDefinitions.Count==1) {
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);                
                removingWindow = null;
                newWindow = null;
                return;
            }
            
            // set removing window column width to zero and remove it from visual tree           
            int removingWindowColumn = (int)removingWindow.GetValue(Grid.ColumnProperty);
            ContentGrid.ColumnDefinitions[removingWindowColumn].Width = new GridLength(0);
            ContentGrid.Children.Remove(removingWindow);
            removingWindow = null;

            if (newWindow == null)              
                return;

            // set the new window column width to star
            int newWindowColumn = (int)newWindow.GetValue(Grid.ColumnProperty);
            
            ContentGrid.ColumnDefinitions[newWindowColumn].Width = new GridLength(1, GridUnitType.Star);
            newWindow = null;

            ExecuteStackAction();
         
            
        }

        private void ExecuteStackAction() {
            if (actionsStack.Count > 0) {
                WindowAction stackAction = actionsStack.Pop();
                if (stackAction.Action == WindowActions.OPEN)
                    OpenNewWindow(stackAction.Window);
                else
                    CloseLastWindow();
            }
        }

        void AfterCloseWindow(object sender, EventArgs e) {
            _AfterCloseWindow();
        }
       
        void _AfterCloseWindow() {
            
            ScrollViewerMediator.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            // remove the window and its column
            int removingWindowColumn = (int)removingWindow.GetValue(Grid.ColumnProperty);
            ContentGrid.Children.Remove(removingWindow);
            ContentGrid.ColumnDefinitions.RemoveAt(removingWindowColumn);
            removingWindow = null;         
                        
            // set the last column width to star
            ContentGrid.ColumnDefinitions.Last<ColumnDefinition>().Width = new GridLength(1, GridUnitType.Star);

            ExecuteStackAction();
            
        }


        

        public void CloseAllWindows() {
            if (childWindows == null || childWindows.Count==0)
                return;

            // remove all child windows from grid
            foreach (FrameworkElement w in childWindows)
                ContentGrid.Children.Remove(w);

            // clear columns
            for (int i = ContentGrid.ColumnDefinitions.Count-1; i > 0; i--)
                ContentGrid.ColumnDefinitions.RemoveAt(i);

            ContentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);

            // clear child windows
            childWindows.Clear();

            // add again the main content
            if(!ContentGrid.Children.Contains(MainContent))
                ContentGrid.Children.Add(MainContent);

        }

        /// <summary>
        /// Closes the last window of this dock container.
        /// </summary>
        public void CloseLastWindow() {

            if (childWindows.Count == 0)
                return;

            // add action on the stack, if already running
            if (removingWindow != null) {                
                actionsStack.Push(new WindowAction() { Action = WindowActions.CLOSE, Window = removingWindow });
                return;
            }

            removingWindow = childWindows.Pop();

            // adds again the previous window at the visual tree
            FrameworkElement previousWindow = MainContent;
            if (childWindows.Count > 0)
                previousWindow = childWindows.Peek();
            ContentGrid.Children.Add(previousWindow);

            // sets grid width
            int previousColumn = (int) previousWindow.GetValue(Grid.ColumnProperty);
            ContentGrid.ColumnDefinitions[previousColumn].Width = new GridLength(NewWindowWidth, GridUnitType.Pixel);
            ContentGrid.ColumnDefinitions.Last<ColumnDefinition>().Width = new GridLength(NewWindowWidth, GridUnitType.Pixel);
            
            WindowScrollViewer.ScrollToHorizontalOffset(NewWindowWidth);

            
            Storyboard myStoryboard = CreateStoryBoard(-NewWindowWidth);
            myStoryboard.Begin(this, this.Template);

        }


        public bool IsLastWindow(FrameworkElement window) {
            return (window == childWindows.Peek());
        }

        private static DockWindowContainer FindDockWindowContainerAncestor(Visual child) {

            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is DockWindowContainer)) {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return (parent as DockWindowContainer);
        }

        private static void OnChildWindowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            DockWindowContainer container = (DockWindowContainer)d;
            if (container == null)
                return;
            var old = e.OldValue as ObservableCollection<object>;

            if (old != null) {
                old.CollectionChanged -= container.OnWorkCollectionChanged;
                container.CloseAllWindows();
            }

            var n = e.NewValue as ObservableCollection<object>;

            if (n != null)
                n.CollectionChanged += container.OnWorkCollectionChanged;
        }

        

        private void OnWorkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                // Clear and update entire collection 
                CloseAllWindows();
            }

            if (e.NewItems != null) {
                foreach (object w in e.NewItems)                    
                        OpenNewWindow(w as FrameworkElement);
            }

            if (e.OldItems != null) {
                foreach (object w in e.OldItems)
                    //if (IsLastWindow(w as FrameworkElement))
                        CloseLastWindow();
            }
        }


        #endregion


    }

    /// <summary>
    /// Mediator that forwards Offset property changes on to a ScrollViewer
    /// instance to enable the animation of Horizontal/VerticalOffset.
    /// </summary>
    public class ScrollViewerOffsetMediator : FrameworkElement {
        /// <summary>
        /// ScrollViewer instance to forward Offset changes on to.
        /// </summary>
        public ScrollViewer ScrollViewer {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register(
                "ScrollViewer",
                typeof(ScrollViewer),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(OnScrollViewerChanged));

        private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var mediator = (ScrollViewerOffsetMediator)o;
            var scrollViewer = (ScrollViewer)(e.NewValue);
            if (null != scrollViewer) {
                scrollViewer.ScrollToVerticalOffset(mediator.VerticalOffset);
            }
        }

        /// <summary>
        /// VerticalOffset property to forward to the ScrollViewer.
        /// </summary>
        public double VerticalOffset {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(OnVerticalOffsetChanged));
        public static void OnVerticalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var mediator = (ScrollViewerOffsetMediator)o;
            if (null != mediator.ScrollViewer) {
                mediator.ScrollViewer.ScrollToVerticalOffset((double)(e.NewValue));
            }
        }

        public double HorizontalOffset {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(
                "HorizontalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(OnHorizontalOffsetChanged));

        public static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var mediator = (ScrollViewerOffsetMediator)o;
            if (null != mediator.ScrollViewer) {
                mediator.ScrollViewer.ScrollToHorizontalOffset((double)(e.NewValue));
            }

        }

    
    }

    public struct WindowAction {
        public FrameworkElement Window { get; set; }
        public WindowActions Action { get; set; }
    }
}
