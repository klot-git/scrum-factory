using System.Runtime.Serialization;
using System.Collections.Generic;

namespace ScrumFactory {

    public enum ArtifactContexts : short {
        PROJECT_ARTIFACT,
        BACKLOGITEM_ARTIFACT,
        TASK_ARTIFACT,
        PROPOSAL_ARTIFACT
    }

    [DataContract]
    public class Artifact {

        [DataMember]
        public string ArtifactUId { get; set; }

        [DataMember]
        public string ArtifactName { get; set; }

        [DataMember]
        public string ArtifactPath { get; set; }

        [DataMember]
        public short ArtifactContext { get; set; }

        [DataMember]
        public string ContextUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

    }
}
