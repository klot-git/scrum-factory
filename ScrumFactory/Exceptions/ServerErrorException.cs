using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {
    public class ServerErrorException : ScrumFactoryException {

        public ServerErrorException() : base("ScrumFactory_Exceptions_ServerErrorException") { }
        public ServerErrorException(string message) : base(message) { }

    }


}
