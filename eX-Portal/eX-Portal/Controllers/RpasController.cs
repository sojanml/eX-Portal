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
using System.IO;
using System.Text;

namespace eX_Portal.Controllers
{
    
    public class RpasController : Controller
    {
        static String RootUploadDir = "~/Upload/Flight/";
        private ExponentPortalEntities db = new ExponentPortalEntities();

        public ActionResult AllApplications()
        {
            if (!exLogic.User.hasAccess("RPAS.ALL_APPLICATION_LIST")) return RedirectToAction("NoAccess", "Home");
            string SQL = @"Select
        ApprovalID,
        ApprovalName,
        StartDate,
        EndDate,
        StartTime,
        EndTime,
        MaxAltitude,
        MinAltitude,
        case IsUseCamara when 1 then 'Yes' else 'No' end as [Camera being used],
        ISNULL(MSTR_User.FirstName,'') + ' ' + ISNULL(MSTR_User.LastName, '') as FullName,
        ApprovalStatus,
        Count(*) Over() as _TotalRecords,
        ApprovalID as _PKey
      FROM
        GCA_Approval
      LEFT JOIN MSTR_User ON
        MSTR_User.UserID = GCA_Approval.CreatedBy where ApprovalStatus = 'Approved'";

            qView nView = new qView(SQL);
            //if (exLogic.User.hasAccess("PILOTLOG.VIEW"))
            nView.addMenu("Rental", Url.Action("Rental", "Blackbox", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }//public ActionResult Applications() 
        public ActionResult Applications()
        {
            if (!exLogic.User.hasAccess("RPAS.APPLICATION_LIST")) return RedirectToAction("NoAccess", "Home");
            string SQLFilter = @"Select
        ApprovalID,
        ApprovalName,
        StartDate,
        EndDate,
        StartTime,
        EndTime,
        MaxAltitude,
        MinAltitude,        
        case IsUseCamara when 1 then 'Yes' else 'No' end as IsUseCamara,       
        ApprovalStatus as Status,
        Count(*) Over() as _TotalRecords,
        ApprovalID as _PKey
      FROM
        GCA_Approval
      LEFT JOIN MSTR_User ON
        MSTR_User.UserID = GCA_Approval.CreatedBy
       LEFT JOIN MSTR_Drone  on GCA_Approval.DroneId= MSTR_Drone.DroneId";

            if (!exLogic.User.hasAccess("DRONE.MANAGE"))
            {
                if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                {
                    if (SQLFilter != "")
                        SQLFilter += " AND";
                    SQLFilter += " \n" +
                      "  MSTR_Drone.AccountID=" + Util.getAccountID();
                }

            }
            qView nView = new qView(SQLFilter);
            if (exLogic.User.hasAccess("FLIGHTREG.DETAIL")) 
            nView.addMenu("Details", Url.Action("FlightRegistrationDetails", "RPAS", new { ID = "_PKey" }));

            if (exLogic.User.hasAccess("RPAS.APPLICATION"))
            nView.addMenu("Approve/Reject", Url.Action("Application", "RPAS", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("DRONE.AUTHORITY_DOCUMENT"))
                nView.addMenu("Authority Approval", Url.Action("AuthorityApproval","Rpas", new { ID = "ApprovalID" }));
            if (exLogic.User.hasAccess("FLIGHT.SETUP"))
                nView.addMenu("Edit", Url.Action("FlightRegister", "rpas", new { ID = "_PKey", Approval ="Status"  }));
            if (exLogic.User.hasAccess("RPAS.FLIGHTDELETE")) 
            nView.addMenu("Delete", Url.Action("DeleteGCAApproval", "rpas", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }//public ActionResult Applications() 


        public ActionResult FlightRegistrationDetails([Bind(Prefix = "ID")] int ApprovalID = 0)
        {
            if (!exLogic.User.hasAccess("FLIGHTREG.DETAIL")) return RedirectToAction("NoAccess", "Home");
            var Approval = db.GCA_Approval.Find(ApprovalID);
            return View(Approval);

        }
        public ActionResult Application([Bind(Prefix = "ID")] int ApprovalID = 0)
        {
            if (!exLogic.User.hasAccess("RPAS.APPLICATION")) return RedirectToAction("NoAccess", "Home");
            var Approval = db.GCA_Approval.Find(ApprovalID);
            return View(Approval);
        }//public ActionResult Application()

        public ActionResult NoAccessApplication([Bind(Prefix = "ID")] int ID = 0)
        {
           
            var Approval = db.GCA_Approval.Find(ID);
            return View(Approval);
        }//public ActionResult Appl

        [HttpPost]
        public ActionResult Application(GCA_Approval GCA)
        {
            if (!exLogic.User.hasAccess("RPAS.APPLICATION")) return RedirectToAction("NoAccess", "Home");

            string SQL = "update GCA_Approval set ApprovalStatus = '" + GCA.ApprovalStatus + "', ApprovalRemarks = '" + GCA.ApprovalRemarks + "' where ApprovalID = " + GCA.ApprovalID;
            int Val = Util.doSQL(SQL);

            return RedirectToAction("Applications", "Rpas");
        }


        public String UploadFile(int ApprovalID, String DocumentType, String DocumentTitle = "", String DocumentDesc = "")
        {
            String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
            //send information in JSON Format always
            StringBuilder JsonText = new StringBuilder();
            Response.ContentType = "text/json";

            //when there are files in the request, save and return the file information
            try
            {
                var TheFile = Request.Files[0];
                String DroneName = Util.getDroneNameByApplicationID(ApprovalID);
                int DroneID = Util.toInt(Util.getDroneIDByApplicationID(ApprovalID));
                DroneName = DroneName.Replace("*", "");
                String FileName = System.Guid.NewGuid() + "~" + TheFile.FileName;
                String UploadDir = UploadPath + DroneName + "\\" + DocumentType + "\\";
                String FileURL = DroneName + "/" + DocumentType + "/" + FileName;
                String FullName = UploadDir + FileName;

                if (!Directory.Exists(UploadDir))
                    Directory.CreateDirectory(UploadDir);
                TheFile.SaveAs(FullName);
                JsonText.Append("{");
                JsonText.Append(Util.Pair("status", "success", true));
                JsonText.Append(Util.Pair("DocumentTitle", Util.toSQL(DocumentTitle), true));
                JsonText.Append(Util.Pair("DocumentDesc", Util.toSQL(DocumentDesc), true));
                JsonText.Append(Util.Pair("FileURL", Util.toSQL(FileURL), true));
                JsonText.Append("\"addFile\":[");
                JsonText.Append(Util.getFileInfo(FullName));
                JsonText.Append("]}");


                //now add the uploaded file to the database
                String SQL = "INSERT INTO DroneDocuments(\n" +
                  " DroneID,FlightID, DocumentType, DocumentName,\n" +
                  " DocumentTitle, DocumentDesc,\n" +
                  " UploadedDate, UploadedBy\n" +
                  ") VALUES (\n" +
                  "  '" + DroneID + "',\n" +
                   "  '" + ApprovalID + "',\n" +
                  "  '" + DocumentType + "',\n" +
                  "  '" + FileURL + "',\n" +
                  "  '" + Util.toSQL(DocumentTitle) + "',\n" +
                  "  '" + Util.toSQL(DocumentDesc) + "',\n" +
                  "  GETDATE(),\n" +
                  "  " + Util.getLoginUserID() + "\n" +
                  ")";
                Util.doSQL(SQL);

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

        public ActionResult Register()
        {

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


            int RegisterUserID = Util.toInt(Session["RegisterUserID"]);
            if (RegisterUserID <= 0) return View("NoAccess");

            var User = (
            from n in db.MSTR_User
            where n.UserId == RegisterUserID
            select new RPAS_Register
            {
                EmailId = n.EmailId,
                UserName = n.UserName,
                UserId = n.UserId,
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
        public ActionResult Register(RPAS_Register mSTR_User)
        {

            int RegisterUserID = Util.toInt(Session["RegisterUserID"]);
            var InternalInfo = (
              from n in db.MSTR_User
              where n.UserId == RegisterUserID
              select new
              {
                  UserName = n.UserName,
                  EmailId = n.EmailId
              }
            ).FirstOrDefault();
            mSTR_User.UserName = InternalInfo.UserName;
            mSTR_User.EmailId = InternalInfo.EmailId;

            ModelState.Remove("UserName");
            ModelState.Remove("EmailId");

            if (ModelState.IsValid)
            {

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
                "  [EmiratesID] ='" + mSTR_User.EmiratesID + "',\n" +
                "  [GeneratedPassword] =''\n" +
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
        public ActionResult NoAccess()
        {
            return View();
        }

        public ActionResult Login([Bind(Prefix = "ID")]int UserID = 0, int Force = 0)
        {
            //this option allow to register the user to the
            //exponent portal
            bool isPasswordSend = false;
            var Rnd = new Random();

            var UserInfo = (
              from n in db.MSTR_User
              where n.UserId == UserID
              select new
              {
                  Password = n.Password,
                  Mobile = n.MobileNo,
                  FullName = n.FirstName
              }
            ).ToList();

            if (UserInfo.Count <= 0)
            {
                //nothing
            }
            else
            {
                var thisUser = UserInfo.First();
                if (String.IsNullOrEmpty(thisUser.Password) || Force == 1)
                {
                    isPasswordSend = true;
                    var NewPassword = Rnd.Next(10000, 99999).ToString();
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
        public JsonResult Login(String UserName = "", String Password = "")
        {
            //UserName = Request["UserName"];
            //Password = Request["Password"];
            var theResult = new
            {
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
              select new
              {
                  UserID = n.UserId,
                  Mobile = n.MobileNo,
                  FullName = n.FirstName
              }
            ).ToList();
            if (UserInfo.Count < 1) return Json(theResult);

            //Setp 2: If the user is found, then redirect the user to 
            //        Registration page
            Session["RegisterUserID"] = UserInfo.First().UserID;
            theResult = new
            {
                Status = "OK",
                Message = "User logged in successfully. Contine to register..."
            };
            return Json(theResult);

        }

        // GET: Rpas
        public ActionResult Index()
        {
            if (!exLogic.User.hasAccess("RPAS.VIEW")) return RedirectToAction("NoAccess", "Home");

            string sqlprofileid = "select UserProfileId from MSTR_User where [UserId]=" + Session["UserId"];
            int profileid = Util.getDBInt(sqlprofileid);
            if (profileid == 9)
            {
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
                  "where LUP_Drone.Type = 'Country' and (MSTR_RPAS_User.Status='New User Request')";

                qView nView = new qView(SQL);
                //if (exLogic.User.hasAccess("PILOTLOG.VIEW"))
                if (exLogic.User.hasAccess("RPASUSER.CREATE")) nView.addMenu("Create User", Url.Action("Create", "RpasUser", new { ID = "_PKey" }));
                if (exLogic.User.hasAccess("RPASREQUEST.EDIT")) nView.addMenu("Edit", Url.Action("Edit", "Rpas", new { ID = "_PKey" }));
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
            else
            {
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
                if (exLogic.User.hasAccess("RPASUSER.CREATE")) nView.addMenu("Create User", Url.Action("Create", "RpasUser", new { ID = "_PKey" }));
                if (exLogic.User.hasAccess("RPASREQUEST.EDIT")) nView.addMenu("Edit", Url.Action("Edit", "Rpas", new { ID = "_PKey" }));
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

        }

        // GET: Rpas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Create
        public ActionResult Create()
        {
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
                        ViewBag.message = "Registration for this user is already done!!";
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
                var mailurl = "~/Email/RPASRegEmail?RpasID=" + id + "&&CreatedbyID=" + Convert.ToInt32(Session["UserId"].ToString());
                var mailsubject = "New User Creation Request From RPAS Registration";
                Util.EmailQue(Convert.ToInt32(Session["UserId"].ToString()), "info@exponent-ts.com", mailsubject, mailurl);
                return RedirectToAction("Index");
            }

            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!exLogic.User.hasAccess("RPASREQUEST.EDIT")) return RedirectToAction("NoAccess", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            if (mSTR_RPAS_User.Status == "User Created")
            {
                Session["access"] = 1;
            }
            else
            {
                Session["access"] = 0;
            }
            return View(mSTR_RPAS_User);
        }

        // POST: Rpas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MSTR_RPAS_User mSTR_RPAS_User)
        {

            if (!exLogic.User.hasAccess("RPASREQUEST.EDIT")) return RedirectToAction("NoAccess", "Home");
            ModelState.Remove("confirmemailid");
            ModelState.Remove("confirmmobno");

            if (ModelState.IsValid)
            {
                string SQL = "update MSTR_RPAS_User set \n" +
                            "[Name] = '" + mSTR_RPAS_User.Name + "',\n" +
                            "[NationalityId] =" + mSTR_RPAS_User.NationalityId + ",\n" +
                            "[EmiratesId] = '" + mSTR_RPAS_User.EmiratesId + "',\n" +
                            "[EmailId]='" + mSTR_RPAS_User.EmailId + "',\n" +
                            "[MobileNo]='" + mSTR_RPAS_User.MobileNo + "',\n" +
                            "[ModifiedBy]=" + Session["UserID"] + ",\n" +
                            "[ModifiedOn]=getdate()\n" +
                            "where [RpasId] =" + mSTR_RPAS_User.RpasId;
                int result = Util.doSQL(SQL);
                return RedirectToAction("Index");
            }
            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_RPAS_User);
        }

        // POST: Rpas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
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
          "  U.Type= 'UAVType'\n" +
          "where D.CreatedBy=" + Session["UserId"] + " and D.IsActive=1";

            qView nView = new qView(SQL);
            nView.addMenu("Edit", Url.Action("UASEdit", new { ID = "_Pkey" }));
            nView.addMenu("Delete", Url.Action("DeleteUAS", "Rpas", new { ID = "_Pkey" }));
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

        // GET: Rpas/UASRegister
        public ActionResult UASRegister(int ID = 0)
        {

            if (!exLogic.User.hasAccess("RPAS.UASCREATE")) return RedirectToAction("NoAccess", "Home");
            ViewBag.UserExist = false;
            if (ID != 0)
            {
                ViewBag.UserExist = true;
                ViewBag.UserID = ID;
            }
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

                return RedirectToAction("Index", "RpasUser");
            }

            return View(mSTR_Drone);
        }

        // GET: Rpas/UASEdit
        public ActionResult UASEdit(int id = 0)
        {
            string SQL = "";
            if (!exLogic.User.hasAccess("RPAS.UASEDIT")) return RedirectToAction("NoAccess", "Home");
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + id;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                if (id == 0)
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
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + mSTR_Drone.DroneId;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                if (ModelState.IsValid)
                {
                    string updatesql = "update MSTR_Drone set [ManufactureId]=" + mSTR_Drone.ManufactureId +
                                       ",[ModifiedBy] =" + Session["UserID"] + ",[ModifiedOn] ='" + System.DateTime.Now.ToString("MM/dd/yyyy") +
                                       "',[MakeID] ='" + mSTR_Drone.MakeID +
                                       "',[MakeOther] ='" + mSTR_Drone.MakeOther +
                                       "',[ModelID] ='" + mSTR_Drone.ModelID +
                                       "',[ManufactureOther] ='" + mSTR_Drone.ManufactureOther +
                                       "',[color] ='" + mSTR_Drone.color +
                                       "',[MaxAllupWeight] ='" + mSTR_Drone.MaxAllupWeight +
                                       "',[Type] ='" + mSTR_Drone.Type +
                                       "',[RefName] ='" + mSTR_Drone.RefName +
                                       "',[ModelName] ='" + mSTR_Drone.ModelName +
                                       "',[CameraDetails] ='" + mSTR_Drone.CameraDetails +
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

        /// <summary>
        /// Roshan created.
        /// </summary>
        /// <returns></returns>


        public ActionResult Flight(int ID = 0)
        {
            if (!exLogic.User.hasAccess("RPAS.FLIGHT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "View";
            string SQL = @"SELECT ";
            if (ID == 0)
                SQL +=
        " d.[RpasSerialNo],\n"; //"   d.[DroneName],\n";;
            SQL += @"g.[ApprovalName]
                  ,CONVERT(NVARCHAR, g.[StartDate], 103) AS [StartDate]
                  ,CONVERT(NVARCHAR, g.[EndDate], 103) AS [EndDate]
                  ,g.[MinAltitude]
                  ,g.[MaxAltitude]
                  ,Count(*) Over() as _TotalRecords 
                  ,g.[ApprovalID] as _PKey
             from GCA_Approval g join mstr_drone d
             on g.droneID = d.droneID  where g.CreatedBy =" + Util.getLoginUserID();
            if (ID > 0) SQL += @"
             and g.DroneID =" + ID;

            qView nView = new qView(SQL);

            ViewBag.Title = "RPAS Approval";
            ViewBag.DroneID = ID;
            ViewBag.Title += " [" + Util.getDroneName(ID) + "]";


            nView.addMenu("Edit", Url.Action("FlightRegister", "rpas", new { ID = "_PKey" }));
            nView.addMenu("Delete", Url.Action("DeleteGCAApproval", "rpas", new { ID = "_PKey" }));


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



        public ActionResult ApprovalList(int ID = 0)
        {
            if (!exLogic.User.hasAccess("RPAS.FLIGHT")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "View";
            string SQL = @"SELECT ";
            if (ID == 0)
                SQL +=
        " d.[RpasSerialNo],\n"; //"   d.[DroneName],\n";;
            SQL += @"g.[ApprovalName]
                  ,CONVERT(NVARCHAR, g.[StartDate], 103) AS [StartDate]
                  ,CONVERT(NVARCHAR, g.[EndDate], 103) AS [EndDate]
                  ,g.[MinAltitude]
                  ,g.[MaxAltitude]
                  ,Count(*) Over() as _TotalRecords 
                  ,g.[ApprovalID] as _PKey
             from GCA_Approval g join mstr_drone d
             on g.droneID = d.droneID  where d.AccountID=" + Util.getAccountID();
            if (ID > 0) SQL += @"
             and g.DroneID =" + ID;


            qView nView = new qView(SQL);

            ViewBag.Title = "Approval List";
            ViewBag.DroneID = ID;
            ViewBag.Title += " [" + Util.getDroneName(ID) + "]";


            nView.addMenu("Edit", Url.Action("FlightRegister", "rpas", new { ID = "_PKey" }));
            nView.addMenu("Delete", Url.Action("DeleteGCAApproval", "rpas", new { ID = "_PKey" }));


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
        public ActionResult Complete()
        {
            return View();
        }
        [HttpPost]
        public String Upload(DroneDocument Doc)
        {
            //Doc.DocumentTitle = Doc.DocumentTitle.Trim();
            if (String.IsNullOrWhiteSpace(Doc.DocumentTitle))
            {
                Doc.DocumentTitle = toTitle(Doc.S3Url);
            }
            if (String.IsNullOrEmpty(Doc.DocumentName))
            {
                Doc.DocumentName = Doc.DocumentTitle;
            }
            String SQL = @"INSERT INTO [DroneDocuments] (
        [DroneID],
        [DocumentType],
        [DocumentName],
        [UploadedDate],
        [UploadedBy],
        [AccountID],
        [DocumentDate],
        [DocumentTitle],
        [DocumentDesc],
        [S3Url]
        ) VALUES (
        '" + Doc.DroneID.ToString() + @"',
        '" + Doc.DocumentType + @"',
        '" + Doc.DocumentName + @"',
        GETDATE(),
        '" + Util.getAccountID() + @"',
        '" + getDroneAccount(Doc.DroneID) + @"',
        '" + Util.toSQLDate(Doc.DocumentDate.ToString()) + @"',
        '" + Doc.DocumentTitle + @"',
        '" + Doc.DocumentDesc + @"',
        '" + Doc.S3Url + @"'
        )
      ";
            Doc.ID = Util.InsertSQL(SQL);
            return Url.Action("Document", "Drone", new { ID = Doc.ID });
        }
        public string getDroneAccount(int DroneID)
        {
            String SQL = "SELECT AccountID From MSTR_Drone WHERE DroneID=" + DroneID;
            return Util.getDBVal(SQL);
        }
        private string toTitle(String S3url)
        {
            var SlashAt = S3url.LastIndexOf('/');
            var LastDot = S3url.LastIndexOf('.');
            var FileOnly = S3url.Substring(SlashAt + 1, LastDot - SlashAt);
            var UKeyEnd = FileOnly.IndexOf('_');
            return FileOnly.Substring(UKeyEnd + 1);
        }
        public ActionResult FlightRegister([Bind(Prefix = "ID")]int  ID=0)
        {
            //to create gcaapproval
            if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                return RedirectToAction("NoAccess", "Home");

            if(Util.IsGcaApproved(ID))
            {
                return RedirectToAction("NoAccessApplication", "Rpas",new { ID=ID });
            }
                

            ViewData["accountid"] = Convert.ToInt32(Session["AccountID"].ToString());

            var viewModel = new ViewModel.FlightSetupViewModel
            {



                GcaApproval = db.GCA_Approval.Find(ID)

               
            };
            return View(viewModel);

        }

        //flight Registration Approval

        public ActionResult AuthorityApproval([Bind(Prefix = "ID")] int ApprovalID = 0)
        {
            if (!exLogic.User.hasAccess("FLIGHT.AUTHORITY_DOCUMENT"))
                return RedirectToAction("NoAccess", "Home");
            ViewBag.ApprovalID = ApprovalID;
          
            return View();
        }

        [ChildActionOnly]
        public ActionResult AuthorityDocuments(int ApprovalID = 0, String Authority = "DCAA")
        {
            ViewBag.ApprovalID = ApprovalID;
            ViewBag.Authority = Authority;
            
         int   DroneID = Util.toInt( Util.getDroneIDByApplicationID(ApprovalID));
            List<DroneDocument> Docs = (
              from o in db.DroneDocuments
              where
              o.DocumentType == "Flight-Registration" &&
              o.DroneID == DroneID &&
              o.FlightID== ApprovalID &&
              o.DocumentTitle == Authority
              select o).ToList();

            return View(Docs);
        }


        public String DeleteFile([Bind(Prefix = "ID")] int ApprovalID, String file)
        {
            bool isDeleted = false;
            StringBuilder JsonText = new StringBuilder();
            JsonText.Append("{");

            if (!exLogic.User.hasAccess("FLIGHT.AUTHORITY_DOCUMENT"))
            {
                JsonText.Append(Util.Pair("status", "error", true));
                JsonText.Append(Util.Pair("message", "You do not have access to UAS Documents", false));
            }
            else
            {
                int DroneID = Util.toInt(Util.getDroneIDByApplicationID(ApprovalID));
                string DroneName=Util.getDroneNameByApplicationID(ApprovalID);
                String SQL = "SELECT Count(*) FROM DroneDocuments\n" +
                  "WHERE\n" +
                  "  DocumentName='" + file + "' AND\n" +
                  "  DroneID = '" + DroneID + "'";
                int DocCount = Util.getDBInt(SQL);
                if (DocCount > 0)
                {
                    String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
                    String FullPath = Path.Combine(UploadPath, file);
                    if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                        //now add the uploaded file to the database
                        SQL = "DELETE FROM DroneDocuments\n" +
                        "WHERE\n" +
                        "  DocumentName='" + file + "' AND\n" +
                        "  DroneID = '" + DroneID + "'";
                        Util.doSQL(SQL);
                        isDeleted = true;
                        JsonText.Append(Util.Pair("status", "success", true));
                        JsonText.Append(Util.Pair("message", "Deleted successfully.", false));
                    }
                }
            }
            if (!isDeleted)
            {
                JsonText.Append(Util.Pair("status", "error", true));
                JsonText.Append(Util.Pair("message", "Can not delete the file from server.", false));
            }
            JsonText.Append("}");

            return JsonText.ToString();
        }


        public String DeleteGCAApproval(int? ID = 0)
        {
            String SQL = "";
            Response.ContentType = "text/json";

            if (!exLogic.User.hasAccess("RPAS.FLIGHTDELETE")) return Util.jsonStat("ERROR", "Access Denied");

            SQL = "select createdBy from [GCA_Approval] WHERE ApprovalID=" + ID;
            if (Util.getLoginUserID() == Util.getDBInt(SQL))
            {
                SQL = "DELETE FROM [GCA_Approval] WHERE ApprovalID = " + ID;
                Util.doSQL(SQL);

                return Util.jsonStat("OK");
            }
            else
            {
                return Util.jsonStat("Access", "No Access");
            }
        }
        [HttpPost]
        public String FlightRegister(FlightSetupViewModel flightsetupvm)
        {
            try
            {
                if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                   // return RedirectToAction("NoAccess", "Home");
                return "You do not have accesss to this page";
                if (flightsetupvm.GcaApproval.DroneID == null || flightsetupvm.GcaApproval.DroneID < 1)
                   
               return "You must select a Drone.";
                if (flightsetupvm.GcaApproval.PilotUserId < 1 || flightsetupvm.GcaApproval.PilotUserId == null)
                   
                 return "You must select a pilot.";
                if (flightsetupvm.GcaApproval.GroundStaffUserId < 1 || flightsetupvm.GcaApproval.GroundStaffUserId == null)
                 //   return RedirectToAction("NoAccess", "Home");
                  return "A Ground staff should be selected.";
               
                    if (flightsetupvm.camera == null)
                {
                    if (flightsetupvm.GcaApproval.CameraId == null)
                        flightsetupvm.GcaApproval.CameraId = 0;
                            DateTime todaydate = System.DateTime.Now;
                                    String SQL = String.Empty;
                                    var StartDate = (flightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(-1) : (DateTime)flightsetupvm.GcaApproval.StartDate);
                                    var EndDate = (flightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(90) : (DateTime)flightsetupvm.GcaApproval.EndDate);
                                    var MinAltitude = (flightsetupvm.GcaApproval.MinAltitude == null ? 0 : flightsetupvm.GcaApproval.MinAltitude);
                                    var MaxAltidute = (flightsetupvm.GcaApproval.MaxAltitude == null ? 40 : flightsetupvm.GcaApproval.MaxAltitude);


                                    int ApprovalID = flightsetupvm.GcaApproval.ApprovalID;
                                    String ApprovalName = flightsetupvm.GcaApproval.ApprovalName;
                                    String Coordinates = flightsetupvm.GcaApproval.Coordinates;
                                    if (String.IsNullOrEmpty(Coordinates))
                                        Coordinates =
                              "24.949901 55.337585," +
                              "25.218555 55.620971," +
                              "25.387706 55.414978," +
                              "25.087092 55.137084";
                                    string[] Coord = Coordinates.Split(',');
                                    string Poly = Coordinates + "," + Coord[0];

                                    if (ApprovalID == 0 && String.IsNullOrEmpty(ApprovalName))
                                    {
                                        //Approval is not selected or no name is specifeid
                                        //then do not update approvals
                                        SQL = String.Empty;
                                    }
                                    else if (!String.IsNullOrEmpty(ApprovalName) && ApprovalID == 0)
                                    {
                                        //when a new name is specified for approval
                                        //save it as new approval     
                                        SQL = @"insert into GCA_Approval(
                            ApprovalName,
                            ApprovalDate,
                            StartDate,
                            EndDate,
                            Coordinates,
                            Polygon,
                            CreatedOn,
                            CreatedBy,
                            DroneID,
                            EndTime,
                            StartTime,
                            BoundaryInMeters,
                            MinAltitude,
                            MaxAltitude,
                            IsUseCamara,
                            PilotUserId,
                            GroundStaffUserId,
                            NotificationEmails,
                            CameraId
                          ) values(
                            '" + flightsetupvm.GcaApproval.ApprovalName + @"',
                            GETDATE(),
                            '" + StartDate.ToString("yyyy-MM-dd") + @"',
                            '" + EndDate.ToString("yyyy-MM-dd") + @"',
                            '" + Coordinates + @"',
                            geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
                            GETDATE(),
                            " + Session["UserID"] + "," +
                                            flightsetupvm.GcaApproval.DroneID + @",
                            '" + flightsetupvm.GcaApproval.EndTime + @"',
                            '" + flightsetupvm.GcaApproval.StartTime + @"',
                            50,
                            " + MinAltitude + @",
                            " + MaxAltidute + @",
                            " + flightsetupvm.GcaApproval.IsUseCamara + @",
                            " + flightsetupvm.GcaApproval.PilotUserId + @",
                            " + flightsetupvm.GcaApproval.GroundStaffUserId + @",
                            '" + flightsetupvm.GcaApproval.NotificationEmails + @"',
                            " + flightsetupvm.GcaApproval.CameraId + @"
                          )";
                                        //
                                    }
                                    else
                                    {
                                        //Got an approval ID 
                                        //Update the selected Approval ID
                                        SQL = @"Update 
                            [GCA_Approval] 
                          set 
                            StartDate ='" + StartDate.ToString("yyyy-MM-dd") + @"',
                            EndDate = '" + EndDate.ToString("yyyy-MM-dd") + @"',
                            Coordinates  = '" + Coordinates + @"',
                            Polygon=geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
                            EndTime='" + flightsetupvm.GcaApproval.EndTime + @"',
                            StartTime='" + flightsetupvm.GcaApproval.StartTime + @"',
                            BoundaryInMeters=50,
                            MinAltitude = " + MinAltitude + @",
                            MaxAltitude = " + MaxAltidute + @",
                            IsUseCamara= " + flightsetupvm.GcaApproval.IsUseCamara + @",
                            PilotUserId=" + flightsetupvm.GcaApproval.PilotUserId + @",
                            GroundStaffUserId=" + flightsetupvm.GcaApproval.GroundStaffUserId + @",
                            NotificationEmails='" + flightsetupvm.GcaApproval.NotificationEmails + @"'
                            CameraId='" + flightsetupvm.GcaApproval.CameraId + @"'
                          where 
                            ApprovalID=" + ApprovalID;
                                    }
                                    if (!String.IsNullOrEmpty(SQL))
                                    {
                                        //Execute the sql statement generated
                                        Util.doSQL(SQL);
                                    }

                                    int DroneID = Util.toInt(flightsetupvm.GcaApproval.DroneID);
                                    SQL = "select DroneSetupId from MSTR_Drone_Setup where DroneId=" + DroneID;
                                    int DroneSetupId = Util.getDBInt(SQL);
                                    if (DroneSetupId == 0)
                                    {
                                        SQL = @"INSERT INTO MSTR_Drone_Setup (
                            DroneID,
                            CreatedBy,
                            CreatedOn,
                            [ModifiedOn]
                          ) VALUES (
                            " + DroneID + @",
                            " + Session["UserID"] + @",
                            GETDATE(),
                            GETDATE()
                          )";
                                        Util.doSQL(SQL);
                                    }
                                    //        if (flightsetupvm.DroneSetup.BatteryVoltage == null)
                                    //            flightsetupvm.DroneSetup.BatteryVoltage = 0;

                                    SQL = @"update 
                         MSTR_Drone_Setup 
                        set 
                          PilotUserId=" + flightsetupvm.GcaApproval.PilotUserId + @",
                          GroundStaffUserId=" + flightsetupvm.GcaApproval.GroundStaffUserId + @",        
                          [ModifiedBy]=" + Util.getLoginUserID() + @",
                         [ModifiedOn]=GETDATE(),
                         [NotificationEmails]='" + flightsetupvm.GcaApproval.NotificationEmails + @"'
                        where 
                         [DroneId]=" + DroneID;
                                    Util.doSQL(SQL);
                                    //  return RedirectToAction("Applications", "Rpas","");
                }
                else
                {
                                            int typeid = Util.getDBInt("SELECT Max(TypeId) + 1 from [LUP_Drone] where [Type]='Camera'");
                                            string BinaryCode = Util.DecToBin(typeid);
                                            string s = flightsetupvm.camera.ToString();
                                            string code = s;
                                            string SQL1 = "INSERT INTO LUP_DRONE(\n" +
                                            "  Type,\n" +
                                            "  Code,\n" +
                                            "  TypeId,\n" +
                                            "  BinaryCode,\n" +
                                            "  Name,\n" +
                                            "  CreatedBy,\n" +
                                            "  CreatedOn,\n" +
                                            "  IsActive\n" +
                                            ") VALUES(\n" +
                                            "  'Camera',\n" +
                                            "  '" + code + "',\n" +
                                            "  " + typeid + ",\n" +
                                            "  '" + BinaryCode + "',\n" +
                                            "  '" + flightsetupvm.camera + "',\n" +
                                            "  " + Util.getLoginUserID() + ",\n" +
                                            "  GETDATE(),\n" +
                                            "  'True'" +
                                            ")";
                                            int cameraid = Util.InsertSQL(SQL1);

                                            DateTime todaydate = System.DateTime.Now;
                                            String SQL = String.Empty;
                                            var StartDate = (flightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(-1) : (DateTime)flightsetupvm.GcaApproval.StartDate);
                                            var EndDate = (flightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(90) : (DateTime)flightsetupvm.GcaApproval.EndDate);
                                            var MinAltitude = (flightsetupvm.GcaApproval.MinAltitude == null ? 0 : flightsetupvm.GcaApproval.MinAltitude);
                                            var MaxAltidute = (flightsetupvm.GcaApproval.MaxAltitude == null ? 40 : flightsetupvm.GcaApproval.MaxAltitude);


                                            int ApprovalID = flightsetupvm.GcaApproval.ApprovalID;
                                            String ApprovalName = flightsetupvm.GcaApproval.ApprovalName;
                                            String Coordinates = flightsetupvm.GcaApproval.Coordinates;
                                            if (String.IsNullOrEmpty(Coordinates))
                                                Coordinates =
                                      "24.949901 55.337585," +
                                      "25.218555 55.620971," +
                                      "25.387706 55.414978," +
                                      "25.087092 55.137084";
                                            string[] Coord = Coordinates.Split(',');
                                            string Poly = Coordinates + "," + Coord[0];

                                            if (ApprovalID == 0 && String.IsNullOrEmpty(ApprovalName))
                                            {
                                                //Approval is not selected or no name is specifeid
                                                //then do not update approvals
                                                SQL = String.Empty;
                                            }
                                            else if (!String.IsNullOrEmpty(ApprovalName) && ApprovalID == 0)
                                            {
                                                //when a new name is specified for approval
                                                //save it as new approval     
                                                SQL = @"insert into GCA_Approval(
                                    ApprovalName,
                                    ApprovalDate,
                                    StartDate,
                                    EndDate,
                                    Coordinates,
                                    Polygon,
                                    CreatedOn,
                                    CreatedBy,
                                    DroneID,
                                    EndTime,
                                    StartTime,
                                    BoundaryInMeters,
                                    MinAltitude,
                                    MaxAltitude,
                                    IsUseCamara,
                                    PilotUserId,
                                    GroundStaffUserId,
                                    NotificationEmails,
                                    CameraId
                                  ) values(
                                    '" + flightsetupvm.GcaApproval.ApprovalName + @"',
                                    GETDATE(),
                                    '" + StartDate.ToString("yyyy-MM-dd") + @"',
                                    '" + EndDate.ToString("yyyy-MM-dd") + @"',
                                    '" + Coordinates + @"',
                                    geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
                                    GETDATE(),
                                    " + Session["UserID"] + "," +
                                                    flightsetupvm.GcaApproval.DroneID + @",
                                    '" + flightsetupvm.GcaApproval.EndTime + @"',
                                    '" + flightsetupvm.GcaApproval.StartTime + @"',
                                    50,
                                    " + MinAltitude + @",
                                    " + MaxAltidute + @",
                                    " + flightsetupvm.GcaApproval.IsUseCamara + @",
                                    " + flightsetupvm.GcaApproval.PilotUserId + @",
                                    " + flightsetupvm.GcaApproval.GroundStaffUserId + @",
                                    '" + flightsetupvm.GcaApproval.NotificationEmails + @"'
                                    '" + cameraid + @"'

                                  )";
                                                //
                                            }
                                            else
                                            {
                                                //Got an approval ID 
                                                //Update the selected Approval ID
                                                SQL = @"Update 
                                    [GCA_Approval] 
                                  set 
                                    StartDate ='" + StartDate.ToString("yyyy-MM-dd") + @"',
                                    EndDate = '" + EndDate.ToString("yyyy-MM-dd") + @"',
                                    Coordinates  = '" + Coordinates + @"',
                                    Polygon=geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
                                    EndTime='" + flightsetupvm.GcaApproval.EndTime + @"',
                                    StartTime='" + flightsetupvm.GcaApproval.StartTime + @"',
                                    BoundaryInMeters=50,
                                    MinAltitude = " + MinAltitude + @",
                                    MaxAltitude = " + MaxAltidute + @",
                                    IsUseCamara= " + flightsetupvm.GcaApproval.IsUseCamara + @",
                                    PilotUserId=" + flightsetupvm.GcaApproval.PilotUserId + @",
                                    GroundStaffUserId=" + flightsetupvm.GcaApproval.GroundStaffUserId + @",
                                    NotificationEmails='" + flightsetupvm.GcaApproval.NotificationEmails + @"'
                                    CameraId='" + cameraid + @"'
                                  where 
                                    ApprovalID=" + ApprovalID;
                                            }
                                            if (!String.IsNullOrEmpty(SQL))
                                            {
                                                //Execute the sql statement generated
                                                Util.doSQL(SQL);
                                            }

                                            int DroneID = Util.toInt(flightsetupvm.GcaApproval.DroneID);
                                            SQL = "select DroneSetupId from MSTR_Drone_Setup where DroneId=" + DroneID;
                                            int DroneSetupId = Util.getDBInt(SQL);
                                            if (DroneSetupId == 0)
                                            {
                                                SQL = @"INSERT INTO MSTR_Drone_Setup (
                                    DroneID,
                                    CreatedBy,
                                    CreatedOn,
                                    [ModifiedOn]
                                  ) VALUES (
                                    " + DroneID + @",
                                    " + Session["UserID"] + @",
                                    GETDATE(),
                                    GETDATE()
                                  )";
                                                Util.doSQL(SQL);
                                            }
                                            //        if (flightsetupvm.DroneSetup.BatteryVoltage == null)
                                            //            flightsetupvm.DroneSetup.BatteryVoltage = 0;

                                            SQL = @"update 
                                 MSTR_Drone_Setup 
                                set 
                                  PilotUserId=" + flightsetupvm.GcaApproval.PilotUserId + @",
                                  GroundStaffUserId=" + flightsetupvm.GcaApproval.GroundStaffUserId + @",        
                                  [ModifiedBy]=" + Util.getLoginUserID() + @",
                                 [ModifiedOn]=GETDATE(),
                                 [NotificationEmails]='" + flightsetupvm.GcaApproval.NotificationEmails + @"'
                                where 
                                 [DroneId]=" + DroneID;
                                            Util.doSQL(SQL);
                }
                    return "OK";
                
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public ActionResult FlightDCAARegister([Bind(Prefix = "ID")]int ID = 0)


        {
            //to create gcaapproval
            if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                return RedirectToAction("NoAccess", "Home");

            if (Util.IsGcaApproved(ID))
            {
                return RedirectToAction("NoAccessApplication", "Rpas", new { ID = ID });
            }
            ViewData["accountid"] = Convert.ToInt32(Session["AccountID"].ToString());
            string sSql = "select count(*) from ApproalDetail where Authority= 'DCAA'";
            int SerialNo = Util.getDBInt(sSql)+1;
            ViewData["NextSerialNo"] = SerialNo;
            var viewModel = new ViewModel.DCAAFlightApproval
            {
                GcaApproval = db.GCA_Approval.Find(ID),
                ApprovalDetails = db.ApproalDetails.FirstOrDefault(i => i.ApprovalID == ID)
        };
            return View(viewModel);

        }

        [HttpPost]
        public String FlightDCAARegister(DCAAFlightApproval DCAAFlightsetupvm)
        {
            try
            {
                if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                    // return RedirectToAction("NoAccess", "Home");
                    return "You do not have accesss to this page";
                if (DCAAFlightsetupvm.GcaApproval.DroneID == null || DCAAFlightsetupvm.GcaApproval.DroneID < 1)

                    return "You must select a Drone.";
                if (DCAAFlightsetupvm.GcaApproval.PilotUserId < 1 || DCAAFlightsetupvm.GcaApproval.PilotUserId == null)

                    return "You must select a pilot.";
                if (DCAAFlightsetupvm.GcaApproval.GroundStaffUserId < 1 || DCAAFlightsetupvm.GcaApproval.GroundStaffUserId == null)
                    //   return RedirectToAction("NoAccess", "Home");
                    return "A Ground staff should be selected.";

                DateTime todaydate = System.DateTime.Now;
                String SQL = String.Empty;
                String SQL1 = String.Empty;
                var StartDate = (DCAAFlightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(-1) : (DateTime)DCAAFlightsetupvm.GcaApproval.StartDate);
                var EndDate = (DCAAFlightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(90) : (DateTime)DCAAFlightsetupvm.GcaApproval.EndDate);
                var MinAltitude = (DCAAFlightsetupvm.GcaApproval.MinAltitude == null ? 0 : DCAAFlightsetupvm.GcaApproval.MinAltitude);
                var MaxAltidute = (DCAAFlightsetupvm.GcaApproval.MaxAltitude == null ? 40 : DCAAFlightsetupvm.GcaApproval.MaxAltitude);


                int ApprovalID = DCAAFlightsetupvm.GcaApproval.ApprovalID;
                String ApprovalName = DCAAFlightsetupvm.GcaApproval.ApprovalName;
                String Coordinates = DCAAFlightsetupvm.GcaApproval.Coordinates;
                if (String.IsNullOrEmpty(Coordinates))
                    Coordinates =
          "24.949901 55.337585," +
          "25.218555 55.620971," +
          "25.387706 55.414978," +
          "25.087092 55.137084";
                string[] Coord = Coordinates.Split(',');
                string Poly = Coordinates + "," + Coord[0];

                if (ApprovalID == 0 && String.IsNullOrEmpty(ApprovalName))
                {
                    //Approval is not selected or no name is specifeid
                    //then do not update approvals
                    SQL = String.Empty;
                }
                else if (!String.IsNullOrEmpty(ApprovalName) && ApprovalID == 0)
                {
                    //when a new name is specified for approval
                    //save it as new approval     
                    SQL = @"insert into GCA_Approval(
            ApprovalName,
            ApprovalDate,
            StartDate,
            EndDate,
            Coordinates,
            Polygon,
            CreatedOn,
            CreatedBy,
            DroneID,
            EndTime,
            StartTime,
            BoundaryInMeters,
            MinAltitude,
            MaxAltitude,
            IsUseCamara,
            PilotUserId,
            GroundStaffUserId,
            NotificationEmails

          ) values(
            '" + DCAAFlightsetupvm.GcaApproval.ApprovalName + @"',
            GETDATE(),
            '" + StartDate.ToString("yyyy-MM-dd") + @"',
            '" + EndDate.ToString("yyyy-MM-dd") + @"',
            '" + Coordinates + @"',
            geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
            GETDATE(),
            " + Session["UserID"] + "," +
                        DCAAFlightsetupvm.GcaApproval.DroneID + @",
            '" + DCAAFlightsetupvm.GcaApproval.EndTime + @"',
            '" + DCAAFlightsetupvm.GcaApproval.StartTime + @"',
            50,
            " + MinAltitude + @",
            " + MaxAltidute + @",
            " + DCAAFlightsetupvm.GcaApproval.IsUseCamara + @",
            " + DCAAFlightsetupvm.GcaApproval.PilotUserId + @",
            " + DCAAFlightsetupvm.GcaApproval.GroundStaffUserId + @",
            '" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'


          )";
                    //write insert here
                    string ssSQL = "select max(ApprovalID) from GCA_Approval";
                    int maxApprovalID = Util.getDBInt(ssSQL) +1;

                    string sSql = "select case count(*) when 0 then 1 else count(*) end from ApproalDetail where Authority= 'DCAA'";
                    int SerialNo = Util.getDBInt(sSql);

                    SQL1 = @"insert into ApproalDetail(
            ApprovalID,
            Authority,
            SerialNo,
            CompanyName,
            Address,
            Fax,
            Telephone,
            POC,
            TypeOfAreal,
            AircraftType,
            AircraftTailNo,
            Registration,
            CallSign,
            DepartureAerodrome,
            DestinationAerodrome
          ) values(
            '" + maxApprovalID + @"',
            '" + "DCAA" + @"',
            '" + SerialNo + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.CompanyName + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Address + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Fax + @"',
           '" + DCAAFlightsetupvm.ApprovalDetails.Telephone + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.POC + @"',
             '" + DCAAFlightsetupvm.ApprovalDetails.TypeOfAreal + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AircraftType + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AircraftTailNo + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Registration + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.CallSign + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.DepartureAerodrome + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.DestinationAerodrome + @"'
          )";

                }
                else
                {
                    //Got an approval ID 
                    //Update the selected Approval ID
                    
                    SQL = @"Update 
            [GCA_Approval] 
          set 
            StartDate ='" + StartDate.ToString("yyyy-MM-dd") + @"',
            EndDate = '" + EndDate.ToString("yyyy-MM-dd") + @"',
            Coordinates  = '" + Coordinates + @"',
            Polygon=geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
            EndTime='" + DCAAFlightsetupvm.GcaApproval.EndTime + @"',
            StartTime='" + DCAAFlightsetupvm.GcaApproval.StartTime + @"',
            BoundaryInMeters=50,
            MinAltitude = " + MinAltitude + @",
            MaxAltitude = " + MaxAltidute + @",
            IsUseCamara= " + DCAAFlightsetupvm.GcaApproval.IsUseCamara + @",
            PilotUserId=" + DCAAFlightsetupvm.GcaApproval.PilotUserId + @",
            GroundStaffUserId=" + DCAAFlightsetupvm.GcaApproval.GroundStaffUserId + @",
            NotificationEmails='" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'
          where 
            ApprovalID=" + ApprovalID;

                    SQL1 = @"Update 
            [ApproalDetail] 
          set 
            CompanyName='" + DCAAFlightsetupvm.ApprovalDetails.CompanyName + @"',
            Address='" + DCAAFlightsetupvm.ApprovalDetails.Address + @"',
            Fax='" + DCAAFlightsetupvm.ApprovalDetails.Fax + @"',
            Telephone='" + DCAAFlightsetupvm.ApprovalDetails.Telephone + @"',
            POC='" + DCAAFlightsetupvm.ApprovalDetails.POC + @"',
            TypeOfAreal='" + DCAAFlightsetupvm.ApprovalDetails.TypeOfAreal + @"',
            AircraftType='" + DCAAFlightsetupvm.ApprovalDetails.AircraftType + @"',
            AircraftTailNo='" + DCAAFlightsetupvm.ApprovalDetails.AircraftTailNo + @"',
            Registration='" + DCAAFlightsetupvm.ApprovalDetails.Registration + @"',
            CallSign='" + DCAAFlightsetupvm.ApprovalDetails.CallSign + @"',
            DepartureAerodrome='" + DCAAFlightsetupvm.ApprovalDetails.DepartureAerodrome + @"',
            DestinationAerodrome='" + DCAAFlightsetupvm.ApprovalDetails.DestinationAerodrome + @"'
          where 
            ApprovalID=" + ApprovalID;

                }
                if (!String.IsNullOrEmpty(SQL))
                {
                    //Execute the sql statement generated
                    Util.doSQL(SQL);
                }
                if (!String.IsNullOrEmpty(SQL1))
                {
                    Util.doSQL(SQL1);
                }
                //------------ Here new code Starts




                //------------ Here new code Ends


                int DroneID = Util.toInt(DCAAFlightsetupvm.GcaApproval.DroneID);
                SQL = "select DroneSetupId from MSTR_Drone_Setup where DroneId=" + DroneID;
                int DroneSetupId = Util.getDBInt(SQL);
                if (DroneSetupId == 0)
                {
                    SQL = @"INSERT INTO MSTR_Drone_Setup (
            DroneID,
            CreatedBy,
            CreatedOn,
            [ModifiedOn]
          ) VALUES (
            " + DroneID + @",
            " + Session["UserID"] + @",
            GETDATE(),
            GETDATE()
          )";
                    Util.doSQL(SQL);
                }
               
                SQL = @"update 
         MSTR_Drone_Setup 
        set 
          PilotUserId=" + DCAAFlightsetupvm.GcaApproval.PilotUserId + @",
          GroundStaffUserId=" + DCAAFlightsetupvm.GcaApproval.GroundStaffUserId + @",        
          [ModifiedBy]=" + Util.getLoginUserID() + @",
         [ModifiedOn]=GETDATE(),
         [NotificationEmails]='" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'
        where 
         [DroneId]=" + DroneID;
                Util.doSQL(SQL);
                //  return RedirectToAction("Applications", "Rpas","");
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }



        public ActionResult FlightGCAARegister([Bind(Prefix = "ID")]int ID = 0)


        {
            //to create gcaapproval
            if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                return RedirectToAction("NoAccess", "Home");

            if (Util.IsGcaApproved(ID))
            {
                return RedirectToAction("NoAccessApplication", "Rpas", new { ID = ID });
            }
            ViewData["accountid"] = Convert.ToInt32(Session["AccountID"].ToString());
            string sSql = "select count(*) from ApproalDetail where Authority= 'GCAA'";
            int SerialNo = Util.getDBInt(sSql) + 1;
            ViewData["NextSerialNo"] = SerialNo;
            var viewModel = new ViewModel.GCAAFlightApproval
            {
                GcaApproval = db.GCA_Approval.Find(ID),
                ApprovalDetails = db.ApproalDetails.FirstOrDefault(i => i.ApprovalID == ID)
            };
            return View(viewModel);

        }

        [HttpPost]
        public String FlightGCAARegister(GCAAFlightApproval DCAAFlightsetupvm)
        {
            try
            {
                //if (!exLogic.User.hasAccess("FLIGHT.SETUP"))
                //    // return RedirectToAction("NoAccess", "Home");
                //    return "You do not have accesss to this page";
                //if (DCAAFlightsetupvm.GcaApproval.DroneID == null || DCAAFlightsetupvm.GcaApproval.DroneID < 1)

                //    return "You must select a Drone.";
                //
                //if (DCAAFlightsetupvm.GcaApproval.PilotUserId < 1 || DCAAFlightsetupvm.GcaApproval.PilotUserId == null)
                //    return "You must select a pilot.";

                //if (DCAAFlightsetupvm.GcaApproval.GroundStaffUserId < 1 || DCAAFlightsetupvm.GcaApproval.GroundStaffUserId == null)
                //    //   return RedirectToAction("NoAccess", "Home");
                //    return "A Ground staff should be selected.";

                DateTime todaydate = System.DateTime.Now;
                String SQL = String.Empty;
                String SQL1 = String.Empty;
                var StartDate = (DCAAFlightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(-1) : (DateTime)DCAAFlightsetupvm.GcaApproval.StartDate);
                var EndDate = (DCAAFlightsetupvm.GcaApproval.StartDate == null ? DateTime.Now.AddDays(90) : (DateTime)DCAAFlightsetupvm.GcaApproval.EndDate);
                var MinAltitude = (DCAAFlightsetupvm.GcaApproval.MinAltitude == null ? 0 : DCAAFlightsetupvm.GcaApproval.MinAltitude);
                var MaxAltidute = (DCAAFlightsetupvm.GcaApproval.MaxAltitude == null ? 40 : DCAAFlightsetupvm.GcaApproval.MaxAltitude);


                int ApprovalID = DCAAFlightsetupvm.GcaApproval.ApprovalID;
                String ApprovalName = DCAAFlightsetupvm.GcaApproval.ApprovalName;
                String Coordinates = DCAAFlightsetupvm.GcaApproval.Coordinates;
                if (String.IsNullOrEmpty(Coordinates))
                    Coordinates =
          "24.949901 55.337585," +
          "25.218555 55.620971," +
          "25.387706 55.414978," +
          "25.087092 55.137084";
                string[] Coord = Coordinates.Split(',');
                string Poly = Coordinates + "," + Coord[0];

                if (ApprovalID == 0 && String.IsNullOrEmpty(ApprovalName))
                {
                    //Approval is not selected or no name is specifeid
                    //then do not update approvals
                    SQL = String.Empty;
                }
                else if (!String.IsNullOrEmpty(ApprovalName) && ApprovalID == 0)
                {
                    //when a new name is specified for approval
                    //save it as new approval     
                    SQL = @"insert into GCA_Approval(
            ApprovalName,
            ApprovalDate,
            StartDate,
            EndDate,
            Coordinates,
            Polygon,
            CreatedOn,
            CreatedBy,
            DroneID,
            EndTime,
            StartTime,
            BoundaryInMeters,
            MinAltitude,
            MaxAltitude,
            IsUseCamara,
            PilotUserId,
            GroundStaffUserId,
            NotificationEmails

          ) values(
            '" + DCAAFlightsetupvm.GcaApproval.ApprovalName + @"',
            GETDATE(),
            '" + StartDate.ToString("yyyy-MM-dd") + @"',
            '" + EndDate.ToString("yyyy-MM-dd") + @"',
            '" + Coordinates + @"',
            geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
            GETDATE(),
            " + Session["UserID"] + "," +
                        DCAAFlightsetupvm.GcaApproval.DroneID + @",
            '" + DCAAFlightsetupvm.GcaApproval.EndTime + @"',
            '" + DCAAFlightsetupvm.GcaApproval.StartTime + @"',
            50,
            " + MinAltitude + @",
            " + MaxAltidute + @",
            " + (DCAAFlightsetupvm.GcaApproval.IsUseCamara==null ? 0 : DCAAFlightsetupvm.GcaApproval.IsUseCamara) + @",
            " + (DCAAFlightsetupvm.GcaApproval.PilotUserId == null ? 0 : DCAAFlightsetupvm.GcaApproval.PilotUserId) + @",
            " + (DCAAFlightsetupvm.GcaApproval.GroundStaffUserId == null ? 0 : DCAAFlightsetupvm.GcaApproval.GroundStaffUserId) + @",
            '" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'


          )";
                    //write insert here
                    string ssSQL = "select max(ApprovalID) from GCA_Approval";
                    int maxApprovalID = Util.getDBInt(ssSQL) + 1;

                    string sSql = "select case count(*) when 0 then 1 else count(*) end from ApproalDetail where Authority= 'GCAA'";
                    int SerialNo = Util.getDBInt(sSql);

                    SQL1 = @"insert into ApproalDetail(
            ApprovalID,
            Authority,
            SerialNo,
            CompanyName,
            POC,
            Title,
            Address,
            Fax,
            Telephone,
            AuthorityPOC,
            AuthorityTitle,
            AuthorityPhone,
            AuthorityEmail,
            AuthorityTelephoneNo,
            AuthorityFaxNo,
            TypeOfOperation,
            DescOfAreaLocation,
            PurposeOfOperation,
            IsCamara,
            IsCamaraDesc,
            IsStakeholderConsultation,
            IsStakeholderConsultationDesc,
            AirtraficImpact,
            Comments
          ) values(
            '" + maxApprovalID + @"',
            '" + "GCAA" + @"',
            '" + SerialNo + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.CompanyName + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.POC + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Title + @"',
           '" + DCAAFlightsetupvm.ApprovalDetails.Address + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Fax + @"',
             '" + DCAAFlightsetupvm.ApprovalDetails.Telephone + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityPOC + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityTitle + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityPhone + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityEmail + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityTelephoneNo + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityFaxNo + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.TypeOfOperation + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.DescOfAreaLocation + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.PurposeOfOperation + @"',
            " + (DCAAFlightsetupvm.ApprovalDetails.IsCamara==true ? 1 : 0) + @",
            '" + DCAAFlightsetupvm.ApprovalDetails.IsCamaraDesc + @"',
            " + (DCAAFlightsetupvm.ApprovalDetails.IsStakeholderConsultation == true ? 1 : 0) + @",
            '" + DCAAFlightsetupvm.ApprovalDetails.IsStakeholderConsultationDesc + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.AirtraficImpact + @"',
            '" + DCAAFlightsetupvm.ApprovalDetails.Comments + @"'
          )";

                }
                else
                {
                    //Got an approval ID 
                    //Update the selected Approval ID

                    SQL = @"Update 
            [GCA_Approval] 
          set 
            StartDate ='" + StartDate.ToString("yyyy-MM-dd") + @"',
            EndDate = '" + EndDate.ToString("yyyy-MM-dd") + @"',
            Coordinates  = '" + Coordinates + @"',
            Polygon=geography::STGeomFromText('POLYGON((" + Poly + @"))', 4326).MakeValid(),
            EndTime='" + DCAAFlightsetupvm.GcaApproval.EndTime + @"',
            StartTime='" + DCAAFlightsetupvm.GcaApproval.StartTime + @"',
            BoundaryInMeters=50,
            MinAltitude = " + MinAltitude + @",
            MaxAltitude = " + MaxAltidute + @",
            IsUseCamara= " + DCAAFlightsetupvm.GcaApproval.IsUseCamara + @",
            PilotUserId=" + DCAAFlightsetupvm.GcaApproval.PilotUserId + @",
            GroundStaffUserId=" + DCAAFlightsetupvm.GcaApproval.GroundStaffUserId + @",
            NotificationEmails='" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'
          where 
            ApprovalID=" + ApprovalID;
                    
                    SQL1 = @"Update 
            [ApproalDetail] 
          set 
            CompanyName = '" + DCAAFlightsetupvm.ApprovalDetails.CompanyName + @"',
            POC = '" + DCAAFlightsetupvm.ApprovalDetails.POC + @"',
            Title = '" + DCAAFlightsetupvm.ApprovalDetails.Title + @"',
            Address = '" + DCAAFlightsetupvm.ApprovalDetails.Address + @"',
            Fax = '" + DCAAFlightsetupvm.ApprovalDetails.Fax + @"',
            Telephone = '" + DCAAFlightsetupvm.ApprovalDetails.Telephone + @"',
            AuthorityPOC = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityPOC + @"',
            AuthorityTitle = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityTitle + @"',
            AuthorityPhone = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityPhone + @"',
            AuthorityEmail = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityEmail + @"',
            AuthorityTelephoneNo = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityTelephoneNo + @"',
            AuthorityFaxNo = '" + DCAAFlightsetupvm.ApprovalDetails.AuthorityFaxNo + @"',
            TypeOfOperation= '" + DCAAFlightsetupvm.ApprovalDetails.TypeOfOperation + @"',
            DescOfAreaLocation= '" + DCAAFlightsetupvm.ApprovalDetails.DescOfAreaLocation + @"',
            PurposeOfOperation= '" + DCAAFlightsetupvm.ApprovalDetails.PurposeOfOperation + @"',
            IsCamara= " + (DCAAFlightsetupvm.ApprovalDetails.IsCamara == true ? 1 : 0) + @",
            IsCamaraDesc= '" + DCAAFlightsetupvm.ApprovalDetails.IsCamaraDesc + @"',
            IsStakeholderConsultation= " + (DCAAFlightsetupvm.ApprovalDetails.IsStakeholderConsultation == true ? 1 : 0) + @",
            IsStakeholderConsultationDesc= '" + DCAAFlightsetupvm.ApprovalDetails.IsStakeholderConsultationDesc + @"',
            AirtraficImpact= '" + DCAAFlightsetupvm.ApprovalDetails.AirtraficImpact + @"',
            Comments= '" + DCAAFlightsetupvm.ApprovalDetails.Comments + @"'
          where
            ApprovalID = " + ApprovalID;

                }
                if (!String.IsNullOrEmpty(SQL))
                {
                    //Execute the sql statement generated
                    Util.doSQL(SQL);
                }
                if (!String.IsNullOrEmpty(SQL1))
                {
                    Util.doSQL(SQL1);
                }
                //------------ Here new code Starts




                //------------ Here new code Ends


                int DroneID = Util.toInt(DCAAFlightsetupvm.GcaApproval.DroneID);
                SQL = "select DroneSetupId from MSTR_Drone_Setup where DroneId=" + DroneID;
                int DroneSetupId = Util.getDBInt(SQL);
                if (DroneSetupId == 0)
                {
                    SQL = @"INSERT INTO MSTR_Drone_Setup (
            DroneID,
            CreatedBy,
            CreatedOn,
            [ModifiedOn]
          ) VALUES (
            " + DroneID + @",
            " + Session["UserID"] + @",
            GETDATE(),
            GETDATE()
          )";
                    Util.doSQL(SQL);
                }

                SQL = @"update 
         MSTR_Drone_Setup 
        set 
          PilotUserId=" + (DCAAFlightsetupvm.GcaApproval.PilotUserId == null ? 0 : DCAAFlightsetupvm.GcaApproval.PilotUserId) + @",
          GroundStaffUserId=" + (DCAAFlightsetupvm.GcaApproval.GroundStaffUserId == null ? 0 : DCAAFlightsetupvm.GcaApproval.GroundStaffUserId) + @",        
          [ModifiedBy]=" + Util.getLoginUserID() + @",
         [ModifiedOn]=GETDATE(),
         [NotificationEmails]='" + DCAAFlightsetupvm.GcaApproval.NotificationEmails + @"'
        where 
         [DroneId]=" + DroneID;
                Util.doSQL(SQL);
                //  return RedirectToAction("Applications", "Rpas","");
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}