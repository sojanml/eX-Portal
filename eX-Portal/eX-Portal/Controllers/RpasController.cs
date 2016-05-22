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
    public ActionResult NoAccess() {
            return View();
        }

    public ActionResult Login([Bind(Prefix = "ID")]int UserID = 0, int Force = 0) {
            //this option allow to register the user to the
            //exponent portal
            bool isPasswordSend = false;
      var Rnd = new Random();

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

    protected override void Dispose(bool disposing) {
      if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    public ActionResult UAS() {
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
          "  U.Type= 'UAVType'\n" +
          "where D.CreatedBy=" + Session["UserId"] + " and D.IsActive=1";

            qView nView = new qView(SQL);
            nView.addMenu("Edit", Url.Action("UASEdit", new { ID = "_Pkey" }));
            nView.addMenu("Delete", Url.Action("DeleteUAS", "Rpas", new { ID = "_Pkey" }));
      if (Request.IsAjaxRequest()) {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
      } else {
                return View(nView);
            }//if(IsAjaxRequest)
        }

        // GET: Rpas/UASRegister
    public ActionResult UASRegister() {
            if (!exLogic.User.hasAccess("RPAS.UASCREATE")) return RedirectToAction("NoAccess", "Home");

            return View();
        }

        // POST: Rpas/UASRegister
        [HttpPost]
    public ActionResult UASRegister(MSTR_Drone mSTR_Drone) {
            if (!exLogic.User.hasAccess("RPAS.UASCREATE")) return RedirectToAction("NoAccess", "Home");

      if (ModelState.IsValid) {
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
    public ActionResult UASEdit(int id = 0) {
            string SQL = "";
            if (!exLogic.User.hasAccess("RPAS.UASEDIT")) return RedirectToAction("NoAccess", "Home");
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + id;
      if (Util.getLoginUserID() == Util.getDBInt(SQL)) {
        if (id == null) {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                MSTR_Drone mSTR_Drone = db.MSTR_Drone.Find(id);
        if (mSTR_Drone == null) {
                    return HttpNotFound();
                }
                return View(mSTR_Drone);
      } else {
                return RedirectToAction("NoAccess", "Home");
            }

        }

        // POST: Rpas/UASEdit
        [HttpPost]
    public ActionResult UASEdit(MSTR_Drone mSTR_Drone) {
            string SQL = "";
            if (!exLogic.User.hasAccess("RPAS.UASEDIT")) return RedirectToAction("NoAccess", "Home");
            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + mSTR_Drone.DroneId;
      if (Util.getLoginUserID() == Util.getDBInt(SQL)) {
        if (ModelState.IsValid) {
                    string updatesql = "update MSTR_Drone set [ManufactureId]=" + mSTR_Drone.ManufactureId +
                                       ",[ModifiedBy] =" + Session["UserID"] + ",[ModifiedOn] ='" + System.DateTime.Now +
                                       "',[CommissionDate] ='" + mSTR_Drone.CommissionDate +
                                       "',RpasSerialNo = '" + mSTR_Drone.RpasSerialNo + "' where[DroneId] =" + mSTR_Drone.DroneId;
                    int result = Util.doSQL(updatesql);
                    return RedirectToAction("UAS");
                }
                return View(mSTR_Drone);
      } else {
                return RedirectToAction("NoAccess", "Home");
            }
        }

    public String DeleteUAS(int? ID = 0) {
            String SQL = "";
            Response.ContentType = "text/json";
            if (!exLogic.User.hasAccess("RPAS.UASDELETE"))
                return Util.jsonStat("ERROR", "Access Denied");


            SQL = "select createdBy from [MSTR_Drone] WHERE DroneId=" + ID;
      if (Util.getLoginUserID() == Util.getDBInt(SQL)) {
                SQL = "DELETE FROM [MSTR_Drone] WHERE DroneId = " + ID;
                Util.doSQL(SQL);

                return Util.jsonStat("OK");
      } else {
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
            if (ID == 0) SQL +=
                 " d.[RpasSerialNo],\n"; //"   d.[DroneName],\n";
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
        public ActionResult FlightRegister(int ID = 0)
        {
            //to create gcaapproval
            if (!exLogic.User.hasAccess("RPAS.FLIGHTCREATE")) return RedirectToAction("NoAccess", "Home");

            var GCAApprovalDoc = new GCA_Approval();
            if (ID != 0)
            {
                GCAApprovalDoc.DroneID = ID;
            }

            if (ID != 0)
            {
                var olist = (from p in db.GCA_Approval where p.ApprovalID == ID select p).ToList();
                if (olist.Count > 0)
                {
                    GCAApprovalDoc.ApprovalID = olist[0].ApprovalID;
                    GCAApprovalDoc.DroneID = olist[0].DroneID;
                    GCAApprovalDoc.ApprovalName = olist[0].ApprovalName;
                    GCAApprovalDoc.Coordinates = olist[0].Coordinates;

                    GCAApprovalDoc.ApprovalDate = olist[0].ApprovalDate == null ? null : olist[0].ApprovalDate;
                    GCAApprovalDoc.StartDate = olist[0].StartDate == null ? null : olist[0].StartDate;
                    GCAApprovalDoc.EndDate = olist[0].EndDate == null ? null : olist[0].EndDate;

                    GCAApprovalDoc.StartTime = olist[0].StartTime;
                    GCAApprovalDoc.EndTime = olist[0].EndTime;
                    GCAApprovalDoc.MinAltitude = olist[0].MinAltitude;

                    GCAApprovalDoc.MaxAltitude = olist[0].MaxAltitude;
                    GCAApprovalDoc.BoundaryInMeters = olist[0].BoundaryInMeters;
                }
            }
            return View(GCAApprovalDoc);

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
        public ActionResult FlightRegister(GCA_Approval GCA)
        {
            if (String.IsNullOrWhiteSpace(GCA.ApprovalName))
            {
                GCA.ApprovalName = toTitle(GCA.ApprovalFileUrl);
            }

            string[] Coord = GCA.Coordinates.Split(',');
            string Poly = GCA.Coordinates + "," + Coord[0];

            if (string.IsNullOrEmpty(GCA.BoundaryInMeters.ToString().Trim()))
                GCA.BoundaryInMeters = 0;

            string SQL = "SELECT Count(*) FROM [GCA_Approval] WHERE ApprovalID = " + GCA.ApprovalID;
            if (Util.getDBInt(SQL) != 0 && GCA.ApprovalID != 0)
            {
                if (!exLogic.User.hasAccess("RPAS.FLIGHTEDIT")) return RedirectToAction("NoAccess", "Home");

                string SQLQ = "Update [GCA_Approval]  set" +
                         "[ApprovalName] = '" + GCA.ApprovalName + "' " +
                        ",[ApprovalDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.ApprovalDate)) + "' " +
                        ",[StartDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.StartDate)) + "' " +
                        ",[EndDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.EndDate)) + "' " +
                        ",[StartTime]= '" + GCA.StartTime + "' " +
                        ",[EndTime]= '" + GCA.EndTime + "' " +
                        ",[Coordinates]= '" + GCA.Coordinates + "' " +
                        ",[Polygon]= geography::STGeomFromText('POLYGON((" + Poly + @"))',4326).MakeValid()  " +
                        ",DroneID= '" + GCA.DroneID + "' " +
                        ",ApprovalFileUrl= '" + GCA.S3Url + "' " +
                        ",MinAltitude= '" + (GCA.MinAltitude == null ? 0 : GCA.MinAltitude) + "' " +
                        ",MaxAltitude= '" + (GCA.MaxAltitude == null ? 60 : GCA.MaxAltitude) + "' " +
                        " WHERE ApprovalID = " + GCA.ApprovalID;
                int res = Util.doSQL(SQLQ);
            }
            else
            {
                if (!exLogic.User.hasAccess("RPAS.FLIGHTCREATE")) return RedirectToAction("NoAccess", "Home");
                SQL = @" insert into [GCA_Approval]
                         ([ApprovalName]
                        ,[ApprovalDate]
                        ,[StartDate]
                        ,[EndDate]
                        ,[StartTime]
                        ,[EndTime]
                        ,[Coordinates]
                        ,[Polygon]
                        ,DroneID
                        ,MinAltitude
                        ,MaxAltitude
                        ,createdBy
                        ,BoundaryInMeters)
                      values
                      ('" + GCA.ApprovalName + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.ApprovalDate)) + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.StartDate)) + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.EndDate)) + @"',
                      '" + GCA.StartTime + @"',
                      '" + GCA.EndTime + @"',
                      '" + GCA.Coordinates + @"',
                      geography::STGeomFromText('POLYGON((" + Poly + @"))',4326).MakeValid(),
                      " + GCA.DroneID + @",
                     " + (GCA.MinAltitude == null ? 0 : GCA.MinAltitude) + @",
                     " + (GCA.MaxAltitude == null ? 60 : GCA.MaxAltitude) + @",
                     " + (Util.getLoginUserID()) + @",50)";

                GCA.ApprovalID = Util.InsertSQL(SQL);

            }
            return RedirectToAction("Flight", "RPAS");
        }

    }
}