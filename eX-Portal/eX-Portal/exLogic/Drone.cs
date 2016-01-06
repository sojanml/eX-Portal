using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public class Drones {
    private int _DroneID = 0;
    public int DroneID {
      get {
        return _DroneID;
      }
      set {
        _DroneID = value;
      }
    }

    public List<DroneFlight> getLogFlights(List<DroneFlight> Flights, bool isNew = false) {
      String SQL = isNew ?
        "SELECT\n" +
        "  droneflight.ID,\n" + 
        "  LogFrom,\n" +
        "  LogTo,\n" +
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 111) as LogDate,\n" + 
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 108) as LogTakeOffTime,\n" +
        "  Convert(Varchar, Max(FlightMapData.ReadTime), 108) as LogLandingTime,\n" +
        "  Convert(Varchar, DATEADD(\n" +
        "     Minute,\n" +
        "     DATEDIFF(MINUTE, Min(FlightMapData.ReadTime), Max(FlightMapData.ReadTime)),\n" +
        "     '2000-01-01 00:00:00'), 104) as Duration,\n" +
        "  FlightMapData.BBFlightID\n" +
        "From\n" +
        "  droneflight\n" +
        "LEFT JOIN FlightMapData On\n" +
        "  FlightMapData.FlightID = droneflight.ID\n" +
        "WHERE\n" +
        "  droneflight.DroneID = " + DroneID.ToString() + " AND\n" +
        "    (isLogged IS NULL OR isLogged = 0)\n" +
        "GROUP BY\n" +
        "  droneflight.ID,\n" +
        "  droneflight.DroneID,\n" +
        "  FlightMapData.BBFlightID,\n" +
        "  LogFrom,\n" +
        "  LogTo\n"
        :
        "SELECT\n" +
        "  droneflight.ID\n" +
        "  LogFrom,\n" +
        "  LogTo,\n" +
        "  LogTakeOffTime,\n" +
        "  LogLandingTime,\n" +
        "  Convert(Varchar, DATEADD(\n" +
        "     Minute,\n" +
        "     DATEDIFF(MINUTE, Min(LogTakeOffTime), Max(LogLandingTime)),\n" +
        "     '2000-01-01 00:00:00'), 104) as Duration\n" +
        "From\n" +
        "  droneflight\n" +
        "WHERE\n" +
        "  1 = 0\n";

      var Rows = Util.getDBRows(SQL);
      foreach(var Row in Rows) {
        DroneFlight Flight = new DroneFlight() {
          ID = Util.toInt(Row["ID"].ToString()),
          LogFrom = Row["LogFrom"].ToString(),
          LogTo = Row["LogFrom"].ToString(),
          LogTakeOffTime = Util.toTime(Row["LogTakeOffTime"].ToString()),
          LogLandingTime = Util.toTime(Row["LogLandingTime"].ToString())
        };
        Flights.Add(Flight);
      }
      return Flights;
    }//getFlights()


  }//class Drone
}//namespace eX_Portal.exLogic