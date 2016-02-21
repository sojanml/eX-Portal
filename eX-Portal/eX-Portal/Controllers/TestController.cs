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

    public ActionResult CompareGraph(int From = 0, int To = 0) {
      String SQL =
      @"SELECT BasePoint.RFID,
        IsNull(BasePoint.Latitude,0) AS bLat,
        IsNull(BasePoint.Longitude,0) AS bLon,
        IsNull(ComparePoint.Latitude,0) AS cLat,
        IsNull(ComparePoint.Longitude,0) AS cLon
      FROM PayLoadMapData AS BasePoint
      LEFT JOIN PayLoadMapData AS ComparePoint
        ON ComparePoint.RFID = BasePoint.RFID
          AND ComparePoint.FlightUniqueID = (
            SELECT FlightUniqueID
            FROM PayLoadFlight
            WHERE PayLoadFlightID = " + To + @"
            )
      WHERE BasePoint.FlightUniqueID = (
          SELECT FlightUniqueID
          FROM PayLoadFlight
          WHERE PayLoadFlightID = " + From + @"
          )";
      ViewBag.Json = Util.getDBRowsJson(SQL);
      return View();
    }
  }

}