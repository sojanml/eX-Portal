using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using eX_Portal.Models;
using System;
using System.Web.Mvc;
using System.Linq;

namespace eX_Portal.ViewModel {

  public class PilotCreateModel: PilotBaseModel {

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password character should be minimum 6")]
    public string Password { get; set; }

    [System.ComponentModel.DataAnnotations.Compare("Password")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

  }

  public class PilotEditModel : PilotBaseModel {

    public PilotEditModel() {

    }

    public PilotEditModel(int UserID) {
      ExponentPortalEntities db = new ExponentPortalEntities();

      MSTR_User mSTR_User = db.MSTR_User.Find(UserID);
      if (mSTR_User == null) {
        this.UserId = 0;
        return;
      }
      this.UserId = mSTR_User.UserId;
      this.UserName = mSTR_User.UserName;
      this.PhotoUrl = mSTR_User.PhotoUrl;
      this.FirstName = mSTR_User.FirstName;
      this.MiddleName = mSTR_User.MiddleName;
      this.LastName = mSTR_User.LastName;
      this.Remarks = mSTR_User.Remarks;
      this.MobileNo = mSTR_User.MobileNo;
      this.HomeNo = mSTR_User.HomeNo;
      this.EmailId = mSTR_User.EmailId;
      this.CountryId = mSTR_User.CountryId;
      this.RPASPermitNo = mSTR_User.RPASPermitNo;
      this.PermitCategory = mSTR_User.PermitCategory;
      this.ContactAddress = mSTR_User.ContactAddress;
      this.RegRPASSerialNo = mSTR_User.RegRPASSerialNo;
      this.EmiratesID = mSTR_User.EmiratesID;
      this.DOI_RPASPermit = mSTR_User.DOI_RPASPermit;
      this.DOE_RPASPermit = mSTR_User.DOE_RPASPermit;
      this.UserDescription = mSTR_User.UserDescription;
      this.Nationality = mSTR_User.Nationality;
            this.AccountID = mSTR_User.AccountId.Value;
      MSTR_User_Pilot mSTR_User_Pilot = db.MSTR_User_Pilot.Find(UserId);
      if(mSTR_User_Pilot != null) { 
        this.Department = mSTR_User_Pilot.Department;
        this.PassportNo = mSTR_User_Pilot.PassportNo;
        this.DateOfExpiry = mSTR_User_Pilot.DateOfExpiry;
                this.EmiratesID = mSTR_User_Pilot.EmiratesId;
            }

      this.LinkedDroneID = db.M2M_Drone_User
        .Where(w => w.UserID == UserId)
        .Select(s => s.DroneID)
        .ToList();

    }

    //Not required password
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password character should be minimum 6")]
    public string Password { get; set; }

    [System.ComponentModel.DataAnnotations.Compare("Password")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }


  }

  public class PilotBaseModel {



    public int UserId { get; set; }
    public int AccountID { get; set; }

        [Required(ErrorMessage = "Please Enter the User Name")]
    [Display(Name = "User Name")]
    public string UserName { get; set; }


    public string PhotoUrl { get; set; }
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    [Display(Name = "Middle Name")]
    public string MiddleName { get; set; }
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    public string Remarks { get; set; }
    [Required]
    [Display(Name = "Mobile Number")]
    public string MobileNo { get; set; }
    [Display(Name = "Office Number")]
    public string OfficeNo { get; set; }
    [Display(Name = "Home Number")]
    public string HomeNo { get; set; }

    [Required]
    [Display(Name = "Email Address")]
    [EmailAddress]
    public string EmailId { get; set; }
    public Nullable<int> CountryId { get; set; }

    [Required]
    [Display(Name = "RPAS Permit Number")]
    public string RPASPermitNo { get; set; }
    [Required]
    [Display(Name = "Permit Category")]
    public string PermitCategory { get; set; }
    public string ContactAddress { get; set; }
    public string RegRPASSerialNo { get; set; }
    [Required]
    [Display(Name = "Emirates ID Number")]
    public string EmiratesID { get; set; }
    [Required]
    [Display(Name = "Date of Issue")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public System.DateTime? DOI_RPASPermit { get; set; }
    [Required]
    [Display(Name = "Date of Expiary")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public System.DateTime? DOE_RPASPermit { get; set; }
    public string UserDescription { get; set; }
    [Required]
    public string Nationality { get; set; }

    //Pilot Information
    [Display(Name = "Passport Number")]
    public string PassportNo { get; set; }
    [Display(Name = "Date of Expiary")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public System.DateTime? DateOfExpiry { get; set; }
    public string Department { get; set; }

    public IList<int> LinkedDroneID { get; set; }


    public String PhotoUrlWithPath {
      get {
        if (!String.IsNullOrEmpty(PhotoUrl)) {
          var FullURL = $"/Upload/User/{UserId}/{PhotoUrl}";
          var FPath = System.Web.Hosting.HostingEnvironment.MapPath(FullURL);
          if (System.IO.File.Exists(FPath))
            return FullURL;
        }
        return "/images/PilotImage.png";
      }//get
    }//GetPhotoUrl
  }

  public class UserViewModel {
    public MSTR_User User { get; set; }
    public MSTR_User_Pilot Pilot { get; set; }
    public IEnumerable<SelectListItem> ProfileList { get; set; }
    public IEnumerable<SelectListItem> CountryList { get; set; }
    public IEnumerable<SelectListItem> AccountList { get; set; }
    public IEnumerable<SelectListItem> DashboardList { get; set; }
    public IEnumerable<SelectListItem> PermitCategoryList { get; set; }
  }


  public class RPAS_Register {
    public int UserId { get; set; }
    [Required(ErrorMessage = "Please Enter the User Name")]
    [Display(Name = "User Name")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Please enter Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password")]
    [System.ComponentModel.DataAnnotations.Compare("Password")]
    public string ConfirmPassword { get; set; }

    public string PhotoUrl { get; set; }

    [Required(ErrorMessage = "Please enter First Name")]
    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [Required(ErrorMessage = "Please enter Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Please enter Mobile Number")]
    public string MobileNo { get; set; }

    [Required(ErrorMessage = "Please enter Email")]
    [EmailAddress]
    public string EmailId { get; set; }

    [Required(ErrorMessage = "Please select Nationality")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Please enter RPAS Permit Number")]
    public string RPASPermitNo { get; set; }

    [Required(ErrorMessage = "Please enter Permit Category")]
    public string PermitCategory { get; set; }

    [Required(ErrorMessage = "Please enter Contact Address")]
    public string ContactAddress { get; set; }

    [Required(ErrorMessage = "Please enter RPAS Serial Number")]
    public string RegRPASSerialNo { get; set; }

    [Required(ErrorMessage = "Please enter Company Address")]
    public string CompanyAddress { get; set; }

    [Required(ErrorMessage = "Please enter Company Telephone")]
    public string CompanyTelephone { get; set; }

    [Required(ErrorMessage = "Please enter Company Email")]
    [EmailAddress]
    public string CompanyEmail { get; set; }
    public string TradeLicenceCopyUrl { get; set; }
    [Required(ErrorMessage = "Please enter Emirates ID")]
    public string EmiratesID { get; set; }
    [Required(ErrorMessage = "Please enter Home Number")]
    public string HomeNo { get; set; }

  }

}