using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Data.Entity;
using System.Text;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.SessionState;
using FileStorageUtils;
using eX_Portal.Models;

namespace eX_Portal.Controllers {
  public class PilotController : Controller {

    public ExponentPortalEntities db = new ExponentPortalEntities();
    static String RootUploadDir = "~/Upload/User/";
    public object EntityState { get; private set; }
    // GET: Pilot
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("PILOTS.VIEW"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "All Pilot";
      string SQL =
        @"select
          MSTR_User.FirstName + ' ' + MSTR_User.LastName as FullName,
          MSTR_User.MobileNo,
          MSTR_User.EmailId,
          MSTR_User.RPASPermitNo as PermitNumber,
          MSTR_User.DOI_RPASPermit as IssueDate,
          MSTR_User.DOE_RPASPermit as ExpiryDate,
          Count(*) Over() as _TotalRecords,
          MSTR_User.UserID as _PKey
        FROM
          MSTR_User";

      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        SQL += "\n WHERE MSTR_User.AccountID=" + Util.getAccountID();
      }

      qView nView = new qView(SQL);
      if (exLogic.User.hasAccess("PILOTS.VIEW"))
        nView.addMenu("Detail", Url.Action("PilotDetail", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("PILOTS.EDIT"))
        nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      // if (exLogic.User.hasAccess("PILOTS.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
      //if (exLogic.User.hasAccess("PILOTLOG.VIEW")) nView.addMenu("Add Pilot Log", Url.Action("Create", "PilotLog", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)


    }

    // GET: Pilot/Details/5
    public ActionResult PilotDetail([Bind(Prefix = "ID")] int UserID = 0) {
      if (exLogic.User.hasAccess("PILOTS.VIEW") ||
          exLogic.User.hasAccess("ORGANIZATION.ADMIN")) {
        //give the access.
      } else {
        return RedirectToAction("NoAccess", "Home");
      }
        
      Models.MSTR_User User = db.MSTR_User.Find(UserID);
      if (User == null)
        return RedirectToAction("Error", "Home");
      ViewBag.Title = String.Concat(User.FirstName, " ", User.LastName);
      return View(User);

    }//UserDetail()

    // GET: Pilot/Create
    [ChildActionOnly]
    public ActionResult PilotDetailView([Bind(Prefix = "ID")] int UserID = 0) {

      string SQL = "SELECT a.[UserName]\n" +
                  " ,a.[FirstName] \n " +
                  ",a.[MiddleName]\n " +
                  ",a.[LastName]\n  " +
                  //",a.[Remarks]\n   " +
                  ",a.[MobileNo]\n  " +
                  ",a.[OfficeNo]\n  " +
                  ",a.[HomeNo]\n" +
                  " ,a.[EmailId]\n  as [Email ID] " +
                  ",b.[PassportNo]" +
                  " ,CONVERT(NVARCHAR, b.[DateOfExpiry], 106) AS DateOfExpiry\n   " +
                  ",b.[Department]\n  " +
                  " ,b.[EmiratesId]  as [Emirates ID]\n   " +
                  ",b.[Title] as JobTitle\n   " +
                  ",a.[RPASPermitNo] as [RPAS Permit No.]\n  " +
                  ",a.[PermitCategory] as [Permit Category]\n  " +
                  ",c.[Name] as Organization\n   " +
                  //",d.[ProfileName]\n   " +
                  " FROM[MSTR_User] a\n   " +
                  " left join mstr_user_pilot b\n  " +
                  "on a.UserId=b.UserId\n   " +
                  "left join MSTR_Account c\n  " +
                  "on a.AccountId=c.AccountId\n  " +
                  "left join MSTR_Profile d " +
                  "on a.UserProfileId=d.ProfileId" +
                  " where a.userid=" + UserID;

      if (exLogic.User.hasAccess("PILOT")) {
        //nothing
      } else if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        SQL +=
      " AND\n" +
      "  a.AccountID=" + Util.getAccountID();
      }

      qDetailView nView = new qDetailView(SQL);
      ViewBag.Message = nView.getTable();
      ViewBag.ProfileImage = Util.getProfileImage(UserID);
      return View();
    }


