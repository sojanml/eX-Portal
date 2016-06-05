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
using FileStorageUtils;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace eX_Portal.Controllers
{
    public class RpasUserController : Controller
    {
        private ExponentPortalEntities db = new ExponentPortalEntities();
        static String RootUploadDir = "~/Upload/User/";
        // GET: RpasUser
        public ActionResult Index()
        {
            string SQL = "SELECT MSTR_User.UserName,\n" +
                         "MSTR_User.FirstName + ' ' + MSTR_User.LastName as Name,\n" +
                         "MSTR_Profile.ProfileName as Profile,\n" +
                         "MSTR_Account.Name as Account,\n" +
                         "MSTR_User.MobileNo as Mobile,\n" +
                         "MSTR_User.EmailId,\n" +                         
                         "Count(*) Over() as _TotalRecords,\n" +
                         "UserId as _PKey\n" +
                         "FROM \n" +
                         "MSTR_User INNER JOIN MSTR_Account \n" +
                         "ON MSTR_User.AccountId = MSTR_Account.AccountId INNER JOIN MSTR_Profile \n" +
                         "ON MSTR_User.UserProfileId = MSTR_Profile.ProfileId \n" +
                         "where MSTR_User.AccountId = " +23;
            qView nView = new qView(SQL);            
            nView.addMenu("Edit", Url.Action("Edit", "RpasUser", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)
            //return View(db.MSTR_User.ToList());
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
            if (!exLogic.User.hasAccess("RPASUSER.CREATE")) return RedirectToAction("NoAccess", "Home");

            MSTR_User EPASValues = new MSTR_User();
            if (RPASID != 0)
            {
                ViewBag.RPASid = RPASID;                
                //ViewBag.IsPassowrdRequired = false;
                var RPASoList = (from p in db.MSTR_RPAS_User where p.RpasId == RPASID select p).ToList();
                EPASValues.FirstName = RPASoList[0].Name;
                EPASValues.CountryId = Convert.ToInt16(RPASoList[0].NationalityId);
                EPASValues.EmiratesID = RPASoList[0].EmiratesId;
                EPASValues.EmailId = RPASoList[0].EmailId;
                EPASValues.MobileNo = RPASoList[0].MobileNo;

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
            if (!exLogic.User.hasAccess("RPASUSER.CREATE")) return RedirectToAction("NoAccess", "Home");
            string hdnRPASid = Request["hdnRPASid"];
            ModelState.Remove("User.AccountId");
            ModelState.Remove("User.UserProfileId");
            ModelState.Remove("User.IsActive");
            ModelState.Remove("User.IsPilot");
            if (String.IsNullOrEmpty(UserModel.User.RPASPermitNo)) ModelState.AddModelError("User.RPASPermitNo","Please enter the RPAS Permit Number");
            if (String.IsNullOrEmpty(UserModel.User.PermitCategory)) ModelState.AddModelError("User.PermitCategory", "Please select the RPAS Permit Category");
            if (String.IsNullOrEmpty(UserModel.User.ContactAddress)) ModelState.AddModelError("User.ContactAddress", "Please enter Contact Address");
            if (String.IsNullOrEmpty(UserModel.User.RegRPASSerialNo)) ModelState.AddModelError("User.RegRPASSerialNo", "Please enter RPAS Serial Number");
            if (String.IsNullOrEmpty(UserModel.User.CompanyAddress)) ModelState.AddModelError("User.CompanyAddress", "Please enter Company Address");
            if (String.IsNullOrEmpty(UserModel.User.CompanyTelephone)) ModelState.AddModelError("User.CompanyTelephone", "Please enter Company Telephone Number.");
            if (String.IsNullOrEmpty(UserModel.User.CompanyEmail)) ModelState.AddModelError("User.CompanyEmail", "Please enter Company Email");
            if (String.IsNullOrEmpty(UserModel.User.RPASPermitNo)) ModelState.AddModelError("User.RPASPermitNo", "Please enter the RPAS Permit Number");
            if (String.IsNullOrEmpty(UserModel.Pilot.EmiratesId)) ModelState.AddModelError("Pilot.EmiratesId", "Emirates ID is required."); 
            if (String.IsNullOrEmpty(UserModel.Pilot.PassportNo)) ModelState.AddModelError("Pilot.PassportNo", "Passport Number is required."); 
            
            int RPASID = ID;
         
            //checking if username or mail already exist
            if (exLogic.User.UserExist(UserModel.User.UserName) > 0)
            {
                ModelState.AddModelError("User.UserName", "This Username already exists.");
            }
            if (exLogic.User.EmailExist(UserModel.User.EmailId) > 0)
            {
                ModelState.AddModelError("User.EmailId", "Email already exists.");
            }

            //if (RPASID == 0)
            //{
            //    if (String.IsNullOrEmpty(UserModel.User.Password))
            //    {
            //        ModelState.AddModelError("User.Password", "Invalid Password.Please enter again.");
            //    }
            //}                      

            if (ModelState.IsValid)
            {
                string Password="";
                //if (RPASID == 0)
                //    Password = Util.GetEncryptedPassword(UserModel.User.Password).ToString();
                //else
                //    Password = "";

                String SQL = "insert into MSTR_User(\n" +
                  "  UserName,\n" +
                  "  Password,\n" +
                  "  FirstName,\n" +
                  "  MiddleName,\n" +
                  "  LastName,\n" +
                  "  CreatedBy,\n" +
                  "  UserProfileId,\n" +
                  //"  Remarks,\n" +
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
                  "  7,\n" +
                  //"  '" + UserModel.User.Remarks + "',\n" +
                  "  '" + UserModel.User.MobileNo + "',\n" +
                  "  '" + UserModel.User.OfficeNo + "',\n" +
                  "  '" + UserModel.User.HomeNo + "',\n" +
                  "  '" + UserModel.User.EmailId + "',\n" +
                  "  " + Util.toInt(UserModel.User.CountryId) + ",\n" +
                  "  'true',\n" +
                  "  GETDATE(),\n" +
                  "  23,\n" +
                  "  'true',\n" +
                  "  '" + UserModel.User.PhotoUrl + "',\n" +
                  "  'RPAS',\n" +
                  "  '" + (UserModel.User.RPASPermitNo) + "',\n" +
                  "  '" + (UserModel.User.PermitCategory) + "',\n" +
                  "  '" + (UserModel.User.ContactAddress) + "',\n" +
                  "  '" + (UserModel.User.RegRPASSerialNo) + "',\n" +
                  "  '" + (UserModel.User.CompanyAddress) + "',\n" +
                  "  '" + (UserModel.User.CompanyTelephone) + "',\n" +
                  "  '" + (UserModel.User.CompanyEmail) + "',\n" +
                  "  '" + (UserModel.Pilot.EmiratesId) + "'\n" +
                  ")";
                //inserting pilot information to the pilot table
                int id = Util.InsertSQL(SQL);

                SQL = "insert into MSTR_User_Pilot(\n" +
                  "  UserId,\n" +
                  "  PassportNo,\n" +
                  "  DateOfExpiry,\n" +
                  //"  Department,\n" +
                  "  EmiratesId\n" +
                  //"  Title\n" +
                  ") values(\n" +
                  "  '" + id + "',\n" +
                  "  '" + UserModel.Pilot.PassportNo + "',\n" +
                  "  '" + UserModel.Pilot.DateOfExpiry + "',\n" +
                    "  '" + UserModel.Pilot.EmiratesId + "'\n)";
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

                //to sent mail to the user created
                if (id != 0)
                {                   
                    var mailurl = "/Email/RPASUserCreated/" + id;
                    var mailsubject = "User has been created";
                    Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, "~" + mailurl);

                    //need to update the RPAS Status after sending mail
                    SQL = "update mstr_rpas_user set [Status] = 'User Created' where rpasID = " + RPASID;
                    Util.doSQL(SQL);
                }
                return RedirectToAction("UASRegister","RPAS");

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
       
        }

        // GET: RpasUser/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!exLogic.User.hasAccess("RPASUSER.EDIT")) return RedirectToAction("NoAccess", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_User mSTR_User = db.MSTR_User.Find(id);
            MSTR_User_Pilot mSTR_User_Pilot = db.MSTR_User_Pilot.Find(id);
            if (mSTR_User == null)
            {
                return HttpNotFound();
            }
            var viewModel = new ViewModel.UserViewModel
            {
                User = mSTR_User,
                Pilot = mSTR_User_Pilot,
                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList(),
                DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")

            };
            return View(viewModel);
            //return View(mSTR_User);
        }

        // POST: RpasUser/Edit/5     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModel.UserViewModel UserModel, int ID = 0)
        {
            if (!exLogic.User.hasAccess("RPASUSER.EDIT")) return RedirectToAction("NoAccess", "Home");
            string hdnRPASid = Request["hdnRPASid"];
            ModelState.Remove("User.AccountId");
            ModelState.Remove("User.UserProfileId");
            ModelState.Remove("User.IsActive");
            ModelState.Remove("User.IsPilot");
            
            if (String.IsNullOrEmpty(UserModel.User.RPASPermitNo)) ModelState.AddModelError("User.RPASPermitNo", "Please enter the RPAS Permit Number");
            if (String.IsNullOrEmpty(UserModel.User.PermitCategory)) ModelState.AddModelError("User.PermitCategory", "Please select the RPAS Permit Category");
            if (String.IsNullOrEmpty(UserModel.User.ContactAddress)) ModelState.AddModelError("User.ContactAddress", "Please enter Contact Address");
            if (String.IsNullOrEmpty(UserModel.User.RegRPASSerialNo)) ModelState.AddModelError("User.RegRPASSerialNo", "Please enter RPAS Serial Number");
            if (String.IsNullOrEmpty(UserModel.User.CompanyAddress)) ModelState.AddModelError("User.CompanyAddress", "Please enter Company Address");
            if (String.IsNullOrEmpty(UserModel.User.CompanyTelephone)) ModelState.AddModelError("User.CompanyTelephone", "Please enter Company Telephone Number.");
            if (String.IsNullOrEmpty(UserModel.User.CompanyEmail)) ModelState.AddModelError("User.CompanyEmail", "Please enter Company Email");
            if (String.IsNullOrEmpty(UserModel.User.RPASPermitNo)) ModelState.AddModelError("User.RPASPermitNo", "Please enter the RPAS Permit Number");
            if (String.IsNullOrEmpty(UserModel.Pilot.EmiratesId)) ModelState.AddModelError("Pilot.EmiratesId", "Emirates ID is required.");
            if (String.IsNullOrEmpty(UserModel.Pilot.PassportNo)) ModelState.AddModelError("Pilot.PassportNo", "Passport Number is required.");

            int RPASID = ID;   

            if (ModelState.IsValid)
            {             
                string SQL = "update MSTR_User set \n" +
                             "UserName = '" + UserModel.User.UserName + "', \n" +
                             "FirstName = '" + UserModel.User.FirstName + "', \n" +
                             "MiddleName = '" + UserModel.User.MiddleName + "', \n" +
                             "LastName = '" + UserModel.User.LastName + "', \n" +
                             "MobileNo = '" + UserModel.User.MobileNo + "', \n" +
                             "OfficeNo = '" + UserModel.User.OfficeNo + "', \n" +
                             "HomeNo = '" + UserModel.User.HomeNo + "', \n" +
                             "EmailId = '" + UserModel.User.EmailId + "', \n" +
                             "CountryId = " + Util.toInt(UserModel.User.CountryId) + ", \n" +
                             "PhotoUrl = '" + UserModel.User.PhotoUrl + "', \n" +
                             "RPASPermitNo = '" + UserModel.User.RPASPermitNo + "', \n" +
                             "PermitCategory = '" + UserModel.User.PermitCategory + "', \n" +
                             "ContactAddress = '" + UserModel.User.ContactAddress + "', \n" +
                             "RegRPASSerialNo = '" + UserModel.User.RegRPASSerialNo + "', \n" +
                             "CompanyAddress = '" + UserModel.User.CompanyAddress + "', \n" +
                             "CompanyTelephone = '" + UserModel.User.CompanyTelephone + "', \n" +
                             "CompanyEmail = '" + UserModel.User.CompanyEmail + "', \n" +
                             "EmiratesID = '" + UserModel.Pilot.EmiratesId + "' \n" +
                             "where UserId =" + ID;                
                int id = Util.doSQL(SQL);

                string SQL1 = "update MSTR_User_Pilot set \n" +
                            "PassportNo = '" + UserModel.Pilot.PassportNo + "',\n" +
                            "DateOfExpiry = '" + UserModel.Pilot.DateOfExpiry + "',\n" +
                            "EmiratesId = '" + UserModel.Pilot.EmiratesId + "'\n " +
                            "where UserId = " + ID;
                int Pid = Util.doSQL(SQL1);

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

                //to sent mail to the user created
                //if (id != 0)
                //{
                //    var mailurl = "/Email/RPASUserCreated/" + id;
                //    var mailsubject = "User has been created";
                //    Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, "~" + mailurl);

                //    //need to update the RPAS Status after sending mail
                //    SQL = "update mstr_rpas_user set [Status] = 'User Created' where rpasID = " + RPASID;
                //    Util.doSQL(SQL);
                //}
                return RedirectToAction("Index");
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
