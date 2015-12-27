using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class DesignController : Controller {
    // GET: Design


    public DesignController() {
      ViewBag.Menu = eX_Portal.exLogic.User.BuildMenu();
    }


    [OutputCache(Duration = 3600, VaryByCustom = "User")]
    public ActionResult SystemMenu() {
      var MenuList = eX_Portal.exLogic.User.BuildMenu();
      return View(MenuList);
    }

    /*[OutputCache(Duration = 3600, VaryByCustom = "User")]*/
    public ActionResult CustomCss() {
      Response.ContentType = "text/css";
      return PartialView();
    }//CustomCss()


  }
}