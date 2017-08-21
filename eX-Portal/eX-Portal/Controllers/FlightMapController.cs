using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

      if (exLogic.User.hasAccess("DRONE.VIEWALL") || exLogic.User.hasAccess("DRONE.MANAGE")) {
        //no permission check
      } else {
        //Check the drone is in user account
        int DroneID = Util.toInt(ctx.DroneFlights.Where(x => x.ID == FlightID).Select(x => x.DroneID).FirstOrDefault());
        int AccID = Util.getAccountID();
        bool CheckValid = ctx.MSTR_Drone.Where(x => x.DroneId == DroneID && x.AccountID == AccID).Count() > 0 ? true : false;
        if (!CheckValid)
          return RedirectToAction("NoAccess", "Home");
      }
      var TheMap = new FlightMap();

      TheMap.GetInformation(FlightID);
      if (!TheMap.IsLive) {
        if (!exLogic.User.hasAccess("FLIGHT.ARCHIVE"))
          return RedirectToAction("NoAccess", "Home");
      }
      return View(TheMap);

    }

    [HttpGet]
    public async Task<ActionResult> Export([Bind(Prefix ="ID")]int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) {
        return HttpNotFound("You do not have acces to this section");
      }

      using (var tempFiles = new System.CodeDom.Compiler.TempFileCollection()) {
        string ExportFileName = tempFiles.AddExtension("csv");
        // do something with the file here 
        using (System.IO.StreamWriter stream = new System.IO.StreamWriter(ExportFileName)) {
          var query = from d in ctx.FlightMapDatas
                      where d.FlightID == FlightID
                      orderby d.ReadTime
                      select new {
                        ReadTime = (DateTime)d.ReadTime,
                        Lat = d.Latitude,
                        Lng = d.Longitude,
                        Altitude = d.Altitude,
                        Speed = d.Speed,
                        Satellite = d.Satellites,
                        Pitch = d.Pitch,
                        Roll = d.Roll,
                        Heading = d.Heading,
                        Voltage = d.voltage,
                        Distance = d.Distance
                      };
          if(query.Any()) {
           await stream.WriteLineAsync("DateTime,Lat,Lng,Altitude,Speed,Satellite,Pitch,Roll,Heading,Voltage,Distance");
            foreach (var row in query.ToList()) {
              await stream.WriteLineAsync($"{row.ReadTime.ToString("yyyy-MM-dd HH:mm:ss")},{row.Lat},{row.Lng},{row.Altitude},{row.Speed},{row.Satellite},{row.Pitch},{row.Roll},{row.Heading},{row.Voltage},{row.Distance}");
            }
          }//if(query.Any())
        }//using (System.IO.StreamWriter stream)

        return File(System.IO.File.OpenRead(ExportFileName), "text/csv", $"Flight-{FlightID}.csv");
      }//using (var tempFiles)
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
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) {
        var oResult = new {
          Status = "Error",
          Message = "Do not have access"
        };
        return Json(oResult, JsonRequestBehavior.AllowGet);
      }

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
    public JsonResult ADSBHistory(DateTime History) {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) {
        var oResult = new {
          Status = "Error",
          Message = "Do not have access"
        };
        return Json(oResult, JsonRequestBehavior.AllowGet);
      }

      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.ADSBHistory(DSN, History);
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


    public ActionResult Upload() {
      return View();
    }

    public ActionResult Notify([Bind(Prefix = "ID")] String RequestAction, NotifyParser TheParser) {
      String RefFile = Server.MapPath("/Upload/Notify.log");
      using (System.IO.StreamWriter sw = System.IO.File.AppendText(RefFile)) {
        sw.WriteLine($"Date: {DateTime.Now.ToLongDateString()}  {DateTime.Now.ToLongTimeString()}");
        sw.WriteLine(Request.Form);
        sw.WriteLine(Request.QueryString);
        sw.WriteLine("");
      }

      switch (TheParser.call) {
      case "publish":
                    if (TheParser.name.Substring(0, 5) == "DMAT0")
                    {
                        string DID = TheParser.name.Substring(5);
                        TheParser.DroneID = Util.toInt(DID);
                        int flightID = getFlightID(TheParser.DroneID);
                        MSTR_Drone dr = ctx.MSTR_Drone.Where(x => x.DroneId == TheParser.DroneID).Select(x => x).FirstOrDefault();
                        if (dr != null)
                        {
                            dr.LastFlightID = flightID;
                            dr.MakeID = 0;
                            dr.RefName = dr.DroneName;
                            dr.RpasSerialNo = dr.DroneName;
                            dr.ModelID = 0;
                        }
                        ctx.SaveChanges();
                    }
                    else
                    {

                        int uservalid = exLogic.User.StreamKeyValidation(TheParser.key);
                        string DID = TheParser.name.Substring(5);
                        TheParser.DroneID = Util.toInt(DID);
                        if (uservalid > 0)
                        {
                            int userID = exLogic.User.GetKeyUserId(TheParser.key);
                            if (exLogic.User.hasAccessUser("STREAM.VIDEO", userID))
                            {
                                try
                                {
                                    int flightID = getFlightID(TheParser.DroneID);
                                    MSTR_Drone dr = ctx.MSTR_Drone.Where(x => x.DroneId == TheParser.DroneID).Select(x => x).FirstOrDefault();
                                    if (dr != null)
                                    {
                                        dr.LastFlightID = flightID;
                                        dr.MakeID = 0;
                                        dr.RefName = dr.DroneName;
                                        dr.RpasSerialNo = dr.DroneName;
                                        dr.ModelID = 0;
                                    }
                                    ctx.SaveChanges();
                                }
                                catch (Exception Ex)
                                {
                                    return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
                                }
                                return new HttpStatusCodeResult(HttpStatusCode.OK);
                            }
                            else
                                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                        }
                        else
                            return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                    }
                    break;
      case "record_done":
        TheParser.RequestAction = RequestAction;
        TheParser.Assign(ctx);
        break;
     
       }


      //NotifyParser TheParser = (NotifyParser)Info;


      return Json(TheParser, JsonRequestBehavior.AllowGet);
    }

        public int getFlightID( int DroneID)
        {
            int FlightID = 0;
            String SQL;
            String TimeOutDate = System.DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
            String ReadTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

          

            // Check 2 - If no flight is available with BBFlightID
            // then find the last flight created before 120 mins
            if (FlightID <= 0)
            {
                SQL = "SELECT TOP 1 [ID]\n" +
                  "FROM\n" +
                  "  [DroneFlight]\n" +
                  "WHERE\n" +
                  "  DroneId = " + DroneID + " and\n" +
                  "  FlightDate BETWEEN '" + TimeOutDate + "' AND '" + ReadTime + "' \n" +
                  "ORDER BY CreatedOn DESC";
               
                FlightID =Util.getDBInt( SQL);

                
                
            } 

            //Check 3 - No flight is created with in 120 Mins before receive
            //          Black Box data, then create a new flight with black box data
            if (FlightID <= 0)
            {
               FlightID = CreateFlight( DroneID);
                FlightID = FlightID;
            }//if (FlightID <=  0)

            return FlightID;
        }
        public int CreateFlight(int DroneID)
        {
            int FlightID = 0;
            int Pilotid = 0, GSCID = 0;

            String Dt = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var checkdefault = ctx.MSTR_Drone_Setup.Where(x => x.DroneId == DroneID).Select(x=>x).FirstOrDefault();
            
            Pilotid = checkdefault.PilotUserId==null?0:Util.toInt(checkdefault.PilotUserId);
            GSCID = checkdefault.GroundStaffUserId == null ? 0 : Util.toInt(checkdefault.GroundStaffUserId);

            DroneFlight df = new DroneFlight();
            df.DroneID = DroneID;
            df.PilotID = Pilotid;
            df.CreatedOn = System.DateTime.UtcNow;
            df.FlightDate = System.DateTime.UtcNow;
            ctx.DroneFlights.Add(df);
            ctx.SaveChanges();
            DroneFlight dft = ctx.DroneFlights.Where(x => x.CreatedOn == df.CreatedOn && x.DroneID==df.DroneID).FirstOrDefault();
            if(df !=null)
            FlightID = df.ID;

            return FlightID;
        }//using (SqlTransaction sqlTrans)




    }//CreateFlight()
    
}
