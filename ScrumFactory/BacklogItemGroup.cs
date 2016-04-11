using System.Runtime.Serialization;
using System.Collections.Generic;
using System;


namespace ScrumFactory {


    public enum DefaultItemGroups : short {
        PLAN_GROUP,
        DEV_GROUP,        
        DELIVERY_GROUP
    }

    [DataContract]
    [Serializable]  // for clipboard reasons
    public class BacklogItemGroup {

        [DataMember]
        public string GroupUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string GroupColor { get; set; }

        [DataMember]
        public short DefaultGroup { get; set; }


        //public override bool Equals(object obj) {
        //    BacklogItemGroup g = obj as BacklogItemGroup;
        //    if(g==null)
        //        return false;
        //    return g.GroupUId == this.GroupUId;
        //}
    }
}
