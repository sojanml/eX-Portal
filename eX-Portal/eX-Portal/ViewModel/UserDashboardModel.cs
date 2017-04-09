using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class UserDashboardModel
    {
            public class PilotDetail { 
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public Nullable<bool> RememberMe { get; set; }
            public string PilotImage { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> CreatedBy { get; set; }
            public Nullable<int> LastModifiedBy { get; set; }
            public Nullable<int> ApprovedBy { get; set; }
            public Nullable<int> UserProfileId { get; set; }
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
            public string PasswordSalt { get; set; }
            public Nullable<int> AccountId { get; set; }
            public Nullable<bool> IsPilot { get; set; }
            public Nullable<int> UserAccountId { get; set; }
            public string Dashboard { get; set; }
            public string GeneratedPassword { get; set; }
            public string RPASPermitNo { get; set; }
            public string PermitCategory { get; set; }
            public string ContactAddress { get; set; }
            public string RegRPASSerialNo { get; set; }
            public string CompanyAddress { get; set; }
            public string CompanyTelephone { get; set; }
            public string CompanyEmail { get; set; }
            public string TradeLicenceCopyUrl { get; set; }
            public string EmiratesID { get; set; }
            public string PassportNo { get; set; }
            public Nullable<System.DateTime> DateOfExpiry { get; set; }
            public string Department { get; set; }
            public string EmiratesId { get; set; }
            public string Title { get; set; }
            public string Nationality { get; set; }
            public DateTime DOI_RPASPermit { get; set; }
            public DateTime DOE_RPASPermit { get; set; }
            public string UserDescription { get; set; }
            public string CompanyName { get; set; }
        }
            public class RPASDetail
        {
            public int DroneId { get; set; }
            public Nullable<int> AccountID { get; set; }
            public Nullable<int> ManufactureId { get; set; }
            public Nullable<int> UavTypeId { get; set; }
            public Nullable<int> PartsGroupId { get; set; }
            public string DroneName { get; set; }
            public string Remarks { get; set; }
            public Nullable<System.DateTime> CreatedOn { get; set; }
            public Nullable<System.DateTime> ModifiedOn { get; set; }
            public Nullable<System.DateTime> ApprovedOn { get; set; }
            public string DroneIdBitString { get; set; }
            public Nullable<int> RecordType { get; set; }
            public Nullable<bool> IsActive { get; set; }
            public string DroneIdHexa { get; set; }
            public string DroneIdBarCode { get; set; }
            public Nullable<System.DateTime> CommissionDate { get; set; }
            public Nullable<int> DroneSerialNo { get; set; }
            public Nullable<int> DroneDefinitionID { get; set; }
            public Nullable<System.DateTime> DecommissionDate { get; set; }
            public Nullable<int> DecommissionBy { get; set; }
            public string DecommissionNote { get; set; }
            public string ModelName { get; set; }
            public Nullable<decimal> Latitude { get; set; }
            public Nullable<decimal> Longitude { get; set; }
            public Nullable<System.DateTime> FlightTime { get; set; }
            public Nullable<int> LastFlightID { get; set; }
            public string RpasSerialNo { get; set; }
            public Nullable<int> BlackBoxID { get; set; }
            public string RefName { get; set; }
            public string Type { get; set; }
            public string MaxAllupWeight { get; set; }
            public string color { get; set; }
            public Nullable<int> IsCamara { get; set; }
            public Nullable<int> MakeID { get; set; }
            public string MakeOther { get; set; }
            public Nullable<int> ModelID { get; set; }
            public string ManufactureOther { get; set; }
            public string CameraDetails { get; set; }
            public string RegistrationDocument { get; set; }
            public string RegistrationAuthority { get; set; }
            public string AccountName { get; set; }
            public string ManufactureName { get; set; }
            public string UavTypeName { get; set; }
            public string PartsGroupName { get; set; }

        }
            public ApproalDetail ApprovalDetails { get; set; }
            public List<GCA_Approval> ApprovalList { get; set; }
            public List<RPASDetail> RPASList { get; set; }
            public PilotDetail Pilot { get; set; }
   }
}