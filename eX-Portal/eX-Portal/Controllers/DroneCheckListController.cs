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


    public ActionResult View([Bind(Prefix = "ID")] int ThisCheckListID = 0) {
      //if (FlightID == 0) Int32.TryParse(Request["FlightID"], out FlightID);
      var Result = Util.getDBRow("SELECT [DroneCheckListID],[FlightID] FROM [DroneCheckList] WHERE [ID]=" + ThisCheckListID);
      if (!(bool)Result["hasRows"]) {
        return RedirectToAction("Error", "Home");
      } else {
        int CheckListID = Int32.Parse(Result["DroneCheckListID"].ToString());
        ViewBag.Title = "View Checklist";
        ViewBag.FlightID = Result["FlightID"].ToString();
        DroneCheckListForm CheckList = new DroneCheckListForm(CheckListID);
        CheckList.getValidationMessages(ThisCheckListID);
        return View(CheckList);
      }
    }//Index

    public ActionResult Complete([Bind(Prefix = "ID")] int ThisCheckListID = 0) {
      var Result = Util.getDBRow("SELECT [DroneCheckListID],[FlightID] FROM [DroneCheckList] WHERE [ID]=" + ThisCheckListID);
      List<ValidationMap> Validated = new List<ValidationMap>();
      if (!(bool)Result["hasRows"]) {
        return RedirectToAction("Error", "Home");
      } else {
        int CheckListID = Int32.Parse(Result["DroneCheckListID"].ToString());
        ViewBag.FlightID = Result["FlightID"].ToString();
        ViewBag.ThisCheckListID = ThisCheckListID;
        DroneCheckListForm CheckList = new DroneCheckListForm(CheckListID);
        CheckList.ThisCheckListID = ThisCheckListID;
        Validated = CheckList.getValidationMessages(ThisCheckListID);
        //Process to save Checklist files
        ViewBag.Title = "Confirm Checklist Action - " + CheckList.CheckListTitle;
      }
      return View(Validated);
    }//Index

    [HttpPost]
    [ActionName("Complete")]
    public ActionResult PostComplete([Bind(Prefix = "ID")] int ThisCheckListID = 0, int FlightID = 0) {
      int IsOverride = Util.getQ("Override") == "1" ? 1 : 0;
      String SQL = "Update DroneCheckList SET \n" +
        "  IsOverride =" + IsOverride + ",\n" +
        " Comments='" + Util.getQ("Comments") + "',\n" +
        " SignedBy='" + Util.getQ("SignedBy") + "'\n" +
        "WHERE\n" +
        "  ID=" + ThisCheckListID;
      Util.doSQL(SQL);

      return RedirectToAction("Detail", "DroneFlight", new { ID = FlightID });
    }

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
      String SQL = "SELECT \n" +
           "  D.[DroneName] + ' - ' +  DroneIdHexa as DroneName\n" +
           "FROM\n" +
           "  [ExponentPortal].[dbo].[MSTR_Drone] D\n" +
           "WHERE\n" +
           "  D.[DroneId]=" + DroneID;
      DroneCheckListForm CheckList = new DroneCheckListForm(CheckListID, DroneID);
      ViewBag.Title = "Checklist set-up - " + CheckList.CheckListTitle + " [" + Util.getDBVal(SQL) + "]";
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
      return RedirectToAction("Detail", "Drone", new { ID = DroneID });
    }

  }//class
}//namespace