using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {

  public class DroneDetailView {
    private ExponentPortalEntities db = new ExponentPortalEntities();
    public DroneInfo DroneInfo;
    public AccountInfo AccountInfo;
    public List<FlightInfo> FlightInfo ;
    public List<PilotInfo> PilotInfo;

    public DroneDetailView(int DroneID) {
      DroneInfo = new DroneInfo(DroneID);
      AccountInfo = new AccountInfo(DroneInfo.AccountID);

      FlightInfo = db
        .DroneFlights
        .Where(w => w.DroneID == DroneID && w.MaxAltitude > 5)
        .OrderBy(o => o.ID)
        .Select(s => new FlightInfo {
          ID = s.ID,
          FlightDate  = (DateTime)s.FlightDate,
          FlightDistance = (int)s.FlightDistance,
          FlightHours = (int)s.FlightHours,
          MaxAltitude = (int)s.MaxAltitude,
          PilotID = (int)s.PilotID
        })
        .Take(5)
        .ToList();

       
      var PilotIDs = db
        .M2M_Drone_User
        .Where(w => w.DroneID == DroneID)
        .OrderBy(o => o.DroneID)
        .Select(s => s.UserID)
        .ToList();
      PilotInfo = new List<ViewModel.PilotInfo>();
      foreach (var PilotID in PilotIDs) {
        PilotInfo.Add(new ViewModel.PilotInfo(PilotID));
      }
    }
    public DroneDetailView(string DroneName)
    {
            DroneInfo = new DroneInfo(DroneName);
            AccountInfo = new AccountInfo(DroneInfo.AccountID);
    }
  }


  public class AccountInfo : MSTR_Account {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    public AccountInfo(int AccountID = 0) {
      var account = db.MSTR_Account.Where(w => w.AccountId == AccountID).FirstOrDefault();
      if (account != null) {
        this.Name = account.Name;
        this.EmailId = account.EmailId;
        this.MobileNo = account.MobileNo;
      }
    }
  }


  public class FlightInfo {
    private String _Duration;

    private ExponentPortalEntities db = new ExponentPortalEntities();
    int _PilotID = 0;
    public int PilotID {
      get { return _PilotID; }
      set {
        _PilotID = value;
        PilotInfo = new PilotInfo(_PilotID);
      }
    }

    public int ID { get; set; }
    public DateTime FlightDate { get; set; }
    public int FlightDistance { get; set; }
    public int FlightHours { get; set; }
    public int MaxAltitude { get; set; }
    public PilotInfo PilotInfo;

    public String Duration {
      get {
        TimeSpan t = TimeSpan.FromSeconds(FlightHours);        
        return t.ToString(@"hh\:mm");
      }
    }

  }

  public class DroneCreateModel {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    public DroneCreateModel() {
      AccountID = exLogic.Util.getAccountID();
      LoginUserID = exLogic.Util.getLoginUserID();
    }

    public int DroneID { get; set; }
    public String Description { get; set; }
    public int ManufactureID { get; set; }
    public String OtherManufacturer { get; set; }
    public int RpasTypeId { get; set; }
    public String OtherRPASType { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateTime CommissionDate { get; set; }

    private int DroneSerialNumber = 0;
    private int AccountID = 0;
    private int LoginUserID = 0;
    public MSTR_Drone Create() {
      int? DroneSerialNo = db.MSTR_Drone.OrderByDescending(u => u.DroneSerialNo).Select(e => e.DroneSerialNo).FirstOrDefault();

      if (RpasTypeId == 0 && !String.IsNullOrEmpty(OtherRPASType)) {
        RpasTypeId = CreateType("UAVType", OtherRPASType);
      }
      if (ManufactureID == 0 && !String.IsNullOrEmpty(OtherManufacturer)) {
        ManufactureID = CreateType("Manufacturer", OtherManufacturer);
      }
            DroneSerialNo = DroneSerialNo + 1;
      if (DroneSerialNo == null) DroneSerialNo = 1;
      if (DroneSerialNo < 1000) DroneSerialNo += 1000;

      MSTR_Drone Drone = new MSTR_Drone {
        AccountID = AccountID,
        ManufactureId = ManufactureID,
        UavTypeId = RpasTypeId,
        CommissionDate = CommissionDate,
        DroneDefinitionID = 11,
        IsActive = true,
        DroneSerialNo = DroneSerialNo,
        ModelName = Description,
        CreatedBy = LoginUserID,
        CreatedOn = DateTime.Now,
        ModelID = 0,
        MakeID = 0,
        RefName = Description,
        RpasSerialNo = Description
      };
      db.MSTR_Drone.Add(Drone);
      db.SaveChanges();

      //Add the drone to user
      M2M_Drone_User UserDrone = new M2M_Drone_User {
        UserID = LoginUserID,
        DroneID = Drone.DroneId
      };
      db.M2M_Drone_User.Add(UserDrone);
      db.SaveChanges();
          //  db.Entry(Drone).GetDatabaseValues();
            //  MSTR_Drone NewDrone = db.MSTR_Drone.Where(l => l.DroneId == Drone.DroneId).Select(l=>l).FirstOrDefault();
            db.Entry(Drone).Reload();
            return Drone;

    }

    public int CreateType(String TypeName, String TypeValue) {
      int? typeid = db.LUP_Drone
        .Where(w => w.Type == TypeName)
        .OrderByDescending(u => u.TypeId)
        .Select(e => e.TypeId)
        .FirstOrDefault();
      typeid = typeid == null ? 1 : typeid + 1;
      string BinaryCode = exLogic.Util.DecToBin((int)typeid);

      LUP_Drone LuD = new LUP_Drone();
      LuD.Type = TypeName;
      LuD.BinaryCode = BinaryCode;
      LuD.TypeId = typeid;
      LuD.Code = TypeValue.ToUpper().Substring(0, 3);
      LuD.IsActive = true;
      LuD.Name = TypeValue;
      LuD.CreatedBy = exLogic.Util.getLoginUserID();
      LuD.CreatedOn = DateTime.Now;
      LuD.GroupName = "MULTI-ROTOR";
      db.LUP_Drone.Add(LuD);
      db.SaveChanges();

      return (int)typeid;

    }

  }




  public class DroneView {

    private List<SelectListItem> SelectListItems = new List<SelectListItem> {
      new SelectListItem {Value = "", Text="Please Select..."},
      new SelectListItem {Value = "DCAA", Text="DCAA"},
      new SelectListItem {Value = "DOT", Text="DOT"},
      new SelectListItem {Value = "GCAA", Text="GCAA"}
    };

    public IEnumerable<SelectListItem> OwnerList { get; set; }
    [AllowHtml]
    public MSTR_Drone Drone { get; set; }
    public IEnumerable<SelectListItem> ManufactureList { get; set; }
    public IEnumerable<SelectListItem> UAVTypeList { get; set; }
    public IEnumerable<SelectListItem> PartsGroupList { get; set; }
    public IEnumerable<SelectListItem> DronePartsList { get; set; }
    public IEnumerable<string> SelectItemsForParts { set; get; }
    public MSTR_Parts DroneParts { get; set; }
    [AllowHtml]
    [MinLength(3, ErrorMessage = "Please enter minimum of 3 characters.")]

    public string Name { get; set; }
    public IEnumerable<SelectListItem> RegistrationAuthority {
      get {
        return SelectListItems;
      }
    }
    public string AccountName { get; set; }
    public string TypeName { get; set; }
  }

}
