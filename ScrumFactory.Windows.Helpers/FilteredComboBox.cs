using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;

namespace ScrumFactory.Windows.Helpers {

    public delegate bool CustomFilterDelegate(string text, object item);

    /// <summary>
    /// Editable combo box which uses the text in its editable textbox to perform a lookup
    /// in its data source.
    /// THANKS to: http://dotbay.blogspot.com/2009/04/building-filtered-combobox-for-wpf.html
    /// </summary>
    public class FilteredComboBox : ComboBox {

        private static System.Text.RegularExpressions.Regex nonSpacingMarkRegex = new System.Text.RegularExpressions.Regex(@"\p{Mn}", System.Text.RegularExpressions.RegexOptions.Compiled);

        ////
        // Public Fields
        ////



        public static readonly DependencyProperty RefreshItemSourceProperty =
            DependencyProperty.Register(
            "RefreshItemSource",
            typeof(System.Action<string>),
            typeof(FilteredComboBox));

        public static readonly DependencyProperty CustomFilterProperty =
            DependencyProperty.Register(
            "CustomFilter",
            typeof(CustomFilterDelegate),
            typeof(FilteredComboBox));

        /// <summary>
        /// The search string treshold length.
        /// </summary>
        /// <remarks>
        /// It's implemented as a Dependency Property, so you can set it in a XAML template
        /// </remarks>
        public static readonly DependencyProperty MinimumSearchLengthProperty =
            DependencyProperty.Register(
            "MinimumSearchLength",
            typeof(int),
            typeof(FilteredComboBox),
            new UIPropertyMetadata(0));

        ////
        // Private Fields
        ////

        /// <summary>
        /// Caches the previous value of the filter.
        /// </summary>
        private string oldFilter = string.Empty;

        /// <summary>
        /// Holds the current value of the filter.
        /// </summary>
        private string currentFilter = string.Empty;

        ////
        // Constructors
        ////

        /// <summary>
        /// Initializes a new instance of the FilteredComboBox class.
        /// </summary>
        /// <remarks>
        /// You could set 'IsTextSearchEnabled' to 'false' here, /// to avoid non-intuitive behavior of the control
        /// </remarks>
        public FilteredComboBox() {
            IsTextSearchEnabled = false;
            
        }


        ////
        // Properties
        ////

        /// <summary>
        /// Gets or sets the search string treshold length.
        /// </summary>
        /// <value>The minimum length of the search string that triggers filtering.</value> [Description("Length of the search string that triggers filtering.")] [Category("Filtered ComboBox")] [DefaultValue(3)]
        public int MinimumSearchLength {
            [System.Diagnostics.DebuggerStepThrough]
            get {
                return (int)this.GetValue(MinimumSearchLengthProperty);
            }

            [System.Diagnostics.DebuggerStepThrough]
            set {
                this.SetValue(MinimumSearchLengthProperty, value);
            }
        }

        public CustomFilterDelegate CustomFilter {
            [System.Diagnostics.DebuggerStepThrough]
            get {
                return (CustomFilterDelegate)this.GetValue(CustomFilterProperty);
            }

            [System.Diagnostics.DebuggerStepThrough]
            set {
                this.SetValue(CustomFilterProperty, value);
            }
        }


        public System.Action<string> RefreshItemSource {
            [System.Diagnostics.DebuggerStepThrough]
            get {
                return (System.Action<string>)this.GetValue(RefreshItemSourceProperty);
            }

            [System.Diagnostics.DebuggerStepThrough]
            set {
                this.SetValue(RefreshItemSourceProperty, value);
            }
        }
       

        public bool ContainsFilter { get; set; }

        private bool _filtering = false;
        private bool Filtering {
            get { return _filtering; }
            set {
                if (value == _filtering)
                    return;
                _filtering = value;
                ForceFilter();
            }
        }

