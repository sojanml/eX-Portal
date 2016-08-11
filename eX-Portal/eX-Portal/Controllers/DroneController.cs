using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;
using System.Text;
using System.IO;
using System.Globalization;

namespace eX_Portal.Controllers {
  public class DroneController :Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();
    static String RootUploadDir = "~/Upload/Drone/";

    public ActionResult Live() {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Live UAS";
      return View();
    }
    public String LiveData() {
      if(!exLogic.User.hasAccess("DRONE"))
        return "{}";
      StringBuilder Json = new StringBuilder();
      var UAS = new Drones();

      Response.ContentType = "text/json";
      Json.Append("[");
      Json.Append(UAS.getLiveUAS());
      Json.Append("]");
      return Json.ToString();
    }

    // GET: Drone
    public ActionResult Index() {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "UAS Listing";

      String SQL = "SELECT \n" +
          "  D.[DroneName] as UAS,\n" +
          "  D.[ModelName] as Description,\n" +
          "  D.[CommissionDate],\n" +
          "  O.Name as OwnerName,\n" +
          "  M.Name as Manufacture,\n" +
          "  U.Name as UASType,\n" +
          "  Count(*) Over() as _TotalRecords,\n" +
          "  D.[DroneId] as _PKey\n" +
          "FROM\n" +
          "  [MSTR_Drone] D\n" +
          "Left join MSTR_Account  O on\n" +
          "  D.AccountID = O.AccountID " +
          "Left join LUP_Drone M on\n" +
          "  ManufactureID = M.TypeID and\n" +
          "  M.Type='Manufacturer' " +
          "Left join LUP_Drone U on\n" +
          "  UAVTypeID = U.TypeID and\n" +
          "  U.Type= 'UAVType'\n";
         if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
         {
                    SQL +=
                      "WHERE\n" +
                      "  D.AccountID=" + Util.getAccountID();
         }
            
      qView nView = new qView(SQL);
      if(exLogic.User.hasAccess("DRONE"))
        nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("DRONE.EDIT")) {
        nView.addMenu("Edit", Url.Action("Edit", new { ID = "_Pkey" }));
        nView.addMenu("Authority Approval", Url.Action("AuthorityApproval", new { ID = "_Pkey" }));
      }
      if(exLogic.User.hasAccess("FLIGHT.CREATE"))
        nView.addMenu("Create Flight", Url.Action("Create", "DroneFlight", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("FLIGHT"))
        nView.addMenu("Flights", Url.Action("Index", "DroneFlight", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("DRONE.MANAGE"))
        nView.addMenu("Manage", Url.Action("Manage", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("BLACKBOX"))
        nView.addMenu("FDR Data", Url.Action("Index", "BlackBox", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("DRONE.DELETE"))
        nView.addMenu("Delete", Url.Action("Delete", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("FLIGHT.CREATE"))
        nView.addMenu("Zone Approval", Url.Action("Index", "Approval", new { ID = "_Pkey" }));
      if(exLogic.User.hasAccess("FLIGHT.GEOTAG"))
        nView.addMenu("Geo-Tagging", Url.Action("GeoTag", "Drone", new { ID = "_Pkey" }));

      if(Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }



       
        
        public ActionResult AuthorityApproval([Bind(Prefix = "ID")] int DroneID = 0) {
      if(!exLogic.User.hasAccess("DRONE.AUTHORITY_DOCUMENT"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneID = DroneID;
      return View();
    }

    [ChildActionOnly]
    public ActionResult AuthorityDocuments(int DroneID = 0, String Authority="DCAA") {
      ViewBag.DroneID = DroneID;
      ViewBag.Authority = Authority;


      List<DroneDocument> Docs = (
        from o in ctx.DroneDocuments
        where 
        o.DocumentType == "UAS-Registration" &&
        o.DroneID == DroneID &&
        o.DocumentTitle == Authority
        select o).ToList();

      return View(Docs);
    }


    public ActionResult Manage([Bind(Prefix = "ID")] int DroneID = 0) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Manage - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }

    public ActionResult Decommission([Bind(Prefix = "ID")] int DroneID) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Decommission - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }//Decommission()

    [HttpPost]
    public ActionResult Decommission([Bind(Prefix = "ID")] int DroneID, String DecommissionNote) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");
      String SQL = "UPDATE MSTR_DRONE SET\n" +
        "  DecommissionNote='" + DecommissionNote + "',\n" +
        "  DecommissionDate = GETDATE(), \n" +
        "  DecommissionBy = " + Util.getLoginUserID() + ",\n" +
        "  IsActive = 0\n" +
        "WHERE\n" +
        "  DroneID=" + DroneID;
      Util.doSQL(SQL);
      return RedirectToAction("Detail", new { ID = DroneID });
    }//Decommission()
     //uploading documents for commission,UAT,incident
    public ActionResult UploadDocument([Bind(Prefix = "ID")] int DroneID, String Type) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");

      switch(Type.ToLower()) {
        case "commission":
          ViewBag.DocumentType = Type;
          break;
        case "uat":
          ViewBag.DocumentType = Type;
          break;
        case "incident":
          ViewBag.DocumentType = Type;
          break;
        default:
          ViewBag.DocumentType = "Commission";
          break;
      }
            ViewBag.DroneID = DroneID;
      ViewBag.Title = ViewBag.DocumentType + " Report - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }//Decommission()

    //Saving notes to mstr_drone database for uat,commission,incident

    [HttpPost]
    public ActionResult UploadDocument([Bind(Prefix = "ID")] int DroneID, String Type, String Note) {
      String SQL = "";
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");

      switch(Type.ToLower()) {
        case "commission":
          SQL = "UPDATE MSTR_DRONE SET\n" +
            "  CommissionReportNote='" + Note + "'\n" +
            "WHERE\n" +
            "  DroneID=" + DroneID;
          ViewBag.DocumentType = Type;
          break;
        case "uat":
          SQL = "UPDATE MSTR_DRONE SET\n" +
          "  UATReportNote='" + Note + "'\n" +
          "WHERE\n" +
          "  DroneID=" + DroneID;
          ViewBag.DocumentType = Type;
          break;
        case "incident":
          SQL = "UPDATE MSTR_DRONE SET\n" +
          " IncidentReportNote='" + Note + "'\n" +
          "WHERE\n" +
          "  DroneID=" + DroneID;
          ViewBag.DocumentType = Type;
          break;
        default:
          ViewBag.DocumentType = "Commission";
          break;
      }
      ViewBag.Title = ViewBag.DocumentType + " Report - " + Util.getDroneName(DroneID);
      Util.doSQL(SQL);
      return RedirectToAction("Detail", new { ID = DroneID });
    }//Decommission()

    //Geo Tagging for Drone
    public ActionResult GeoTag([Bind(Prefix = "ID")] int DroneID = 0) {
      if(!exLogic.User.hasAccess("FLIGHT.GEOTAG"))
        return RedirectToAction("NoAccess", "Home");
      ExponentPortalEntities Db = new ExponentPortalEntities();
      ViewBag.DroneID = DroneID;
      List<DroneDocument> Docs = (from o in Db.DroneDocuments
                                  where o.DocumentType == "GEO Tag" &&
                                  o.DroneID == DroneID
                                  select o).ToList();
      ViewBag.FirstRow = true;


      return View(Docs);

    }//GeoTag


    [HttpPost]
    public String UploadGeoFile([Bind(Prefix = "ID")]  int DroneID = 0, String DocumentType = "Regulator Approval", String CreatedOn = "") {
      int FlightID = 0;
      DateTime FileCreatedOn = DateTime.MinValue;
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      GPSInfo GPS = new GPSInfo();
      Response.ContentType = "text/json";

      try {
        FileCreatedOn = DateTime.ParseExact(CreatedOn, "ddd, d MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture);

        FlightID = Util.getFlightID(DroneID, FileCreatedOn);

      } catch { }

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName.ToLower();
        String DroneName = Util.getDroneNameByDroneID(DroneID);
        String UploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
        String FileURL = FileName;
        String FullName = UploadDir + FileName;
        String GPSFixName = UploadDir + "GPS-" + FileName;

        //Save the file to Disk
        if(!Directory.Exists(UploadDir)) {
          Directory.CreateDirectory(UploadDir);
        }
        TheFile.SaveAs(FullName);

        //Do the calculation for GPS
        if(DocumentType == "Geo Tag" && System.IO.Path.GetExtension(FullName).ToLower() == ".jpg") {
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
        JsonText.Append(",");
        JsonText.Append("\"FlightID\":");
        JsonText.Append(FlightID);
                JsonText.Append(",");
                //JsonText.Append("\"CDATE\":");
        JsonText.Append(Util.Pair("CreatedDate",  FileCreatedOn.ToString("dd-MMM-yyyy hh:mm"),false ));
        JsonText.Append("},");
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName, FileURL));
        JsonText.Append("]}");

        //now add the uploaded file to the database
        String SQL = "INSERT INTO DroneDocuments(\n" +
          " DroneID, FlightID, DocumentType,DocumentName,DocumentDate,UploadedDate, UploadedBy,\n" +
          " Latitude, Longitude, Altitude \n" +
          ") VALUES (\n" +
          "  '" + DroneID + "',\n" +
          "  '" + FlightID + "',\n" +
          "  '" + DocumentType + "',\n" +
          "  '" + FileURL + "',\n" +
          "  GETDATE(),\n" +
          " '" + FileCreatedOn + "'," +
          "  " + Util.getLoginUserID() + ",\n" +
          "  " + GPS.Latitude + ", " + GPS.Longitude + ", " + GPS.Altitude + "\n" +
          ")";
        int DocumentID = Util.InsertSQL(SQL);

      } catch(Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//UploadGeoFile()



    public String DeleteFile([Bind(Prefix = "ID")] int DroneID, String file) {
      bool isDeleted = false;
      StringBuilder JsonText = new StringBuilder();
      JsonText.Append("{");

      if(!exLogic.User.hasAccess("DRONE.AUTHORITY_DOCUMENT")) {
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", "You do not have access to UAS Documents", false));        
      } else { 
        String SQL = "SELECT Count(*) FROM DroneDocuments\n" +
          "WHERE\n" +
          "  DocumentName='" + file + "' AND\n" +
          "  DroneID = '" + DroneID + "'";
        int DocCount = Util.getDBInt(SQL);
        if(DocCount > 0) {
          String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
          String FullPath = Path.Combine(UploadPath, file);

                    using (System.IO.File.Create(FullPath)) { }
                    
                    if (System.IO.File.Exists(FullPath)) {
            System.IO.File.Delete(FullPath);
            //now add the uploaded file to the database
            SQL = "DELETE FROM DroneDocuments\n" +
            "WHERE\n" +
            "  DocumentName='" + file + "' AND\n" +
            "  DroneID = '" + DroneID + "'";
            Util.doSQL(SQL);
            isDeleted = true;
            JsonText.Append(Util.Pair("status", "success", true));
            JsonText.Append(Util.Pair("message", "Deleted successfully.", false));
          }
        }
      }
      if(!isDeleted) { 
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", "Can not delete the file from server.", false));
      }
      JsonText.Append("}");

      return JsonText.ToString();
    }


    private void MoveUploadFileTo(int DroneID, int FlightID) {
      String[] Files = Request["FileName"].Split(',');
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      String OldUploadDir = UploadPath + "0\\0\\";
      String DroneName = DroneID == 0 ? "0" : Util.getDroneName(DroneID);
      String NewUploadDir = UploadPath + DroneName + "\\" + FlightID + "\\";
      foreach(String file in Files) {
        if(String.IsNullOrEmpty(file))
          continue;
        String OldFullPath = OldUploadDir + file;
        String NewFullPath = NewUploadDir + file;
        if(System.IO.File.Exists(OldFullPath)) {
          if(!Directory.Exists(NewFullPath))
            Directory.CreateDirectory(NewUploadDir);
          if(!System.IO.File.Exists(NewFullPath))
            System.IO.File.Move(OldFullPath, NewFullPath);
          String SQL = "UPDATE DroneDocuments SET\n" +
          "  DroneID=" + DroneID + ",\n" +
          "  FlightID=" + FlightID + "\n" +
          "WHERE\n" +
          "  DocumentName='" + file + "'";
          Util.doSQL(SQL);
        }//if(System.IO.File.Exists
      }//foreach (String file in Files)
    }//MoveUploadFileT

    public String UploadFile(int DroneID, String DocumentType, String DocumentTitle="", String DocumentDesc="") {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String DroneName = Util.getDroneName(DroneID);
        DroneName = DroneName.Replace("*", "");
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
        String UploadDir = UploadPath + DroneName + "\\" + DocumentType + "\\";
        String FileURL = DroneName + "/" + DocumentType + "/" + FileName;
        String FullName = UploadDir + FileName;

        if(!Directory.Exists(UploadDir))
          Directory.CreateDirectory(UploadDir);
        TheFile.SaveAs(FullName);
               
                JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append(Util.Pair("DocumentTitle", Util.toSQL(DocumentTitle), true));
        JsonText.Append(Util.Pair("DocumentDesc", Util.toSQL(DocumentDesc), true));
        JsonText.Append(Util.Pair("FileURL", Util.toSQL(FileURL), true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName));
        JsonText.Append("]}");


        //now add the uploaded file to the database
        String SQL = "INSERT INTO DroneDocuments(\n" +
          " DroneID, DocumentType, DocumentName,\n" +
          " DocumentTitle, DocumentDesc,\n" +
          " UploadedDate, UploadedBy\n" +
          ") VALUES (\n" +
          "  '" + DroneID + "',\n" +
          "  '" + DocumentType + "',\n" +
          "  '" + FileURL + "',\n" +
          "  '" + Util.toSQL(DocumentTitle) + "',\n" +
          "  '" + Util.toSQL(DocumentDesc) + "',\n" +
          "  GETDATE(),\n" +
          "  " + Util.getLoginUserID() + "\n" +
          ")";
        Util.doSQL(SQL);

      } catch(Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//Save()


    // GET: Drone/Details/5
    public ActionResult Detail([Bind(Prefix = "ID")] int DroneID) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      if(!exLogic.User.hasDrone(DroneID))
        return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = Util.getDroneName(DroneID);
      ViewBag.DroneID = DroneID;

      return View();
    }

    //Partial view for Details of file uploaded for commission,decommission,uat,incident etc.
    public ActionResult FileDetail(int ID, String DocumentType) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneID = ID;
      ViewBag.DocumentType = DocumentType;
      var FileList = Listing.getFileNames(ID, DocumentType);
      return PartialView(FileList);
    }//FileDetail()

    public ActionResult DroneParts(int ID = 0) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      List<String> Parts = new List<String>();
      Parts = Listing.DroneListing(ID);
      return PartialView(Parts);
    }

    public ActionResult getDroneParts(int ID = 0) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      var Parts = Listing.getParts(ID);
      return PartialView(Parts);
    }

    // GET: Drone/Details/5
    public String DroneDetail([Bind(Prefix = "ID")]  int DroneID) {
      string OwnerFormat;
      int OwnerId;
      if(!exLogic.User.hasAccess("DRONE"))
        return "Access Denied";
      String SQL = "SELECT \n" +
          "  D.[DroneName] as UAS,\n" +
          "  Convert(varchar(12), D.[CommissionDate], 6) As [Date],\n" +
          "  D.[DroneSerialNo] as [UAS S.no],\n" +
          "  O.Name as OwnerName,\n" +
          "  M.Name as ManufactureName,\n" +
          "  U.Name as UASType,\n" +
          "  D.[DroneIdHexa] as UASHexaId,\n" +
          "  D.[ModelName] as Description,\n" +
          "  RegistrationAuthority as RegistrationAuthority\n" +
          "FROM\n" +
          "  [MSTR_Drone] D\n" +
          "Left join MSTR_Account  O on\n" +
          "  D.AccountID = O.AccountID\n" +
          "Left join LUP_Drone M on\n" +
          "  ManufactureID = M.TypeID and\n" +
          "  M.Type='Manufacturer' " +
          "Left join LUP_Drone U on\n" +
          "  UAVTypeID = U.TypeID and\n" +
          "  U.Type= 'UAVType'\n" +
          "WHERE\n" +
          "  D.[DroneId]=" + DroneID;
            if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
            {
                    SQL += " AND\n" +
                      "  D.AccountID=" + Util.getAccountID();
            }

      qDetailView nView = new qDetailView(SQL);
      //this part for adding link to requred fields in the details
      OwnerId = Util.GetAccountIDFromDrone(DroneID);
      OwnerFormat = "<a  href='/Admin/AccountDetail/" + OwnerId + "'>$OwnerName$</a>";//url

      nView.FormatCols.Add("OwnerName", OwnerFormat); //Adding the Column required for formatting  
      return
        ReassignDetail(DroneID) +
        DecommissionDetail(DroneID) +
        nView.getTable() +
        Util.getDroneRegistrationDocuments(DroneID);
      //+
    }



    public String ReassignDetail(int DroneID) {
      if(!exLogic.User.hasAccess("DRONE"))
        return "Access Denied";
      StringBuilder Detail = new StringBuilder();

      string SQL = "select\n" +
          "  ISNULL(a.isactive, 'True') as IsActive,\n" +
          "  a.DroneId as UASSno,\n" +
          "  a.ReAssignNote,\n" +
          "  a.DroneName as UAS,\n" +
          "  Convert(varchar, a.ReAssignDate, 9) as ReAssignDate,\n" +
          "  c.UserName as ReAssignedBy \n" +
          "from \n" +
          "  MSTR_Drone a \n" +
          "left join MSTR_Drone\n" +
          "  b on a.DroneId = b.ReAssignFrom\n" +
          "left join mstr_user c \n" +
          "  on a.ReAssignBy = c.UserId\n" +
          "where\n" +
          "  b.DroneId =" + DroneID;


      var Row = Util.getDBRow(SQL);
      if((bool)Row["hasRows"] && Row["IsActive"].ToString() != "True") {
        Detail.AppendLine("<div class=\"decommission-info\">");
        Detail.AppendLine("ReAssigned For");
        Detail.AppendLine("<span>" + Row["UAS"] + "</span>");
        Detail.AppendLine("on");
        Detail.AppendLine("<span>" + Row["ReAssignDate"] + "</span>");
        Detail.AppendLine("by");
        Detail.AppendLine("<span>" + Row["ReAssignedBy"] + "</span>");

        Detail.AppendLine("<div>" + Row["ReAssignNote"] + "</div>");
        Detail.AppendLine("</div>");
      }
      return Detail.ToString();
    } //Decommission()
    public String DecommissionDetail(int DroneID) {
      if(!exLogic.User.hasAccess("DRONE"))
        return "Access Denied";
      StringBuilder Detail = new StringBuilder();
      String SQL = "SELECT\n" +
         "  ISNULL(Mstr_drone.isactive ,'True') as isactive,\n" +
         "  Convert(varchar, [DecommissionDate], 9) as DecommissionDate,\n" +
         "  DecommissionNote,\n" +
         "  MSTR_User.FirstName as DecommissionBy\n" +
         "from\n" +
         "  Mstr_drone\n" +
         "LEFT JOIN MSTR_User ON\n" +
         "  MSTR_User.UserId = Mstr_drone.DecommissionBy\n" +
         "where\n" +
         "  Mstr_drone.DroneID=" + DroneID;
      var Row = Util.getDBRow(SQL);
      if((bool)Row["hasRows"] && Row["isactive"].ToString() != "True") {
        Detail.AppendLine("<div class=\"decommission-info\">");
        Detail.AppendLine("Decommissioned on");
        Detail.AppendLine("<span>" + Row["DecommissionDate"] + "</span>");
        Detail.AppendLine("by");
        Detail.AppendLine("<span>" + Row["DecommissionBy"] + "</span>");
        Detail.AppendLine("<div>" + Row["DecommissionNote"] + "</div>");
        Detail.AppendLine("</div>");
      }
      return Detail.ToString();
    } //Decommission()

    // GET: Drone/Create
    public ActionResult Create() {
      if(!exLogic.User.hasAccess("DRONE.CREATE"))
        return RedirectToAction("NoAccess", "Home");
      String OwnerListSQL = 
        "SELECT Name + ' [' + Code + ']', AccountId FROM MSTR_Account";
                if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                OwnerListSQL += " WHERE AccountId=" + Util.getAccountID();           
      OwnerListSQL +=" ORDER BY Name";
      var viewModel = new ViewModel.DroneView {
        Drone = new MSTR_Drone(),
        OwnerList = Util.getListSQL(OwnerListSQL),
        UAVTypeList = Util.GetDropDowntList("UAVType", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };
            //deleting if unwanted files exist in drone documents;
            string SQL = " Delete from droneDocuments where droneid=0 and documenttype='Drone Image'";
            Util.doSQL(SQL);

            return View(viewModel);
    }

    [ChildActionOnly]
    public String UAVTypeList(int UavTypeId = 0) {
      StringBuilder TypeList = new StringBuilder();
      String LastGroupName = String.Empty;
      String SQL = 
      @"select DISTINCT
        LUP_Drone.TypeID,
        LUP_Drone.Name,
        LUP_Drone.GroupName
      from
       LUP_Drone 
      where 
        LUP_Drone.[Type]='UAVType'
      ORDER BY
        GroupName,
        Name";
      TypeList.AppendLine(@"<select data-val=""true"" data-val-number=""The field UavTypeId must be a number."" id=""Drone_UavTypeId"" name=""Drone.UavTypeId"" class=""valid"">");
      using(var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = SQL;
        using(var reader = cmd.ExecuteReader()) {
          while(reader.Read()) {
            String GroupName = reader["GroupName"].ToString();
            String Name = reader["Name"].ToString();
            int ID = reader.GetInt32(reader.GetOrdinal("TypeID"));
            
            if(GroupName != LastGroupName) {
              if(!String.IsNullOrEmpty(LastGroupName))
                TypeList.AppendLine("</optgroup>");
              TypeList.AppendLine("<optgroup label=\"" + GroupName  + "\">");
            }
            TypeList.Append("<option value=\"");
            TypeList.Append(ID);
            TypeList.Append("\"");
            if(ID == UavTypeId) TypeList.Append(" selected");
            TypeList.Append(">" + Name);
            TypeList.AppendLine("</option>");

            LastGroupName = GroupName;
          }//while
        }//using reader
      }//using ctx.Database.Connection.CreateCommand
      TypeList.AppendLine("</select>");
      return TypeList.ToString();
    }


    // POST: Drone/Create
    [HttpPost]
    public ActionResult Create(ViewModel.DroneView DroneView) {
      if(!exLogic.User.hasAccess("DRONE.CREATE")) return RedirectToAction("NoAccess", "Home");      
      try {
                // TODO: Add insert logic here   

                //insert into LUP_Drone table -- to insert the manufacturer then to use it for inserting to the other table       
                if (DroneView.Name != null)
                {
                    int typeid = Util.getDBInt("SELECT Max(TypeId) + 1 from [LUP_Drone] where [Type]='Manufacturer'");
                    string BinaryCode = Util.DecToBin(typeid);
                    string s = DroneView.Name.ToString();
                    string code = s.Substring(0, 3);
                    string SQL1 = "INSERT INTO LUP_DRONE(\n" +
                    "  Type,\n" +
                    "  Code,\n" +
                    "  TypeId,\n" +
                    "  BinaryCode,\n" +
                    "  Name,\n" +
                    "  CreatedBy,\n" +
                    "  CreatedOn,\n" +
                    "  IsActive\n" +
                    ") VALUES(\n" +
                    "  'Manufacturer',\n" +
                    "  '" + code + "',\n" +
                    "  " + typeid + ",\n" +
                    "  '" + BinaryCode + "',\n" +
                    "  '" + DroneView.Name + "',\n" +
                    "  " + Util.getLoginUserID() + ",\n" +
                    "  GETDATE(),\n" +
                    "  'True'" +
                    ")";
                    int manufacturerid = Util.InsertSQL(SQL1);
                    int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) + 1 FROM MSTR_DRONE");
                    if (DroneSerialNo < 1001)
                        DroneSerialNo = 1001;
                    MSTR_Drone Drone = DroneView.Drone;
                    if (Drone.CommissionDate == null) Drone.CommissionDate = DateTime.Now;
                    if (Drone.ModelName == null) Drone.ModelName = "Other";
                    Drone.RegistrationDocument = String.IsNullOrEmpty(Request["FileName"]) ? "" : Request["FileName"];
                    String SQL = "INSERT INTO MSTR_DRONE(\n" +
                                 "  AccountID,\n" +
                                 "  MANUFACTUREID,\n" +
                                 "  UAVTYPEID,\n" +
                                 "  COMMISSIONDATE,\n" +
                                 "  DRONEDEFINITIONID,\n" +
                                 "  ISACTIVE,\n" +
                                 "  DroneSerialNo,\n" +
                                 "  ModelName,\n" +
                                 "  RegistrationDocument,\n" +
                                 "  RegistrationAuthority,\n" +
                                 "  CreatedBy,\n" +
                                 "  CreatedOn\n" +
                                 ") VALUES(\n" +
                                 "  '" + Drone.AccountID + "',\n" +
                                 "  '" + manufacturerid + "',\n" +
                                 "  '" + Drone.UavTypeId + "',\n" +
                                 "  '" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
                                 "  11,\n" +
                                 "  'True',\n" +
                                 "  " + DroneSerialNo + ",\n" +
                                 "  '" + Drone.ModelName + "',\n" +
                                 "  '" + Drone.RegistrationDocument + "',\n" +
                                 "  '" + Drone.RegistrationAuthority + "',\n" +
                                 "  '" + Util.getLoginUserID() + "',\n" +
                                 "  GETDATE()\n" +
                                 ");";
                    int DroneId = Util.InsertSQL(SQL);
                    MoveDroneUploadFileTo(DroneId);
                    if (DroneView.SelectItemsForParts != null)
                    {
                        for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++)
                        {
                            string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
                            int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
                            SQL = "Insert into M2M_DroneParts (\n" +
                                    "  DroneId,\n" +
                                    "  PartsId,\n" +
                                    "  Quantity\n" +
                                    ") values(\n" +
                                    "  " + DroneId + ",\n" +
                                    "  " + PartsId + ",\n" +
                                    "  " + Qty + "\n" +
                                    ");";
                            int ID = Util.doSQL(SQL);
                        }

                    }

                        if (exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("Manage", new { ID = DroneId });
                        else
                            return RedirectToAction("Detail", new { ID = DroneId });
                  
                }
                else
                {
                    int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) + 1 FROM MSTR_DRONE");
                    if (DroneSerialNo < 1001)
                        DroneSerialNo = 1001;
                    MSTR_Drone Drone = DroneView.Drone;
                    if (Drone.CommissionDate == null) Drone.CommissionDate = DateTime.Now;
                    if (Drone.ModelName == null) Drone.ModelName = "Other";
                    Drone.RegistrationDocument = String.IsNullOrEmpty(Request["FileName"]) ? "" : Request["FileName"];
                    String SQL = "INSERT INTO MSTR_DRONE(\n" +
                                 "  AccountID,\n" +
                                 "  MANUFACTUREID,\n" +
                                 "  UAVTYPEID,\n" +
                                 "  COMMISSIONDATE,\n" +
                                 "  DRONEDEFINITIONID,\n" +
                                 "  ISACTIVE,\n" +
                                 "  DroneSerialNo,\n" +
                                 "  ModelName,\n" +
                                 "  RegistrationDocument,\n" +
                                 "  RegistrationAuthority,\n" +
                                 "  CreatedBy,\n" +
                                 "  CreatedOn\n" +
                                 ") VALUES(\n" +
                                 "  '" + Drone.AccountID + "',\n" +
                                 "  '" + Drone.ManufactureId + "',\n" +
                                 "  '" + Drone.UavTypeId + "',\n" +
                                 "  '" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
                                 "  11,\n" +
                                 "  'True',\n" +
                                 "  " + DroneSerialNo + ",\n" +
                                 "  '" + Drone.ModelName + "',\n" +
                                 "  '" + Drone.RegistrationDocument + "',\n" +
                                 "  '" + Drone.RegistrationAuthority + "',\n" +
                                 "  '" + Util.getLoginUserID() + "',\n" +
                                 "  GETDATE()\n" +
                                 ");";                    
                    int DroneId = Util.InsertSQL(SQL);
                    MoveDroneUploadFileTo(DroneId);
                    if (DroneView.SelectItemsForParts != null)
                    {
                        for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++)
                        {
                            string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
                            int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
                            SQL = "Insert into M2M_DroneParts (\n" +
                                    "  DroneId,\n" +
                                    "  PartsId,\n" +
                                    "  Quantity\n" +
                                    ") values(\n" +
                                    "  " + DroneId + ",\n" +
                                    "  " + PartsId + ",\n" +
                                    "  " + Qty + "\n" +
                                    ");";
                            int ID = Util.doSQL(SQL);
                        }

                    }
                    if (exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("Manage", new { ID = DroneId });
                    else
                        return RedirectToAction("Detail", new { ID = DroneId });
                }
        } catch(Exception ex)
        {
            Util.ErrorHandler(ex);
            return View("InternalError", ex);
        }
    }






    public ActionResult ReAssign(int id) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneId = id;
      ExponentPortalEntities db = new ExponentPortalEntities();
      String OwnerListSQL = "SELECT Name + ' [' + Code + ']', AccountId FROM MSTR_Account ORDER BY Name";
      var viewModel = new ViewModel.DroneView {
        Drone = db.MSTR_Drone.Find(id),
        OwnerList = Util.getListSQL(OwnerListSQL),
        UAVTypeList = Util.GetDropDowntList("UAVType", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };
      return View(viewModel);
    }
    [HttpPost]
    public ActionResult ReAssign(ViewModel.DroneView DroneView) {
      if(!exLogic.User.hasAccess("DRONE.MANAGE"))
        return RedirectToAction("NoAccess", "Home");
      try {
        int DroneId = 0;
        // TODO: Add update logic here
        if(ModelState.IsValid) {


          MSTR_Drone Drone = DroneView.Drone;
          //Inserting the Reassigned Drone
          int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) + 1 FROM MSTR_DRONE");
          if(DroneSerialNo < 1001)
            DroneSerialNo = 1001;

          int OldDroneId = Drone.DroneId;
          String SQL = "INSERT INTO MSTR_DRONE(\n" +
          "  AccountID,\n" +
          "  MANUFACTUREID,\n" +
          "  UAVTYPEID,\n" +
          "  COMMISSIONDATE,\n" +
          "  DRONEDEFINITIONID,\n" +
          "  ISACTIVE,\n" +
          "  DroneSerialNo,\n" +
          "  ReAssignFrom\n" +
          ") VALUES(\n" +
          "  '" + Drone.AccountID + "',\n" +
          "  '" + Drone.ManufactureId + "',\n" +
          "  '" + Drone.UavTypeId + "',\n" +
          "  '" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
          "  11,\n" +
          "  'True',\n" +
          "  " + DroneSerialNo +
          " ," + Drone.DroneId +
          ");";
          DroneId = Util.InsertSQL(SQL);

          SQL = "update MSTR_Drone set \n" +
          "  ReAssignBy =" + Util.getLoginUserID() + ",\n" +
          "  ReAssignDate = GETDATE(),\n" +
          "  ReAssignNote ='" + Drone.ReAssignNote + "',\n" +
          "  ReAssignTo =" + DroneId + ",\n" +
          "  IsActive = 'false'\n" +
          "where \n" +
          "  DroneId =" + OldDroneId;
          int Id = Util.doSQL(SQL);
          /* SQL = "UPDATE MSTR_DRONE SET\n" +
            "   AccountID ='" + Drone.AccountID + "'," +
            "  MANUFACTUREID ='" + Drone.ManufactureId + "',\n" +
            "  UAVTYPEID ='" + Drone.UavTypeId + "',\n" +
            "  COMMISSIONDATE ='" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "'\n" +
            "WHERE\n" +
            "  DroneId =" + Drone.DroneId;
            int DroneId = Util.doSQL(SQL);*/

          //Parts Inserting to New Drone

          if(DroneView.SelectItemsForParts != null) {
            for(var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
              string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
              int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
              SQL = "Insert into M2M_DroneParts (\n" +
            "  DroneId,\n" +
            "  PartsId,\n" +
            "  Quantity\n" +
            ") values(\n" +
            "  " + DroneId + ",\n" +
            "  " + PartsId + ",\n" +
            "  " + Qty + "\n" +
            ");";
              int ID = Util.doSQL(SQL);
            }

          }
        }
        return RedirectToAction("Detail", new { ID = DroneId });

      } catch(Exception ex) {
        return View("InternalError", ex);
      }
    }

