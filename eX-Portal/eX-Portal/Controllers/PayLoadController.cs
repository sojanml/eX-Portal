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

      String SQL = "SELECT  FlightUniqueID,COUNT(*) as RFIDCount,\n" +
      " Convert(nvarchar(10), ReadTime, 110) as ReadDate,\n" +
      " Count(*) Over() as _TotalRecords,\n" +
      " FlightUniqueID as _PKey\n" +
      " FROM [PayLoadMapData]\n" +
      " group by \n" +
      " FlightUniqueID,Convert(nvarchar(10), ReadTime, 110)";

      //",[RFIDCount]\n" +
      //",[CreatedTime]\n" +


      SQL = "SELECT\n" +
          "  PayLoadFlightID, \n" +
          "  FlightUniqueID,\n" +
          "[RFIDCount],\n" +
          "[CreatedTime],\n" +
          "  Count(*) Over() as _TotalRecords,\n" +
          "  FlightUniqueID as _PKey\n" +
          "FROM\n" +
          "  PayLoadFlight";

      qView nView = new qView(SQL);
      nView.addMenu("PayLoad Data", Url.Action("PayLoadDataView", "Map", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)


    }
  }
}