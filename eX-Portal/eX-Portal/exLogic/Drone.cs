using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    public String getVideoStartDate(int FlightID) {
      String SQL = "select TOP 1 VideoURL from DroneFlightVideo WHERE FlightID=" + FlightID + " ORDER BY VideoURL ASC";
      String VideoURL = Util.getDBVal(SQL);
      return getVideoDate(VideoURL);

    }


    public String getVideoDate(String VideoFileName) {
      String TheDate = "new Date(1970,0,0,24,0,0)"; 
      //drone100-2016-03-24T10-38-47.flv
      Match m = Regex.Match(VideoFileName, @"\-(\d+)-(\d+)-(\d+)T(\d+)-(\d+)-(\d+)");
      if (m.Groups.Count == 7) {
        TheDate = "new Date(" +
        int.Parse(m.Groups[1].Value) + "," +
        (int.Parse(m.Groups[2].Value) - 1) + "," +
        int.Parse(m.Groups[3].Value) + "," +
        int.Parse(m.Groups[4].Value) + "," +
        int.Parse(m.Groups[5].Value) + "," +
        int.Parse(m.Groups[6].Value) +
        ")";
      }
      return TheDate;
    }

    public String parseDate(String VideoFileName) {

      String TheDate = DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss");
      //drone100-2016-03-24T10-38-47.flv
      Match m = Regex.Match(VideoFileName, @"\-(\d+)-(\d+)-(\d+)T(\d+)-(\d+)-(\d+)");
      if (m.Groups.Count == 7) {
        TheDate = m.Groups[1].Value + "-" +
        m.Groups[2].Value + "-" +
        m.Groups[3].Value +
        " " +
        m.Groups[4].Value + ":" +
        m.Groups[5].Value + ":" +
        m.Groups[6].Value;
      }

      return TheDate;
    }


    public int getDroneIDForFlight(int FlightID) {
      String SQL = "Select DroneID From DroneFlight WHERE ID=" + FlightID;
      return Util.getDBInt(SQL);
    }

    public String getAllowedLocation(int FlightID) {
      _DroneID = getDroneIDForFlight(FlightID);

      StringBuilder Cord = new StringBuilder();
      String SQL = @"SELECT 
        ApprovalID,
        Coordinates,
        InnerBoundaryCoord
      FROM
        [GCA_Approval]
      WHERE
        DroneID = " + _DroneID + @" AND
        GETDATE() BETWEEN StartDate and EndDate";

      var Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {

        String Coordinates = Row["Coordinates"].ToString();
        if (Cord.Length > 0) Cord.AppendLine(",");
        Cord.AppendLine("[");
        Cord.Append(toScriptArray(Coordinates));
        Cord.AppendLine("]");

        //Inner cordinates
        String InnerCoordinates = Row["InnerBoundaryCoord"].ToString();
        if (Cord.Length > 0) Cord.AppendLine(",");
        Cord.AppendLine("[");
        Cord.Append(toScriptArray(InnerCoordinates));
        Cord.AppendLine("]");
      }

      return Cord.ToString();
    }


    private StringBuilder toScriptArray(String Coordinates) {
      Coordinates = Coordinates.Replace("POLYGON ((", "");
      Coordinates = Coordinates.Replace("))", "");

      StringBuilder thisCord = new StringBuilder();
      foreach (String LatLng in Coordinates.Split(',')) {
        var tLatLng = LatLng.Trim();
        String[] temp = tLatLng.Split(' ');
        if (thisCord.Length > 0) thisCord.Append(",");
        thisCord.Append("new google.maps.LatLng(" + temp[0] + "," + temp[1] + ")");
      }

      return thisCord;
    }

    public List<DroneFlight> getLogFlights(List<DroneFlight> Flights, int FlightID = 0) {
      String SQL = FlightID == 0 ?
        "SELECT\n" +
        "  DroneFlight.ID,\n" +
        "  LogFrom,\n" +
        "  LogTo,\n" +
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 111) as 'FlightDate(UTC)',\n" +
        "  Convert(Varchar, Min(FlightMapData.ReadTime), 108) as LogTakeOffTime,\n" +
        "  Convert(Varchar, Max(FlightMapData.ReadTime), 108) as LogLandingTime,\n" +
        "  Convert(Varchar, DATEADD(\n" +
        "     Minute,\n" +
        "     DATEDIFF(MINUTE, Min(FlightMapData.ReadTime), Max(FlightMapData.ReadTime)),\n" +
        "     '2000-01-01 00:00:00'), 108) as Duration,\n" +
        "  FlightMapData.BBFlightID,\n" +
        "  '' as LogBattery1ID,\n" +
        "  '' as   LogBattery1StartV,\n" +
        "  '' as   LogBattery1EndV,\n" +
        "  '' as   LogBattery2ID,\n" +
        "  '' as   LogBattery2StartV,\n" +
        "  '' as   LogBattery2EndV,\n" +
        "  '' as   Descrepency,\n" +
        "  '' as   ActionTaken\n" +
        "From\n" +
        "  DroneFlight\n" +
        "LEFT JOIN FlightMapData On\n" +
        "  FlightMapData.FlightID = DroneFlight.ID\n" +
        "WHERE\n" +
        "  DroneFlight.DroneID = " + DroneID.ToString() + " AND\n" +
        "    (isLogged IS NULL OR isLogged = 0)\n" +
        "GROUP BY\n" +
        "  DroneFlight.ID,\n" +
        "  DroneFlight.DroneID,\n" +
        "  FlightMapData.BBFlightID,\n" +
        "  LogFrom,\n" +
        "  LogTo\n"
        :
        "SELECT\n" +
        "  DroneFlight.ID,\n" +
        "  LogFrom,\n" +
        "  LogTo,\n" +
        "  Convert(Varchar(11), FlightDate, 111) as 'FlightDate(UTC)',\n" +
        "  Convert(Varchar, LogTakeOffTime, 108) as LogTakeOffTime,\n" +
        "  Convert(Varchar, LogLandingTime, 108) as LogLandingTime,\n" +
        "  Convert(Varchar, DATEADD(\n" +
        "     Minute,\n" +
        "     DATEDIFF(MINUTE, LogTakeOffTime, LogLandingTime),\n" +
        "     '2000-01-01 00:00:00'), 104) as Duration,\n" +
        "  LogBattery1ID,\n" +
        "  LogBattery1StartV,\n" +
        "  LogBattery1EndV,\n" +
        "  LogBattery2ID,\n" +
        "  LogBattery2StartV,\n" +
        "  LogBattery2EndV,\n" +
        "  Descrepency,\n" +
        "  ActionTaken,\n" +
        "  '0' as BBFlightID\n" +
        "From\n" +
        "  DroneFlight\n" +
        "WHERE\n" +
        "  ID=" + FlightID;

      var Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        DroneFlight Flight = new DroneFlight() {
          ID = Util.toInt(Row["ID"].ToString()),
          LogFrom = Row["LogFrom"].ToString(),
          LogTo = Row["LogFrom"].ToString(),
          LogTakeOffTime = Util.toDate(Row["FlightDate"].ToString() + " " + Row["LogTakeOffTime"].ToString()),
          LogLandingTime = Util.toDate(Row["FlightDate"].ToString() + " " + Row["LogLandingTime"].ToString()),
          FlightDate = Util.toDate(Row["FlightDate"].ToString()),
          LogBattery1ID = Row["LogBattery1ID"].ToString(),
          LogBattery1StartV = Util.toDecimal(Row["LogBattery1StartV"].ToString()),
          LogBattery1EndV = Util.toDecimal(Row["LogBattery1EndV"].ToString()),
          LogBattery2ID = Row["LogBattery2ID"].ToString(),
          LogBattery2StartV = Util.toDecimal(Row["LogBattery2StartV"].ToString()),
          LogBattery2EndV = Util.toDecimal(Row["LogBattery2EndV"].ToString()),
          Descrepency = Row["Descrepency"].ToString(),
          ActionTaken = Row["ActionTaken"].ToString()
        };
        Flights.Add(Flight);
      }
      return Flights;
    }//getFlights()

    public void saveTechnicalLog(HttpRequestBase Request) {
      String SQL;
      String[] Index = Request["theFlight.Index"].Split(',');
      foreach (String RowID in Index) {
        if (RowID == "SLNO") continue;
        String FlightID = Request["theFlight[" + RowID + "].ID"];
        String LogTakeOffTime = Request["theFlight[" + RowID + "].FlightDate"] + " " +
                         Request["theFlight[" + RowID + "].LogTakeOffTime"];
        String LogLandingTime = Request["theFlight[" + RowID + "].FlightDate"] + " " +
                         Request["theFlight[" + RowID + "].LogLandingTime"];

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
          "  [LogCreatedBy],\n" +
          "  [Descrepency],\n" +
          "  [ActionTaken]\n" +
          ") VALUES (" +
          "  " + Util.toInt(Request["DroneID"]) + ",\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  '" + Request["theFlight[" + RowID + "].LogFrom"] + "',\n" +
          "  '" + Request["theFlight[" + RowID + "].LogTo"] + "',\n" +
          "  '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
          "  '" + Util.toSQLDate(LogLandingTime) + "',\n" +
          "  '" + Request["theFlight[" + RowID + "].LogBattery1ID"] + "',\n" +
          "  " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery1StartV"]) + ",\n" +
          "  " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery1EndV"]) + ",\n" +
          "  '" + Request["theFlight[" + RowID + "].LogBattery2ID"] + "',\n" +
          "  " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery2StartV"]) + ",\n" +
          "  " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery2EndV"]) + ",\n" +
          "  1,\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  '" + Request["theFlight[" + RowID + "].Descrepency"] + "',\n" +
          "  '" + Request["theFlight[" + RowID + "].ActionTaken"] + "'\n" +
          ")";
          Util.doSQL(SQL);
        } else { //if(FlightID == "0")
                 //Update flight information for technical Log
          SQL =
         "UPDATE\n" +
         "  DroneFlight\n" +
         "SET\n" +
         "  [LogFrom] = '" + Request["theFlight[" + RowID + "].LogFrom"] + "',\n" +
         "  [LogTo] = '" + Request["theFlight[" + RowID + "].LogTo"] + "',\n" +
         "  [LogTakeOffTime] = '" + Util.toSQLDate(LogTakeOffTime) + "',\n" +
         "  [LogLandingTime] = '" + Util.toSQLDate(LogLandingTime) + "',\n" +
         "  [LogBattery1ID] = '" + Request["theFlight[" + RowID + "].LogBattery1ID"] + "',\n" +
         "  [LogBattery1StartV] = " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery1StartV"]) + ",\n" +
         "  [LogBattery1EndV] = " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery1EndV"]) + ",\n" +
         "  [LogBattery2ID] = '" + Request["theFlight[" + RowID + "].LogBattery2ID"] + "',\n" +
         "  [LogBattery2StartV] = " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery2StartV"]) + ",\n" +
         "  [LogBattery2EndV] = " + Util.toDecimal(Request["theFlight[" + RowID + "].LogBattery2EndV"]) + ",\n" +
         "  [isLogged] = 1,\n" +
         "  [LogDateTime] = GETDATE(),\n" +
         "  LogCreatedBy=" + Util.getLoginUserID() + ",\n" +
         "  Descrepency = '" + Request["theFlight[" + RowID + "].Descrepency"] + "',\n" +
         "  ActionTaken = '" + Request["theFlight[" + RowID + "].ActionTaken"] + "'\n" +
         "WHERE\n" +
         "  ID=" + FlightID;
          Util.doSQL(SQL);
        }//if(FlightID == "0")

      }//foreach(Index)
    }//saveTechnicalLog()


    public String getLiveURL(int FlightID) {
      String VideoURL = String.Empty;
      if (_DroneID == 0) _DroneID = getDroneIDForFlight(FlightID);
      //Find the drone is live
      VideoURL = "rtmp://52.29.242.123/live/drone" + _DroneID;
      return VideoURL;
    }

    public bool isLive(int FlightID) {
      if (_DroneID == 0) _DroneID = getDroneIDForFlight(FlightID);

      String SQL = "SELECT IsLiveVideo FROM MSTR_Drone WHERE DroneID=" + _DroneID;
      int IsLiveVideo = Util.getDBInt(SQL);
      if (IsLiveVideo == 1) {
        int LastFlightID = getLastFlightID(_DroneID);
        if (LastFlightID == FlightID) return true;
      }
      return false;
    }

    public int getLastFlightID(int DroneID) {
      String SQL = "SELECT Max(ID) FROM DroneFlight WHERE DroneID=" + DroneID;
      return Util.getDBInt(SQL);
    }

  
    public String getPlayListURL(int FlightID) {
      String VideoURL = String.Empty;
      int MovieCount = getPlayListCount(FlightID);
      if (MovieCount > 0) {
        VideoURL = "'/Map/PlayList/" + FlightID + "'";
      }
      return VideoURL;
    }

    public String getPlayList(int FlightID) {
      String SQL = "select VideoURL from DroneFlightVideo WHERE FlightID=" + FlightID;

      StringBuilder JSon = new StringBuilder();
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              if (JSon.Length > 0) JSon.AppendLine(",");
              JSon.Append("{");
              JSon.Append("\"file\":\"");
              JSon.Append(reader["VideoURL"].ToString());
              JSon.Append("\",\n");
              JSon.Append("\"title\":\"");
              JSon.Append("https://exponent-s3.s3-us-west-2.amazonaws.com/VOD/" + reader["VideoURL"].ToString());
              JSon.Append("\"}\n");
            }//if
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using (var ctx = new ExponentPortalEntities())

      return JSon.ToString();
    }

    public int getPlayListCount(int FlightID) {
      String SQL = "select Count(*) from DroneFlightVideo WHERE FlightID=" + FlightID;
      return Util.getDBInt(SQL);
    }

    public String getLiveUAS() {
      String SQL;

      SQL = @"IF OBJECT_ID('tempdb..#TempLiveDrons') IS NOT NULL
        DROP TABLE #TempLiveDrons;";
      Util.doSQL(SQL);

      SQL = @"select 
        DroneID,
        max(FlightMapDataID) as FlightMapDataID
      INTO 
        #TempLiveDrons
      from 
        FlightMapData
      WHERE
        CreatedTime > DATEADD(month, -1, GETDATE())
      Group BY
        DroneID";
      Util.doSQL(SQL);

      SQL = @"SELECT 
        MSTR_Drone.DroneID,
        FlightMapData.FlightID,
        MSTR_Drone.AccountID,
        MSTR_Drone.DroneName,
        UAVType.Name as UAVType,
        FlightMapData.Latitude,
        FlightMapData.Longitude,
        FlightMapData.ReadTime,
        FlightMapData.Speed,
        FlightMapData.Altitude,
        FlightMapData.TotalFlightTime
      FROM
        FlightMapData,
        #TempLiveDrons,
        MSTR_Drone
      LEFT JOIN LUP_Drone AS UAVType ON
        UAVType.Type='UAVType' AND
        MSTR_Drone.UavTypeId = UAVType.TypeID
      WHERE
        FlightMapData.FlightMapDataID = #TempLiveDrons.FlightMapDataID AND
        MSTR_Drone.DroneID = FlightMapData.DroneID
      ";
      return Util.getDBRowsJson(SQL);
    }

  }//class Drone
}//namespace eX_Portal.exLogic