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
    
    public partial class BacklogItem
    {
        public BacklogItem()
        {
            this.PlannedHours = new HashSet<PlannedHour>();
        }
    
        public string BacklogItemUId { get; set; }
        public int BacklogItemNumber { get; set; }
        public string ProjectUId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Status { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> StartedAt { get; set; }
        public Nullable<System.DateTime> FinishedAt { get; set; }
        public int BusinessPriority { get; set; }
        public Nullable<int> Size { get; set; }
        public string ItemSizeUId { get; set; }
        public short OccurrenceConstraint { get; set; }
        public string GroupUId { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public short IssueType { get; set; }
        public Nullable<short> CancelReason { get; set; }
        public int SizeFactor { get; set; }
        public Nullable<int> ArtifactCount { get; set; }
    
        public virtual ICollection<PlannedHour> PlannedHours { get; set; }
        public virtual BacklogItemGroup Group { get; set; }
    }
}