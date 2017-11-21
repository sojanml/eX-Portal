using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class nocController : Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();
    public ViewModel.latlng somehting;
    // GET: NOC
    public ActionResult Register(int ID = 0) {
      ViewBag.DbErrors = "";

      //to create gcaapproval
      if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
        return RedirectToAction("NoAccess", "Home");

      var NOC = new Models.MSTR_NOC();
      if(ID != 0) {
        NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == ID).FirstOrDefault();
      }

      if(NOC.NOC_Details.Count == 0) NOC.NOC_Details.Add(new Models.NOC_Details() {
        StartDate = DateTime.Now,
        EndDate = DateTime.Now,
        StartTime = new TimeSpan(6,0,0),
        EndTime = new TimeSpan(18, 0, 0),
        MaxAltitude = 40,
        MinAltitude = 0
      });
      return View(NOC);
    }

    [HttpPost]
    public ActionResult Register(Models.MSTR_NOC NOC) {
      StringBuilder SB = new StringBuilder();
      ViewBag.DbErrors = "";
      NOC.CreateBy = Util.getLoginUserID();
      NOC.CreatedOn = DateTime.UtcNow;
      NOC.NocName = NOC.FlightType + " for " + NOC.FlightFor;

      foreach (var noc in NOC.NOC_Details) {
        noc.Status = "New";
        noc.StatusChangedBy = Util.getLoginUserID();
        noc.StatusChangedOn = DateTime.UtcNow;
        noc.OuterCoordinates = noc.Coordinates;
      }
      NOC.CreatedOn = DateTime.UtcNow;
      try {
        ctx.MSTR_NOC.Add(NOC);
        ctx.SaveChanges();
      } catch (DbEntityValidationException e) {
        foreach (var eve in e.EntityValidationErrors) {
          SB.AppendLine($"Entity of type '{eve.Entry.Entity.GetType().Name}' in state '{eve.Entry.State}'<br>");
          foreach (var ve in eve.ValidationErrors) {
            SB.AppendLine($" - '{eve.Entry.Entity.GetType().Name}' in state '{eve.Entry.State}'<br>");
          }
        }
      } catch (DbUpdateException e) {
        foreach (var eve in e.Entries) {
          SB.AppendLine($"Entity of type '{eve.Entity.GetType().Name}' in state '{eve.State}'<br>");
        }
      }

      if(SB.Length > 0) {
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
      return View(NOC);
    }

    [HttpGet]
    [ActionName("3D")]
    public ActionResult ViewNocApplication3D([Bind(Prefix = "ID")]int NocApplicationID = 0) {
      Models.MSTR_NOC NOC = ctx.MSTR_NOC.Where(w => w.NocApplicationID == NocApplicationID).FirstOrDefault();
      if (NOC == null)
        return HttpNotFound();
      return View(NOC);
    }



    public class NocZone
    {
      public String Coordinates { get; set; }
      public String Color { get; set; }
    }

    [HttpGet]
    public ActionResult NoFlyZone(){
      List<NocZone> AllZones = new List<NocZone>();
      var FixedZones = ctx.MSTR_NoFlyZone
        .Where(w => w.DisplayType == "Fixed" && w.IsDeleted == false)
        .Select(s => new NocZone{
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
        select new NocZone{
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
        "&path=" + HttpUtility.UrlEncode("color:0xff0000|weight:5|enc:") + Util.gEncode(OuterCoordinates);

      return Content(GoogleMapURL);
    }

      
        public ActionResult Index()
        {
            //if (!exLogic.User.hasAccess("DRONE"))
            //    return RedirectToAction("NoAccess", "Home");
            string style= "'<span style=\"color:red\"> '+";
            String SQL = $@"SELECT  
                            [MSTR_Account].[name],
                            Firstname + ' ' + MiddleName + ' ' + LastName as Createdby,
                            [NocName],
                            [StartDate],
                            [EndDate],
                            [CountTotal] as [Total],
	                        CAST([CountApproved] as varchar(10))+  ' Approved'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountRejected] as varchar(20)) +' Rejected'
	                        + CHAR(13) + CHAR(10)+
                            CAST([CountAmended] as varchar(20)) +' Amended'
	                        +  CHAR(13) + CHAR(10)+ 
                            CAST([CountNew] as varchar(20)) +' New' +'' as Status,
                            Count(*) Over() as _TotalRecords,
                            [NocApplicationID] as _PKey
                            FROM[DCAAPortal].[dbo].[MSTR_NOC]
                            LEFT OUTER JOIN MSTR_Account
                            ON MSTR_Account.AccountId = [MSTR_NOC].AccountID
                            LEFT OUTER JOIN MSTR_User
                            ON MSTR_User.UserId=[CreateBy]";
            NOC_QView nView = new NOC_QView(SQL);
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("NOC_qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }
    }//public class nocController
}//namespace eX_Portal.Controllers