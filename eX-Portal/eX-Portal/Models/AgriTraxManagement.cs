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
    
    public partial class AgriTraxManagement
    {
        public int AgriTraxID { get; set; }
        public string CustomerReference { get; set; }
        public string AccountNumber { get; set; }
        public Nullable<System.DateTime> DisburementDate { get; set; }
        public Nullable<int> Tenor { get; set; }
        public string FacilityType { get; set; }
        public Nullable<decimal> PrincipalAmount { get; set; }
        public string BranchID { get; set; }
        public string LoanOfficer { get; set; }
        public Nullable<decimal> Lat { get; set; }
        public Nullable<decimal> Lng { get; set; }
        public string LandAddress { get; set; }
        public Nullable<decimal> LandSize { get; set; }
        public Nullable<System.DateTime> SiteVisitDate { get; set; }
        public Nullable<System.DateTime> NextSiteVisitDate { get; set; }
        public string InspectionOfficer { get; set; }
        public string InspectionNote { get; set; }
        public string Images { get; set; }
    }
}
