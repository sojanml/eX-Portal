using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaxMind.Db;

namespace eX_Portal.Controllers {
  public class HomeController : Controller {
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("Index", "User");
      return View();
    }

    public ActionResult About() {
      ViewBag.Message = "Your application description page.";

      // This creates the DatabaseReader object, which should be reused across
      // lookups.
      String Path = @"C:\Web\eX-Portal\eX-Portal\eX-Portal\eX-Portal\Upload\MaxIP\GeoLite2-City.mmdb";
      using (var reader = new MaxMind.Db.Reader(Path)) {
        // Replace "City" with the appropriate method for your database, e.g.,
        // "Country".
        Newtonsoft.Json.Linq.JToken city = reader.Find("128.101.101.101");
        return View(city);

      }
      
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