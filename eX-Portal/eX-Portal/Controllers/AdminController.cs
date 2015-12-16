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
            var AccountModel = new ViewModel.AccountViewModel
            {
                Account = new MSTR_Account(),
                CountryList = Util.GetCountryLists("Country", "DroneName", "Code", "usp_Portal_DroneServiceType"),
               

            };


            return View(AccountModel);
    }

    [HttpPost]
    public ActionResult AccountCreate(MSTR_Account Account) {
            if (!exLogic.User.hasAccess("ACCOUNT.CREATE")) return RedirectToAction("NoAccess", "Home");
            if (ModelState.IsValid) {

                if (Session["UserId"] == null)
                {
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
        return RedirectToAction("AccountDetail", new { id = Account.AccountId});
      }

            var viewModel = new ViewModel.AccountViewModel
            {
                Account = new MSTR_Account(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),



            };
            return View(viewModel);
          
    }

    public ActionResult AccountEdit(int  id) {
      
      ViewBag.Title = "Edit Account";
            if (!exLogic.User.hasAccess("ACCOUNT.EDIT")) return RedirectToAction("NoAccess", "Home");

            var viewModel = new ViewModel.AccountViewModel
            {



                Account = db.MSTR_Account.Find(id),

                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
            };
            return View(viewModel);
           
    }

    [HttpPost]
    public ActionResult AccountEdit(Models.MSTR_Account Account ) {
      ViewBag.Title = "Edit Account";
            if (!exLogic.User.hasAccess("ACCOUNT.EDIT")) return RedirectToAction("NoAccess", "Home");

            try
            {
        if (ModelState.IsValid) {

                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }
                    Account.ModifiedBy = Util.toInt(Session["UserID"].ToString());
          Account.ModifiedOn = DateTime.Now;

                string    SQL = "Update Mstr_Account set Name='" + Account.Name + "',Code='"
                                   + Account.Code + "',EmailId='" + Account.EmailId + "',MobileNo='"
                                   + Account.MobileNo + "',OfficeNo='" + Account.OfficeNo + "',IsActive='" + Account.IsActive + "',ModifiedBy=" + Session["UserId"] +
                                   ",ModifiedOn='"+ DateTime.Now.ToString("yyyy - MM - dd")+ "', AccountDescription='"+ Account.AccountDescription +
                                   "',Address1='"+Account.Address1  +"', Address2='"+Account.Address2 + "' ,Address3='"+ Account.Address3 + 
                                   "',CountryCode="+ Account.CountryCode +" where AccountId=" + Account.AccountId;

                    int id = Util.doSQL(SQL);

                    /* db.Entry(Account).Property(x => x.CreatedBy).IsModified = false;
                       db.Entry(Account).Property(x => x.CreatedOn).IsModified = false;
                       db.MSTR_Account.Add(Account);
                       db.SaveChanges();*/
                    return RedirectToAction("Account");
        }
      } catch (Exception  ex) {
        //Log the error (uncomment dex variable name and add a line here to write a log.
        return View("InternalError", ex);
      }

            //for the server side validation
            var viewModel = new ViewModel.AccountViewModel
            {



                Account = db.MSTR_Account.Find(Account.AccountId),

                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
            };
            return View(viewModel);
          
    }//ActionEdit()

    public ActionResult AccountDetail([Bind(Prefix = "ID")] int AccountID) {
      Models.MSTR_Account Account = db.MSTR_Account.Find(AccountID);
      if(Account == null) return RedirectToAction("Error", "Home");
      ViewBag.Title = Account.Name;
      return View(Account);
    }//AccountDetail()

    [ChildActionOnly]
    public String AccountDetailView([Bind(Prefix = "ID")] int AccountID = 0) {
      String SQL = "SELECT * FROM MSTR_Account WHERE AccountID=" + AccountID;
      qDetailView nView = new qDetailView(SQL);
      return nView.getTable();
    }



        public String AccountDelete([Bind(Prefix = "ID")]int AccountID = 0)
        {
            if (!exLogic.User.hasAccess("ACCOUNT.DELETE"))

                return Util.jsonStat("ERROR", "Access Denied");
            String SQL = "";
            Response.ContentType = "text/json";            

            //Delete the Account from database if there is no Drone createdby
            SQL = "SELECT Count(*) FROM MSTR_Drone where AccountId = " + AccountID;

            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached Drone");

            SQL= "select Count(*) from MSTR_Parts where SupplierId =" + AccountID;
            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to Parts");

            SQL = "DELETE FROM [MSTR_Account] WHERE  AccountId = " + AccountID;
            Util.doSQL(SQL);

            return Util.jsonStat("OK");
        }

    }//class
}//namespace