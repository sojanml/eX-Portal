﻿using eX_Portal.exLogic;
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
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) {
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
        "SELECT\n" +
        "  [DroneDataId] as UASDataId ," +
        "  MSTR_Drone.DroneName as UAS,\n" +
        "  [ReadTime] as [Date],\n" +
        "  CASE ISNUMERIC([DroneData].[Latitude])\n" +
        "		 WHEN  1 THEN CONVERT(numeric(12, 3),[DroneData].[Latitude])\n" +
        "		 ELSE 0.00\n" +
        "   END as [Latitude] ,\n" +
        "  CASE ISNUMERIC([DroneData].[Longitude])\n" +
        "    WHEN  1 THEN  CONVERT(numeric(12, 3),[DroneData].[Longitude])\n" +
        "    ELSE 0.00\n" +
        "  END as [Longitude],\n" +
        "  [Altitude] as [Altitude],\n" +
        "  [Speed] as [Speed],\n" +
        "  [FixQuality],\n" +
        "  [Satellites],\n" +
        "  CASE ISNUMERIC([BBFlightID])\n" +
        "    WHEN  1 THEN  CONVERT(numeric(20, 0),[BBFlightID])\n" +
        "    ELSE 0.00\n" +
        "  END as [FDRFlightId],\n" + 
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
      if (!exLogic.User.hasAccess("DRONE.MANAGE")) {
        SQL += " AND\n" +
          "  MSTR_Drone.AccountID=" + Util.getAccountID() + "\n" +
          "WHERE\n" +
          "  MSTR_Drone.DroneID IS NOT NULL";
      }

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
       "MOSELECT \n" +
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
        public ActionResult Create()
        {
            //if (!exLogic.User.hasAccess("BLACKBOX.CREATE")) return RedirectToAction("NoAccess", "Home");

            MSTR_BlackBox BB = new MSTR_BlackBox();
             return View(BB);
        }

        // POST: BlackBox/Create
        [HttpPost]
        public ActionResult Create(Models.MSTR_BlackBox BlackBox)
        {
         //   if (!exLogic.User.hasAccess("BLACLBOX.CREATE")) return RedirectToAction("NoAccess", "Home");
            try
            {
                // TODO: Add insert logic here
               // BlackBox.BlackBoxID = 1;

                if (ModelState.IsValid)
                {
                  //  BlackBox.BlackBoxID = 0;
                    BlackBox.IsActive = 1;
                    BlackBox.CreatedBy = Util.getLoginUserID();
                    BlackBox.CreatedOn = DateTime.Now;
                    BlackBox.LastUpdateDate = DateTime.Now;
                    db.MSTR_BlackBox.Add(BlackBox);

                    db.SaveChanges();

                    db.Dispose();

                    return RedirectToAction("BlackBoxList", "BlackBox");
                }
                else
                {
                    ViewBag.Title = "Create BlackBox";
                    return View(BlackBox);
                }
                
            }
            catch (Exception ex)
            {
                Util.ErrorHandler(ex);
                return View("InternalError", ex);
            }
        }

        public ActionResult BlackBoxList()
     {

      //      if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Blackbox";

            string SQL = @"SELECT  m.[BlackBoxID] as _PKey
                          ,m.[BlackBoxSerial]
                          ,m.[BlackBoxName]
                          ,m.[CurrentStatus]
                          ,u.Firstname +' '+u.lastname as CreatedBy
                          ,d.RpasSerialNo
                            , Count(*) Over() as _TotalRecords
                      FROM  MSTR_BlackBox m left join mstr_user u
                      on m.CurrentUserID = u.userid
                      left join mstr_drone d
                      on d.droneid = m.currentDroneID where m.[IsActive] = 1";
            qView nView = new qView(SQL);
            nView.addMenu("Detail", Url.Action("BlackBoxDetails", new { ID = "_PKey" }));
            nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            nView.addMenu("Receive", Url.Action("ReceiveBlackBox", new { ID = "_PKey" }));

            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }//PartsBlackBoxlist

        // GET: Parts/Edit/5
        public ActionResult Edit(int id)
        {
         //   if (!exLogic.User.hasAccess("BLACKBOX.EDIT")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Edit Blackbox";
        //    ExponentPortalEntities db = new ExponentPortalEntities();
            MSTR_BlackBox BB = db.MSTR_BlackBox.Find(id);

            return View(BB);

        }

        // POST: BlackBox/Edit/5
        [HttpPost]
        public ActionResult Edit(MSTR_BlackBox  BB)
        {
            try
            {
                // TODO: Add update logic here

           //     if (!exLogic.User.hasAccess("BlackBox.EDIT")) return RedirectToAction("NoAccess", "Home");
                
                if (ModelState.IsValid)
                {
                    ViewBag.Title = "Edit BlackBox";
                    BB.CreatedBy = Util.getLoginUserID();
                    BB.LastUpdateDate = DateTime.Now;

                    db.Entry(BB).State = EntityState.Modified;
                    
                    db.SaveChanges();
                    return RedirectToAction("BlackBoxList", "BlackBox");
                }
                else
                {
                    return View(BB);
                }

            }
            catch
            {
                return View(BB);
            }
        }

        // GET: BlackBox/Details/5
        public String BBDetails(int id)
        {
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
                            , Count(*) Over() as _TotalRecords
                      FROM  MSTR_BlackBox
                        where BlackBoxID="+id;

            qDetailView nView = new qDetailView(SQL);
            ViewBag.Message = nView.getTable();
            ViewBag.Title = "Black Box Details";
            return nView.getTable();
        }



        public ActionResult BlackBoxDetails([Bind(Prefix = "ID")] int BlackBoxID)
        {
           // if (!exLogic.User.hasAccess("PARTS.VIEW")) return RedirectToAction("NoAccess", "Home");

            Models.MSTR_BlackBox BB = db.MSTR_BlackBox.Find(BlackBoxID);
            if (BB == null) return RedirectToAction("Error", "Home");
            ViewBag.Title = BB.BlackBoxID;
            return View(BB);
        }

        // GET: BlackBox/Issue/5
        public ActionResult Rental([Bind(Prefix = "ID")] int approvalid =0)

        {
            //   if (!exLogic.User.hasAccess("BLACKBOX.EDIT")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Blackbox Rental";
            var dronedet = (from d in db.GCA_Approval
                           where d.ApprovalID == approvalid
                           select d).ToList();
            if (dronedet.Count > 0)
            {
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
            ModelState.Remove("Note");
            if (ModelState.IsValid)
            {
                BBTransaction.BBStatus = "OUT";
                BBTransaction.CreatedBy = Session["UserID"].ToString();
                BBTransaction.DroneID = Convert.ToInt32(TempData["Droneid"]);
                BBTransaction.ApprovalID = Convert.ToInt32(TempData["Approvalid"]);
                //db.BlackBoxTransactions.Add(BBTransaction);
                //db.SaveChanges();
                //int id = BBTransaction.ID;

                string insertsql = "insert into[BlackBoxTransaction] ([BlackBoxID] \n" +
                                 ",[BBStatus] \n" +
                                 ",[CollectionMode] \n" +
                                 ",[NameOnCard] \n" +
                                 ",[BankName] \n" +
                                 ",[Amount] \n" +
                                 ",[ChequeNumber] \n" +
                                 ",[DateOfCheque] \n" +
                                 ",[CreatedDate] \n" +
                                 ",[CreatedBy] \n" +
                                 ",[DroneID] \n" +
                                 ",[ApprovalID])values( \n" +
                                 BBTransaction.BlackBoxID + ",'OUT'," + BBTransaction.CollectionMode + ",'" + BBTransaction.NameOnCard + "','" + BBTransaction.BankName + "',\n" +
                                 BBTransaction.Amount + ",'" + BBTransaction.ChequeNumber + "','" + BBTransaction.DateOfCheque + "','" + System.DateTime.Now + "','\n" +
                                 BBTransaction.CreatedBy + "'," + BBTransaction.DroneID + "," + BBTransaction.ApprovalID + ")";
                int bbtransctionid = Util.InsertSQL(insertsql);
                string bbupdatesql = "update [MSTR_BlackBox] set [LastRentalId]=" + bbtransctionid + ",[CurrentStatus]='OUT',CurrentUserID=" + Convert.ToInt32(Session["UserID"].ToString()) + ",CurrentDroneID=" + Convert.ToInt32(TempData["Droneid"]) + " where [BlackBoxID]=" + BBTransaction.BlackBoxID;
                Util.doSQL(bbupdatesql);
                string droneupdatesql = "update [MSTR_Drone] set [BlackBoxID]=" + BBTransaction.BlackBoxID + " where [DroneId]=" + BBTransaction.DroneID;
                Util.doSQL(droneupdatesql);
                return RedirectToAction("AllApplications", "Rpas");
            }
            return View();
        }

        public ActionResult ReceiveBlackBox([Bind(Prefix = "ID")] int BlackBoxID = 0)
        {
            BlackBoxTransaction btx = new BlackBoxTransaction();
            if (BlackBoxID != 0)
            {
                btx = db.BlackBoxTransactions.Find(BlackBoxID);
                if (btx == null) return RedirectToAction("Error", "Home");
            }
            btx.BBStatus = "IN";
            return View(btx);
        }

        [HttpPost]
        public ActionResult ReceiveBlackBox(BlackBoxTransaction Btx)
        {
            //var oList = from p in db.BlackBoxTransactions select p;
            //if (!exLogic.User.hasAccess("RPAS.APPLICATION")) return RedirectToAction("NoAccess", "Home");

            string SQL = "update BlackBoxTransaction set DroneID = '0', BBStatus = '" + Btx.BBStatus + "', Note = '" + Btx.Note + "',CreatedBy='" + Util.getLoginUserID() + "' where ID = " + Btx.ID;
            int Val = Util.doSQL(SQL);

            SQL = "update MSTR_BlackBox set LastReceiveId = '" + Btx.ID + "',CurrentStatus='IN',CurrentUserID = '" + Util.getLoginUserID() + "',CurrentDroneID='0' where BlackBoxID = " + Btx.BlackBoxID;
            Val = Util.doSQL(SQL);

            SQL = "update dbo.MSTR_Drone set BlackBoxID = 0 where DroneId = " + Btx.DroneID;
            Val = Util.doSQL(SQL);

            return RedirectToAction("BlackBoxList", "Blackbox");
            //return View();
        }

    }//class
}//namespace