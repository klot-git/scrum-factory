using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System;

namespace ScrumFactory.Composition.ViewModel {

    public class BaseEditableObjectViewModel : BasePanelViewModel, IEditableObjectViewModel, IChildWindow, INotifyPropertyChanged, IDisposable {

        public void NotifyAdded() {
            JustHasBeenInserted = true;            
        }

        private System.DateTime insertedAt;
        public bool JustHasBeenInserted {
            get {
                if (System.DateTime.Now.Subtract(insertedAt).Seconds <= 1)
                    return true;
                return false;
                
            }
            private set {
                insertedAt = System.DateTime.Now;                
                OnPropertyChanged("JustHasBeenInserted");
            }
        }

      

        private bool isSelected;
        public virtual bool IsSelected {
            get {
                return isSelected;
            }
            set {
                isSelected = value;                
                OnPropertyChanged("IsSelected");
            }
        }

        public virtual void Dispose() {
            if (isDisposed)
                return;
            this.OnDispose();            
            isDisposed = true;
        }

     

        protected virtual void OnDispose() {
        }

        protected bool isDisposed;

       

    }
}
