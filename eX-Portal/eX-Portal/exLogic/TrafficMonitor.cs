using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

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

      while(true) {
        Decimal FrameTime = RowCount * Interval * 60 * 1000;
        TrafficMonitor ThisData =
          (from d in db.TrafficMonitors
           where d.FlightID == FlightID && d.FrameTime >= FrameTime
           select d).FirstOrDefault();
        if (ThisData == null)
          break;
        ExportData.Add(ThisData);
        RowCount++;
      }

      //get the last data
      TrafficMonitor LastData =
        (from d in db.TrafficMonitors
         where d.FlightID == FlightID 
         orderby d.FrameTime descending
         select d).FirstOrDefault();
      if (LastData != null)
      ExportData.Add(LastData);

      SB.AppendLine("Time,Location," +
        "Zone1Straight,Zone1UTurn,Zone1Left,Zone1Right," +
        "Zone2Straight,Zone2UTurn,Zone2Left,Zone2Right," +
        "Zone3Straight,Zone3UTurn,Zone3Left,Zone3Right," +
        "Zone4Straight,Zone4UTurn,Zone4Left,Zone4Right" );

      foreach (var Data in ExportData) {
        TimeSpan ts = TimeSpan.FromMilliseconds((Double)Data.FrameTime);
        SB.AppendLine(ts.ToString(@"hh\:mm\:ss") + "," +
          (Location == null ? "N/A" : Location.GetLocation(false)) + "," +
          $"{Data.DD},{Data.DU},{Data.DR},{Data.DL}," +
          $"{Data.LL},{Data.LR},{Data.LD},{Data.LU}," +
          $"{Data.UU},{Data.UD},{Data.UL},{Data.UR}," +
          $"{Data.RR},{Data.RL},{Data.RU},{Data.RD}" 
        );
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
        VideoURL = $"rtmp://52.29.242.123/live/drone{DroneID}",
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