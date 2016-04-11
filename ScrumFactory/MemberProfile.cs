using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace ScrumFactory {

    [DataContract]
    public class MemberProfile {

        [DataMember]
        public string MemberUId { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public string Skills { get; set; }

        [DataMember]
        public string EmailAccount { get; set; }

        [DataMember]
        public string ContactData { get; set; }

        [DataMember]
        public string AuthorizationProvider { get; set; }

        [DataMember]
        public string CreateBy { get; set; }

        [DataMember]
        [XmlIgnore]
        public List<ProjectMembership> Memberships { get; set;}

        [DataMember]
        public bool IsFactoryOwner { get; set; }

        [DataMember]
        public bool CanSeeProposalValues { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string TeamCode { get; set; }

        [DataMember]
        [XmlIgnore]
        public ICollection<Task> OpenTasks { get; set; }

        public bool IsContactMember {
            get {
                if (AuthorizationProvider == null)
                    return false;
                return AuthorizationProvider.Equals("Factory Contact");
            }
        }

        public int DayOccupation {
            get {
                if (Memberships == null)
                    return 0;
                int? oc = Memberships.Where(ms => ms.IsActive).Sum(ms => ms.DayAllocation);
                if (oc == null)
                    return 0;
                if (oc > 4)
                    return 4;
                return (int)oc;
            }
        }

        public string MemberAvatarUrl { get; set; }

        public bool IsSignedMember { get; set; }

        [DataMember]
        public decimal? PlannedHoursForToday { get; set; }
        
        public bool IsTodayOverPlanned {
            get {
                if (!PlannedHoursForToday.HasValue)
                    return false;
                return PlannedHoursForToday > 8;
            }
        }
        public bool IsTodayHalfPlanned {
            get {
                if (!PlannedHoursForToday.HasValue)
                    return false;
                if (PlannedHoursForToday < 0)
                    return true;
                return PlannedHoursForToday >= 4 && PlannedHoursForToday <= 8;
            }
        }

        public bool IsTheSame(MemberProfile other) {
            if (other == null)
                return false;
            return 
                this.FullName==other.FullName &&
                this.EmailAccount==other.EmailAccount &&
                this.ContactData==other.ContactData &&
                this.CompanyName==other.CompanyName &&
                this.CanSeeProposalValues==other.CanSeeProposalValues &&
                this.AuthorizationProvider==other.AuthorizationProvider &&
                this.DayOccupation==other.DayOccupation &&
                this.IsFactoryOwner==other.IsFactoryOwner &&
                this.Skills==other.Skills &&
                this.CreateBy==other.CreateBy &&
                this.MemberUId==other.MemberUId;
                
        }

        [DataMember]
        public MemberPerformance Performance { get; set; }

        ~MemberProfile() {
            System.Console.Out.WriteLine("***< member died here");
        }

    }

   

    [DataContract]
    public class MemberAvatar {

        [DataMember]
        public string MemberUId { get; set; }

        [DataMember]
        public byte[] AvatarImage { get; set; }


    }
}
