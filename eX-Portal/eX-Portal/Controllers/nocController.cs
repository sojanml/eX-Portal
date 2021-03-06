﻿using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {

  public class nocController : Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();


    [HttpGet]
    public ActionResult Process(int ID = 0) {
      Models.MSTR_NOC NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == ID).FirstOrDefault();
      if (NOC == null)
        return HttpNotFound();
      ViewModel.MSTR_NOC_Ext _noc = new ViewModel.MSTR_NOC_Ext(NOC);
      return View(_noc);
    }

    [HttpPost]
    public ActionResult Process(int ID, List<NOC_Details> NocDetails) {
      Models.MSTR_NOC NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == ID).FirstOrDefault();
      if (NOC == null)
        return HttpNotFound();

      foreach (Models.NOC_Details Details in NocDetails) {
        NOC_Details dbNocDetails = ctx.NOC_Details.Where(w => w.NocID == Details.NocID).FirstOrDefault();
        if (dbNocDetails != null) {
          dbNocDetails.MaxAltitude = Details.MaxAltitude;
          dbNocDetails.NocBuffer = Details.NocBuffer;
          dbNocDetails.Status = Details.Status.Trim();
          dbNocDetails.StatusChangedBy = Util.getLoginUserID();
          dbNocDetails.StatusChangedOn = DateTime.UtcNow;
          dbNocDetails.Coordinates = Details.Coordinates;
          ctx.Entry(dbNocDetails).State = System.Data.Entity.EntityState.Modified;
          ctx.SaveChanges();
        }
      }

      NOC.StartDate = NOC.NOC_Details.Min(m => m.StartDate);
      NOC.EndDate = NOC.NOC_Details.Max(m => m.EndDate);
      NOC.CountAmended = NOC.NOC_Details.Where(w => w.Status == "Amended").Count();
      NOC.CountApproved = NOC.NOC_Details.Where(w => w.Status == "Approved").Count();
      NOC.CountRejected = NOC.NOC_Details.Where(w => w.Status == "Rejected").Count();
      NOC.CountNew = NOC.NOC_Details.Where(w => w.Status == "New").Count();

      ctx.Entry(NOC).State = System.Data.Entity.EntityState.Modified;
      ctx.SaveChanges();

      return RedirectToAction("View", new { ID = ID });
    }

    // GET: NOC
    public ActionResult Register(int ID = 0) {
      ViewBag.DbErrors = "";

      //to create gcaapproval
      if (!exLogic.User.hasAccess("FLIGHT.SETUP") && !exLogic.User.hasAccess("NOC.REGISTER"))
        return RedirectToAction("NoAccess", "Home");

      var NOC = new Models.MSTR_NOC();
      if (ID != 0) {
        NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == ID).FirstOrDefault();
      }

      if (NOC.NOC_Details.Count == 0)
        NOC.NOC_Details.Add(new Models.NOC_Details() {
          StartDate = DateTime.Now,
          EndDate = DateTime.Now,
          StartTime = new TimeSpan(6, 0, 0),
          EndTime = new TimeSpan(18, 0, 0),
          MaxAltitude = 40,
          MinAltitude = 0
        });
      return View(NOC);
    }

    [HttpPost]
    public async Task<ActionResult> Register(Models.MSTR_NOC NOC) {
      StringBuilder SB = new StringBuilder();
      ViewBag.DbErrors = "";
      NOC.CreateBy = Util.getLoginUserID();
      NOC.CreatedOn = DateTime.UtcNow;
      if (NOC.FlightType == "0")
        NOC.FlightType = Request.Form["FlightTypeOther"];
      NOC.NocName = NOC.FlightType + " for " + NOC.FlightFor;
      NOC.AccountID = Util.getAccountID();
      foreach (var noc in NOC.NOC_Details) {
        noc.Status = "New";
        if (noc.PilotID == 0)
          noc.PilotID = Util.getLoginUserID();
        noc.StatusChangedBy = Util.getLoginUserID();
        noc.StatusChangedOn = DateTime.UtcNow;
        noc.OuterCoordinates = noc.Coordinates;
        noc.NocBuffer = 50;
      }
      NOC.CreatedOn = DateTime.UtcNow;

      //find out total count and update the master
      var NocCount = NOC.NOC_Details.Count();
      NOC.CountTotal = NocCount;
      NOC.CountNew = NocCount;
      NOC.CountApproved = 0;
      NOC.CountRejected = 0;
      NOC.CountAmended = 0;
      NOC.StartDate = NOC.NOC_Details.Min(m => m.StartDate);
      NOC.EndDate = NOC.NOC_Details.Max(m => m.EndDate);

      try {
        ctx.MSTR_NOC.Add(NOC);
        await ctx.SaveChangesAsync();
      } catch (DbEntityValidationException e) {
        foreach (var eve in e.EntityValidationErrors) {
          SB.AppendLine($"Entity of type '{eve.Entry.Entity.GetType().Name}' in state '{eve.Entry.State}'<br>");
          foreach (var ve in eve.ValidationErrors) {
            SB.AppendLine($" - '{ve.PropertyName}' -> '{ve.ErrorMessage}'<br>");
          }
        }
      } catch (DbUpdateException e) {
        foreach (var eve in e.Entries) {
          SB.AppendLine($"Entity of type '{eve.Entity.GetType().Name}' in state '{eve.State}'<br>");
        }
      }

      if (SB.Length > 0) {
        ViewBag.DbErrors = SB.ToString();
        return View(NOC);
      }
      return RedirectToAction("View", new { ID = NOC.NocApplicationID });
    }//public ActionResult Register(Models.MSTR_NOC NOC)

    [HttpGet]
    [ActionName("View")]
    public ActionResult ViewNocApplication([Bind(Prefix = "ID")]int NocApplicationID = 0) {
      Models.MSTR_NOC NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == NocApplicationID).FirstOrDefault();
      if (NOC == null)
        return HttpNotFound();
      return View(new ViewModel.MSTR_NOC_Ext(NOC));
    }

    [HttpGet]
    [ActionName("3D")]
    public ActionResult ViewNocApplication3D([Bind(Prefix = "ID")]int NocApplicationID = 0) {
      Models.MSTR_NOC NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == NocApplicationID).FirstOrDefault();
      if (NOC == null)
        return HttpNotFound();
      return View(NOC);
    }

    public ActionResult ExtentCoordinates(String Coordinates, int Distance = 50) {
      String SQL = $"SELECT [dbo].[ExtentPolygon] ('{Coordinates}',{Distance})";
      String NewCoordinates = Util.getDBVal(SQL);
      ViewModel.DynamicZone Zone = new ViewModel.DynamicZone();
      Zone.setPath(NewCoordinates);
      return Json(Zone, JsonRequestBehavior.AllowGet);
    }

    public class NocZone {
      public String Coordinates { get; set; }
      public String Color { get; set; }
    }

    [HttpGet]
    public ActionResult NoFlyZone() {
      List<NocZone> AllZones = new List<NocZone>();
      var FixedZones = ctx.MSTR_NoFlyZone
        .Where(w => w.DisplayType == "Fixed" && w.IsDeleted == false)
        .Select(s => new NocZone {
          Coordinates = s.Coordinates,
          Color = s.FillColour
        });
      AllZones.AddRange(FixedZones.ToList());

      DateTime Today = DateTime.UtcNow.Date;
      TimeSpan nowTime = DateTime.UtcNow.AddHours(+4).TimeOfDay;
      var DynamicZone =
        from m in ctx.MSTR_NoFlyZone
        where
          m.DisplayType == "Dynamic" &&
          m.StartDate <= Today &&
          m.EndDate >= Today &&
          m.StartTime <= nowTime &&
          m.EndTime >= nowTime &&
          m.IsDeleted != true
        select new NocZone {
          Coordinates = m.Coordinates,
          Color = m.FillColour
        };
      AllZones.AddRange(DynamicZone.ToList());

      return Json(AllZones, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    [ChildActionOnly]
    public ActionResult StaticGoogleMap(int NocID = 0) {
      var NOC = ctx.NOC_Details.Where(w => w.NocID == NocID).FirstOrDefault();
      if (NOC == null)
        return Content("/images/world.jpg");

      var Coordinates = Util.ToGeoLocation(NOC.Coordinates);
      var OuterCoordinates = Util.ToGeoLocation(NOC.OuterCoordinates);
      String GoogleMapURL = "https://maps.googleapis.com/maps/api/staticmap" +
        "?key=AIzaSyDuaEF6XG32DPKX-r5BX_0hm4Q99iFlgAw" +
        "&size=567x300" +
        "&style=element:labels|visibility:off" +
        "&style=element:geometry.stroke|visibility:off" +
        "&style=feature:landscape|element:geometry|saturation:-50" +
        "&style=feature:water|saturation:-50|invert_lightness:true" +
        "&path=" + HttpUtility.UrlEncode("fillcolor:0x00ff00|weight:0|enc:") + Util.gEncode(Coordinates) +
        "&path=" + HttpUtility.UrlEncode("color:0xff0000ff|weight:1|enc:") + Util.gEncode(OuterCoordinates);

      return Content(GoogleMapURL);
    }

    [HttpGet]
    [ChildActionOnly]
    public ActionResult NOCDetail(int ID = 0) {
      var TheDetail = ctx.NOC_Details.Where(w => w.NocID == ID).FirstOrDefault();
      if (TheDetail == null) {
        var c = new ContentResult();
        c.Content = "404 - NOC Detail is not found.";
        return c;
      }
      return View(new ViewModel.NOC_Details_Ext(TheDetail));
    }

    [HttpPost]
    public async Task<ActionResult> Billing(BillingModule.BillingNOC noc) {
      var BillingGroupID = Util.GetBillingGroupID();

      BillingModule.BillingGroup group = new BillingModule.BillingGroup(BillingGroupID);
      await noc.GenerateFields();
      var Billing = await group.GenerateBilling(noc);
      return Json(Billing, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public async Task<ActionResult> Estimate(BillingModule.BillingNOC noc) {
      var BillingGroupID = Util.GetBillingGroupID();
      BillingModule.BillingGroup group = new BillingModule.BillingGroup(BillingGroupID);
      await noc.GenerateFields();
      var Billing = await group.GenerateEstimate(noc);
      return Json(Billing, JsonRequestBehavior.AllowGet);
    }


    public ActionResult Index() {
      //if (!exLogic.User.hasAccess("NOC"))
      //    return RedirectToAction("NoAccess", "Home");
      String SQL = "";

      if (exLogic.User.hasAccess("DRONE.MANAGE")) {
                SQL = $@"SELECT  
                            [NocApplicationID] ,
                            [MSTR_Account].[name] as [Organization],
                            Firstname + ' ' + MiddleName + ' ' + LastName as [User],
                            [NOCName],
                            [StartDate],
                            [EndDate],
	                        CAST([CountApproved] as varchar(10))+  ' Approved'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountRejected] as varchar(20)) +' Rejected'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountAmended] as varchar(20)) +' Amended'
	                        +  CHAR(13) + CHAR(10)+ 
                            CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                            Count(*) Over() as _TotalRecords,
                            [NocApplicationID] as _PKey
                            FROM [MSTR_NOC]
                            LEFT OUTER JOIN MSTR_Account
                            ON MSTR_Account.AccountId = [MSTR_NOC].AccountID
                            LEFT OUTER JOIN MSTR_User
                            ON MSTR_User.UserId=[CreateBy]";
                     //       order by MSTR_NOC.NocApplicationID";
                          //  where enddate>=getdate() ";
      } else if(exLogic.User.hasAccess("ORGANIZATION.ADMIN")) {
        int Accountid = Util.getAccountID();
        SQL = $@"SELECT  
                            [NocApplicationID] ,
                            Firstname + ' ' + MiddleName + ' ' + LastName as [User],
                            [NOCName],
                            [StartDate],
                            [EndDate],
	                        CAST([CountApproved] as varchar(10))+  ' Approved'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountRejected] as varchar(20)) +' Rejected'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountAmended] as varchar(20)) +' Amended'
	                        +  CHAR(13) + CHAR(10)+ 
                            CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                            Count(*) Over() as _TotalRecords,
                            [NocApplicationID] as _PKey
                            FROM [MSTR_NOC]
                            LEFT OUTER JOIN MSTR_User
                            ON MSTR_User.UserId=[CreateBy]
                            where  [MSTR_NOC].Accountid= " + Accountid;

      }
      else
        {
                int Userid = Util.getLoginUserID();
            SQL = $@"SELECT   NocApplicationID,
                        [NOCName],
                        [StartDate],
                        [EndDate],
	                    CAST([CountApproved] as varchar(10))+  ' Approved'
	                    + CHAR(13) + CHAR(10)+
                        CAST([CountRejected] as varchar(20)) +' Rejected'
	                    + CHAR(13) + CHAR(10)+
                        CAST([CountAmended] as varchar(20)) +' Amended'
	                    +  CHAR(13) + CHAR(10)+ 
                        CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                        Count(*) Over() as _TotalRecords,
                        [NocApplicationID] as _PKey
                        FROM [MSTR_NOC]
                        LEFT OUTER JOIN MSTR_User
                        ON MSTR_User.UserId=[CreateBy]
                        where [MSTR_NOC].[CreateBy]= " + Userid;
        }



      NOC_QView nView = new NOC_QView(SQL);
      if (exLogic.User.hasAccess("NOC.VIEW"))
        nView.addMenu("View", Url.Action("View", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("NOC.PROCESS")) {
        nView.addMenu("Process", Url.Action("Process", new { ID = "_Pkey" }));
      }
      /*
      if (exLogic.User.hasAccess("NOC.3D")) {
        nView.addMenu("3D", Url.Action("3D", new { ID = "_Pkey" }));
      }
      */
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("NOC_qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult History() {
      //if (!exLogic.User.hasAccess("NOC"))
      //    return RedirectToAction("NoAccess", "Home");
      String SQL = "";

      if (exLogic.User.hasAccess("DRONE.MANAGE")) {
        SQL = $@"SELECT  
                            [MSTR_Account].[name] as [Organization],
                            Firstname + ' ' + MiddleName + ' ' + LastName as [User],
                            [NOCName],
                            [StartDate],
                            [EndDate],
	                        CAST([CountApproved] as varchar(10))+  ' Approved'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountRejected] as varchar(20)) +' Rejected'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountAmended] as varchar(20)) +' Amended'
	                        +  CHAR(13) + CHAR(10)+ 
                            CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                            Count(*) Over() as _TotalRecords,
                            [NocApplicationID] as _PKey
                            FROM [MSTR_NOC]
                            LEFT OUTER JOIN MSTR_Account
                            ON MSTR_Account.AccountId = [MSTR_NOC].AccountID
                            LEFT OUTER JOIN MSTR_User
                            ON MSTR_User.UserId=[CreateBy]
                            where enddate<getdate() ";
      } else {
        int Accountid = Util.getAccountID();
        SQL = $@"SELECT  
                            Firstname + ' ' + MiddleName + ' ' + LastName as [User],
                            [NOCName],
                            [StartDate],
                            [EndDate],
	                        CAST([CountApproved] as varchar(10))+  ' Approved'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountRejected] as varchar(20)) +' Rejected'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountAmended] as varchar(20)) +' Amended'
	                        +  CHAR(13) + CHAR(10)+ 
                            CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                            Count(*) Over() as _TotalRecords,
                            [NocApplicationID] as _PKey
                            FROM [MSTR_NOC]
                            LEFT OUTER JOIN MSTR_User
                            ON MSTR_User.UserId=[CreateBy]
                            where enddate<getdate() and [MSTR_NOC].Accountid= " + Accountid;

      }


      NOC_QView nView = new NOC_QView(SQL);
      if (exLogic.User.hasAccess("NOC.VIEW"))
        nView.addMenu("View", Url.Action("View", new { ID = "_Pkey" }));

      if (exLogic.User.hasAccess("NOC.3D")) {
        nView.addMenu("3D", Url.Action("3D", new { ID = "_Pkey" }));
      }
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("NOC_qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

  }//public class nocController
}//namespace eX_Portal.Controllers