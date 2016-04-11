using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;


namespace ScrumFactory {

    [DataContract]
    public class TaskTag {

        [DataMember]
        public string TagUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public int OpenTasksCount { get; set; }
    }
}
