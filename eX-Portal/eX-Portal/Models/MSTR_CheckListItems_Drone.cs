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
    
    public partial class MSTR_CheckListItems_Drone
    {
        public int ID { get; set; }
        public int CheckList_DroneID { get; set; }
        public Nullable<decimal> SlNo { get; set; }
        public string Title { get; set; }
        public string FieldType { get; set; }
        public string Responsibility { get; set; }
    }
}
