using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.Controllers {
  public class HomeController :Controller {

    [OutputCache(Duration = 3600, VaryByCustom = "User")]
    public ActionResult Index() {
      //if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home")
      ViewBag.DashBoard = Util.getDashboard();

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

    public ActionResult Demo(String ID = "") {
      using(ExponentPortalEntities db = new ExponentPortalEntities()) {
        var CMS = db.ContentManagements.Where(e => e.CmsRefName == ID);
        if(CMS.Any()) {
          var CMSItem = CMS.First();
          ViewBag.Title = CMSItem.PageTitle;
          return View(CMSItem);
        }
        return View("Contact");
      }
    }//public ActionResult Demo

  }//public class HomeController
}//namespace eX_Portal.Controllers