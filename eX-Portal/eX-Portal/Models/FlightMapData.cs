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
    
    public partial class FlightMapData
    {
        public int FlightMapDataID { get; set; }
        public Nullable<int> DroneId { get; set; }
        public string DroneRFID { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public string ProductRFID { get; set; }
        public string ProductQrCode { get; set; }
        public string ProductRSSI { get; set; }
        public Nullable<System.DateTime> ReadTime { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<int> RecordType { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<decimal> Altitude { get; set; }
        public Nullable<decimal> Speed { get; set; }
        public Nullable<decimal> FixQuality { get; set; }
        public Nullable<int> Satellites { get; set; }
        public Nullable<decimal> Pitch { get; set; }
        public Nullable<decimal> Roll { get; set; }
        public Nullable<decimal> Heading { get; set; }
        public Nullable<int> TotalFlightTime { get; set; }
        public Nullable<int> FlightID { get; set; }
        public string BBFlightID { get; set; }
        public Nullable<decimal> avg_Altitude { get; set; }
        public Nullable<decimal> Min_Altitude { get; set; }
        public Nullable<decimal> Max_Altitude { get; set; }
        public Nullable<decimal> Avg_Speed { get; set; }
        public Nullable<decimal> Min_Speed { get; set; }
        public Nullable<decimal> Max_Speed { get; set; }
        public Nullable<decimal> Avg_Satellites { get; set; }
        public Nullable<decimal> Min_Satellites { get; set; }
        public Nullable<decimal> Max_Satellites { get; set; }
        public Nullable<decimal> PointDistance { get; set; }
        public Nullable<decimal> Distance { get; set; }
        public Nullable<bool> IsChecked { get; set; }
        public Nullable<bool> IsOutSide { get; set; }
        public Nullable<int> GCAID { get; set; }
        public Nullable<int> IsSoftFence { get; set; }
    }
}
