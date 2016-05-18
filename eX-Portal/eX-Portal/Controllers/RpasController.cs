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

namespace eX_Portal.Controllers {
  public class RpasController : Controller {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    public ActionResult Register() {
            int RegisterUserID = Util.toInt(69);//Session["RegisterUserID"]);
      if(RegisterUserID <= 0) return View("NoAccess");

      var User = (from n in db.MSTR_User
                  where n.UserId == RegisterUserID
                  select n
      ).FirstOrDefault();
      if(User == null) return View("NoAccess");

      return View(User);
      
    }

        [HttpPost]
        public ActionResult Register(MSTR_User mSTR_User)
        {
            if (mSTR_User.CountryId < 1 || mSTR_User.CountryId == null) ModelState.AddModelError("CountryId", "Please select Nationality");
            if (mSTR_User.EmiratesID == null) ModelState.AddModelError("EmiratesID", "Please enter Emirates Id");
            if (mSTR_User.RPASPermitNo == null) ModelState.AddModelError("RPASPermitNo", "Please enter RPAS Permit Number");
            if (mSTR_User.PermitCategory == null) ModelState.AddModelError("PermitCategory", "Please enter Permit Category");
            if (mSTR_User.ContactAddress == null) ModelState.AddModelError("ContactAddress", "Please enter Contact Address");
            if (mSTR_User.RegRPASSerialNo == null) ModelState.AddModelError("RegRPASSerialNo", "Please enter Registered RPAS SerialNo");
            if (mSTR_User.CompanyAddress == null) ModelState.AddModelError("CompanyAddress", "Please enter Company Address");
            if (mSTR_User.CompanyTelephone == null) ModelState.AddModelError("CompanyTelephone", "Please enter Company Telephone");
            if (mSTR_User.CompanyEmail == null) ModelState.AddModelError("CompanyEmail", "Please enter Company Email");
            if (mSTR_User.Password == null) ModelState.AddModelError("Password", "Please enter Password");
            ModelState.Remove("AccountId");
            ModelState.Remove("UserProfileId");
            ModelState.Remove("IsActive");
            ModelState.Remove("IsPilot");

            if (ModelState.IsValid)
            {                            
                string encryptedpasword = Util.GetEncryptedPassword(mSTR_User.Password);
                mSTR_User.Password = encryptedpasword;

                string updatesql = " update [ExponentPortal].[dbo].[MSTR_User] set [Password]='"+ mSTR_User.Password + "',[PhotoUrl]='"+mSTR_User.PhotoUrl+
                    "',[FirstName] ='"+mSTR_User.FirstName+"',[MiddleName] ='"+mSTR_User.MiddleName+"',[LastName] ='"+mSTR_User.LastName+
                    "',[LastModifiedBy] ="+ Session["RegisterUserID"] + ",[MobileNo] ='"+mSTR_User.MobileNo+"',[HomeNo] ='"+mSTR_User.HomeNo+"',[CountryId] ="+mSTR_User.CountryId+
                    ",[IsActive] =1,[LastModifiedOn] ='"+System.DateTime.Now+"',[RPASPermitNo] ='"+mSTR_User.RPASPermitNo+"',[PermitCategory] ='"+mSTR_User.PermitCategory+
                    "',[ContactAddress] ='"+mSTR_User.ContactAddress+"',[RegRPASSerialNo] ='"+mSTR_User.RegRPASSerialNo+"',[CompanyAddress] ='"+mSTR_User.CompanyAddress+
                    "',[CompanyTelephone] ='"+mSTR_User.CompanyTelephone+"',[CompanyEmail] ='"+mSTR_User.CompanyEmail+
                    "',[TradeLicenceCopyUrl] ='"+mSTR_User.TradeLicenceCopyUrl+"',[EmiratesID] ='"+mSTR_User.EmiratesID+"' where[UserId] ="+Session["RegisterUserID"];
                int result = Util.doSQL(updatesql);

                return RedirectToAction("Internal", "DashBoard");
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
        public ActionResult Create(MSTR_RPAS_User mSTR_RPAS_User)
        {
            if (!exLogic.User.hasAccess("RPAS.CREATE")) return RedirectToAction("NoAccess", "Home");
            if (mSTR_RPAS_User.NationalityId == null) ModelState.AddModelError("NationalityId", "Please select Nationality.");
            if (mSTR_RPAS_User.EmailId == null)
            {
                ModelState.AddModelError("EmailId", "Please enter Email Id.");
            }
            else
            {
                string sqlmailcheck = "select EmailId from MSTR_RPAS_User where [EmailId] ='" + mSTR_RPAS_User.EmailId.ToString() + "'";
                var Row = Util.getDBRow(sqlmailcheck);
                if (Row.Count > 1)
                {
                    if (Row["EmailId"].ToString() == mSTR_RPAS_User.EmailId)
                    {
                        ViewBag.message = "Registeration for this user is already done!!";
                        return View(mSTR_RPAS_User);
                    }
                    else { }
                }
                else { }
            }
            if (ModelState.IsValid)
            {
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

    protected override void Dispose(bool disposing) {
      if (disposing) {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
