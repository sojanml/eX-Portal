using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class TrafficController : Controller {
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
  }
}