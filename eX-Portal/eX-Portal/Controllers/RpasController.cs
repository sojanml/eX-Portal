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
using eX_Portal.ViewModel;
using FileStorageUtils;

namespace eX_Portal.Controllers {
  public class RpasController : Controller {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    public ActionResult Register() {

      var fileStorageProvider = new AmazonS3FileStorageProvider();

      var fileUploadViewModel = new S3Upload(
        fileStorageProvider.PublicKey,
        fileStorageProvider.PrivateKey,
        fileStorageProvider.BucketName,
        Url.Action("complete", "home", null, Request.Url.Scheme)
      );

      fileUploadViewModel.SetPolicy(
        fileStorageProvider.GetPolicyString(
          fileUploadViewModel.FileId,
          fileUploadViewModel.RedirectUrl
        )
      );


      ViewBag.FormAction = fileUploadViewModel.FormAction;
      ViewBag.FormMethod = fileUploadViewModel.FormMethod;
      ViewBag.FormEnclosureType = fileUploadViewModel.FormEnclosureType;
      ViewBag.AWSAccessKey = fileUploadViewModel.AWSAccessKey;
      ViewBag.Acl = fileUploadViewModel.Acl;
      ViewBag.Base64EncodedPolicy = fileUploadViewModel.Base64EncodedPolicy;
      ViewBag.Signature = fileUploadViewModel.Signature;

      Session["RegisterUserID"] = 77;
      int RegisterUserID = Util.toInt(Session["RegisterUserID"]);
      if (RegisterUserID <= 0) return View("NoAccess");

      var User = (
      from n in db.MSTR_User
      where n.UserId == RegisterUserID
      select new RPAS_Register {
        EmailId = n.EmailId,
        UserName = n.UserName,

        PhotoUrl = n.PhotoUrl,
        FirstName = n.FirstName,
        MiddleName = n.MiddleName,
        LastName = n.LastName,
        MobileNo = n.MobileNo,
        HomeNo = n.HomeNo,
        CountryId = (n.CountryId == null ? 0 : (int)n.CountryId),
        RPASPermitNo = n.RPASPermitNo,
        PermitCategory = n.PermitCategory,
        ContactAddress = n.ContactAddress,
        RegRPASSerialNo = n.RegRPASSerialNo,
        CompanyAddress = n.CompanyAddress,
        CompanyTelephone = n.CompanyTelephone,
        CompanyEmail = n.CompanyEmail,
        TradeLicenceCopyUrl = n.TradeLicenceCopyUrl,
        EmiratesID = n.EmiratesID
      }).FirstOrDefault();
      if (User == null) return View("NoAccess");

      return View(User);

    }

    [HttpPost]
    public ActionResult Register(RPAS_Register mSTR_User) {

      int RegisterUserID = Util.toInt(Session["RegisterUserID"]);
      var InternalInfo = (
        from n in db.MSTR_User
        where n.UserId == RegisterUserID
        select new {
          UserName = n.UserName,
          EmailId = n.EmailId
        }
      ).FirstOrDefault();
      mSTR_User.UserName = InternalInfo.UserName;
      mSTR_User.EmailId = InternalInfo.EmailId;

      ModelState.Remove("UserName");
      ModelState.Remove("EmailId");

      if (ModelState.IsValid) {
        string encryptedpasword = Util.GetEncryptedPassword(mSTR_User.Password);
        mSTR_User.Password = encryptedpasword;

        string updatesql = " update [ExponentPortal].[dbo].[MSTR_User]\n" +
        "set\n" +
        "  [Password]='" + mSTR_User.Password + "',\n" +
        "  [PhotoUrl]='" + mSTR_User.PhotoUrl + "',\n" +
        "  [FirstName] ='" + mSTR_User.FirstName + "',\n" +
        "  [MiddleName] ='" + mSTR_User.MiddleName + "',\n" +
        "  [LastName] ='" + mSTR_User.LastName + "',\n" +
        "  [LastModifiedBy] =" + Session["RegisterUserID"] + ",\n" +
        "  [MobileNo] ='" + mSTR_User.MobileNo + "',\n" +
        "  [HomeNo] ='" + mSTR_User.HomeNo + "',\n" +
        "  [CountryId] =" + mSTR_User.CountryId + ",\n" +
        "  [IsActive] =1,\n" +
        "  [LastModifiedOn] =GETDATE(),\n" +
        "  [RPASPermitNo] ='" + mSTR_User.RPASPermitNo + "',\n" +
        "  [PermitCategory] ='" + mSTR_User.PermitCategory + "',\n" +
        "  [ContactAddress] ='" + mSTR_User.ContactAddress + "',\n" +
        "  [RegRPASSerialNo] ='" + mSTR_User.RegRPASSerialNo + "',\n" +
        "  [CompanyAddress] ='" + mSTR_User.CompanyAddress + "',\n" +
        "  [CompanyTelephone] ='" + mSTR_User.CompanyTelephone + "',\n" +
        "  [CompanyEmail] ='" + mSTR_User.CompanyEmail + "',\n" +
        "  [TradeLicenceCopyUrl] ='" + mSTR_User.TradeLicenceCopyUrl + "',\n" +
        "  [EmiratesID] ='" + mSTR_User.EmiratesID + "'\n" +
        "where\n" +
        "  [UserId] =" + RegisterUserID;
        int result = Util.doSQL(updatesql);

        //Login the user to the system
        Session["UserID"] = RegisterUserID;
        Session["FirstName"] = mSTR_User.FirstName;
        Session["UserName"] = mSTR_User.UserName;

        return RedirectToAction("Index", "Home");
      }
      return View(mSTR_User);
    }
    public ActionResult NoAccess() {
      return View();
    }

