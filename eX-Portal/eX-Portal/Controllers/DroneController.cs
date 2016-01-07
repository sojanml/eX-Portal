using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;
using System.Text;
using System.IO;

namespace eX_Portal.Controllers {
  public class DroneController : Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();
    static String RootUploadDir = "~/Upload/Drone/";
    // GET: Drone
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "Drone Listing";

      String SQL = "SELECT \n" +
          "  D.[DroneName],\n" +
          "  D.[ModelName] as Description,\n" +
          "  D.[CommissionDate],\n" +         
          "  O.Name as OwnerName,\n" +
          "  M.Name as Manufacture,\n" +
          "  U.Name as UAVType,\n" +
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

      if (!exLogic.User.hasAccess("DRONE.MANAGE")) {
        SQL +=
          "WHERE\n" +
          "  D.AccountID=" + Util.getAccountID();
      }
      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("DRONE.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("FLIGHT.CREATE")) nView.addMenu("Create Flight", Url.Action("Create", "DroneFlight", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("FLIGHT")) nView.addMenu("Flights", Url.Action("Index", "DroneFlight", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("DRONE.MANAGE")) nView.addMenu("Manage", Url.Action("Manage", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("BLACKBOX")) nView.addMenu("Blackbox", Url.Action("Index", "BlackBox", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("DRONE.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_Pkey" }));


      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }

    public ActionResult Manage([Bind(Prefix = "ID")] int DroneID = 0) {
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Manage - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }

    public ActionResult Decommission([Bind(Prefix = "ID")] int DroneID) {
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Decommission - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }//Decommission()

    [HttpPost]
    public ActionResult Decommission([Bind(Prefix = "ID")] int DroneID, String DecommissionNote) {
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");
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
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");

      switch (Type.ToLower()) {
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
      ViewBag.Title = ViewBag.DocumentType + " Report - " + Util.getDroneName(DroneID);
      return View(DroneID);
    }//Decommission()

    //Saving notes to mstr_drone database for uat,commission,incident

    [HttpPost]
    public ActionResult UploadDocument([Bind(Prefix = "ID")] int DroneID, String Type, String Note) {
      String SQL = "";
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");

      switch (Type.ToLower()) {
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


    public String UploadFile([Bind(Prefix = "ID")] int DroneID, String DocumentType) {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
        String UploadDir = UploadPath + Util.getDroneName(DroneID) + "\\" + DocumentType + "\\";
        String FileURL = Util.getDroneName(DroneID) + "/" + DocumentType + "/" + FileName;
        String FullName = UploadDir + FileName;

        if (!Directory.Exists(UploadDir)) Directory.CreateDirectory(UploadDir);
        TheFile.SaveAs(FullName);
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName));
        JsonText.Append("]}");

        //now add the uploaded file to the database
        String SQL = "INSERT INTO DroneDocuments(\n" +
          " DroneID, DocumentType, DocumentName, UploadedDate, UploadedBy\n" +
          ") VALUES (\n" +
          "  '" + DroneID + "',\n" +
          "  '" + DocumentType + "',\n" +
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
    }//Save()


    // GET: Drone/Details/5
    public ActionResult Detail([Bind(Prefix = "ID")] int DroneID) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
      if(!exLogic.User.hasDrone(DroneID)) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = Util.getDroneName(DroneID);
      ViewBag.DroneID = DroneID;

      return View();
    }

