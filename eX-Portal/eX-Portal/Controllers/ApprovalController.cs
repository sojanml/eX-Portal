using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class ApprovalController : Controller {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    // GET: Approval
    public ActionResult Index(int ID = 0) {
      if (!exLogic.User.hasAccess("DRONE.VIEW")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "View";
      string SQL = @"SELECT ";
      if (ID == 0) SQL += "   d.[DroneName],\n";
      SQL += @"g.[ApprovalName]
            ,g.[StartDate]
            ,g.[EndDate]
            ,g.[MinAltitude]
            ,g.[MaxAltitude]
            ,g.[BoundaryInMeters] as  Boundary
            ,Count(*) Over() as _TotalRecords 
            ,g.[ApprovalID] as _PKey
        from GCA_Approval g join mstr_drone d
        on g.droneID = d.droneID";

      String SQLWhere = "";
      if (ID > 0) SQLWhere += "g.DroneID =" + ID;
            if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
            {
                if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                {
                    if (SQLWhere != "") SQLWhere += " AND";
                    SQLWhere += " g.AccountID=" + Util.getAccountID();
                }
            }
      if (SQLWhere != "") SQL += " WHERE " + SQLWhere;

      qView nView = new qView(SQL);

      ViewBag.Title = "Regulatory Approval";
      ViewBag.DroneID = ID;
      ViewBag.Title += " [" + Util.getDroneName(ID) + "]";

      //if (exLogic.User.hasAccess("USER.VIEW")) nView.addMenu("Detail", Url.Action("UserDetail", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("DRONE.EDIT")) nView.addMenu("Edit", Url.Action("GCAApproval", "S3Upload", new { ID = "_PKey" }));
      if (exLogic.User.hasAccess("DRONE.DELETE")) nView.addMenu("Delete", Url.Action("DeleteGCAApproval", "S3Upload", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }



    protected override void Dispose(bool disposing) {
      if (disposing) {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
