using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DroneFlightController : Controller {
    // GET: DroneFlight
    public ActionResult Index() {
      ViewBag.Title = "Drone Flights";

      String SQL =
      "SELECT\n" +
      "  DroneFlight.ID,\n" +
      "   MSTR_Drone.DroneName,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate,\n" +
      "   Count(*) Over() as _TotalRecords\n" +
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

      qView nView = new qView(SQL);
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Index()

    public ActionResult Detail(int ID = 0) {
      ViewBag.Title = "Drone Flight Details";
      ViewBag.FlightID = ID;

      String SQL =
      "SELECT \n" +
      "  [DroneCheckList].[ID],\n" +
      "  MSTR_DroneCheckList.CheckListTitle,\n" +
      "  MSTR_DroneCheckList.CheckListSubTitle,\n" +
      "  MSTR_User.FirstName as CreatedBy,\n" +
      "  [DroneCheckList].[CreatedOn],\n" +
      "  Count(*) Over() as _TotalRecords \n" +
      "FROM\n" +
      "[DroneCheckList]\n" +
      "LEFT JOIN MSTR_DroneCheckList ON\n" +
      "MSTR_DroneCheckList.ID = [DroneCheckList].DroneCheckListID\n" +
      "LEFT JOIN MSTR_User ON\n" +
      "  MSTR_User.UserID = [DroneCheckList].CreatedBy\n" +
      "WHERE\n" +
      "  [DroneCheckList].[FlightID] = " + ID.ToString();

      qView nView = new qView(SQL);
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Detail()

    public String DroneFlightDetail(int ID = 0) {
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



  }//class
}//namespace