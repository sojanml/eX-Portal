using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {

  public class PilotInfo : MSTR_User {

    private ExponentPortalEntities ctx = new ExponentPortalEntities();


    public PilotInfo(int PilotID = 0) {
      var Usr = ctx.MSTR_User.Where(w => w.UserId == PilotID).FirstOrDefault();
      if (Usr != null) {
        if (String.IsNullOrEmpty(Usr.PhotoUrl)) {
          this.PhotoUrl = "/images/PilotImage.png";
        } else {
          Usr.PhotoUrl = $"/Upload/User/{Usr.UserId}/{Usr.PhotoUrl}";
          if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(Usr.PhotoUrl)))
            this.PhotoUrl = "/images/PilotImage.png";
        }
        this.FirstName = Usr.FirstName;
        this.LastName = Usr.LastName;
        this.UserId = Usr.UserId;
        this.EmailId = Usr.EmailId;
        this.RPASPermitNo = Usr.RPASPermitNo;
        this.DOE_RPASPermit = Usr.DOE_RPASPermit;
        this.DOI_RPASPermit = Usr.DOI_RPASPermit;
      }
    }

    public String FullName { get { return String.Concat(FirstName, " ", LastName); } }
    public String IssueDate { get { return DOI_RPASPermit == null ? "Invalid" : ((DateTime)DOI_RPASPermit).ToString("dd-MMM-yyyy"); } }
    public String ExpiryDate { get { return DOE_RPASPermit == null ? "Invalid" : ((DateTime)DOE_RPASPermit).ToString("dd-MMM-yyyy"); } }
  }



  public class DroneInfo {
    private String _DroneRefName;
    private String _Manufacturer;
    private String _UAVType;
    private String _QRCode;
    private String _UAVGroup;

    private int DroneID = 0;
    private MSTR_Drone _droneinfo = null;


    private ExponentPortalEntities ctx = new ExponentPortalEntities();

    public DroneInfo(int DroneID = 0) {
      this.DroneID = DroneID;
      _droneinfo = ctx.MSTR_Drone.Where(w => w.DroneId == DroneID).FirstOrDefault();
      if (_droneinfo != null) {
        _DroneRefName = _droneinfo.RefName;
        if (String.IsNullOrWhiteSpace(_DroneRefName))
          _DroneRefName = _droneinfo.ModelName;
        _Manufacturer = ctx.LUP_Drone
          .Where(w => w.Type == "Manufacturer" && w.TypeId == _droneinfo.ManufactureId)
          .Select(s => s.Name)
          .FirstOrDefault();
        var xUAVType = ctx.LUP_Drone
          .Where(w => w.Type == "UAVType" && w.TypeId == _droneinfo.UavTypeId)
          .Select(s => new {
            Name = s.Name,
            Group = s.GroupName
          })
          .FirstOrDefault();

        if (xUAVType != null) {
          _UAVType = xUAVType.Name;
          _UAVGroup = xUAVType.Group;
        }

        String QRCodePath = System.Web.Hosting.HostingEnvironment.MapPath("/Upload/QRCode");
        String QRImagePath = $"{QRCodePath}//{DroneName}.png";
        if (System.IO.File.Exists(QRImagePath)) {
          _QRCode = $"/Upload/QRCode/By100/{DroneName}.png";
        } else {
          _QRCode = $"/Images/QRCode.png";
        }
      }
    }
        public DroneInfo(string DroneName = "")
        {
          //  this._droneinfo.DroneName = DroneName;
            _droneinfo = ctx.MSTR_Drone.Where(w => w.DroneName == DroneName).FirstOrDefault();
            if (_droneinfo != null)
            {
                _DroneRefName = _droneinfo.RefName;
                if (String.IsNullOrWhiteSpace(_DroneRefName))
                    _DroneRefName = _droneinfo.ModelName;
                _Manufacturer = ctx.LUP_Drone
                  .Where(w => w.Type == "Manufacturer" && w.TypeId == _droneinfo.ManufactureId)
                  .Select(s => s.Name)
                  .FirstOrDefault();
                var xUAVType = ctx.LUP_Drone
                  .Where(w => w.Type == "UAVType" && w.TypeId == _droneinfo.UavTypeId)
                  .Select(s => new {
                      Name = s.Name,
                      Group = s.GroupName
                  })
                  .FirstOrDefault();

                if (xUAVType != null)
                {
                    _UAVType = xUAVType.Name;
                    _UAVGroup = xUAVType.Group;
                }

                String QRCodePath = System.Web.Hosting.HostingEnvironment.MapPath("/Upload/QRCode");
                String QRImagePath = $"{QRCodePath}//{DroneName}.png";
                if (System.IO.File.Exists(QRImagePath))
                {
                    _QRCode = $"/Upload/QRCode/By100/{DroneName}.png";
                }
                else
                {
                    _QRCode = $"/Images/QRCode.png";
                }
            }
        }

        public String DroneRefName {
      get { return _DroneRefName; }
    }

    public String Manufacturer {
      get { return _Manufacturer; }
    }
    public String UAVType {
      get { return _UAVType; }
    }
    public String QRCode {
      get { return _QRCode; }
    }
    public String UAVGroup {
      get { return _UAVGroup; }
    }
    public int AccountID {
      get { return _droneinfo == null ? 0 : (int)_droneinfo.AccountID; }
    }
    public String CommissionDate {
      get { return _droneinfo == null ? "Invalid" : ((DateTime)_droneinfo.CommissionDate).ToString("dd-MMM-yyyy"); }
    }
    public String DroneName { 
      get { return _droneinfo == null ? "Invalid" : _droneinfo.DroneName; }
    }
  }


  public class NOC_Details_Ext: Models.NOC_Details {
    private ExponentPortalEntities ctx = new ExponentPortalEntities();
    private String _droneName;
    private String _pilotName;
    private String _droneIcon;
    public DroneInfo DroneInfo;
    public PilotInfo PilotInfo;

    public NOC_Details_Ext(NOC_Details det) {
      this.DroneID = det.DroneID;
      this.Coordinates = det.Coordinates;
      this.EndDate = det.EndDate;
      this.EndTime = det.EndTime;
      this.IsUseCamara = det.IsUseCamara;
      this.LOS = det.LOS;
      this.MaxAltitude = det.MaxAltitude;
      this.MinAltitude = det.MinAltitude;
      this.NocID = det.NocID;
      this.OuterCoordinates = det.OuterCoordinates;
      this.PilotID = det.PilotID;
      this.StartDate = det.StartDate;
      this.StartTime = det.StartTime;
      this.Status = det.Status;
      this.StatusChangedBy = det.StatusChangedBy;
      this.StatusChangedOn = det.StatusChangedOn;
      this.NocBuffer = det.NocBuffer;

      DroneInfo = new DroneInfo(det.DroneID);
      PilotInfo = new PilotInfo(det.PilotID);
    }

    public String DroneName {
      get {
        if (DroneID <= 0) return String.Empty;
        var _Query = 
          from d in ctx.MSTR_Drone
          where d.DroneId == DroneID
          select d.DroneName;
        _droneName = _Query.FirstOrDefault();
        return _droneName;
      }
    }

    public String PilotName {
      get {
        if (PilotID <= 0)
          return String.Empty;
        var _Query =
          from d in ctx.MSTR_User
          where d.UserId == PilotID
          select String.Concat(d.FirstName, " ", d.LastName);
        _pilotName = _Query.FirstOrDefault();
        return _pilotName;
      }
    }

    
  }

  public class MSTR_NOC_Ext: Models.MSTR_NOC {
    private String _accountName = String.Empty;
    private List<NOC_Details_Ext> _NOC_Details_Ext = new List<NOC_Details_Ext>();

    public MSTR_NOC_Ext(MSTR_NOC noc) {
      this.AccountID = noc.AccountID;
      this.CountApproved = noc.CountApproved;
      this.CountRejected = noc.CountRejected;
      this.CountNew = noc.CountNew;
      this.CountTotal = noc.CountTotal;
      this.CountAmended = noc.CountAmended;
      this.StartDate = noc.StartDate;
      this.EndDate = noc.EndDate;
      this.FlightFor = noc.FlightFor;
      this.FlightType = noc.FlightType;
      this.NocApplicationID = noc.NocApplicationID;
      this.AccountID = noc.AccountID;
      this.CreateBy = noc.CreateBy;

      this.NOC_Details = noc.NOC_Details;

      foreach(var det in noc.NOC_Details) {
        NOC_Details_Ext NocDetail = new NOC_Details_Ext(det);
        _NOC_Details_Ext.Add(NocDetail);
      }
    }

    public String AccountName { get {
        if(String.IsNullOrEmpty(_accountName)) {
          _accountName = this.getAccountName();
        }
        return _accountName;
      }
      set {
        _accountName = value;
      }
    }

    public String sStartDate {
      get { return StartDate.ToString("dd-MMM-yyyy"); }
    }
    public String sEndDate {
      get { return EndDate.ToString("dd-MMM-yyyy"); }
    }

    private String getAccountName() {
      if (this.AccountID == 0)
        return String.Empty;
      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        var query = from a in ctx.MSTR_Account
                    where a.AccountId == this.AccountID
                    select a.Name;
        return query.FirstOrDefault();
      }
    }

    public List<NOC_Details_Ext> NOC_Details_Ext {
      get {
        return _NOC_Details_Ext;
      }
    }

  }


}