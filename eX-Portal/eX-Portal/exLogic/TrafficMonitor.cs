using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace eX_Portal.Models {
  public partial class TrafficMonitor {
    public TrafficMonitor Copy() {
      TrafficMonitor Result = new TrafficMonitor();
      Result.UD = this.UD;
      Result.UL = this.UL;
      Result.UR = this.UR;
      Result.UU = this.UU;
      Result.RD = this.RD;
      Result.RL = this.RL;
      Result.RR = this.RR;
      Result.RU = this.RU;
      Result.DD = this.DD;
      Result.DL = this.DL;
      Result.DR = this.DR;
      Result.DU = this.DU;
      Result.LD = this.LD;
      Result.LL = this.LL;
      Result.LR = this.LR;
      Result.LU = this.LU;

      return Result;
    }
    // Overload - operator to add two TrafficMonitor objects.
    public static TrafficMonitor operator- (TrafficMonitor FromNode, TrafficMonitor ToNode) {
      TrafficMonitor Result = new TrafficMonitor();
      Result.UD = FromNode.UD - ToNode.UD;
      Result.UL = FromNode.UL - ToNode.UL;
      Result.UR = FromNode.UR - ToNode.UR;
      Result.UU = FromNode.UU - ToNode.UU;
                              
      Result.RD = FromNode.RD - ToNode.RD;
      Result.RL = FromNode.RL - ToNode.RL;
      Result.RR = FromNode.RR - ToNode.RR;
      Result.RU = FromNode.RU - ToNode.RU;
                                      
      Result.DD = FromNode.DD - ToNode.DD;
      Result.DL = FromNode.DL - ToNode.DL;
      Result.DR = FromNode.DR - ToNode.DR;
      Result.DU = FromNode.DU - ToNode.DU;
                                       
      Result.LD = FromNode.LD - ToNode.LD;
      Result.LL = FromNode.LL - ToNode.LL;
      Result.LR = FromNode.LR - ToNode.LR;
      Result.LU = FromNode.LU - ToNode.LU;

      return Result;
    }
  }
}

namespace eX_Portal.exLogic {
  public class TrafficMonitorVideo {
    public String VideoURL { get; set; }
    public String Title { get; set; }
    public Double StartTime { get; set; }
  }

  public class LatLng {
    public Decimal Lat { get; set; }
    public Decimal Lng { get; set; }

    public LatLng(Decimal tLat, Decimal tLng) {
      Lat = tLat;
      Lng = tLng;
    }
    public LatLng() {

    }

    public String GetLocation(bool IsWithSymbol = true) {
      String Symbol = IsWithSymbol ? " °" : "";
      return Math.Abs(Lat) + Symbol +
        (Lat > 0 ? "E" : "W") + " " +
        Math.Abs(Lng) + Symbol +
        (Lng > 0 ? "N" : "S");
    }
  }


  public class TrafficMamager {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    public int DroneID { get; set; }
    public int FlightID { get; set; }
    public LatLng Location { get; private set; }
    //constructor
    public TrafficMamager(int DroneID = 0, int FlightID = 0) {
      this.DroneID = DroneID;
      this.FlightID = FlightID;
      if (FlightID > 0) {
        SetFlightLocation();
      } else if (DroneID > 0){
        SetDroneLocation();
      }
    }

    private void SetFlightLocation() {
      var MyLocation = from d in db.FlightMapDatas
                       where d.FlightID == FlightID
                       orderby d.CreatedTime descending
                       select d;
      var MapData = MyLocation.FirstOrDefault();
      if(MapData != null) {
        Location = new LatLng((Decimal)MapData.Latitude, (Decimal)MapData.Longitude);
      }
    }

    private void SetDroneLocation() {
      var MyLocation = from d in db.MSTR_Drone
                       where d.DroneId == DroneID
                       select d;
      var TheDrone = MyLocation.FirstOrDefault();
      if (TheDrone != null) {
        Location = new LatLng((Decimal)TheDrone.Latitude, (Decimal)TheDrone.Longitude);
      }
    }

    public String GetLocation() {
      return Location == null ? "N/A" : Location.GetLocation();
    }

