using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class NotAuthorizedException : ScrumFactoryException {
        public NotAuthorizedException() : base("The action you are trying to perform requires authentication.") { }
    }
}