    public ActionResult Login([Bind(Prefix = "ID")]int UserID = 0, int Force = 0) {
      //this option allow to register the user to the
      //exponent portal
      bool isPasswordSend = false;

      var UserInfo = (
        from n in db.MSTR_User
        where n.UserId == UserID
        select new {
          Password = n.Password,
          Mobile = n.MobileNo,
          FullName = n.FirstName
        }
      ).ToList();

      if (UserInfo.Count <= 0) {
        //nothing
      } else {
        var thisUser = UserInfo.First();
        if (String.IsNullOrEmpty(thisUser.Password) || Force == 1) {
          isPasswordSend = true;
          var NewPassword = Util.getNewPassword();
          var PasswordMD5 = Util.MD5(NewPassword);
          String Body = "Hi " + thisUser.FullName + ", Your password for " +
          "exponent is " + NewPassword;
          Util.SMSQue(UserID, thisUser.Mobile, Body);

          //Save the password to the database, so it can be used to login
          String SQL = "UPDATE MSTR_User Set GeneratedPassword='" + PasswordMD5 + "' " +
          "WHERE UserID=" + UserID;
          Util.doSQL(SQL);
          isPasswordSend = true;
        }
      }

      ViewBag.isPasswordSend = isPasswordSend;
      return View();
    }

    [HttpPost]
    public JsonResult Login(String UserName = "", String Password = "") {
      //UserName = Request["UserName"];
      //Password = Request["Password"];
      var theResult = new {
        Status = "Error",
        Message = "Username or Password does not match. Please Try again."
      };
      String PasswordMD5 = Util.MD5(Password);
      //Check 1: Is username and password match the login
      var UserInfo = (
        from n in db.MSTR_User
        where (n.UserName == UserName ||
        n.EmailId == UserName) &&
        n.GeneratedPassword == PasswordMD5
        select new {
          UserID = n.UserId,
          Mobile = n.MobileNo,
          FullName = n.FirstName
        }
      ).ToList();
      if (UserInfo.Count < 1) return Json(theResult);

      //Setp 2: If the user is found, then redirect the user to 
      //        Registration page
      Session["RegisterUserID"] = UserInfo.First().UserID;
      theResult = new {
        Status = "OK",
        Message = "User logged in successfully. Contine to register..."
      };
      return Json(theResult);

    }

    // GET: Rpas
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("RPAS.VIEW")) return RedirectToAction("NoAccess", "Home");
      string SQL = "SELECT MSTR_RPAS_User.Name as [FullName],\n" +
                   "LUP_Drone.Name AS Nationality,\n" +
                   "MSTR_RPAS_User.EmiratesId as [EmiratesID],\n" +
                   "MSTR_RPAS_User.EmailId as [Email],\n" +
                   "MSTR_RPAS_User.MobileNo as [MobileNo],\n" +
                   "MSTR_RPAS_User.Status,\n" +
                   "Count(*) Over() as _TotalRecords,\n" +
                   "RpasId as _PKey\n" +
                   "FROM MSTR_RPAS_User INNER JOIN LUP_Drone\n" +
                   "ON MSTR_RPAS_User.NationalityId = LUP_Drone.TypeId\n" +
                   "where LUP_Drone.Type = 'Country' and (MSTR_RPAS_User.Status='New User Request' or MSTR_RPAS_User.Status='User Created')";

