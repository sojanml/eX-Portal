using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Reflection;

namespace Exponent.ADSB {
  public class Live {

    private const String APIKey = "";
    private const String ApiID = "";
    private SqlConnection CN;
    private const int NewFlightTime = 5;
    private bool isDemoMode = false;
    public List<FlightPosition> FlightStat(String DSN, bool DemoMode = false, ADSBQuery QueryData = null)  {
      var Data = new List<FlightPosition>();
      isDemoMode = DemoMode;
      if (QueryData == null) QueryData = new ADSBQuery();
      //connect to database for reading data
      using (CN = new SqlConnection(DSN)) {
        CN.Open();
        setActiveFlights();

        //get active positions
        Data = getLivePositions(QueryData);
      }


      return Data;
    }

    public List<FlightSummary> GetSummary(String DSN, int LastProcessedID = 0, int MaxRecords = 20) {
      var TheSummary = new List<FlightSummary>();
      String SQL = @"  SELECT
        *
        FROM (
          SELECT TOP " + MaxRecords + @" * FROM ADSBSummary";
      if (LastProcessedID > 0) SQL = SQL + " WHERE ID > " + LastProcessedID;
        SQL = SQL + @"
        ORDER BY 
          SummaryDate DESC
        ) as InnerTable
       ORDER BY
         SummaryDate";
      using (CN = new SqlConnection(DSN)) {
        CN.Open();
        using (var Cmd = new SqlCommand(SQL, CN)) {
          var RS = Cmd.ExecuteReader();
          while (RS.Read()) {
            TheSummary.Add(new FlightSummary {
              SummaryDate = RS.GetDateTime(RS.GetOrdinal("SummaryDate")).ToString("HH:mm"),
              Breach = RS.GetInt32(RS.GetOrdinal("BreachCount")),
              Alert = RS.GetInt32(RS.GetOrdinal("AlertCount")),
              ID = RS.GetInt32(RS.GetOrdinal("ID"))
            });
          }//while
          RS.Close();
        }//using (var Cmd)

        if(TheSummary.Any()) {
          TheSummary.Last().SetSummary(CN);
        }

        CN.Close();
      }//using (CN)







      return TheSummary;
    }
    public List<FlightStatus> GetFlightStatus(String DSN, Exponent.ADSB.ADSBQuery QueryData) {
      var Dist = new List<FlightStatus>();
      String SQL = @"Select 
        ADSBDetail.FromFlightID,
        ADSBDetail.ToFlightID,
        ADSBDetail.VerticalDistance,
        ADSBDetail.HorizontalDistance,
        CASE
          WHEN HorizontalDistance <= " + QueryData.hBreach + " AND VerticalDistance <= " + QueryData.vBreach + @" Then 'Breach'
          WHEN HorizontalDistance <= " + QueryData.hAlert + " AND VerticalDistance <= " + QueryData.vAlert + @" Then 'Alert'
          Else 'Safe'
        END as StatusModel
      FROM
        ADSBDetail
      Where
        ADSBDetail.HorizontalDistance <= " + QueryData.hSafe + @" AND
        ADSBDetail.VerticalDistance <= " + QueryData.vSafe;


      using (CN = new SqlConnection(DSN)) {
        CN.Open();
        using (var Cmd = new SqlCommand(SQL, CN)) {
          var RS = Cmd.ExecuteReader();
          while (RS.Read()) {
            Dist.Add(new FlightStatus {
              FromFlightID = RS["FromFlightID"].ToString(),
              ToFlightID = RS["ToFlightID"].ToString(),
              vDistance = toDouble(RS["VerticalDistance"].ToString()),
              hDistance = toDouble(RS["HorizontalDistance"].ToString()),
              Status = RS["StatusModel"].ToString()
            });
          }//while
          RS.Close();
        }//using (var Cmd)
        CN.Close();
      }//using (CN)
      return Dist;
    }

    private Double toDouble(String Num) {
      Double dNum = 0;
      Double.TryParse(Num, out dNum);
      return dNum;
    }