    //Partial view for Details of file uploaded for commission,decommission,uat,incident etc.
    public ActionResult FileDetail(int ID, String DocumentType) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneID = ID;
      ViewBag.DocumentType = DocumentType;
      var FileList = Listing.getFileNames(ID, DocumentType);
      return PartialView(FileList);
    }//FileDetail()

    public ActionResult DroneParts(int ID = 0) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
      List<String> Parts = new List<String>();
      Parts = Listing.DroneListing(ID);
      return PartialView(Parts);
    }

    public ActionResult getDroneParts(int ID = 0) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
      var Parts = Listing.getParts(ID);
      return PartialView(Parts);
    }

    // GET: Drone/Details/5
    public String DroneDetail([Bind(Prefix = "ID")]  int DroneID) {

      if (!exLogic.User.hasAccess("DRONE")) return "Access Denied";
      String SQL = "SELECT \n" +
          "  D.[DroneName],\n" +
          "  Convert(varchar(12), D.[CommissionDate], 6) As [Date],\n" +
          "  D.[DroneSerialNo],\n" +
          "  O.Name as OwnerName,\n" +
          "  M.Name as ManufactureName,\n" +
          "  U.Name as UAVType,\n" +
          "  D.[DroneIdHexa],\n" +
          "  D.[ModelName] as Description\n" +
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
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) {
        SQL += " AND\n" +
          "  D.AccountID=" + Util.getAccountID();
      }

      qDetailView nView = new qDetailView(SQL);
      return ReassignDetail(DroneID) + DecommissionDetail(DroneID) + nView.getTable();
      //+
    }



    public String ReassignDetail(int DroneID) {
      StringBuilder Detail = new StringBuilder();

      string SQL = "select\n" +
          "  ISNULL(a.isactive, 'True') as IsActive,\n" +
          "  a.DroneId,\n" +
          "  a.ReAssignNote,\n" +
          "  a.DroneName,\n" +
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
      if ((bool)Row["hasRows"] && Row["IsActive"].ToString() != "True") {
        Detail.AppendLine("<div class=\"decommission-info\">");
        Detail.AppendLine("ReAssigned For");
        Detail.AppendLine("<span>" + Row["DroneName"] + "</span>");
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
      if ((bool)Row["hasRows"] && Row["isactive"].ToString() != "True") {
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
      if (!exLogic.User.hasAccess("DRONE.CREATE")) return RedirectToAction("NoAccess", "Home");
      String OwnerListSQL = "SELECT Name + ' [' + Code + ']', AccountId FROM MSTR_Account ORDER BY Name";
      var viewModel = new ViewModel.DroneView {
        Drone = new MSTR_Drone(),
        OwnerList = Util.getListSQL(OwnerListSQL),
        UAVTypeList = Util.GetDropDowntList("UAVType", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };

      return View(viewModel);
    }



    // POST: Drone/Create
    [HttpPost]
    public ActionResult Create(ViewModel.DroneView DroneView) {
      if (!exLogic.User.hasAccess("DRONE.CREATE")) return RedirectToAction("NoAccess", "Home");
      try {
        // TODO: Add insert logic here
        int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) + 1 FROM MSTR_DRONE");
        if (DroneSerialNo < 1001) DroneSerialNo = 1001;
        MSTR_Drone Drone = DroneView.Drone;
              
        String SQL = "INSERT INTO MSTR_DRONE(\n" +
          "  AccountID,\n" +
          "  MANUFACTUREID,\n" +
          "  UAVTYPEID,\n" +
          "  COMMISSIONDATE,\n" +
          "  DRONEDEFINITIONID,\n" +
          "  ISACTIVE,\n" +
          "  DroneSerialNo,\n" +
          "  ModelName\n" +
          ") VALUES(\n" +
          "  '" + Drone.AccountID + "',\n" +
          "  '" + Drone.ManufactureId + "',\n" +
          "  '" + Drone.UavTypeId + "',\n" +
          "  '" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
          "  11,\n" +
          "  'True',\n" +
          "  " + DroneSerialNo +
          "  ,'" +  Drone.ModelName +
          "');";
        int DroneId = Util.InsertSQL(SQL);

        if (DroneView.SelectItemsForParts != null) {
          for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
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

        return RedirectToAction("Detail", new { ID = DroneId });
      } catch (Exception ex) {
        Util.ErrorHandler(ex);
        return View("InternalError", ex);
      }
    }






    public ActionResult ReAssign(int id) {
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");
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
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) return RedirectToAction("NoAccess", "Home");
      try {
        int DroneId = 0;
        // TODO: Add update logic here
        if (ModelState.IsValid) {


          MSTR_Drone Drone = DroneView.Drone;
          //Inserting the Reassigned Drone
          int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) + 1 FROM MSTR_DRONE");
          if (DroneSerialNo < 1001) DroneSerialNo = 1001;

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

          if (DroneView.SelectItemsForParts != null) {
            for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
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

      } catch (Exception ex) {
        return View("InternalError", ex);
      }
    }

    // GET: Drone/Edit/5
    public ActionResult Edit(int id) {
      if (!exLogic.User.hasAccess("DRONE.EDIT")) return RedirectToAction("NoAccess", "Home");
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
      if (!exLogic.User.hasAccess("DRONE.EDIT")) return RedirectToAction("NoAccess", "Home");
      try {
        // TODO: Add update logic here
        if (ModelState.IsValid) {
          //int DroneSerialNo = Util.getDBInt("SELECT Max(DroneSerialNo) FROM MSTR_DRONE");
          //if (DroneSerialNo < 1000) DroneSerialNo = 1001;

          MSTR_Drone Drone = DroneView.Drone;
          //master updating

          string SQL = "UPDATE MSTR_DRONE SET\n" +
          "   AccountID ='" + Drone.AccountID + "'," +
          "  MANUFACTUREID ='" + Drone.ManufactureId + "',\n" +
          "  UAVTYPEID ='" + Drone.UavTypeId + "',\n" +
          "  COMMISSIONDATE ='" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',\n" +
           "  MODELNAME ='" + Drone.ModelName + "'\n" +
          "WHERE\n" +
          "  DroneId =" + Drone.DroneId;
          int DroneId = Util.doSQL(SQL);

          //Parts updating

          SQL = "delete from M2M_DroneParts where DroneId=" + Drone.DroneId;
          int Id = Util.doSQL(SQL);
          if (DroneView.SelectItemsForParts != null) {
            for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
              string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
              int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
              SQL = "Insert into M2M_DroneParts (DroneId,PartsId,Quantity) values(" + Drone.DroneId + "," + PartsId + "," + Qty + ");";
              int ID = Util.doSQL(SQL);
            }

          }

        }
        return RedirectToAction("Detail", new { ID = DroneView.Drone.DroneId });
      } catch (Exception ex) {
        return View("InternalError", ex);
      }
    }

    // GET: Drone/Delete/5
    public String Delete([Bind(Prefix = "ID")]int DroneID = 0) {
      String SQL = "";
      Response.ContentType = "text/json";
      if (!exLogic.User.hasAccess("DRONE.DELETE"))
        return Util.jsonStat("ERROR", "Access Denied");

      //Delete the drone from database if there is no flights are created
      SQL = "SELECT Count(*) FROM [DroneFlight] WHERE DroneID = " + DroneID;
      if (Util.getDBInt(SQL) != 0)
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
      ViewBag.Title = "Technical Log";
      Drones theDrone = new Drones(){ DroneID = DroneID };
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
      Drones theDrone = new Drones() { DroneID = DroneID };
      theDrone.saveTechnicalLog(Request);
      return RedirectToAction("Manage", new { ID = DroneID });
    }//TechnicalLogAdd

    public ActionResult TechnicalLog([Bind(Prefix = "ID")] int DroneID = 0) {
      String SQL =
      "SELECT\n" +
      "  LogFrom,\n" +
      "  LogTo,\n" +
      "  Convert(Varchar(11), FlightDate, 9) as FlightDate,\n" +
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
      if (exLogic.User.hasAccess("DRONE.EDIT")) nView.addMenu("Edit", Url.Action("TechnicalLogAdd", 
         new { ID = DroneID, FlightID = "_Pkey" }));


      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    }//class
}//namespace