      qView nView = new qView(SQL);
      //if (exLogic.User.hasAccess("PILOTLOG.VIEW"))
      nView.addMenu("Create User", Url.Action("Create", "User", new { ID = "_PKey" }));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
       //return View(db.MSTR_RPAS_User.ToList());
    }

    // GET: Rpas/Details/5
    public ActionResult Details(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
      if (mSTR_RPAS_User == null) {
        return HttpNotFound();
      }
      return View(mSTR_RPAS_User);
    }

    // GET: Rpas/Create
    public ActionResult Create() {
      if (!exLogic.User.hasAccess("RPAS.CREATE")) return RedirectToAction("NoAccess", "Home");
      return View();
    }

    // POST: Rpas/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public ActionResult Create(MSTR_RPAS_User mSTR_RPAS_User) {
      if (!exLogic.User.hasAccess("RPAS.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (mSTR_RPAS_User.NationalityId == null) ModelState.AddModelError("NationalityId", "Please select Nationality.");
      if (mSTR_RPAS_User.EmailId == null) {
        ModelState.AddModelError("EmailId", "Please enter Email Id.");
      } else {
        string sqlmailcheck = "select EmailId from MSTR_RPAS_User where [EmailId] ='" + mSTR_RPAS_User.EmailId.ToString() + "'";
        var Row = Util.getDBRow(sqlmailcheck);
        if (Row.Count > 1) {
          if (Row["EmailId"].ToString() == mSTR_RPAS_User.EmailId) {
            ViewBag.message = "Registeration for this user is already done!!";
            return View(mSTR_RPAS_User);
          } else { }
        } else { }
      }
      if (ModelState.IsValid) {
        mSTR_RPAS_User.Status = "New User Request";
        mSTR_RPAS_User.CreatedBy = Convert.ToInt32(Session["UserId"].ToString());
        mSTR_RPAS_User.CreatedOn = System.DateTime.Now;
        db.MSTR_RPAS_User.Add(mSTR_RPAS_User);
        db.SaveChanges();
        int id = mSTR_RPAS_User.RpasId;
        var mailurl = "~/Email/RPASRegEmail/" + id;  //"~/"+Url.Action("RPASRegEmail", "Email", new { id = mSTR_RPAS_User.RpasId });
        var mailsubject = "New User Creation Request From RPAS Registeration";
        Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, mailurl);
        return RedirectToAction("Index");
      }

      return View(mSTR_RPAS_User);
    }

    // GET: Rpas/Edit/5
    public ActionResult Edit(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
      if (mSTR_RPAS_User == null) {
        return HttpNotFound();
      }
      return View(mSTR_RPAS_User);
    }

    // POST: Rpas/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "RpasId,Name,NationalityId,EmiratesId,EmailId,MobileNo,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] MSTR_RPAS_User mSTR_RPAS_User) {
      if (ModelState.IsValid) {
        db.Entry(mSTR_RPAS_User).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(mSTR_RPAS_User);
    }

    // GET: Rpas/Delete/5
    public ActionResult Delete(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
      if (mSTR_RPAS_User == null) {
        return HttpNotFound();
      }
      return View(mSTR_RPAS_User);
    }

    // POST: Rpas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id) {
      MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
      db.MSTR_RPAS_User.Remove(mSTR_RPAS_User);
      db.SaveChanges();
      return RedirectToAction("Index");
    }

   protected override void Dispose(bool disposing)
   {
      if (disposing)
       {
            db.Dispose();
       }
       base.Dispose(disposing);
   }

        public ActionResult UAS()
        {
            if (!exLogic.User.hasAccess("RPAS.UAS")) return RedirectToAction("NoAccess", "Home");
            String SQL = "SELECT \n" +
          "  D.[DroneName] as Name,\n" +
          "  D.[CommissionDate],\n" +
          "  M.Name as Manufacture,\n" +
          "  D.RpasSerialNo as 'RPAS Serial Number',\n" +
          "  Count(*) Over() as _TotalRecords,\n" +
          "  D.[DroneId] as _PKey\n" +
          "FROM\n" +
          "  [MSTR_Drone] D\n" +
          "Left join MSTR_Account  O on\n" +
          "  D.AccountID = O.AccountID " +
          "Left join LUP_Drone M on\n" +
          "  ManufactureID = M.TypeID and\n" +
          "  M.Type='Manufacturer' " +
          "Left join LUP_Drone U on\n" +
          "  UAVTypeID = U.TypeID and\n" +
          "  U.Type= 'UAVType'\n"+
          "where D.CreatedBy="+Session["UserId"]+ " and D.IsActive=1";

            qView nView = new qView(SQL);
            nView.addMenu("Edit", Url.Action("UASEdit", new { ID = "_Pkey" }));
            nView.addMenu("Delete", Url.Action("DeleteUAS","Rpas", new { ID = "_Pkey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)
        }

        // GET: Rpas/UASRegister
        public ActionResult UASRegister()
        {
            if (!exLogic.User.hasAccess("RPAS.UASCREATE")) return RedirectToAction("NoAccess", "Home");
         
            return View();
        }

        // POST: Rpas/UASRegister
        [HttpPost]      
        public ActionResult UASRegister(MSTR_Drone mSTR_Drone)
        {
            if (!exLogic.User.hasAccess("RPAS.UASCREATE")) return RedirectToAction("NoAccess", "Home");
          
            if (ModelState.IsValid)
            {
                mSTR_Drone.AccountID = 0;
                mSTR_Drone.IsActive = true;
                mSTR_Drone.CreatedBy = Convert.ToInt32(Session["UserId"].ToString());
                mSTR_Drone.CreatedOn = System.DateTime.Now;
                db.MSTR_Drone.Add(mSTR_Drone);
                db.SaveChanges();
                int id = mSTR_Drone.DroneId;
               
                return RedirectToAction("UAS");
            }

            return View(mSTR_Drone);
        }

        // GET: Rpas/UASEdit
        public ActionResult UASEdit(int id=0)
        {
            string SQL = "";
            if (!exLogic.User.hasAccess("RPAS.UASEDIT")) return RedirectToAction("NoAccess", "Home");
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + id;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                MSTR_Drone mSTR_Drone = db.MSTR_Drone.Find(id);
                if (mSTR_Drone == null)
                {
                    return HttpNotFound();
                }
                return View(mSTR_Drone);
            }
            else
            {
                return RedirectToAction("NoAccess", "Home");
            }
            
        }

        // POST: Rpas/UASEdit
        [HttpPost]
        public ActionResult UASEdit(MSTR_Drone mSTR_Drone)
        {
            string SQL = "";
            if (!exLogic.User.hasAccess("RPAS.UASEDIT")) return RedirectToAction("NoAccess", "Home");
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" +mSTR_Drone.DroneId;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                if (ModelState.IsValid)
                {
                    string updatesql = "update MSTR_Drone set [ManufactureId]=" + mSTR_Drone.ManufactureId +
                                       ",[ModifiedBy] =" + Session["UserID"] + ",[ModifiedOn] ='" + System.DateTime.Now +
                                       "',[CommissionDate] ='" + mSTR_Drone.CommissionDate +
                                       "',RpasSerialNo = '" + mSTR_Drone.RpasSerialNo + "' where[DroneId] =" + mSTR_Drone.DroneId;
                    int result = Util.doSQL(updatesql);
                    return RedirectToAction("UAS");
                }
                return View(mSTR_Drone);
            }
            else
            {
                return RedirectToAction("NoAccess", "Home");
            }     
        }   

        public String DeleteUAS(int? ID = 0)
        {
            String SQL = "";
            Response.ContentType = "text/json";
            if (!exLogic.User.hasAccess("RPAS.UASDELETE"))
                return Util.jsonStat("ERROR", "Access Denied");


            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + ID;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                SQL = "DELETE FROM [MSTR_Drone] WHERE DroneId = " + ID;
                Util.doSQL(SQL);

                return Util.jsonStat("OK");
            }
            else
            {
                return Util.jsonStat("Access", "No Access");
            }
        }

    }
}