    // GET: Drone/Edit/5
    public ActionResult Edit(int id) {
      if(!exLogic.User.hasAccess("DRONE.EDIT"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneId = id;
      ExponentPortalEntities db = new ExponentPortalEntities();
      String OwnerListSQL = "SELECT Name + ' [' + Code + ']', AccountId FROM MSTR_Account ORDER BY Name";
      var viewModel = new ViewModel.DroneView {
        Drone = db.MSTR_Drone.Find(id),
        OwnerList = Util.getListSQL(OwnerListSQL),
        UAVTypeList = Util.GetDropDowntList("UAVType", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };
      return View(viewModel);
    }

    // POST: Drone/Edit/5
    [HttpPost]
    public ActionResult Edit(ViewModel.DroneView DroneView) {
      if(!exLogic.User.hasAccess("DRONE.EDIT"))
        return RedirectToAction("NoAccess", "Home");
      ModelState.Remove("Drone.RpasSerialNo");
      ModelState.Remove("Drone.RefName");
      ModelState.Remove("Drone.MakeID");
      ModelState.Remove("Drone.ModelID");
      try {
        // TODO: Add update logic here
        if(ModelState.IsValid) {
          //int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) FROM MSTR_DRONE");
          //if (DroneSerialNo < 1000) DroneSerialNo = 1001;

          MSTR_Drone Drone = DroneView.Drone;
          //master updating
          Drone.RegistrationDocument = String.IsNullOrEmpty(Request["FileName"]) ? "" : Request["FileName"];

          string SQL = "UPDATE MSTR_DRONE SET\n" +
          "   AccountID ='" + Drone.AccountID + "'," +
          "  MANUFACTUREID ='" + Drone.ManufactureId + "',\n" +
          "  UAVTYPEID ='" + Drone.UavTypeId + "',\n" +
          "  COMMISSIONDATE ='" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
           "  MODELNAME ='" + Drone.ModelName + "',\n" +
          "  RegistrationDocument ='" + Drone.RegistrationDocument + "',\n" +
          "  RegistrationAuthority ='" + Drone.RegistrationAuthority + "'\n" +
          "WHERE\n" +
          "  DroneId =" + Drone.DroneId;
          int DroneId = Util.doSQL(SQL);

          //Parts updating

          SQL = "delete from M2M_DroneParts where DroneId=" + Drone.DroneId;
          int Id = Util.doSQL(SQL);
          if(DroneView.SelectItemsForParts != null) {
            for(var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
              string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
              int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
              SQL = "Insert into M2M_DroneParts (DroneId,PartsId,Quantity) values(" + Drone.DroneId + "," + PartsId + "," + Qty + ");";
              int ID = Util.doSQL(SQL);
            }

          }
                   
                        if (exLogic.User.hasAccess("DRONE.MANAGE"))
                        return RedirectToAction("Manage", new { ID = DroneView.Drone.DroneId });
                    else
                        return RedirectToAction("Detail", new { ID = DroneView.Drone.DroneId });
                  //  
        } else {

          DroneView.OwnerList = Util.getListSQL("SELECT Name + ' [' + Code + ']', AccountId FROM MSTR_Account ORDER BY Name");
          DroneView.UAVTypeList = Util.GetDropDowntList("UAVType", "Name", "Code", "usp_Portal_GetDroneDropDown");
          DroneView.ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown");
            //PartsGroupList = Util.GetDropDowntList();

          return View(DroneView);
        }
        
      } catch(Exception ex) {
        return View("InternalError", ex);
      }
    }

    // GET: Drone/Delete/5
    public String Delete([Bind(Prefix = "ID")]int DroneID = 0) {
      String SQL = "";
      Response.ContentType = "text/json";
      if(!exLogic.User.hasAccess("DRONE.DELETE"))
        return Util.jsonStat("ERROR", "Access Denied");

      //Delete the drone from database if there is no flights are created
      SQL = "SELECT Count(*) FROM [DroneFlight] WHERE DroneID = " + DroneID;
      if(Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a drone with a flight attached");

      SQL = "DELETE FROM [M2M_DroneParts] WHERE DroneID = " + DroneID;
      Util.doSQL(SQL);

      SQL = "DELETE FROM [DroneFlight] WHERE DroneID = " + DroneID;
      Util.doSQL(SQL);


      SQL = "DELETE FROM [MSTR_DRONE] WHERE DroneID = " + DroneID;
      Util.doSQL(SQL);

      return Util.jsonStat("OK");
    }


    public ActionResult FillParts(int DroneId) {
      var DroneParts = (from MSTR_Parts in ctx.MSTR_Parts
                        select new {
                          MSTR_Parts.PartsId,
                          MSTR_Parts.Model,
                          MSTR_Parts.PartsName,
                        });

      return Json(DroneParts, JsonRequestBehavior.AllowGet);
    }

    private object Toint(int? supplierId) {
      throw new NotImplementedException();
    }


    public ActionResult TechnicalLogAdd([Bind(Prefix = "ID")] int DroneID = 0, int FlightID = 0) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Technical Log";
      Drones theDrone = new Drones() { DroneID = DroneID };
      List<DroneFlight> Flights = new List<DroneFlight>() {
        new DroneFlight{
          ID = 0,
          DroneID = 0,
          FlightDate = DateTime.Now
        }
      };
      ViewBag.DroneID = DroneID;
      theDrone.getLogFlights(Flights, FlightID);
      return View(Flights);
    }//TechnicalLogAdd


    [HttpPost]
    [ActionName("TechnicalLogAdd")]
    public ActionResult PostTechnicalLogAdd(List<DroneFlight> theFlight, [Bind(Prefix = "ID")] int DroneID = 0) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      Drones theDrone = new Drones() { DroneID = DroneID };
      theDrone.saveTechnicalLog(Request);
      return RedirectToAction("Manage", new { ID = DroneID });
    }//TechnicalLogAdd

    public ActionResult TechnicalLog([Bind(Prefix = "ID")] int DroneID = 0) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      String SQL =
"SELECT\n" +
"  LogFrom,\n" +
"  LogTo,\n" +
"  Convert(Varchar(11), FlightDate, 9) as 'FlightDate(UTC)',\n" +
"  Convert(Varchar, LogTakeOffTime, 108) as TakeOff,\n" +
"  Convert(Varchar, LogLandingTime, 108) as Landing,\n" +
"  Convert(Varchar, DATEADD(\n" +
"    Minute,\n" +
"    DATEDIFF(MINUTE, LogTakeOffTime, LogLandingTime),\n" +
"    '2000-01-01 00:00:00'), 108) as Duration,\n" +
"  LogBattery1ID as BatteryID1,\n" +
"  LogBattery2ID as BatteryID2,\n" +
"  ID as _Pkey,\n" +
"  Count(*) Over() as _TotalRecords\n" +
"FROM\n" +
"  DroneFlight\n" +
"WHERE\n" +
"  IsLogged = 1 AND\n" +
"  DroneID=" + DroneID;

      ViewBag.Title = "Technical Log";
      ViewBag.DroneID = DroneID;
      qView nView = new qView(SQL);
      if(exLogic.User.hasAccess("DRONE.EDIT"))
        nView.addMenu("Edit", Url.Action("TechnicalLogAdd",
new { ID = DroneID, FlightID = "_Pkey" }));


      if(Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult Document([Bind(Prefix = "ID")] int? DroneID) {
      if(!exLogic.User.hasAccess("DRONE"))
        return RedirectToAction("NoAccess", "Home");
      using(var ctx = new ExponentPortalEntities()) {
        var List = ctx.DroneDocuments.ToList();
        var aa = (from p in List where p.DroneID == DroneID select p).ToList();
        ;
        return View(aa);

      }

    }

    public ActionResult GCAApproval() {
      if(!exLogic.User.hasAccess("FLIGHT.GCAAPPROVAL"))
        return RedirectToAction("NoAccess", "Home");
      String SQL = "SELECT [ApprovalID]\n ,[ApprovalName]\n,[ApprovalDate]\n,[StartDate]\n ,[EndDate]\n ,[StartTime]\n,[EndTime]\n,[ApprovalFileUrl]\n,Count(*) Over() as _TotalRecords,DroneID as _PKey FROM[ExponentPortal].[dbo].[GCA_Approval]";
      qView nView = new qView(SQL);
      if(Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult GCADetails([Bind(Prefix = "ID")] int? DroneID) {
      if(!exLogic.User.hasAccess("FLIGHT.GCAAPPROVAL"))
        return RedirectToAction("NoAccess", "Home");
      using(var ctx = new ExponentPortalEntities()) {
        var List = ctx.GCA_Approval.ToList();
        var aa = (from p in List where p.DroneID == DroneID select p).ToList();
        return View(aa);
      }
    }


        //uploading multiple images for the drone


        public ActionResult DroneImaging([Bind(Prefix = "ID")] int DroneID = 0,string Actions="")
        {
            if (!exLogic.User.hasAccess("DRONE"))
                return RedirectToAction("NoAccess", "Home");
            ViewBag.DroneID = 0;
            ViewBag.Actions = Actions;
              ExponentPortalEntities Db = new ExponentPortalEntities();
            ViewBag.DroneID = DroneID;
           
            List<DroneDocument> Docs = (
              from o in Db.DroneDocuments
              where o.DocumentType == "Drone Image" &&
              o.DroneID == DroneID
              select o).ToList();

            return View(Docs);
        }//Drone Imaging



        [HttpPost]
        public String UploadDroneFile([Bind(Prefix = "ID")]  int DroneID = 0, String DocumentType = "Drone Image", String CreatedOn = "")
        {
            if (!exLogic.User.hasAccess("DRONE"))
                return "Access Denied";
            DateTime FileCreatedOn = DateTime.MinValue;
            String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
            //send information in JSON Format always

            StringBuilder JsonText = new StringBuilder();
            GPSInfo GPS = new GPSInfo();
            Response.ContentType = "text/json";

            try
            {
                FileCreatedOn = DateTime.ParseExact(CreatedOn, "ddd, d MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture);
                string FileCreated_ON = FileCreatedOn.ToString();
            }
            catch { }

            //when there are files in the request, save and return the file information
            try
            {
                var TheFile = Request.Files[0];
                String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName.ToLower();
                String DroneName = Util.getDroneNameByDroneID(DroneID);
                String UploadDir = UploadPath + DroneName + "\\" + DroneID + "\\";                
                String FileURL = FileName;
                String FullName = UploadDir + FileName;
                //Save the file to Disk
                if (!Directory.Exists(UploadDir))
                    Directory.CreateDirectory(UploadDir);
               TheFile.SaveAs(FullName);
                TheFile.InputStream.Close();
                TheFile.InputStream.Dispose();
                GC.Collect();


                ExifLib GeoTag = new ExifLib(FullName, FullName);
               // GPS = GeoTag.getGPS(FlightID, FileCreatedOn);
               // GeoTag.setGPS(GPS);
                GeoTag.setThumbnail(100);

               






                JsonText.Append("{");
                JsonText.Append(Util.Pair("status", "success", true));
                JsonText.Append(Util.Pair("url", "/upload/drone/" + DroneName + "/" + DroneID + "/" + FileURL, true));
                JsonText.Append("\"GPS\":{");
                JsonText.Append("\"Info\":\"");
                JsonText.Append(2.00);
                JsonText.Append("\",");
                JsonText.Append("\"Latitude\":");
                JsonText.Append(2.20);
                JsonText.Append(",");
                JsonText.Append("\"Longitude\":");
                JsonText.Append(2.20);
                JsonText.Append(",");
                JsonText.Append("\"Altitude\":");
                JsonText.Append(0);
                JsonText.Append(",");
                JsonText.Append(Util.Pair("CreatedDate", FileCreatedOn.ToString("dd-MMM-yyyy hh:mm"), false));
                JsonText.Append(",");
                JsonText.Append("\"DroneID\":");
                JsonText.Append(DroneID);
                JsonText.Append("},");

                JsonText.Append("\"addFile\":[");
                JsonText.Append(Util.getFileInfo(FullName, FileURL));
                JsonText.Append("]}");

                //now add the uploaded file to the database
                String SQL = "INSERT INTO DroneDocuments(\n" +
                  " DroneID,  DocumentType, DocumentName, UploadedDate,DocumentDate, UploadedBy\n" +
                 
                  ") VALUES (\n" +
                  "  '" + DroneID + "',\n" +                
                  "  '" + DocumentType + "',\n" +
                  "  '" + FileURL + "',\n" +
                  "  GETDATE(),\n" +
                  " '" + FileCreatedOn + "'," +
                  "  " + Util.getLoginUserID() + "\n" +
                
                  ")";
                int DocumentID = Util.InsertSQL(SQL);

            }
            catch (Exception ex)
            {
                JsonText.Clear();
                JsonText.Append("{");
                JsonText.Append(Util.Pair("status", "error", true));
                JsonText.Append(Util.Pair("message", ex.Message, false));
                JsonText.Append("}");
            }//catch
            return JsonText.ToString();
        }//UploadDroneFile

        private void MoveDroneUploadFileTo(int DroneID)
        {
          

      //  String[] Files = Request["data-file"].Split(',');
   ExponentPortalEntities db = new ExponentPortalEntities();
        var FileList = (
        from t1 in db.DroneDocuments
        where t1.DroneID ==0 &&
     t1.DocumentType=="Drone Image"       
        select new
        {
            Name=t1.DocumentName
         
        }).ToList();

            String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
            String OldUploadDir = UploadPath + "0\\";
            String DroneName = DroneID == 0 ? "0" : Util.getDroneName(DroneID);
            String NewUploadDir = UploadPath + DroneName + "\\" + DroneID + "\\";
            foreach (var file in FileList)
            {
                if (String.IsNullOrEmpty(file.Name.ToString()))
                    continue;
                String OldFullPath = OldUploadDir + file.Name.ToString();
                String NewFullPath = NewUploadDir + file.Name.ToString();
                String OldThumbFullPath = OldUploadDir + file.Name.Replace(".jpg", ".t.png");
                String NewThumbFullPath = NewUploadDir + file.Name.Replace(".jpg", ".t.png");

                if (System.IO.File.Exists(OldFullPath))
                {
                    if (!Directory.Exists(NewFullPath))
                        Directory.CreateDirectory(NewUploadDir);

                    if (!System.IO.File.Exists(NewFullPath))
                    {
                        System.IO.File.Move(OldFullPath, NewFullPath);
                        System.IO.File.Move(OldThumbFullPath, NewThumbFullPath);
                    }
                    String SQL = "UPDATE DroneDocuments SET\n" +
                    "  DroneID=" + DroneID + "\n" +                    
                    "WHERE\n" +
                    "  DocumentName='" + file.Name.ToString() + "'" +
                    " and DocumentType='Drone Image'";
                    Util.doSQL(SQL);
                }//if(System.IO.File.Exists
            }//foreach (String file in Files)
        }//MoveUploadFileT

        public String DeleteDroneFile([Bind(Prefix = "ID")] int DroneID, String file = "")
        {
            if (!exLogic.User.hasAccess("DRONE"))
                return "Access Denied";
            bool isDeleted = false;
            StringBuilder JsonText = new StringBuilder();
            JsonText.Append("{");

                String SQL = "SELECT Count(*) FROM DroneDocuments\n" +
                  "WHERE\n" +
                  "  DocumentName='" + file + "' AND\n" +
                  "  DroneID = '" + DroneID + "'\n" +
                  " and DocumentType='Drone Image'";
                int DocCount = Util.getDBInt(SQL);

                if (DocCount > 0)
                {
                string DroneName = Util.getDroneNameByDroneID(DroneID);
                    String UploadPath = Server.MapPath(Url.Content(RootUploadDir+ DroneName+"//"+ DroneID +"//"));
                    String FullPath = Path.Combine(UploadPath, file);
                string PngPath= Path.Combine(UploadPath, file.Replace(".jpg", ".t.png"));
                if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    System.IO.File.Delete(PngPath);
                    //now add the uploaded file to the database
                    SQL = "DELETE FROM DroneDocuments\n" +
                        "WHERE\n" +
                        "  DocumentName='" + file + "' AND\n" +
                        "  DroneID = '" + DroneID + "'"+
                        " and DocumentType='Drone Image'";
                    Util.doSQL(SQL);
                        isDeleted = true;
                        JsonText.Append(Util.Pair("status", "success", true));
                        JsonText.Append(Util.Pair("message", "Deleted successfully.", false));
                    }
                }
            
            if (!isDeleted)
            {
                JsonText.Append(Util.Pair("status", "error", true));
                JsonText.Append(Util.Pair("message", "Can not delete the file from server.", false));
            }
            JsonText.Append("}");

            return JsonText.ToString();
        }



    }
}//class/namespace
