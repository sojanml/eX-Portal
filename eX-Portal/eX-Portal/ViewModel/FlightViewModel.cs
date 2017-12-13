using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.ViewModel {

  public class ApprovalInfo {
    public String ApprovalName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public String StartTime { get; set; }
    public String EndTime { get; set; }
  }



  public class FlightViewModel {
    public FlightViewModel() {
      ApprovalID = 0;
    }
    public int ID { get; set; }
    public Nullable<int> PilotID { get; set; }
    public Nullable<int> GSCID { get; set; }
    public Nullable<System.DateTime> FlightDate { get; set; }
    public Nullable<System.DateTime> CreatedOn { get; set; }
    public Nullable<int> CreatedBy { get; set; }
    public Nullable<int> DroneID { get; set; }
    public Nullable<int> BBFlightID { get; set; }
    public Nullable<decimal> Latitude { get; set; }
    public Nullable<decimal> Longitude { get; set; }
    public string LogFrom { get; set; }
    public string LogTo { get; set; }
    public Nullable<System.DateTime> LogTakeOffTime { get; set; }
    public Nullable<System.DateTime> LogLandingTime { get; set; }
    public string LogBattery1ID { get; set; }
    public Nullable<decimal> LogBattery1StartV { get; set; }
    public Nullable<decimal> LogBattery1EndV { get; set; }
    public string LogBattery2ID { get; set; }
    public Nullable<decimal> LogBattery2StartV { get; set; }
    public Nullable<decimal> LogBattery2EndV { get; set; }
    public Nullable<System.DateTime> LogDateTime { get; set; }
    public Nullable<int> LogCreatedBy { get; set; }
    public string Descrepency { get; set; }
    public string ActionTaken { get; set; }
    public Nullable<byte> IsLogged { get; set; }
    public Nullable<bool> IsFlightOutside { get; set; }
    public string RecordedVideoURL { get; set; }
    public Nullable<int> IsFlightSoftFence { get; set; }
    public Nullable<int> LowerLimit { get; set; }
    public Nullable<int> HigherLimit { get; set; }
    public Nullable<decimal> GridLat { get; set; }
    public Nullable<decimal> GridLng { get; set; }
    public Nullable<int> FlightHours { get; set; }

    public string PilotName { get; set; }
    public string GSCName { get; set; }
    public int ZoneId { get; set; }
    public string ZoneCoordinates { get; set; }
    public string DroneName { get; set; }

    public IList<PortalAlert> PortalAlerts { get; set; }
    public IList<GCA_Approval> Approvals { get; set; }
    public IList<DroneFlightVideo> Videos { get; set; }
    public IList<LatLng> MapData { get; set; }
    public GCA_Approval Approval { get; set; }

    public Models.FlightInfo Info { get; set; }
    public Decimal? FlightDistance { get; internal set; }

    public String getCreatedOn() {
      return Util.fmtDt(CreatedOn);
    }
    public String getFlightDate() {
      return Util.fmtDt(FlightDate);
    }


    public String getFlightTime() {
      if (FlightHours == null) FlightHours = 0;
      TimeSpan time = TimeSpan.FromSeconds((int)FlightHours);
      return time.ToString(@"hh\:mm\:ss");
    }

    public int ApprovalID { get; set; }

    public List<BillingModule.BillingGroupRule> Billing { get; set; }
  }
}