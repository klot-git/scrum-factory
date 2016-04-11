using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ScrumFactory.Composition.ViewModel;

namespace ScrumFactory.Composition.ViewModel {
   
    /// <summary>
    /// Base class for a basic Factory Panel ViewModel.
    /// </summary>    
    public class BasePanelViewModel : IChildWindow {

        
        private bool isLoadingData = false;        

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual bool NeedRefresh { get; set; }



        /// <summary>
        /// Gets and Sets whenever the panel is retrieving data from the server.
        /// </summary>
        public bool IsLoadingData {
            get {
                return this.isLoadingData;
            }
            protected set {                
                isLoadingData = value;                
    
                OnPropertyChanged("IsLoadingData");          
            }
        }

    

        private ICollection<object> openedWindows = new ObservableCollection<object>();

        /// <summary>
        /// Gets and sets the child windows of the panel.
        /// </summary>
        public ICollection<object> OpenedWindows {
            get {
                return this.openedWindows;
            }
            private set {
                this.openedWindows = value;
                OnPropertyChanged("OpenedWindows");
            }
        }

        private IChildWindow ParentViewModel;

        public virtual void Show(IChildWindow parent) {
            ParentViewModel = parent;
            ParentViewModel.ShowChildWindow((IViewModel)this);
        }

        public virtual void Close() {
            if (ParentViewModel == null)
                return;
            ParentViewModel.CloseChildWindow((IViewModel)this);

            
        }

        public void ShowChildWindow(IViewModel viewModel) {
            if (OpenedWindows.Contains(viewModel.View))
                return;

            OpenedWindows.Add(viewModel.View);
        }

        public void CloseChildWindow(IViewModel viewModel) {
            OpenedWindows.Remove(viewModel.View);
        }

        /// <summary>
        /// Called when a property has changed.
        /// </summary>
        /// <param name="property">The property.</param>
        protected void OnPropertyChanged(string property) {
            if (this.PropertyChanged != null) 
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));            
        }
               
    }
}
