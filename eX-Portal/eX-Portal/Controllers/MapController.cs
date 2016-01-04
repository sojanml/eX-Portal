using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;
using System.Data;

namespace eX_Portal.Controllers {
  public class MapController : Controller {
    // GET: Map
    public ActionResult Index() {
      return View();
    }
    public ActionResult FightMap() {
      return View();
    }

    public ActionResult FlightData(int id = 0) {
      ViewBag.FlightID = id;
      return View();
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
    public JsonResult GetFlightData(int FlightID = 0,int LastFlightDataID=0, int MaxRecords = 1) {

            ViewBag.FlightID = FlightID;
      IList<FlightMapData> DroneDataList = Util.GetDroneData(FlightID,LastFlightDataID, MaxRecords);
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
  }
}