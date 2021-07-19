using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class ForbittenException : ScrumFactoryException {
        public ForbittenException() : base("ScrumFactory_Exceptions_ForbittenException") { }
    }
}
