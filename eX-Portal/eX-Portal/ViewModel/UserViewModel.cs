using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using eX_Portal.Models;
using System;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {
    public class UserViewModel
    {
        public MSTR_User User { get; set; }
        public MSTR_User_Pilot Pilot { get; set; }
        public IEnumerable<SelectListItem> ProfileList { get; set; }
        public IEnumerable<SelectListItem> CountryList { get; set; }
        public IEnumerable<SelectListItem> AccountList { get; set; }
        public IEnumerable<SelectListItem> DashboardList { get; set; }
        public IEnumerable<SelectListItem> PermitCategoryList { get; set; }

        //hudha chanded
    }


  public class RPAS_Register {
    public int UserId { get; set; }
    [Required(ErrorMessage = "Please Enter the User Name")]
    [Display(Name = "User Name")]
    public string UserName { get; set; }

    [Required(ErrorMessage ="Please enter Password")]
    public string Password { get; set; }

    [Required(ErrorMessage ="Confirm Password")]
    [System.ComponentModel.DataAnnotations.Compare("Password")]
    public string ConfirmPassword { get; set; }

    public string PhotoUrl { get; set; }

    [Required(ErrorMessage ="Please enter First Name")]
    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [Required(ErrorMessage ="Please enter Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage ="Please enter Mobile Number")]
    public string MobileNo { get; set; }
    
    [Required(ErrorMessage ="Please enter Email")]
    [EmailAddress]
    public string EmailId { get; set; }

    [Required(ErrorMessage = "Please select Nationality")]
    public int CountryId { get; set; }

    [Required(ErrorMessage ="Please enter RPAS Permit Number")]
    public string RPASPermitNo { get; set; }

    [Required(ErrorMessage ="Please enter Permit Category")]
    public string PermitCategory { get; set; }

    [Required(ErrorMessage ="Please enter Contact Address")]
    public string ContactAddress { get; set; }

    [Required(ErrorMessage ="Please enter RPAS Serial Number")]
    public string RegRPASSerialNo { get; set; }

    [Required(ErrorMessage ="Please enter Company Address")]
    public string CompanyAddress { get; set; }

    [Required(ErrorMessage ="Please enter Company Telephone")]
    public string CompanyTelephone { get; set; }

    [Required(ErrorMessage ="Please enter Company Email")]
    [EmailAddress]
    public string CompanyEmail { get; set; }
    public string TradeLicenceCopyUrl { get; set; }
    [Required(ErrorMessage ="Please enter Emirates ID")]
    public string EmiratesID { get; set; }
    [Required(ErrorMessage ="Please enter Home Number")]
    public string HomeNo { get; set; }

  }

}