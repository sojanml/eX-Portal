using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.IO;

namespace eX_Portal.Controllers {
   
    public class TrafficController : Controller {

        ExponentPortalEntities db = new ExponentPortalEntities();
        // GET: Traffic
        public ActionResult Index() {
      ViewBag.Title = "Trafic Monitoring";
      return View();
    }

    public ActionResult Detail(int ID = 0) {
      return View();
    }

    public ActionResult RTA() {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      return View();
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetMoreInfo()
    {            
            String SQL = "select convert(varchar(8),convert(time,ProcessTime),108) as ProcessTime,MaxSpeed,MinSpeed,MediumSpeed,NumberOfCar from PayloadTraffic";
            var Row = Util.getDBRows(SQL);                        
            return Json(Row, JsonRequestBehavior.AllowGet);      
    }       
  }
}