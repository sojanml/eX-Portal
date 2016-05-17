using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
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

namespace eX_Portal.Controllers
{
    public class UserController : Controller
    {
        // GET: UserLogin
        public ExponentPortalEntities db = new ExponentPortalEntities();
        static String RootUploadDir = "~/Upload/User/";
        public object EntityState { get; private set; }

        public ActionResult Index()
        {
            ViewBag.Title = "Login";
            UserLogin _objuserlogin = new UserLogin();
            return View(_objuserlogin);
        }//Login()

        
       
        public string ExponentCertificateDetails([Bind(Prefix = "ID")] int PilotID)
        {
            if (!exLogic.User.hasAccess("EXPCERT.VIEW")) return "Access Denied";

            string SQL = " select b.name as Certification,\n " +
                  " a.score as Score,\n" +
                 "CONVERT(NVARCHAR, a.DateOfEnrollement, 106) AS DateOfEnrollement  ,\n" +
                  "CONVERT(NVARCHAR, a.DateOfCertification, 106) AS DateOfCertification  ,\n" +
                  "  a.Id as _PKey" +
                  " from mstr_user_pilot_ExponentUAS a \n " +
                  " left join LUP_Drone b \n " +
                  " on \n" +
                  " a.CertificateId = b.TypeId\n " +
                  " where \n" +
                  "  b.Type = 'ExponentCertificate' \n" +
                  "  and \n" +
                  "  a.UserId =" + PilotID;
            qView nView = new qView(SQL);
            if (nView.HasRows)
            {
                nView.isFilterByTop = false;
                return
                  "<h2>Exponent Certification Details</h2>\n" +
                  nView.getDataTable(isIncludeData: true, isIncludeFooter: false, qDataTableID: "ExponentCertificateDetails");
            }

            return "";

        }
        public String PilotCertificateDetails([Bind(Prefix = "ID")] int PilotID)
        {
            if (!exLogic.User.hasAccess("PILOTCERT.VIEW")) return "Access Denied";

            String SQL = "  select b.name as Certification, \n" +
             "  a.score as Score,c.Name as IssuingAuthority ,\n" +
             "CONVERT(NVARCHAR, a.DateOfIssue, 106) AS DateOfIssue , \n" +
             " CONVERT(NVARCHAR, a.DateOfExpiry, 106) AS DateOfExpiry  , \n" +
             "  CONVERT(NVARCHAR, a.NextRenewal, 106) AS NextRenewal  ,\n" +
             "  a.Id as _PKey" +
             "  from \n" +
             "  mstr_user_pilot_Certification a \n" +
             "  left join LUP_Drone b \n" +
             "  on a.CertificateId = b.TypeId \n" +
             "  left join LUP_Drone c \n" +
             "  on a.IssuingAuthorityId = c.typeId \n" +
             "  where \n" +
             "  b.Type = 'Certificate'\n " +
             "  and c.Type = 'IssueAuthority' \n" +
             "  and a.UserId =" + PilotID;
            qView nView = new qView(SQL);

            if (nView.HasRows)
            {
                nView.isFilterByTop = false;
                return
                  "<h2>Pilot Certification Details</h2>\n" +
                  nView.getDataTable(
                    isIncludeData: true,
                    isIncludeFooter: false,
                    qDataTableID: "PilotCertificateDetails"
                  );
            }
            return "";
        }


