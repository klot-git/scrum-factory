using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System.Windows.Input;
using System.Linq;
using System;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Windows.ViewModel {

    /// <summary>
    /// Message Box View Model.
    /// </summary>
    public class MessageBoxViewModel : BasePanelViewModel, IDialogViewModel, INotifyPropertyChanged {

        string message;
        string title;
        string imageSource;
        System.Windows.MessageBoxButton buttons;

        public MessageBoxViewModel() {
            ImageSource = "/Images/ErrorStatus/Alert.png";
            ResultButtonPressedCommand = new DelegateCommand<string>(OnResultButtonPressed);
            CloseWindowCommand = new DelegateCommand(Close);
            MoveWindowCommand = new DelegateCommand(() => { View.DragMove(); });            

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxViewModel"/> class.        
        /// </summary>
        /// <remarks>This is a private constructor, use the static method Show to create MessageBox instances.</remarks>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="imageSource">The image source.</param>
        /// <param name="isErrorAlert">if set to <c>true</c> [is error alert].</param>
        public MessageBoxViewModel(string title, string message, System.Windows.MessageBoxButton buttons, string imageSource, IDialogView view) {
            View = view;
            View.Model = this;
            Title = title;
            Message = message;
            Buttons = buttons;
            if (imageSource != null)
                ImageSource = imageSource;
            else
                ResetErrorImage();
            ResultButtonPressedCommand = new DelegateCommand<string>(OnResultButtonPressed);
            CloseWindowCommand = new DelegateCommand(Close);
            MoveWindowCommand = new DelegateCommand(() => { View.DragMove(); });
        }

        public MessageBoxViewModel(string title, IDialogView view, IView contentView) {
            View = view;
            View.Model = this;
            ContentView = contentView;
            Title = title;            
            CloseWindowCommand = new DelegateCommand(Close);
            MoveWindowCommand = new DelegateCommand(() => { View.DragMove(); });
        }

        /// <summary>
        /// Called when the result button pressed.
        /// </summary>
        /// <param name="result">The result.</param>
        private void OnResultButtonPressed(string result) {
            Result = (System.Windows.MessageBoxResult) Enum.Parse(typeof(System.Windows.MessageBoxResult), result);
            Close();
        }


        public void Close() {               
            View.Close();                        
        }

        /// <summary>
        /// Reset the error image to the standart alert sign.
        /// </summary>
        public void ResetErrorImage() {
            ImageSource = "/Images/ErrorStatus/Alert.png";
        }

            


        public IView ContentView { get; set; }

      

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show() {            
            View.Show();
        }


        /// <summary>
        /// Gets the move window command.
        /// </summary>
        /// <value>The move window command.</value>
        public ICommand MoveWindowCommand { get; set; }

        /// <summary>
        /// Gets the close window command.        
        /// </summary>
        /// <remarks>Always returns Null because MessageBox can not be closed. </remarks>
        /// <value>The close window command.</value>
        public ICommand CloseWindowCommand { get; set; }

        /// <summary>
        /// Gets the minimize window command.
        /// </summary>
        /// <remarks>Always returns Null because MessageBox can not be minimized. </remarks>
        /// <value>The minimize window command.</value>
        public ICommand MinimizeWindowCommand {
            get { return null; }
        }

        /// <summary>
        /// Gets the maximize window command.
        /// </summary>
        /// <remarks>Always returns Null because MessageBox can not be maximized. </remarks>
        /// <value>The maximize window command.</value>
        public ICommand MaximizeWindowCommand {
            get { return null; }
        }
        
        

        #region IMessageBoxViewModel Members

        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        public IDialogView View { get; set; }

       

        /// <summary>
        /// Gets the result chossen by the user.
        /// </summary>
        /// <value>The result.</value>
        public System.Windows.MessageBoxResult Result { get; private set; }

        /// <summary>
        /// Gets the title of the message box.
        /// </summary>
        /// <value>The title.</value>
        public string Title {
            get {
                return title;
            }
            set {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Gets the message of the message box.
        /// </summary>
        /// <value>The message.</value>
        public string Message {
            get {
                return message;
            }
            set {
                message = value;
                OnPropertyChanged("Message");
            }
        }

        /// <summary>
        /// Gets the source of the image to be displayed at the message box.
        /// </summary>
        /// <value>The image source.</value>
        public string ImageSource {
            get {
                return imageSource;
            }
            set {
                imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        
        /// <summary>
        /// Gets the buttons that should be displayed at the message box.
        /// </summary>
        /// <value>The buttons.</value>
        public System.Windows.MessageBoxButton Buttons {
            get {
                return buttons;
            }
            set {
                buttons = value;
                OnPropertyChanged("Buttons");
            }
        }


        /// <summary>
        /// Gets the result button pressed command.
        /// </summary>
        /// <value>The result button pressed command.</value>
        public ICommand ResultButtonPressedCommand { get; private set; }

        
        #endregion

    }
}
