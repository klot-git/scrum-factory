using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ScrumFactory {

    public enum RiskImpacts : short {
        NONE_IMPACT_RISK,
        LOW_IMPACT_RISK,
        MEDIUM_IMPACT_RISK,
        HIGH_IMPACT_RISK,
    }

    public enum RiskProbabilities : short {
        NONE_PROBABILITY_RISK,
        LOW_PROBABILITY_RISK,
        MEDIUM_PROBABILITY_RISK,
        HIGH_PROBABILITY_RISK,
    }

    [DataContract]
    public class Risk {

        [DataMember]
        public string RiskUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string RiskDescription { get; set; }

        [DataMember]
        public string RiskAction { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public DateTime UpdatedAt { get; set; }

        [DataMember]
        public short Impact { get; set; }

        [DataMember]
        public short Probability { get; set; }

        [DataMember]
        public bool IsPrivate { get; set; }


        public bool IsTheSame(Risk other) {
            if (other == null)
                return false;
            return this.RiskAction == other.RiskAction &&
                this.RiskDescription == other.RiskDescription &&
                this.ProjectUId == other.ProjectUId &&
                this.Impact == other.Impact &&
                this.Probability == other.Probability &&
                this.IsPrivate == other.IsPrivate &&
                this.CreateDate == other.CreateDate;
        }
    }
}
