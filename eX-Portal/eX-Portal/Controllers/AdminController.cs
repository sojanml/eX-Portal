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
      if (exLogic.User.hasAccess("ACCOUNT.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("ACCOUNT.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Account()

    public ActionResult AccountCreate() {
      ViewBag.Title = "Create Account";
      return View();
    }

    [HttpPost]
    public ActionResult AccountCreate(MSTR_Account Account) {
      if (ModelState.IsValid) {
        Account.CreatedBy = Util.toInt(Session["UserID"].ToString());
        Account.CreatedOn = DateTime.Now;
        db.MSTR_Account.Add(Account);
        db.SaveChanges();
        return RedirectToAction("AccountDetail", new { id = Account.AccountId});
      }
      return View(Account);
    }

    public ActionResult AccountEdit() {
      ViewBag.Title = "Edit Account";
      return View();
    }

    [HttpPost]
    public ActionResult AccountEdit(Models.MSTR_Account Account ) {
      ViewBag.Title = "Edit Account";
      try {
        if (ModelState.IsValid) {
          Account.ModifiedBy = Util.toInt(Session["UserID"].ToString());
          Account.ModifiedOn = DateTime.Now;
          db.Entry(Account).Property(x => x.CreatedBy).IsModified = false;
          db.Entry(Account).Property(x => x.CreatedOn).IsModified = false;
          db.MSTR_Account.Add(Account);
          db.SaveChanges();
          return RedirectToAction("AccountDetail");
        }
      } catch (Exception  ex) {
        //Log the error (uncomment dex variable name and add a line here to write a log.
        return View("InternalError", ex);
      }
      return View(Account);
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

  }//class
}//namespace