using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class AdsbController : Controller {
    // GET: Adsb
    private String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
    public JsonResult Index(Exponent.ADSB.ADSBQuery QueryData) {
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.FlightStat(DSN, false, QueryData);

      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Summary(int LastProcessedID = 0) {
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.GetSummary(DSN, LastProcessedID, 20);
      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Distance(Exponent.ADSB.ADSBQuery QueryData) {
      var ADSB = new Exponent.ADSB.Live();
      var Data = ADSB.GetFlightStatus(DSN, QueryData);
      return Json(Data, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Dashboard() {
      Exponent.ADSB.ADSBQuery Params = new Exponent.ADSB.ADSBQuery();
      using (SqlConnection CN = new SqlConnection(DSN)) {
        CN.Open();
        Params.GetDefaults(CN);
        CN.Close();
      }      

      return View(Params);
    }
  }


}