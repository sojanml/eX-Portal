using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class NOC_Details_Ext: Models.NOC_Details {
    private ExponentPortalEntities ctx = new ExponentPortalEntities();
    private String _droneName;
    private String _pilotName;
    private String _droneIcon;

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


      this.NOC_Details = noc.NOC_Details;

      foreach(var det in noc.NOC_Details) {
        NOC_Details_Ext NocDetail = new NOC_Details_Ext {
          DroneID = det.DroneID,
          Coordinates = det.Coordinates,
          EndDate = det.EndDate,
          EndTime = det.EndTime,
          IsUseCamara = det.IsUseCamara,
          LOS = det.LOS,
          MaxAltitude = det.MaxAltitude,
          MinAltitude = det.MinAltitude,
          NocID = det.NocID,
          OuterCoordinates = det.OuterCoordinates,
          PilotID = det.PilotID,
          StartDate = det.StartDate,
          StartTime = det.StartTime,
          Status = det.Status,
          StatusChangedBy = det.StatusChangedBy,
          StatusChangedOn = det.StatusChangedOn,
          NocBuffer = det.NocBuffer
        };
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