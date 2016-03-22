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

namespace eX_Portal.Controllers {
  public class MapController : Controller {
    // GET: Map
    public ActionResult Index() {
      return View();
    }
    public ActionResult FightMap() {
      return View();
    }

    public ActionResult Select() {
      return View();
    }
    public ActionResult FlightData(int id = 0) {
      ViewBag.Title = "Flight Map";
      ViewBag.FlightID = id;
      Drones thisDrone = new Drones();
      ViewBag.AllowedLocation = thisDrone.getAllowedLocation(id);
      ViewBag.DroneID = thisDrone.DroneID;
      ViewBag.VideoURL = thisDrone.getVideoURL(id);
      return View();
    }

    public String CheckAlert(int id = 0) {
      StringBuilder AlertMsg = new StringBuilder();
      int AccountID = Util.getDBInt("SELECT AccountID From MSTR_User WHERE UserID=" + id);
      String SQL = @"SELECT
          PortalAlert.AlertID,
          PortalAlert.AlertMessage,
          PortalAlert.AlertType
        FROM
          PortalAlert
      LEFT JOIN DroneFlight ON
          DroneFlight.ID = PortalAlert.FlightID
      LEFT JOIN MSTR_Drone ON
          MSTR_Drone.DroneID = DroneFlight.DroneID AND
          MSTR_Drone.AccountID = " + AccountID + @"
        LEFT JOIN PortalAlert_User ON
          PortalAlert_User.AlertID = PortalAlert.AlertID and
          PortalAlert_User.UserID =  " + id + @"
        WHERE
          MSTR_Drone.DroneID IS NOT NULL AND
          PortalAlert_User.UserID IS NULL AND
          PortalAlert.CreatedOn > DATEADD(minute, -30, GETDATE())";
      var Rows = Util.getDBRows(SQL);
      foreach(var Row in Rows) {
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
      
      return AlertMsg.ToString();
    }

    public ActionResult FlightChart(int id = 0) {
      ViewBag.FlightID = id;
      return View();
    }

    public ActionResult PayLoad([Bind(Prefix = "ID")] String FlightUniqueID = "") {
      if (!exLogic.User.hasAccess("PAYLOAD.MAP")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Payload Data";



      String SQL =
      "SELECT \n" +
      "  [RFID], \n" +
      "  [RSSI], \n" +
      "  [ReadTime], \n" +
      "  [ReadCount], \n" +
      "  [Latitude], \n" +
      "  [Longitude],\n" +
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

    [System.Web.Mvc.HttpPost]
    public JsonResult setYard(PayLoadYard Yard) {
      ExponentPortalEntities DB = new ExponentPortalEntities();
      if (Request["chkNewSetting"] == "1") {
        DB.PayLoadYards.Add(Yard);
        DB.SaveChanges();
      } else {
        DB.Entry(Yard).State = EntityState.Modified;
        DB.SaveChanges();
      }

      //If created a new Yard, then set the Yard ID
      //of current Payload Flight to new one
      if (Request["chkNewSetting"] == "1") {
        String SQL;
        SQL = "UPDATE PayLoadMapData SET YardID=" + Yard.YardID +
        " WHERE FlightUniqueID='" + Request["FlightUniqueID"] + "'";
        Util.doSQL(SQL);

        SQL = "UPDATE PayLoadFlight SET YardID=" + Yard.YardID +
        " WHERE FlightUniqueID='" + Request["FlightUniqueID"] + "'";
        Util.doSQL(SQL);
      }

      if(Request["chkReProcess"] == "1") {
        String SQL;
        SQL = "UPDATE PayLoadMapData SET \n" + 
        " YardID=" + Yard.YardID + ",\n" +
        " RowNumber= -1,\n" +
        " ColumnNumber= -1,\n" +
        " IsProcessed= 0,\n" +
        " RowFixExecuted= 0,\n" +
        " ColFixExecuted= 0,\n" +
        " AutoFixGap= 0\n" +
        "WHERE\n" +
        "  FlightUniqueID='" + Request["FlightUniqueID"] + "'";
        Util.doSQL(SQL);
      }

      return Json(Yard, JsonRequestBehavior.AllowGet);

    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetDrones() {
      string SQL =
      "SELECT\n" +
      "  [LastLatitude],\n" +
      "  [LastLongitude],\n" +
      "  IsNull([MSTR_Drone].[DroneName], 'Drone ' + Convert(varchar, [LiveDrone].DroneID)) as DroneName,\n" +
      "  [MSTR_Drone].[ModelName] AS Description,\n" +
      "  [MSTR_Drone].[CommissionDate],\n" +
      "  [MSTR_Account].NAME AS OwnerName,\n" +
      "  M.NAME AS Manufacture,\n" +
      "  U.NAME AS UAVType\n" +
      "FROM\n" +
      "  [LiveDrone]\n" +
      "LEFT JOIN [MSTR_Drone]\n" +
      "  ON [MSTR_Drone].DroneID = [LiveDrone].DroneID\n" +
      "LEFT JOIN [MSTR_Account]\n" +
      "  ON [MSTR_Drone].AccountID = [MSTR_Account].AccountID\n" +
      "LEFT JOIN LUP_Drone M\n" +
      "  ON [MSTR_Drone].ManufactureID = M.TypeID\n" +
      "    AND M.Type = 'Manufacturer'\n" +
      "LEFT JOIN LUP_Drone U\n" +
      "  ON [MSTR_Drone].UAVTypeID = U.TypeID\n" +
      "    AND U.Type = 'UAVType'\n" +
      "WHERE\n" +
      "  [MSTR_Drone].AccountID=" + Util.getAccountID();


      var LiveDrones = Util.getDBRows(SQL);
      //  LiveDrones.SQL = ;
      //string JsonData=Json(LiveDrones)
      // return Json(JsonData);
      return Json(LiveDrones, JsonRequestBehavior.AllowGet);
      // return LiveDrones;GetMessage
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetFlightData(int FlightID = 0, int LastFlightDataID = 0, int MaxRecords = 1, int Replay = 0) {

      ViewBag.FlightID = FlightID;
      IList<FlightMapData> DroneDataList = Util.GetDroneData(FlightID, LastFlightDataID, MaxRecords, Replay);
      //  LiveDrones.SQL = ;
      //string JsonData=Json(LiveDrones)
      // return Json(JsonData);
      return Json(DroneDataList, JsonRequestBehavior.AllowGet);
      // return LiveDrones;
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
        labels.Add(FMD.ReadTime.Value.Hour + ":" + FMD.ReadTime.Value.Minute + ":" + FMD.ReadTime.Value.Second);
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

  }
}