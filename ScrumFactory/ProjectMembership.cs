using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace ScrumFactory {

    [DataContract]
    public class ProjectMembership {

        [DataMember]
        public string MemberUId { get; set; }

        [DataMember]
        public string RoleUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public int? DayAllocation { get; set; }

        [DataMember]        
        public MemberProfile Member { get; set; }

        [DataMember]
        public Role Role { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public System.DateTime? InactiveSince { get; set; }

        [DataMember]
        public Project Project { get; set; }
             
        
        public bool IsSame(ProjectMembership other) {
            return (other.MemberUId == MemberUId && other.ProjectUId == ProjectUId && other.RoleUId == RoleUId);
        }
    }

}
