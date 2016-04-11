using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Windows.ViewModel;


namespace ScrumFactory.Windows {
    
    [Export(typeof(IDialogService))]
    public class DialogService : IDialogService, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        [Import(typeof(ShellViewModel))]
        Lazy<ShellViewModel> shellViewModel { get; set; }

        [Import(typeof(IEventAggregator))]
        private IEventAggregator aggregator { get; set; }


        public void ShowAlertMessage(string title, string message, string imageSource) {
            shellViewModel.Value.ShowAlertMessage(title, message, imageSource);
        }

        public void CloseAlertMessage() {
            shellViewModel.Value.CloseAlertMessage();
        }
        

        public IDialogViewModel NewDialog(string title, ScrumFactory.Composition.View.IView contentView) {
            MessageBoxViewModel vm = new MessageBoxViewModel(title, new MessageBox(), contentView);            
            return vm;
        }
        
        /// <summary>
        /// Creates and shows a message box dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <returns>The user choice.</returns>
        public void ShowMessageBox(string title, string message) {
            MessageBoxViewModel vm = new MessageBoxViewModel(title, message, System.Windows.MessageBoxButton.OK, null, new MessageBox());
            vm.Show();            
        }


        /// <summary>
        /// Creates and shows a message box dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="buttons">The buttons.</param>
        /// <returns>The user choice.</returns>
        public System.Windows.MessageBoxResult ShowMessageBox(string title, string message, System.Windows.MessageBoxButton buttons) {
            MessageBoxViewModel vm = new MessageBoxViewModel(title, message, buttons, null, new MessageBox());
            vm.Show();
            return vm.Result;
        }

        /// <summary>
        /// Creates and shows a message box dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="imageSource">The image source.</param>
        /// <returns>The user choice.</returns>
        public System.Windows.MessageBoxResult ShowMessageBox(string title, string message, string imageSource) {
            MessageBoxViewModel vm = new MessageBoxViewModel(title, message, System.Windows.MessageBoxButton.OK, imageSource, new MessageBox());
            vm.Show();
            return vm.Result;
        }

        public System.Windows.MessageBoxResult ShowMessageBox(string title, string message, System.Windows.MessageBoxButton buttons, string imageSource) {
            MessageBoxViewModel vm = new MessageBoxViewModel(title, message, buttons, imageSource, new MessageBox());
            vm.Show();
            return vm.Result;
        }


        public string ShowOpenFileDialog(string title, string path, string[] customPlaces, bool defaultCustomPlaces) {

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            //System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = title;
            //dialog.InitialDirectory = path;

            //if(!defaultCustomPlaces)
            //    dialog.CustomPlaces.Clear();
            //foreach (string s in customPlaces)
            //    dialog.CustomPlaces.Add(s);

            bool? d = dialog.ShowDialog();

            if (!d.HasValue || !d.Value)
                return null;

            return dialog.FileName;

        }

        public string ShowSaveFileDialog(string title, string path, string suggestedFileName, string filter, string[] customPlaces, bool defaultCustomPlaces) {

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.FileName = suggestedFileName;
            dialog.Title = title;
            dialog.Filter = filter;
            dialog.AddExtension = true;

            System.Windows.Forms.DialogResult d = dialog.ShowDialog();

            if (d == System.Windows.Forms.DialogResult.Cancel)
                return null;

            return dialog.FileName;

        }

        protected void OnPropertyChanged(string property) {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void SelectTopMenu(ITopMenuViewModel viewModel) {
            if (shellViewModel.Value.SelectedTopMenuItem == viewModel)
                return;
            shellViewModel.Value.SetTopMenu(viewModel, false);
        }

        public void GoBackSelectedTopMenu() {
            if (!shellViewModel.IsValueCreated)
                return;
            shellViewModel.Value.GoBackSelectedTopMenu();         
        }

        public void GoToFirstTopMenu() {
            if (!shellViewModel.IsValueCreated)
                return;
            shellViewModel.Value.GoToFirstTopMenu();
        }

        
        public void SetBackTopMenu() {            
            SetBackTopMenu(shellViewModel.Value.SelectedTopMenuItem);
        }
        public void SetBackTopMenu(ITopMenuViewModel viewModel) {
            if (!shellViewModel.IsValueCreated)
                return;
            if (viewModel == null)
                viewModel = shellViewModel.Value.SelectedTopMenuItem;
            shellViewModel.Value.SetBackTopMenu(viewModel);
        }
    }
}
