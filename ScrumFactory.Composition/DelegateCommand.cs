namespace ScrumFactory.Composition {
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand {
        private Action commandAction;
        private Func<bool> canExecuteFunc;

        public DelegateCommand(Action commandAction)
            : this(() => true, commandAction) {
        }

        public DelegateCommand(Func<bool> canExecuteFunc, Action commandAction) {
            this.canExecuteFunc = canExecuteFunc;
            this.commandAction = commandAction;
        }

        public event EventHandler CanExecuteChanged;

        bool ICommand.CanExecute(object parameter) {
            return this.canExecuteFunc();
        }

        public void NotifyCanExecuteChanged() {
            if (this.CanExecuteChanged != null) {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        void ICommand.Execute(object parameter) {
            this.commandAction();
        }
    }

    public class DelegateCommand<T> : ICommand {
        private Action<T> commandAction;
        private Func<bool> canExecuteFunc;

        public DelegateCommand(Action<T> commandAction)
            : this(() => true, commandAction) {
        }

        public DelegateCommand(Func<bool> canExecuteFunc, Action<T> commandAction) {
            this.canExecuteFunc = canExecuteFunc;
            this.commandAction = commandAction;
        }

        public event EventHandler CanExecuteChanged;

        bool ICommand.CanExecute(object parameter) {
            return this.canExecuteFunc();
        }

        public void NotifyCanExecuteChanged() {
            if (this.CanExecuteChanged != null) {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        void ICommand.Execute(object parameter) {
            this.commandAction((T)parameter);
        }
    }
}