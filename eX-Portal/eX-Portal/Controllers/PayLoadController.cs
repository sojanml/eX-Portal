﻿using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class PayLoadController : Controller {
    public ActionResult InDoorFlightDetails([Bind(Prefix = "ID")] String FlightUniqueID) {
      // if (!exLogic.User.hasAccess("PAYLOAD.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "In Door Flight Details";

      String SQL = @"SELECT
              PayLoadFlight.FlightUniqueID, 
              PayLoadMapData.RFID ,            
              [RFIDCount],
              PayLoadFlight.CreatedTime,              
              PayLoadFlight.ShelfID,
               Count(*) Over() as _TotalRecords,
              PayLoadFlight.FlightUniqueID as _PKey
      FROM
              PayLoadFlight

      LEFT JOIN PayLoadMapData ON
              PayLoadMapData.FlightUniqueID=payloadflight.FlightUniqueID
     WHERE
              PayLoadFlight.Processingmodel= 1
     AND PayLoadFlight.FlightUniqueID= " + FlightUniqueID +
" AND PayLoadMapData.IsValid= 0";
      qView nView = new qView(SQL);


      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }





    public ActionResult OutDoor() {
      if (!exLogic.User.hasAccess("OUTDOOR.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Out Door Flights";
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
        PayLoadYard.YardID = PayLoadFlight.YardID 
     WHERE
        PayLoadFlight.Processingmodel=0";

      qView nView = new qView(SQL);
      nView.addMenu("PayLoad Data", Url.Action("PayLoad", "Map", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }


    //InDoor
    public ActionResult InDoor() {
      if (!exLogic.User.hasAccess("INDOOR.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "In Door Flights";
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
        PayLoadYard.YardID = PayLoadFlight.YardID 
     WHERE
        PayLoadFlight.Processingmodel=1";

      qView nView = new qView(SQL);
      nView.addMenu("InDoor Flight Details", Url.Action("InDoorFlightDetails", "PayLoad", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }



    // GET: PayLoad


    public ActionResult Index() {
      if (!exLogic.User.hasAccess("PAYLOAD.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "PayLoad Flights";
      String SQL =
      @"SELECT
        PayLoadFlightID as ID, 
        FlightID,
        PayLoadYard.YardName,
        (CASE Processingmodel WHEN 1 Then 'Outdoor' ELSE 'Indoor' END) as Processingmodel,
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


        public ActionResult InfraRed()
        {
            return View();
        }

        public ActionResult Videography()
        {
            return View();
        }
        public String getRFID(int Row, int Column, String FlightUniqueID) {
      StringBuilder theRow = new StringBuilder();
      String SQL = @"SELECT RFID, RSSI FROM PayLoadMapData
      WHERE
        FlightUniqueID='" + FlightUniqueID + @"' AND
        RowNumber=" + (Row - 1) + @" AND
        ColumnNumber=" + (Column - 1);
      var Rows = Util.getDBRows(SQL);
      foreach (var Record in Rows) {
        if (theRow.Length > 0) theRow.Append(", ");
        theRow.Append(Record["RFID"]);
        theRow.Append(" [");
        theRow.Append(Record["RSSI"]);
        theRow.Append("]");
      }
      return theRow.ToString();
    }

    public String AutoCorrect(String FlightUniqueID) {
      GeoGrid myGrid = new GeoGrid(FlightUniqueID);
      String SQL = "EXEC usp_PayLoad_AutoCorrectGrid '" + FlightUniqueID + "', " + myGrid.YardID;
      Util.doSQL(SQL);
      return myGrid.getGrid(FlightUniqueID, true);
      //return "OK";
    }

    public ActionResult Table(String FlightUniqueID) {
      GeoGrid myGrid = new GeoGrid(FlightUniqueID);
      ViewBag.JSon = myGrid.getTable(FlightUniqueID);
      return View();
    }

    public ActionResult Detail([Bind(Prefix = "ID")] String FlightUniqueID) {
      var Parts = FlightUniqueID.Split(',');
      ViewBag.Title = "UAS Listing";

      String SQL = @"SELECT 
        [RSSI],
        [ReadTime],
        [ReadCount],
        [Latitude],
        [Longitude],
        [RowNumber],
        [ColumnNumber],
        [CellID],        
        [IsProcessed],
        Count(*) Over() as _TotalRecords
      FROM 
        [PayLoadData]
      WHERE
         [RFID] ='" + Parts[0] + @"' AND
          [FlightUniqueID]  ='" + Parts[1] + @"'";

      qView nView = new qView(SQL);
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

  }//class PayLoadController
}//namespace eX_Portal.Controllers