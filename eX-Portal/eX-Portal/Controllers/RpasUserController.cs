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

namespace eX_Portal.Controllers
{
    public class RpasUserController : Controller
    {
        private ExponentPortalEntities db = new ExponentPortalEntities();

        // GET: RpasUser
        public ActionResult Index()
        {
            return View(db.MSTR_User.ToList());
        }

        // GET: RpasUser/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_User mSTR_User = db.MSTR_User.Find(id);
            if (mSTR_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_User);
        }

        // GET: RpasUser/Create
        public ActionResult Create([Bind(Prefix = "ID")] int RPASID = 0)
        {            
            if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");

            ViewBag.IsPassowrdRequired = true;

            MSTR_User EPASValues = new MSTR_User();
            if (RPASID != 0)
            {
                ViewBag.RPASid = RPASID;
                ViewBag.IsPassowrdRequired = false;
                //var RPASoList = (from p in db.MSTR_RPAS_User where p.RpasId == RPASID select p).ToList();
                //EPASValues.FirstName = RPASoList[0].Name;
                //EPASValues.CountryId = Convert.ToInt16(RPASoList[0].NationalityId);
                //EPASValues.EmiratesID = RPASoList[0].EmiratesId;
                //EPASValues.EmailId = RPASoList[0].EmailId;
                //EPASValues.MobileNo = RPASoList[0].MobileNo;

                //EPASValues.UserProfileId = Convert.ToInt16(7);
                //EPASValues.Dashboard = "RPAS";
                //EPASValues.IsActive = false;
                //EPASValues.IsPilot = false;
            }

            var viewModel = new ViewModel.UserViewModel
            {
                User = EPASValues,
                Pilot = new MSTR_User_Pilot(),
                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList(),
                DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };

            return View(viewModel);
        }

