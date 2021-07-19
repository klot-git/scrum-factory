using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class NotFoundException : ScrumFactoryException {
        public NotFoundException() : base("ScrumFactory_Exceptions_NotFoundException") { }
    }
}
