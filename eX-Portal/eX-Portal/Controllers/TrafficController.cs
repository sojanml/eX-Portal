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

    public ActionResult Live([Bind(Prefix = "ID")] int FlightID = 0) {
            if (!exLogic.User.hasAccess("TRAFFIC.LIVE")) return RedirectToAction("NoAccess", "Home");

            var oList = from f in db.DroneFlights join d in db.MSTR_Drone 
                        on f.DroneID equals d.DroneId into ps
                        from p in ps.DefaultIfEmpty()
                        where f.ID == FlightID 
                        select f;

            Models.DroneFlight dFlight = db.DroneFlights.Find(FlightID);


            var oListDrone = from d in db.MSTR_Drone where d.DroneId == dFlight.DroneID
                             select d;

            
            //var viewModel = new ViewModel.TrafficViewModel
            //{
            //    DroneFlight = db.DroneFlights.Find(FlightID),
            //    MSTR_Drone = db.MSTR_Drone.Where(viewModel.MSTR_Drone.DroneId == dFlight.DroneID)
            //};
            return View(oList);
        }

    public JsonResult LiveData(int LastProcessedID = 0) {
      String SQL = @"SELECT 
        PTid,
        ProcessTime,
        MinSpeed,
        MedSpeed,
        AvgSpeed,
        MaxSpeed,
        VechileCount
      FROM (
        SELECT TOP 200 
          PTid,
          CreatedDate,
          FORMAT(CreatedDate, 'hh:mm', 'en-US') AS ProcessTime,
          40 as MinSpeed,
          50 as MedSpeed,
          --floor(((MinSpeed+MediumSpeed+MaxSpeed)/3)) as AvgSpeed,
          45 as AvgSpeed,
          60 as MaxSpeed,
          NumberOfCar AS VechileCount
        FROM dbo.PayloadTraffic ";
     if(LastProcessedID > 0) {
        SQL = SQL + @"
        WHERE
          PTid > " + LastProcessedID;
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