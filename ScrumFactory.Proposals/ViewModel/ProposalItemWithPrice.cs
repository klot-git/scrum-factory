using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace ScrumFactory.Proposals.ViewModel {

    [DataContract]
    public class ProposalItemWithPrice : ProposalItem {

        [DataMember]
        public decimal Price { get; set; }
        
        public ProposalItemWithPrice() : base() { }

        public ProposalItemWithPrice(string proposalUId, BacklogItem item, decimal value) {
            Price = value;
            BacklogItemUId = item.BacklogItemUId;
            Item = item;
            ProposalUId = ProposalUId;
        }
    }
}
