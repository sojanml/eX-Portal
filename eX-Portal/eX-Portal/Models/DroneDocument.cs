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
    
    public partial class DroneDocument
    {
        public int ID { get; set; }
        public int DroneID { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public Nullable<System.DateTime> UploadedDate { get; set; }
        public Nullable<int> UploadedBy { get; set; }
        public Nullable<int> FlightID { get; set; }
    }
}
