using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public class FlightVideo {
    public String VideoName { get; set; }
    public DateTime? VideoDate { get; set; }
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
          e.Altitude >= 1
          );
        if(FlightMapDataID > 0) {
          Query = Query.Where(e => e.FlightMapDataID > FlightMapDataID);
        }
        var Data = Query
          .OrderBy(o => o.ReadTime)
          .Select(e => new {
            Lat = e.Latitude,
            Lng = e.Longitude,
            FlightTime = e.ReadTime,
            Altitude = e.Altitude,
            Speed = e.Speed,
            FlightDuration = e.TotalFlightTime,
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

    private String GetString(System.Data.Common.DbDataReader RS, String FiledName) {
      return RS.IsDBNull(RS.GetOrdinal(FiledName)) ? "" : RS[FiledName].ToString();
    }
    private int GetInt(System.Data.Common.DbDataReader RS, String FiledName) {
      int FieldID = RS.GetOrdinal(FiledName);
      return RS.IsDBNull(FieldID) ? 0 : RS.GetInt32(FieldID);
    }
  }
}