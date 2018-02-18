using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;
using System.Data;
using System.Data.Entity;
using System.Text;
using eX_Portal.ViewModel;
using System.Configuration;
using System.Data.SqlClient;

namespace eX_Portal.Controllers {
  public class MapController : Controller {
    // GET: Map
    string strConnection = ConfigurationManager.ConnectionStrings["ADSB_DB"].ConnectionString;

    public ActionResult Index() {
      return View();
    }
    public ActionResult FightMap() {
      return View();
    }

    public ActionResult Select() {
      return View();
    }
    public ActionResult FlightData([Bind(Prefix = "ID")] int FlightID = 0, int IsLive = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      ViewBag.FlightID = FlightID;
      Drones thisDrone = new Drones();

      ViewBag.AllowedLocation = thisDrone.getAllowedLocation(FlightID);
      ViewBag.DroneID = thisDrone.DroneID;

      if (thisDrone.isLiveFlight(FlightID) || IsLive == 1) {
        ViewBag.IsLive = true;
        ViewBag.PlayerURL = thisDrone.getLiveURL(FlightID);
        ViewBag.Title = "Flight Map (Live)";
      } else {
        ViewBag.IsLive = false;
        ViewBag.Title = "Flight Map (Replay)";
        ViewBag.VideoStartAt = thisDrone.getVideoStartDate(FlightID);
        ViewBag.PlayerURL = thisDrone.getPlayListURL(FlightID);
      }
      //if (IsLive == 1) IsLive = ViewBag.IsLive = true;
      if (!exLogic.User.hasAccess("FLIGHT.VIDEOS")) {
        ViewBag.PlayerURL = String.Empty;
        ViewBag.VideoStartAt = String.Empty;
      }
      return View(thisDrone);
    }

    [HttpGet]
    public JsonResult GeoTag([Bind(Prefix = "ID")]  int FlightID) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP"))
        return Json(null, JsonRequestBehavior.AllowGet);
      ExponentPortalEntities db = new ExponentPortalEntities();
      var thisDrone = new Drones();
      var DroneName = thisDrone.getDroneNameForFlight(FlightID);
      var Records = (
        from n in db.DroneDocuments
        where n.DocumentType == "Geo Tag" &&
        n.FlightID == FlightID
        select new GeoTagReport {
          ID = n.ID,
          DocumentName = n.DocumentName,
          FlightID = FlightID,
          Altitude = n.Altitude,
          DroneName = DroneName,
          Latitude = n.Latitude,
          Longitude = n.Longitude,
          UpLoadedDate = n.UploadedDate
        }
        ).ToList();

      return Json(Records, JsonRequestBehavior.AllowGet);
    }


    [HttpGet]
    public JsonResult GetAdsbData([Bind(Prefix = "ID")]  int FlightMapID, double Lat, double Lng) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP"))
        return Json(null, JsonRequestBehavior.AllowGet);

