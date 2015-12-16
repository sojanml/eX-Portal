using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eX_Portal.Models {
  /*
    Reference
    http://stackoverflow.com/questions/12610319/mvc-db-first-fix-display-name
  */

  public class DataAnnotationsHelper {
  }

  [MetadataType(typeof(MSTR_AccountHelper))]
  public partial class MSTR_Account { }

  public class MSTR_AccountHelper {
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
    [Display(Name = "Internal Reference Code")]
    public string Code { get; set; }
  }

}