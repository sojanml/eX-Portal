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
    
    public partial class DroneFlightVideo
    {
        public int VideoID { get; set; }
        public int DroneID { get; set; }
        public int FlightID { get; set; }
        public string VideoURL { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> VideoDateTime { get; set; }
        public int IsDeleted { get; set; }
    }
}