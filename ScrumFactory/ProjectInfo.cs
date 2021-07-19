using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;


namespace ScrumFactory {

    [DataContract]
    public class ProjectInfo {

        public ProjectInfo() { }

        public ProjectInfo(Project p) {
            ProjectName = p.ProjectName;
            ProjectNumber = p.ProjectNumber;
            ProjectUId = p.ProjectUId;
            ClientName = p.ClientName;            
        }

        [DataMember]
        public string ProjectName { get;  set; }
        
        [DataMember]
        public string ClientName { get;  set; }
        
        [DataMember]
        public int ProjectNumber { get;  set; }
        
        [DataMember]
        public string ProjectUId { get;  set; }
    }
}