        [HttpPost]
        public ActionResult Index(UserLogin _objuserlogin)
        {
            ViewBag.Title = "Login";

            /*Create instance of entity model*/
            ExponentPortalEntities objentity = new ExponentPortalEntities();
            /*Getting data from database for user validation*/
            if (exLogic.User.UserIsActive(_objuserlogin.UserName, _objuserlogin.Password) > 0)
            {
                if (exLogic.User.UserValidation(_objuserlogin.UserName, _objuserlogin.Password) > 0)
                {
                    /*Redirect user to success apge after successfull login*/
                    ViewBag.Message = 1;
                    UserInfo thisUser = exLogic.User.getInfo(_objuserlogin.UserName);
                    Session["FirstName"] = thisUser.FullName;
                    Session["UserID"] = thisUser.UserID;
                    Session["UserName"] = thisUser.UserName;
                    Session["BrandLogo"] = thisUser.BrandLogo;
                    Session["BrandColor"] = thisUser.BrandColor;
                    Session["AccountID"] = thisUser.AccountID;
                    Session["userIpAddress"] = Request.ServerVariables["REMOTE_ADDR"];
                    Session["Lat"] = _objuserlogin.Lat;
                    Session["Lng"] = _objuserlogin.Lng;
                    var browser = Request.Browser.Browser;

                    string sessionId = this.Session.SessionID;
                    string sql = "insert into userlog(UserID,loggedintime,UserIPAddress,Browser,SessionID) values('" + thisUser.UserID + "',getdate(),'" + Session["userIpAddress"] + "','" + browser + "','" + sessionId + "') Select @@Identity";
                    Session["uid"] = Util.InsertSQL(sql);
                    return RedirectToAction("Index", "Home");

                }
                else {
                    ViewBag.Message = 0;
                }
            }
            else {
                ViewBag.Message = 2;
            }


            return View(_objuserlogin);
        }//HttpPost Login()

        public ActionResult Logout()
        {

            string sql = "update UserLog set loggedoftime=getdate() where ID=" + Session["uid"];

            int log = Util.doSQL(sql);

            ViewBag.Title = "Logout";
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return View();
        }//Login()

