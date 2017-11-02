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
    
    public partial class PortalAlert
    {
        public int AlertID { get; set; }
        public Nullable<int> FlightID { get; set; }
        public Nullable<System.DateTime> FlightReadTime { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public Nullable<int> Altitude { get; set; }
        public Nullable<int> FlightDataID { get; set; }
        public string AlertMessage { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> SMSSend { get; set; }
        public string AlertType { get; set; }
        public string AlertCategory { get; set; }
        public Nullable<int> AccountID { get; set; }
        public Nullable<int> ApprovalID { get; set; }
        public Nullable<int> DroneID { get; set; }
        public Nullable<int> PilotID { get; set; }
        public string Proximity { get; set; }
        public int ZoneID { get; set; }
    }
}
