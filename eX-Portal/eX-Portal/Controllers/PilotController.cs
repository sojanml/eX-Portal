using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Data.Entity;
using System.Text;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.SessionState;
using FileStorageUtils;
using eX_Portal.Models;

namespace eX_Portal.Controllers
{
    public class PilotController : Controller
    {

        public ExponentPortalEntities db = new ExponentPortalEntities();
        static String RootUploadDir = "~/Upload/User/";
        public object EntityState { get; private set; }
        // GET: Pilot
        public ActionResult Index()
        {
            if (!exLogic.User.hasAccess("PILOTS.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "All Pilot";
            string SQL = "select\n" +
            "  a.UserName,\n" +
            "  a.FirstName" + " + '  '+ " +" a.LastName as FullName,\n" +
            "  c.Name as AccountName, \n" +
            "  a.MobileNo,\n" +          
            "  Count(*) Over() as _TotalRecords ,\n" +
            "  a.UserId as _PKey\n" +
            "from \n" +
            "  MSTR_User a \n" +
            "left join MSTR_Profile b \n" +
            
            "  on a.UserProfileId = b.ProfileId\n" +
            "left join MSTR_Account c on  a.AccountId=c.AccountId \n" +
            "where \n" +
            "  a.ispilot=1";
            if (!exLogic.User.hasAccess("DRONE.MANAGE"))
            {
                SQL += "AND\n" +
                  "  a.AccountID=" + Util.getAccountID();
            }

            qView nView = new qView(SQL);
            if (exLogic.User.hasAccess("PILOTS.VIEW")) nView.addMenu("Detail", Url.Action("PilotDetail", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("PILOTS.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("PILOTS.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("PILOTLOG.VIEW")) nView.addMenu("Add Pilot Log", Url.Action("Create", "PilotLog", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)


        }

        // GET: Pilot/Details/5
        public ActionResult PilotDetail([Bind(Prefix = "ID")] int UserID = 0)
        {
            if (!exLogic.User.hasAccess("PILOTS.VIEW")) return RedirectToAction("NoAccess", "Home");


            Models.MSTR_User User = db.MSTR_User.Find(UserID);
            if (User == null) return RedirectToAction("Error", "Home");
            ViewBag.Title = User.FirstName;
            return View(User);

        }//UserDetail()

        // GET: Pilot/Create
        [ChildActionOnly]
        public ActionResult PilotDetailView([Bind(Prefix = "ID")] int UserID = 0)
        {

            string SQL = "SELECT a.[UserName]\n" +
                        " ,a.[FirstName] \n " +
                        ",a.[MiddleName]\n " +
                        ",a.[LastName]\n  " +
                        ",a.[Remarks]\n   " +
                        ",a.[MobileNo]\n  " +
                        ",a.[OfficeNo]\n  " +
                        ",a.[HomeNo]\n" +
                        " ,a.[EmailId]\n  as [Email ID] " +
                        ",b.[PassportNo]" +
                        " ,CONVERT(NVARCHAR, b.[DateOfExpiry], 106) AS DateOfExpiry\n   " +
                        ",b.[Department]\n  " +
                        " ,b.[EmiratesId]  as [Emirates ID]\n   " +
                        ",b.[Title] as JobTitle\n   " +
                        ",c.[Name] as OrganizationName\n   " +
                        ",d.[ProfileName]\n   " +
                        " FROM[MSTR_User] a\n   " +
                        " left join mstr_user_pilot b\n  " +
                        "on a.UserId=b.UserId\n   " +
                        "left join MSTR_Account c\n  " +
                        "on a.AccountId=c.AccountId\n  " +
                        "left join MSTR_Profile d " +
                        "on a.UserProfileId=d.ProfileId" +
                        " where a.userid=" + UserID;

            if (exLogic.User.hasAccess("PILOT"))
            {
                //nothing
            }
            else if (!exLogic.User.hasAccess("DRONE.MANAGE"))
            {
                SQL +=
                  " AND\n" +
                  "  a.AccountID=" + Util.getAccountID();
            }

            qDetailView nView = new qDetailView(SQL);
            ViewBag.Message = nView.getTable();
            ViewBag.ProfileImage = Util.getProfileImage(UserID);
            return View();
        }


        public ActionResult Create([Bind(Prefix = "ID")] int RPASID = 0)
        {

            ViewBag.Title = "Create User";
            if (!exLogic.User.hasAccess("PILOTS.CREATE")) return RedirectToAction("NoAccess", "Home");


            //var fileStorageProvider = new AmazonS3FileStorageProvider();
            //var fileUploadViewModel = new S3Upload(
            //  fileStorageProvider.PublicKey,
            //  fileStorageProvider.PrivateKey,
            //  fileStorageProvider.BucketName,
            //  Url.Action("complete", "home", null, Request.Url.Scheme)
            //);
            //fileUploadViewModel.SetPolicy(
            //  fileStorageProvider.GetPolicyString(
            //    fileUploadViewModel.FileId,
            //    fileUploadViewModel.RedirectUrl
            //  )
            //);


            //ViewBag.FormAction = fileUploadViewModel.FormAction;
            //ViewBag.FormMethod = fileUploadViewModel.FormMethod;
            //ViewBag.FormEnclosureType = fileUploadViewModel.FormEnclosureType;
            //ViewBag.AWSAccessKey = fileUploadViewModel.AWSAccessKey;
            //ViewBag.Acl = fileUploadViewModel.Acl;
            //ViewBag.Base64EncodedPolicy = fileUploadViewModel.Base64EncodedPolicy;
            //ViewBag.Signature = fileUploadViewModel.Signature;

            ViewBag.IsPassowrdRequired = true;
            MSTR_User EPASValues = new MSTR_User();
            if (RPASID != 0)
            {
                ViewBag.RPASid = RPASID;
                ViewBag.IsPassowrdRequired = false;
                var RPASoList = (from p in db.MSTR_RPAS_User where p.RpasId == RPASID select p).ToList();
                EPASValues.FirstName = RPASoList[0].Name;
                EPASValues.CountryId = Convert.ToInt16(RPASoList[0].NationalityId);
                EPASValues.EmiratesID = RPASoList[0].EmiratesId;
                EPASValues.EmailId = RPASoList[0].EmailId;
                EPASValues.MobileNo = RPASoList[0].MobileNo;

                EPASValues.UserProfileId = Convert.ToInt16(7);
                EPASValues.Dashboard = "RPAS";
                EPASValues.IsActive = false;
                EPASValues.IsPilot = false;
            }

            var viewModel = new ViewModel.UserViewModel
            {

                User = EPASValues,
                Pilot = new MSTR_User_Pilot()  
              
                ,
             //   ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
             //   AccountList = Util.GetAccountList(),
             
              //  DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };
            
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Create(ViewModel.UserViewModel UserModel, int ID = 0)
        {
            string hdnRPASid = Request["hdnRPASid"];
            //
            //int RPASID = string.IsNullOrEmpty(hdnRPASid) ? 0 : Convert.ToInt16(hdnRPASid);
            int RPASID = ID;
           
            if (!exLogic.User.hasAccess("PILOTS.CREATE")) return RedirectToAction("NoAccess", "Home");
            if (ModelState.IsValid)
            {
                if (exLogic.User.UserExist(UserModel.User.UserName) > 0)
                {
                    ModelState.AddModelError("User.UserName", "This Pilot already exists.");
                }
                if (exLogic.User.EmailExist(UserModel.User.EmailId) > 0)
                {
                    ModelState.AddModelError("User.EmailId", "This email id already exists.");
                }
                if (RPASID == 0)
                {
                    if (String.IsNullOrEmpty(UserModel.User.Password))
                    {
                        ModelState.AddModelError("User.Password", "Invalid Password. Please enter again.");
                    }
                }

            }
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
         

            if (ModelState.IsValid)
            {
                string Password;
                if (RPASID == 0)
                    Password = Util.GetEncryptedPassword(UserModel.User.Password).ToString();
                else
                    Password = "";

                int AccountID = Util.getAccountID();
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
                  "  '" + Util.FirstLetterToUpper( UserModel.User.UserName) + "',\n" +
                  "  '" + Password + "',\n" +
                  "  '" + Util.FirstLetterToUpper( UserModel.User.FirstName) + "',\n" +
                  "  '" + Util.FirstLetterToUpper(UserModel.User.MiddleName) + "',\n" +
                    "  '" + Util.FirstLetterToUpper( UserModel.User.LastName) + "',\n" +
                  "  " + Util.getLoginUserID() + ",\n" +
                  " '"+Util.getPilotProfileID(AccountID)  +"' ,\n" +
                  "  '" + UserModel.User.Remarks + "',\n" +
                  "  '" + UserModel.User.MobileNo + "',\n" +
                  "  '" + UserModel.User.OfficeNo + "',\n" +
                  "  '" + UserModel.User.HomeNo + "',\n" +
                  "  '" + UserModel.User.EmailId + "',\n" +
                  "  " + Util.toInt(UserModel.User.CountryId) + ",\n" +
                  "  '" + UserModel.User.IsActive + "',\n" +
                  "  GETDATE(),\n" +
                  "  " + Util.toInt(AccountID) + ",\n" +
                  "    'True',\n" +
                  "  '" + UserModel.User.PhotoUrl + "',\n" +
                  "  '" + Util.getDashboard() + "',\n" +
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
                    var mailsubject = "Pilot has been created";
                    Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, "~" + mailurl);

                    //need to update the RPAS Status after sending mmail
                    SQL = "update mstr_rpas_user set [Status] = 'Pilot Created' where rpasID = " + RPASID;
                    Util.doSQL(SQL);
                }
                return RedirectToAction("PilotDetail","Pilot", new { ID = id });

            }

            var viewModel = new ViewModel.UserViewModel
            {
                User = UserModel.User,
                Pilot = UserModel.Pilot,
               // ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
             //   AccountList = Util.GetAccountList(),
               // DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")

            };
            ViewBag.IsPassowrdRequired = true;

            if (RPASID != 0)
            {
                ViewBag.IsPassowrdRequired = false;
            }
                return View(viewModel);
        }//Create() HTTPPost
         // POST: Pilot/Create




        public ActionResult Edit(int id)
        {


            if (!exLogic.User.hasAccess("PILOTS.EDIT")) return RedirectToAction("NoAccess", "Home");
            var viewModel = new ViewModel.UserViewModel
            {
                User = db.MSTR_User.Find(id),
                Pilot = db.MSTR_User_Pilot.Find(id),
               // ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
               // AccountList = Util.GetAccountList(),
               // DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };
            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Edit(ViewModel.UserViewModel UserModel)
        {
            String Pass_SQL = "\n";
            if (!exLogic.User.hasAccess("PILOTS.EDIT")) return RedirectToAction("NoAccess", "Home");
            UserModel.User.IsPilot = true;
            
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(UserModel.User.Password) && !String.IsNullOrEmpty(UserModel.User.ConfirmPassword))
                {
                    if (UserModel.User.Password != UserModel.User.ConfirmPassword)
                    {
                        ModelState.AddModelError("User.Password", "Password doesn't match.");
                    }
                    else
                    {
                        Pass_SQL = ",\n  Password='" + Util.GetEncryptedPassword(UserModel.User.Password).ToString() + "'\n";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                int AccountID = Util.getAccountID();

                string SQL = "UPDATE MSTR_USER SET\n" +
               "  UserProfileId=" + Util.toInt(Util.getPilotProfileID(AccountID)) + ",\n" +
                  "  FirstName='" + Util.FirstLetterToUpper( UserModel.User.FirstName) + "',\n" +
                  "  MiddleName='" + Util.FirstLetterToUpper( UserModel.User.MiddleName) + "',\n" +
                  "  LastName='" + Util.FirstLetterToUpper(UserModel.User.LastName) + "',\n" +
                  "  Remarks='" + UserModel.User.Remarks + "',\n" +
                  "  MobileNo='" + UserModel.User.MobileNo + "',\n" +
                  "  EmailId='" + UserModel.User.EmailId + "',\n" +
                  "  CountryId=" + Util.toInt(UserModel.User.CountryId.ToString()) + ",\n" +
                  "  AccountId=" + Util.toInt(AccountID) + ",\n" +
                  "  OfficeNo='" + UserModel.User.OfficeNo + "',\n" +
                  "  HomeNo='" + UserModel.User.HomeNo + "',\n" +
                 // "  IsActive='" + UserModel.User.IsActive + "', \n" +
                 // "  IsPilot='" + UserModel.User.IsPilot + "',\n" +
                //  "  Dashboard= '" + UserModel.User.Dashboard.ToString() + "',\n" +
                  "  PhotoUrl='" + UserModel.User.PhotoUrl + "',\n" +
                  "  RPASPermitNo='" + UserModel.User.RPASPermitNo + "',\n" +
                  "  PermitCategory='" + UserModel.User.PermitCategory + "'\n" +
                 // "  ContactAddress='" + UserModel.User.ContactAddress + "',\n" +
                 // "  RegRPASSerialNo='" + UserModel.User.RegRPASSerialNo + "',\n" +
                //  "  CompanyAddress='" + UserModel.User.CompanyAddress + "',\n" +
                //  "  CompanyTelephone='" + UserModel.User.CompanyTelephone + "',\n" +
                //  "  CompanyEmail='" + UserModel.User.CompanyEmail + "',\n" +
                 // "  EmiratesID='" + UserModel.User.EmiratesID + "'\n" +
                  Pass_SQL +
                  "where\n" +
                  "  UserId=" + UserModel.User.UserId;



                int id = Util.doSQL(SQL);

                //updating pilot information to pilot table

                SQL = "UPDATE MSTR_USER_PILOT SET\n" +
                 "  DateOfExpiry='" + UserModel.Pilot.DateOfExpiry + "',\n" +
                 "  Department='" + UserModel.Pilot.Department + "',\n" +
                 "  EmiratesId='" + UserModel.Pilot.EmiratesId + "',\n" +
                 "  Title='" + UserModel.Pilot.Title + "'\n" +
                  "where\n" +
                 "  UserId=" + UserModel.User.UserId; ;
                int idPilot = Util.doSQL(SQL);

                return RedirectToAction("PilotDetail", new { ID = UserModel.User.UserId });
            }

            var viewModel = new ViewModel.UserViewModel
            {

              //  ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
              //  AccountList = Util.GetAccountList(),
              //  DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };
            return View(viewModel);

        }//ActionEdit()

        public String UploadFile([Bind(Prefix = "ID")] int UserID = 0)
        {

            String UploadPath = Server.MapPath(Url.Content(RootUploadDir) + UserID + "/");
            //send information in JSON Format always
            StringBuilder JsonText = new StringBuilder();
            Response.ContentType = "text/json";

            //when there are files in the request, save and return the file information
            try
            {
                var TheFile = Request.Files[0];
                String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
                String FullName = UploadPath + FileName;

                if (!Directory.Exists(UploadPath)) Directory.CreateDirectory(UploadPath);
                TheFile.SaveAs(FullName);
                JsonText.Append("{");
                JsonText.Append(Util.Pair("status", "success", true));
                JsonText.Append("\"addFile\":[");
                JsonText.Append(Util.getFileInfo(FullName));
                JsonText.Append("]}");

            }
            catch (Exception ex)
            {
                JsonText.Clear();
                JsonText.Append("{");
                JsonText.Append(Util.Pair("status", "error", true));
                JsonText.Append(Util.Pair("message", ex.Message, false));
                JsonText.Append("}");
            }//catch
            return JsonText.ToString();
        }//Save()



        public String Delete([Bind(Prefix = "ID")]int UserID = 0)
        {
            if (!exLogic.User.hasAccess("PILOTS.DELETE"))

                return Util.jsonStat("ERROR", "Access Denied");
            String SQL = "";
            Response.ContentType = "text/json";


            //Delete the drone from database if there is no user createdby
            SQL = "SELECT Count(*) FROM MSTR_User where CreatedBy = " + UserID;

            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to another user");

            SQL = "select count(*) from Mstr_Drone where CreatedBy=" + UserID;
            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to Drone Creation");
            SQL = "select count(*) from Mstr_DroneService where CreatedBy=" + UserID;
            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to DroneService Creation");



            SQL = "DELETE FROM [MSTR_USER] WHERE UserId = " + UserID;
            Util.doSQL(SQL);
            SQL = "DELETE FROM [MSTR_USER_PILOT] WHERE UserId = " + UserID;
            Util.doSQL(SQL);

            return Util.jsonStat("OK");
        }

    }
}
