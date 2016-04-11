using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Backlog.ViewModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Backlog {

    /// <summary>
    /// Interaction logic for SizeList.xaml
    /// </summary>
    [Export(typeof(SizeList))]
    public partial class SizeList : UserControl, IView  {

        private object model;

        public SizeList() {
            InitializeComponent();
            Loaded += new System.Windows.RoutedEventHandler(SizeList_Loaded);
        }

        void SizeList_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            newSizeMenuItem.Focus();
        }

        #region IItemSizeListView Members

        [Import(typeof(SizeListViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }

        #endregion
    }
}
