using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DroneFlightController : Controller {
    // GET: DroneFlight
    public ActionResult Index([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Drone Flights";
      ViewBag.DroneID = DroneID;

      String SQL =
      "SELECT\n" +
      "  DroneFlight.ID,\n" +
      "   MSTR_Drone.DroneName,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate,\n" +
      "   Count(*) Over() as _TotalRecords,\n" +
      "   DroneFlight.ID as _PKey\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy";

      if (DroneID > 0) {
        SQL = SQL + "\n" +
        "WHERE\n" +
        "  DroneFlight.DroneID=" + DroneID;

        ViewBag.Title += " [" + Util.getDroneName(DroneID) + "]";
      }

      qView nView = new qView(SQL);
      if (exLogic.User.hasAccess("FLIGHT.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.VIEW")) nView.addMenu("Detail", Url.Action("Detail", new { ID = "_PKey" }));
      nView.addMenu("Live Map", Url.Action("FlightData", "Map", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Index()

    public ActionResult Create([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.CREATE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Create Drone Flight";
      DroneFlight InitialData = new DroneFlight();
      InitialData.DroneID = DroneID;
      return View(InitialData);
    }

    [HttpPost]
    public ActionResult Create(DroneFlight theFlight) {
      if (!exLogic.User.hasAccess("FLIGHT.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (theFlight.DroneID < 1 || theFlight.DroneID == null) ModelState.AddModelError("DroneID", "You must select a Drone.");
      if (theFlight.PilotID < 1 || theFlight.PilotID == null) ModelState.AddModelError("PilotID", "Pilot is required for a Flight.");
      if (theFlight.GSCID < 1 || theFlight.GSCID == null) ModelState.AddModelError("GSCID", "A Ground Station Controller should be slected.");

      if (ModelState.IsValid) {
        int ID = 0;
        ExponentPortalEntities db = new ExponentPortalEntities();
        db.DroneFlights.Add(theFlight);
        db.SaveChanges();
        ID = theFlight.ID;
        db.Dispose();
        return RedirectToAction("Detail", new { ID = ID });
      } else {
        ViewBag.Title = "Create Drone Flight";
        return View(theFlight);
      }

    }

    public ActionResult Edit([Bind(Prefix = "ID")] int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.EDIT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Edit Drone Flight";
      ExponentPortalEntities db = new ExponentPortalEntities();   
      DroneFlight InitialData = db.DroneFlights.Find(FlightID); 
      return View(InitialData);
    }

    [HttpPost]
    public ActionResult Edit(DroneFlight InitialData) {
      if (!exLogic.User.hasAccess("FLIGHT.EDIT")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "Edit Drone Flight";
      ExponentPortalEntities db = new ExponentPortalEntities();
      db.Entry(InitialData).State = EntityState.Modified;
      db.SaveChanges();
      return RedirectToAction("Detail", new { ID = InitialData.ID });
    }


    // GET: Drone/Delete/5
    public String Delete([Bind(Prefix = "ID")]int FlightID = 0) {
      String SQL = "";
      Response.ContentType = "text/json";
      if (!exLogic.User.hasAccess("FLIGHT.DELETE"))
        return Util.jsonStat("ERROR", "Access Denied");

      //Delete the drone from database if there is no checklist is created
      SQL = "SELECT Count(*) FROM [DroneCheckList] WHERE FlightID = " + FlightID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a flight after creating checklist");

      SQL = "SELECT Count(*) FROM [FlightMapData] WHERE FlightID = " + FlightID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a flight when contain the GPS data");

      SQL = "DELETE FROM [DroneFlight] WHERE ID = " + FlightID;
      Util.doSQL(SQL);


      return Util.jsonStat("OK");
    }

    public ActionResult Detail(int ID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Drone Flight Details";
      ViewBag.FlightID = ID;

      String SQL =
      "SELECT \n" +
      "  [DroneCheckList].[ID],\n" +
      "  MSTR_DroneCheckList.CheckListTitle,\n" +
      "  MSTR_DroneCheckList.CheckListSubTitle,\n" +
      "  MSTR_User.FirstName as CreatedBy,\n" +
      "  [DroneCheckList].[CreatedOn],\n" +
      "  Count(*) Over() as _TotalRecords, \n" +
      "  [DroneCheckList].[ID] as _PKey\n" +
      "FROM\n" +
      "[DroneCheckList]\n" +
      "LEFT JOIN MSTR_DroneCheckList ON\n" +
      "MSTR_DroneCheckList.ID = [DroneCheckList].DroneCheckListID\n" +
      "LEFT JOIN MSTR_User ON\n" +
      "  MSTR_User.UserID = [DroneCheckList].CreatedBy\n" +
      "WHERE\n" +
      "  [DroneCheckList].[FlightID] = " + ID.ToString();

      qView nView = new qView(SQL);
      nView.addMenu("View", Url.Action("View", "DroneCheckList", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Detail()

    public String DroneFlightDetail(int ID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.VIEW")) return "Access Denied";
      String SQL =
      "SELECT\n" +
      "   DroneFlight.ID,\n" +
      "   MSTR_Drone.DroneName,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy\n" +
      "WHERE\n" +
      "  DroneFlight.ID =" + ID.ToString();

      qDetailView theView = new qDetailView(SQL);
      theView.Columns = 3;

      return theView.getTable();
    }//DroneFlightDetail ()


    public String ByDrone([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT")) return "Access Denied";
      String SQL =
      "SELECT TOP 5" +
      "   MSTR_Drone.DroneName,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy\n" +
      "WHERE\n" +
      "  DroneFlight.DroneID=" + DroneID + "\n" +
      "ORDER BY" +
      "  DroneFlight.ID DESC";

      qView nView = new qView(SQL);
      if(nView.HasRows) { 
        nView.isFilterByTop = false;
        return 
          "<h2>Recent Flights</h2>\n"+
          nView.getDataTable(true, false);
      }

      return "";
      
    }


  }//class
}//namespace