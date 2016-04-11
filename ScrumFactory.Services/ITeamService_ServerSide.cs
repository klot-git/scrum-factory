using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services {

    /// <summary>
    /// Defines methods that can only be called at the server side of the services.
    /// </summary>
    public interface ITeamService_ServerSide : ITeamService {

        ICollection<MemberProfile> GetProjectMembers_skipAuth(string projectUId);

    }
}
