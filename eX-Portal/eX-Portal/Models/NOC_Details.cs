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
    
    public partial class NOC_Details
    {
        public int NocID { get; set; }
        public int PilotID { get; set; }
        public int DroneID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public int MinAltitude { get; set; }
        public int MaxAltitude { get; set; }
        public string LOS { get; set; }
        public bool IsUseCamara { get; set; }
    
        public virtual MSTR_NOC MSTR_NOC { get; set; }
    }
}
