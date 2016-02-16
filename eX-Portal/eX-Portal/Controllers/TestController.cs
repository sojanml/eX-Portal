using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class TestController : Controller {
    // GET: Test
    public ActionResult Index(int id = 9) {
      GeoGrid Info = new GeoGrid(id);
      return View(Info);
    }

    public ActionResult Video() {
      return View();
    }
  }

}