//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eX_Portal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CommsDetail
    {
        public int CommsID { get; set; }
        public int FromID { get; set; }
        public int ToID { get; set; }
        public int MessageID { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> StatusUpdatedOn { get; set; }
    
        public virtual MSTR_Comms MSTR_Comms { get; set; }
    }
}
