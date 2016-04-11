using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services.AuthProviders {

    public interface IWindowsTokenStore {


        string CreateTokenFor(string user);

        MemberProfile ValidateToken(string token);

    }

}
