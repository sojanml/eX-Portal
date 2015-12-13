using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class AdminController : Controller {
    // GET: Admin
    public ActionResult Index() {
      return View();
    }//function index()

    public ActionResult Account() {
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
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

  }//class
}//namespace