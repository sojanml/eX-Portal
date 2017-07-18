using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {

  public class AdminController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Admin
    public ActionResult Index() {
      return View();
    }//function index()

    public ActionResult Profiles([Bind(Prefix = "ID")] int ProfileID = 0) {
      if (!exLogic.User.hasAccess("PROFILE.MANAGE")) return RedirectToAction("NoAccess", "Home");
      if (ProfileID == 0) return RedirectToAction("ProfileMenu", "Admin");
      ViewBag.Title = "Profile Management - " + Util.getDBVal("SELECT [ProfileName] FROM [MSTR_Profile] WHERE ProfileID=" + ProfileID);
      ViewBag.ProfileID = ProfileID;
      ViewBag.ParentID = 0;
      return View();
    }


    public ActionResult CMS() {
      if(!exLogic.User.hasAccess("CMS.VIEW"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Content Management";
      String SQL = @"SELECT 
          [CmsID],
          [CmsRefName] as URL,
          (CASE WHEN [IsShowInMenu] = 1 THEN 'Yes' ELSE 'No' END) as InMenu ,
          [PageTitle],
          [MenuTitle],
          [ContentManagement].[CreatedOn],
          [FirstName] + ' ' + [LastName] as CreatedBy,
          Count(*) Over() as _TotalRecords,
          [CmsID] as _PKey
        FROM
          [ContentManagement]
        LEFT JOIN MSTR_User ON
          MSTR_User.UserID = ContentManagement.CreatedBy  ";

      qView nView = new qView(SQL);
      if(exLogic.User.hasAccess("CMS.VIEW"))
        nView.addMenu("View Result", Url.Action("demo", "home", new { id = "URL" }));
      if(exLogic.User.hasAccess("CMS.EDIT"))
        nView.addMenu("Edit", Url.Action("CmsEdit", new { ID = "_PKey" }));
      if(exLogic.User.hasAccess("CMS.DELETE"))
        nView.addMenu("Delete", Url.Action("CmsDelete", new { ID = "_PKey" }));

      if(Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Account()

    public ActionResult CMSCreate() {
      if(!exLogic.User.hasAccess("CMS.CREATE"))
        return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "New Contenet Page";
      return View();
    }

    public ActionResult CmsEdit(int id = 0) {
      if(!exLogic.User.hasAccess("CMS.EDIT"))
        return RedirectToAction("NoAccess", "Home");

      var CMS = db.ContentManagements.Find(id);
      if(CMS == null) {
        return RedirectToAction("NoAccess", "Home");
      }
      ViewBag.Title = "Edit: " + CMS.PageTitle;

      return View("CMSCreate", CMS);

    }

    public JsonResult CmsDelete(int id = 0) {
      if(!exLogic.User.hasAccess("CMS.DELETE")) {
        var Obj = new  {
          status = "ERROR",
          message = "You do not have acess to delete a CMS Content"
        };
        return Json(Obj, JsonRequestBehavior.AllowGet);
      }

      var CMS = db.ContentManagements.Find(id);
      if(CMS == null) {
        var Obj = new {
          status = "ERROR",
          message = "CMS Content Page can not be found"
        };
        return Json(Obj, JsonRequestBehavior.AllowGet);
      }

      db.ContentManagements.Remove(CMS);
      db.SaveChanges();

      var SuccessObj = new {
        status = "OK",
        message = "Item is removed successfully"
      };
      return Json(SuccessObj, JsonRequestBehavior.AllowGet);
    }

    [HttpPost ValidateInput(false)]
    public ActionResult CMSCreate(ContentManagement CMS) {
      if(!exLogic.User.hasAccess("CMS.CREATE"))
        return RedirectToAction("NoAccess", "Home");

      CMS.CreatedOn = DateTime.Now;
      CMS.CreatedBy = Util.getLoginUserID();

      //Clear any CMSID error genereated
      if(ModelState.ContainsKey("CmsID"))
        ModelState["CmsID"].Errors.Clear();

      if(!ModelState.IsValid) {
        return View(CMS);
      }

      try { 
        db.ContentManagements.Add(CMS);
        db.SaveChanges();
      } catch (Exception ex) {
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(CMS);
      }

      return RedirectToAction("demo", "home", new { id = CMS.CmsRefName });
      
    }


    [HttpPost ValidateInput(false)]
    public ActionResult CmsEdit(ContentManagement CMS) {
      if(!exLogic.User.hasAccess("CMS.CREATE"))
        return RedirectToAction("NoAccess", "Home");

      CMS.CreatedOn = DateTime.Now;
      CMS.CreatedBy = Util.getLoginUserID();

 
      try {
        db.Entry(CMS).State = System.Data.Entity.EntityState.Modified;
        db.SaveChanges();
      } catch(Exception ex) {
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(CMS);
      }

      return RedirectToAction("demo", "home", new { id = CMS.CmsRefName });

    }

    [ChildActionOnly]
    public ActionResult ProfileMenuGen(int ParentID = 0, int ProfileID = 0) {
      String SQL =
      "SELECT\n" +
      "  [MSTR_Menu].[MenuId],\n" +
      "  [MenuName],\n" +
      "  [PageUrl],\n" +
      "  [Visible],\n" +
      "  [PermissionId],\n" +
      "  (CASE WHEN M2M_ProfileMenu.[MenuId] IS NULL THEN 0 ELSE 1 END) as IsChecked,\n" +
      "  (SELECT Count(*) FROM [MSTR_Menu] as SubItems WHERE SubItems.ParentID = [MSTR_Menu].MenuID) as SubCount\n" +
      "FROM\n" +
      "  [MSTR_Menu]\n" +
      "LEFT JOIN M2M_ProfileMenu ON\n" +
      "  M2M_ProfileMenu.MenuID = [MSTR_Menu].[MenuId] AND\n" +
      "  M2M_ProfileMenu.ProfileID = " + ProfileID + "\n" +
      "WHERE\n" +
      "  [MSTR_Menu].ParentID =" + ParentID;
      var Rows = Util.getDBRows(SQL);
      ViewBag.ProfileID = ProfileID;
      ViewBag.ParentID = ParentID;
      return View(Rows);
    }


    public ActionResult ProfileMenu() {
      if (!exLogic.User.hasAccess("PROFILE.MANAGE")) return RedirectToAction("NoAccess", "Home");
      String SQL = "SELECT [ProfileId], [ProfileName] FROM [MSTR_Profile]";
      var Rows = Util.getDBRows(SQL);
      return View(Rows);
    }

    public String ProfileSet(int ProfileID = 0, int MenuID= 0) {
      if (!exLogic.User.hasAccess("PROFILE.MANAGE")) return "off";
      String Result = "";
      String SQL = "SELECT ID FROM [M2M_ProfileMenu]\n" +
        "WHERE\n" +
        "  ProfileID=" + ProfileID + " AND\n" +
        "  MenuId=" + MenuID;
      int ID = Util.getDBInt(SQL);
      if(ID == 0) {
        SQL = "INSERT INTO [M2M_ProfileMenu](\n" +
        "  ProfileID, MenuID\n" +
        ") VALUES (\n" +
        " " + ProfileID + ", " + MenuID + "\n" +
        ")";
        Result = "on";
      } else {
        SQL = "DELETE FROM [M2M_ProfileMenu]\n" +
          "WHERE\n" +
          "  ProfileID=" + ProfileID + " AND\n" +
          "  MenuId=" + MenuID;
        Result = "off";
      }
      Util.doSQL(SQL);

      return Result; 
    }


    public ActionResult Account() {
      if (!exLogic.User.hasAccess("ACCOUNT.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Accounts";
      String SQL = "SELECT \n" +
        "  [AccountId],\n" +
        "  [Name],\n" +
        "  [Code],\n" +
        "  [EmailId],\n" +
        "  [MobileNo],\n" +
        "  Count(*) Over() as _TotalRecords,\n" +
        "  [AccountId] as _PKey\n" +
        "FROM\n" +
        "  [MSTR_Account]";

      qView nView = new qView(SQL);
      if (exLogic.User.hasAccess("ACCOUNT.VIEW")) nView.addMenu("Detail", Url.Action("AccountDetail", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("ACCOUNT.EDIT")) nView.addMenu("Edit", Url.Action("AccountEdit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("ACCOUNT.DELETE")) nView.addMenu("Delete", Url.Action("AccountDelete", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Account()


    public ActionResult AccountCreate() {

      if (!exLogic.User.hasAccess("ACCOUNT.CREATE")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Create Account";
      var AccountModel = new ViewModel.AccountViewModel {
        Account = new MSTR_Account(),
        CountryList = Util.GetCountryLists("Country", "DroneName", "Code", "usp_Portal_DroneServiceType"),


      };


      return View(AccountModel);
    }

    [HttpPost]
    public ActionResult AccountCreate(MSTR_Account Account) {
      if (!exLogic.User.hasAccess("ACCOUNT.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (ModelState.IsValid) {

        if (Session["UserId"] == null) {
          Session["UserId"] = -1;
        }
        Account.CreatedBy = Util.toInt(Session["UserID"].ToString());
        Account.CreatedOn = DateTime.Now;
        db.MSTR_Account.Add(Account);
        db.SaveChanges();
        //setting up binary code after the  account id creation  and saving
        String CodeBinary = Util.DecToBin(Account.AccountId);
        String SQL = "Update Mstr_Account set BinaryCode='" + CodeBinary + "' where AccountId=" + Account.AccountId;
        Util.doSQL(SQL);
        return RedirectToAction("AccountDetail", new { id = Account.AccountId });
      }

      var viewModel = new ViewModel.AccountViewModel {
        Account = new MSTR_Account(),
        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),



      };
      return View(viewModel);

    }

    public ActionResult AccountEdit(int id) {

      ViewBag.Title = "Edit Account";
      if (!exLogic.User.hasAccess("ACCOUNT.EDIT")) return RedirectToAction("NoAccess", "Home");

      var viewModel = new ViewModel.AccountViewModel {



        Account = db.MSTR_Account.Find(id),

        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
      };
      return View(viewModel);

    }

    [HttpPost]
    public ActionResult AccountEdit(Models.MSTR_Account Account) {
      ViewBag.Title = "Edit Account";
      if (!exLogic.User.hasAccess("ACCOUNT.EDIT")) return RedirectToAction("NoAccess", "Home");

      try {
        if (ModelState.IsValid) {

          if (Session["UserId"] == null) {
            Session["UserId"] = -1;
          }
          Account.ModifiedBy = Util.toInt(Session["UserID"].ToString());
          Account.ModifiedOn = DateTime.Now;

          string SQL = "Update Mstr_Account set Name='" + Account.Name + "',Code='"
                             + Account.Code + "',EmailId='" + Account.EmailId + "',MobileNo='"
                             + Account.MobileNo + "',OfficeNo='" + Account.OfficeNo + "',IsActive='" + Account.IsActive + "',ModifiedBy=" + Session["UserId"] +
                             ",ModifiedOn='" + DateTime.Now.ToString("yyyy - MM - dd") + "', AccountDescription='" + Account.AccountDescription +
                             "',Address1='" + Account.Address1 + "', Address2='" + Account.Address2 + "' ,Address3='" + Account.Address3 +
                             "',CountryCode=" + Account.CountryCode + ",ContactName='" + Account.ContactName + "',ContactTitle='" +
                             Account.ContactTitle + "' where AccountId=" + Account.AccountId;

          int id = Util.doSQL(SQL);

          /* db.Entry(Account).Property(x => x.CreatedBy).IsModified = false;
             db.Entry(Account).Property(x => x.CreatedOn).IsModified = false;
             db.MSTR_Account.Add(Account);
             db.SaveChanges();*/
          return RedirectToAction("Account");
        }
      } catch (Exception ex) {
        //Log the error (uncomment dex variable name and add a line here to write a log.
        return View("InternalError", ex);
      }

      //for the server side validation
      var viewModel = new ViewModel.AccountViewModel {



        Account = db.MSTR_Account.Find(Account.AccountId),

        CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
      };
      return View(viewModel);

    }//ActionEdit()

    public ActionResult AccountDetail([Bind(Prefix = "ID")] int AccountID) {
      if (!exLogic.User.hasAccess("ACCOUNT.VIEW")) return RedirectToAction("NoAccess", "Home");
      Models.MSTR_Account Account = db.MSTR_Account.Find(AccountID);
      if (Account == null) return RedirectToAction("Error", "Home");
      ViewBag.Title = Account.Name;
      return View(Account);
    }//AccountDetail()

    [ChildActionOnly]
    public String AccountDetailView([Bind(Prefix = "ID")] int AccountID = 0) {
      if (!exLogic.User.hasAccess("ACCOUNT.VIEW")) return "Access Denied";
     

       String SQL = @"SELECT
       [Name]
      ,[EmailId] AS [Email ID]
      ,[MobileNo] as [Mobile No.]
      ,[OfficeNo] as [Office No.]    
      ,[ContactName] as [Contact Person]     
        FROM [MSTR_Account] WHERE AccountID=" + AccountID; ;

        qDetailView nView = new qDetailView(SQL);
      return nView.getTable();
    }



    public String AccountDelete([Bind(Prefix = "ID")]int AccountID = 0) {
      if (!exLogic.User.hasAccess("ACCOUNT.DELETE"))

        return Util.jsonStat("ERROR", "Access Denied");
      String SQL = "";
      Response.ContentType = "text/json";

      //Delete the Account from database if there is no Drone createdby
      SQL = "SELECT Count(*) FROM MSTR_Drone where AccountId = " + AccountID;

      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the Account Attached Drone");
      //attached to parts
      SQL = "select Count(*) from MSTR_Parts where SupplierId =" + AccountID;
      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the the Account Attached to Parts");
      //attached to user
      SQL = "select Count(*) from MSTR_User where AccountId=" + AccountID;

      if (Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a the Account Attached to User");

      SQL = "DELETE FROM [MSTR_Account] WHERE  AccountId = " + AccountID;
      Util.doSQL(SQL);

      return Util.jsonStat("OK");
    }

    public ActionResult SMS() {
      if (!exLogic.User.hasAccess("ADMIN.SMS")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Mode = "edit";
      ViewBag.SMS = getSMS();
      return View();
    }

    private String getSMS() {
      if (!exLogic.User.hasAccess("ADMIN.SMS")) return "Access Denied";
      String SMS = "";
      String SQL = "SELECT CellNumber FROM SMSTable";
      var Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        if (!String.IsNullOrEmpty(SMS)) SMS = SMS + Environment.NewLine;
        SMS = SMS + Row["CellNumber"];
      }
      return SMS;
    }

    [HttpPost]
    public ActionResult SMS(String CellNumber) {
      String SQL = "DELETE FROM SMSTable";
      Util.doSQL(SQL);
      String[] CellNumbers = CellNumber.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);
      foreach(String cell in CellNumbers) {
        SQL = "Insert into SMSTable(CellNumber) VALUES('" + cell.Trim() + "')";
        Util.doSQL(SQL);
      }

      ViewBag.Mode = "view";
      ViewBag.SMS = getSMS();
      return View();
    }

  }//class
}//namespace