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
    
    public partial class GCA_Approval
    {
        public int ApprovalID { get; set; }
        public string ApprovalName { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Coordinates { get; set; }
        public System.Data.Entity.Spatial.DbGeography Polygon { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> DroneID { get; set; }
        public string ApprovalFileUrl { get; set; }
        public string EndTime { get; set; }
        public string StartTime { get; set; }
        public System.Data.Entity.Spatial.DbGeography InnerBoundary { get; set; }
        public string InnerBoundaryCoord { get; set; }
        public Nullable<int> BoundaryInMeters { get; set; }
        public Nullable<int> MinAltitude { get; set; }
        public Nullable<int> MaxAltitude { get; set; }
        public Nullable<int> MinDefault { get; set; }
        public Nullable<int> MaxDefault { get; set; }
    }
}
