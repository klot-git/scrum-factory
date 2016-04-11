using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ScrumFactory;
using ScrumFactory.Composition;
using System.ComponentModel;
using Moq;


namespace ScrumFactory.Test {


    /// <summary>
    /// This emulates a UserControl as a view.
    /// </summary>
    public class FakeView: ScrumFactory.Composition.View.IView {

        private Dictionary<string, ICommand> commandBinds = new Dictionary<string, ICommand>();
        private object model;

        /// <summary>
        /// The View Model.
        /// Once it is set, binds all of its commands.
        /// </summary>
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                if (model == null)
                    return;

                BindAllCommands();
                ((INotifyPropertyChanged)model).PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(view_PropertyChanged);
            }
        }

        public bool IsVisible {
            get {
                return true;
            }
        }

        /// <summary>
        /// Emulates the bind of all viewmodel commands.
        /// </summary>
        public void BindAllCommands() {
            foreach (PropertyInfo cmdInfo in GetViewModelCommands()) {
                ICommand cmd = (ICommand)cmdInfo.GetValue(model, null);
                if (cmd != null)
                    cmd.CanExecuteChanged += new EventHandler(View_CanExecuteChanged);
                commandBinds.Add(cmdInfo.Name, cmd);
            }
        }

        /// <summary>
        /// Returns all view model commands.
        /// </summary>
        /// <returns></returns>
        private PropertyInfo[] GetViewModelCommands() {
            Type vmType = model.GetType();
            PropertyInfo[] props = vmType.GetProperties();
            return props.Where(p => p.PropertyType == typeof(ICommand)).ToArray();
        }


        /// <summary>
        /// Verify if the property changed is a command, and if it is set to null, clean its
        /// reference from view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void view_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {

            // get the property changed
            Type objType = sender.GetType();
            PropertyInfo propInfo = objType.GetProperty(e.PropertyName);

            if (propInfo == null)
                return;

            // if the property is a command
            if (commandBinds.ContainsKey(propInfo.Name)) {

                // and its sets to null, unbind the command
                ICommand cmd = commandBinds[propInfo.Name];
                object propNewValue = propInfo.GetValue(sender, null);
                if (cmd!=null && propNewValue == null) 
                    cmd.CanExecuteChanged -= View_CanExecuteChanged;                                    
                commandBinds.Remove(propInfo.Name);
            }            
        }

        private void View_CanExecuteChanged(object sender, EventArgs e) {
            return;
        }
    }
}
