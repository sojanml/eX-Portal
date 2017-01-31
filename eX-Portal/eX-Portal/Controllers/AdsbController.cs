using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class AdsbController : Controller {
    // GET: Adsb
    public JsonResult Index(Exponent.ADSB.ADSBQuery QueryData) {
      String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.FlightStat(DSN, false, QueryData);

      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Summary(int LastProcessedID = 0) {
      String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.GetSummary(DSN, LastProcessedID, 20);
      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Distance(Exponent.ADSB.ADSBQuery QueryData) {
      String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.GetFlightStatus(DSN, QueryData);
      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Dashboard() {
      /*
      Session["UID"] = 77;
      Session["FullName"] = "Sojan";
      Session["UserId"] = 77;
      */

      return View();
    }
  }


}