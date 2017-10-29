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
    
    public partial class DroneData
    {
        public int DroneDataId { get; set; }
        public Nullable<int> DroneId { get; set; }
        public string DroneRFID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string ProductRFID { get; set; }
        public string ProductQrCode { get; set; }
        public string ProductRSSI { get; set; }
        public string ReadTime { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<int> RecordType { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string Altitude { get; set; }
        public string Speed { get; set; }
        public string FixQuality { get; set; }
        public string Satellites { get; set; }
        public string Pitch { get; set; }
        public string Roll { get; set; }
        public string Heading { get; set; }
        public string TotalFlightTime { get; set; }
        public string BBFlightID { get; set; }
        public Nullable<int> IsProcessed { get; set; }
        public string QueueMessage { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Voltage { get; set; }
        public Nullable<bool> IsLogData { get; set; }
        public int BlackBoxID { get; set; }
        public int BlackBoxSerial { get; set; }
    }
}
