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
        [Required(ErrorMessage = "Please Enter The Country Name.")]
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
    public partial class MSTR_User
    {
        public string ConfirmPassword { get; set; }
    }

    public class MSTR_UserHelper
    {
        [Required(ErrorMessage = "Please Enter the User Name")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
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
        [Required]
        public string IsActive { get; set; }

        [Display(Name = "Is Pilot?")]
        [Required]
        public string IsPilot { get; set; }


        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; }
        [Display(Name = "Office Number")]
        public string OfficeNo { get; set; }
        [Display(Name = "Home Number")]
        public string HomeNo { get; set; }

        [Display(Name = "Country")]
        public string CountryId { get; set; }
    }//MSTR_UserHelper



    //MSTR_User_Pilot_Certification helper class

    [MetadataType(typeof(MSTR_User_Pilot_CertificationHelper))]
    public partial class MSTR_User_Pilot_Certification
    {

    }

    public class MSTR_User_Pilot_CertificationHelper
    {
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        [Display(Name = "Certificate Name")]
        public Nullable<int> CertificateId { get; set; }
        [Display(Name = "Issuing Authority")]
        public Nullable<int> IssuingAuthorityId { get; set; }
        [Display(Name = "Date Of Issue")]
        public Nullable<System.DateTime> DateOfIssue { get; set; }
        [Display(Name = "Date Of Expiry")]
        public Nullable<System.DateTime> DateOfExpiry { get; set; }
        [Display(Name = "Next Renewal")]
        public Nullable<System.DateTime> NextRenewal { get; set; }
        public Nullable<decimal> Score { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    }



    //MSTR_User_Pilot_ExponentUAS helper class

    [MetadataType(typeof(MSTR_User_Pilot_ExponentUASHelper))]
    public partial class MSTR_User_Pilot_ExponentUAS
    {

    }

    public class MSTR_User_Pilot_ExponentUASHelper
    {
        [Display(Name = "Certificate Name")]
        public Nullable<int> CertificateId { get; set; }
        [Display(Name = "Date Of Enrollement")]
        public Nullable<System.DateTime> DateOfEnrollement { get; set; }
        [Display(Name = "Date Of Certification")]
        public Nullable<System.DateTime> DateOfCertification { get; set; }
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<decimal> Score { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
    }



    //MSTR_User_Pilot_ExponentUAS helper class

    [MetadataType(typeof(MSTR_User_PilotHelper))]
    public partial class MSTR_User_Pilot
    {

    }

    public class MSTR_User_PilotHelper
    {

        [Display(Name = "Passport No")]
        public string PassportNo { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Date Of Expiry")]
        public Nullable<System.DateTime> DateOfExpiry { get; set; }
        public string Department { get; set; }
        [Display(Name = "Emirates ID")]
        public string EmiratesId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
    }



    [MetadataType(typeof(MSTR_Pilot_LogHelper))]
    public partial class MSTR_Pilot_Log
    {

    }

    public class MSTR_Pilot_LogHelper
    {


        public int Id { get; set; }
        [Display(Name = "UAS Name")]

        public Nullable<int> DroneId { get; set; }
        [Required(ErrorMessage = "Please Enter the Date")]
        public Nullable<System.DateTime> Date { get; set; }
        [Display(Name = "From")]
        public string RouteFrom { get; set; }
        [Display(Name = "To")]
        public string RouteTo { get; set; }
        public string Remarks { get; set; }
        [Display(Name = "Fixed Wing")]
        public Nullable<int> FixedWing { get; set; }
        [Display(Name = "Multi Dash Rotor")]
        public Nullable<int> MultiDashRotor { get; set; }
        [Display(Name = "Simulaor")]
        public Nullable<int> SimulatedInstrument { get; set; }
        [Display(Name = "As Flight Instructor")]
        public Nullable<int> AsflightInstructor { get; set; }
        [Display(Name = "Dual Recieved")]
        public Nullable<int> DualRecieved { get; set; }
        [Display(Name = "Pilot In Command")]
        public Nullable<int> FloatingCommand { get; set; }
        public Nullable<int> PilotId { get; set; }
    }


    [MetadataType(typeof(MSTR_Parts_LogHelper))]
    public partial class MSTR_Parts
    {

    }

    public class MSTR_Parts_LogHelper
    {
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> RecordType { get; set; }
        public string Description { get; set; }
        public int PartsId { get; set; }
        [Required(ErrorMessage ="Please Enter the Parts Name")]
        [Display(Name = "Parts Name")]
        public string PartsName { get; set; }
        [Required(ErrorMessage = "Please Enter the Supplier Name")]
        [Display(Name = "Supplier Name")]
        public Nullable<int> SupplierId { get; set; }
       
        [Required(ErrorMessage = "Please Enter the Model Name")]
        [Display(Name = "Model Name")]
        public string Model { get; set; }

    }


}