      List<AdsbData> AdsbRecords = new List<AdsbData>();
      using (SqlConnection MainDBcon = new SqlConnection(strConnection)) {
        MainDBcon.Open();
        String SQL = "SELECT Count(*) FROM AdsbFlightData Where [FlightMapDataId]=" + FlightMapID;
        int HistoryCount = 0;
        using (var cmd = new SqlCommand(SQL, MainDBcon)) {
          var TheCount = cmd.ExecuteScalar();
          int.TryParse(TheCount.ToString(), out HistoryCount);
        }
        if (HistoryCount > 0) {
          SQL = @"select 
            a.[ID], b.[FlightId], b.[Heading], b.[TailNumber],
            b.[FlightSource],b.[CallSign],
            b.[Lon],b.[Lat],b.[SpeedMph],b.[Altitude],
            b.[CreatedDate],b.[AdsbDate],b.[BoxID]
           from AdsbFlightData a left
           join AdsbHistory b
            on a.HistoryID = b.ID
          where a.[FlightMapDataId]=" + FlightMapID;
        } else {
          String BoxID = String.Empty;

          //finding box id using lat and long
          SQL = @"SELECT [BoxID]      
            FROM [AdsbBoundingBox]
            WHERE  (BottomLat>=" + Lat + " and TopLat<=" + Lat + " )and " +
                  "(LeftLon<=" + Lng + " and RightLon>=" + Lng + ")";
          using (var cmd = new SqlCommand(SQL, MainDBcon)) {
            var TheBoxID = cmd.ExecuteScalar();
            BoxID = TheBoxID == null ? String.Empty : TheBoxID.ToString();
          }
          SQL = @"select
            [FlightId], [Heading], [TailNumber],[FlightSource],[CallSign],
            [Lon],[Lat],[SpeedMph],[Altitude],
            [CreatedDate],[AdsbDate],[BoxID]
          from 
            adsblive 
          where 
            boxid='" + BoxID + "'";
        }

        using (var cmd = new SqlCommand(SQL, MainDBcon)) {
          using (var sdr = cmd.ExecuteReader()) {
            while (sdr.Read()) {
              AdsbData Adsb_Data = new AdsbData();
              Adsb_Data.FlightId = sdr["FlightId"].ToString();
              Adsb_Data.heading = sdr["Heading"].ToString();
              Adsb_Data.ident = sdr["TailNumber"].ToString();
              Adsb_Data.FlightSource = sdr["FlightSource"].ToString();
              Adsb_Data.CallSign = sdr["CallSign"].ToString();
              Adsb_Data.longitude = Util.toDouble(sdr["Lon"].ToString());
              Adsb_Data.latitude = Util.toDouble(sdr["Lat"].ToString());
              Adsb_Data.speed = Util.toDouble(sdr["SpeedMph"].ToString());
              Adsb_Data.altitude = Util.toDouble(sdr["Altitude"].ToString());
              Adsb_Data.CreatedDate = sdr.GetDateTime(sdr.GetOrdinal("CreatedDate"));
              Adsb_Data.AdsbsDate = sdr.GetDateTime(sdr.GetOrdinal("AdsbDate"));
              AdsbRecords.Add(Adsb_Data);
            }//while (sdr.Read())
          }//using (var sdr = cmd.ExecuteReader())
        }//using (var cmd = new SqlCommand(SQL, MainDBcon))
      }//using (SqlConnection MainDBcon = new SqlConnection(strConnection))
      return Json(AdsbRecords, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public String PayloadData([Bind(Prefix = "ID")]  int FlightID) {
      var FlightUniqueID = "20160411095652";
      var theGrid = new GeoGrid(FlightUniqueID);
      Object GridInfo = theGrid.getGridKML();

      Response.ContentType = "text/xml";
      return GridInfo.ToString();
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<ActionResult> DynamicZone() {
      var Path = new List<DynamicZone>();

      using (ExponentPortalEntities db = new ExponentPortalEntities()) {
        DateTime Today = DateTime.UtcNow.Date;
        //Convert the Time specifed to GMP
        //It is in UAE (Dubai) Time zone in table
        TimeSpan nowTime = DateTime.UtcNow.AddHours(+4).TimeOfDay;
        var AllZone =
          from m in db.MSTR_NoFlyZone
          where
            m.DisplayType == "Dynamic" &&
            m.StartDate <= Today &&
            m.EndDate >= Today &&
            m.StartTime <= nowTime &&
            m.EndTime >= nowTime &&
            m.IsDeleted != true
          select m;
        foreach (var Row in await AllZone.ToListAsync()) {
          var Zone = new DynamicZone() {
            ID = Row.ID
          };
          Zone.setPath(Row.Coordinates);
          Path.Add(Zone);
        }
      }
      return Json(Path, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public String NoFlyZone() {
      Response.ContentType = "application/vnd.google-earth.kml+xml";
      NoFlyZoneMap TheMap = new NoFlyZoneMap();
      //if (!exLogic.User.hasAccess("FLIGHT.MAP")) return "";
      using (ExponentPortalEntities db = new ExponentPortalEntities()) { 
        var AllZone =
          from m in db.MSTR_NoFlyZone
          where m.DisplayType=="FIXED"
          select m
          ;
        foreach(var Row in AllZone) {
          var Zone = new FlyZone(Row.FillColour, Row.Coordinates);
          TheMap.NoFlyZone.Add(Zone);
        }
      }
      return TheMap.GetKML().ToString();
    }


    [HttpGet]
    public String NoFlyZone2() {
      Response.ContentType = "application/vnd.google-earth.kml+xml";
      NoFlyZoneMap2 TheMap = new NoFlyZoneMap2();
      //if (!exLogic.User.hasAccess("FLIGHT.MAP")) return "";
      using (ExponentPortalEntities db = new ExponentPortalEntities()) {
        var AllZone =
          from m in db.MSTR_NoFlyZone
          where m.DisplayType == "FIXED"
          select m;
        foreach (var Row in AllZone) {
          var Zone = new FlyZone2(Row.FillColour, Row.Coordinates);
          TheMap.NoFlyZone.Add(Zone);
        }

        DateTime Today = DateTime.UtcNow.Date;
        //Convert the Time specifed to GMP
        //It is in UAE (Dubai) Time zone in table
        TimeSpan nowTime = DateTime.UtcNow.AddHours(+4).TimeOfDay;
        var AllZone2 =
          from m in db.MSTR_NoFlyZone
          where
            m.DisplayType == "Dynamic" &&
            m.StartDate <= Today &&
            m.EndDate >= Today &&
            m.StartTime <= nowTime &&
            m.EndTime >= nowTime &&
            m.IsDeleted != true
          select m;
        foreach (var Row in AllZone2.ToList()) {
          var Zone = new FlyZone2(Row.FillColour, Row.Coordinates);
          TheMap.NoFlyZone.Add(Zone);
        }

      }
      return TheMap.GetKML().ToString();
    }


    [HttpGet]
    public async System.Threading.Tasks.Task<ActionResult> DynamicZoneNotification([Bind(Prefix = "ID")]int FlightID = 0) {
      using (ExponentPortalEntities db = new ExponentPortalEntities()) {
        var Alerts =
          from m in db.PortalAlerts
          where 
            m.FlightID == FlightID &&
            m.AlertCategory == "NoFlyZone"
          select new {
            ID = m.AlertID,
            Description = m.AlertMessage
          };
        return Json(await Alerts.ToListAsync(), JsonRequestBehavior.AllowGet);
      }      
    }

    public String RFID([Bind(Prefix = "ID")]String RFID = "") {
      String SQL = @"SELECT 
        [VIN]
        ,[CarMake]
        ,[Model]
        ,[Year]
        ,[Color]
      FROM 
        [MSTR_Product]
      WHERE
        RFID='" + RFID + "'";
      var Row = Util.getDBRow(SQL);
      if (Row.Count == 1) {
        return "<br>Vechile not accociated";
      }

      return
      "<br>\n" +
      "VIN: <b>" + Row["VIN"].ToString().ToUpper() + "</b><br>\n" +
      "Make: <b>" + Row["CarMake"].ToString() + "</b><br>\n" +
      "Model: <b>" + Row["Model"].ToString() + "</b><br>\n" +
      "Year: <b>" + Row["Year"].ToString() + "</b><br>\n" +
      "Color: <b>" + Row["Color"].ToString() + "</b>";

    }

    public ActionResult PlayList(int id = 0) {
      String SQL = "select VideoURL from DroneFlightVideo WHERE FlightID=" + id;
      var Rows = Util.getDBRows(SQL);
      // Response.ContentType = "text/json";
      return View(Rows);
    }

    public String CheckAlert(int id = 0) {
      StringBuilder AlertMsg = new StringBuilder();
      int AccountID = Util.getDBInt("SELECT AccountID From MSTR_User WHERE UserID=" + id);
      String SQL = @"SELECT TOP 1
          PortalAlert.AlertID,
          PortalAlert.AlertMessage,
          PortalAlert.AlertType
        FROM
          PortalAlert
      LEFT JOIN DroneFlight ON
          DroneFlight.ID = PortalAlert.FlightID
      LEFT JOIN MSTR_Drone ON
          MSTR_Drone.DroneID = DroneFlight.DroneID";
      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) SQL = SQL + @" AND
          MSTR_Drone.AccountID = " + AccountID;
      SQL = SQL + @"
        LEFT JOIN PortalAlert_User ON
          PortalAlert_User.AlertID = PortalAlert.AlertID and
          PortalAlert_User.UserID =  " + id + @"
        WHERE
          MSTR_Drone.DroneID IS NOT NULL AND
          PortalAlert_User.UserID IS NULL AND
          PortalAlert.CreatedOn > DATEADD(minute, -30, GETDATE())
       ORDER BY
         PortalAlert.CreatedOn";
      var Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        String AlertType = Row["AlertType"].ToString();
        if (String.IsNullOrEmpty(AlertType)) AlertType = "High";
        AlertMsg.Append("<LI");
        AlertMsg.Append(" class=\"");
        AlertMsg.Append(AlertType.ToLower());
        AlertMsg.Append("\">");

        AlertMsg.Append(Row["AlertMessage"]);
        AlertMsg.Append("</LI>");

        SQL = "INSERT INTO PortalAlert_User(UserID, AlertID) VALUES( " +
          id + ", " + Row["AlertID"] + ")";
        Util.doSQL(SQL);
      }
      //AlertMsg.Append("<LI class=\"warning\">Testing</Li>");
      return AlertMsg.ToString();
    }

    public ActionResult FlightChart(int id = 0) {
      ViewBag.FlightID = id;
      return View();
    }

    public ActionResult PayLoad([Bind(Prefix = "ID")] String FlightUniqueID = "") {
      if (!exLogic.User.hasAccess("PAYLOAD.MAP")) return RedirectToAction("NoAccess", "Home");
      int ProcessingModel = Util.getDBInt("Select ISNULL(ProcessingModel,1) From PayLoadFlight where FlightUniqueID='" + FlightUniqueID + "'");
      if (ProcessingModel == 1) {
        return RedirectToAction("PayLoadIndoor", new { ID = FlightUniqueID });
      }
      ViewBag.Title = "Payload Data";

      String SQL =
      "SELECT \n" +
      "  [RFID], \n" +
      "  [RSSI], \n" +
      "  [ReadTime], \n" +
      "  [ReadCount], \n" +
      "  CASE WHEN [GridLat] = 0 THEN [Latitude] ELSE [GridLat] END  as [Latitude], \n" +
      "  CASE WHEN [GridLng] = 0 THEN [Longitude] ELSE [GridLng] END  as [Longitude],\n" +
      "  [RowNumber] as [Row], \n" +
      "  [ColumnNumber]  as [Col],\n" +
      "  Count(*) Over() as _TotalRecords,\n" +
      "  Concat([RFID],',',FlightUniqueID) as _PKey\n" +
      "FROM \n" +
      "  [PayLoadMapData] \n" +
      "WHERE\n" +
      "  FlightUniqueID='" + FlightUniqueID + "'";

      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("Detail", "Payload", new { ID = "_Pkey" }));
      ViewBag.FlightUniqueID = FlightUniqueID;

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        //get yard information for FlightUniqueID
        GeoGrid theYard = new GeoGrid(FlightUniqueID);
        ViewBag.Yard = theYard.getYard();

        return View(nView);

      }//if(IsAjaxRequest)

    }


