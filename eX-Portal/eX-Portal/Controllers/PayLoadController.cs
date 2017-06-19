﻿using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class PayloadController : Controller {
    // GET: Payload
    public ActionResult Index() {
      ViewBag.Title = "Payload - RFIDS";
      return View();

    }

    public JsonResult Runs() {
      ExponentPortalEntities db = new ExponentPortalEntities();
      var Query = from data in db.PayLoadDataRFIDs
                  group data by data.FlightUniqueID into gdata
                  let TheData = new {
                    FlightUniqueID = gdata.Key,
                    RFIDCount = gdata.Count(),
                    ReadDate = gdata.Min(_ => _.ReadMinDate)
                  }
                  orderby TheData.ReadDate
                  select TheData;
                                    
      return Json(Query, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Data(String ID) {
      ExponentPortalEntities db = new ExponentPortalEntities();
      var Query1 = from data in db.PayLoadDataRFIDs
                  where data.FlightUniqueID == ID
                  orderby data.Row, data.Col
                  select data;

      var Query2 = from data in db.PayLoadDatas
                   where data.FlightUniqueID == ID &&
                   data.Latitude > 0 &&
                   data.Longitude > 0
                   orderby data.ReadTime
                   select new {
                     lat = data.Latitude,
                     lng = data.Longitude
                   };

      var LatLngQuery = from data in db.PayloadDataGrids
                        where data.FlightUniqueID == ID
                        orderby data.SlNo
                        select new {
                          lat = data.Lat,
                          lng = data.Lng
                        };
    
                   

      var jObject = new {
        RFID = Query1,
        FlightPath = Query2,
        BoundBox = LatLngQuery
      };

      return Json(jObject, JsonRequestBehavior.AllowGet);
    }



    public JsonResult RFID(String ID, String RFID) {
      ExponentPortalEntities db = new ExponentPortalEntities();


      var Query2 = from data in db.PayLoadDatas
                   where data.FlightUniqueID == ID &&
                   data.RFID == RFID &&
                   data.Latitude > 0 &&
                   data.Longitude > 0
                   orderby data.ReadTime
                   select new {
                     lat = data.Latitude,
                     lng = data.Longitude,
                     rssi = data.RSSI
                   };
      

      return Json(Query2, JsonRequestBehavior.AllowGet);
    }

  }
}