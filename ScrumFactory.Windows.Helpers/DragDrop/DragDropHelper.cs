﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Documents;
using System.Reflection;
using ScrumFactory.Windows.Helpers.Extensions;

namespace ScrumFactory.Windows.Helpers.DragDrop {

	public class DragDropHelper {
		// source and target
		private DataFormat format = DataFormats.GetDataFormat("DragDropItemsControl");
		private Point initialMousePosition;
		private Vector initialMouseOffset;
		private object draggedData;
		private DraggedAdorner draggedAdorner;
		private InsertionAdorner insertionAdorner;
		private Window topWindow;
		// source
		private ItemsControl sourceItemsControl;
		private FrameworkElement sourceItemContainer;
		// target
		private ItemsControl targetItemsControl;
		private FrameworkElement targetItemContainer;
		private bool hasVerticalOrientation;
		private int insertionIndex;
		private bool isInFirstHalf;

        private FrameworkElement targetInsertionItemContainer;


		// singleton
		private static DragDropHelper instance;
		private static DragDropHelper Instance 
		{
			get 
			{  
				if(instance == null)
				{
					instance = new DragDropHelper();
				}
				return instance;
			}
		}



        public static bool GetShowInsertAdorner(DependencyObject obj) {
            return (bool)obj.GetValue(ShowInsertAdornerProperty);
        }

        public static void SetShowInsertAdorner(DependencyObject obj, bool value) {
            obj.SetValue(ShowInsertAdornerProperty, value);
        }

        public static readonly DependencyProperty ShowInsertAdornerProperty =
            DependencyProperty.RegisterAttached("ShowInsertAdorner", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(true));


        public static ICommand GetOnDropCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(OnDropCommandProperty);
        }

        public static void SetOnDropCommand(DependencyObject obj, ICommand value) {
            obj.SetValue(OnDropCommandProperty, value);
        }

        public static readonly DependencyProperty OnDropCommandProperty =
            DependencyProperty.RegisterAttached("OnDropCommand", typeof(ICommand), typeof(DragDropHelper), new UIPropertyMetadata(null));

        public static bool GetIsDragAllowed(DependencyObject obj) {
            return (bool)obj.GetValue(IsDragAllowedProperty);
        }

        public static void SetIsDragAllowed(DependencyObject obj, bool value) {
            obj.SetValue(IsDragAllowedProperty, value);
        }

        public static readonly DependencyProperty IsDragAllowedProperty =
            DependencyProperty.RegisterAttached("IsDragAllowed", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(true, IsDropAllowedChanged));



