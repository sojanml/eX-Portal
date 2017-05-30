using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class PayLoad_Removed_Controller : Controller {

    public ActionResult RawData(String ID = "") {
      if (!exLogic.User.hasAccess("PAYLOAD.RAW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Payload RAW Data";
      String SQL = @"SELECT  
          PayLoadDataID as ID,
          FlightUniqueID as UID,
          RFID,
          RSSI,
          ReadTime,
          ReadCount,
          ProcessingModel as PModel,
          CreatedTime,
            Count(*) Over() as _TotalRecords,
            PayLoadDataID as _PKey
      FROM 
        [PayLoadData]";
      if (!String.IsNullOrEmpty(ID)) SQL = SQL + "\n" +
      " WHERE FlightUniqueID='" + ID + "'";

      qView nView = new qView(SQL);
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }

    public ActionResult InDoorFlightDetails([Bind(Prefix = "ID")] String FlightUniqueID) {
      if (!exLogic.User.hasAccess("INDOOR.VIEW")) return RedirectToAction("NoAccess", "Home");
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
        (CASE Processingmodel WHEN 1 Then 'Indoor' ELSE 'Outdoor' END) as Processingmodel,
        [RFIDCount],
        [CreatedTime],
        Count(*) Over() as _TotalRecords,
        FlightUniqueID as _PKey
      FROM
        PayLoadFlight
      LEFT JOIN PayLoadYard ON
        PayLoadYard.YardID = PayLoadFlight.YardID
      LEFT JOIN [MSTR_Drone] ON
        [MSTR_Drone].DroneId = PayLoadFlight.PayLoadDroneID
      ";

            if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
            {

                    SQL +=
                      "WHERE\n" +
                      "  [MSTR_Drone].AccountID=" + Util.getAccountID();
            }

      qView nView = new qView(SQL);
      nView.addMenu("PayLoad Data", Url.Action("PayLoad", "Map", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("PAYLOAD.RAW"))
        nView.addMenu("Raw Data", Url.Action("RawData", "Payload", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//ActionResult Index()


    public ActionResult InfraRed() {
      if (!exLogic.User.hasAccess("PAYLOAD.INFRARED")) return RedirectToAction("NoAccess", "Home");
      return View();
    }

    public ActionResult Videography() {
      if (!exLogic.User.hasAccess("PAYLOAD.VIDEOGRAPHY")) return RedirectToAction("NoAccess", "Home");
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




    /// <summary>
    /// Created By Roshan
    /// </summary>
    /// <returns></returns>
    /// 
    static String RootUploadDir = "~/Upload/PayLoad/Drone";
    public ActionResult GeoTag([Bind(Prefix = "ID")] int DroneID = 0) {
      //if (!exLogic.User.hasAccess("FLIGHT.GEOTAG")) return RedirectToAction("NoAccess", "Home");
      ExponentPortalEntities Db = new ExponentPortalEntities();
      ViewBag.DroneID = DroneID;
      List<DroneDocument> Docs = (from o in Db.DroneDocuments
                                  where o.DocumentType == "GEO Tag1" &&
                                  o.DroneID == DroneID
                                  select o).ToList();
      ViewBag.FirstRow = true;

      if (Docs.Count == 0) {
        DroneDocument DC = new DroneDocument();
        DC.DocsType = "Infrared";
        Docs.Add(DC);
      }
      return View(Docs);

    }//GeoTag


    private String getUploadedDocs([Bind(Prefix = "ID")] int DroneID = 0) {
      StringBuilder theList = new StringBuilder();
      String DroneName = "";
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<DroneDocument> Docs = (from r in db.DroneDocuments
                                  where (int)r.DroneID == DroneID
                                  select r).ToList();
      theList.Append("<UL>");
      foreach (var Doc in Docs) {
        if (DroneName == "") DroneName = Util.getDroneName(Doc.DroneID);
        theList.AppendLine("<LI><span class=\"icon\">&#xf0f6;</span> <a href=\"/upload/Drone/" + DroneName + "/" + DroneID +
        "/" + Doc.DocumentName + "\">" + Util.getFilePart(Doc.DocumentName) + "</a></LI>");
      }
      theList.Append("</UL>");
      return theList.ToString();
    }



    public String DeleteFile([Bind(Prefix = "ID")] int FlightID, String file) {

      //now add the uploaded file to the database
      String SQL = "DELETE FROM DroneDocuments\n" +
        "WHERE\n" +
        "  DocumentName='" + file + "' AND\n" +
        "  FlightID = '" + FlightID + "'";
      Util.doSQL(SQL);

      StringBuilder JsonText = new StringBuilder();
      JsonText.Append("{");
      JsonText.Append(Util.Pair("status", "success", false));
      JsonText.Append("}");

      return JsonText.ToString();
    }

    public String UploadFile([Bind(Prefix = "ID")] int DroneID = 0, String DocumentType = "Infrared", String CreatedOn = "") {
      int FlightID = 0;
      //DateTime FileCreatedOn = DateTime.MinValue;
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      GPSInfo GPS = new GPSInfo();
      Response.ContentType = "text/json";

      try {
        //FileCreatedOn = DateTime.ParseExact(CreatedOn, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        string script = "select flightID from flightmapdata where droneID = '" + DroneID + "' and readtime = '" + Util.toDate(CreatedOn) + "'";
        FlightID = Util.getDBInt(script);
      } catch { 
      
      }

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName.ToLower();
        String DroneName = Util.getDroneNameByDroneID(FlightID);
        String UploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
        String FileURL = FileName;
        String FullName = UploadDir + FileName;
        String GPSFixName = UploadDir + "GPS-" + FileName;
        String testFullName;
        //Save the file to Disk
        if (!Directory.Exists(UploadDir)) Directory.CreateDirectory(UploadDir);
        TheFile.SaveAs(FullName);
        testFullName = FullName;
        //Do the calculation for GPS
        if (DocumentType == "Geo Tag" && System.IO.Path.GetExtension(FullName).ToLower() == ".jpg") {
          //here find the code to find the GPS Cordinate
          ExifLib GeoTag = new ExifLib(FullName, GPSFixName);
          GPS = GeoTag.getGPS(FlightID, Util.toDate(CreatedOn));
          GeoTag.setGPS(GPS);
          GeoTag.SetThumbnail(100);
          //System.IO.File.Delete(FullName);
          FullName = GPSFixName;
          FileURL = "GPS-" + FileName;
        }


        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append(Util.Pair("url", "/upload/payload/drone/" + DroneName + "/" + FlightID + "/" + FileURL, true));
        JsonText.Append("\"GPS\":{");
        JsonText.Append("\"Info\":\"");
        JsonText.Append(GPS.getInfo());
        JsonText.Append("\",");
        JsonText.Append("\"Latitude\":");
        JsonText.Append(GPS.Latitude);
        JsonText.Append(",");
        JsonText.Append("\"Longitude\":");
        JsonText.Append(GPS.Longitude);
        JsonText.Append(",");
        JsonText.Append("\"Altitude\":");
        JsonText.Append(GPS.Altitude);
        JsonText.Append("},");
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName, FileURL));
        JsonText.Append("]}");

        //now add the uploaded file to the database
        String SQL = "INSERT INTO DroneDocuments(\n" +
          " DroneID, FlightID, DocumentType, DocumentName, UploadedDate, UploadedBy,\n" +
          " Latitude, Longitude, Altitude \n" +
          ") VALUES (\n" +
          "  '" + DroneID + "',\n" +
          "  '" + FlightID + "',\n" +
          "  '" + DocumentType + "',\n" +
          "  '" + testFullName + "',\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  " + GPS.Latitude + ", " + GPS.Longitude + ", " + GPS.Altitude + "\n" +
          ")";
        int DocumentID = Util.InsertSQL(SQL);

      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//UploadFile()

    private void MoveUploadFileTo(int DroneID, int FlightID) {
      String[] Files = Request["FileName"].Split(',');
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      String OldUploadDir = UploadPath + "0\\0\\";
      String DroneName = DroneID == 0 ? "0" : Util.getDroneName(DroneID);
      String NewUploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
      foreach (String file in Files) {
        if (String.IsNullOrEmpty(file)) continue;
        String OldFullPath = OldUploadDir + file;
        String NewFullPath = NewUploadDir + file;
        if (System.IO.File.Exists(OldFullPath)) {
          if (!Directory.Exists(NewFullPath)) Directory.CreateDirectory(NewUploadDir);
          if (!System.IO.File.Exists(NewFullPath)) System.IO.File.Move(OldFullPath, NewFullPath);
          String SQL = "UPDATE DroneDocuments SET\n" +
          "  DroneID=" + DroneID + ",\n" +
          "  FlightID=" + FlightID + "\n" +
          "WHERE\n" +
          "  DocumentName='" + file + "'";
          Util.doSQL(SQL);
        }//if(System.IO.File.Exists
      }//foreach (String file in Files)


    }//MoveUploadFileTo
  }//class PayLoadController

}//namespace eX_Portal.Controllers