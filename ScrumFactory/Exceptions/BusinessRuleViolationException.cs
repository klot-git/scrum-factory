using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class BusinessRuleViolationException : ScrumFactoryException {

        public BusinessRuleViolationException(string message) : base(message) {}
        
    }
}
