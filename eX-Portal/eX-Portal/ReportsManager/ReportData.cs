using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Models {
  public class ReportData {
    public List<MSTR_Drone> get_MSTR_Drone() {
      using (var db = new ExponentPortalEntities()) {
        var Query = (from n in db.MSTR_Drone select n);
        var Data = Query.ToList();
        return Data;
      }
    }

    public List<MSTR_User> MSTR_User() {
      using (var db = new ExponentPortalEntities()) {
        return (from n in db.MSTR_User select n).ToList();
      }
    }

    public List<AlertReportData> getAlertReportData(FlightReportFilter Filter = null) {
      var Request = HttpContext.Current.Request;
      var thisReport = new exLogic.Report();
      var Records = new List<AlertReportData>();

      if (Filter == null) Filter = new FlightReportFilter();
      var SQL = thisReport.getAlertSQL(Filter);
      SQL = SQL + "\nORDER BY PortalAlert.CreatedOn DESC";
      using (var db = new ExponentPortalEntities()) {
        using (var cmd = db.Database.Connection.CreateCommand()) {
          db.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              var TheRow = new AlertReportData {
                 AlertID = reader.GetInt32(reader.GetOrdinal("AlertID")),
                 FlightID = reader.GetInt32(reader.GetOrdinal("FlightID")),
                 CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                 DroneName = reader["DroneName"].ToString(),
                 Pilot = reader["Pilot"].ToString(),
                 SMS = reader["Pilot"].ToString(),
                 AlertCategory = reader["AlertCategory"].ToString(),
                 AlertType = reader["AlertType"].ToString(),
                 Latitude = reader.GetDecimal(reader.GetOrdinal("Latitude")),
                Longitude = reader.GetDecimal(reader.GetOrdinal("Longitude")),
                Altitude = reader.GetInt32(reader.GetOrdinal("Altitude"))
              };
              Records.Add(TheRow);
            }//while
          }//using reader
        }
      }//using (var db = new ExponentPortalEntities())

      return Records;
    }

    public List<FlightReportData>getFlightReportData(FlightReportFilter Filter = null) {
      var Request = HttpContext.Current.Request;
      var thisReport = new exLogic.Report();
      var Records = new List<FlightReportData>();

      if (Filter == null) Filter = new FlightReportFilter();
      var SQL = thisReport.getFlightReportSQL(Filter, true) ;
      SQL = SQL + "\nORDER BY DroneFlight.ID DESC";
      using (var db = new ExponentPortalEntities()) {
        using (var cmd = db.Database.Connection.CreateCommand()) {
          db.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              var TheRow = new FlightReportData {
                ID = Util.toInt(reader["Ref"]),
                FlightDate = reader.GetDateTime(reader.GetOrdinal("FlightDate")),
                FlightTime = reader["FlightTime"].ToString(),
                Pilot = reader["Pilot"].ToString(),
                UAS = reader["UAS"].ToString(),
                BoundaryAlerts = reader["BoundaryAlerts"].ToString(),
                HeightAlerts = reader["AltitudeAlerts"].ToString(),
                ProximityAlerts = reader["ProximityAlerts"].ToString(),
                MaxAltitude = reader["MaxAltitude"].ToString(),
                Height = Util.toInt(reader["Height"]),
                HeightCritical = Util.toInt(reader["HeightCritical"]),
                Boundary = Util.toInt(reader["Boundary"]),
                BoundaryCritical = Util.toInt(reader["BoundaryCritical"]),
                Proximity = Util.toInt(reader["Proximity"]),
                ProximityCritical = Util.toInt(reader["ProximityCritical"])
              };
              Records.Add(TheRow);
            }//while
          }//using reader
        }
      }//using (var db = new ExponentPortalEntities())

      return Records;
    }//getFlightReportData



  }//public class ReportData
}//namespace eX_Portal.Models