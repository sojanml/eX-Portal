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

    public Double FeetToKiloMeter { get; private set; }  = 0.0003048;
    public Double ATCRadious { get; set; }
    public Double hSafe { get; set; }
    public Double hAlert { get; set; }
    public Double hBreach { get; set; }
    public Double vSafe { get; set; }
    public Double vAlert { get; set; }
    public Double vBreach { get; set; }
    public int tracking_adsb_commercial { get; set; }
    public int tracking_adsb_rpas { get; set; }
    public int tracking_adsb_skycommander { get; set; }
    public int adsb_omdb { get; set; }
    public int adsb_omdw { get; set; }
    public int adsb_omsj { get; set; }
    public int IsQueryChanged { get; set; }
    public Double maxAltitude { get; set; }
    public Double minAltitude { get; set; }
    public Double maxSpeed { get; set; }
    public Double minSpeed { get; set; }
    public int BreachLine { get; set; }
    public int AlertLine { get; set; }


    public ADSBQuery() {
      ATCRadious = 150;
      hSafe = 10;
      vSafe = 1000;
      hAlert = 8;
      vAlert = 1000;
      hBreach = 8;
      vBreach = 500;
      tracking_adsb_commercial = 0;
      tracking_adsb_rpas = 0;
      tracking_adsb_skycommander = 0;
      adsb_omdb = 0;
      adsb_omdw = 0;
      adsb_omsj = 0;
      maxAltitude = 300000;
      minAltitude = 0;
      minSpeed = 0;
      maxSpeed = 1000;
      BreachLine = 1;
      AlertLine = 0;
      IsQueryChanged = 0;
    }

    public bool GetDefaults(SqlConnection CN) {
      String SQL = "SELECT SettingKey, SettingValue FROM ADSBSettings";
      Double Value = 0;
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        using (SqlDataReader RS = cmd.ExecuteReader()) {
          while (RS.Read()) {
            String SettingKey = RS["SettingKey"].ToString();
            Double.TryParse(RS["SettingValue"].ToString(), out Value);
            switch (SettingKey.ToLower()) {
            case "atcradious":
              if (Value > 0)
                this.ATCRadious = Value;
              break;
            case "hsafe":
              if (Value > 0)
                this.hSafe = Value;
              break;
            case "vsafe":
              if (Value > 0)
                this.vSafe = Value;
              break;
            case "halert":
              if (Value > 0)
                this.hAlert = Value;
              break;
            case "valert":
              if (Value > 0)
                this.vAlert = Value;
              break;
            case "hbreach":
              if (Value > 0)
                this.hBreach = Value;
              break;
            case "vbreach":
              if (Value > 0)
                this.vBreach = Value;
              break;
            case "tracking_adsb_commercial":
              if (Value > 0)
                this.tracking_adsb_commercial = (int)Value;
              break;
            case "tracking_adsb_rpas":
              if (Value > 0)
                this.tracking_adsb_rpas = (int)Value;
              break;
            case "tracking_adsb_skycommander":
              if (Value > 0)
                this.tracking_adsb_skycommander = (int)Value;
              break;
            case "adsb_omdb":
              if (Value > 0)
                this.adsb_omdb = (int)Value;
              break;
            case "adsb_omdw":
              if (Value > 0)
                this.adsb_omdw = (int)Value;
              break;
            case "adsb_omsj":
              if (Value > 0)
                this.adsb_omsj = (int)Value;
              break;
            case "minaltitude":
              this.minAltitude = Value;
              break;
            case "maxaltitude":
              this.maxAltitude = Value;
              break;
            case "minspeed":
              this.minSpeed = Value;
              break;
            case "maxspeed":
              this.maxSpeed = Value;
              break;
            case "breach_line":
              this.BreachLine = (int)Value;
              break;
            case "alert_line":
              this.AlertLine = (int)Value;
              break;
            }//switch
          }//while(RS.Read)
        }//using(SqlDataReader RS)
      }//using(SqlCommand cmd)
      return true;
    }

    public bool SetDefaults(SqlConnection CN) {
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={hSafe} WHERE SettingKey='hSafe'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={vSafe} WHERE SettingKey='vSafe'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={hAlert} WHERE SettingKey='hAlert'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={vAlert} WHERE SettingKey='vAlert'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={hBreach} WHERE SettingKey='hBreach'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={vBreach} WHERE SettingKey='vBreach'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={tracking_adsb_commercial} WHERE SettingKey='tracking_adsb_commercial'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={tracking_adsb_rpas} WHERE SettingKey='tracking_adsb_rpas'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={tracking_adsb_skycommander} WHERE SettingKey='tracking_adsb_skycommander'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={adsb_omdb} WHERE SettingKey='adsb_omdb'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={adsb_omdw} WHERE SettingKey='adsb_omdw'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={adsb_omsj} WHERE SettingKey='adsb_omsj'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={ATCRadious} WHERE SettingKey='atcradious'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={minAltitude} WHERE SettingKey='minaltitude'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={maxAltitude} WHERE SettingKey='maxaltitude'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={minSpeed} WHERE SettingKey='minspeed'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={maxSpeed} WHERE SettingKey='maxspeed'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={BreachLine} WHERE SettingKey='breach_line'");
      SetDefaults(CN, $"UPDATE ADSBSettings SET SettingValue ={AlertLine} WHERE SettingKey='alert_line'");

      return true;
    }

    private bool SetDefaults(SqlConnection CN, String SQL) {
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        cmd.ExecuteNonQuery();
      }
      return true;
    }

    public String getTrackingFilter() {
      String TheFilter = String.Empty;
      if ((tracking_adsb_commercial == 1 && tracking_adsb_skycommander == 1 && tracking_adsb_rpas == 1) ||
          (tracking_adsb_commercial == 1 && tracking_adsb_skycommander == 1)) {
        TheFilter = "  AdsbLive.FlightSource IN ('Exponent', 'SkyCommander','FlightAware')";
      } else if (tracking_adsb_commercial == 1 && tracking_adsb_rpas == 1) {
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
    public String getaltitudeFilter() {
      String TheFilter = String.Empty;
      TheFilter = $" AdsbLive.Altitude between {minAltitude*FeetToKiloMeter} AND {maxAltitude * FeetToKiloMeter}";

      return TheFilter;
    }

    public String getspeedFilter() {
      String TheFilter = String.Empty;
      TheFilter = $"AdsbLive.Speed between {minSpeed} AND {maxSpeed}";

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
    public String HexCode { get; set; }
    public List<String> BreachToFlights { get; set; } = new List<String>();
    public List<String> AlertToFlights { get; set; } = new List<String>();

    public List<Double> History { get; set; }

    public FlightPosition() {
      History = new List<Double>();
    }


    public void SetBreachFlights(SqlConnection CN, ADSBQuery QueryData) {
      String SQL = $"Select ToFlightID FROM ADSBDetail WHERE HorizontalDistance <= {QueryData.hBreach} AND VerticalDistance <= {QueryData.vBreach * QueryData.FeetToKiloMeter} AND FromFlightID = '{FlightID}'";
      using(SqlCommand cmd = new SqlCommand(SQL, CN)) {
        using(SqlDataReader RS = cmd.ExecuteReader()) {
          BreachToFlights = new List<String>();
          while (RS.Read()) {
            BreachToFlights.Add(RS["ToFlightID"].ToString());
          }
        }//using(SqlDataReader RS)
      }//using(SqlCommand cmd)
    }//public void SetBreachFlights


    public void SetAlertFlights(SqlConnection CN, ADSBQuery QueryData) {
      String SQL = $"Select ToFlightID FROM ADSBDetail WHERE HorizontalDistance <= {QueryData.hAlert} AND VerticalDistance <= {QueryData.vAlert * QueryData.FeetToKiloMeter} AND FromFlightID = '{FlightID}'";
      if(BreachToFlights.Any()) {
        SQL = SQL  + " AND ToFlightID NOT IN('" + String.Join("','", BreachToFlights) + "')";
      }
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        using (SqlDataReader RS = cmd.ExecuteReader()) {
          AlertToFlights = new List<String>();
          while (RS.Read()) {
            AlertToFlights.Add(RS["ToFlightID"].ToString());
          }
        }//using(SqlDataReader RS)
      }//using(SqlCommand cmd)

    }//public void SetAlertFlights
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

    public int TotalRPAS { get; set; }
    public Double Area { get; set; }
    public int Breach24H { get; set; }

    public void SetSummary(SqlConnection CN) {
      ADSBQuery adsb = new ADSBQuery();
      adsb.GetDefaults(CN);
      String SQL1 = $"select Count(DISTINCT FromFlightID) from ADSBDetailHistory WHERE VerticalDistance <= {adsb.vBreach * adsb.FeetToKiloMeter}  and HorizontalDistance <= {adsb.hBreach}  and CreatedDate >=DATEADD(day, -1, GETDATE());";
      Breach24H = GetDBInt(CN, SQL1);
      TotalRPAS = GetDBInt(CN, "select Count(*) from AdsbLive WHERE FlightSource='SkyCommander'");
      Area = GetArea(CN);
    }

    private int GetDBInt(SqlConnection CN, String SQL) {
      int Result = 0;
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        var oResult = cmd.ExecuteScalar();
        int.TryParse(oResult.ToString(), out Result);
      }
      return Result;
    }

    private Double GetArea(SqlConnection CN) {
      String SQL = "select Min(Lat) as MinLat, Min(Lon) as MinLon, Max(Lat) as MaxLat, Max(Lon) as MaxLon from AdsbLive";
      Double Area = 0;
      Double MinLat = 0, MinLon = 0, MaxLat = 0, MaxLon = 0;
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        using (SqlDataReader RS = cmd.ExecuteReader()) {
          while (RS.Read()) {
            MinLat = RS.IsDBNull(0) ? 0 : (Double)RS.GetDecimal(0);
            MinLon = RS.IsDBNull(1) ? 0 : (Double)RS.GetDecimal(1);
            MaxLat = RS.IsDBNull(2) ? 0 : (Double)RS.GetDecimal(2);
            MaxLon = RS.IsDBNull(3) ? 0 : (Double)RS.GetDecimal(3);
          }
        }
      }

      String AreaSQL = $@"SELECT ROUND((SELECT geography::STGeomFromText(
      'POLYGON((' +
      '{MinLat} {MinLon},' +
      '{MaxLat} {MinLon},' +
      '{MaxLat} {MaxLon},' +
      '{MinLat} {MaxLon},' +
      '{MinLat} {MinLon} ' +
      '))', 4326).STArea() as Area)/1000000,0)";

      using (SqlCommand cmd = new SqlCommand(AreaSQL, CN)) {
        try {
          var oResult = cmd.ExecuteScalar();
          Double.TryParse(oResult.ToString(), out Area);
        } catch {
          //nothing
        }
      }

      return Area;
    }
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
      if (!String.IsNullOrEmpty(hex))
        hex = hex.Trim();
      if (String.IsNullOrEmpty(flight))
        flight = hex;
      if (!String.IsNullOrEmpty(flight))
        flight = flight.Trim();
      bool result = true;
      if (lat == 0 && lon == 0.0)
        return false;
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
        if (HistoryCount >= 5)
          SB.Remove(0, HeadingHistory.IndexOf(',') + 1);
        if (SB.Length > 0)
          SB.Append(',');
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

