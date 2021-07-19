using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ScrumFactory {

    public enum ContraintGroups : short {
        BUSINESS_CONSTRAINT,
        DATABASE_CONSTRAINT,
        BROWSER_CONSTRAINT,        
        DEVICE_CONSTRAINT,        
        ENVIRONMENT_CONSTRAINT
    }

    [DataContract]
    public class ProjectConstraint {

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string ConstraintUId { get; set; }

        [DataMember]
        public string ConstraintId { get; set; }

        [DataMember]
        public string Constraint { get; set; }

        [DataMember]
        public short ConstraintGroup { get; set; }

        [DataMember]
        public double AdjustPointFactor { get; set; }

        public string ConstraintGroupName {
            get {
                return System.Enum.GetName(typeof(ContraintGroups), ConstraintGroup);
            }
        }
        
    }
}
