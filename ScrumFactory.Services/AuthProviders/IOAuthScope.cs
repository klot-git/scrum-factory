using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services.AuthProviders {

    public interface IOAuthScope {

        string ScopeName { get; }
        string ProviderName { get; }
    }
}
