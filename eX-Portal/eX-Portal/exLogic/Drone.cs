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
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 111) as FlightDate,\n" + 
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 108) as LogTakeOffTime,\n" +
        "  Convert(Varchar, Max(FlightMapData.ReadTime), 108) as LogLandingTime,\n" +
        "  Convert(Varchar, DATEADD(\n" +
        "     Minute,\n" +
        "     DATEDIFF(MINUTE, Min(FlightMapData.ReadTime), Max(FlightMapData.ReadTime)),\n" +
        "     '2000-01-01 00:00:00'), 108) as Duration,\n" +
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
          LogTakeOffTime = Util.toDate(Row["FlightDate"].ToString() + " " + Row["LogTakeOffTime"].ToString()),
          LogLandingTime = Util.toDate(Row["FlightDate"].ToString() + " " + Row["LogLandingTime"].ToString()),
          FlightDate = Util.toDate(Row["FlightDate"].ToString())
        };
        Flights.Add(Flight);
      }
      return Flights;
    }//getFlights()

    public void saveTechnicalLog(HttpRequestBase Request) {
      String SQL;
      String[] Index = Request["DroneFlight.Index"].Split(',');
      foreach(String RowID in Index) {
        if (RowID == "SLNO") continue;
        String FlightID = Request["DroneFlight[" + RowID + "].ID"];
        String LogTakeOffTime = Request["DroneFlight[" + RowID + "].FlightDate"] + " " +
                         Request["DroneFlight[" + RowID + "].LogTakeOffTime"];
        String LogLandingTime = Request["DroneFlight[" + RowID + "].FlightDate"] + " " +
                         Request["DroneFlight[" + RowID + "].LogLandingTime"];

        if (FlightID == "0") {
          //create new flight for drone
          SQL =
          "INSERT INTO DroneFlight (\n" +
          "  DroneID,\n" +
          "  PilotID,\n" +
          "  FlightDate,\n" +
          "  CreatedOn,\n" +
          "  CreatedBy,\n" +
          "  [LogFrom],\n" +
          "  [LogTo],\n" +
          "  [LogTakeOffTime],\n" +
          "  [LogLandingTime],\n" +
          "  [LogBattery1ID],\n" +
          "  [LogBattery1StartV],\n" +
          "  [LogBattery1EndV],\n" +
          "  [LogBattery2ID],\n" +
          "  [LogBattery2StartV],\n" +
          "  [LogBattery2EndV],\n" +
          "  [isLogged],\n" +
          "  [LogDateTime],\n" +
          "  [LogCreatedBy]\n" +
          ") VALUES (" +
          "  " + Util.toInt(Request["DroneID"]) + ",\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  '" + Request["DroneFlight[" + RowID + "].LogFrom"] + "',\n" +
          "  '" + Request["DroneFlight[" + RowID + "].LogTo"] + "',\n" +
          "  '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
          "  '" + Util.toSQLDate(LogLandingTime) + "',\n" +
          "  '" + Request["DroneFlight[" + RowID + "].LogBattery1ID"] + "',\n" +
          "  " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery1StartV"]) + ",\n" +
          "  " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery1EndV"]) + ",\n" +
          "  '" + Request["DroneFlight[" + RowID + "].LogBattery2ID"] + "',\n" +
          "  " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery2StartV"]) + ",\n" +
          "  " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery2EndV"]) + ",\n" +
          "  1,\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + "\n" +
          ")";
          Util.doSQL(SQL);
        } else { //if(FlightID == "0")
          //Update flight information for technical Log
           SQL = 
          "UPDATE\n" +
          "  DroneFlight\n" +
          "SET\n" +
          "  [LogFrom] = '" + Request["DroneFlight[" + RowID + "].LogFrom"] + "',\n" +
          "  [LogTo] = '" + Request["DroneFlight[" + RowID + "].LogTo"] + "',\n" +
          "  [LogTakeOffTime] = '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
          "  [LogLandingTime] = '" + Util.toSQLDate(LogLandingTime) + "',\n" +
          "  [LogBattery1ID] = '" + Request["DroneFlight[" + RowID + "].LogBattery1ID"] + ",\n" +
          "  [LogBattery1StartV] = " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery1StartV"]) + ",\n" +
          "  [LogBattery1EndV] = " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery1EndV"]) + "',\n" +
          "  [LogBattery2ID] = '" + Request["DroneFlight[" + RowID + "].LogBattery2ID"] + "',\n" +
          "  [LogBattery2StartV] = " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery2StartV"]) + ",\n" +
          "  [LogBattery2EndV] = " + Util.toDecimal(Request["DroneFlight[" + RowID + "].LogBattery2EndV"]) + ",\n" +
          "  [isLogged] = 1,\n" +
          "  [LogDateTime] = GETDATE(),\n" +
          "  LogCreatedBy=" + Util.getLoginUserID() + "\n" +
          "WHERE\n" +
          "  ID=" + FlightID;
          Util.doSQL(SQL);
        }//if(FlightID == "0")

}//foreach(Index)
    }//saveTechnicalLog()


  }//class Drone
}//namespace eX_Portal.exLogic