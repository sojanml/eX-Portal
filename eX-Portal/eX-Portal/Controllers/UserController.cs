using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Data.Entity;

namespace eX_Portal.Controllers {

  public class UserController : Controller {
    // GET: UserLogin
    public ExponentPortalEntities db = new ExponentPortalEntities();

    public object EntityState { get; private set; }

    public ActionResult Index() {
      ViewBag.Title = "Login";
      UserLogin _objuserlogin = new UserLogin();
      return View(_objuserlogin);
    }//Login()


    [HttpPost]
    public ActionResult Index(UserLogin _objuserlogin) {
      ViewBag.Title = "Login";

      /*Create instance of entity model*/
      ExponentPortalEntities objentity = new ExponentPortalEntities();
      /*Getting data from database for user validation*/

      if (exLogic.User.UserValidation(_objuserlogin.UserName, _objuserlogin.Password) > 0) {
        /*Redirect user to success apge after successfull login*/
        ViewBag.Message = 1;
        UserInfo thisUser = exLogic.User.getInfo(_objuserlogin.UserName);
        Session["FirstName"] = thisUser.FullName;
        Session["UserID"] = thisUser.UserID;
        Session["UserName"] = thisUser.UserName;
        Session["BrandLogo"] = thisUser.BrandLogo;
        Session["BrandColor"] = thisUser.BrandColor;
        Session["AccountID"] = thisUser.AccountID;
        return RedirectToAction("Index", "Home");

      } else {
        ViewBag.Message = 0;
      }
      return View(_objuserlogin);
    }//HttpPost Login()

    public ActionResult Logout() {
      ViewBag.Title = "Logout";
      Session.RemoveAll();
      return View();
    }//Login()

    public ActionResult UserList() {
      if (!exLogic.User.hasAccess("USER.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "User View";
      string SQL = "select a.UserName,a.FirstName,a.MobileNo,b.ProfileName, Count(*) Over() as _TotalRecords ,  a.UserId as _PKey " +
          " from MSTR_User a left join MSTR_Profile b on a.UserProfileId = b.ProfileId  ";


      qView nView = new qView(SQL);
      if (exLogic.User.hasAccess("USER.VIEW")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("USER.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)



    }


    public ActionResult Create() {

      ViewBag.Title = "Create User";
      if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");

      var viewModel = new ViewModel.UserViewModel {
        User = new MSTR_User(),

        ProfileList = Util.GetProfileList(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
        AccountList = Util.GetAccountList()

      };
      return View(viewModel);
    }




    // GET: DroneService/Edit/5
    public ActionResult Edit(int id) {

      if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
      var viewModel = new ViewModel.UserViewModel {
        User = db.MSTR_User.Find(id),
        ProfileList = Util.GetProfileList(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
        AccountList = Util.GetAccountList()
      };
      return View(viewModel);
    }



    [HttpPost]
    public ActionResult Edit(MSTR_User User) {
      String Pass_SQL = "\n";
      if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
      if (ModelState.IsValid) {
        if (!String.IsNullOrEmpty(User.Password) && !String.IsNullOrEmpty(User.ConfirmPassword)) {
          if (User.Password != User.ConfirmPassword) {
            ModelState.AddModelError("User.Password", "Password doesn't match.");
          } else {
            Pass_SQL = ",\n  Password='" + Util.GetEncryptedPassword(User.Password).ToString() + "'\n";
          }
        }
      }

      if (ModelState.IsValid) {
        string SQL = "UPDATE MSTR_USER SET\n"+
          "  UserProfileId=" + User.UserProfileId + ",\n" +
          "  FirstName='" + User.FirstName + "',\n" +
          "  LastName='" + User.LastName + "',\n" +
          "  Remarks='" + User.Remarks + "',\n" +
          "  MobileNo='" + User.MobileNo + "',\n" +
          "  EmailId='" + User.EmailId + "',\n" +
          "  CountryId=" + User.CountryId + ",\n" +
          "  AccountId=" + User.AccountId + ",\n" +
          "  OfficeNo='" + User.OfficeNo + "',\n" +
          "  HomeNo='" + User.HomeNo + "',\n" +
          "  IsActive='" + User.IsActive + "'\n" +
          Pass_SQL + 
          "where\n" +
          "  UserId=" + User.UserId;
    int id = Util.doSQL(SQL);
      return RedirectToAction("UserList");
      }

      var viewModel = new ViewModel.UserViewModel {
        User = User,
        ProfileList = Util.GetProfileList(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
        AccountList = Util.GetAccountList()
      };
      return View(viewModel);

    }//ActionEdit()


    [HttpPost]
    public ActionResult Create(MSTR_User User) {
      if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (ModelState.IsValid) {
        if (exLogic.User.UserExist(User.UserName) > 0) {
          ModelState.AddModelError("User.UserName", "This username already exists.");
        }

        if (String.IsNullOrEmpty(User.Password)) {
          ModelState.AddModelError("User.Password", "Invalid Password. Please enter again.");
        }


      }

      if (ModelState.IsValid) {
        string Password = Util.GetEncryptedPassword(User.Password).ToString();
        String SQL = "insert into MSTR_User(\n" +
          "  UserName,\n" +
          "  Password,\n" +
          "  FirstName,\n" +
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
          "  AccountId\n" +
          ") values(\n" +
          "  '" + User.UserName + "',\n" +
          "  '" + Password + "',\n" +
          "  '" + User.FirstName + "',\n" +
          "  " + Util.getLoginUserID() + ",\n" +
          "  " + User.UserProfileId + ",\n" +
          "  '" + User.Remarks + "',\n" +
          "  '" + User.MobileNo + "',\n" +
          "  '" + User.OfficeNo + "',\n" +
          "  '" + User.HomeNo + "',\n" +
          "  '" + User.EmailId + "',\n" +
          "  " + User.CountryId + ",\n" +
          "  '" + User.IsActive + "',\n" +
          "  GETDATE(),\n" +
          "  " + User.AccountId + "\n" +
          ")";

        int id = Util.InsertSQL(SQL);
        return RedirectToAction("UserList");
      }

      var viewModel = new ViewModel.UserViewModel {
        User = User,
        ProfileList = Util.GetProfileList(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
        AccountList = Util.GetAccountList()

      };
      return View(viewModel);
    }//Create() HTTPPost




    public String Delete([Bind(Prefix = "ID")]int UserID = 0) {
      if (!exLogic.User.hasAccess("USER.DELETE"))

        return Util.jsonStat("ERROR", "Access Denied");
      String SQL = "";
      Response.ContentType = "text/json";


      //Delete the drone from database if there is no user createdby
      SQL = "SELECT Count(*) FROM MSTR_User where CreatedBy = " + UserID;

      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the User Attached to another user");

      SQL = "select count(*) from Mstr_Drone where CreatedBy=" + UserID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the User Attached to Drone Creation");
      SQL = "select count(*) from Mstr_DroneService where CreatedBy=" + UserID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the User Attached to DroneService Creation");



      SQL = "DELETE FROM [MSTR_USER] WHERE UserId = " + UserID;
      Util.doSQL(SQL);

      return Util.jsonStat("OK");
    }





  } //class
}//namespace

