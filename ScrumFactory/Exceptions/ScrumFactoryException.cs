using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class ScrumFactoryException : Exception {

        public ScrumFactoryException(string message) : base(message) {}
        
        public string LocalizedMessage { get; private set; }
        public string LocalizedMessageTitle { get; private set; }

        public bool IsLocalized {
            get {
                return (LocalizedMessage!=null);
            }
        }

        public virtual void LocalizeException(Type type) {            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(type);
            System.Resources.ResourceManager r = new System.Resources.ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
            LocalizeException(r);
        }

        public virtual void LocalizeException(System.Resources.ResourceManager r) {
            string errorMessage = null;
            try {
                errorMessage = r.GetString(Message + "_errorMessage");
            }
            catch (Exception) { }

            if (errorMessage != null)
                LocalizedMessage = errorMessage;
            
            string errorMessageTitle = null;
            try {
                errorMessageTitle = r.GetString(Message + "_errorMessageTitle");
            } 
            catch (Exception) { }

            if (errorMessageTitle != null)
                LocalizedMessageTitle = errorMessageTitle;
            else
                LocalizedMessageTitle = Message;

        }

        
            
    }
}