        // POST: RpasUser/Create       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModel.UserViewModel UserModel, int ID = 0)
        {
            string hdnRPASid = Request["hdnRPASid"];
            //
            //int RPASID = string.IsNullOrEmpty(hdnRPASid) ? 0 : Convert.ToInt16(hdnRPASid);
            int RPASID = ID;

            if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");
            //if (ModelState.IsValid) {
            if (exLogic.User.UserExist(UserModel.User.UserName) > 0)
            {
                ModelState.AddModelError("User.UserName", "This username already exists.");
            }
            if (exLogic.User.EmailExist(UserModel.User.EmailId) > 0)
            {
                ModelState.AddModelError("User.EmailId", "Email already exists.");
            }
            if (RPASID == 0)
            {
                if (String.IsNullOrEmpty(UserModel.User.Password))
                {
                    ModelState.AddModelError("User.Password", "Invalid Password. Please enter again.");
                }
            }

            if (UserModel.User.IsPilot == true)
            {
                if (String.IsNullOrEmpty(UserModel.Pilot.EmiratesId))
                {
                    ModelState.AddModelError("Pilot.EmiratesId", "Emirates ID is required.");
                }
                if (String.IsNullOrEmpty(UserModel.Pilot.PassportNo))
                {
                    ModelState.AddModelError("Pilot.PassportNo", "Passport  ID is required.");
                }
                if (String.IsNullOrEmpty(UserModel.Pilot.Department))
                {
                    ModelState.AddModelError("Pilot.Department", "Department is required.");
                }
            }//if(UserModel.User.IsPilot == true) {
             //}

            if (ModelState.IsValid)
            {
                string Password;
                if (RPASID == 0)
                    Password = Util.GetEncryptedPassword(UserModel.User.Password).ToString();
                else
                    Password = "";

                String SQL = "insert into MSTR_User(\n" +
                  "  UserName,\n" +
                  "  Password,\n" +
                  "  FirstName,\n" +
                  "  MiddleName,\n" +
                  "  LastName,\n" +
                  "  CreatedBy,\n" +
                  "  UserProfileId,\n" +
                  "  Remarks,\n" +
                  "  MobileNo,\n" +
                  "  OfficeNo,\n" +
                  "  HomeNo,\n" +
                  "  EmailId,\n" +
                  "  CountryId,\n" +
                  "  IsActive,\n" +
                  "  CreatedOn,\n" +
                  "  AccountId,\n" +
                  "  IsPilot, \n" +
                  "  PhotoUrl,\n" +
                  " Dashboard,\n" +
                  " RPASPermitNo,\n" +
                  " PermitCategory,\n" +
                  " ContactAddress,\n" +
                  " RegRPASSerialNo,\n" +
                  " CompanyAddress,\n" +
                  " CompanyTelephone,\n" +
                  " CompanyEmail,\n" +
                  " EmiratesID\n" +
                  ") values(\n" +
                  "  '" + UserModel.User.UserName + "',\n" +
                  "  '" + Password + "',\n" +
                  "  '" + UserModel.User.FirstName + "',\n" +
                  "  '" + UserModel.User.MiddleName + "',\n" +
                    "  '" + UserModel.User.LastName + "',\n" +
                  "  " + Util.getLoginUserID() + ",\n" +
                  "  " + UserModel.User.UserProfileId + ",\n" +
                  "  '" + UserModel.User.Remarks + "',\n" +
                  "  '" + UserModel.User.MobileNo + "',\n" +
                  "  '" + UserModel.User.OfficeNo + "',\n" +
                  "  '" + UserModel.User.HomeNo + "',\n" +
                  "  '" + UserModel.User.EmailId + "',\n" +
                  "  " + Util.toInt(UserModel.User.CountryId) + ",\n" +
                  "  '" + UserModel.User.IsActive + "',\n" +
                  "  GETDATE(),\n" +
                  "  " + Util.toInt(UserModel.User.AccountId) + ",\n" +
                  "  '" + UserModel.User.IsPilot + "',\n" +
                  "  '" + UserModel.User.PhotoUrl + "',\n" +
                  "  '" + UserModel.User.Dashboard + "',\n" +
                  "  '" + (UserModel.User.RPASPermitNo) + "',\n" +
                  "  '" + (UserModel.User.PermitCategory) + "',\n" +
                  "  '" + (UserModel.User.ContactAddress) + "',\n" +
                  "  '" + (UserModel.User.RegRPASSerialNo) + "',\n" +
                  "  '" + (UserModel.User.ContactAddress) + "',\n" +
                  "  '" + (UserModel.User.CompanyTelephone) + "',\n" +
                  "  '" + (UserModel.User.CompanyEmail) + "',\n" +
                  "  '" + (UserModel.User.EmiratesID) + "'\n" +
                  ")";
                //inserting pilot information to the pilot table
                int id = Util.InsertSQL(SQL);

                SQL = "insert into MSTR_User_Pilot(\n" +
                  "  UserId,\n" +
                  "  PassportNo,\n" +
                  "  DateOfExpiry,\n" +
                  "  Department,\n" +
                  "  EmiratesId,\n" +
                  "  Title\n" +
                  ") values(\n" +
                  "  '" + id + "',\n" +
                  "  '" + UserModel.Pilot.PassportNo + "',\n" +
                  "  '" + UserModel.Pilot.DateOfExpiry + "',\n" +
                  "  '" + UserModel.Pilot.Department + "',\n" +
                    "  '" + UserModel.Pilot.EmiratesId + "',\n" +
                  "  '" + UserModel.Pilot.Title + "'\n)";
                int Pid = Util.InsertSQL(SQL);

                //move the image to correct path
                String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
                String newPath = UploadPath + id + "/";
                String PhotoURL = UploadPath + "0/" + UserModel.User.PhotoUrl;
                if (!System.IO.Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
                if (!String.IsNullOrEmpty(UserModel.User.PhotoUrl) &&
                    System.IO.File.Exists(PhotoURL))
                {
                    System.IO.File.Move(PhotoURL, newPath + UserModel.User.PhotoUrl);
                }
                if (RPASID != 0)
                {
                    //var mailurl = Url.Action("RPASUserCreated", "Email", new { UserID = id });
                    var mailurl = "/Email/RPASUserCreated/" + id;
                    var mailsubject = "User has been created";
                    Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, "~" + mailurl);

                    //need to update the RPAS Status after sending mmail
                    SQL = "update mstr_rpas_user set [Status] = 'User Created' where rpasID = " + RPASID;
                    Util.doSQL(SQL);
                }
                return RedirectToAction("UserDetail", new { ID = id });

            }

            var viewModel = new ViewModel.UserViewModel
            {
                User = UserModel.User,
                Pilot = UserModel.Pilot,
                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList(),
                DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")

            };

            return View(viewModel);

            //if (ModelState.IsValid)
            //{
            //    db.MSTR_User.Add(mSTR_User);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            //return View(mSTR_User);
        }

        // GET: RpasUser/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_User mSTR_User = db.MSTR_User.Find(id);
            if (mSTR_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_User);
        }

        // POST: RpasUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,UserName,Password,RememberMe,PhotoUrl,FirstName,MiddleName,LastName,CreatedBy,LastModifiedBy,ApprovedBy,UserProfileId,Remarks,MobileNo,OfficeNo,HomeNo,EmailId,CountryId,RecordType,IsActive,LastModifiedOn,ApprovedOn,CreatedOn,PasswordSalt,AccountId,IsPilot,UserAccountId,Dashboard,GeneratedPassword,RPASPermitNo,PermitCategory,ContactAddress,RegRPASSerialNo,CompanyAddress,CompanyTelephone,CompanyEmail,TradeLicenceCopyUrl,EmiratesID")] MSTR_User mSTR_User)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mSTR_User).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mSTR_User);
        }

        // GET: RpasUser/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_User mSTR_User = db.MSTR_User.Find(id);
            if (mSTR_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_User);
        }

        // POST: RpasUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MSTR_User mSTR_User = db.MSTR_User.Find(id);
            db.MSTR_User.Remove(mSTR_User);
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
    }
}
