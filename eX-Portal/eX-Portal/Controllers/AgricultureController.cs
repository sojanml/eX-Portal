﻿using eX_Portal.Models;
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


    [HttpGet]
    public ActionResult BulkUpload() {
      return View();
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
        Stat.CreatedBy = exLogic.Util.getLoginUserID();
        Stat.AgriTraxGroupID = 0;
        db.AgriTraxImages.Add(Stat);
        db.SaveChanges();

      } catch(Exception e) {
        var ErrorMsg = new {
          Status = "error",
          Message = e.Message
        };
        return Json(ErrorMsg, JsonRequestBehavior.AllowGet);
      }

      if(ID > 0) { 
        return MapLocation(ID, true);
      } else {
        return MapBulkUpload(true);
      }
    }


    public JsonResult MapBulkUpload(bool IsSetGroup = false) {
      int CreatedBy = exLogic.Util.getLoginUserID();
      int AgriTraxGroupID = 0;
      //1. Find all images uploaded by the user order by date
      var QueryImages = from i in db.AgriTraxImages
                        where i.CreatedBy == CreatedBy && i.AgriTraxID == 0
                        orderby i.ImageDateTime
                        select i;
      IList<AgriTraxImage> Images = QueryImages.ToList();


      //2. For each record, setup the Group ID if the time is more than 5 min   
      if(IsSetGroup) { 
        DateTime ImageCheckDate = DateTime.MinValue;         
        foreach (var Image in Images) {
          if (ImageCheckDate == DateTime.MinValue) { 
            ImageCheckDate = (DateTime)Image.ImageDateTime;
            AgriTraxGroupID++;
          } else { 
            var DateDiff = (DateTime)Image.ImageDateTime - ImageCheckDate;
            if(DateDiff.TotalMinutes > 5) AgriTraxGroupID++;
            ImageCheckDate = (DateTime)Image.ImageDateTime;
          }
          //update Group Index
          Image.AgriTraxGroupID = AgriTraxGroupID;
          db.Entry(Image).State = EntityState.Modified;
          
        }
        //3. Save changes to database together
        db.SaveChanges();
      }

      //4. Return the Images to the browser
      var SuccessMsg = new {
        Status = "ok",
        Message = "Success Reading",
        Location = new {Lat = 0, Lng = 0 },
        Images = Images
      };
      return Json(SuccessMsg, JsonRequestBehavior.AllowGet);
    }


    public JsonResult AssignCustomer(int AgriTraxID = 0, int AgriTraxGroupID = 0) {
      int CreatedBy = exLogic.Util.getLoginUserID();
      String SourcePath = Server.MapPath("/Upload/Agriculture/0");
      String DestPath = Server.MapPath($"/Upload/Agriculture/{AgriTraxID}");
      //1. Find all images uploaded by the user order by date
      var QueryImages = 
        from i in db.AgriTraxImages
        where 
          i.CreatedBy == CreatedBy && 
          i.AgriTraxID == 0 &&
          i.AgriTraxGroupID == AgriTraxGroupID
        orderby i.ImageDateTime
        select i;
      IList<AgriTraxImage> Images = QueryImages.ToList();

      //2. Find the Maximum of GroupID for the customer
      int? NextAgriTraxGroupID = db.AgriTraxImages
        .Where(i => i.CreatedBy == CreatedBy && i.AgriTraxID == AgriTraxID)
        .Max(i => i.AgriTraxGroupID);
      if (NextAgriTraxGroupID == null) NextAgriTraxGroupID = 0;
      NextAgriTraxGroupID++;

      //3. Move the images to new AgriTraxID Folder and
      //   update the database
      foreach (var Image in Images) {
        String[] SouceImages = {
          Image.ImageFile,
          Image.Thumbnail,
          Image.Thumbnail.Replace(".t.png", ".m.png")
        };

        if (!System.IO.Directory.Exists(DestPath))
          System.IO.Directory.CreateDirectory(DestPath);

        foreach (var ImageName in SouceImages) {
          String sPath = System.IO.Path.Combine(SourcePath, ImageName);
          String dPath = System.IO.Path.Combine(DestPath, ImageName);
          try { 
            System.IO.File.Move(sPath, dPath);
          } catch {
            //skip the error
          }
        }
        //update Group Index
        Image.AgriTraxGroupID = NextAgriTraxGroupID;
        Image.AgriTraxID = AgriTraxID;
        db.Entry(Image).State = EntityState.Modified;
      }
      db.SaveChanges();

      var SuccessMsg = new {
        Status = "ok",
        Message = "Moved successfully"
      };
      return Json(SuccessMsg, JsonRequestBehavior.AllowGet);
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


      if (ID > 0) {
        return MapLocation(ID, true);
      } else {
        return MapBulkUpload(true);
      }
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