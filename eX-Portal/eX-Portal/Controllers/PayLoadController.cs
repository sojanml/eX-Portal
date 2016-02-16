using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class PayLoadController : Controller {
    // GET: PayLoad
    public ActionResult Index() {
      ViewBag.Title = "PayLoad Flights";
      String SQL =
      @"SELECT
        PayLoadFlightID, 
        FlightUniqueID,
        PayLoadYard.YardName,
        [RFIDCount],
        [CreatedTime],
        Count(*) Over() as _TotalRecords,
        FlightUniqueID as _PKey
      FROM
        PayLoadFlight
      LEFT JOIN PayLoadYard ON
        PayLoadYard.YardID = PayLoadFlight.YardID";
      
      qView nView = new qView(SQL);
      nView.addMenu("PayLoad Data", Url.Action("PayLoad", "Map", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)


    }//ActionResult Index()
  }//class PayLoadController
}//namespace eX_Portal.Controllers