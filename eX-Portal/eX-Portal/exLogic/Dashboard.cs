using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace eX_Portal.exLogic {
  public class Dashboard {
    private ExponentPortalEntities ctx;
    public Dashboard() {
      ctx = new ExponentPortalEntities();
      ctx.Database.Connection.Open();
    }


    public List<ChartViewModel>  GetFlightHoursByAccount() {
      List<ChartViewModel> AllChartData = new List<ChartViewModel>();

      DateTime StartDateTime = DateTime.Now.AddMonths(-10);
      DateTime StartAt = new DateTime(StartDateTime.Year, StartDateTime.Month, 1);
      StringBuilder SQLString = new StringBuilder("SELECT\n");
      for(int m = 0; m < 12; m++) {
        String sDate = StartAt.AddMonths(m).ToString("yyyy-MM-dd");
        SQLString.AppendLine($"Sum(CASE WHEN DroneFlight.FlightDate < '{sDate}' THEN DroneFlight.FlightHours ELSE 0 END) As M{m},");
      }
      SQLString.AppendLine(@"
        MSTR_Drone.AccountID
      from 
        DroneFlight,
        MSTR_Drone
      WHERE
        DroneFlight.DroneID = MSTR_Drone.DroneId ");
      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        SQLString.AppendLine($" AND MSTR_Drone.AccountID = {Util.getAccountID()}");
      }
      SQLString.AppendLine(@"GROUP BY MSTR_Drone.AccountID");

      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQLString.ToString();
        cmd.CommandType = CommandType.Text;
        using (var RS = cmd.ExecuteReader()) {
          while (RS.Read()) {
            String AccountName = Util.getDBVal($"SELECT Name From MSTR_Account WHERE AccountID={RS["AccountID"].ToString()}");
            AllChartData.Add(new ChartViewModel() {
              AccountID = RS.GetInt32(RS.GetOrdinal("AccountID")),
              AccountName = AccountName,
              M1 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M0"))) / 60, 2),
              M2 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M1"))) / 60, 2),
              M3 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M2"))) / 60, 2),
              M4 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M3"))) / 60, 2),
              M5 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M4"))) / 60, 2),
              M6 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M5"))) / 60, 2),
              M7 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M6"))) / 60, 2),
              M8 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M7"))) / 60, 2),
              M9 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M8"))) / 60, 2),
              M10 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M9"))) / 60, 2),
              M11 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M10"))) / 60, 2),
              M12 = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("M11"))) / 60, 2)
            });
          }//while(RS.Read())
        }//using(var RS)
      }//using(cmd)
      return AllChartData;
    }


    public List<ChartViewModel> getRecentFlights() {
      List<ChartViewModel> AllChartData = new List<ChartViewModel>();
      String SQL = @"SELECT 
        w.NAME,
        w.ChartColor,
        v.DroneId,
        v.AccountID,
        v.DroneName,
        IsNull((
          SELECT TOP 1 ISNULL(a.flighthours,0)
          FROM droneflight a
          WHERE a.DroneId = v.DroneID
          ORDER BY a.FlightDate DESC
          ),0) AS LastFlightHours,
        IsNull((
          SELECT Sum(ISNULL(b.flighthours,0))
          FROM droneflight b
          WHERE b.DroneId = v.DroneID
            AND b.FlightDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
          ),0) AS LastMonthHours,
        IsNull((
          SELECT Sum(ISNULL(b.flighthours,0))
          FROM droneflight b
          WHERE b.DroneId = v.DroneID
          ),0) AS TotalFlightHours
      FROM MSTR_Drone v
      LEFT JOIN mstr_account w
        ON v.AccountID = w.AccountID";
      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        SQL = SQL + $" WHERE v.AccountID = {Util.getAccountID()}";
      }
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        cmd.CommandType = CommandType.Text;
        using(var RS = cmd.ExecuteReader()) {
          while(RS.Read()) { 
            AllChartData.Add(new ChartViewModel() {
              DroneID = RS.GetInt32(RS.GetOrdinal("DroneID")),
              DroneName = RS["DroneName"].ToString(),
              ShortName = RS["DroneName"].ToString().Split('-').Last(),
              AccountID = RS.GetInt32(RS.GetOrdinal("AccountID")),
              AccountName = RS["NAME"].ToString(),
              ChartColor = RS["ChartColor"].ToString(),
              TotalFightTime = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("TotalFlightHours"))) / 60,2),
              CurrentFlightTime = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("LastMonthHours"))) / 60,2),
              LastFlightTime = Math.Round((Double)(RS.GetInt32(RS.GetOrdinal("LastFlightHours"))) / 60,2)
            });
          }//while(RS.Read())
        }//using(var RS)
      }//using(cmd)

      return AllChartData;
    }
  }
}