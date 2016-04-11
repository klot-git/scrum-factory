using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition.ViewModel;

namespace ScrumFactory.Composition {

    public interface IDialogService {

        IDialogViewModel NewDialog(string title, ScrumFactory.Composition.View.IView contentView);

        void ShowAlertMessage(string title, string message, string imageSource);

        void CloseAlertMessage();

        void ShowMessageBox(string title, string message);
        
        System.Windows.MessageBoxResult ShowMessageBox(string title, string message, System.Windows.MessageBoxButton buttons);

        System.Windows.MessageBoxResult ShowMessageBox(string title, string message, string imageSource);

        System.Windows.MessageBoxResult ShowMessageBox(string title, string message, System.Windows.MessageBoxButton buttons, string imageSource);

        string ShowOpenFileDialog(string title, string path, string[] customPlaces, bool defaultCustomPlaces);

        string ShowSaveFileDialog(string title, string path, string suggestedFileName, string filter, string[] customPlaces, bool defaultCustomPlaces);

        //ICollection<object> OpenedWindows { get; }

        //void ShowWindow(IViewModel viewModel);

        //void CloseWindow(IViewModel viewModel);


        void SelectTopMenu(ITopMenuViewModel viewModel);

        void GoBackSelectedTopMenu();

        void GoToFirstTopMenu();

        void SetBackTopMenu(ITopMenuViewModel viewModel);

        void SetBackTopMenu();

    }
}
