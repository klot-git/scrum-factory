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
    
    public partial class BacklogItemEffectiveHours
    {
        public string ProjectUId { get; set; }
        public string BacklogItemUId { get; set; }
        public int SprintNumber { get; set; }
        public Nullable<decimal> EffectiveHours { get; set; }
    }
}
