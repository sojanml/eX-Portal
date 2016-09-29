using eX_Portal.exLogic;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace eX_Portal.Controllers {
  public class BlackBoxController : Controller {
    static String RootUploadDir = "~/Upload/BlackBox/";
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: BlackBox

    public ActionResult Index() {
      if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "FDR Data";

      String SQL = "SELECT \n" +
        "  MSTR_Drone.DroneName as UAS,\n" +
        "  BBFlightID as FdrFlightId,\n" +
        "  Min([BlackBoxData].ReadTime) as StartTime,\n" +
        "  Max([BlackBoxData].ReadTime) as EndTime,\n" +
        "  Max(Speed) as MaxSpeed,\n" +
        "  CASE isnumeric(Max(TotalFlightTime))\n" +
        "    WHEN 1 THEN cast(round(CONVERT(numeric(12, 3), Max(TotalFlightTime)) / 60.0, 2) as numeric(36, 2))\n" +
        "    ELSE 0.00\n" +
        "  END as TotalFlightTime, \n " +
        "  Max(Altitude) as MaxAltitude,\n" +
        "  Count(*) Over() as _TotalRecords,\n" +
        "  Cast([BlackBoxData].DroneId as varchar) + ',' + Cast(BBFlightID as varchar) as _Pkey\n" +
        "FROM\n" +
        "  [BlackBoxData]\n" +
        "LEFT JOIN MSTR_Drone ON\n" +
        "  MSTR_Drone.DroneId = [BlackBoxData].DroneId\n" +
        "WHERE\n" +
        "  Speed > 0.00";
                if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                {
                    SQL += " AND\n" +
                      "  MSTR_Drone.AccountID=" + Util.getAccountID();
                }
      SQL = SQL + "\n" +
        "GROUP BY\n" +
        "  [BlackBoxData].DroneId,\n" +
        "  MSTR_Drone.DroneName,\n" +
        "  BBFlightID\n";

      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Index()


    public ActionResult Live() {
      if (!exLogic.User.hasAccess("BLACKBOX.LIVE")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "FDR Live Data";
      string SQL =
        "SELECT  \n" +
        "  [DroneDataId] as UASDataId ," +
        "  MSTR_Drone.DroneName as UAS,\n" +
        "  [ReadTime] as [Date],\n" +
        "  [DroneData].[Latitude] ,\n" +
        "  [DroneData].[Longitude],\n" +
        "  [Altitude] as [Altitude],\n" +
        "  [Speed] as [Speed],\n" +
        "  [FixQuality],\n" +
        "  [Satellites],\n" +
        "  [Pitch],[Roll],[Heading],[TotalFlightTime],\n" +
        "  [BBFlightID],\n" +
        "  Count(*) Over() as _TotalRecords,[DroneDataId] as _PKey\n" +
        "FROM\n" +
        "  [DroneData]\n" +
        "LEFT JOIN MSTR_Drone ON\n" +
        "  MSTR_Drone.DroneID = [DroneData].DroneID";
            /*
                    "  CASE isnumeric(TotalFlightTime)\n" +
                    "    WHEN 1 THEN cast(round(CONVERT(numeric(12, 3), TotalFlightTime) / 60.0, 2) as numeric(36, 2))\n" +
                    "    ELSE 0.00\n" +
                    "  END as TotalFlightTime, \n " +
            */
            if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
            {
                SQL += " AND\n" +
                  "  MSTR_Drone.AccountID=" + Util.getAccountID() + "\n" +
                  "WHERE\n" +
                  "  MSTR_Drone.AccountID=" + Util.getAccountID();
            }
            //   SQL+= " Order By UasDataid";


            qView nView = new qView(SQL);
      //if (!exLogic.User.hasAccess("BLACKBOX.LIVE")) nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Index()

    public ActionResult Detail([Bind(Prefix = "ID")] String DroneID_BBFlightID = "") {
      if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");
      String[] SplitData = DroneID_BBFlightID.Split(',');
      if (SplitData.Length != 2) return RedirectToAction("Error");
      int DroneID = Util.toInt(SplitData[0]);
      int BBFlightID = Util.toInt(SplitData[1]);
      if (DroneID < 1 || BBFlightID < 1) return RedirectToAction("Error");
      ViewBag.Title = "FDR  Data";
      ViewBag.DroneID = DroneID;

      String SQL =
       "SELECT \n" +
       "  RecordNumber,\n" +
       "  ReadTime,\n" +
       "  Latitude,\n" +
       "  Longitude,\n" +
       "  Altitude,\n" +
       "  Speed,\n" +
       "  FixQuality,\n" +
       "  Satellites,\n" +
       "  Pitch,\n" +
       "  Roll,\n" +
       "  Heading,\n" +
       "  TotalFlightTime,\n" +
       "  Count(*) OVER() as _TotalRecords\n" +
       "FROM\n" +
       "  BlackBoxData\n" +
       "WHERE\n" +
       "  DroneID=" + DroneID + " AND\n" +
       "  BBFlightID=" + BBFlightID;

      qView nView = new qView(SQL);

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult Upload() {
      if (!exLogic.User.hasAccess("BLACKBOX.UPLOAD")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Upload";
      return View();
    }//upload()



    public String Delete([Bind(Prefix = "file")] String FileName) {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      StringBuilder JsonText = new StringBuilder();
      String FullName = UploadPath + FileName;

      Response.ContentType = "text/json";
      try {
        System.IO.File.Delete(FullName);
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "ok", true));
        JsonText.Append(Util.Pair("message", "Deleted", false));
        JsonText.Append("}");
      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }

    public String Save() {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FullName = UploadPath + TheFile.FileName;

        if (!Directory.Exists(UploadPath)) Directory.CreateDirectory(UploadPath);
        TheFile.SaveAs(FullName);
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "success", true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(Util.getFileInfo(FullName));
        JsonText.Append("]}");
      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(Util.Pair("status", "error", true));
        JsonText.Append(Util.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//Save()

    public String getFiles() {
      StringBuilder JsonText = new StringBuilder();
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      if (!Directory.Exists(UploadPath))
        Directory.CreateDirectory(UploadPath);
      JsonText.Append("{");
      JsonText.Append(Util.Pair("status", "success", true));
      JsonText.Append("\"addFile\":[");
      bool isAddComma = false;
      foreach (string file in Directory.EnumerateFiles(UploadPath, "*.*")) {
        if (isAddComma) JsonText.Append(",\n");
        JsonText.Append(Util.getFileInfo(file));
        isAddComma = true;
      }
      JsonText.Append("]}");
      return JsonText.ToString();
    }//getFiles()

    public String Import(String file) {
      String Status = "ok";
      String StatusMessage = "Completed Successfully";
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));

      BlackBox Process = new BlackBox();
      StringBuilder JsonText = new StringBuilder();
      int ImportedRows = 0;
      //String newFile = Process.FixImportFile(UploadPath + file);
      try {
        ImportedRows = Process.BulkInsert(UploadPath + file);
        StatusMessage = "Imported " + ImportedRows + " Rows";
      } catch (Exception ex) {
        Status = "error";
        StatusMessage = ex.Message;
      }

      JsonText.AppendLine("{");
      JsonText.AppendLine(Util.Pair("status", Status, true));
      JsonText.AppendLine(Util.Pair("message", StatusMessage, false));
      JsonText.AppendLine("}");

      return JsonText.ToString();
    }

    //Get:BlackBox/Create
    public ActionResult Create() {
      //if (!exLogic.User.hasAccess("BLACKBOX.CREATE")) return RedirectToAction("NoAccess", "Home");
      //ViewBag.IsAESVisible = false;
      //if (exLogic.User.hasAccess("BLACKBOX.AES"))
      //    ViewBag.IsAESVisible = true;

      MSTR_BlackBox BB = new MSTR_BlackBox();
      return View(BB);
    }

    // POST: BlackBox/Create
    [HttpPost]
    public ActionResult Create(Models.MSTR_BlackBox BlackBoxData) {
      //   if (!exLogic.User.hasAccess("BLACLBOX.CREATE")) return RedirectToAction("NoAccess", "Home");
      if (ModelState.IsValid) {
        BlackBox BB = new BlackBox();
        //  BlackBox.BlackBoxID = 0;
        BlackBoxData.IsActive = 1;
        BlackBoxData.CurrentStatus = "IN";
        BlackBoxData.LastReceiveId = 0;
        BlackBoxData.LastRentalId = 0;
        BlackBoxData.CreatedBy = Util.getLoginUserID();
        BlackBoxData.CreatedOn = DateTime.Now;
        BlackBoxData.LastUpdateDate = DateTime.Now;
        BlackBoxData.BlackBoxCode = BB.getSerialNumber(BlackBoxData.BlackBoxSerial).ToString();
        db.MSTR_BlackBox.Add(BlackBoxData);

        db.SaveChanges();

        db.Dispose();

        return RedirectToAction("BlackBoxList", "BlackBox");
      } else {
        ViewBag.Title = "Create BlackBox";
        return View(BlackBoxData);
      }
    }

    public ActionResult BlackBoxList() {

      //      if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Blackbox";

      string SQL = @"SELECT  m.[BlackBoxID] as _PKey
                            ,m.[LastRentalId]
                          ,m.[BlackBoxSerial]
                          ,m.[BlackBoxName]
                          ,m.[CurrentStatus]
                          ,isnull(u.Firstname,'') +' '+ isnull(u.lastname,'') as CreatedBy
                          ,d.RpasSerialNo
                            , Count(*) Over() as _TotalRecords
                      FROM  MSTR_BlackBox m left join mstr_user u
                      on m.CreatedBy = u.userid
                      left join mstr_drone d
                      on d.droneid = m.currentDroneID where m.[IsActive] = 1";
      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("BlackBoxDetails", new { ID = "_PKey" }));
      nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
      nView.addMenu("Receive", Url.Action("ReceiveBlackBox", new { ID = "LastRentalId" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//PartsBlackBoxlist

    // GET: Parts/Edit/5
    public ActionResult Edit(int id) {
      if (!exLogic.User.hasAccess("BLACKBOX.EDIT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Edit Blackbox";
      ViewBag.IsAESVisible = false;
      if (exLogic.User.hasAccess("BLACKBOX.AES"))
        ViewBag.IsAESVisible = true;

      //    ExponentPortalEntities db = new ExponentPortalEntities();
      MSTR_BlackBox BB = db.MSTR_BlackBox.Find(id);

      return View(BB);

    }

    // POST: BlackBox/Edit/5
    [HttpPost]
    public ActionResult Edit(MSTR_BlackBox BB) {
      try {
        // TODO: Add update logic here
        if (!exLogic.User.hasAccess("BlackBox.EDIT")) return RedirectToAction("NoAccess", "Home");
        if (ModelState.IsValid) {
          ViewBag.Title = "Edit BlackBox";
          BB.CreatedBy = Util.getLoginUserID();
          BB.LastUpdateDate = DateTime.Now;

          db.Entry(BB).State = EntityState.Modified;

          db.SaveChanges();
          return RedirectToAction("BlackBoxList", "BlackBox");
        } else {
          return View(BB);
        }

      } catch {
        return View(BB);
      }
    }

    // GET: BlackBox/Details/5
    public String BBDetails(int id) {
      // if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return Util.jsonStat("ERROR", "Access Denied");
      string SQL = @"SELECT  [BlackBoxID] as _PKey
                          ,[BlackBoxSerial]
                          ,[BlackBoxName]
                          ,[IsActive]
                          ,[CreatedOn]
                          ,[CreatedBy]
                          ,[CurrentStatus]
                          ,[CurrentUserID]
                          ,[LastUpdateDate]
                          ,[CurrentDroneID]
                          , Count(*) Over() as _TotalRecords ";
      //if (exLogic.User.hasAccess("BLACKBOX.AES"))
      //      SQL = SQL + ",[EncryptionKey]";
      SQL = SQL + @"
                            FROM  MSTR_BlackBox
                           where BlackBoxID = " + id;

      qDetailView nView = new qDetailView(SQL);
      ViewBag.Message = nView.getTable();
      ViewBag.Title = "Black Box Details";
      return nView.getTable();
    }

    public ActionResult BlackBoxDetails([Bind(Prefix = "ID")] int BlackBoxID) {
      // if (!exLogic.User.hasAccess("PARTS.VIEW")) return RedirectToAction("NoAccess", "Home");

      Models.MSTR_BlackBox BB = db.MSTR_BlackBox.Find(BlackBoxID);
      ViewBag.EncryptionKeyStatus = false;
      if (exLogic.User.hasAccess("BLACKBOX.AES")) {
        ViewBag.EncryptionKeyStatus = true;
        if (BB != null)
          ViewBag.EncryptionKey = BB.EncryptionKey;
      }
      if (BB == null) return RedirectToAction("Error", "Home");
      ViewBag.Title = "Black Box Details";
      return View(BB);
    }

    //by BT
    public ActionResult TransactionDet([Bind(Prefix = "ID")] int BlackBoxID) {
            //string SQL = @"SELECT MSTR_BlackBox.BlackBoxName,
            //                     BlackBoxTransaction.BBStatus as Status,
            //                     BlackBoxTransaction.CollectionMode as 'TransactionMode',
            //                     BlackBoxTransaction.BankName as 'BankName',
            //                     BlackBoxTransaction.Amount, 
            //                     BlackBoxTransaction.ChequeNumber as 'ChequeNumber',
            //                     BlackBoxTransaction.DateOfCheque as 'ChequeDate', 
            //                     BlackBoxTransaction.NameOnCard as 'NameOnCard',
            //                     BlackBoxTransaction.CreatedDate as 'CreatedDate',
            //                     MSTR_Drone.DroneName as 'DroneName',
            //                     BlackBoxTransaction.Note,
            //                     Count(*) Over() as _TotalRecords,
            //                     BlackBoxTransaction.ID as _PKey                           
            //                     FROM
            //                     BlackBoxTransaction LEFT OUTER JOIN
            //                     MSTR_BlackBox ON BlackBoxTransaction.BlackBoxID = MSTR_BlackBox.BlackBoxID LEFT OUTER JOIN
            //                     MSTR_Drone ON BlackBoxTransaction.DroneID = MSTR_Drone.DroneId 
            //                     WHERE(BlackBoxTransaction.BlackBoxID = " + BlackBoxID + ")";



            string SQL = @"SELECT MSTR_BlackBox.BlackBoxName,
                                 BlackBoxTransaction.BBStatus as Status,                               
                                 BlackBoxTransaction.Amount,                              
                                 MSTR_Drone.DroneName as 'DroneName',
                                 BlackBoxTransaction.Note,
                                 Count(*) Over() as _TotalRecords,
                                 BlackBoxTransaction.ID as _PKey                           
                                 FROM
                                 BlackBoxTransaction LEFT OUTER JOIN
                                 MSTR_BlackBox ON BlackBoxTransaction.BlackBoxID = MSTR_BlackBox.BlackBoxID LEFT OUTER JOIN
                                 MSTR_Drone ON BlackBoxTransaction.DroneID = MSTR_Drone.DroneId 
                                 WHERE(BlackBoxTransaction.BlackBoxID = " + BlackBoxID + ")";


            qView nView = new qView(SQL);

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text /javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }


    // GET: BlackBox/Issue/5
    public ActionResult Rental([Bind(Prefix = "ID")] int approvalid = 0) {
      if (!exLogic.User.hasAccess("BLACKBOX.RENT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Blackbox Rental";
      var dronedet = (from d in db.GCA_Approval
                      where d.ApprovalID == approvalid
                      select d).ToList();
      if (dronedet.Count > 0) {
        ViewBag.CreatedBy = dronedet[0].CreatedBy == null ? "" : dronedet[0].CreatedBy.ToString();
        ViewBag.DroneId = dronedet[0].DroneID == null ? "" : dronedet[0].DroneID.ToString();

      }
      //string sqldronedet = "select * from GCA_Approval where ApprovalID="+ approvalid;
      //var Row=Util.getDBRow(sqldronedet);
      //if (Row.Count > 1)
      //{
      //    ViewBag.CreatedBy = Row["CreatedBy"]==null?  "" :Row["CreatedBy"];
      //    ViewBag.DroneId = Row["DroneId"]==null?"":Row["DroneID"];
      //}

      return View();
    }

        [HttpPost]
        public ActionResult Rental(BlackBoxTransaction BBTransaction)
        {
            if (!exLogic.User.hasAccess("BLACKBOX.RENT")) return RedirectToAction("NoAccess", "Home");
            ModelState.Remove("Note");
            ModelState.Remove("VerifyCode");
            if (ModelState.IsValid)
            {
                BBTransaction.BBStatus = "OUT";
                BBTransaction.CreatedBy = Session["UserID"].ToString();
                BBTransaction.DroneID = Convert.ToInt32(TempData["Droneid"]);
                BBTransaction.ApprovalID = Convert.ToInt32(TempData["Approvalid"]);
                

                string insertsql = "insert into[BlackBoxTransaction] ([BlackBoxID] \n" +
                         ",[BBStatus] \n" +
                         ",[CollectionMode] \n" +
                         ",[NameOnCard] \n" +
                         ",[BankName] \n" +
                         ",[Amount] \n" +
                         ",[ChequeNumber] \n" +
                         ",[CreatedDate] \n" +
                         ",[CreatedBy] \n" +
                         ",[DroneID] \n" +
                         ",[RentType] \n" +
                         ",[RentAmount] \n" +
                         ",[ApprovalID])values( \n" +
                         BBTransaction.BlackBoxID + ",'OUT','" + BBTransaction.CollectionMode + "','" + BBTransaction.NameOnCard + "','" + BBTransaction.BankName + "',\n" +
                         BBTransaction.Amount + ",'" + BBTransaction.ChequeNumber + "',"+"sysdatetime()"+",'\n" +
                         BBTransaction.CreatedBy + "'," + BBTransaction.DroneID + ",'" + BBTransaction.RentType + "','" + BBTransaction.RentAmount + "'," + BBTransaction.ApprovalID + ")";
                int bbtransctionid = Util.InsertSQL(insertsql);
                string bbupdatesql = "update [MSTR_BlackBox] set [LastRentalId]=" + bbtransctionid + ",[CurrentStatus]='OUT',CurrentUserID=" + Convert.ToInt32(Session["UserID"].ToString()) + ",CurrentDroneID=" + Convert.ToInt32(TempData["Droneid"]) + " where [BlackBoxID]=" + BBTransaction.BlackBoxID;
                Util.doSQL(bbupdatesql);
                string droneupdatesql = "update [MSTR_Drone] set [BlackBoxID]=" + BBTransaction.BlackBoxID + " where [DroneId]=" + BBTransaction.DroneID;
                Util.doSQL(droneupdatesql);
                return RedirectToAction("AllApplications", "Rpas");
            }
            return View();
        }

        [HttpGet]
        public JsonResult BlackBoxTransAmountPaidVal([Bind(Prefix = "ID")] int BlackBoxID = 0)
        {
            var lastID = (
                from y in db.MSTR_BlackBox
                where y.BlackBoxID == BlackBoxID
                select y.LastRentalId
                  ).FirstOrDefault();

            BlackBoxTransaction btx = (
                from n in db.BlackBoxTransactions
                where n.ID == lastID
                select n
              ).FirstOrDefault();

            decimal? Amount = 0;
            if (btx != null)
                Amount = btx.Amount;

            return Json(Amount, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult BlackBoxTransDroneVal([Bind(Prefix = "ID")] int BlackBoxID = 0)
        {
            var lastID = (
                from y in db.MSTR_BlackBox
                where y.BlackBoxID == BlackBoxID
                select y.LastRentalId
                  ).FirstOrDefault();

            BlackBoxTransaction btx = (
                from n in db.BlackBoxTransactions
                where n.ID == lastID
                select n
              ).FirstOrDefault();


            int? DroneID=0;
            if (btx != null)
                DroneID = btx.DroneID;

            return Json(DroneID, JsonRequestBehavior.AllowGet);

        }

        public int GetLastTransactionID(int BlackBoxID = 0)
        {try
            {
                var lastID = (
                   from y in db.MSTR_BlackBox
                   where y.BlackBoxID == BlackBoxID
                   select y.LastRentalId
                     ).FirstOrDefault();

                BlackBoxTransaction btx = (
                    from n in db.BlackBoxTransactions
                    where n.ID == lastID
                    select n
                  ).FirstOrDefault();
                return btx.ID;
            }
            catch(Exception Ex)
            {
                return 0;
            }
        }

        [HttpGet]
        public JsonResult BlackBoxInfo(
      [Bind(Prefix = "ID")] int BlackBoxID = 0,
      DateTime? StartDate = null,
      DateTime? EndDate = null
    )
        {

            var lastID = (
                from y in db.MSTR_BlackBox
                where y.BlackBoxID == BlackBoxID
                select y.LastRentalId
                  ).FirstOrDefault();

            BlackBoxTransaction btx = (
                from n in db.BlackBoxTransactions
                where n.ID == lastID
                select n
              ).FirstOrDefault();

            if (btx == null)
                return Json(new BlackBox(), JsonRequestBehavior.AllowGet);

            if (StartDate == null) StartDate = btx.RentStartDate == null ? System.DateTime.Now : btx.RentStartDate;
            if (EndDate == null) EndDate = btx.RentEndDate == null ? System.DateTime.Now : btx.RentEndDate;
            //get the information of calucation for the dates
            ViewData["RAmount"] = btx.Amount == null ? 0 : btx.Amount;
            var NumDays = (int)((TimeSpan)(StartDate - EndDate)).TotalDays;
            if (NumDays < 0) NumDays = -1 * NumDays;
            var BB = new BlackBox();
            var BBInfo = new
            {
                Cost = BB.getBlackBoxCost(NumDays),
                Info = btx
            };
            //      ViewData["BalanceAmount"] = totalAmt;
            return Json(BBInfo, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReceiveBlackBox([Bind(Prefix = "ID")] int BlackBoxTransID = 0)
        {
            if (!exLogic.User.hasAccess("BLACKBOX.RECEIVE")) return RedirectToAction("NoAccess", "Home");
            BlackBoxTransaction btx = new BlackBoxTransaction();
            if (BlackBoxTransID != 0)
            {
                btx = db.BlackBoxTransactions.Find(BlackBoxTransID);
                if (btx == null) return RedirectToAction("Error", "Home");

                //BlackBoxTransaction btxTran = (
                //                 from n in db.BlackBoxTransactions
                //                 where n.ID == BlackBoxTransID
                //                 select n
                //               ).FirstOrDefault();
                //btx.ID = btxTran.BlackBoxID;
            }
            btx.RentStartDate = btx.RentStartDate == null ? System.DateTime.Now : btx.RentStartDate;
            btx.RentEndDate = btx.RentEndDate == null ? System.DateTime.Now : btx.RentEndDate;
            btx.BBStatus = "IN";
            return View(btx);
        }

        [HttpPost]
        public ActionResult ReceiveBlackBox(BlackBoxTransaction Btx)
        {
            if (!exLogic.User.hasAccess("BLACKBOX.RECEIVE")) return RedirectToAction("NoAccess", "Home");            
            string StartDate = Request.Form["hdnRentStartDate"];
            string tAmount = Request.Form["hdnTotalAmount"];
            string DroneID = Request.Form["DroneID"];
            if ((Convert.ToDateTime(Btx.RentEndDate)) < (Convert.ToDateTime(StartDate)))
            {
                ModelState.AddModelError("RentEndDate", "Rent End Date should be greater than Start Date!!");
                Btx.BBStatus = "IN";
                return View(Btx);
            }
            string sDate = Convert.ToDateTime(StartDate).ToString("yyyy/MM/dd");
            string eDate = Convert.ToDateTime(Btx.RentEndDate).ToString("yyyy/MM/dd");

            string ye;
         
            ye = " RentStartDate='" + sDate + "',  RentEndDate='" + eDate + "', Amount='" + Btx.Amount + "',TotalAmount = " + (tAmount == "" ? 0 :Util.toDecimal(tAmount)) + "";
            //ye = " Amount='" + Btx.Amount + "',TotalAmount = " + (tAmount == "" ? 0 : Util.toDecimal(tAmount)) + "";

            string SQL = "update BlackBoxTransaction set DroneID = 0, BBStatus = 'IN', Note = '" + Btx.Note + "',CreatedBy='" + Util.getLoginUserID() + "'," + ye + " where ID = " + GetLastTransactionID(Btx.BlackBoxID);
            //need to appnd after testing
           
            int Val = Util.doSQL(SQL);

      SQL = "update MSTR_BlackBox set LastReceiveId = '" + GetLastTransactionID(Btx.BlackBoxID) + "',CurrentStatus='IN',CurrentUserID = '" + Util.getLoginUserID() + "',CurrentDroneID='0' where BlackBoxID = " + Btx.BlackBoxID;
      Val = Util.doSQL(SQL);

      SQL = "update dbo.MSTR_Drone set BlackBoxID = 0 where DroneId = " + Btx.DroneID;
      Val = Util.doSQL(SQL);

      
      return RedirectToAction("BlackBoxList", "Blackbox");
      //return View();
    }


    //Get:BlackBoxCost/Cost
    public ActionResult Cost() {
      //if (!exLogic.User.hasAccess("BLACKBOX.CREATE")) return RedirectToAction("NoAccess", "Home");

      BlackBoxCost BB = new BlackBoxCost();

      List<BlackBoxCost> Bl = db.BlackBoxCosts.ToList();
      return View(Bl);
    }

    [HttpPost]
    public ActionResult Cost(List<BlackBoxCost> BBCList) {
      //if (!exLogic.User.hasAccess("BLACKBOX.CREATE")) return RedirectToAction("NoAccess", "Home");

      foreach (BlackBoxCost BB in BBCList) {

        // BB.CreatedBy = Util.getLoginUserID();

        BB.LastUpdatedBy = Util.getLoginUserID();
        db.Entry(BB).State = EntityState.Modified;
        db.SaveChanges();
      }

      // IList<BlackBoxCost> Bl = db.BlackBoxCosts.ToList();
      return View(db.BlackBoxCosts.ToList());
    }

    //Get:BlackBoxCost/Rent
    public ActionResult Rent([Bind(Prefix = "ID")] int BlackBoxID = 0) {
            //if (!exLogic.User.hasAccess("BLACKBOX.CREATE")) return RedirectToAction("NoAccess", "Home");

            //BlackBoxViewModel BV = new BlackBoxViewModel();
            //BV.BlackBoxCostList = new List<List<BlackBoxCostCalucation>>();


            BlackBoxTransaction btx = new BlackBoxTransaction();
            if (BlackBoxID != 0)
            {
                btx = db.BlackBoxTransactions.Find(BlackBoxID);
                if (btx == null) return RedirectToAction("Error", "Home");
            }
            btx.BBStatus = "OUT";
            return View(btx);
           
    }


        [HttpPost]
        public ActionResult Rent(BlackBoxTransaction Btx)
        {
           Nullable<int> DroneID = Btx.DroneID;
            Nullable<int> BlackBoxID = Btx.BlackBoxID;
            ModelState.Remove("Amount");
            if (Btx.DroneID < 1 || Btx.DroneID == null)
            {
                ModelState.AddModelError("DroneID", "You must select a Drone.");
                Btx.BBStatus = "OUT";
                return View(Btx);
            }

            if ((Btx.RentStartDate==null)||(Btx.RentEndDate==null))
            {
                ModelState.AddModelError("RentStartDate", "Please select both dates!!");
                Btx.BBStatus = "OUT";
                return View(Btx);
            }
            
            string sDate = Convert.ToDateTime(Btx.RentStartDate).ToString("yyyy/MM/dd");
            string eDate = Convert.ToDateTime(Btx.RentEndDate).ToString("yyyy/MM/dd");
            if (Convert.ToDateTime(Btx.RentEndDate).Date < Convert.ToDateTime(Btx.RentStartDate).Date)
            {
                ModelState.AddModelError("RentEndDate", "Rent end date should be greater than rent start date!!");
                Btx.BBStatus = "OUT";
                return View(Btx);
            }
            string SQL = "insert into blackboxtransaction(DroneID,BBstatus,Note,createdby,Blackboxid,amount,rentamount,RentStartDate,RentEndDate) values("+Btx.DroneID +",'OUT','" + Btx.Note + "'," + Util.getLoginUserID() + "," +BlackBoxID + "," + Util.toInt(Btx.Amount) + "," + Util.toInt(Btx.RentAmount) + ",'"+ sDate + "','"+ eDate +"')";
            //  string SQL = "update BlackBoxTransaction set DroneID = '0', BBStatus = '" + Btx.BBStatus + "', Note = '" + Btx.Note + "',CreatedBy='" + Util.getLoginUserID() + "' where ID = " + Btx.ID;

            int bbtransctionid = Util.InsertSQL(SQL);
           

            SQL = "update MSTR_BlackBox  set [LastRentalId]=" + bbtransctionid + ", CurrentStatus='OUT',CurrentUserID = '" + Util.getLoginUserID() + "',CurrentDroneID="+Btx.DroneID +" where BlackBoxID = " + BlackBoxID;
          int  Val = Util.doSQL(SQL);

            SQL = "update dbo.MSTR_Drone set BlackBoxID ="+ BlackBoxID + " where DroneId = " + Btx.DroneID;
            Val = Util.doSQL(SQL);

            return RedirectToAction("BlackBoxList", "Blackbox");
        }


        //adding this for auto complete drone name
        public ActionResult DroneFilter(FlightReportFilter ReportFilter)
        {
            return View(ReportFilter);
        }

        public String getUAS(String Term = "")
        {
            var theReport = new exLogic.Report();
            return theReport.getUAS(Term);
        }

      
    public ActionResult Acknowledgement([Bind(Prefix = "ID")] int TransactionID = 0) {
      //   if (!exLogic.User.hasAccess("BLACKBOX.EDIT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Blackbox Rental";

      BlackBoxTransaction btx = new BlackBoxTransaction();
      if (TransactionID != 0) {
        btx = db.BlackBoxTransactions.Find(TransactionID);
        if (btx == null) return RedirectToAction("Error", "Home");
      }
      return View(btx);

    }

    [HttpPost]
    public ActionResult Acknowledgement(BlackBoxTransaction BBTransaction) {
      string sqlscript = "select count(*) from dbo.BlackBoxTransaction where ID = " + BBTransaction.ID + " and isnull(verifycode,'') = '" + BBTransaction.VerifyCode + "' ";

      int Count = Util.getDBRows(sqlscript).Count;
      if (Count > 0) {
        sqlscript = "update dbo.BlackBoxTransaction set RentAcknowledged = 1 where ID = " + BBTransaction.ID;
        Count = Util.doSQL(sqlscript);
        return RedirectToAction("Index", "Home");
      } else {
        BlackBoxTransaction btx = new BlackBoxTransaction();
        ViewBag.ErrorStatus = 1;
        return View(btx);
      }
    }
  }
}//namespace