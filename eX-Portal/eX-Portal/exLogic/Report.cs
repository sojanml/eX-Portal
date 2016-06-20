using System;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using eX_Portal.Models;

namespace eX_Portal.exLogic {

  public class FlightReportFilter {
    private DateTime _From = DateTime.Now.AddDays(-30);
    private DateTime _To = DateTime.Now;
    private int _Pilot = 0;
    private int _UAS = 0;
    private int _Boundary = 0;
    private int _Height = 0;
    private int _Proximity = 0;
    private int _BoundaryCritical = 0;
    private int _HeightCritical = 0;
    private int _ProximityCritical = 0;

    public String From {
      get {
        return _From.ToString("dd-MMM-yyyy");
      }
      set {
        if (!String.IsNullOrEmpty(value))
          DateTime.TryParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _From);
      }
    }
    public String To {
      get {
        return _To.ToString("dd-MMM-yyyy"); ;
      }
      set {
        if (!String.IsNullOrEmpty(value))
          DateTime.TryParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _To);
      }
    }
    public int Pilot {
      get {return _Pilot;}
      set {_Pilot = value;}
    }

    public int UAS {
      get { return _UAS; }
      set { _UAS = value; }
    }

    public int Boundary { 
      get { return _Boundary; } 
      set { _Boundary = value; } 
    }
    public int Height { 
      get { return _Height; } 
      set { _Height = value; } 
    }
    public int Proximity { 
      get { return _Proximity; } 
      set { _Proximity = value; } 
    }
    public int BoundaryCritical { 
      get { return _BoundaryCritical; } 
      set { _BoundaryCritical = value; } 
    }
    public int HeightCritical { 
      get { return _HeightCritical; } 
      set { _HeightCritical = value; } 
    }
    public int ProximityCritical { 
      get { return _ProximityCritical; } 
      set { _ProximityCritical = value; } 
    }



    public String getPilotName() {
      String SQL = "SELECT FirstName + ' ' +  LastName From MSTR_User WHERE UserID=" + _Pilot;
      return Util.getDBVal(SQL);
    }

    public String getUASName() {
      String SQL = "SELECT DroneName From MSTR_Drone WHERE DroneID=" + _UAS;
      return Util.getDBVal(SQL);
    }

    public String FromSQL() {
      return _From.ToString("yyyy-MM-dd 00:00:00");
    }
    public String ToSQL() {
      return _To.ToString("yyyy-MM-dd 23:59:59");
    }
  }

  public class FlightReportData {
    public int ID { get; set; }
    public DateTime FlightDate { get; set; }
    public String Pilot { get; set; }
    public String UAS { get; set; }
    public String FlightTime { get; set; }
    public String MaxAltitude { get; set; }
    public String BoundaryAlerts { get; set; }
    public String ProximityAlerts { get; set; }
    public String HeightAlerts { get; set; }

  }

  public class Report {
    public String getFlightReportSQL(FlightReportFilter Filter) {
      StringBuilder SQLFilter = new StringBuilder();
      StringBuilder SQL = new StringBuilder();
      SQL.AppendLine(@"SELECT
  DroneFlight.ID AS Ref,
  DroneFlight.FlightDate,
  ( MSTR_User.FirstName + ' ' + MSTR_User.LastName ) AS Pilot,
  MSTR_Drone.DroneName as UAS,
  convert(varchar, DATEADD(ms, DroneFlight.FlightHours * 1000, 0),108) as FlightTime,
  DroneFlight.MaxAltitude,
  Convert(Varchar(10), PortalAlertCounter.BoundaryCritical) + ' of ' + 
  Convert(Varchar(10), PortalAlertCounter.Boundary)  as BoundaryAlerts,
  Convert(Varchar(10), PortalAlertCounter.ProximityCritical) + ' of' + 
  Convert(Varchar(10), PortalAlertCounter.Proximity)  as ProximityAlerts,
  Convert(Varchar(10), PortalAlertCounter.HeightCritical) + ' of ' + 
  Convert(Varchar(10), PortalAlertCounter.Height)  as AltitudeAlerts,
  Count(*)
    OVER() AS _TotalRecords,
  DroneFlight.ID AS _PKey
FROM
  DroneFlight
INNER JOIN MSTR_Drone ON
  MSTR_Drone.DroneID = DroneFlight.DroneID
LEFT JOIN  MSTR_User ON
  MSTR_User.UserID = DroneFlight.PilotID
LEFT JOIN  (SELECT
  FlightID,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Boundary' THEN
          1
        ELSE
          0
      END) AS Boundary,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Boundary' AND
             AlertType = 'Critical' THEN
          1
        ELSE
          0
      END) AS BoundaryCritical,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Height' THEN
          1
        ELSE
          0
      END) AS Height,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Height' AND
             AlertType = 'Critical' THEN
          1
        ELSE
          0
      END) AS HeightCritical,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Proximity' THEN
          1
        ELSE
          0
      END) AS Proximity,
  Sum(CASE
        WHEN PortalAlert.AlertCategory = 'Proximity' AND
             AlertType = 'Critical' THEN
          1
        ELSE
          0
      END) AS ProximityCritical
