using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScrumFactory.Windows.Helpers.Adorners {

    public class SurroundElementAdorner : Adorner {

        private List<FrameworkElement> children = new List<FrameworkElement>();

        private FrameworkElement leftControl;
        public FrameworkElement LeftControl {
            get {
                return leftControl;
            }
            set {
                if (leftControl != null) {
                    RemoveVisualChild(leftControl);
                    children.Remove(leftControl);
                }
                leftControl = value;
                AddVisualChild(leftControl);
                children.Add(leftControl);
                UpdateLayout();
            }
        }

        private FrameworkElement rightControl;
        public FrameworkElement RightControl {
            get {
                return rightControl;
            }
            set {
                if (rightControl != null) {
                    RemoveVisualChild(rightControl);
                    children.Remove(rightControl);
                }
                rightControl = value;
                AddVisualChild(rightControl);
                children.Add(rightControl);
                UpdateLayout();
            }
        }

        private FrameworkElement topControl;
        public FrameworkElement TopControl {
            get {
                return topControl;
            }
            set {
                RemoveVisualChild(topControl);
                topControl = value;
                AddVisualChild(topControl);
                UpdateLayout();
            }
        }

        private FrameworkElement bottomControl;
        public FrameworkElement BottomControl {
            get {
                return bottomControl;
            }
            set {
                if (bottomControl != null) {
                    RemoveVisualChild(bottomControl);
                    children.Remove(bottomControl);
                }
                bottomControl = value;
                AddVisualChild(bottomControl);
                children.Add(bottomControl);
                UpdateLayout();
            }
        }


        protected override int VisualChildrenCount {
            get {
                return children.Count;
            }
        }

        protected override Visual GetVisualChild(int index) {
            if (index < children.Count)
                return children.ElementAt(index);
            return base.GetVisualChild(index);
        }


        public SurroundElementAdorner(UIElement adornedElement)
            : base(adornedElement) {

        }

        protected override Size ArrangeOverride(Size finalSize) {

            if (RightControl != null) {
                Point adorningPoint = new Point();
                adorningPoint.X = ((System.Windows.FrameworkElement)AdornedElement).ActualWidth;
                double height = ((System.Windows.FrameworkElement)AdornedElement).ActualHeight;
                if (height < RightControl.MinHeight)
                    height = RightControl.MinHeight;                    
                RightControl.Height = height;
                RightControl.Arrange(new Rect(adorningPoint, new Size(RightControl.DesiredSize.Width, height)));                
            }

            if (LeftControl != null) {
                Point adorningPoint = new Point();
                adorningPoint.X -= LeftControl.DesiredSize.Width;
                double height = ((System.Windows.FrameworkElement)AdornedElement).ActualHeight;
                if (height < LeftControl.MinHeight)
                    height = LeftControl.MinHeight;                    
                LeftControl.Height = height;
                LeftControl.Arrange(new Rect(adorningPoint, new Size(LeftControl.DesiredSize.Width,height)));                
            }

            if (TopControl != null) {
                Point adorningPoint = new Point();
                adorningPoint.Y -= TopControl.DesiredSize.Height;
                double width = ((System.Windows.FrameworkElement)AdornedElement).ActualWidth;
                if (width < TopControl.MinWidth)
                    width = TopControl.MinWidth;
                TopControl.Width = width;
                TopControl.Arrange(new Rect(adorningPoint, new Size(width, TopControl.DesiredSize.Height)));
            }

            if (BottomControl != null) {
                Point adorningPoint = new Point();
                adorningPoint.Y = ((System.Windows.FrameworkElement)AdornedElement).ActualHeight;
                double width = ((System.Windows.FrameworkElement)AdornedElement).ActualWidth;
                if (width < BottomControl.MinWidth)
                    width = BottomControl.MinWidth;
                BottomControl.Width = width;
                BottomControl.Arrange(new Rect(adorningPoint, new Size(width, BottomControl.DesiredSize.Height)));                
            }

            return finalSize;

        }

        
    }




    public class SurroundBorderAdorner : PixelBorder {

        internal SurroundElementAdorner surroundAdorner;
        private AdornerLayer adLayer;

        public static readonly DependencyProperty LeftControlProperty = DependencyProperty.Register(
            "LeftControl", typeof(UIElement), typeof(SurroundBorderAdorner), new PropertyMetadata(OnLeftControlChanged));
        public UIElement LeftControl {
            get { return (UIElement)this.GetValue(LeftControlProperty); }
            set {
                this.SetValue(LeftControlProperty, value);
            }
        }

        public static readonly DependencyProperty RightControlProperty = DependencyProperty.Register(
            "RightControl", typeof(UIElement), typeof(SurroundBorderAdorner), new PropertyMetadata(OnRightControlChanged));
        public UIElement RightControl {
            get { return (UIElement)this.GetValue(RightControlProperty); }
            set {
                this.SetValue(RightControlProperty, value);
            }
        }

        public static readonly DependencyProperty TopControlProperty = DependencyProperty.Register(
            "TopControl", typeof(UIElement), typeof(SurroundBorderAdorner), new PropertyMetadata(OnTopControlChanged));
        public UIElement TopControl {
            get { return (UIElement)this.GetValue(TopControlProperty); }
            set {
                this.SetValue(TopControlProperty, value);
            }
        }

        public static readonly DependencyProperty BottomControlProperty = DependencyProperty.Register(
            "BottomControl", typeof(UIElement), typeof(SurroundBorderAdorner), new PropertyMetadata(OnBottomControlChanged));
        public UIElement BottomControl {
            get { return (UIElement)this.GetValue(BottomControlProperty); }
            set {
                this.SetValue(BottomControlProperty, value);
            }
        }

        public static readonly DependencyProperty AdornerLayerElementProperty = DependencyProperty.Register(
            "AdornerLayerElement", typeof(UIElement), typeof(SurroundBorderAdorner), null);

        public UIElement AdornerLayerElement {
            get { return (UIElement)this.GetValue(AdornerLayerElementProperty); }
            set { this.SetValue(AdornerLayerElementProperty, value); }
        }

        private static void OnLeftControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SurroundBorderAdorner border = d as SurroundBorderAdorner;
            if (border == null)
                return;
            border.surroundAdorner.LeftControl = (FrameworkElement) e.NewValue;
            border.AddLogicalChild(e.NewValue);
        }

        private static void OnRightControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SurroundBorderAdorner border = d as SurroundBorderAdorner;
            if (border == null)
                return;
            border.surroundAdorner.RightControl = (FrameworkElement)e.NewValue;
            border.AddLogicalChild(e.NewValue);
        }

        private static void OnTopControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SurroundBorderAdorner border = d as SurroundBorderAdorner;
            if (border == null)
                return;
            border.surroundAdorner.TopControl = (FrameworkElement)e.NewValue;
            border.AddLogicalChild(e.NewValue);
        }

        private static void OnBottomControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SurroundBorderAdorner border = d as SurroundBorderAdorner;
            if (border == null)
                return;
            border.surroundAdorner.BottomControl = (FrameworkElement)e.NewValue;
            border.AddLogicalChild(e.NewValue);
        }

        public SurroundBorderAdorner()
            : base() {        
            surroundAdorner = new SurroundElementAdorner(this);            
        }


        private AdornerLayer AdLayer {
            get {
                if (adLayer == null) {
                    if(AdornerLayerElement==null)
                        adLayer = AdornerLayer.GetAdornerLayer(this);
                    else
                        adLayer = AdornerLayer.GetAdornerLayer(AdornerLayerElement);
                }
                return adLayer;
            }
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e) {
            base.OnMouseEnter(e);                        
            AdLayer.Add(surroundAdorner);           
            UpdateLayout();
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e) {
            base.OnMouseLeave(e);            
            AdLayer.Remove(surroundAdorner);
            UpdateLayout();
        }


    }
}