    public ActionResult PayLoadIndoor([Bind(Prefix = "ID")] String FlightUniqueID = "") {
      if (!exLogic.User.hasAccess("PAYLOAD.INDOOR")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Indoor Payload Data";
      String SQL =
      @"SELECT DISTINCT
        [ShelfID],
        ISNULL(MSTR_Product.Product_Name, [ShelfID]) as ShelfName
      FROM 
        [PayLoadMapData]
      LEFT JOIN MSTR_Product ON
        MSTR_Product.RFID = [PayLoadMapData].[ShelfID]
      WHERE
        IsValid = 1 AND
        FlightUniqueID='" + FlightUniqueID + @"'
      ORDER BY
         [PayLoadMapData].[ShelfID]";
      //return View("PayLoadIndoor", new { ID = FlightUniqueID });
      var Rows = Util.getDBRows(SQL);
      ViewBag.FlightUniqueID = FlightUniqueID;
      return View(Rows);
    }

    public ActionResult PayLoadIndoorShelf(String ShelfID, String FlightUniqueID) {
      if (!exLogic.User.hasAccess("PAYLOAD.INDOOR")) return RedirectToAction("NoAccess", "Home");
      String SQL =
      @"SELECT
        [PayLoadMapData].[RFID],
        ISNULL(MSTR_Product.Product_Name, [PayLoadMapData].[RFID]) as ProductName
      FROM 
        [PayLoadMapData]
      LEFT JOIN MSTR_Product ON
        MSTR_Product.RFID = [PayLoadMapData].[RFID]
      WHERE
        [PayLoadMapData].ShelfID='" + ShelfID + @"' AND 
        IsValid = 1 AND
        FlightUniqueID='" + FlightUniqueID + @"'
      ORDER BY
         [PayLoadMapData].readtime, 
        [PayLoadMapData].[RFID]";
      //return View("PayLoadIndoor", new { ID = FlightUniqueID });
      var Rows = Util.getDBRows(SQL);

      return PartialView(Rows);
    }

    public JsonResult getYard([Bind(Prefix = "ID")] int YardID = 0) {
      GeoGrid theYard = new GeoGrid(0);
      return Json(theYard.getYard(YardID), JsonRequestBehavior.AllowGet);
    }

    [System.Web.Mvc.HttpPost]
    public JsonResult setYard(PayLoadYard Yard) {
      ExponentPortalEntities DB = new ExponentPortalEntities();
      String SQL;
      if (Request["chkNewSetting"] == "1") {
        DB.PayLoadYards.Add(Yard);
        DB.SaveChanges();
      } else {
        var entry = DB.Entry(Yard);
        entry.State = EntityState.Modified;
        entry.Property("YardName").IsModified = false;
        DB.SaveChanges();
      }

      //If created a new Yard, then set the Yard ID
      //of current Payload Flight to new one
      //if (Request["chkNewSetting"] == "1") {

      SQL = "UPDATE PayLoadMapData SET YardID=" + Yard.YardID +
      " WHERE FlightUniqueID='" + Request["FlightUniqueID"] + "'";
      Util.doSQL(SQL);

      SQL = "UPDATE PayLoadFlight SET YardID=" + Yard.YardID +
      " WHERE FlightUniqueID='" + Request["FlightUniqueID"] + "'";
      Util.doSQL(SQL);
      //}

      SQL = "DELETE from PayLoadYardGrid where Yardid = " + Yard.YardID;
      Util.doSQL(SQL);

      if (Request["chkReProcess"] == "1") {
        SQL = @"UPDATE PayLoadMapData SET 
          YardID=" + Yard.YardID + @",
          RowNumber= -1,
          ColumnNumber= -1,
          IsProcessed= 0,
          RowFixExecuted= 0,
          ColFixExecuted= 0,
          AutoFixGap= 0,
          GridLat = 0,
          GridLng = 0
        WHERE
          FlightUniqueID='" + Request["FlightUniqueID"] + "'";
        Util.doSQL(SQL);
      }
      return Json(Yard, JsonRequestBehavior.AllowGet);
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetDrones() {
      string SQL =
      @"SELECT
       [Latitude] AS LastLatitude,
       [Longitude] as LastLongitude,
       IsNull([MSTR_Drone].[DroneName], 'Drone ' + Convert(varchar, [MSTR_Drone].DroneID)) as DroneName,
       [MSTR_Drone].[ModelName] AS Description,
       [MSTR_Drone].[CommissionDate],
       [MSTR_Account].NAME AS OwnerName,
       U.name as UAVType,
       M.NAME AS Manufacture,
       FlightTime,
       DroneID,
        LastFlightID,
        DATEDIFF(mi,FlightTime,GetDate()),
       case when DATEDIFF(mi,FlightTime,GetDate())<5 then 1 else 0 end as Live
      from [MSTR_Drone]
          LEFT JOIN [MSTR_Account]
        ON [MSTR_Drone].AccountID = [MSTR_Account].AccountID
      LEFT JOIN LUP_Drone M
        ON [MSTR_Drone].ManufactureID = M.TypeID
          AND M.Type = 'Manufacturer'
      LEFT JOIN LUP_Drone U
        ON [MSTR_Drone].UAVTypeID = U.TypeID
          AND U.Type = 'UAVType'
      WHERE
        Latitude > 0 AND Longitude > 0";

      // FlightTime >= DATEADD(month,-1,GETDATE())";

      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        SQL = SQL + " AND [MSTR_Drone].AccountID =" + Util.getAccountID() + "";
      }

      


      var LiveDrones = Util.getDBRows(SQL);
      //  LiveDrones.SQL = ;
      //string JsonData=Json(LiveDrones)
      // return Json(JsonData);
      return Json(LiveDrones, JsonRequestBehavior.AllowGet);
      // return LiveDrones;GetMessage
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetFlightData(int FlightID = 0, int LastFlightDataID = 0, int MaxRecords = 1, int Replay = 0) {
      //if (!exLogic.User.hasAccess("FLIGHT.MAP")) return "NoAccess", "Home";
      ViewBag.FlightID = FlightID;

      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {

        var MinFlightHour = (from d in ctx.FlightMapDatas
                             where d.FlightID == FlightID
                             orderby d.FlightMapDataID ascending
                             select d.TotalFlightTime).FirstOrDefault();

        if (!MinFlightHour.HasValue) MinFlightHour = 0;

        var Flights = (
            from d in ctx.FlightMapDatas
            where d.FlightID == FlightID &&
                  d.FlightMapDataID > LastFlightDataID
            select d
            );

        var FlightMapDataList = Flights
          .OrderBy(x => x.FlightMapDataID)
          .Take(MaxRecords).ToList();

        //filtering the data
        var FilteredMapData = new List<FlightMapData>();
        var isRecordAdded = false;
        Double LastLat = 0;
        Double LastLng = 0;
        foreach(var MapData in FlightMapDataList) {
          isRecordAdded = false;
          Double Lat = MapData.Latitude == null ? 0 : (Double)MapData.Latitude;
          Double Lng = MapData.Longitude == null ? 0 : (Double)MapData.Longitude;
          Double DiffGeo = (Double)Math.Abs(Lat - LastLat) + (Double)Math.Abs(Lng - LastLng);
          Decimal? TotalHours = MapData.TotalFlightTime;
          if (!TotalHours.HasValue) TotalHours = 0;
          TotalHours = TotalHours - MinFlightHour;

          LastLat = Lat;
          LastLng = Lng;
          MapData.TotalFlightTime = TotalHours;

          //DiffGeo = 1;
          if (DiffGeo > 0.00001) {
            FilteredMapData.Add(MapData);
            isRecordAdded = true;
          }
        }

        if (!isRecordAdded && FlightMapDataList.Count() > 0) FilteredMapData.Add(FlightMapDataList.Last());

        return Json(FilteredMapData, JsonRequestBehavior.AllowGet);
        //return Json(FlightMapDataList, JsonRequestBehavior.AllowGet);
        // return LiveDrones;

      }


    }


    public string Send() {

      string reuestquery = Request.QueryString["Message"];
      string[] data = reuestquery.Split('|');
      try {
        string SQL = "INSERT INTO [DroneData] (\n" +
          "  [CreatedDate], \n" +
          "  [QueueMessage],\n" +              // 1
          "  [DroneId],\n" +                   // 2
          "  [Latitude],\n" +                  // 3
          "  [Longitude],\n" +                 // 4
          "  [Altitude],\n" +                  // 5
          "  [Speed],\n" +                     // 6
          "  [FixQuality],\n" +                // 7
          "  [Satellites],\n" +                // 8
          "  [ReadTime],\n" +                  // 9
          "  [Pitch],\n" +                     // 10
          "  [Roll],\n" +                      // 11
          "  [Heading],\n" +                   // 12
          "  [TotalFlightTime],\n" +           // 13
          "  [BBFlightID]\n" +                 // 14
          ") VALUES (\n" +
          "  GETDATE(),\n" +       // 1
          "  '" + reuestquery + "',\n" +       // 1
          "  '" + data[0] + "',\n" +           // 2
          "  '" + data[1] + "',\n" +           // 3
          "  '" + data[2] + "',\n" +           // 4
          "  '" + data[3] + "',\n" +           // 5
          "  '" + data[4] + "',\n" +           // 6
          "  '" + data[5] + "',\n" +           // 7
          "  '" + data[6] + "',\n" +           // 8
          "  '" + data[7] + "',\n" +           // 9
          "  '" + data[8] + "',\n" +           // 10
          "  '" + data[9] + "',\n" +           // 11
          "  '" + data[10] + "',\n" +          // 12
          "  '" + data[11] + "',\n" +          // 13
          "  '" + data[12] + "'\n" +           // 14
          ")";
        Util.doSQL(SQL);
        return "OK";
      } catch (Exception ex) {
        return "ERROR - " + ex.Message;
      }


    }
    [System.Web.Mvc.HttpGet]
    public JsonResult getLineChartData(int FlightID = 80, int LastFlightDataID = 0, int MaxRecords = 20) {
      List<object> iData = new List<object>();
      List<string> labels = new List<string>();
      List<int> lst_dataItem_2 = new List<int>();
      List<int> lst_dataItem_1 = new List<int>();
      List<int> lst_dataItem_3 = new List<int>();
      List<int> lst_dataItem_4 = new List<int>();
      List<int> lst_dataItem_5 = new List<int>();
      IList<FlightMapData> DroneDataList = Util.GetFlightChartData(FlightID, LastFlightDataID, MaxRecords);
      foreach (FlightMapData FMD in DroneDataList) {
        labels.Add(((DateTime)FMD.ReadTime).ToString("HH:mm:ss"));
        lst_dataItem_1.Add(Convert.ToInt32(FMD.Altitude));
        lst_dataItem_2.Add(Convert.ToInt32(FMD.Satellites));
        lst_dataItem_3.Add(Convert.ToInt32(FMD.Pitch));
        lst_dataItem_4.Add(Convert.ToInt32(FMD.Roll));
        lst_dataItem_5.Add(Convert.ToInt32(FMD.Speed));
      }
      iData.Add(labels);
      iData.Add(lst_dataItem_1);
      iData.Add(lst_dataItem_2);
      iData.Add(lst_dataItem_3);
      iData.Add(lst_dataItem_4);
      iData.Add(lst_dataItem_5);

      return Json(iData, JsonRequestBehavior.AllowGet);

    }

    public ActionResult FlightDataView([Bind(Prefix = "ID")] String FlightID = "") {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      ViewBag.FlightID = FlightID;
      int FID = Util.toInt(FlightID);
      if (FID < 1) return RedirectToAction("Error");
      ViewBag.Title = "Flight Data";
      //            ViewBag.DroneID = DroneID;

      String SQL =
       "SELECT \n" +
       "  FlightMapDataID,\n" +
       "  ReadTime,\n" +
       "  Latitude,\n" +
       "  Longitude,\n" +
       "  Altitude,\n" +
       "  Speed,\n" +
       "  FixQuality,\n" +
       "  Satellites,\n" +
       "  Pitch,\n" +
       "  Roll,\n" +
       "  Heading,\n" +
       "  TotalFlightTime,\n" +
       "  Count(*) OVER() as _TotalRecords\n" +
       "FROM\n" +
       "  FlightMapData\n" +
       "WHERE\n" +
       "  FlightID=" + FID;

      qView nView = new qView(SQL);
      nView.IsFormatDate = false;
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetPayLoadData(string FlightUniqueID = "", int LastFlightDataID = 0, int MaxRecords = 1) {
      ViewBag.FlightUniqueID = FlightUniqueID;
      IList<PayLoadMapData> PayLoadDataList = Util.GetPayLoadData(FlightUniqueID, LastFlightDataID, MaxRecords);
      //  LiveDrones.SQL = ;
      //string JsonData=Json(LiveDrones)
      // return Json(JsonData);
      return Json(PayLoadDataList, JsonRequestBehavior.AllowGet);
      // return LiveDrones;
    }

    public JsonResult FlightInfo([Bind(Prefix = "ID")] int FlightID = 0) {
      DroneFlight df = new DroneFlight();
      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        df = (from d in ctx.DroneFlight
              where d.ID == FlightID
              select d).FirstOrDefault();
      }
      return Json(df, JsonRequestBehavior.AllowGet);
    }
  }
}