using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory {

    public class ServerConfiguration {

        public string ScrumFactorySenderEmail { get; set; }
        public string ScrumFactorySenderName { get; set; }
        public string DefaultCompanyName  { get; set; }
        public string TrustedEmailDomains { get; set; }

    }
}
