using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exponent.ADSB {

  public static class Airport {
    public static Coordinate OMDB = new Coordinate { lat = 25.2532, lng = 55.3657 };
    public static Coordinate OMDW = new Coordinate { lat = 24.8978, lng = 55.1431 };
    public static Coordinate OMSJ = new Coordinate { lat = 25.3284, lng = 55.5123 };
  }

public class ADSBQuery {
    public Double ATCRadious { get; set; }
    public Double hSafe { get; set; }
    public Double vSafe { get; set; }
    public Double hAlert { get; set; }
    public Double vAlert { get; set; }
    public Double hBreach { get; set; }
    public Double vBreach { get; set; }
    public int tracking_adsb_commercial { get; set; }
    public int tracking_adsb_rpas { get; set; }
    public int tracking_adsb_skycommander { get; set; }
    public int adsb_omdb { get; set; }
    public int adsb_omdw { get; set; }
    public int adsb_omsj { get; set; }

    public ADSBQuery() {
      ATCRadious = 150;
      hSafe = 10;
      vSafe = 4;
      hAlert = 4;
      vAlert = 3;
      hBreach = 2;
      vBreach = 2;
      tracking_adsb_commercial = 0;
      tracking_adsb_rpas = 0;
      tracking_adsb_skycommander = 0;
      adsb_omdb = 0;
      adsb_omdw = 0;
      adsb_omsj = 0;
    }

    public String getTrackingFilter() {
      String TheFilter = String.Empty;
      if((tracking_adsb_commercial == 1 && tracking_adsb_skycommander == 1 && tracking_adsb_rpas == 1) ||
          (tracking_adsb_commercial == 1 && tracking_adsb_skycommander == 1)) {
        TheFilter = "  AdsbLive.FlightSource IN ('Exponent', 'SkyCommander','FlightAware')";
      } else if(tracking_adsb_commercial == 1 && tracking_adsb_rpas == 1) {
        TheFilter = "  (AdsbLive.FlightSource IN ('Exponent', 'FlightAware') OR AdsbLive.HexCode LIKE 'A000%')";
      } else if (tracking_adsb_commercial == 1) {
        TheFilter = "  AdsbLive.FlightSource IN ('Exponent', 'FlightAware')";
      } else if (tracking_adsb_rpas == 1) {
        TheFilter = "  AdsbLive.HexCode LIKE 'A000%'";
      } else if (tracking_adsb_skycommander == 1) {
        TheFilter = "  AdsbLive.FlightSource = 'SkyCommander'";
      }
      return TheFilter;
    }
  }
  public class FlightPosition {
    public String FlightID { get; set; }
    public Double Heading { get; set; }
    public string TailNumber { get; set; }
    public string FlightSource { get; set; }
    public string CallSign { get; set; }
    public Double Lon { get; set; }
    public Double Lat { get; set; }
    public Double Speed { get; set; }
    public Double Altitude { get; set; }
    public DateTime ADSBDate { get; set; }


    public List<Double> History { get; set; }

    public FlightPosition() {
      History = new List<Double>();

    }

  }

  public class FlightStatus {
    public String FromFlightID { get; set; }
    public String ToFlightID { get; set; }
    public Double vDistance { get; set; }
    public Double hDistance { get; set; }
    public String Status { get; set; }
  }

  public class FlightSummary {
    public String SummaryDate { get; set; }
    public int Breach { get; set; }
    public int Alert { get; set; }
    public int ID { get; set; }
  }


  public class Coordinate {
    public Double lat { get; set; }
    public Double lng { get; set; }
  }


  public class ADSBData {

    public string hex { get; set; }
    public string altitude { get; set; }
    public double seen { get; set; }
    public double speed { get; set; }
    public string squawk { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
    public double track { get; set; }
    public double vert_rate { get; set; }
    public string flight { get; set; }
    private DateTime ADSBDate { get; set; }
    public String flightsource { get; set; }

    private int ID { get; set; }
    private double dbLat { get; set; }
    private double dbLon { get; set; }
    private String LatLonHistory { get; set; }
    private String HeadingHistory { get; set; }


    public ADSBData() {
      ID = 0;
      flightsource = "Exponent";
    }

    public double Alt() {
      double temp = 0.0;
      Double.TryParse(altitude, out temp);
      return temp;
    }
    public void Setflightdate(long timeinsecond) {
      ADSBDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      ADSBDate = ADSBDate.AddSeconds(timeinsecond);
      ADSBDate = ADSBDate.AddMilliseconds(-1 * seen);
    }

    public void Setflightdate(DateTime Date) {
      ADSBDate = Date;
    }

    public bool InsertADSBSingle(SqlConnection sqlCon) {
      if (!String.IsNullOrEmpty(hex)) hex = hex.Trim();
      if (String.IsNullOrEmpty(flight)) flight = hex;
      if (!String.IsNullOrEmpty(flight)) flight = flight.Trim();
      bool result = true;
      if (lat == 0 && lon == 0.0) return false;
      setDataFromDb(sqlCon);

      if (ID == 0) {
        InsertRow(sqlCon);
      } else {
        UpdateRow(sqlCon);
      }
      AddToHistory(sqlCon);
      return result;
    }

    private void setDataFromDb(SqlConnection CN) {
      string SQL = "Select  " +
        "  ID, Lat, Lon, " +
        "  ISNULL(HeadingHistory,'') as HeadingHistory " +
        "from " +
        "  ADSBLive " +
        "where " +
        "  hexcode='" + hex + "' or flightid='" + flight + "'";
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        using (SqlDataReader RS = cmd.ExecuteReader()) {
          while (RS.Read()) {
            ID = RS.GetInt32(RS.GetOrdinal("ID"));
            dbLat = (Double)RS.GetDecimal(RS.GetOrdinal("Lat"));
            dbLon = (Double)RS.GetDecimal(RS.GetOrdinal("Lon"));
            HeadingHistory = RS["HeadingHistory"].ToString();
          }//while(RS.Read())
          RS.Close();
        }//using(SqlDataReader RS)
      }//using (SqlCommand cmd)

    }

    private void InsertRow(SqlConnection CN) {
      string SQL = @"Insert into ADSBLive(
        Adsbdate, Lat, Lon, CallSign, Altitude, Speed, heading, flightid, flightsource, TailNumber, hexcode,
        HeadingHistory, IsCalculated,
        OMDB, OMDW, OMSJ
        ) values (" +
        "  '" + ADSBDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'," +
        lat + "," +
        lon + "," +
        "'" + flight + "'," +
        Alt() + "," +
        speed + "," +
        track + "," +
        "'" + flight + "'," +
        "'" + flightsource + "'," +
        "'" + flight + "'," +
        "'" + hex + "'," +
        "'" + track + "'," +
        "0," +
        "abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMDB.lat + ", " + lon + ", " + Airport.OMDB.lng + ")),\n" +
        "abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMDW.lat + ", " + lon + ", " + Airport.OMDW.lng + ")),\n" +
        "abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMSJ.lat + ", " + lon + ", " + Airport.OMSJ.lng + "))\n" +
        ")";
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        cmd.ExecuteNonQuery();
      }
    }

    private void UpdateRow(SqlConnection CN) {
      StringBuilder SB = new StringBuilder(HeadingHistory);
      if (dbLat == lat && dbLon == lon) {
        //keep the history. do not change
      } else {
        int HistoryCount = HeadingHistory.Count(c => c == ',');
        if (HistoryCount >= 5) SB.Remove(0, HeadingHistory.IndexOf(',') + 1);
        if (SB.Length > 0) SB.Append(',');
        SB.Append(track);
      }

      String SQL = @"UPDATE ADSBLive SET
        Adsbdate='" + ADSBDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"',
        Lat = " + lat + @", 
        Lon = " + lon + @", 
        Altitude = " + Alt() + @", 
        Speed = " + speed + @", 
        heading = " + track + @", 
        HeadingHistory = '" + SB.ToString() + @"', 
        IsCalculated = 0, 
        CreatedDate = GETDATE(),
        OMDB = abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMDB.lat + ", " + lon + ", " + Airport.OMDB.lng + @")),
        OMDW = abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMDW.lat + ", " + lon + ", " + Airport.OMDW.lng + @")),
        OMSJ = abs([dbo].[fnCalcDistanceKM](" + lat + ", " + Airport.OMSJ.lat + ", " + lon + ", " + Airport.OMSJ.lng + @"))
      WHERE
        ID =" + ID;
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        cmd.ExecuteNonQuery();
      }
    }


    private void AddToHistory(SqlConnection CN) {
      var SQL = @"
        INSERT INTO [dbo].[AdsbHistory] (
          [FlightId]      ,
          [Heading]       ,
          [TailNumber]    ,
          [FlightSource]  ,
          [CallSign]      ,
          [Lon]           ,
          [Lat]           ,
          [Speed]         ,
          [Altitude]      ,
          [CreatedDate]   ,
          [AdsbDate]      ,
          [HexCode]       ,
          [HeadingHistory] ,
          [IsCalculated]
        )  
        SELECT
          [FlightId]      ,
          [Heading]       ,
          [TailNumber]    ,
          [FlightSource]  ,
          [CallSign]      ,
          [Lon]           ,
          [Lat]           ,
          [Speed]         ,
          [Altitude]      ,
          [CreatedDate]   ,
          [AdsbDate]      ,
          [HexCode]       ,
          [HeadingHistory] ,
          [IsCalculated]
        FROM
          ADSBLive
        WHERE
          hexcode='" + hex + "' or flightid='" + flight + "'";
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        cmd.ExecuteNonQuery();
      }
    }

  }


}

