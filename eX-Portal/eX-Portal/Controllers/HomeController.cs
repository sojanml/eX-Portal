using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class HomeController : Controller {
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("Index", "User");
      ViewBag.DashBoard = "Dewa";
      return View();
    }

    public ActionResult About() {
      ViewBag.Message = "Your application description page.";
      return View();
    }

    public ActionResult Contact() {
      ViewBag.Message = "Your contact page.";
      return View();
    }

    public ActionResult NoAccess() {
      ViewBag.Title = "No Access";
      return View();
    }

    public ActionResult Error(Exception ex) {
      ViewBag.Title = "Error in processing";
      ViewBag.Message = "Internal Error while processing.";

      return View(ex);
    }

  }
}