using System.ServiceModel;
using System.Collections.Generic;

namespace ScrumFactory.Services {

    
    public interface IProposalsService_ServerSide : IProposalsService {

        decimal GetBudgetIndicator_skipAuth(string projectUId);

    }
}
