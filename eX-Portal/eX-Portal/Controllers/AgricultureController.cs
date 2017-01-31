using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace eX_Portal.Controllers {

  public class JsonAgriTraxImage : AgriTraxImage {
    public System.String Status { get; set; }
  }

  public class AgricultureController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Agriculture
    public ActionResult Index() {
      return View();
    }

    [HttpGet]
    public ActionResult Create() {
      ViewData["InspectionOfficer"] = (
        from Officer in (
          db.AgriTraxManagements.Select(e => e.InspectionOfficer).Distinct()
        )
        select new SelectListItem {
          Selected = false,
          Text = Officer,
          Value = Officer
        }
        ).ToList();
      ViewData["LoanOfficer"] = (
        from Officer in (
          db.AgriTraxManagements.Select(e => e.LoanOfficer).Distinct()
        )
        select new SelectListItem {
          Selected = false,
          Text = Officer,
          Value = Officer
        }
        ).ToList();
      return View();
    }
    [HttpPost]
    public JsonResult Create(AgriTraxManagement AgriData) {
      //ModelState.AddModelError("LandAddress", "Land Address generate error");
      if (!ModelState.IsValid) {
        // do something to display errors .  
        var errorList = ModelState.Values
          .SelectMany(m => m.Errors)          
          .Select(e => e.ErrorMessage)
          .ToList();
        return Json(
          new {
            Status = "error",
            Message = errorList
          }, JsonRequestBehavior.AllowGet
        );
      }

      AgriData.FacilityType = "Agriculture";
      db.AgriTraxManagements.Add(AgriData);
      db.SaveChanges();

      return Json(
        new {
          Status = "ok",
          Message = "Created Successfully...",
          ID = AgriData.AgriTraxID
        }, JsonRequestBehavior.AllowGet
      );
    }



    [HttpGet]
    public ActionResult Images(int ID = 0) {
      var Customer = db.AgriTraxManagements.Where(e => e.AgriTraxID == ID).FirstOrDefault();
      if (Customer == null) return new HttpNotFoundResult();
      return View(Customer);
    }


    [HttpPost]
    public JsonResult UploadImage(int ID = 0) {
      try {
        var Stat = new AgriTraxImage();

        long TimeInSeconds = 0;
        long.TryParse(Request["CreatedDate"], out TimeInSeconds);
        DateTime CreatedDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)
          .AddMilliseconds(TimeInSeconds)
          .ToLocalTime();

        var ImageFile = Request.Files[0];
        Stat.ImageFile = ImageFile.FileName;
        var ImagePath = Server.MapPath($"/Upload/Agriculture/{ID}");
        var ImageFilePath = System.IO.Path.Combine(ImagePath, ImageFile.FileName);
        if (!System.IO.Directory.Exists(ImagePath)) System.IO.Directory.CreateDirectory(ImagePath);
        ImageFile.SaveAs(ImageFilePath);

        var MyLib = new exLogic.ExifLib(ImageFilePath);
        var GPS = MyLib.getGPS();
        Stat.Thumbnail = System.IO.Path.GetFileName(MyLib.setThumbnail(80, "t"));
        MyLib.setThumbnail(400, "m");
        Stat.Lat = (decimal)GPS.Latitude;
        Stat.Lng = (decimal)GPS.Longitude;
        Stat.CreatedOn = DateTime.Now;
        Stat.ImageDateTime = CreatedDate;
        Stat.AgriTraxID = ID;

        db.AgriTraxImages.Add(Stat);
        db.SaveChanges();

      } catch(Exception e) {
        var ErrorMsg = new {
          Status = "error",
          Message = e.Message
        };
        return Json(ErrorMsg, JsonRequestBehavior.AllowGet);
      }

      return MapLocation(ID, true);
    }

    public JsonResult MapImage(int ID = 0, int ImageID = 0,
      String Process = "", Double Lat = 0, Double Lng = 0) {
      var Row = db.AgriTraxImages
                      .Where(e => e.AgriTraxID == ID && e.AgriTraxImageID == ImageID);
      var ThisRec = Row.FirstOrDefault();
      if(ThisRec != null) { 
        switch(Process) {
        case "delete":
          db.AgriTraxImages.Remove(ThisRec);
          db.SaveChanges();
          break;
        case "location":
          ThisRec.Lat = (decimal)Lat;
          ThisRec.Lng = (decimal)Lng;
          db.Entry(ThisRec).State = EntityState.Modified;
          db.SaveChanges();
          break;
        }        
      }


      return MapLocation(ID, true);
    }

    public JsonResult MapLocation(int ID = 0, bool isUpdateCenter = false) {
      var AllImages = db.AgriTraxImages.Where(e => e.AgriTraxID == ID);
      var AgriTrax = db.AgriTraxManagements.Where(e => e.AgriTraxID == ID).FirstOrDefault();

      if(isUpdateCenter) { 
        //get Lat and Lng
        var UpdateLatLng = AllImages
          .GroupBy(g => g.AgriTraxID)
          .Select(s => new {
            Lat = s.Average(e => e.Lat),
            Lng = s.Average(e => e.Lng)
          })
          .FirstOrDefault();
        AgriTrax.Lat = UpdateLatLng.Lat;
        AgriTrax.Lng = UpdateLatLng.Lng;
        db.Entry(AgriTrax).State = EntityState.Modified;
        db.SaveChanges();
      }

      var SuccessMsg = new {
        Status = "ok",
        Message = "Success Reading",
        Location = AgriTrax,
        Images = AllImages.OrderBy(e => e.ImageDateTime).ToList()
      };
      return Json(SuccessMsg, JsonRequestBehavior.AllowGet);
    }

    


    public JsonResult Data(ViewModel.AgricultureFilter Filter) {

      var List = Filter.SetFilter(db);

      return Json(List.ToList(), JsonRequestBehavior.AllowGet);
    }
  }
}