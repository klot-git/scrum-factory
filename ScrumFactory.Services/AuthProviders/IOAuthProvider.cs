using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services.AuthProviders {

    public enum TokenGet {
        SERVER_SIDE,
        CLIENT_SIDE
    }

    public interface IOAuthProvider {

        string ProviderName { get; }

        string LoginUrl { get; }
        
        string ACCESS_TOKEN { get; }

        bool IsSignedIn { get; }

        bool GetAccesToken();        
        bool RefreshAccesToken();

        bool GetAuthorizationToken(Uri url, string title);
        bool GetAuthorizationToken(string user, string pass);

        void SignOut();

        TokenGet LoginType { get; }


    }
}
