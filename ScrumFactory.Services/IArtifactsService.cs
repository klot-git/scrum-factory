using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IArtifactsService {

        [OperationContract]
        ICollection<Artifact> GetArtifacts(string contextUId);

        [OperationContract]
        int AddArtifact(Artifact artifact);

        [OperationContract]
        void UpdateArtifact(string contextUId, string artifactUId, Artifact artifact);

        [OperationContract]
        int RemoveArtifact(string contextUId, string artifactUId);
    }
}
