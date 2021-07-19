using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services.AuthProviders {

    public interface IOAuthServerTokenValidator {

        string ProviderName { get; }

        string ValidateUrl { get; }

        bool IsProviderEnabled();
        
        bool ValidateAccessToken(string token);

        MemberProfile MemberInfo { get; }

    }
}
