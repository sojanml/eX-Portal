using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DroneCheckListController : Controller {
    // GET: DroneCheckList
    [HttpGet]
    public ActionResult Index(int CheckListID = 1) {
      DroneCheckList CheckList = new DroneCheckList(CheckListID);
      return View(CheckList);
    }//Index
    
    [HttpPost]
    [ActionName("Index")]
    public ActionResult IndexPost(int CheckListID = 0 ) {
      //Process to save Checklist files
      DroneCheckList CheckList = new DroneCheckList(CheckListID);
      CheckList.saveCheckList();
      return View();
    }//Index

  }//class
}//namespace