    public ActionResult Create([Bind(Prefix = "ID")] int RPASID = 0) {
      ViewBag.Title = "Create Pilot";
      if (exLogic.User.hasAccess("PILOTS.CREATE") || 
          exLogic.User.hasAccess("ORGANIZATION.ADMIN")) {
        //allow access
      } else { 
        return RedirectToAction("NoAccess", "Home");
      }

      ViewBag.IsPassowrdRequired = true;
      PilotCreateModel EPASValues = new PilotCreateModel();
      if (RPASID != 0) {
        ViewBag.RPASid = RPASID;
        ViewBag.IsPassowrdRequired = false;
        var RPASoList = (from p in db.MSTR_RPAS_User where p.RpasId == RPASID select p).ToList();
        EPASValues.FirstName = RPASoList[0].Name;
        EPASValues.CountryId = Convert.ToInt16(RPASoList[0].NationalityId);
        EPASValues.EmiratesID = RPASoList[0].EmiratesId;
        EPASValues.EmailId = RPASoList[0].EmailId;
        EPASValues.MobileNo = RPASoList[0].MobileNo;
      }

      return View(EPASValues);
    }

    [HttpPost]
    public ActionResult Create(PilotCreateModel UserModel, int ID = 0) {
      if (exLogic.User.hasAccess("PILOTS.CREATE") ||
          exLogic.User.hasAccess("ORGANIZATION.ADMIN")) {
        //allow access
      } else {
        return RedirectToAction("NoAccess", "Home");
      }

      if (ModelState.IsValid) {
        if (exLogic.User.UserExist(UserModel.UserName) > 0) {
          ModelState.AddModelError("User.UserName", "This Pilot already exists.");
        }
        if (exLogic.User.EmailExist(UserModel.EmailId) > 0) {
          ModelState.AddModelError("User.EmailId", "This email id already exists.");
        }
      }
 

      if (ModelState.IsValid) {
        int AccountID = Util.getAccountID();
        String SQL = "insert into MSTR_User(\n" +
          "  UserName,\n" +
          "  Password,\n" +
          "  FirstName,\n" +
          "  MiddleName,\n" +
          "  LastName,\n" +
          "  CreatedBy,\n" +
          "  UserProfileId,\n" +
          "  Remarks,\n" +
          "  MobileNo,\n" +
          "  OfficeNo,\n" +
          "  HomeNo,\n" +
          "  EmailId,\n" +
          "  CountryId,\n" +
          "  IsActive,\n" +
          "  CreatedOn,\n" +
          "  AccountId,\n" +
          "  IsPilot, \n" +
          "  PhotoUrl,\n" +
          "  Dashboard,\n" +
          "  RPASPermitNo,\n" +
          "  PermitCategory,\n" +
          "  ContactAddress,\n" +
          "  RegRPASSerialNo,\n" +
          "  EmiratesID,\n" +
          "  Nationality\n" +
          ") values(\n" +
          "  '" + Util.FirstLetterToUpper(UserModel.UserName) + "',\n" +
          "  '" + Util.MD5(UserModel.Password) + "',\n" +
          "  '" + Util.FirstLetterToUpper(UserModel.FirstName) + "',\n" +
          "  '" + Util.FirstLetterToUpper(UserModel.MiddleName) + "',\n" +
          "  '" + Util.FirstLetterToUpper(UserModel.LastName) + "',\n" +
          "   " + Util.getLoginUserID() + ",\n" +
          "  '" + Util.getPilotProfileID(AccountID) + "' ,\n" +
          "  '" + UserModel.Remarks + "',\n" +
          "  '" + UserModel.MobileNo + "',\n" +
          "  '" + UserModel.OfficeNo + "',\n" +
          "  '" + UserModel.HomeNo + "',\n" +
          "  '" + UserModel.EmailId + "',\n" +
          "   " + Util.toInt(UserModel.CountryId) + ",\n" +
          "  1,\n" +
          "  GETDATE(),\n" +
          "  " + AccountID + ",\n" +
          "  1,\n" +
          "  '" + UserModel.PhotoUrl + "',\n" +
          "  '" + Util.getDashboard() + "',\n" +
          "  '" + UserModel.RPASPermitNo + "',\n" +
          "  '" + UserModel.PermitCategory + "',\n" +
          "  '" + UserModel.ContactAddress + "',\n" +
          "  '" + UserModel.RegRPASSerialNo + "',\n" +
          "  '" + UserModel.EmiratesID + "',\n" +
          "  '" + UserModel.Nationality + "'\n" +
          ")";
        //inserting pilot information to the pilot table
        int id = Util.InsertSQL(SQL);

        SQL = "insert into MSTR_User_Pilot(\n" +
          "  UserId,\n" +
          "  PassportNo,\n" +
          "  DateOfExpiry,\n" +
          "  Department,\n" +
          "  EmiratesId,\n" +
          "  Title\n" +
          ") values(\n" +
          "  '" + id + "',\n" +
          "  '" + UserModel.PassportNo + "',\n" +
          "  '" + UserModel.DateOfExpiry + "',\n" +
          "  '" + UserModel.Department + "',\n" +
            "  '" + UserModel.EmiratesID + "',\n" +
          "  '" + UserModel.Department + "'\n)";
        int Pid = Util.InsertSQL(SQL);


        MovePhto(UserModel.PhotoUrl, id);
        UpdateLinkedDrone(UserModel.LinkedDroneID, id);
        return RedirectToAction("PilotDetail", "Pilot", new { ID = id });

      }
      
      return View(UserModel);
    }//Create() HTTPPost
     // POST: Pilot/Create

