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
    
    public partial class PayLoadData
    {
        public int PayLoadDataID { get; set; }
        public string FlightUniqueID { get; set; }
        public string DroneId { get; set; }
        public string RFID { get; set; }
        public string ProductQrCode { get; set; }
        public string RSSI { get; set; }
        public string ReadTime { get; set; }
        public string ReadCount { get; set; }
        public string ReadFreq { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string GPSTime { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<int> FlightID { get; set; }
        public string GPSFix { get; set; }
    }
}