FROM
  PortalAlert
GROUP  BY
  FlightID) AS PortalAlertCounter ON
  PortalAlertCounter.FlightID = DroneFlight.ID
  ");

      SQLFilter.AppendLine("WHERE");
      SQLFilter.AppendLine("  DroneFlight.FlightDate BETWEEN '" + Filter.FromSQL() + "' AND '" + Filter.ToSQL() + "'");
      if (Filter.Pilot > 0)
        SQLFilter.AppendLine("AND  DroneFlight.PilotID=" + Filter.Pilot);
      if (Filter.UAS > 0)
        SQLFilter.AppendLine("AND  DroneFlight.DroneID=" + Filter.UAS);
      if (Filter.Proximity > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.Proximity > 0");
      if (Filter.ProximityCritical > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.ProximityCritical > 0");
      if (Filter.Height > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.Height > 0");
      if (Filter.HeightCritical > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.HeightCritical > 0");
      if (Filter.Boundary > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.Boundary > 0");
      if (Filter.BoundaryCritical > 0)
        SQLFilter.AppendLine("AND  PortalAlertCounter.BoundaryCritical > 0");


      SQL.Append(SQLFilter);
      return SQL.ToString();
    }//public String getFlightReportSQL()

    public String getUAS(String Term) {
      StringBuilder SQL = new StringBuilder();
      StringBuilder SQLFilter = new StringBuilder();
      SQL.AppendLine(@"SELECT
        Mstr_Drone.DroneID as value,
        MSTR_Drone.DroneName as label
      FROM
        MSTR_Drone");
      if (!User.hasAccess("DRONE.MANAGE")) {
        SQLFilter.AppendLine("MSTR_Drone.AccountID = " + Util.getAccountID());
      }
      if (!String.IsNullOrEmpty(Term)) {
        if (SQLFilter.Length > 0) SQLFilter.AppendLine("  AND ");
        SQLFilter.AppendLine("  MSTR_Drone.DroneName LIKE '%" + Term + "%'");
      }

      if (SQLFilter.Length > 0) {
        SQL.AppendLine("WHERE");
        SQL.Append(SQLFilter);
      }
      SQL.AppendLine("ORDER BY label");

      StringBuilder Result = new StringBuilder();
      Result.AppendLine("[");
      Result.AppendLine(Util.getDBRowsJson(SQL.ToString()));
      Result.AppendLine("]");
      return Result.ToString();

    }//public String getUAS

    public String getPilots(String Term) {
      StringBuilder SQL = new StringBuilder();
      SQL.AppendLine("SELECT ");
      SQL.AppendLine("  MSTR_User.UserID as value, ");
      SQL.AppendLine("  MSTR_User.FirstName + ' ' + MSTR_User.LastName as label ");
      SQL.AppendLine("FROM ");
      SQL.AppendLine("  MSTR_User");
      if (User.hasAccess("DRONE.MANAGE")) {
        SQL.AppendLine("WHERE \n");
        SQL.AppendLine("  MSTR_User.IsPilot = 1");
      } else {
        SQL.AppendLine("  MSTR_User as AccountHolder");
        SQL.AppendLine("WHERE");
        SQL.AppendLine("  MSTR_User.IsPilot = 1  AND");
        SQL.AppendLine("  MSTR_User.CreatedBy = AccountHolder.UserID and");
        SQL.AppendLine("  AccountHolder.AccountID = " + Util.getAccountID());
      }
      if (!String.IsNullOrEmpty(Term)) {
        SQL.AppendLine("  AND (");
        SQL.AppendLine("  MSTR_User.FirstName LIKE '%" + Term + "%' OR");
        SQL.AppendLine("  MSTR_User.LastName LIKE '%" + Term + "%' )");
      }
      SQL.AppendLine("ORDER BY");
      SQL.AppendLine("  label");

      StringBuilder Result = new StringBuilder();
      Result.AppendLine("[");
      Result.AppendLine(Util.getDBRowsJson(SQL.ToString()));
      Result.AppendLine("]");
      return Result.ToString();
    }//public String getPilots

    /* End of Class closing brackets*/
  }//public class Report
}//namespace eX_Portal.exLogic