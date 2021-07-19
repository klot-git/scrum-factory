
using System.Collections.Generic;

namespace ScrumFactory.Data {

    public interface IArtifactRepository {

        Artifact GetArtifact(string artifactUId);

        ICollection<Artifact> GetArtifacts(string contextUId);

        void SaveArtifact(Artifact artifact);

        void RemoveArtifact(string artifactUId);

        int GetArtifactContextCount(string contextUId);
    }
}
