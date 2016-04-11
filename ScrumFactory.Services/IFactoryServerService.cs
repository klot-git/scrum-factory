using System.ServiceModel;
using System.Collections.Generic;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IFactoryServerService {

        [OperationContract]
        ServerConfiguration GetConfiguration();

        [OperationContract]
        void UpdateConfiguration(ServerConfiguration config);
    }
}
