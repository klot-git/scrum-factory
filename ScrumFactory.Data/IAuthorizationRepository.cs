
using System.Collections.Generic;

namespace ScrumFactory.Data {

    public interface IAuthorizationRepository {

        AuthorizationInfo GetAuthorizationInfo(string token);

        void SaveAuthorizationInfo(AuthorizationInfo info);
        
    }
}
