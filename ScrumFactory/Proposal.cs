using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;


namespace ScrumFactory {

    public enum ProposalStatus : short {
        PROPOSAL_WAITING,
        PROPOSAL_APPROVED,
        PROPOSAL_REJECTED
    }

    public enum ProposalRejectReasons : short {
        REJECTED_BY_UNKNOW,
        REJECTED_BY_PRICE,
        REJECTED_BY_DEADLINE,
        REJECTED_BY_CLIENT_CANCEL,
        REJECTED_BY_OTHER_PROPOSAL
    }

    [DataContract]
    public class Proposal {

      
        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string ProposalUId { get; set; }

        [DataMember]
        public string ProposalName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public short ProposalStatus { get; set; }

        [DataMember]
        public System.DateTime CreateDate { get; set; }

        [DataMember]
        public System.DateTime? ApprovalDate { get; set; }

        [DataMember]
        public string ApprovedBy { get; set; }

        [DataMember]
        public bool UseCalcPrice { get; set; }

        [DataMember]
        public decimal TotalValue { get; set; }

        [DataMember]
        public decimal Discount { get; set; }

        [DataMember]
        public List<ProposalItem> Items { get; set; }

        [DataMember]
        public List<ProposalClause> Clauses { get; set; }

        [DataMember]
        public List<ProposalFixedCost> FixedCosts { get; set; }

        [DataMember]
        public short? RejectReason { get; set; }

        [DataMember]
        public System.DateTime EstimatedStartDate { get; set; }

        [DataMember]
        public System.DateTime EstimatedEndDate { get; set; }

        [DataMember]
        public string CurrencySymbol { get; set; }

        [DataMember]
        public ProposalDocument ProposalDocument { get; set; }

        [DataMember]
        public string TemplateName { get; set; }

        [DataMember]
        public decimal? CurrencyRate { get; set; }
    
        public decimal CalcItemsPrice(RoleHourCost[] roleHourCosts) {
            decimal? price = 0;
            if (Items == null || roleHourCosts == null)
                return 0;
            foreach (ProposalItem pItem in Items)
                price = price + CalcItemPrice(pItem.Item, roleHourCosts);

            if (!price.HasValue)
                return 0;

            return price.Value;
        }

        public decimal CalcItemPrice(BacklogItem item, RoleHourCost[] roleHourCosts) {
            if (item == null)
                return 0;
            decimal? price = 0;
            foreach (PlannedHour h in item.PlannedHours) {
                RoleHourCost roleCost = roleHourCosts.SingleOrDefault(r => r.RoleUId == h.RoleUId);
                if (roleCost != null)
                    price = price + h.Hours * roleCost.Price;
            }
            if (!price.HasValue)
                return 0;
            return price.Value;
        }

        public decimal CalcFixedCosts() {
            if (FixedCosts == null)
                return 0;
            return FixedCosts.Where(f => f.RepassToClient).Sum(f => f.Cost);
        }

        public decimal CalcTotalPrice(RoleHourCost[] roleHourCosts) {
            if (!UseCalcPrice)
                return TotalValue;
            decimal price = CalcItemsPrice(roleHourCosts);
            price = price + CalcFixedCosts();
            if (Discount == 0)
                return price;
            else
                return price * (1 - (Discount / 100));
        }

        public bool IsTheSame(Proposal other) {
            return
                this.ProjectUId == other.ProjectUId
                && this.ProposalUId == other.ProposalUId
                && this.ProposalName == other.ProposalName
                && this.ProposalStatus == other.ProposalStatus
                && this.TotalValue == other.TotalValue
                && this.TemplateName == other.TemplateName
                && this.UseCalcPrice == other.UseCalcPrice
                && this.Description == other.Description
                && this.Discount == other.Discount
                && this.CreateDate == other.CreateDate
                && this.ApprovalDate == other.ApprovalDate
                && this.EstimatedEndDate == other.EstimatedEndDate
                && this.EstimatedStartDate == other.EstimatedStartDate
                && this.CurrencySymbol == other.CurrencySymbol
                && this.CurrencyRate == other.CurrencyRate
                && this.HasTheSameFixedCosts(other)
                && this.HasTheSameItems(other)
                && this.HasTheSameClauses(other);                
        }

        public bool HasTheSameItems(Proposal other) {
            if (this.Items == null)
                this.Items = new List<ProposalItem>();
            if (other.Items == null)
                other.Items = new List<ProposalItem>();
            return this.Items.All(i => other.Items.Any(oi => oi.BacklogItemUId == i.BacklogItemUId))
                && other.Items.All(oi => this.Items.Any(i => oi.BacklogItemUId == i.BacklogItemUId));            
        }

        public bool HasTheSameFixedCosts(Proposal other) {
            if (this.FixedCosts == null)
                this.FixedCosts = new List<ProposalFixedCost>();
            if (other.FixedCosts == null)
                other.FixedCosts = new List<ProposalFixedCost>();
            return this.FixedCosts.All(i => other.FixedCosts.Any(oi => oi.ProposalFixedCostUId == i.ProposalFixedCostUId && oi.Cost == i.Cost && oi.CostDescription == oi.CostDescription && oi.RepassToClient == i.RepassToClient))
                && other.FixedCosts.All(oi => this.FixedCosts.Any(i => oi.ProposalFixedCostUId == i.ProposalFixedCostUId && oi.Cost == i.Cost && oi.CostDescription == oi.CostDescription && oi.RepassToClient == i.RepassToClient));
        }

        public bool HasTheSameClauses(Proposal other) {
            if (this.Clauses == null)
                this.Clauses = new List<ProposalClause>();
            if (other.Clauses == null)
                other.Clauses = new List<ProposalClause>();
            return this.Clauses.All(i => other.Clauses.Any(oi => oi.ClauseName == i.ClauseName && oi.ClauseText==i.ClauseText && oi.ClauseOrder==i.ClauseOrder))
                && other.Clauses.All(oi => this.Clauses.Any(i => oi.ClauseName == i.ClauseName && oi.ClauseText == i.ClauseText && oi.ClauseOrder == i.ClauseOrder));
        }
        
        public void SetBacklogItems(ICollection<BacklogItem> items) {                
            foreach (ProposalItem pItem in Items)
                pItem.Item = items.SingleOrDefault(i => i.BacklogItemUId == pItem.BacklogItemUId);
            
        }


    }

    [DataContract]
    public class ProposalClause {

        [DataMember]
        public string ProposalUId { get; set; }

        [DataMember]
        public int ClauseOrder { get; set; }

        [DataMember]
        public string ClauseName { get; set; }

        [DataMember]
        public string ClauseText { get; set; }
    }

    [DataContract]
    public class ProposalFixedCost {

        [DataMember]
        public string ProposalFixedCostUId { get; set; }

        [DataMember]
        public string ProposalUId { get; set; }

        [DataMember]
        public string CostDescription { get; set; }

        [DataMember]
        public decimal Cost { get; set; }

        [DataMember]
        public bool RepassToClient { get; set; }
    }

    [DataContract]
    public class ProposalDocument {

        [DataMember]
        public string ProposalUId { get; set; }

        [DataMember]
        public string ProposalXAML { get; set; }

    }

    [DataContract]
    public class ProposalItem {

        [DataMember]
        public string ProposalUId { get; set; }

        [DataMember]
        public string BacklogItemUId { get; set; }

        public BacklogItem Item { get; set; }

       

    }

    [DataContract]
    public class RoleHourCost {

        [DataMember]
        public string RoleUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public decimal Cost { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        public Role Role { get; set; }

    }

}