        public ActionResult UserList()
        {
            if (!exLogic.User.hasAccess("USER.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "User View";
            string SQL = "select a.UserName,a.FirstName,a.MobileNo,b.ProfileName, Count(*) Over() as _TotalRecords ,  a.UserId as _PKey " +
                " from MSTR_User a left join MSTR_Profile b on a.UserProfileId = b.ProfileId  ";

            qView nView = new qView(SQL);
           if (!exLogic.User.hasAccess("USER.SESSIONLOG")) return RedirectToAction("NoAccess", "Home");
            
               nView.addMenu("Session Log", Url.Action("UserLogList", new { ID = "_PKey" }));
            
            if (exLogic.User.hasAccess("USER.VIEW")) nView.addMenu("Detail", Url.Action("UserDetail", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("USER.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("USER.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("PILOTLOG.VIEW")) nView.addMenu("Pilot Log", Url.Action("Detail", "PilotLog", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)

        }
        public ActionResult PilotList()
        {
            if (!exLogic.User.hasAccess("PILOT")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "User View";
            string SQL = "select\n" +
            "  a.UserName,\n" +
            "  a.FirstName,\n" +
            "  a.MobileNo,\n" +
            "  b.ProfileName, \n" +
            "  Count(*) Over() as _TotalRecords ,\n" +
            "  a.UserId as _PKey\n" +
            "from \n" +
            "  MSTR_User a \n" +
            "left join MSTR_Profile b \n" +
            "  on a.UserProfileId = b.ProfileId\n" +
            "where \n" +
            "  a.ispilot=1";
            if (!exLogic.User.hasAccess("DRONE.MANAGE"))
            {
                SQL += "AND\n" +
                  "  a.AccountID=" + Util.getAccountID();
            }

            qView nView = new qView(SQL);
            if (exLogic.User.hasAccess("USER.VIEW")) nView.addMenu("Detail", Url.Action("UserDetail", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("USER.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("USER.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("PILOTLOG.VIEW")) nView.addMenu("Pilot Log", Url.Action("Detail", "PilotLog", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)



        }


        public ActionResult Create([Bind(Prefix = "ID")] int RPASID=0)
        {

            ViewBag.Title = "Create User";
            if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");


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
            if (RPASID!=0)
            {
                ViewBag.RPASid = RPASID;
                ViewBag.IsPassowrdRequired = false;
                var RPASoList = (from p in db.MSTR_RPAS_User where p.RpasId== RPASID select p).ToList();
                EPASValues.FirstName = RPASoList[0].Name;
                EPASValues.CountryId = Convert.ToInt16(RPASoList[0].NationalityId);
                EPASValues.EmiratesID = RPASoList[0].EmiratesId;
                EPASValues.EmailId = RPASoList[0].EmailId;
                EPASValues.MobileNo = RPASoList[0].MobileNo;

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

        public ActionResult UserDetail([Bind(Prefix = "ID")] int UserID = 0)
        {
            if (!exLogic.User.hasAccess("USER.VIEW")) return RedirectToAction("NoAccess", "Home");


            Models.MSTR_User User = db.MSTR_User.Find(UserID);
            if (User == null) return RedirectToAction("Error", "Home");
            ViewBag.Title = User.FirstName;
            return View(User);

        }//UserDetail()

        [ChildActionOnly]
        public ActionResult UserDetailView([Bind(Prefix = "ID")] int UserID = 0)
        {

            string SQL = "SELECT a.[UserName]\n" +
                        " ,a.[FirstName] \n " +
                        ",a.[MiddleName]\n " +
                        ",a.[LastName]\n  " +
                        ",a.[Remarks]\n   " +
                        ",a.[MobileNo]\n  " +
                        ",a.[OfficeNo]\n  " +
                        ",a.[HomeNo]\n" +
                        " ,a.[EmailId]\n  " +
                        ",b.[PassportNo]" +
                        " ,CONVERT(NVARCHAR, b.[DateOfExpiry], 106) AS DateOfExpiry\n   " +
                        ",b.[Department]\n  " +
                        " ,b.[EmiratesId] \n   " +
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

            if (!exLogic.User.hasAccess("DRONE.MANAGE"))
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

        // GET: DroneService/Edit/5
        public ActionResult Edit(int id)
        {
            

            if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
            var viewModel = new ViewModel.UserViewModel
            {
                User = db.MSTR_User.Find(id),
                Pilot = db.MSTR_User_Pilot.Find(id),
                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList(),
                DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };
            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Edit(ViewModel.UserViewModel UserModel)
        {
            String Pass_SQL = "\n";
            if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(UserModel.User.Password) && !String.IsNullOrEmpty(UserModel.User.ConfirmPassword))
                {
                    if (UserModel.User.Password != UserModel.User.ConfirmPassword)
                    {
                        ModelState.AddModelError("User.Password", "Password doesn't match.");
                    }
                    else {
                        Pass_SQL = ",\n  Password='" + Util.GetEncryptedPassword(UserModel.User.Password).ToString() + "'\n";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                string SQL = "UPDATE MSTR_USER SET\n" +
                  "  UserProfileId=" + Util.toInt(UserModel.User.UserProfileId.ToString()) + ",\n" +
                  "  FirstName='" + UserModel.User.FirstName + "',\n" +
                  "  MiddleName='" + UserModel.User.MiddleName + "',\n" +
                  "  LastName='" + UserModel.User.LastName + "',\n" +
                  "  Remarks='" + UserModel.User.Remarks + "',\n" +
                  "  MobileNo='" + UserModel.User.MobileNo + "',\n" +
                  "  EmailId='" + UserModel.User.EmailId + "',\n" +
                  "  CountryId=" + Util.toInt(UserModel.User.CountryId.ToString()) + ",\n" +
                  "  AccountId=" + Util.toInt(UserModel.User.AccountId.ToString()) + ",\n" +
                  "  OfficeNo='" + UserModel.User.OfficeNo + "',\n" +
                  "  HomeNo='" + UserModel.User.HomeNo + "',\n" +
                  "  IsActive='" + UserModel.User.IsActive + "', \n" +
                  "  IsPilot='" + UserModel.User.IsPilot + "',\n" +
                  "  Dashboard= '" + UserModel.User.Dashboard.ToString() +"',\n"+
                  "  PhotoUrl='" + UserModel.User.PhotoUrl + "',\n" +
                  "  RPASPermitNo='" + UserModel.User.RPASPermitNo + "',\n" +
                  "  PermitCategory='" + UserModel.User.PermitCategory + "',\n" +
                  "  ContactAddress='" + UserModel.User.ContactAddress + "',\n" +
                  "  RegRPASSerialNo='" + UserModel.User.RegRPASSerialNo + "',\n" +
                  "  CompanyAddress='" + UserModel.User.CompanyAddress + "',\n" +
                  "  CompanyTelephone='" + UserModel.User.CompanyTelephone + "',\n" +
                  "  CompanyEmail='" + UserModel.User.CompanyEmail + "',\n" +
                  "  TradeLicenceCopyUrl='" + UserModel.User.TradeLicenceCopyUrl + "',\n" +
                  "  EmiratesID='" + UserModel.User.EmiratesID + "'\n" +
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

                return RedirectToAction("UserDetail", new { ID = UserModel.User.UserId });
            }

            var viewModel = new ViewModel.UserViewModel
            {

                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList(),
                DashboardList = Util.GetDashboardLists(),
                PermitCategoryList = Util.GetLists("RPASCategory")
            };
            return View(viewModel);

        }//ActionEdit()


        [HttpPost]
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
                ModelState.AddModelError("User.EmailId", "This email id already exists.");
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
                if(RPASID==0)
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
                  " Dashboard,\n"+
                  " RPASPermitNo,\n" +
                  " PermitCategory,\n" +
                  " ContactAddress,\n" +
                  " RegRPASSerialNo,\n" +
                  " CompanyAddress,\n" +
                  " CompanyTelephone,\n" +
                  " CompanyEmail,\n" +
                  " TradeLicenceCopyUrl,\n" +
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
                  "  " + Util.toInt(UserModel.User.CountryId.ToString()) + ",\n" +
                  "  '" + UserModel.User.IsActive + "',\n" +
                  "  GETDATE(),\n" +
                  "  " + Util.toInt(UserModel.User.AccountId.ToString()) + ",\n" +
                  "  '" + UserModel.User.IsPilot + "',\n" +
                  "  '" + UserModel.User.PhotoUrl + "',\n" +
                  "  '" +(UserModel.User.Dashboard).ToString()+ "',\n" +
                  "  '" + (UserModel.User.RPASPermitNo) + "',\n" +
                  "  '" + (UserModel.User.PermitCategory) + "',\n" +
                  "  '" + (UserModel.User.ContactAddress) + "',\n" +
                  "  '" + (UserModel.User.RegRPASSerialNo) + "',\n" +
                  "  '" + (UserModel.User.ContactAddress) + "',\n" +
                  "  '" + (UserModel.User.CompanyTelephone) + "',\n" +
                  "  '" + (UserModel.User.CompanyEmail) + "',\n" +
                  "  '" + (UserModel.User.TradeLicenceCopyUrl) + "',\n" +
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
                    var mailurl = Url.Action("RPASUserCreated", "Email", new { RpasID = RPASID, UserID = id });
                    var mailsubject = "User has been created";
                    Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, "~"+mailurl);

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
        }//Create() HTTPPost




        public String Delete([Bind(Prefix = "ID")]int UserID = 0)
        {
            if (!exLogic.User.hasAccess("USER.DELETE"))

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



        public ActionResult PilotCertificationCreate([Bind(Prefix = "ID")] int PilotID = 0)
        {

            if (!exLogic.User.hasAccess("PILOTCERT.CREATE")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Create Pilot Certificate";
            MSTR_User_Pilot_Certification PCertification = new MSTR_User_Pilot_Certification();
            PCertification.UserId = PilotID;
            return View(PCertification);
        }

        // POST: user/PilotCertificationCreate
        [HttpPost]
        public ActionResult PilotCertificationCreate(MSTR_User_Pilot_Certification PCertificate)
        {
            if (!exLogic.User.hasAccess("PILOTCERT.CREATE")) return RedirectToAction("NoAccess", "Home");

            if (PCertificate.CertificateId < 1 || PCertificate.CertificateId == null) ModelState.AddModelError("CertificateId", "You must select a Certificate.");
            if (PCertificate.IssuingAuthorityId < 1 || PCertificate.IssuingAuthorityId == null) ModelState.AddModelError("IssuingAuthorityId", "Please Select Issuing Authority.");

            if (ModelState.IsValid)
            {
                int ID = 0;


                ExponentPortalEntities db = new ExponentPortalEntities();
                PCertificate.CreatedBy = Util.getLoginUserID();
                PCertificate.CreatedOn = DateTime.Now;
                db.MSTR_User_Pilot_Certification.Add(PCertificate);

                db.SaveChanges();
                ID = PCertificate.Id;
                db.Dispose();

                return RedirectToAction("UserDetail", new { ID = PCertificate.UserId });
            }
            else {
                ViewBag.Title = "Create Pilot Certification";
                return View(PCertificate);
            }
        }


        public ActionResult PilotCertificationEdit([Bind(Prefix = "ID")] int PCertId = 0)
        {
            if (!exLogic.User.hasAccess("PILOTCERT.EDIT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Edit Pilot Certificate";
            ExponentPortalEntities db = new ExponentPortalEntities();
            MSTR_User_Pilot_Certification PCertificate = db.MSTR_User_Pilot_Certification.Find(PCertId);
            return View(PCertificate);
        }

        [HttpPost]
        public ActionResult PilotCertificationEdit(MSTR_User_Pilot_Certification PCertificate)
        {
            if (!exLogic.User.hasAccess("PILOTCERT.EDIT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Edit Piltot Certificate";
            ExponentPortalEntities db = new ExponentPortalEntities();
            PCertificate.ModifiedBy = Util.getLoginUserID();
            PCertificate.ModifiedOn = DateTime.Now;
            db.Entry(PCertificate).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("UserDetail", new { ID = PCertificate.UserId });

        }


        public String PilotCertificationDelete([Bind(Prefix = "ID")]int PCertId = 0)
        {
            if (!exLogic.User.hasAccess("PILOTCERT.DELETE")) return Util.jsonStat("ERROR", "Access Denied");

            string SQL = "DELETE FROM MSTR_User_Pilot_Certification WHERE Id = " + PCertId;
            Util.doSQL(SQL);


            return Util.jsonStat("OK");

        }


        public ActionResult ExponentCertificationCreate([Bind(Prefix = "ID")] int PilotID = 0)
        {
            if (!exLogic.User.hasAccess("EXPCERT.CREATE")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Create Exponent Certificate";
            MSTR_User_Pilot_ExponentUAS ExpCertification = new MSTR_User_Pilot_ExponentUAS();
            ExpCertification.UserId = PilotID;
            return View(ExpCertification);
        }

        // POST: user/PilotCertificationCreate
        [HttpPost]
        public ActionResult ExponentCertificationCreate(MSTR_User_Pilot_ExponentUAS ExpCertificate)
        {
            if (!exLogic.User.hasAccess("EXPCERT.CREATE")) return RedirectToAction("NoAccess", "Home");

            if (ExpCertificate.CertificateId < 1 || ExpCertificate.CertificateId == null) ModelState.AddModelError("CertificateId", "You must select a Certificate.");


            if (ModelState.IsValid)
            {
                int ID = 0;


                ExponentPortalEntities db = new ExponentPortalEntities();

                ExpCertificate.CreatedBy = Util.getLoginUserID();
                ExpCertificate.CreatedOn = DateTime.Now;
                db.MSTR_User_Pilot_ExponentUAS.Add(ExpCertificate);

                db.SaveChanges();
                ID = ExpCertificate.Id;
                db.Dispose();

                return RedirectToAction("UserDetail", new { ID = ExpCertificate.UserId });
            }
            else {
                ViewBag.Title = "Create Exponent Certification";
                return View(ExpCertificate);
            }
        }


        public ActionResult ExponentCertificationEdit([Bind(Prefix = "ID")] int ExpCertId = 0)
        {
            if (!exLogic.User.hasAccess("EXPCERT.EDIT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Edit Exponent Certificate";
            ExponentPortalEntities db = new ExponentPortalEntities();

            MSTR_User_Pilot_ExponentUAS ExpCertificate = db.MSTR_User_Pilot_ExponentUAS.Find(ExpCertId);
            return View(ExpCertificate);
        }

        [HttpPost]
        public ActionResult ExponentCertificationEdit(MSTR_User_Pilot_ExponentUAS ExpCertificate)
        {
            if (!exLogic.User.hasAccess("EXPCERT.EDIT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Edit Exponent Certificate";
            ExponentPortalEntities db = new ExponentPortalEntities();
            ExpCertificate.ModifiedBy = Util.getLoginUserID();
            ExpCertificate.ModifiedOn = DateTime.Now;
            db.Entry(ExpCertificate).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("UserDetail", new { ID = ExpCertificate.UserId });

        }


        public String ExponentCertificationDelete([Bind(Prefix = "ID")]int ExpCertId = 0)
        {

            if (!exLogic.User.hasAccess("EXPCERT.DELETE")) return Util.jsonStat("ERROR", "Access Denied");
            string SQL = "DELETE FROM  MSTR_User_Pilot_ExponentUAS WHERE Id = " + ExpCertId;
            Util.doSQL(SQL);


            return Util.jsonStat("OK");
        }

        public ActionResult ResetPassword()
        {

            return View();
        }
        public static ExponentPortalEntities ctx = new ExponentPortalEntities();
        [HttpPost]
        public ActionResult ResetPassword(ChangePasswordViewModel password)
        {

            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(password.OldPassword) && String.IsNullOrEmpty(password.NewPassword) && String.IsNullOrEmpty(password.ConfirmPassword)) ModelState.AddModelError("Error", "Enter password");
                {
                    MSTR_User mu = new MSTR_User();
                    string code = Util.GetEncryptedPassword(password.OldPassword).ToString();
                    string sql = "Select password from MSTR_USER where password ='" + code + "' and UserId='" + Session["UserID"] + "'";
                    {
                        if (Util.getDBRows(sql).Count > 0)
                        {
                            try
                            {
                                if (password.NewPassword == password.ConfirmPassword)
                                {
                                    string Password = Util.GetEncryptedPassword(password.NewPassword).ToString();
                                    string SQL = "UPDATE MSTR_User SET Password='" + Password + "' where Userid='" + Session["UserID"] + "'";

                                    int Uid = Util.doSQL(SQL);
                                    {
                                        return View("ChangedSuccessfully");
                                    }
                                }

                                else
                                {
                                    ViewBag.Message = 0;
                                    ViewBag.MessageText = "Password Does not match....";
 
                                }
                            }
                            catch (Exception ex)
                            {
                               
                                ViewBag.Message = 0;
                                ViewBag.MessageText = "Please Enter NewPassword and ConfirmPassword";
                                Console.WriteLine("{0} Exception caught.", ex);

                            }
                            //}
                        }
                        else
                        {
                            ModelState.AddModelError("error", "Old Password is wrong..");
                            ViewBag.Message = 0;
                            ViewBag.MessageText = "Old Password is wrong..";
                        }

                    }
                }
            }
            return View();
        }


        public ActionResult UserLogList([Bind(Prefix = "ID")]int UserID = 0)
        {
            String SQL = "SELECT [ID]\n ,[UserID]\n,[LoggedInTime]\n,[LoggedOfTime]\n ,[UserIPAddress]\n ,[Browser]\n,[SessionID]\n,Count(*) Over() as _TotalRecords,ID as _PKey FROM [ExponentPortal].[dbo].[UserLog] Where UserID = "+ UserID;
            qView nView = new qView(SQL);
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public String ForgotPassword(MSTR_User mSTR_USER)
        {
            if(String.IsNullOrEmpty(mSTR_USER.UserName))
            {
                return  "Please enter your Username/Email";
            }
            else
            {              
                    string unamemail = mSTR_USER.UserName;
                    string sqlcheck = "select EmailId,UserId from MSTR_User where UserName='" + mSTR_USER.UserName + "' or EmailId='" + mSTR_USER.UserName + "'";
                    
                    if (Util.getDBRow(sqlcheck).Count > 0)
                    {
                        var Row = Util.getDBRow(sqlcheck);
                        if (String.IsNullOrEmpty(Row["EmailId"].ToString()))
                        {
                            return "Your Email is not updated in the system,kindly update your Email Id.";
                        }
                        else
                        {
                        var toaddress = Row["EmailId"].ToString();
                        int userid =Convert.ToInt32(Row["UserId"].ToString());              
                        var newpaswd = Util.RandomPassword();
                        string updatepswdsql = "update MSTR_User set GeneratedPassword='" + Util.GetEncryptedPassword(newpaswd).ToString() + "' where EmailId='" + toaddress + "' and UserId="+ userid;
                        int result = Util.doSQL(updatepswdsql);
                        var mailurl = "~/Email/ForgotPassword/" + Session["UserID"] + "?newpassword=" + newpaswd;
                        var mailsubject = "Confidential Mail from Exponent";
                        Util.EmailQue(userid, toaddress, mailsubject, mailurl);
                    }                
                    } 
                        
            }
            return "OK";
        }

    }


}


