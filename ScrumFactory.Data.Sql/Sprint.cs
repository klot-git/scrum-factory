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
    
    public partial class Sprint
    {
        public string SprintUId { get; set; }
        public int SprintNumber { get; set; }
        public string ProjectUId { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        public virtual Project Project { get; set; }
    }
}
