using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel

{
    public class UserLogin
    {
        public int UserId { get; set; }
        [Required(ErrorMessage ="Please enter Username")]
    [Display(Name = "User Name")]
    public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
        public Nullable<bool> RememberMe { get; set; }
        public string PhotoUrl { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<int> UserProfileId { get; set; }
        public Nullable<int> UserAccountId { get; set; }
        public string Remarks { get; set; }
        public string MobileNo { get; set; }
        public string OfficeNo { get; set; }
        public string HomeNo { get; set; }
        public string EmailId { get; set; }
        public Nullable<int> CountryId { get; set; }
        public Nullable<int> RecordType { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
    }
}