    private List<FlightPosition> getLivePositions(Exponent.ADSB.ADSBQuery QueryData = null) {
      if (QueryData == null) QueryData = new ADSBQuery();
      if (QueryData.tracking_adsb_commercial == 0 &
        QueryData.tracking_adsb_rpas == 0 &&
        QueryData.tracking_adsb_skycommander == 0) QueryData.tracking_adsb_commercial = 1;
      var PositionDatas = new List<FlightPosition>();
      StringBuilder Filter = new StringBuilder();
      StringBuilder WHERE = new StringBuilder();
      StringBuilder SQL = new StringBuilder(@"
      select
        [FlightId],
        [Heading],
        [TailNumber],
        [CallSign],
        [Lon],
        [Lat],
        [Speed],
        HeadingHistory,
        [Altitude],
        [AdsbDate]
      from
        AdsbLive
      ");

      if(QueryData.adsb_omdb == 1 || QueryData.adsb_omdw == 1 || QueryData.adsb_omsj == 1) {

        if(QueryData.adsb_omdb == 1) {
          if (Filter.Length > 0) Filter.AppendLine(" OR");
          Filter.Append("[OMDB] <= " + QueryData.ATCRadious);
        }

        if (QueryData.adsb_omdw == 1) {
          if (Filter.Length > 0) Filter.AppendLine(" OR");
          Filter.Append("[OMDW] <= " + QueryData.ATCRadious);
        }

        if (QueryData.adsb_omsj == 1) {
          if (Filter.Length > 0) Filter.AppendLine(" OR");
          Filter.Append("[OMSJ] <= " + QueryData.ATCRadious);
        }

        WHERE.Append("(");
        WHERE.Append(Filter);
        WHERE.Append(")");
        Filter.Clear();
      }

      Filter.Append(QueryData.getTrackingFilter());
      if (Filter.Length > 0) {
        if(WHERE.Length > 0) WHERE.AppendLine(" AND");
        WHERE.Append(Filter);
        Filter.Clear();
      }


      if (WHERE.Length > 0) { 
        SQL.AppendLine(" WHERE");
        SQL.Append(WHERE);
      }


      using (var Cmd = new SqlCommand(SQL.ToString(), CN)) {
         var RS = Cmd.ExecuteReader();
        while(RS.Read()) {
          int fSpeed = RS.GetOrdinal("Speed");
          int fHeading = RS.GetOrdinal("Heading");

          var Position = new FlightPosition {
            FlightID = RS["FlightId"].ToString(),
            Heading = RS.IsDBNull(fHeading) ? 0 : (Double)RS.GetDecimal(fHeading),
            TailNumber = RS["TailNumber"].ToString(),
            CallSign = RS["CallSign"].ToString(),
            Lon = (Double)RS.GetDecimal(RS.GetOrdinal("Lon")),
            Lat = (Double)RS.GetDecimal(RS.GetOrdinal("Lat")),
            Speed = RS.IsDBNull(fSpeed) ? 0 : (Double)RS.GetDecimal(fSpeed),
            Altitude = (Double)RS.GetDecimal(RS.GetOrdinal("Altitude")),
            ADSBDate = RS.GetDateTime(RS.GetOrdinal("AdsbDate")),
            History = getHistory(RS["HeadingHistory"].ToString())
          };
          PositionDatas.Add(Position);
        }//while
      }//using

      return PositionDatas;
    }



    private List<Double> getHistory(String HeadingHistory) {
      List<Double> History = new List<Double>();
      if (String.IsNullOrEmpty(HeadingHistory)) return History;
      foreach (String sHeading in HeadingHistory.Split(',')) {
        Double Heading = 0;
        Double.TryParse(sHeading, out Heading);
        History.Add(Heading);
      }
      return History;
    }

    private void setActiveFlights() {
      String SQL;

      var FlightCount = getActiveFlights();
      if (FlightCount > 0) return;

      //Change for today - 12-Mar-2017 (Do not get data from FlightAware)
      var NewPositions =  getFlightAware();

      //go through each postion and add to database
      foreach (var Position in NewPositions) {
        //Delete if already exists
        SQL = "DELETE FROM AdsbLive WHERE [FlightId]='" + Position.FlightID + "'";
        doSQL(SQL);

        SQL = @"INSERT INTO AdsbLive(
        [FlightId],
        [Heading],
        [TailNumber],
        [CallSign],
        [Lon],
        [Lat],
        [Speed],
        [Altitude],
        [AdsbDate],
        [FlightSource],
        [CreatedDate],
        [HeadingHistory],
        OMDB, 
        OMDW, 
        OMSJ
        ) VALUES (
          '" + Position.FlightID + @"',
          " + Position.Heading + @",
          '" + Position.TailNumber + @"',
          '" + Position.CallSign + @"',
          " + Position.Lon + @",
          " + Position.Lat + @",
          " + Position.Speed + @",
          " + (Position.Altitude * 100) + @",
          '" + Position.ADSBDate.ToString("yyyy-MM-dd hh:mm:ss") + @"',
          '" + Position.FlightSource + @"',
          GETDATE(),
          '" + Position.Heading + @"',
          abs([dbo].[fnCalcDistanceKM](" + Position.Lat + ", " + Airport.OMDB.lat + ", " + Position.Lon + ", " + Airport.OMDB.lng + @")),
          abs([dbo].[fnCalcDistanceKM](" + Position.Lat + ", " + Airport.OMDW.lat + ", " + Position.Lon + ", " + Airport.OMDW.lng + @")),
          abs([dbo].[fnCalcDistanceKM](" + Position.Lat + ", " + Airport.OMSJ.lat + ", " + Position.Lon + ", " + Airport.OMSJ.lng + @"))
        )";
        doSQL(SQL);
      }
    }

