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
    
    public partial class DroneFlight
    {
        public int ID { get; set; }
        public Nullable<int> PilotID { get; set; }
        public Nullable<int> GSCID { get; set; }
        public Nullable<System.DateTime> FlightDate { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> DroneID { get; set; }
        public Nullable<int> BBFlightID { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public string LogFrom { get; set; }
        public string LogTo { get; set; }
        public Nullable<System.DateTime> LogTakeOffTime { get; set; }
        public Nullable<System.DateTime> LogLandingTime { get; set; }
        public string LogBattery1ID { get; set; }
        public Nullable<decimal> LogBattery1StartV { get; set; }
        public Nullable<decimal> LogBattery1EndV { get; set; }
        public string LogBattery2ID { get; set; }
        public Nullable<decimal> LogBattery2StartV { get; set; }
        public Nullable<decimal> LogBattery2EndV { get; set; }
        public Nullable<System.DateTime> LogDateTime { get; set; }
        public Nullable<int> LogCreatedBy { get; set; }
        public string Descrepency { get; set; }
        public string ActionTaken { get; set; }
        public Nullable<byte> IsLogged { get; set; }
        public Nullable<bool> IsFlightOutside { get; set; }
        public string RecordedVideoURL { get; set; }
        public Nullable<int> IsFlightSoftFence { get; set; }
        public Nullable<int> LowerLimit { get; set; }
        public Nullable<int> HigherLimit { get; set; }
    }
}