        private void ForceFilter() {
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
            if (view == null)
                return;
            view.Filter -= this.FilterPredicate;
            if (_filtering)
                view.Filter += this.FilterPredicate;
            view.Refresh();
        }

        /// <summary>
        /// Gets a reference to the internal editable textbox.
        /// </summary>
        /// <value>A reference to the internal editable textbox.</value>
        /// <remarks>
        /// We need this to get access to the Selection.
        /// </remarks>
        protected TextBox EditableTextBox {
            get {
                return this.GetTemplateChild("PART_EditableTextBox") as TextBox;
            }
        }

        protected Border MoreResultsBorder {
            get {
                return this.GetTemplateChild("PART_MoreResultsBorder") as Border;
            }
        }

        protected ScrollViewer ScrollViewer {
            get {
                return this.GetTemplateChild("PART_Scroll") as ScrollViewer;
            }
        }

        
        public int? MaxItemsResults { get; set; }
                    
        
        ////
        // Event Raiser Overrides
        ////

        private bool changingSource = false;

        /// <summary>
        /// Keep the filter if the ItemsSource is explicitly changed.
        /// </summary>
        /// <param name="oldValue">The previous value of the filter.</param>
        /// <param name="newValue">The current value of the filter.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {

            if(RefreshItemSource==null)
                changingSource = true;

            base.OnItemsSourceChanged(oldValue, newValue);

            if (RefreshItemSource != null) {

                if (newValue != null) {
                    ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
                    view.Filter -= this.FilterPredicate;
                    view.Filter += this.FilterPredicate;
                }

                if (oldValue != null) {
                    ICollectionView view = CollectionViewSource.GetDefaultView(oldValue);
                    view.Filter -= this.FilterPredicate;
                }

                
                this.RefreshFilter();
                this.IsDropDownOpen = true;
                

                // Unselect
                this.EditableTextBox.SelectionStart = int.MaxValue;
            }

            changingSource = false;

        }

        //protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
        //    base.OnSelectionChanged(e);
        //    ClearFilter();
        //}

        /// <summary>
        /// Confirm or cancel the selection when Tab, Enter, or Escape are hit.
        /// Open the DropDown when the Down Arrow is hit.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// The 'KeyDown' event is not raised for Arrows, Tab and Enter keys.
        /// It is swallowed by the DropDown if it's open.
        /// So use the Preview instead.
        /// </remarks>
        protected override void OnPreviewKeyDown(KeyEventArgs e) {

            if (e.Key == Key.Tab || e.Key == Key.Enter) {
                // Explicit Selection -> Close ItemsPanel
                this.IsDropDownOpen = false;
                this.SetItem();
                
            } else if (e.Key == Key.Escape) {                
                // Escape -> Close DropDown and redisplay Filter
                this.IsDropDownOpen = false;
                this.SelectedIndex = -1;
                this.Text = this.currentFilter;
                Filtering = false;
            } else {
                if (e.Key == Key.Down) {

                    //if (IsDropDownOpen) {
                    //    ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
                    //    view.MoveCurrentToNext();
                    //    SelectedItem = view.CurrentItem;
                    //}
                    //else
                        // Arrow Down -> Open DropDown
                        this.IsDropDownOpen = true;
                }

                

                Filtering = true;
                base.OnPreviewKeyDown(e);
            }

            // Cache text
            this.oldFilter = this.Text;
        }

