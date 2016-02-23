using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DashBoardController : Controller {
    // GET: DashBoard
    public ActionResult Default() {
      return View();
    }

    public ActionResult Dewa() {
      return View();
    }
  }
}