    public StringBuilder Export(int Interval = 0) {
      StringBuilder SB = new StringBuilder();
      List<TrafficMonitor> ExportData = new List<TrafficMonitor>();
      int RowCount = 0;
      DateTime FlightDate = DateTime.MinValue;
      //find the time of flight starting
      int LastFlightID = FlightID;
      if(DroneID > 0) {
        var Query = from d in db.MSTR_Drone
                    where d.DroneId == DroneID
                    select d.LastFlightID;
        var FoundFlightID = Query.FirstOrDefault();
        if(FoundFlightID != null && FoundFlightID != 0) {
          LastFlightID = (int)FoundFlightID;
        }        
      }

      var FlightDateQuery = from d in db.FlightMapDatas
                            where d.FlightID == LastFlightID
                            orderby d.CreatedTime descending
                            select d.CreatedTime;
      var FoundFlightDate = FlightDateQuery.FirstOrDefault();
      if (FoundFlightDate != null) FlightDate = (DateTime)FoundFlightDate;

      while (true) {
        RowCount++;
        Decimal FrameTime = RowCount * Interval * 60 * 1000;
        TrafficMonitor ThisData =
          (from d in db.TrafficMonitor
           where d.FlightID == FlightID && d.FrameTime >= FrameTime
           select d).FirstOrDefault();
        if (ThisData == null)
          break;
        ExportData.Add(ThisData);
      }

      //get the last data
      TrafficMonitor LastData =
        (from d in db.TrafficMonitor
         where d.FlightID == FlightID 
         orderby d.FrameTime descending
         select d).FirstOrDefault();
      if (LastData != null)
      ExportData.Add(LastData);

      SB.AppendLine("Time,Location," +
        "Leg1Straight,Leg1UTurn,Leg1Left,Leg1Right," +
        "Leg2Straight,Leg2UTurn,Leg2Left,Leg2Right," +
        "Leg3Straight,Leg3UTurn,Leg3Left,Leg3Right," +
        "Leg4Straight,Leg4UTurn,Leg4Left,Leg4Right" );

      TrafficMonitor LastExportRow = null;
      foreach (var Data in ExportData) {
        var ProcessedRow = Data;
        if(LastExportRow != null) ProcessedRow = Data - LastExportRow;
        DateTime ExportDate = FlightDate.AddMilliseconds((Double)Data.FrameTime);

        SB.AppendLine(ExportDate.ToString(@"yyyy-MM-dd hh\:mm\:ss") + "," +
          (Location == null ? "N/A" : Location.GetLocation(false)) + "," +
          $"{ProcessedRow.DD},{ProcessedRow.DU},{ProcessedRow.DR},{ProcessedRow.DL}," +
          $"{ProcessedRow.LL},{ProcessedRow.LR},{ProcessedRow.LD},{ProcessedRow.LU}," +
          $"{ProcessedRow.UU},{ProcessedRow.UD},{ProcessedRow.UL},{ProcessedRow.UR}," +
          $"{ProcessedRow.RR},{ProcessedRow.RL},{ProcessedRow.RU},{ProcessedRow.RD}" 
        );
        LastExportRow = Data.Copy();
      }

      return SB;
    }

    public String GetTrafficVideos() {
      List<TrafficMonitorVideo> TheVideos = new List<TrafficMonitorVideo>();
      if(DroneID > 0) {
        TheVideos = GetLiveVideos();
      } else if(FlightID > 0) {
        TheVideos = GetSavedVideos();
      }     
      return Newtonsoft.Json.JsonConvert.SerializeObject(TheVideos);
    }

    public List<TrafficMonitorVideo> GetLiveVideos() {
      List<TrafficMonitorVideo> TheVideos = new List<TrafficMonitorVideo>();
      
      TheVideos.Add( new TrafficMonitorVideo {
        VideoURL = $"rtmp://52.34.136.76/live/drone{DroneID}",
        StartTime = 0,
        Title = "Live"
      });

      return TheVideos;
    }

    //get Traffic videos
    public List<TrafficMonitorVideo> GetSavedVideos() {
      DateTime FirstVideoDate = DateTime.MinValue;
      Double StartVideoTime = 0;
      List<TrafficMonitorVideo> VideoInfo = new List<TrafficMonitorVideo>();

      var PlayList =
        from Videos in db.DroneFlightVideos
        where Videos.FlightID == FlightID && Videos.IsDeleted == 0
        orderby Videos.VideoDateTime
        select new {
          Videos.VideoURL,
          Videos.VideoDateTime
        };
      foreach (var VideoItem in PlayList.ToList()) {
        DateTime VideoDateTime = (DateTime)VideoItem.VideoDateTime;
        if (FirstVideoDate == DateTime.MinValue) {
          FirstVideoDate = VideoDateTime;
        } else {
          StartVideoTime += Math.Abs((VideoDateTime - FirstVideoDate).TotalSeconds);
          FirstVideoDate = VideoDateTime;
        }

        TimeSpan t = TimeSpan.FromSeconds(StartVideoTime);
        string VideoTitle = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
        TrafficMonitorVideo thisVideo = new TrafficMonitorVideo {
          VideoURL = VideoItem.VideoURL,
          StartTime = StartVideoTime,
          Title = VideoTitle
        };
        VideoInfo.Add(thisVideo);

      }//foreach (var VideoItem in PlayList.ToList())
      
      return VideoInfo;

    }//public List<TrafficMonitorVideo> TrafficVideos()
  }//public class TrafficMonitor
}