        /// <summary>
        /// Modify and apply the filter.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// Alternatively, you could react on 'OnTextChanged', but navigating through
        /// the DropDown will also change the text.
        /// </remarks>
        protected override void OnKeyUp(KeyEventArgs e) {
            

            if (e.Key == Key.Up || e.Key == Key.Down) {
                // Navigation keys are ignored
            } else if (e.Key == Key.Tab || e.Key == Key.Enter) {                
                // Explicit Select -> Clear Filter
                this.ClearFilter();
            } else {
                // The text was changed
                if (this.Text != this.oldFilter) {

                    // Update Filter Value
                    this.currentFilter = this.Text;

                    if (this.currentFilter.Length >= this.MinimumSearchLength && RefreshItemSource != null) {
                        RefreshItemSource.Invoke(this.Text);
                        
                    }

                    // Clear the filter if the text is empty,
                    // apply the filter if the text is long enough
                    if (this.currentFilter.Length == 0 || this.currentFilter.Length >= this.MinimumSearchLength && RefreshItemSource == null) {                        
                        this.RefreshFilter();
                        this.IsDropDownOpen = true;

                        // Unselect
                        this.EditableTextBox.SelectionStart = int.MaxValue;
                    }
                }

                base.OnKeyUp(e);

            
            }
        }

        /// <summary>
        /// Make sure the text corresponds to the selection when leaving the control.
        /// </summary>
        /// <param name="e">A KeyBoardFocusChangedEventArgs.</param>
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            
            this.ClearFilter();            
            base.OnPreviewLostKeyboardFocus(e);            
        }

        ////
        // Helpers
        ////

        private int itemCount = 0;

        /// <summary>
        /// Re-apply the Filter.
        /// </summary>
        private void RefreshFilter() {

            if (this.ItemsSource == null ||this.EditableTextBox==null)
                return;

            itemCount = 0;
            HideMoreItemsResults();            
            ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);            
            view.Refresh();
                
        }

        private void ShowMoreItemsResults() {
            if (MoreResultsBorder == null)
                return;
            if(MoreResultsBorder.Visibility== Visibility.Collapsed)
                MoreResultsBorder.Visibility = Visibility.Visible;
            if(ScrollViewer!=null)
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void HideMoreItemsResults() {
            if (MoreResultsBorder == null)
                return;
            MoreResultsBorder.Visibility = Visibility.Collapsed;
        }

        private void SetItem() {
            if (this.ItemsSource == null) return;
            if (SelectedItem != null) return;

            ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
            view.MoveCurrentToFirst();
            SelectedItem = view.CurrentItem;
            Filtering = false;
            
        }

        /// <summary>
        /// Clear the Filter.
        /// </summary>
        private void ClearFilter() {
            this.currentFilter = string.Empty;
            Filtering = false;
            RefreshFilter();            
        }

        /// <summary>
        /// The Filter predicate that will be applied to each row in the ItemsSource.
        /// </summary>
        /// <param name="value">A row in the ItemsSource.</param>
        /// <returns>Whether or not the item will appear in the DropDown.</returns>
        private bool FilterPredicate(object value) {

     
            
            bool passed = false;

            if (MaxItemsResults != null && itemCount >= MaxItemsResults && !changingSource) {
                ShowMoreItemsResults();
                return false;
            }

            // if is a dynamic source, all items are ok, cuz they are already filtered at the server
            if (RefreshItemSource != null) {
                itemCount++;
                return true;
            }

            // No item, no text
            if (value == null)
                return false;

            // No text, no filter
            if (this.currentFilter.Length == 0) {
                itemCount++;
                return true;
            }

            // if has a custom filter use it
            if (CustomFilter != null) {
                passed = CustomFilter(this.currentFilter, value);
                if (passed)
                    itemCount++;
                return passed;
            }

            
            // Case insensitive search
            if (ContainsFilter) {
                string[] tags = Normalize(this.currentFilter).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                passed = tags.All(t => Normalize(value.ToString()).Contains(t));
                if (passed)
                    itemCount++;
                return passed;
            }

            // start width search
            passed = value.ToString().ToLower().StartsWith(this.currentFilter.ToLower());
            if (passed)
                itemCount++;
            return passed;
        }


        public string Normalize(string str) {

            string norm1 = str.Normalize(System.Text.NormalizationForm.FormD).ToLower();
            norm1 = nonSpacingMarkRegex.Replace(norm1, string.Empty);

            return norm1;

        }
    }
}