    private void doSQL(String SQL) {
      using(var CMD = new SqlCommand(SQL, CN)) {
        CMD.ExecuteNonQuery();
      }
    }
    private int getActiveFlights() {
      int Result = 0;
      String SQL = @"select count(*) from 
        AdsbLive 
      where 
         CreatedDate > (DATEADD(SECOND, -" + NewFlightTime + @", GETDATE())) AND
         FlightSource NOT IN ('SkyCommander')";
      using (var RS = new SqlCommand(SQL, CN)) {
        var ObjResult = RS.ExecuteScalar();
        Result = ObjResult == null ? 0 : (int)ObjResult;
      }
      return Result;
    }


    private List<FlightPosition> getFlightAware() {
      //Timeframe check. available from 6AM to 6 PM Dubai Time
      //2:00 to 14:00 GMT (24 Hour Clock)
      DateTime Now = DateTime.UtcNow;
      var FlightPositions = new List<FlightPosition>();

      if (Now.Hour < 2 || Now.Hour > 14) {
        return FlightPositions;
      }

      String Query = 
        "{> alt 5} " +
        "{> speed 200} " +
        "{true inAir} " +
        "{range lat 23.85438 26.39335} " +
        "{range lon 52.99612 59.31375}";
      String WebURL = "http://flightxml.flightaware.com/json/FlightXML2/" +
        "SearchBirdseyeInFlight" +
        "?query=" + System.Uri.EscapeDataString(Query) +
        "&howMany=30" +
        "&offset=0";

      using (var webClient = new System.Net.WebClient()) {
        webClient.UseDefaultCredentials = true;
        webClient.Credentials = new NetworkCredential("catheythattil", "9f8b8719108bdaa973fe4a96fef5646cd3fd32ea");
        webClient.Encoding = System.Text.Encoding.UTF8;
        String Json2 = isDemoMode ?
          System.IO.File.ReadAllText(getSampleData()) :
          webClient.DownloadString(WebURL);

        dynamic ADSBInfo = JsonConvert.DeserializeObject(Json2);
        var FlightPosition = ADSBInfo.SearchBirdseyeInFlightResult.aircraft;
        for (var i = 0; i < FlightPosition.Count; i++) {
          var thisPos = FlightPosition[i];
          System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
          int UnixTimeStamp = thisPos.timestamp;
          var ADSBDate = dtDateTime.AddSeconds(UnixTimeStamp);

          var thePos = new FlightPosition {
            FlightID = thisPos.ident,
            Heading = thisPos.heading,
            TailNumber = thisPos.faFlightID,
            CallSign = thisPos.faFlightID,
            Lon = thisPos.longitude,
            Lat = thisPos.latitude,
            Speed = thisPos.groundspeed,
            Altitude = thisPos.altitude,
            ADSBDate = ADSBDate,
            FlightSource = "flightaware"
          };
          FlightPositions.Add(thePos);


        }//for
      }//using
      return FlightPositions;

    }

    private String getSampleData() {
      return @"C:\SourceCode\Exponent.ADSB\ADSB\SampleData.json";
    }


  }
}
