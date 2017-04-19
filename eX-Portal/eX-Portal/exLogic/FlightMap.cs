using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public class NotifyInfo {
    public String RequestAction { get; set; }
    public String app { get; set; }
    public String flashver { get; set; }
    public String swfurl { get; set; }
    public String tcurl { get; set; }
    public String pageurl { get; set; }
    public String addr { get; set; }
    public String clientid { get; set; }
    public String call { get; set; }
    public String recorder { get; set; }
    public String name { get; set; }
    public String path { get; set; }
  }

  public class NotifyParser: NotifyInfo {
    public int DroneID { get; set; }
    public DateTime VideoDate { get; set; }
    public int FlightID { get; set; }
    public bool AssignStatus { get; set; } = false;

    public bool Assign(ExponentPortalEntities ctx) {
      //get the DroneID from path
      DroneID = GetDroneID();
      VideoDate = GetVideoDate();
      if(DroneID < 0 || VideoDate <= DateTime.MinValue) {
        return false;
      }
      ctx.Database.Connection.Open();
      FlightID = GetFlightID(ctx);
      if(FlightID > 0) {
        SetFlightID(ctx);
        AssignStatus = true;
        return true;
      }

      return false;
    }


    private int GetFlightID(ExponentPortalEntities ctx) {
      int iFlightID = 0;
      String sVideoDate = VideoDate.ToString("yyyy-MM-dd HH:mm:ss");
      String SQL = String.Empty;

      SQL = $@"Select TOP 1 
        FlightID 
      FROM 
        FlightMapData
      WHERE 
        CreatedTime BETWEEN DATEADD(MINUTE, -10, '{sVideoDate}')  AND DATEADD(MINUTE,10, '{sVideoDate}') AND
        DroneID={DroneID}
      ORDER BY
        ABS(DATEDIFF(SECOND, CreatedTime, '{sVideoDate}')) ASC";
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var oFlightID = cmd.ExecuteScalar();
        if (oFlightID == null)
          return iFlightID;
        int.TryParse(oFlightID.ToString(), out iFlightID);
      }
      return iFlightID;
    }

    private bool SetFlightID(ExponentPortalEntities ctx) {
      String SQL;
      String VideoURL = path.Substring(path.LastIndexOf('/') + 1);
      SQL = $@"DELETE FROM DroneFlightVideo        
      WHERE
        VideoURL='{VideoURL}' AND
        FlightID={FlightID}";
      DoSQL(SQL, ctx);

      SQL = $@"UPDATE DroneFlight SET
        RecordedVideoURL='{VideoURL}'
      WHERE
        ID={FlightID}";
      DoSQL(SQL, ctx);

      SQL = $@"INSERT INTO DroneFlightVideo(
        DroneID, FlightID, VideoURL, CreatedDate, VideoDateTime
      ) VALUES (
        {DroneID}, {FlightID}, '{VideoURL}', GETDATE(), '{VideoDate.ToString("yyyy-MM-dd HH:mm:ss")}'
      )";
      DoSQL(SQL, ctx);

      return true;
    }

    private void DoSQL(String SQL, ExponentPortalEntities ctx) {
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        cmd.ExecuteNonQuery();
      }
    }

    private int GetDroneID() {
      int iDroneID = 0;
      String sDroneID = name.Substring(5);
      int.TryParse(sDroneID, out iDroneID);
      return iDroneID;
    }

    private DateTime GetVideoDate() {
      try { 
        int HyphenAt = path.IndexOf('-');
        if (HyphenAt < 1) return DateTime.MinValue;
        String sDate = path.Substring(HyphenAt+1, path.Length - HyphenAt - 5);
        //2017-04-13T12-00-19
        //0   -1 -2 -3 -4 -5
        String[] sDateParts = sDate.Split(new char[]{'-', 'T' });
        int[] iDateParts = Array.ConvertAll(sDateParts, s => int.Parse(s));
        return new DateTime(iDateParts[0], iDateParts[1], iDateParts[2], iDateParts[3], iDateParts[4], iDateParts[5]);
      } catch(Exception ex) {
        return DateTime.MinValue;
      }
    }

  }


  public class FlightVideo {
    public String VideoName { get; set; }
    public DateTime? VideoDate { get; set; }
  }

  public class ChartData {
    public Double Speed { get; set; } = 0;
    public Double Pich { get; set; } = 0;
    public Double Roll { get; set; } = 0;
    public Double Altitude { get; set; } = 0;
    public Double Satellites { get; set; } = 0;
    public DateTime FlightTime { get; set; } = DateTime.MinValue;
  }

  public class FlightMap {
    public String PilotName { get; private set; }
    public String PilotImage { get; private set; }
    public String GroundStaff { get; private set; }
    public String RPAS { get; private set; }
    public String ApprovalName { get; private set; }
    public String FlightDate { get; private set; }
    public int DroneID { get; private set; }
    public int FlightID { get; private set; }
    public int ApprovalID { get; private set; }
    public int PilotID { get; private set; }
    public List<FlightVideo> Videos { get; set; }
    public bool IsLive { get; private set; }
    public String InnerPolygon { get; private set; }
    public String OuterPolygon { get; private set; }

    public void GetInformation(int FlightID) {
      String SQL = @"SELECT 
        DroneFlight.ID as FlightID,
        MSTR_Drone.DroneID,
        MSTR_Drone.DroneName AS RPAS,
        tblPilot.UserID AS PilotID,
        ISNULL(tblPilot.FirstName,'') + ' ' +  
          ISNULL(tblPilot.MiddleName,'') + ' ' +  
          ISNULL(tblPilot.LastName,'')AS PilotName,
        ISNULL(tblPilot.PhotoURL,'') as PilotImage,
        tblGSC.FirstName AS GroundStaff,
        CONVERT(VARCHAR, FlightDate,120) AS 'FlightDate',
        ISNULL(g.ApprovalName,'NO NOC') as ApprovalName,
        ISNULL(g.ApprovalID,0) as ApprovalID,
        g.Coordinates as InnerPolygon,
        g.InnerBoundaryCoord as OuterPolygon        
      FROM 
        DroneFlight
      LEFT JOIN GCA_Approval as g
        ON g.ApprovalID = DroneFlight.ApprovalID
      LEFT JOIN MSTR_Drone
        ON MSTR_Drone.DroneId = DroneFlight.DroneID
      LEFT JOIN MSTR_User AS tblPilot
        ON tblPilot.UserID = DroneFlight.PilotID
      LEFT JOIN MSTR_User AS tblGSC
        ON tblGSC.UserID = DroneFlight.GSCID
      WHERE
       DroneFlight.ID=" + FlightID;
      
      using(var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        using (var cmd = ctx.Database.Connection.CreateCommand()) {          
          cmd.CommandText = SQL;
          var RS = cmd.ExecuteReader();
          while(RS.Read()) {
            PilotName = GetString(RS, "PilotName");
            PilotImage = GetString(RS, "PilotImage");
            GroundStaff = GetString(RS, "GroundStaff");
            RPAS = GetString(RS, "RPAS");
            FlightDate = GetString(RS, "FlightDate");
            ApprovalName =  GetString(RS, "ApprovalName");
            DroneID = GetInt(RS, "DroneID");
            this.FlightID = GetInt(RS, "FlightID");
            ApprovalID = GetInt(RS, "ApprovalID");
            PilotID = GetInt(RS, "PilotID");
            InnerPolygon = GetString(RS, "InnerPolygon");
            OuterPolygon = GetString(RS, "OuterPolygon");
          }
        }//using (var cmd 

        var vQuery = from v in ctx.DroneFlightVideos
                     where v.FlightID == FlightID
                     orderby v.VideoDateTime
                     select new FlightVideo {
                       VideoName = v.VideoURL,
                       VideoDate = v.VideoDateTime
                     };
        this.Videos = vQuery.ToList();
        DateTime CheckDate = DateTime.UtcNow.AddMinutes(-2);

        var LiveQuery = from f in ctx.FlightMapDatas
                        where f.FlightID == FlightID &&
                        f.ReadTime >= CheckDate
                        select f.FlightMapDataID;
        this.IsLive = LiveQuery.Any();

      }//using(var ctx)

      if (String.IsNullOrEmpty(PilotImage)) {
        PilotImage = "/images/PilotImage.png";
      } else {
        PilotImage = $"/Upload/User/{PilotID}/{PilotImage}";
        if(!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(PilotImage)))
          PilotImage = "/images/PilotImage.png";
      }
    }

    public Object MapData(int FlightID, int FlightMapDataID = 0) {
      using(var ctx = new ExponentPortalEntities()) {
        var Query = ctx.FlightMapDatas.Where(e => 
          e.FlightID == FlightID &&
          (e.Altitude > 0 || e.Speed > 0)
          );
        if(FlightMapDataID > 0) {
          Query = Query.Where(e => e.FlightMapDataID > FlightMapDataID);
        }

        //we need to find the last flight time from database for the ID
        var FirstFlightTime = ctx.FlightMapDatas
          .Where(e => e.FlightID == FlightID)
          .OrderBy(e => e.FlightMapDataID)
          .Select(e => e.TotalFlightTime)
          .FirstOrDefault();
        if (FirstFlightTime == null)
          FirstFlightTime = 0;
      
        var Data = Query
          .OrderBy(o => o.ReadTime)
          .Select(e => new {
            Lat = e.Latitude,
            Lng = e.Longitude,
            FlightTime = e.ReadTime,
            Altitude = e.Altitude,
            Speed = e.Speed,
            FlightDuration = e.TotalFlightTime - FirstFlightTime,
            Distance = e.Distance,
            Satellites = e.Satellites,
            Pich = e.Pitch,
            Roll = e.Roll,
            FlightMapDataID = e.FlightMapDataID
          })
          .Take(1000);

        return new {
          Status = "Ok",
          Message = "Return the requested data",
          Data = Data.ToList()
        };
      }//using(var ctx)
    }

    public Object ChartSummaryData(int FlightID) {
      List<ChartData> ChartDatas = new List<ChartData>();
      String SQL = $@"SELECT
         ChunkStart = Min(FlightMapDataID),
         ChunkEnd = Max(FlightMapDataID),
         Speed = Avg(Speed),
         Pitch = Avg(Pitch),
         Roll = Avg(Roll),
         Altitude = Avg(Altitude),
         Satellites =Avg(Satellites),
         FlightTime = Max(ReadTime),
         Chunk
      FROM
         (
            SELECT
               Chunk = NTILE(80) OVER (ORDER BY FlightMapDataID),
               *
            FROM
               FlightMapData
	        WHERE
	          FlightID= {FlightID} AND
		        (Speed > 0 OR Altitude > 0)
         ) AS T
      GROUP BY
         Chunk
      ORDER BY 
         ChunkStart;
      ";
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        using(var cmd = ctx.Database.Connection.CreateCommand()) {
          cmd.CommandText = SQL;
          using(var RS = cmd.ExecuteReader() ) {
            while(RS.Read()) {
              ChartDatas.Add(new ChartData() {
                Speed = (Double)RS.GetDecimal(RS.GetOrdinal("Speed")),
                Pich = (Double)RS.GetDecimal(RS.GetOrdinal("Pitch")),
                Roll = (Double)RS.GetDecimal(RS.GetOrdinal("Roll")),
                Altitude = (Double)RS.GetDecimal(RS.GetOrdinal("Altitude")),
                Satellites = (Double)RS.GetInt32(RS.GetOrdinal("Satellites")),
                FlightTime = RS.GetDateTime(RS.GetOrdinal("FlightTime"))
              });
            }//while(RS.Read())
          }//using(var RS)
        }//using(var cmd)
      }//using (var ctx)
      return ChartDatas;
    }

    private String GetString(System.Data.Common.DbDataReader RS, String FiledName) {
      return RS.IsDBNull(RS.GetOrdinal(FiledName)) ? "" : RS[FiledName].ToString();
    }
    private int GetInt(System.Data.Common.DbDataReader RS, String FiledName) {
      int FieldID = RS.GetOrdinal(FiledName);
      return RS.IsDBNull(FieldID) ? 0 : RS.GetInt32(FieldID);
    }
  }
}