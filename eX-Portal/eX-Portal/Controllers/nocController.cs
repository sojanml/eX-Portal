using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class nocController : Controller {
    // GET: NOC
    public ActionResult Register() {
      //to create gcaapproval
      if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
        return RedirectToAction("NoAccess", "Home");

      var NOC = new Models.MSTR_NOC();
      if(NOC.NOC_Details.Count == 0) NOC.NOC_Details.Add(new Models.NOC_Details() {
        StartDate = DateTime.Now,
        EndDate = DateTime.Now,
        StartTime = new TimeSpan(6,0,0),
        EndTime = new TimeSpan(18, 0, 0),
        MaxAltitude = 40,
        MinAltitude = 0
      });
      return View(NOC);
    }
  }
}