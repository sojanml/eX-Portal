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
    [Required]
    public string Password { get; set; }
    [Required]
    [System.ComponentModel.DataAnnotations.Compare("Password")]
    public string ConfirmPassword { get; set; }
    public string PhotoUrl { get; set; }
    [Required]
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string MobileNo { get; set; }
    
    [Required]
    [EmailAddress]
    public string EmailId { get; set; }
    [Required(ErrorMessage = "Please select Nationality")]
    public int CountryId { get; set; }

    [Required]
    public string RPASPermitNo { get; set; }
    [Required]
    public string PermitCategory { get; set; }
    [Required]
    public string ContactAddress { get; set; }
    [Required]
    public string RegRPASSerialNo { get; set; }
    [Required]
    public string CompanyAddress { get; set; }
    [Required]
    public string CompanyTelephone { get; set; }
    [Required]
    [EmailAddress]
    public string CompanyEmail { get; set; }
    public string TradeLicenceCopyUrl { get; set; }
    [Required]
    public string EmiratesID { get; set; }
    [Required]
    public string HomeNo { get; set; }

  }

}