using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eX_Portal.Models
{
    /*
      Reference
      http://stackoverflow.com/questions/12610319/mvc-db-first-fix-display-name
    */

    public class DataAnnotationsHelper
    {
    }

    [MetadataType(typeof(MSTR_AccountHelper))]
    public partial class MSTR_Account { }

    public class MSTR_AccountHelper
    {


        [Required(ErrorMessage = "Please Enter The Company Name")]
        public string Name { get; set; }
        [Display(Name = "Email Address")]
        public string EmailId { get; set; }
        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; }
        [Display(Name = "Office Number")]
        public string OfficeNo { get; set; }
        [Display(Name = "Active?")]
        public Nullable<bool> IsActive { get; set; }
        [Display(Name = "Description")]
        public string AccountDescription { get; set; }
        [Display(Name = "Address")]
        public string Address1 { get; set; }
        [Display(Name = "Country")]
        public Nullable<int> CountryCode { get; set; }
        [Required(ErrorMessage = "Please Enter The Code")]
        [Display(Name = "Internal Reference Code")]
        public string Code { get; set; }
    }




    [MetadataType(typeof(MSTR_UserHelper))]
    public partial class MSTR_User { }
    public class MSTR_UserHelper
    {

        [Required(ErrorMessage = "Please Enter The UserName")]
        public string UserName { get; set; }
    }
}