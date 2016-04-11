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
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Tasks {


    /// <summary>
    /// Interaction logic for TaskSelector.xaml
    /// </summary>
    [Export]
    public partial class TaskSelector : UserControl, IDialogView {


        private object model;

        public TaskSelector() {
            InitializeComponent();
           
        }



        public void Close() {
            this.Visibility = Visibility.Collapsed;
        }

        public void Show() {
            this.Visibility = Visibility.Visible;   
        }
        
        

        [Import(typeof(ViewModel.UserTasksSelectorViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }


        public void DragMove() {
            throw new NotImplementedException();
        }

        public WindowState WindowState {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
