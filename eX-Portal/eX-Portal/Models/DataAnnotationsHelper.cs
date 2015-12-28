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
        [Display(Name = "Email Address")]
        public string EmailId { get; set; }
        [Display(Name = "Contact Mobile Number")]
        public string MobileNo { get; set; }
        [Display(Name = "Contact Office Number")]
        public string OfficeNo { get; set; }
        [Display(Name = "Active?")]
        public Nullable<bool> IsActive { get; set; }
        [Display(Name = "Description")]
        public string AccountDescription { get; set; }
        [Display(Name = "Address")]
        public string Address1 { get; set; }
        [Display(Name = "Country")]
        public Nullable<int> CountryCode { get; set; }
        [Display(Name = "Internal Reference Code")]
       
        [Required(ErrorMessage = "Please Enter the Company Code.")]
        [StringLength(4, MinimumLength = 4,ErrorMessage = "The field Internal Reference Code must be a string with a  length of 4"  )]
        public string Code { get; set; }
        [Required(ErrorMessage = "Please Enter The Company Name.")]
        public string Name { get; set; }
    }



    //user helper class

    [MetadataType(typeof(MSTR_UserHelper))]
    public partial class MSTR_User { }

    public class MSTR_UserHelper
    {
        [Required(ErrorMessage ="Please Enter the User Name")]
        public string UserName { get; set; }
    }


}