using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class FlightMapController : Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();
    // GET: Adsb
    private String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
    // GET: FlightMap
    public ActionResult Index([Bind(Prefix = "ID")] int DroneID = 0, string FlightType = "") {
      if (!exLogic.User.hasAccess("FLIGHT"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "RPAS Flights";
      ViewBag.DroneID = DroneID;
      String SQLVideo = @"CASE 
      WHEN 
          [MSTR_Drone].LastFlightID = DroneFlight.ID and
          [MSTR_Drone].FlightTime > DATEADD(MINUTE, -1, GETDATE())
        THEN '<span class=green_icon>&#xf04b;</span>'
      WHEN (
          SELECT Count(*)
          FROM DroneFlightVideo
          WHERE DroneFlightVideo.FlightID = DroneFlight.ID
          ) = 0
        THEN ''
      ELSE '<span class=icon>&#xf03d;</span>'
      END AS Video,";
      //tblPilot.FirstName AS CreatedBy,
      String SQLFilter = "";
      String SQL = @"SELECT 
        DroneFlight.ID,
        MSTR_Drone.DroneName AS RPAS,
        tblPilot.FirstName AS PilotName,
        tblGSC.FirstName AS GCSName,
        FlightDate AS 'FlightDate',
        ISNULL(g.ApprovalName,'NO NOC') as ApprovalName,
        " + (exLogic.User.hasAccess("FLIGHT.VIDEOS") ? SQLVideo : "") + @"
        Count(*) OVER () AS _TotalRecords,
        DroneFlight.ID AS _PKey
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
      LEFT JOIN MSTR_User AS tblCreated
        ON tblCreated.UserID = DroneFlight.CreatedBy
      ";
      if (DroneID > 0) {
        if (SQLFilter != "")
          SQLFilter += " AND";
        SQLFilter += "\n" +
        "  DroneFlight.DroneID=" + DroneID;
        ViewBag.Title += " [" + Util.getDroneName(DroneID) + "]";
      }

      if (exLogic.User.hasAccess("DRONE.VIEWALL") || exLogic.User.hasAccess("DRONE.MANAGE")) {
      } else {
        if (SQLFilter != "")
          SQLFilter += " AND";
        SQLFilter += " \n" +
          "  MSTR_Drone.AccountID=" + Util.getAccountID();
      }

      //this is using when click link on the dashboard
      if (FlightType == "LastFlight") {
        if (SQLFilter != "")
          SQLFilter += " AND";
        SQLFilter += " \n" +
          "  DroneFlight.ID=" + Util.GetLastFlightFromDrone(DroneID);
      } else if (FlightType == "CurrentMonthFlight") {
        if (SQLFilter != "")
          SQLFilter += " AND";
        SQLFilter += " \n" +
          "  DroneFlight.FlightDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)";
      }

      //this is using when click link on the dashboard
      if (SQLFilter != "") {
        SQL += "\n WHERE\n" + SQLFilter;
      }
      qView nView = new qView(SQL);

      // if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      if (exLogic.User.hasAccess("FLIGHT.EDIT"))
        nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.MAP")) {
        nView.addMenu("Flight Map", Url.Action("Map", "FlightMap", new { ID = "_PKey" }));
        nView.addMenu("Flight Data", Url.Action("Data", "FlightMap", new { ID = "_PKey" }));
      }
      if (exLogic.User.hasAccess("FLIGHT.VIDEOS"))
        nView.addMenu("Flight Videos", Url.Action("ListVdeos", "DroneFlight", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.REPORT"))
        nView.addMenu("Post Flight Report", Url.Action("PostFlightReport", "Report", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.GEOTAG"))
        nView.addMenu("Geo-Tagging", Url.Action("GeoTag", "DroneFlight", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.EXPORTCSV"))
        nView.addMenu("Export-CSV", Url.Action("ExportExcel", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.DELETE"))
        nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("TRAFFIC.LIVE"))
        nView.addMenu("RTA Live", Url.Action("Live", "Traffic", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Index()

    [HttpGet]
    public ActionResult Map([Bind(Prefix = "ID")] int FlightID = 0, int IsLive = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP"))
        return RedirectToAction("NoAccess", "Home");
      var TheMap = new FlightMap();
      TheMap.GetInformation(FlightID);
      return View(TheMap);
    }

    [HttpGet]
    public JsonResult Data([Bind(Prefix = "ID")] int FlightID = 0, int FlightMapDataID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) {
        var oResult = new {
          Status = "Error",
          Message = "Do not have access"
        };
        return Json(oResult, JsonRequestBehavior.AllowGet);
      }

      var TheMap = new FlightMap();
      return Json(TheMap.MapData(FlightID, FlightMapDataID), JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult ADSBData() {
      Exponent.ADSB.ADSBQuery QueryData = new Exponent.ADSB.ADSBQuery();
      using (SqlConnection CN = new SqlConnection(DSN)) {
        CN.Open();
        QueryData.GetDefaults(CN);
        CN.Close();
      }
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.FlightStat(DSN, false, QueryData);
      //  var Data = "";
      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult ChartSummaryData([Bind(Prefix = "ID")] int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) {
        var oResult = new {
          Status = "Error",
          Message = "Do not have access"
        };
        return Json(oResult, JsonRequestBehavior.AllowGet);
      }

      var TheMap = new FlightMap();
      return Json(TheMap.ChartSummaryData(FlightID), JsonRequestBehavior.AllowGet);
    }

    
    public ActionResult Notify([Bind(Prefix = "ID")] String RequestAction, NotifyParser TheParser) {
      String RefFile = Server.MapPath("/Upload/Notify.log");      
      using (System.IO.StreamWriter sw = System.IO.File.AppendText(RefFile)) {
        sw.WriteLine($"Date: {DateTime.Now.ToLongDateString()}  {DateTime.Now.ToLongTimeString()}");
        sw.WriteLine(Request.Form);
        sw.WriteLine(Request.QueryString);
        sw.WriteLine("");
      }

      TheParser.RequestAction = RequestAction;
      //NotifyParser TheParser = (NotifyParser)Info;
      TheParser.Assign(ctx);

      return Json(TheParser, JsonRequestBehavior.AllowGet);
    }
  }
}
 