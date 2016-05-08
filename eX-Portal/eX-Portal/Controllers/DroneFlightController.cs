using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using eX_Portal.ViewModel;

namespace eX_Portal.Controllers {
  public class DroneFlightController : Controller {
    static String RootUploadDir = "~/Upload/Drone/";
    ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: DroneFlight
    public ActionResult Index([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "UAS Flights";
      ViewBag.DroneID = DroneID;
      String SQLFilter = "";
      String SQL = @"SELECT 
        DroneFlight.ID,
        MSTR_Drone.DroneName AS UAS,
        tblPilot.FirstName AS PilotName,
        tblGSC.FirstName AS GSCName,
        tblPilot.FirstName AS CreatedBy,
        FlightDate AS 'FlightDate(UTC)',
        CASE 
          WHEN 
             [MSTR_Drone].LastFlightID = DroneFlight.ID and
             [MSTR_Drone].FlightTime > DATEADD(MINUTE, -1, GETDATE())
            THEN '<span class=green_icon>&#xf04b;</span>'
          WHEN (
              SELECT Count(*)
              FROM DroneFlightVideo
              WHERE DroneFlightVideo.FlightID = DroneFlight.ID
              ) = 0
            THEN ''
          ELSE '<span class=icon>&#xf03d;</span>'
          END AS Video,
        Count(*) OVER () AS _TotalRecords,
        DroneFlight.ID AS _PKey
      FROM 
        DroneFlight
      LEFT JOIN MSTR_Drone
        ON MSTR_Drone.DroneId = DroneFlight.DroneID
      LEFT JOIN MSTR_User AS tblPilot
        ON tblPilot.UserID = DroneFlight.PilotID
      LEFT JOIN MSTR_User AS tblGSC
        ON tblGSC.UserID = DroneFlight.GSCID
      LEFT JOIN MSTR_User AS tblCreated
        ON tblCreated.UserID = DroneFlight.CreatedBy
      ";
      if (DroneID > 0) {
        if (SQLFilter != "") SQLFilter += " AND";
        SQLFilter += "\n" +
        "  DroneFlight.DroneID=" + DroneID;
        ViewBag.Title += " [" + Util.getDroneName(DroneID) + "]";
      }

      if (!exLogic.User.hasAccess("DRONE.MANAGE")) {
        if (SQLFilter != "") SQLFilter += " AND";
        SQLFilter += " \n" +
          "  MSTR_Drone.AccountID=" + Util.getAccountID();
      }

      if (SQLFilter != "") {
        SQL += "\n WHERE\n" + SQLFilter;
      }
      qView nView = new qView(SQL);


      // if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      if (exLogic.User.hasAccess("FLIGHT.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.VIEW")) nView.addMenu("Detail", Url.Action("Detail", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.MAP")) nView.addMenu("Flight Map", Url.Action("FlightData", "Map", new { ID = "_PKey" }));
      nView.addMenu("Flight Data", Url.Action("FlightDataView", "Map", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("FLIGHT.VIDEOS")) nView.addMenu("Flight Videos", Url.Action("ListVdeos", "DroneFlight", new { ID = "_PKey" }));
            //nView.addMenu("Flight Videos", Url.Action("List", "DroneFlight", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("FLIGHT.GEOTAG")) nView.addMenu("GEO Tagging", Url.Action("GeoTag", "DroneFlight", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Index()


    public ActionResult GeoTag([Bind(Prefix = "ID")] int FlightID = 0) {
      ExponentPortalEntities Db = new ExponentPortalEntities();
      ViewBag.FlightID = FlightID;
      List<DroneDocument> Docs = (from o in Db.DroneDocuments
                                  where o.DocumentType == "GEO Tag" &&
                                  o.FlightID == FlightID
                                  select o).ToList();

      return View(Docs);
    }//GeoTag

    public ActionResult Create([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.CREATE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Create UAS Flight";
      DroneFlight InitialData = new DroneFlight();
      InitialData.DroneID = DroneID;
      return View(InitialData);
    }

    [HttpPost]
    public ActionResult Create(DroneFlight theFlight) {
      if (!exLogic.User.hasAccess("FLIGHT.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (theFlight.DroneID < 1 || theFlight.DroneID == null) ModelState.AddModelError("DroneID", "You must select a Drone.");
      if (theFlight.PilotID < 1 || theFlight.PilotID == null) ModelState.AddModelError("PilotID", "Pilot is required for a Flight.");
      if (theFlight.GSCID < 1 || theFlight.GSCID == null) ModelState.AddModelError("GSCID", "A Ground Station Controller should be slected.");

      if (ModelState.IsValid) {
        int ID = 0;
        ExponentPortalEntities db = new ExponentPortalEntities();
        db.DroneFlights.Add(theFlight);
        db.SaveChanges();
        ID = theFlight.ID;
        db.Dispose();

        //Move the uploaded file to correct path
        MoveUploadFileTo((int)theFlight.DroneID, theFlight.ID);

        //insert into log table
        string sql = "insert into MSTR_Pilot_Log(DroneId,Date,PilotId,MultiDashRotor,FlightID)values(" + theFlight.DroneID + ",'" + System.DateTime.Now + "'," + theFlight.PilotID + "," + (theFlight.FlightHours == null ? 0 : (theFlight.FlightHours / 60)) + "," + ID + ")";
        Util.doSQL(sql);

      return RedirectToAction("Detail", new { ID = ID });
      } else {
        ViewBag.Title = "Create UAS Flight";
        return View(theFlight);
      }

    }


    public ActionResult Edit([Bind(Prefix = "ID")] int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.EDIT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Edit UAS Flight";
      ExponentPortalEntities db = new ExponentPortalEntities();
      DroneFlight InitialData = db.DroneFlights.Find(FlightID);
      return View(InitialData);
    }

    [HttpPost]
    public ActionResult Edit(DroneFlight InitialData) {
      if (!exLogic.User.hasAccess("FLIGHT.EDIT")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "Edit UAS Flight";
      ExponentPortalEntities db = new ExponentPortalEntities();
      db.Entry(InitialData).State = EntityState.Modified;
      db.SaveChanges();
            //insert/update in log table
            string sqlcheck = "select Id from MSTR_Pilot_Log where FlightID=" + InitialData.ID;
            int result = Convert.ToInt32(Util.getDBInt(sqlcheck));
            if (result > 0)
            {
                string sql = "update MSTR_Pilot_Log set DroneId=" + InitialData.DroneID + ",PilotId=" + InitialData.PilotID + ",MultiDashRotor=" + (InitialData.FlightHours == null ? 0 : (InitialData.FlightHours / 60)) + "where FlightID=" + InitialData.ID;
                Util.doSQL(sql);
            }
            else
            {
                string sql = "insert into MSTR_Pilot_Log(DroneId,Date,PilotId,MultiDashRotor,FlightID)values(" + (InitialData.DroneID == null ? 0 : InitialData.DroneID) + ",'" + System.DateTime.Now + "'," + (InitialData.PilotID == null ? 0 : InitialData.PilotID) + "," + (InitialData.FlightHours == null ? 0 : (InitialData.FlightHours / 60)) + "," + InitialData.ID + ")";
                Util.doSQL(sql);
            }

            return RedirectToAction("Detail", new { ID = InitialData.ID });
    }


    // GET: Drone/Delete/5
    public String Delete([Bind(Prefix = "ID")]int FlightID = 0) {
      String SQL = "";
      Response.ContentType = "text/json";
      if (!exLogic.User.hasAccess("FLIGHT.DELETE"))
        return Util.jsonStat("ERROR", "Access Denied");

      //Delete the drone from database if there is no checklist is created
      SQL = "SELECT Count(*) FROM [DroneCheckList] WHERE FlightID = " + FlightID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a flight after creating checklist");

      SQL = "SELECT Count(*) FROM [FlightMapData] WHERE FlightID = " + FlightID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a flight when contain the GPS data");

      SQL = "DELETE FROM [DroneFlight] WHERE ID = " + FlightID;
      Util.doSQL(SQL);

      string sqldeltlog = "delete from MSTR_Pilot_Log where FlightID=" + FlightID;
      Util.doSQL(sqldeltlog);

      return Util.jsonStat("OK");
    }

    public ActionResult Detail(int ID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.VIEW")) return RedirectToAction("NoAccess", "Home");
      if (!exLogic.User.hasDrone(Util.GetDroneIdFromFlight(ID))) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "UAS Flight Details";
      ViewBag.FlightID = ID;

      String SQL =
      "SELECT \n" +
      "  [DroneCheckList].[ID],\n" +
      "  MSTR_DroneCheckList.CheckListTitle,\n" +
      "  MSTR_DroneCheckList.CheckListSubTitle,\n" +
      "  MSTR_User.FirstName as CreatedBy,\n" +
      "  [DroneCheckList].[CreatedOn],\n" +
      "  Count(*) Over() as _TotalRecords, \n" +
      "  [DroneCheckList].[ID] as _PKey\n" +
      "FROM\n" +
      "[DroneCheckList]\n" +
      "LEFT JOIN MSTR_DroneCheckList ON\n" +
      "MSTR_DroneCheckList.ID = [DroneCheckList].DroneCheckListID\n" +
      "LEFT JOIN MSTR_User ON\n" +
      "  MSTR_User.UserID = [DroneCheckList].CreatedBy\n" +
      "WHERE\n" +
      "  [DroneCheckList].[FlightID] = " + ID.ToString();

      qView nView = new qView(SQL);
      nView.addMenu("View", Url.Action("View", "DroneCheckList", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Detail()

    public String DroneFlightDetail(int ID = 0) {
      String CheckListMessage = "";
      if (!exLogic.User.hasAccess("FLIGHT.VIEW")) return "Access Denied";
      int DroneId, UserId;

      string UASFormat, PilotFormat;
      String UploadedDocs = "";

      String SQL =
      "SELECT\n" +
      "   DroneFlight.ID UASFlightId,\n" +
      "   MSTR_Drone.DroneName as UAS,\n" +
      "   tblPilot.FirstName + ' ' + tblPilot.MiddleName + ' ' + tblPilot.LastName  as PilotName,\n" +
      "   tblGSC.FirstName as [GroundStation],\n" +
      "   tblPilot.FirstName as CreatedBy,\n" +
      "   FORMAT(FlightDate, 'dd-MMM-yyyy HH:mm:ss', 'en-US' ) as 'FlightDate(UTC)'\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy\n" +
      "WHERE\n" +
      "  DroneFlight.ID =" + ID.ToString();

      qDetailView theView = new qDetailView(SQL);
      theView.Columns = 3;
      //this part for adding link to requred fields in the details
      DroneId = Util.GetDroneIdFromFlight(ID);
      UserId = Util.GetPilotIdFromFlight(ID);

      UASFormat = "<a href='/Drone/Detail/" + DroneId + "'> " + Util.GetUASFromFlight(ID) + "</a>";//url
      PilotFormat = "<a href='/User/UserDetail/" + UserId + "'> " + Util.GetPilotFromFlight(ID) + "</a>";//url
      theView.FormatCols.Add("UAS", UASFormat); //Adding the Column required for formatting  
      theView.FormatCols.Add("PilotName", PilotFormat); // //Adding the Column required for formatting  

      SQL = @"
      SELECT 
        Count([DroneCheckList].[ID]) as FlightCheckList
      FROM 
        [DroneCheckList]
      LEFT JOIN [MSTR_DroneCheckList] ON
        [MSTR_DroneCheckList].ID = [DroneCheckListID]
      WHERE
        [MSTR_DroneCheckList].[CheckListTitle]='Pre-Flight Checklist' AND
        [DroneCheckList].[FlightID]=" + ID;
      int CheckListCount = Util.getDBInt(SQL);
      if (CheckListCount >= 3) {
        CheckListMessage = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "You have successfully completed all the checklist</div>";
      } else if (CheckListCount >= 1) {
        CheckListMessage = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "Please complete all the checklist for the flight.</div>";
      } else {
        CheckListMessage = "<div class=\"invalid\"><span class=\"icon\">&#xf071;</span>" +
        "You need to complete all checklist before the flight</div>";
      }

      //Check the documents for GCA is uploaded
      SQL = "SELECT\n" +
      "  Count(*)\n" +
      "FROM\n" +
      "  [DroneDocuments]\n" +
      "WHERE\n" +
      "  FlightID = " + ID.ToString() + " and\n" +
      "  DocumentType = 'GCA Approval'\n";
      int TheCount = Util.getDBInt(SQL);
      if (TheCount < 1) {
        UploadedDocs = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "Please upload your DCAA Authorisation document before the flight</div>";
      } else {
        UploadedDocs = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "Your DCAA Authorization: " + getUploadedDocs(ID) +
        "</div>";
      }
      return UploadedDocs + CheckListMessage + theView.getTable();



    }//DroneFlightDetail ()

    private String getUploadedDocs(int FlightID) {
      StringBuilder theList = new StringBuilder();
      String DroneName = "";
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<DroneDocument> Docs = (from r in db.DroneDocuments
                                  where (int)r.FlightID == FlightID
                                  select r).ToList();
      theList.Append("<UL>");
      foreach (var Doc in Docs) {
        if (DroneName == "") DroneName = Util.getDroneName(Doc.DroneID);
        theList.AppendLine("<LI><span class=\"icon\">&#xf0f6;</span> <a href=\"/upload/Drone/" + DroneName + "/" + FlightID +
        "/" + Doc.DocumentName + "\">" + Util.getFilePart(Doc.DocumentName) + "</a></LI>");
      }
      theList.Append("</UL>");
      return theList.ToString();
    }


    public String ByDrone([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT")) return "Access Denied";
      String SQL =
      "SELECT TOP 5" +
      "   MSTR_Drone.DroneName as UAS,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate as 'FlightDate(UTC)'\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy\n" +
      "WHERE\n" +
      "  DroneFlight.DroneID=" + DroneID + "\n" +
      "ORDER BY" +
      "  DroneFlight.ID DESC";

      qView nView = new qView(SQL, false);
      if (nView.HasRows) {
        return
          "<h2>Recent Flights</h2>\n" +
          nView.getDataTable(true, false);
      }

      return "";

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

    public String UploadFile([Bind(Prefix = "ID")] int FlightID = 0, int DroneID = 0, String DocumentType = "GCAA Approval", String CreatedOn = "") {
      DateTime FileCreatedOn = DateTime.MinValue;
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      GPSInfo GPS = new GPSInfo();
      Response.ContentType = "text/json";

      try {
        FileCreatedOn = DateTime.ParseExact(CreatedOn, "ddd, d MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture);
      } catch { }

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName.ToLower();
        String DroneName = Util.getDroneNameByFlight(FlightID);
        String UploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
        String FileURL = FileName;
        String FullName = UploadDir + FileName;
        String GPSFixName = UploadDir + "GPS-" + FileName;

        //Save the file to Disk
        if (!Directory.Exists(UploadDir)) Directory.CreateDirectory(UploadDir);
        TheFile.SaveAs(FullName);

        //Do the calculation for GPS
        if (DocumentType == "Geo Tag" && System.IO.Path.GetExtension(FullName).ToLower() == ".jpg") {
          //here find the code to find the GPS Cordinate
          ExifLib GeoTag = new ExifLib(FullName, GPSFixName);
          GPS = GeoTag.getGPS(FlightID, FileCreatedOn);
          GeoTag.setGPS(GPS);
          GeoTag.setThumbnail(100);
          //System.IO.File.Delete(FullName);
          FullName = GPSFixName;
          FileURL = "GPS-" + FileName;
        }


        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append(Util.Pair("url", "/upload/drone/" + DroneName + "/" + FlightID + "/" + FileURL, true));
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
          "  '" + FileURL + "',\n" +
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
    public ActionResult UASFiles(int FlightID = 0) {
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<DroneDocument> Docs = (from r in db.DroneDocuments
                                  where (int)r.FlightID == FlightID
                                  select r).ToList();
      return View(Docs);
    }
    public ActionResult PortalAlert() {
      String SQL = "SELECT [AlertID] ,[FlightID] ,[FlightReadTime],[Latitude],[Longitude],[Altitude],[FlightDataID],[AlertMessage],[CreatedOn],Count(*) Over() as _TotalRecords,FlightID as _PKey FROM[ExponentPortal].[dbo].[PortalAlert]";
      qView nView = new qView(SQL);
      nView.addMenu("Report", Url.Action("Index", "Drone", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult List([Bind(Prefix = "ID")]int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.VIDEOS")) return RedirectToAction("NoAccess", "Home");
      //using (var  = new ExponentPortalEntities())
      {

        var List = db.DroneFlightVideos.ToList();
        var vlist = (from p in List where p.FlightID == FlightID && p.IsDeleted == 0 select p).ToList();
        return View(vlist);


      }
    }


        public ActionResult ListVdeos([Bind(Prefix = "ID")]int FlightID = 0)
        {
            if (!exLogic.User.hasAccess("FLIGHT.VIDEOS")) return RedirectToAction("NoAccess", "Home");

            ViewBag.FlightID = FlightID;
            var Videos = (from n in db.DroneFlightVideos
                          where n.FlightID == FlightID && n.IsDeleted == 0
                          select n).ToList();


            return View(Videos);
        }

        public string DeleteVdeo([Bind(Prefix = "ID")]int id = 0)
        {
            if (!exLogic.User.hasAccess("FLIGHT.DELTVIDEOS")) return "Access Denied";
            Response.ContentType = "text/json";
            string sql = "Update DroneFlightVideo set IsDeleted=1 where VideoID=" + id;
            Util.doSQL(sql);
            string sql1 = "select FlightID from DroneFlightVideo where VideoID=" + id;
            int flightid = Util.getDBInt(sql1);
            // return RedirectToAction("ListVdeos", "DroneFlight", new { id = flightid });
            return Util.jsonStat("OK");
        }

        public ActionResult FlightDataVideo(int ID = 0)
        {
            //to get the url of the video
            Drones thisDrone = new Drones();
            string sql = "select VideoURL from DroneFlightVideo where videoId=" + ID;
            string videourl = Util.getDBVal(sql);
            ViewBag.PlayerURL = videourl; //thisDrone.getLiveURL(ID);
            return View();
        }
        

        public ActionResult Deleted([Bind(Prefix = "ID")]int FlightID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT.VIDEOS")) return RedirectToAction("NoAccess", "Home");
      String SQL = "";
      using (var ctx = new ExponentPortalEntities())
        if (exLogic.User.hasAccess("FLIGHT.VIDEOS"))
          SQL = "Update [DroneFlightVideo] set IsDeleted=1 WHERE VideoID = " + FlightID;
      Util.doSQL(SQL);

      return RedirectToAction("Index");

    }
    public ActionResult PlayVideo([Bind(Prefix = "ID")]int id = 0) {
      ViewBag.Title = "Flight Data";
      ViewBag.VideoID = id;
      Drones thisDrone = new Drones();
      ViewBag.AllowedLocation = thisDrone.getAllowedLocation(id);
      ViewBag.DroneID = thisDrone.DroneID;
      //ViewBag.VideoURL = thisDrone.getVideoURL(id);
      //ViewBag.PlayerURL = thisDrone.getPlayerURL(id);

      return View();

    }

    public String CheckAlert(int id = 0) {
      StringBuilder AlertMsg = new StringBuilder();
      int AccountID = Util.getDBInt("SELECT AccountID From MSTR_User WHERE UserID=" + id);
      String SQL = @"SELECT
          PortalAlert.AlertID,
          PortalAlert.AlertMessage,
          PortalAlert.AlertType
        FROM
          PortalAlert
      LEFT JOIN DroneFlight ON
          DroneFlight.ID = PortalAlert.FlightID
      LEFT JOIN MSTR_Drone ON
          MSTR_Drone.DroneID = DroneFlight.DroneID AND
          MSTR_Drone.AccountID = " + AccountID + @"
        LEFT JOIN PortalAlert_User ON
          PortalAlert_User.AlertID = PortalAlert.AlertID and
          PortalAlert_User.UserID =  " + id + @"
        WHERE
          MSTR_Drone.DroneID IS NOT NULL AND
          PortalAlert_User.UserID IS NULL AND
          PortalAlert.CreatedOn > DATEADD(minute, -30, GETDATE())";
      var Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        String AlertType = Row["AlertType"].ToString();
        if (String.IsNullOrEmpty(AlertType)) AlertType = "High";
        AlertMsg.Append("<LI");
        AlertMsg.Append(" class=\"");
        AlertMsg.Append(AlertType.ToLower());
        AlertMsg.Append("\">");

        AlertMsg.Append(Row["AlertMessage"]);
        AlertMsg.Append("</LI>");

        SQL = "INSERT INTO PortalAlert_User(UserID, AlertID) VALUES( " +
          id + ", " + Row["AlertID"] + ")";
        Util.doSQL(SQL);
      }

      return AlertMsg.ToString();
    }

    
   public ActionResult FlightSetup()
  {
            if (!exLogic.User.hasAccess("FLIGHT.SETUP")) return RedirectToAction("NoAccess", "Home");
           
            ViewData["accountid"] = Convert.ToInt32(Session["AccountID"].ToString());           
            return View();
    }
    [HttpPost]
    public ActionResult FlightSetup(FlightSetupViewModel flightsetupvm)//Models.MSTR_Drone_Setup droneSetup)
    {
            if (!exLogic.User.hasAccess("FLIGHT.SETUP")) return RedirectToAction("NoAccess", "Home");
            if (flightsetupvm.DroneSetup.DroneId < 1 ) ModelState.AddModelError("DroneId", "You must select a Drone.");
            if (flightsetupvm.DroneSetup.PilotUserId < 1 || flightsetupvm.DroneSetup.PilotUserId == null) ModelState.AddModelError("PilotUserId", "You must select a pilot.");
            if (flightsetupvm.DroneSetup.GroundStaffUserId < 1 || flightsetupvm.DroneSetup.GroundStaffUserId == null) ModelState.AddModelError("GroundStaffUserId", "A Ground staff should be selected.");
            string sqlcheck = "select DroneSetupId from MSTR_Drone_Setup where DroneId=" + flightsetupvm.DroneSetup.DroneId;
            int result = Util.getDBInt(sqlcheck);
            if (result > 0)
            {
                string sqlupdate = @"update MSTR_Drone_Setup set PilotUserId=" + flightsetupvm.DroneSetup.PilotUserId + ",GroundStaffUserId=" + flightsetupvm.DroneSetup.GroundStaffUserId +
                ",[BatteryVoltage]=" + flightsetupvm.DroneSetup.BatteryVoltage + ",[Weather]='" + flightsetupvm.DroneSetup.Weather + "',[UasPhysicalCondition]='" + flightsetupvm.DroneSetup.UasPhysicalCondition
                + "',[ModifiedBy]=" + Convert.ToInt32(Session["UserID"].ToString()) + ",[ModifiedOn]='" + System.DateTime.Now + "' where [DroneId]=" + flightsetupvm.DroneSetup.DroneId;
                int result1 = Util.doSQL(sqlupdate);

                //To update in GCA_Approval table
                string sqlgcaapprovalupdate = @"Update [GCA_Approval] set Polygon=Polygon.ReorientObject().MakeValid(),
                                              Coordinates=" + flightsetupvm.GcaApproval.Coordinates +
                                              "where Polygon.STArea()>999999999999 and ApprovalID=" + flightsetupvm.GcaApproval.ApprovalID;
                int result2 = Util.doSQL(sqlgcaapprovalupdate);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    int ID = 0;
                    //To update in GCA_Approval table
                    string sqlgcaapprovalupdate = @"Update [GCA_Approval] set Polygon=Polygon.ReorientObject().MakeValid(),
                                              Coordinates=" + flightsetupvm.GcaApproval.Coordinates +
                                              "where Polygon.STArea()>999999999999 and ApprovalID=" + flightsetupvm.GcaApproval.ApprovalID;
                    int result2 = Util.doSQL(sqlgcaapprovalupdate);

                    ExponentPortalEntities db = new ExponentPortalEntities();
                    flightsetupvm.DroneSetup.CreatedBy = Convert.ToInt32(Session["UserID"].ToString());
                    flightsetupvm.DroneSetup.CreatedOn = System.DateTime.Now;
                    db.MSTR_Drone_Setup.Add(flightsetupvm.DroneSetup);
                    db.SaveChanges();                    
                    ID = flightsetupvm.DroneSetup.DroneSetupId;
                    db.Dispose();
                }
            }
            return View();
        }

        
        [HttpGet]
    public ActionResult FillPilot(int? id,int?droneid){            

      String SQL = "SELECT * FROM MSTR_Drone_Setup where DroneId=" + droneid;
      var Row = Util.getDBRow(SQL);
      var Approvals = (
        from p in db.GCA_Approval
        where p.DroneID == droneid
        select new {
          ApprovalID = p.ApprovalID,
          ApprovalName = p.ApprovalName,
          Cordinates = p.Coordinates
        }
      ).ToList();

      Row.Add("Approvals", Approvals);
      
      return Json(Row, JsonRequestBehavior.AllowGet);
    }

        [HttpGet]
        public ActionResult FillCordinates(int? ApprovalID)
        {

              var olistCoordinates = (
              from p in db.GCA_Approval
              where p.ApprovalID == ApprovalID
              select new
              {
                  Cordinates = p.Coordinates
              }
            ).ToList();
            
            return Json(olistCoordinates, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult FlightSetupMap()
        //{
        //    return View();
        //}

    }//class
}//namespace