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
    
    public partial class MSTR_Account
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string OfficeNo { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> RecordType { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public string AccountDescription { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public Nullable<int> CountryCode { get; set; }
        public string Code { get; set; }
        public string BinaryCode { get; set; }
    }
}
