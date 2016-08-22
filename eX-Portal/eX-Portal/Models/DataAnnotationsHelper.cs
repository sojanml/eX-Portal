using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;


namespace eX_Portal.Models {
  /*
    Reference
    http://stackoverflow.com/questions/12610319/mvc-db-first-fix-display-name
  */

  public class DataAnnotationsHelper {
  }

  [MetadataType(typeof(MSTR_BlackBoxHelper))]
  public partial class MSTR_BlackBox { }

  public class MSTR_BlackBoxHelper {
    [Required(ErrorMessage = "Please Enter the BlackBox Serial")]
    [Display(Name = "BlackBox Serial")]
    public string BlackBoxSerial { get; set; }

    [Required(ErrorMessage = "Please Enter the BlackBox Name")]
    [Display(Name = "BlackBox Name")]
    public string BlackBoxName { get; set; }

    [Required(ErrorMessage = "Please Enter the Encryption Key")]
    [Display(Name = "Encryption Key")]
    public string EncryptionKey { get; set; }

  }//MSTR_AccountHelper

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

    [Required(ErrorMessage = "Please enter First Name")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Please enter Last Name")]
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
    [Required]
    public string IsActive { get; set; }

    [Display(Name = "Is Pilot?")]
    [Required]
    public string IsPilot { get; set; }

    [Required(ErrorMessage = "Please enter Mobile Number")]
    [Display(Name = "Mobile Number")]
    public string MobileNo { get; set; }

    [Display(Name = "Office Number")]
    public string OfficeNo { get; set; }
    [Display(Name = "Home Number")]
    public string HomeNo { get; set; }

    [Display(Name = "Country")]
    public string CountryId { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string CompanyEmail { get; set; }

  }//MSTR_UserHelper



  //MSTR_User_Pilot_Certification helper class

  [MetadataType(typeof(MSTR_User_Pilot_CertificationHelper))]
  public partial class MSTR_User_Pilot_Certification {

  }

  public class MSTR_User_Pilot_CertificationHelper {
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
  public partial class MSTR_User_Pilot_ExponentUAS {

  }

  public class MSTR_User_Pilot_ExponentUASHelper {
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
  public partial class MSTR_User_Pilot {

  }

  public class MSTR_User_PilotHelper {

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
  public partial class MSTR_Pilot_Log {

  }

  public class MSTR_Pilot_LogHelper {


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
  public partial class MSTR_Parts {

  }

  public class MSTR_Parts_LogHelper {
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
    [Required(ErrorMessage = "Please Enter the Parts Name")]
    [Display(Name = "Parts Name")]
    public string PartsName { get; set; }
    [Required(ErrorMessage = "Please Enter the Supplier Name")]
    [Display(Name = "Supplier Name")]
    public Nullable<int> SupplierId { get; set; }

    [Required(ErrorMessage = "Please Enter the Model Name")]
    [Display(Name = "Model Name")]
    public string Model { get; set; }

  }



  [MetadataType(typeof(MSTR_DroneService_Helper))]
  public partial class MSTR_DroneService {

  }

  public class MSTR_DroneService_Helper {

    public int ServiceId { get; set; }
    public string Description { get; set; }
    public Nullable<int> CreatedBy { get; set; }
    public Nullable<int> ModifiedBy { get; set; }
    public Nullable<int> AcceptedBy { get; set; }
    public Nullable<System.DateTime> CreatedOn { get; set; }
    public Nullable<System.DateTime> ModifiedOn { get; set; }
    public Nullable<System.DateTime> AcceptedOn { get; set; }
    public Nullable<bool> IsActive { get; set; }
    public Nullable<int> RecordType { get; set; }
    [Required(ErrorMessage = "Please select UAS ")]
    public Nullable<int> DroneId { get; set; }
    [Required(ErrorMessage = "Please select Type Of Service ")]
    public string TypeOfService { get; set; }
    public Nullable<int> FlightHour { get; set; }
    [Required(ErrorMessage = "Please select Date Of Service ")]
    public Nullable<System.DateTime> DateOfService { get; set; }

    public Nullable<int> TypeOfServiceId { get; set; }
  }


  [MetadataType(typeof(DroneDocument_Helper))]
  public partial class DroneDocument {
    public String getThumbnail() {
      //if the thumbnail does not exist on FileSystem
      //then create it 
      S3Url = S3Url.Trim();
      var S3Path = HttpContext.Current.Server.MapPath("/Upload/" + S3Url);
      String HTTPURL = "https://exponent-s3.s3.amazonaws.com/" + S3Url;
      String theExt = Path.GetExtension(S3Path);
      switch(theExt) {
        case ".jpg":
        case ".jpeg":
        case ".gif":
        case ".png":
          string AppPath = Path.GetDirectoryName(S3Path);
          if(!File.Exists(S3Path)) {
            if(!Directory.Exists(AppPath))
              Directory.CreateDirectory(AppPath);
            Image image = getImageFromURL(HTTPURL);
            Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            thumb.Save(S3Path);
          }
          HTTPURL = "/Upload/" + S3Url;
          break;
        case ".aac":
        case ".ai":
        case ".aiff":
        case ".avi":
        case ".bmp":
        case ".c":
        case ".cpp":
        case ".css":
        case ".dat":
        case ".dmg":
        case ".doc":
        case ".dotx":
        case ".dwg":
        case ".dxf":
        case ".eps":
        case ".exe":
        case ".flv":
        case ".h":
        case ".hpp":
        case ".html":
        case ".ics":
        case ".iso":
        case ".java":
        case ".js":
        case ".key":
        case ".less":
        case ".mid":
        case ".mp3":
        case ".mp4":
        case ".mpg":
        case ".odf":
        case ".ods":
        case ".odt":
        case ".otp":
        case ".ots":
        case ".ott":
        case ".pdf":
        case ".php":
        case ".ppt":
        case ".psd":
        case ".py":
        case ".qt":
        case ".rar":
        case ".rb":
        case ".rtf":
        case ".sass":
        case ".scss":
        case ".sql":
        case ".tga":
        case ".tgz":
        case ".tiff":
        case ".txt":
        case ".wav":
        case ".xls":
        case ".xlsx":
        case ".xml":
        case ".yml":
        case ".zip":
          HTTPURL = "/images/512px/" + theExt.Substring(1) + ".png";
          break;
        default:
          HTTPURL = "/images/512px/_page.png";
          break;

      }//switch (theExt)

      return HTTPURL;
    }

    private Image getImageFromURL(string url) {
      HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

      using(HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
        using(Stream stream = httpWebReponse.GetResponseStream()) {
          return Image.FromStream(stream);
        }
      }
    }

    public bool isVideo() {
      S3Url = S3Url.Trim();
      var S3Path = HttpContext.Current.Server.MapPath("/Upload/" + S3Url);
      String theExt = Path.GetExtension(S3Path);
      switch(theExt) {
        case ".mp4":
        case ".flv":
          return true;
      }
      return false;
    }

    public String getURL() {
      String URL = String.Empty;
      if(!String.IsNullOrEmpty(DocumentName)) {
        URL = "/Upload/Drone/" + DocumentName;
      }
      return URL;
    }
    public String getName() {
      String FileName = String.Empty;
      if(!String.IsNullOrEmpty(DocumentName)) {
        int TiledAt = DocumentName.IndexOf('~');
        if(TiledAt > 0) FileName = DocumentName.Substring(TiledAt + 1);
      }
      return FileName;
    }

    public String getMoreInfo() {
      StringBuilder MoreInfo = new StringBuilder();
      if(!String.IsNullOrEmpty(DocumentTitle)) {
        MoreInfo.Append("<span class=\"DocumentTitle\">");
        MoreInfo.Append(DocumentTitle);
        MoreInfo.Append("</span>");
      }
      if(!String.IsNullOrEmpty(DocumentDesc)) {
        bool isAppendBraket = (MoreInfo.Length > 0);
        MoreInfo.Append("<span class=\"DocumentDesc\">");
        if(isAppendBraket)
          MoreInfo.Append(" [");
        MoreInfo.Append(DocumentDesc);
        if(isAppendBraket)
          MoreInfo.Append("]");
        MoreInfo.Append("</span>");
      }
      return MoreInfo.ToString();
    }

    public string DocsType { get; set; }
  }

  public class DroneDocument_Helper {
    [Required]
    public int DroneID { get; set; }
    [Required]
    public string DocumentType { get; set; }
    [Required]
    public string DocumentName { get; set; }
    [DataType(DataType.MultilineText)]
    public string DocumentDesc { get; set; }

  }

  [MetadataType(typeof(GCA_Approval_Helper))]
  public partial class GCA_Approval {
    public string S3Url { get; set; }
  }

  public class GCA_Approval_Helper {
    [Required(ErrorMessage = "Please select a drone")]
    [Display(Name = "Drone Name")]
    public string DroneID { get; set; }

    [DataType(DataType.MultilineText)]
    [Required(ErrorMessage = "Please Enter Coordinates")]
    public string Coordinates { get; set; }

    [Required(ErrorMessage = "Please Enter Approval Name")]
    [Display(Name = "Approval Name")]
    public string ApprovalName { get; set; }

    [Display(Name = "Approval Date")]
    public string ApprovalDate { get; set; }

    [Required(ErrorMessage = "Please Enter Start Date")]
    [Display(Name = "Start Date")]
    public string StartDate { get; set; }

    [Required(ErrorMessage = "Please Enter End Date")]
    [Display(Name = "End Date")]
    public string EndDate { get; set; }

    [Required(ErrorMessage = "Please Enter Start Time")]
    [Display(Name = "Start Time")]
    public string StartTime { get; set; }

    [Required(ErrorMessage = "Please Enter End Time")]
    [Display(Name = "End Time")]
    public string EndTime { get; set; }

    [DataType(DataType.MultilineText)]
    [Required(ErrorMessage = "Please Enter Remarks")]
    [Display(Name = "Approval Remarks")]
    public string ApprovalRemarks { get; set; }


  }

  [MetadataType(typeof(MSTR_RPAS_User_Helper))]
  public partial class MSTR_RPAS_User {
    public string confirmemailid { get; set; }
    public string confirmmobno { get; set; }
  }

  public class MSTR_RPAS_User_Helper {
    [Required(ErrorMessage = "Please Enter your Name")]
    [Display(Name = "First Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please Enter Emirates Id")]
    [Display(Name = "Emirates ID")]
    public string EmiratesId { get; set; }

    [Required(ErrorMessage = "Please Enter Email Address")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Display(Name = "Email Address")]
    public string EmailId { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Compare("EmailId")]
    [Display(Name = "Confirm Email Address")]
    public string confirmemailid { get; set; }

    [Required(ErrorMessage = "Please Enter Mobile Number")]
    [Display(Name = "Mobile Number")]
    public string MobileNo { get; set; }

    [Compare("MobileNo")]
    [Display(Name = "Confirm Mobile No")]
    public string confirmmobno { get; set; }


  }


  [MetadataType(typeof(MSTR_DroneHelper))]
  public partial class MSTR_Drone {
  }

  public class MSTR_DroneHelper {
    [Required(ErrorMessage = "Please select Manufacturer")]
    [Display(Name = "Manufacturer")]
    public int ManufactureId { get; set; }

    //[Required(ErrorMessage = "Please select Drone Name")]
    //[Display(Name = "Drone Name")]
    //public string DroneName { get; set; }

    //[Required(ErrorMessage = "Please select Commission Date")]
    //[Display(Name = "Commission Date")]
    //public string CommissionDate { get; set; }

    [Required(ErrorMessage = "Please enter Rpas Serial Number")]
    [Display(Name = "Rpas Serial No")]
    public string RpasSerialNo { get; set; }

    [Required(ErrorMessage = "Please enter Reference Name")]
    [Display(Name = "Reference name")]
    public int RefName { get; set; }

    [Required(ErrorMessage = "Please select Make")]
    [Display(Name = "Make")]
    public string MakeID { get; set; }

    [Required(ErrorMessage = "Please select Model Name")]
    [Display(Name = "Model Name")]
    public string ModelID { get; set; }


    //[Required(ErrorMessage = "Please select Camara")]
    //[Display(Name = "Is Camara")]
    //public int? IsCamara { get; set; }

  }

  [MetadataType(typeof(BlackBoxTransactionHelper))]
  public partial class BlackBoxTransaction {
    public bool VerifyCheck { get; set; }
  }

  public class BlackBoxTransactionHelper {
    [Required(ErrorMessage = "Please select BlackBox")]
    [Display(Name = "BlackBox")]
    public int BlackBoxID { get; set; }

    [Required(ErrorMessage = "Please select Transaction Mode")]
    [Display(Name = "Collection Mode")]
    public string CollectionMode { get; set; }

    //[Required(ErrorMessage = "Please enter Bank Name")]
    //[Display(Name = "Bank Name")]
    //public string BankName { get; set; }

    [Required(ErrorMessage = "Please enter Amount")]
    [Display(Name = "Amount")]
    public Nullable<decimal> Amount { get; set; }

    [DataType(DataType.MultilineText)]
    [Required(ErrorMessage = "Please enter Note")]
    [Display(Name = "Note")]
    public string Note { get; set; }

    [Required(ErrorMessage = "Please enter Verify Code")]
    [Display(Name = "Verify Code")]
    public string VerifyCode { get; set; }

    //[Required(ErrorMessage = "Please Is Verified ")]
    //[Display(Name = "Is Verified")]
    //public string VerifyCheck { get; set; }
  }

  [MetadataType(typeof(ContentManagementHelper))]
  public partial class ContentManagement {
    
  }

  public class ContentManagementHelper {
    private String _BodyContent = String.Empty;

    [Required]
    [Display(Name = "Reference URL")]
    [RegularExpression("^([a-zA-Z0-9_]+)$", ErrorMessage = "Invalid URL Reference (No Special characters or space allowed)")]
    public String CmsRefName;

    [Required]
    [Display(Name = "Page Title")]
    public String PageTitle;

    [Required]
    [Display(Name = "Show in Main Menu")]
    public int IsShowInMenu;


    [Required]
    [Display(Name = "Page Body (in HTML)")]
    public String Body;

    [Required]
    [Display(Name = "Short Page Title")]
    public String MenuTitle;

  }



}