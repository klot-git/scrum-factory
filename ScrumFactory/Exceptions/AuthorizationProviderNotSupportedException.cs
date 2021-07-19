using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {
    public class AuthorizationProviderNotSupportedException : ScrumFactoryException {

        public AuthorizationProviderNotSupportedException() : base("AuthorizationProviderNotSupportedException") { }
    }
}
