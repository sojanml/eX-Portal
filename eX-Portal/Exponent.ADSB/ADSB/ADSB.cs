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
    private const int NewFlightTime = 120;
    private bool isDemoMode = false;
    public List<FlightPosition> FlightStat(String DSN, bool DemoMode = false)  {
      var Data = new List<FlightPosition>();
      isDemoMode = DemoMode;

      //connect to database for reading data
      using (CN = new SqlConnection(DSN)) {
        CN.Open();
        //setActiveFlights();

        //get active positions
        Data = getLivePositions();
      }


      return Data;
    }

    public List<FlightDistance> FlightDist(String DSN, Double hDistance = 5, Double vDistance = 2) {
      var Dist = new List<FlightDistance>();
      String SQL = @"Select 
        ADSBDetail.FromFlightID,
        ADSBDetail.ToFlightID,
        ADSBDetail.VerticalDistance,
        ADSBDetail.HorizontalDistance
      FROM
        ADSBDetail
      Where
        ADSBDetail.HorizontalDistance <= " + hDistance + @" AND
        ADSBDetail.VerticalDistance <= " + vDistance;
      using (CN = new SqlConnection(DSN)) {
        CN.Open();
        using (var Cmd = new SqlCommand(SQL, CN)) {
          var RS = Cmd.ExecuteReader();
          while (RS.Read()) {
            Dist.Add(new FlightDistance {
              FromFlightID = RS["FromFlightID"].ToString(),
              ToFlightID = RS["ToFlightID"].ToString(),
              vDistance = toDouble(RS["VerticalDistance"].ToString()),
              hDistance = toDouble(RS["HorizontalDistance"].ToString())
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

    private List<FlightPosition> getLivePositions() {
      var PositionDatas = new List<FlightPosition>();
      String SQL = @"
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
        AdsbLive";
      using(var Cmd = new SqlCommand(SQL, CN)) {
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
      var NewPositions =  getFlightAware();

      //remove any old records from LIVE tabe
      //SQL = "DELETE FROM AdsbLive WHERE  CreatedDate < (DATEADD(SECOND, -120, GETDATE()))";
      //doSQL(SQL);

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
        [HeadingHistory]
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
          Position.Heading
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
      String SQL = "select count(*) from AdsbLive where CreatedDate > (DATEADD(SECOND, -" + NewFlightTime + ", GETDATE()))";
      using(var RS = new SqlCommand(SQL, CN)) {
        var ObjResult = RS.ExecuteScalar();
        Result = ObjResult == null ? 0 : (int)ObjResult;
      }
      return Result;
    }


    private List<FlightPosition> getFlightAware() {
      var FlightPositions = new List<FlightPosition>();
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
