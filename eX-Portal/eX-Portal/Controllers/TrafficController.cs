﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.IO;
using Newtonsoft.Json;
using System.Text;

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

    public ActionResult DMAT([Bind(Prefix="id")] int DroneID = 0, int FlightID = 0) {
      //if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      var Monitor = new TrafficMamager(DroneID, FlightID);
      return View(Monitor);
    }

        public ActionResult LPR()
        {
            //if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
           // var Monitor = new TrafficMamager(DroneID, FlightID);
            return View();
        }

        [System.Web.Mvc.HttpGet]
    public ActionResult Export([Bind(Prefix = "id")] int DroneID = 0, int FlightID = 0, int Interval = 5) {
      var Monitor = new TrafficMamager(DroneID, FlightID);
      StringBuilder CSV = Monitor.Export(Interval);
      byte[] dest = Encoding.ASCII.GetBytes(CSV.ToString());
      String FileName = $"Export-{FlightID}-{DateTime.Now.ToString("yymmddHHMMss")}.csv";
      return File(dest, "application/vnd.ms-excel", FileName);
    }

    public ActionResult RTA() {
      if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      return View();
    }

    public ActionResult Live([Bind(Prefix = "ID")] int FlightID = 0) {
      if (!exLogic.User.hasAccess("TRAFFIC.LIVE")) return RedirectToAction("NoAccess", "Home");

      ViewModel.TrafficDashboard DashBoardInfo = null; 

      var FlightInfo = db.DroneFlight.Where(f => f.ID == FlightID).FirstOrDefault();
      if(FlightInfo != null) {
        DashBoardInfo = new ViewModel.TrafficDashboard() {
          FlightID = FlightInfo.ID,
          FlightDate = ((DateTime)(FlightInfo.FlightDate)).ToString("dd-MMM-yyyy HH:MM GST"),
          Drone = db.MSTR_Drone.Where(d => d.DroneId == FlightInfo.DroneID).Select(s => s.DroneName).FirstOrDefault(),
          Lat = (Double)FlightInfo.Latitude,
          Lng = (Double)FlightInfo.Longitude,
          Pilot = db.MSTR_User.Where(w => w.UserId == FlightInfo.PilotID).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault(),
          GSC = db.MSTR_User.Where(w => w.UserId == FlightInfo.GSCID).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault(),
          FlightVideo = db.DroneFlightVideos.Where(w=>w.FlightID == FlightInfo.ID).Select(s => s.VideoURL).FirstOrDefault()
        };
      }
      
      return View(DashBoardInfo);
    }

    public JsonResult LiveData(int FlightID = 0, int LastProcessedID = 0) {
      String SQL = $@"SELECT 
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
        FROM 
          PayloadTraffic 
        WHERE
          PayloadTraffic.FlightID='{FlightID}'
          {((LastProcessedID > 0) ? " AND PTid > " + LastProcessedID : "") }
      ORDER BY CreatedDate DESC
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

    public ActionResult TrafficMonitoring()
        {
            ViewBag.Title = "Trafic Monitoring";
          //  if (!exLogic.User.hasAccess("TRAFFIC.LIVE")) return RedirectToAction("NoAccess", "Home");

            string SQL = $@"Select [MonitorID]  [Monitor_ID]
                          ,[CreatedDate],
                            [DD]
                          ,[DR]
                          ,[DU]
                          ,[DL]
                          ,[RD]
                          ,[RR]
                          ,[RU]
                          ,[RL]
                          ,[UD]
                          ,[UR]
                          ,[UU]
                          ,[UL]
                          ,[LD]
                          ,[LR]
                          ,[LU]
                          ,[LL],FlightID,
                            Count(*) Over() as _TotalRecords,
                           [MonitorID] as _PKey
                      FROM [MSTR_TrafficMonitor]";
            qView nView = new qView(SQL);
            
            nView.addMenu("View", Url.Action("dmat", new { FlightID = "FlightID" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
            
        }
  }
}