    private void MovePhto(String PhotoUrl, int UserID) {
      //move the image to correct path
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      String newPath = UploadPath + UserID + "/";
      String PhotoURL = UploadPath + "0/" + PhotoUrl;
      if (!System.IO.Directory.Exists(newPath))
        Directory.CreateDirectory(newPath);
      if (!String.IsNullOrEmpty(PhotoUrl) &&
          System.IO.File.Exists(PhotoURL)) {
        System.IO.File.Move(PhotoURL, newPath + PhotoUrl);
      }
    }

    private void UpdateLinkedDrone(IList<int> LinkedDroneIDs, int id) {
      String SQL = $"DELETE FROM M2M_Drone_User Where UserID={id}";
      Util.doSQL(SQL);
      if (LinkedDroneIDs.Any()) {
        SQL = "";
        foreach (var LinkedDroneID in LinkedDroneIDs) {
          if (SQL != "")
            SQL += ",";
          SQL += $"({id}, {LinkedDroneID})";
        }
        SQL = "Insert Into M2M_Drone_User (UserID, DroneID) VALUES " + SQL;
        Util.doSQL(SQL);
      }

    }


    public ActionResult Edit(int id) {
      if (!exLogic.User.hasAccess("PILOTS.EDIT"))
        return RedirectToAction("NoAccess", "Home");
      PilotEditModel UserModel = new PilotEditModel(id);
      if (UserModel.UserId == 0)
        return HttpNotFound();

      return View(UserModel);
    }


    [HttpPost]
    public ActionResult Edit(int id, ViewModel.PilotEditModel UserModel) {
      //String Pass_SQL = "\n";
      if (!exLogic.User.hasAccess("PILOTS.EDIT"))
        return RedirectToAction("NoAccess", "Home");

      if (ModelState.IsValid) {
        int AccountID = Util.getAccountID();
        UserModel.UserId = id;

        string SQL = "UPDATE MSTR_USER SET\n" +
          "  FirstName='" + Util.FirstLetterToUpper(UserModel.FirstName) + "',\n" +
          "  MiddleName='" + Util.FirstLetterToUpper(UserModel.MiddleName) + "',\n" +
          "  LastName='" + Util.FirstLetterToUpper(UserModel.LastName) + "',\n" +
          "  Remarks='" + UserModel.Remarks + "',\n" +
          "  MobileNo='" + UserModel.MobileNo + "',\n" +
          "  EmailId='" + UserModel.EmailId + "',\n" +
          "  CountryId=" + Util.toInt(UserModel.CountryId.ToString()) + ",\n" +
          "  OfficeNo='" + UserModel.OfficeNo + "',\n" +
          "  HomeNo='" + UserModel.HomeNo + "',\n" +
          "  PhotoUrl='" + UserModel.PhotoUrl + "',\n" +
          "  RPASPermitNo='" + UserModel.RPASPermitNo + "',\n" +
          "  PermitCategory='" + UserModel.PermitCategory + "',\n" +
          "  DOE_RPASPermit='" + UserModel.DOE_RPASPermit + "',\n" +
          "  DOI_RPASPermit='" + UserModel.DOI_RPASPermit + "',\n" +
          "  EmiratesId='" + UserModel.EmiratesID + "',\n" +
          "  Nationality='" + UserModel.Nationality + "'\n";
        if(!String.IsNullOrWhiteSpace(UserModel.ConfirmPassword)) {
          SQL += "," +
          "  [Password]='" + Util.MD5(UserModel.ConfirmPassword.ToLower())+ "'\n";
        }
        SQL +=
          " where\n" +
          "  UserId=" + id;

        Util.doSQL(SQL);

        //updating pilot information to pilot table

        SQL = "UPDATE MSTR_USER_PILOT SET\n" +
         "  DateOfExpiry='" + UserModel.DateOfExpiry + "',\n" +
         "  PassportNo= '" + UserModel.PassportNo + "',\n" +
         "  Department='" + UserModel.Department + "',\n" +
         "  EmiratesId='" + UserModel.EmiratesID + "',\n" +
         "  Title='" + UserModel.Department + "'\n" +
          "where\n" +
         "  UserId=" + id;
        ;
        int idPilot = Util.doSQL(SQL);

        MovePhto(UserModel.PhotoUrl, id);
        UpdateLinkedDrone(UserModel.LinkedDroneID, id);

        return RedirectToAction("Index", "Home");
      }

      var viewModel = new ViewModel.UserViewModel {

        //  ProfileList = Util.GetProfileList(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
        //  AccountList = Util.GetAccountList(),
        //  DashboardList = Util.GetDashboardLists(),
        PermitCategoryList = Util.GetLists("RPASCategory")
      };
      return View(viewModel);

    }//ActionEdit()

