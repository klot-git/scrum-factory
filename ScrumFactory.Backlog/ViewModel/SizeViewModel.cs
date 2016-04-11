using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Windows.Input;
using System.Linq;


namespace ScrumFactory.Backlog.ViewModel {

    public class SizeViewModel : BaseEditableObjectViewModel, INotifyPropertyChanged {

        
        public SizeViewModel(ItemSize size) {
        
            ItemSize = size;

        }


        public override string ToString() {
            return ItemSize.Name;
        }

        #region IItemSizeViewModel Members

        public ItemSize ItemSize { get; private set; }

        public string SearchName {
            get {
                return ItemSize.Name;
            }
        }

        public int OccurrenceConstraint {
            get {
                return ItemSize.OccurrenceConstraint;
            }
            set {
                ItemSize.OccurrenceConstraint = value;
                OnPropertyChanged("OccurrenceConstraint");
            }
        }

       

        #endregion
    }
}
