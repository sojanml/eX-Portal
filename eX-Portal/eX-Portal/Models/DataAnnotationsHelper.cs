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
    [StringLength(4, MinimumLength = 4, ErrorMessage = "The field Internal Reference Code must be a string with a  length of 4")]
    public string Code { get; set; }
    [Required(ErrorMessage = "Please Enter The Company Name.")]
    public string Name { get; set; }
    [Display(Name = "Contact Name")]
    public string ContactName { get; set; }
    [Display(Name = "Contact Title")]
    public string ContactTitle { get; set; }

  }//MSTR_AccountHelper



  //user helper class

  [MetadataType(typeof(MSTR_UserHelper))]
  public partial class MSTR_User {
    public string ConfirmPassword { get; set; }
  }

  public class MSTR_UserHelper {
    [Required(ErrorMessage = "Please Enter the User Name")]
    [Display(Name = "User Name")]
    public string UserName { get; set; }

    [Compare("Password")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
    
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string LastName { get; set; }


    [Required(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string EmailId { get; set; }

    [Display(Name = "Account/Company")]
    [Required(ErrorMessage = "Account must be selected.")]
    public string AccountId { get; set; }

    [Display(Name = "Profile Selector")]
    [Required(ErrorMessage = "Profile is requried to create the user.")]    
    public string UserProfileId { get; set; }

    [Display(Name = "Is Active?")]
    public string IsActive { get; set; }

    [Display(Name = "Mobile Number")]
    public string MobileNo { get; set; }
    [Display(Name = "Office Number")]
    public string OfficeNo { get; set; }
    [Display(Name = "Home Number")]
    public string HomeNo { get; set; }

    [Display(Name = "Country")]
    public string CountryId { get; set; }
  }//MSTR_UserHelper


}