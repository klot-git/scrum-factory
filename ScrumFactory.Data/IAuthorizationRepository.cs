
using System.Collections.Generic;

namespace ScrumFactory.Data {

    public interface IAuthorizationRepository {

        AuthorizationInfo GetAuthorizationInfo(string token, int validPeriod = 0);

        void SaveAuthorizationInfo(AuthorizationInfo info);
        
    }
}
