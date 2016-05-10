using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class EmailController : Controller {
    private ExponentPortalEntities ctx = new ExponentPortalEntities();
    // GET: Email
    public ActionResult Index() {
      return View();
    }
    public ActionResult ForgotPassword(
      [Bind(Prefix ="ID")] int UserID = 0,
      String NewPassword = "null"
    ) {
      var User = ctx.MSTR_User.Find(UserID);

      ViewBag.Title = "Recover Password";
      ViewBag.NewPassword = NewPassword;
      return View(User);
    }
  }
}