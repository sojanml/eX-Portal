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
    
    public partial class BlackBoxTransaction
    {
        public int ID { get; set; }
        public int BlackBoxID { get; set; }
        public string BBStatus { get; set; }
        public Nullable<bool> IsCollectionDeposit { get; set; }
        public string CollectionMode { get; set; }
        public string NameOnCard { get; set; }
        public string BankName { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string ChequeNumber { get; set; }
        public Nullable<System.DateTime> DateOfCheque { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<int> DroneID { get; set; }
        public Nullable<int> ApprovalID { get; set; }
    }
}
