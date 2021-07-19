using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScrumFactory
{
    [DataContract]
    public class PokerCard
    {

        [DataMember]
        public string BacklogItemUId { get; set; }

        [DataMember]
        public DateTime VoteDate { get; set; }

        [DataMember]
        public Int16? Value { get; set; }

        [DataMember]
        public string MemberUId { get; set; }

        [DataMember]
        public bool IsFaceDown { get; set; }
    }
}
