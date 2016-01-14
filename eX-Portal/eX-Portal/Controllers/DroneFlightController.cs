using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DroneFlightController : Controller {
    static String RootUploadDir = "~/Upload/Drone/";

    // GET: DroneFlight
    public ActionResult Index([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("FLIGHT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "UAS Flights";
      ViewBag.DroneID = DroneID;
      String SQLFilter = "";
      String SQL =
      "SELECT\n" +
      "  DroneFlight.ID,\n" +
      "   MSTR_Drone.DroneName as UAS,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate,\n" +
      "   Count(*) Over() as _TotalRecords,\n" +
      "   DroneFlight.ID as _PKey\n" +
      "FROM\n" +
      "  DroneFlight\n" +
      "LEFT JOIN MSTR_Drone ON\n" +
      "  MSTR_Drone.DroneId = DroneFlight.DroneID\n" +
      "LEFT JOIN MSTR_User as tblPilot ON\n" +
      "  tblPilot.UserID = DroneFlight.PilotID\n" +
      "LEFT JOIN MSTR_User as tblGSC ON\n" +
      "  tblGSC.UserID = DroneFlight.GSCID\n" +
      "LEFT JOIN MSTR_User as tblCreated ON\n" +
      "  tblCreated.UserID = DroneFlight.CreatedBy";

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

      if (SQLFilter != "" ) {
        SQL += "\n WHERE\n" + SQLFilter;
      }

      qView nView = new qView(SQL);
      if (exLogic.User.hasAccess("FLIGHT.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.VIEW")) nView.addMenu("Detail", Url.Action("Detail", new { ID = "_PKey" }));
      nView.addMenu("Flight Map", Url.Action("FlightData", "Map", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("FLIGHT.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }//Index()

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
      if (!exLogic.User.hasAccess("FLIGHT.VIEW")) return "Access Denied";
      int DroneId,UserId;

      string UASFormat,PilotFormat;
      String UploadedDocs = "";

      String SQL =
      "SELECT\n" +
      "   DroneFlight.ID UASFlightId,\n" +
      "   MSTR_Drone.DroneName as UAS,\n" +
      "   tblPilot.FirstName as PilotName,\n" +
      "   tblGSC.FirstName as GSCName,\n" +
      "   tblCreated.FirstName as CreatedBy,\n" +
      "   FlightDate\n" +
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
      UASFormat = "<a href='/Drone/Detail/" + DroneId + "'>$UAS</a>";//url
      PilotFormat = "<a href='/User/UserDetail/" + UserId + "'>$PilotName</a>";//url
      theView.FormatCols.Add("UAS", UASFormat); //Adding the Column required for formatting  
      theView.FormatCols.Add("PilotName", PilotFormat); // //Adding the Column required for formatting  


      //Check the documents for GCA is uploaded
      SQL = "SELECT\n" +
      "  Count(*)\n" +
      "FROM\n" +
      "  [DroneDocuments]\n" +
      "WHERE\n" +
      "  FlightID = " + ID.ToString() + " and\n" +
      "  DocumentType = 'GCA Approval'\n";
      int TheCount = Util.getDBInt(SQL);
      if(TheCount < 1 ) {
        UploadedDocs = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "Please upload your GCA Authorisation document before the flight</div>";
      } else {
        UploadedDocs = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "Your GAC Authorization: " + getUploadedDocs(ID) +
        "</div>";
      }
      


      return UploadedDocs + theView.getTable();

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
      "   FlightDate\n" +
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

      qView nView = new qView(SQL);
      if(nView.HasRows) { 
        nView.isFilterByTop = false;
        return 
          "<h2>Recent Flights</h2>\n"+
          nView.getDataTable(true, false);
      }

      return "";
      
    }

    public String DeleteFile(String file, int FlightID=0) {

      //now add the uploaded file to the database
      String SQL = "DELETE FROM DroneDocuments\n" +
        "WHERE\n" +
        "  DocumentName='" + file + "' AND\n" +
        "  FlightID = '" + FlightID + "'";
      Util.doSQL(SQL);

      StringBuilder JsonText = new StringBuilder();
      JsonText.Append("{");
      JsonText.Append(Util.Pair("status", "success", true));
      JsonText.Append("}");

      return JsonText.ToString();
    }

    public String UploadFile(int FlightID = 0, int DroneID = 0) {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
        String DroneName = DroneID == 0 ? "0" : Util.getDroneName(DroneID);
        String UploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
        String FileURL = FileName;
        String FullName = UploadDir + FileName;

        if (!Directory.Exists(UploadDir)) Directory.CreateDirectory(UploadDir);
        TheFile.SaveAs(FullName);
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName, FileURL));
        JsonText.Append("]}");

        //now add the uploaded file to the database
        String SQL = "INSERT INTO DroneDocuments(\n" +
          " DroneID, FlightID, DocumentType, DocumentName, UploadedDate, UploadedBy\n" +
          ") VALUES (\n" +
          "  '" + DroneID + "',\n" +
          "  '" + FlightID + "',\n" +
          "  'GCA Approval',\n" +
          "  '" + FileURL + "',\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + "\n" +
          ")";
        Util.doSQL(SQL);

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
      String OldUploadDir = UploadPath +  "0\\0\\";
      String DroneName = DroneID == 0 ? "0" : Util.getDroneName(DroneID);
      String NewUploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
      foreach (String file in Files) {
        if (String.IsNullOrEmpty(file)) continue;
        String OldFullPath = OldUploadDir + file;
        String NewFullPath = NewUploadDir + file;
        if(System.IO.File.Exists(OldFullPath)) {
          if (!Directory.Exists(NewFullPath)) Directory.CreateDirectory(NewUploadDir);
          if(!System.IO.File.Exists(NewFullPath)) System.IO.File.Move(OldFullPath, NewFullPath);          
          String SQL = "UPDATE DroneDocuments SET\n" +
          "  DroneID=" + DroneID + ",\n" +
          "  FlightID=" + FlightID + "\n" +
          "WHERE\n" +
          "  DocumentName='" + file + "'";
          Util.doSQL(SQL);
        }//if(System.IO.File.Exists
      }//foreach (String file in Files)
    }//MoveUploadFileTo


    public ActionResult UASFiles(int FlightID=0) {
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<DroneDocument> Docs = (from r in db.DroneDocuments
                                 where (int)r.FlightID == FlightID
                                 select r).ToList();
      return View(Docs);
    }

  }//class
}//namespace