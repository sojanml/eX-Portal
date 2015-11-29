using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DroneCheckListController : Controller {
    // GET: DroneCheckList
    [HttpGet]
    public ActionResult Create(int ID = 1, int FlightID = 0) {
      //if (FlightID == 0) Int32.TryParse(Request["FlightID"], out FlightID);
      ViewBag.Title = "Create Checklist";
      DroneCheckListForm CheckList = new DroneCheckListForm(ID);
      CheckList.FlightID = FlightID;
      return View(CheckList);
    }//Index
    
    [HttpPost]
    [ActionName("Create")]
    public ActionResult PostCreate(int ID = 0 ) {
      //Process to save Checklist files
      DroneCheckListForm CheckList = new DroneCheckListForm(ID);
      CheckList.FlightID = Int32.Parse(Request["FlightID"]);
      int DroneCheckListID = CheckList.saveCheckList();
      //return RedirectToAction("Details",new { ID = DroneCheckListID });
      //return RedirectToAction("Detail", "DroneFlight", new { ID = CheckList.FlightID });
      return RedirectToAction("Complete", "DroneCheckList", new { ID = DroneCheckListID });
    }//Index


    public ActionResult Complete([Bind(Prefix = "ID")] int DroneCheckListID = 0) {
      ViewBag.Title = "Confirm Checklist Action - " + DroneCheckListID.ToString();
      DroneCheckListForm CheckList = new DroneCheckListForm(DroneCheckListID);

      //Process to save Checklist files
      return View();
    }//Index


    public ActionResult Details(int ID = 0) {
      ViewBag.FlightID = ID.ToString();
      ViewBag.Title = "View Checklist";
      //Process to save Checklist files
      return View();
    }//Index


    public ActionResult CreateCheckList(int FlightID = 0) {
      ViewBag.FlightID = FlightID.ToString();
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<MSTR_DroneCheckList> TheRows = db.MSTR_DroneCheckList.ToList();
      return View(TheRows);
    }
        

    public ActionResult ValidationList(int DroneID = 0) {
      ViewBag.DroneID = DroneID.ToString();
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<MSTR_DroneCheckList> TheRows = db.MSTR_DroneCheckList.ToList();
      return View(TheRows);
    }


    public ActionResult Validation([Bind(Prefix = "ID")] int CheckListID = 0, int DroneID = 0) {
      //if (FlightID == 0) Int32.TryParse(Request["FlightID"], out FlightID);
      ViewBag.Title = "Create Checklist";
      DroneCheckListForm CheckList = new DroneCheckListForm(CheckListID, DroneID);
      CheckList.DroneID = DroneID;
      return View(CheckList);
    }

    [HttpPost]
    [ActionName("Validation")]
    public ActionResult PostValidation(int CheckListID = 0, int DroneID = 0) {
      //if (FlightID == 0) Int32.TryParse(Request["FlightID"], out FlightID);
      ViewBag.Title = "Create Checklist";
      DroneCheckListForm CheckList = new DroneCheckListForm(CheckListID, DroneID);
      CheckList.saveValidation();
      return View(CheckList);
    }

  }//class
}//namespace