		public static bool GetIsDragSource(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragSourceProperty);
		}

		public static void SetIsDragSource(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragSourceProperty, value);
		}

		public static readonly DependencyProperty IsDragSourceProperty =
			DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDragSourceChanged));


		public static bool GetIsDropTarget(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDropTargetProperty);
		}

		public static void SetIsDropTarget(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDropTargetProperty, value);
		}

        public static readonly DependencyProperty PositionDropProperty =
            DependencyProperty.RegisterAttached("PositionDrop", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(true));

        public static bool GetPositionDrop(DependencyObject obj) {
            return (bool)obj.GetValue(PositionDropProperty);
        }

        public static void SetPositionDrop(DependencyObject obj, bool value) {
            obj.SetValue(PositionDropProperty, value);
        }


		public static readonly DependencyProperty IsDropTargetProperty =
			DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDropTargetChanged));

		public static DataTemplate GetDragDropTemplate(DependencyObject obj)
		{
			return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
		}

		public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
		{
			obj.SetValue(DragDropTemplateProperty, value);
		}

		public static readonly DependencyProperty DragDropTemplateProperty =
			DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(DragDropHelper), new UIPropertyMetadata(null));

        private static void IsDropAllowedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            /*
            FrameworkElement template = obj as FrameworkElement;
            if (template == null)
                return;
            if(template.TemplatedParent!=null)
                template.TemplatedParent.SetValue(DragDrop.DragDropHelper.IsDragAllowedProperty, e.NewValue);
             */
        }
     

		private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dragSource = obj as ItemsControl;
			if (dragSource != null)
			{
				if (Object.Equals(e.NewValue, true))
				{                    
					dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
					dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
					dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
				}
				else
				{
					dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
					dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
					dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
				}
			}
		}

		private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dropTarget = obj as ItemsControl;
			if (dropTarget != null)
			{
				if (Object.Equals(e.NewValue, true))
				{                    
					dropTarget.AllowDrop = true;
					dropTarget.PreviewDrop += Instance.DropTarget_PreviewDrop;
					dropTarget.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
					dropTarget.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
					dropTarget.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
				}
				else
				{
					dropTarget.AllowDrop = false;
					dropTarget.PreviewDrop -= Instance.DropTarget_PreviewDrop;
					dropTarget.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
					dropTarget.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
					dropTarget.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
				}
			}
		}

		// DragSource

		private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.sourceItemsControl = (ItemsControl)sender;
            DependencyObject visual = e.OriginalSource as DependencyObject;
            if (visual == null)
                return;

            this.sourceItemContainer = sourceItemsControl.ContainerFromElement(visual) as FrameworkElement;
            if (this.sourceItemContainer == null)
                return;

            
            bool? canDrag = sourceItemContainer.GetValue(DragDrop.DragDropHelper.IsDragAllowedProperty) as bool?;
            if (canDrag.HasValue && !canDrag.Value)
                return;
            
            this.draggedData = this.sourceItemContainer.DataContext;            
			this.topWindow = Window.GetWindow(this.sourceItemsControl);
			this.initialMousePosition = e.GetPosition(this.topWindow);

			

            
		}

		// Drag = mouse down + move by a certain amount
		private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (this.draggedData != null)
			{
				// Only drag when user moved the mouse by a reasonable amount.
				if (Utilities.IsMovementBigEnough(this.initialMousePosition, e.GetPosition(this.topWindow)))
				{
					this.initialMouseOffset = this.initialMousePosition - this.sourceItemContainer.TranslatePoint(new Point(0, 0), this.topWindow);

					DataObject data = new DataObject(this.format.Name, this.draggedData);

					// Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
					bool previousAllowDrop = this.topWindow.AllowDrop;
					this.topWindow.AllowDrop = true;
					this.topWindow.DragEnter += TopWindow_DragEnter;
					this.topWindow.DragOver += TopWindow_DragOver;
					this.topWindow.DragLeave += TopWindow_DragLeave;
					
					DragDropEffects effects = System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

					// Without this call, there would be a bug in the following scenario: Click on a data item, and drag
					// the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
					// the Window leave event, and the dragged adorner is left behind.
					// With this call, the dragged adorner will disappear when we release the mouse outside of the window,
					// which is when the DoDragDrop synchronous method returns.
					RemoveDraggedAdorner();

					this.topWindow.AllowDrop = previousAllowDrop;
					this.topWindow.DragEnter -= TopWindow_DragEnter;
					this.topWindow.DragOver -= TopWindow_DragOver;
					this.topWindow.DragLeave -= TopWindow_DragLeave;
					
					this.draggedData = null;
				}
			}
		}
			
		private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.draggedData = null;
		}

		// DropTarget

		private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
		{
			this.targetItemsControl = (ItemsControl)sender;
			object draggedItem = e.Data.GetData(this.format.Name);

			DecideDropTarget(e);
			if (draggedItem != null)
			{
				// Dragged Adorner is created on the first enter only.
				ShowDraggedAdorner(e.GetPosition(this.topWindow));
				CreateInsertionAdorner();
			}   
			e.Handled = true;
		}

		private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
		{
			object draggedItem = e.Data.GetData(this.format.Name);

			DecideDropTarget(e);
			if (draggedItem != null)
			{
				// Dragged Adorner is only updated here - it has already been created in DragEnter.
				ShowDraggedAdorner(e.GetPosition(this.topWindow));
				UpdateInsertionAdornerPosition();
			}
			e.Handled = true;
		}

		private void DropTarget_PreviewDrop(object sender, DragEventArgs e) {

            if (this.draggedData == null)
                return;

			object draggedItem = e.Data.GetData(this.format.Name);
			int indexRemoved = -1;            

			if (draggedItem != null) {
                
                ICommand OnDropCommand = GetOnDropCommand(this.targetItemsControl);

                // if is using the Command model
                if (OnDropCommand != null) {

                    

                    // create de drop command parameter
                    DropCommandParameter p = new DropCommandParameter();
                    p.Item = draggedItem;
                    p.InsertionIndex = this.insertionIndex;       
             
                    if (this.targetInsertionItemContainer != null)
                        p.DropTargetItem = this.targetInsertionItemContainer.DataContext;
                    else
                        p.DropTargetItem = null;

                    p.DropTargetListTag = this.targetItemsControl.Tag;
                    p.Group = FindGroup(this.targetItemsControl, e.GetPosition(this.targetItemsControl));

                    // invoke command
                    OnDropCommand.Execute(p);
                } else {
                    if ((e.Effects & DragDropEffects.Move) != 0) {
                        indexRemoved = Utilities.RemoveItemFromItemsControl(this.sourceItemsControl, draggedItem);
                    }
                    // This happens when we drag an item to a later position within the same ItemsControl.
                    if (indexRemoved != -1 && this.sourceItemsControl == this.targetItemsControl && indexRemoved < this.insertionIndex) {
                        this.insertionIndex--;
                    }
                    Utilities.InsertItemInItemsControl(this.targetItemsControl, draggedItem, this.insertionIndex);
                }

				RemoveDraggedAdorner();
				RemoveInsertionAdorner();
            }

            
            e.Handled = true;
		}

        
        private System.Windows.Data.CollectionViewGroup FindGroup(ItemsControl itemsControl, Point position) {
            DependencyObject element = itemsControl.InputHitTest(position) as DependencyObject;

            if (element != null) {
                GroupItem groupItem = element.GetVisualAncestor<GroupItem>();

                if (groupItem != null) {
                    return groupItem.Content as System.Windows.Data.CollectionViewGroup;
                }
            }

            return null;
        }

		private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
		{
			// Dragged Adorner is only created once on DragEnter + every time we enter the window. 
			// It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
			object draggedItem = e.Data.GetData(this.format.Name);


			if (draggedItem != null)
			{
				RemoveInsertionAdorner();
			}
			e.Handled = true;
		}

		// If the types of the dragged data and ItemsControl's source are compatible, 
		// there are 3 situations to have into account when deciding the drop target:
		// 1. mouse is over an items container
		// 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
		// 3. mouse is over an empty ItemsControl.
		// The goal of this method is to decide on the values of the following properties: 
		// targetItemContainer, insertionIndex and isInFirstHalf.
		private void DecideDropTarget(DragEventArgs e) {

			int targetItemsControlCount = this.targetItemsControl.Items.Count;
			object draggedItem = e.Data.GetData(this.format.Name);

            bool positionDrop = (bool)this.targetItemsControl.GetValue(PositionDropProperty);
       
			if (!positionDrop || IsDropDataTypeAllowed(draggedItem)) {

				if (targetItemsControlCount > 0) {
					this.hasVerticalOrientation = Utilities.HasVerticalOrientation(this.targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
					this.targetItemContainer = targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;
                    this.targetInsertionItemContainer = targetItemContainer;
                    
					if (this.targetItemContainer != null) {

						Point positionRelativeToItemContainer = e.GetPosition(this.targetItemContainer);
						this.isInFirstHalf = Utilities.IsInFirstHalf(this.targetItemContainer, positionRelativeToItemContainer, this.hasVerticalOrientation);
						this.insertionIndex = this.targetItemsControl.ItemContainerGenerator.IndexFromContainer(this.targetItemContainer);

                        if (!this.isInFirstHalf && positionDrop) {
                            this.insertionIndex++;
                            // add my me
                            this.targetInsertionItemContainer = this.targetItemsControl.ItemContainerGenerator.ContainerFromIndex(this.insertionIndex) as FrameworkElement;
                        }
						
					}
					else {
                        /*
						this.targetItemContainer = this.targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
						this.isInFirstHalf = false;
						this.insertionIndex = targetItemsControlCount;*/
					}                    
				}
				else {
					this.targetItemContainer = null;
					this.insertionIndex = 0;
				}


			}
			else {
				this.targetItemContainer = null;
				this.insertionIndex = -1;
				e.Effects = DragDropEffects.None;
			}

		}

		// Can the dragged data be added to the destination collection?
		// It can if destination is bound to IList<allowed type>, IList or not data bound.
		private bool IsDropDataTypeAllowed(object draggedItem)
		{
            


			bool isDropDataTypeAllowed;
			IEnumerable collectionSource = this.targetItemsControl.ItemsSource;
            // added by me
            if (collectionSource is System.Windows.Data.ListCollectionView) {
                System.Windows.Data.ListCollectionView view = collectionSource as System.Windows.Data.ListCollectionView;
                collectionSource = view.SourceCollection;
            }
			if (draggedItem != null)
			{
				if (collectionSource != null)
				{
					Type draggedType = draggedItem.GetType();
					Type collectionType = collectionSource.GetType();

					Type genericIListType = collectionType.GetInterface("IList`1");
					if (genericIListType != null)
					{
						Type[] genericArguments = genericIListType.GetGenericArguments();
						isDropDataTypeAllowed = genericArguments[0].IsAssignableFrom(draggedType);
					}
					else if (typeof(IList).IsAssignableFrom(collectionType))
					{
						isDropDataTypeAllowed = true;                                        
                    } else {
                        isDropDataTypeAllowed = false;
                    }
				}
				else // the ItemsControl's ItemsSource is not data bound.
				{
					isDropDataTypeAllowed = true;
				}
			}
			else
			{
				isDropDataTypeAllowed = false;			
			}
			return isDropDataTypeAllowed;
		}

		// Window

		private void TopWindow_DragEnter(object sender, DragEventArgs e)
		{
			ShowDraggedAdorner(e.GetPosition(this.topWindow));
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragOver(object sender, DragEventArgs e)
		{
			ShowDraggedAdorner(e.GetPosition(this.topWindow));
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragLeave(object sender, DragEventArgs e)
		{
			RemoveDraggedAdorner();
			e.Handled = true;
		}

		// Adorners

		// Creates or updates the dragged Adorner. 
		private void ShowDraggedAdorner(Point currentPosition)
		{
			if (this.draggedAdorner == null)
			{
				var adornerLayer = AdornerLayer.GetAdornerLayer(this.sourceItemsControl);
				this.draggedAdorner = new DraggedAdorner(this.draggedData, GetDragDropTemplate(this.sourceItemsControl), this.sourceItemContainer, adornerLayer);
			}
			this.draggedAdorner.SetPosition(currentPosition.X - this.initialMousePosition.X + this.initialMouseOffset.X, currentPosition.Y - this.initialMousePosition.Y + this.initialMouseOffset.Y);
		}

		private void RemoveDraggedAdorner()
		{
			if (this.draggedAdorner != null)
			{
				this.draggedAdorner.Detach();
				this.draggedAdorner = null;
			}
		}

		private void CreateInsertionAdorner()
		{

			if (this.targetItemContainer != null)
			{

                if (this.targetItemsControl!=null && GetShowInsertAdorner(this.targetItemsControl) == false)
                    return;

                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
				// This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
				var adornerLayer = AdornerLayer.GetAdornerLayer(this.targetItemContainer);
				this.insertionAdorner = new InsertionAdorner(this.hasVerticalOrientation, this.isInFirstHalf, this.targetItemContainer, adornerLayer);
			}
		}

		private void UpdateInsertionAdornerPosition()
		{
			if (this.insertionAdorner != null) {
				this.insertionAdorner.IsInFirstHalf = this.isInFirstHalf;
				this.insertionAdorner.InvalidateVisual();
			}
		}

		private void RemoveInsertionAdorner()
		{
			if (this.insertionAdorner != null)
			{
				this.insertionAdorner.Detach();
				this.insertionAdorner = null;
			}
		}
	}

    public class DropCommandParameter {
        public object Item { get; set; }
        public int InsertionIndex { get; set; }
        public object DropTargetListTag { get; set; }
        public object DropTargetItem { get; set; }
        public System.Windows.Data.CollectionViewGroup Group { get; set; }
    }
}