    public String UploadFile([Bind(Prefix = "ID")] int UserID = 0) {

      String UploadPath = Server.MapPath(Url.Content(RootUploadDir) + UserID + "/");
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
        String FullName = UploadPath + FileName;

        if (!Directory.Exists(UploadPath))
          Directory.CreateDirectory(UploadPath);
        TheFile.SaveAs(FullName);
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName));
        JsonText.Append("]}");

      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//Save()

    [ChildActionOnly]
    public ActionResult Drones(int PilotID = 0) {
      int AccountID = Util.getAccountID();
      var Query = from d in db.MSTR_Drone
                  where d.AccountID == AccountID
                  join m in db.LUP_Drone.Where(e => e.Type == "Manufacturer") on
                    d.ManufactureId equals m.TypeId into m1
                  from m2 in m1.DefaultIfEmpty()
                  join u in db.LUP_Drone.Where(e => e.Type == "UAVType") on
                    d.UavTypeId equals u.TypeId into u1
                  from u2 in u1.DefaultIfEmpty()
                  join p in db.M2M_Drone_User.Where(e => e.UserID == PilotID) on
                    d.DroneId equals p.DroneID into p1
                  from p2 in p1.DefaultIfEmpty()
                  select new UserDones {
                    DroneID = d.DroneId,
                    DroneName = d.DroneName,
                    Manufacture = m2.Name,
                    UavTypeGroup = u2.GroupName,
                    UavType = u2.Name,
                    LinkDroneID = p2.DroneID 
                  };
      if(Query.Any()) 
        return View(Query.ToList());

      return null;

    }

    public ActionResult Delete([Bind(Prefix = "ID")]int UserID = 0) {
      Response.ContentType = "text/json";

      if (exLogic.User.hasAccess("PILOTS.DELETE") && exLogic.User.hasAccess("ORGANIZATION.ADMIN")) {
        //success
      } else {
        return  new HttpNotFoundResult("Invalid Access Key");
      }

      int AccountID = Util.getAccountID();      
      String SQL = $"SELECT Count(*) FROM MSTR_User where UserID = {UserID} and AccountID = {AccountID}";
      int UserCount = Util.getDBInt(SQL);

      if(UserCount > 0) {
        SQL = $"DELETE FROM Mstr_User WHERE UserID={UserID}";
        Util.doSQL(SQL);
        SQL = $"DELETE FROM MSTR_USER_PILOT WHERE UserID={UserID}";
        Util.doSQL(SQL);
      } else {
        return new HttpNotFoundResult("User not found in DB");        
      }

      return Json(new {
        Status = "OK",
        UserID = UserID,
        AccountID = AccountID
      }, JsonRequestBehavior.AllowGet);
    }
  }
}
