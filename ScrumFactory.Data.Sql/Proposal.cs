//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScrumFactory.Data.Sql
{
    using System;
    using System.Collections.Generic;
    
    public partial class Proposal
    {
        public Proposal()
        {
            this.Items = new HashSet<ProposalItem>();
            this.Clauses = new HashSet<ProposalClause>();
            this.FixedCosts = new HashSet<ProposalFixedCost>();
        }
    
        public string ProposalUId { get; set; }
        public string ProjectUId { get; set; }
        public string ProposalName { get; set; }
        public short ProposalStatus { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public bool UseCalcPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalValue { get; set; }
        public Nullable<decimal> CurrencyRate { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<short> RejectReason { get; set; }
        public System.DateTime EstimatedStartDate { get; set; }
        public System.DateTime EstimatedEndDate { get; set; }
        public string CurrencySymbol { get; set; }
        public string TemplateName { get; set; }
    
        public virtual ICollection<ProposalItem> Items { get; set; }
        public virtual ProposalDocument ProposalDocument { get; set; }
        public virtual ICollection<ProposalClause> Clauses { get; set; }
        public virtual ICollection<ProposalFixedCost> FixedCosts { get; set; }
    }
}
