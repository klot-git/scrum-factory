using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ScrumFactory {

    [DataContract]
    public class AuthorizationInfo  {

        [DataMember]
        public string ProviderName { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string MemberUId { get; set; }

        [DataMember]
        public System.DateTime IssueDate { get; set; }
    }

}
