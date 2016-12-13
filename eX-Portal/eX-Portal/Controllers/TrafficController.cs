using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.IO;

namespace eX_Portal.Controllers {

  public class TrafficController : Controller {

    ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Traffic
    public ActionResult Index() {
      ViewBag.Title = "Trafic Monitoring";
      return View();
    }

    public ActionResult Detail(int ID = 0) {
      return View();
    }

    public ActionResult RTA() {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      return View();
    }

    public ActionResult Live() {
      return View();
    }

    public JsonResult LiveData(int LastProcessedID = 0) {
      String SQL = @"SELECT 
        ID,
        ProcessTime,
        MinSpeed,
        MedSpeed,
        AvgSpeed,
        MaxSpeed,
        VechileCount
      FROM (
        SELECT TOP 20 
          ID,
          CreatedDate,
          FORMAT(CreatedDate, 'hh:mm', 'en-US') AS ProcessTime,
          40 AS MinSpeed,
          50 AS MedSpeed,
          50 AS AvgSpeed,
          80 AS MaxSpeed,
          CarCount AS VechileCount
        FROM TrafficData";
     if(LastProcessedID > 0) {
        SQL = SQL + @"
        WHERE
          ID > " + LastProcessedID;
      }
      SQL = SQL +
      @"   ORDER BY CreatedDate DESC
        ) AS InnerSelect
      ORDER BY InnerSelect.CreatedDate ASC
      ";

      return Json(Util.jsonRS(SQL), JsonRequestBehavior.AllowGet);
    }

    [System.Web.Mvc.HttpGet]
    public JsonResult GetMoreInfo() {
      String SQL = "select convert(varchar(8),convert(time,ProcessTime),108) as ProcessTime,MaxSpeed,MinSpeed,MediumSpeed,NumberOfCar from PayloadTraffic";
      var Row = Util.getDBRows(SQL);
      return Json(Row, JsonRequestBehavior.AllowGet);
    }
  }
}