using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers
{
    public class DroneServiceController : Controller
    {
        // GET: DroneService
        public ActionResult Index()
        {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Drone Service Listing";

            String SQL = "select  a.ServiceId As ServiceId, " +
                " b.DroneName as DroneName,c.Name as ServiceType,a.DateOfService as DateOfService,a.FlightHour" +
                " , a.Description,Count(*) Over() as _TotalRecords ,  a.ServiceId as _PKey " +
                " from MSTR_DroneService a left join " +
                " MSTR_Drone b on a.DroneId = b.DroneId" +
                " left join [ExponentPortal].[dbo].LUP_Drone c on a.TypeOfServiceId " +
                "= c.TypeId where c.Type = 'ServiceType'";
            qView nView = new qView(SQL);

            if (exLogic.User.hasAccess("SERVICE.VIEW")) nView.addMenu("Detail", Url.Action("Details", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("SERVICE.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            if (exLogic.User.hasAccess("SERVICE.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
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

        // GET: DroneService/Details/5
        public ActionResult Details(int id)
        {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) return RedirectToAction("NoAccess", "Home");

            String SQL = "select DroneId from MSTR_DroneService where  ServiceId=" + id;

            ViewBag.FlightID = Util.getDBVal(SQL);
            ViewBag.ServiceId = id;
            ViewBag.Title = "View Checklist";
            ViewBag.Title = "Drone Service Details";

            SQL = "select b.PartsName,a.ServicePartsType as Info,b.Model as ModelType,a.QtyCount as Qty, Count(*) Over() as _TotalRecords from M2M_DroneServiceParts" +
                " a left join MSTR_Parts b on a.PartsId=b.PartsId where a.ServiceId=" + id +
                " group by a.ServicePartsType,b.PartsName,b.Model,a.QtyCount";


            qView nView = new qView(SQL);
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


        public int GetFlightHours(int ID = 0)
        {
            //finding the flight hours from the Flight map data table
            int Flightours = Util.GetTotalFlightTime(ID);

            return Flightours;
           

        }//
        public ActionResult ServicePartsReplaced(int ID = 0)
        {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) RedirectToAction("NoAccess", "Home");

            List<String> theData = new List<String>();
            theData = Listing.ServicePartsListing(ID, "REP");
            return PartialView(theData);

        }//



        public ActionResult ServicePartsRefurbished(int ID = 0)
         {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) RedirectToAction("NoAccess", "Home");
            List<String> theData = new List<String>();
            theData = Listing.ServicePartsListing(ID, "REF");
            return PartialView(theData);

        }//

        public String DroneServiceDetail(int ID=0)
             {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) return "Access Denied";
            string SQL = "select a.ServiceId as ServiceId ,a.DateOfService as " +
                "ServiceDate,b.DroneId,b.DroneIdHexa as DroneHex,c.UserName as  " +
                " ServicedBy, Count(*) Over() as _TotalRecords from MSTR_DroneService" +
                " a left join MSTR_Drone b on a.DroneId=b.DroneId  " +
                " left join MSTR_User c on a.CreatedBy=c.UserId where a.ServiceId="+ID;
            qDetailView theView = new qDetailView(SQL);
            theView.Columns = 3;

            return theView.getTable();

        }


    public String ByDrone([Bind(Prefix = "ID")] int DroneID = 0) {
      String SQL = "select TOP 5" +
          "  a.ServiceId As ServiceId," +
          "  b.DroneName as DroneName," +
          "  c.Name as ServiceType," +
          "  a.DateOfService as DateOfService," +
          "  a.FlightHour\n" +
          "from" +
          "  MSTR_DroneService a\n" +
          "left join MSTR_Drone b on\n" +
          "  a.DroneId = b.DroneId\n" +
          "left join LUP_Drone c on\n" +
          "  a.TypeOfServiceId = c.TypeId\n" +
          "where\n" +
          " c.Type = 'ServiceType' AND\n" +
          " b.DroneID = " + DroneID + "\n" +
          "ORDER BY\n" +
          "  a.DateOfService DESC";
      qView nView = new qView(SQL);
      return
        (nView.HasRows ? "<h2>Recent Services</h2>" + nView.getDataTable(true, false) : "");
    }
        public String DroneDetail(int ID = 0)
        {
            if (!exLogic.User.hasAccess("SERVICE.VIEW")) return  "Access Denied";
            String SQL =
            "SELECT \n" +
            "  D.[DroneId] , \n" +
            "  D.[DroneName], \n" +
            "  D.[DroneIdHexa] as DroneHex, \n" +
            "  D.[CommissionDate] , \n" +
            "  D.[DroneSerialNo],  \n" +
            "  O.Name as OwnerName,  \n" +
            "  M.Name as ManufactureName,  \n" +
            "  U.Name as UAVType,  \n" +
            "  Count(*) Over() as _TotalRecords, \n" +
            "  D.[DroneId] as _PKey \n" +
            "FROM \n" +
            "  [ExponentPortal].[dbo].[MSTR_Drone] D \n" +
            "inner join[ExponentPortal].[dbo].LUP_Drone  O on\n" +
            "  OwnerID = O.TypeID and O.Type = 'Owner' " +
            "inner join[ExponentPortal].[dbo].LUP_Drone M on\n" +
            "  ManufactureID = M.TypeID and M.Type='Manufacturer' " +
            "inner join [ExponentPortal].[dbo].LUP_Drone U on\n" +
            "  UAVTypeID = U.TypeID and U.Type= 'UAV Type' \n" +
            "WHERE\n" +
            "  D.DroneId =" + ID.ToString();

            qDetailView theView = new qDetailView(SQL);
            theView.Columns = 3;

            return theView.getTable();
        }//DroneFlightDeta

        // GET: DroneService/Create
        public ActionResult Create()
        {
            if (!exLogic.User.hasAccess("SERVICE.CREATE")) return RedirectToAction("NoAccess", "Home");
            var viewModel = new ViewModel.DroneServiceViewModel
            {

                DroneService = new MSTR_DroneService(),

                ServiceType = Util.GetDropDowntLists("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util.DroneList("usp_Portal_DroneNameList"),
             
                //   DronePartsList=Util1.DronePartsList("usp_Portal_GetDroneParts")
                
            };
            
            return View(viewModel);
        }



        // POST: DroneService/Create
        [HttpPost]
        public ActionResult Create(ViewModel.DroneServiceViewModel DroneServiceView)
        {
            try
            {
                if (!exLogic.User.hasAccess("SERVICE.CREATE")) return RedirectToAction("NoAccess", "Home");
                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {

                    MSTR_DroneService DroneService = DroneServiceView.DroneService;

                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }
                    string SQL = "INSERT INTO MSTR_DRONESERVICE(Description,CreatedBy,CreatedOn,DroneId,TypeOfServiceId,TypeOfService,DateOfService,FlightHour) VALUES('"
                              + DroneService.Description + "'," + Session["UserId"] + ",'" +
                               DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "','"
                              + DroneServiceView.DroneID + "'," + DroneService.TypeOfService + ",'" + DroneService.TypeOfService + "','" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "'," + DroneService.FlightHour + "); ";
                 // int ID = Util.InsertSQL(SQL);
                    int ServiceId= Util.InsertSQL(SQL);

                    //   int ServiceId = Util.GetServiceId();
                    if (DroneServiceView.SelectItemsForReplaced != null)
                    {
                        if (DroneServiceView.SelectItemsForReplaced != null)
                        {
                            for (var count = 0; count < DroneServiceView.SelectItemsForReplaced.Count(); count++)
                            {


                                string PartsId = ((string[])DroneServiceView.SelectItemsForReplaced)[count];
                                int Qty = Util.toInt(Request["SelectItemsForReplaced_" + PartsId]);
                                SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REP'," + Qty + ");";



                                int ID = Util.InsertSQL(SQL);
                            }
                        }
                    }


                    if (DroneServiceView.SelectItemsForRefurbished != null)
                    {
                        for (var count = 0; count < DroneServiceView.SelectItemsForRefurbished.Count(); count++)
                        { //int PartsId = Int32.Parse((DroneServiceView.SelectItems)[2])

                            string PartsId = ((string[])DroneServiceView.SelectItemsForRefurbished)[count];
                            int Qty = Util.toInt(Request["SelectItemsForRefurbished_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REF'," + Qty + " );";

                            int ID = Util.InsertSQL(SQL);
                        }


                    }

                }

                // return RedirectToAction("Index");
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DroneService/Edit/5
        public ActionResult Edit(int id)
        {
            if (!exLogic.User.hasAccess("SERVICE.EDIT")) return RedirectToAction("NoAccess", "Home");
            ExponentPortalEntities db = new ExponentPortalEntities();
            int DroneId = Util.GetDroneIdFromService(id);
            int TypeOfServiceId = Util.GetTypeOfIdFromService(id);
            ViewBag.ServiceId = id;
            var list = (from data in db.M2M_DroneServiceParts
                        where data.ServiceId == 39
                        select new
                        {
                            data.PartsId
                        });
            var viewModel = new ViewModel.DroneServiceViewModel
            {


                DroneService = db.MSTR_DroneService.Find(id),

                ServiceType = Util.GetDropDowntLists("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util.DroneList("usp_Portal_DroneNameList")
             // SelectItemsForRefurbished = list;
            };
            

           
            return View(viewModel);
       
    }
        // POST: DroneService/Edit/5
        [HttpPost]
        public ActionResult Edit(ViewModel.DroneServiceViewModel DroneServiceView)
        {
            try
            {
                // TODO: Add update logic here
                if (!exLogic.User.hasAccess("SERVICE.EDIT")) return RedirectToAction("NoAccess", "Home");
                if (ModelState.IsValid)
                {

                    MSTR_DroneService DroneService = DroneServiceView.DroneService;
                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }

                    string SQL = "UPDATE MSTR_DRONESERVICE SET Description='" + DroneService.Description + "',CreatedBy=" + Session["UserId"] + ", CreatedOn='" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") +
                        "', DroneId=" + DroneService.DroneId + ",TypeOfServiceId='" + DroneService.TypeOfService + "' ,DateOfService='" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "', FlightHour=" + DroneService.FlightHour + " WHERE ServiceId=" + DroneService.ServiceId;

                    int ID = Util.doSQL(SQL);
                    SQL = "delete from M2M_DroneServiceParts where ServiceId =" + DroneService.ServiceId;
                    ID = Util.doSQL(SQL);
                    if (DroneServiceView.SelectItemsForReplaced != null)
                    {
                       
                        for (var count = 0; count < DroneServiceView.SelectItemsForReplaced.Count(); count++)
                        {
                           
                            string PartsId = ((string[])DroneServiceView.SelectItemsForReplaced)[count];
                            int Qty = Util.toInt(Request["SelectItemsForReplaced_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + DroneService.ServiceId + "," + PartsId + ",'REP'," + Qty + ");";
                            ID = Util.InsertSQL(SQL);
                        }
                    }

                    if (DroneServiceView.SelectItemsForRefurbished != null)
                    {
                      
                        for (var count = 0; count < DroneServiceView.SelectItemsForRefurbished.Count(); count++)
                        { 
                            
                            string PartsId = ((string[])DroneServiceView.SelectItemsForRefurbished)[count];
                            int Qty = Util.toInt(Request["SelectItemsForRefurbished_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + DroneService.ServiceId + "," + PartsId + ",'REF'," + Qty + " );";

                            ID = Util.InsertSQL(SQL);
                        }


                    }


                }
                    return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
        Util.ErrorHandler(ex);
        return View();
            }
        }

        // GET: DroneService/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}



        //*drone  call//
        public ExponentPortalEntities ctx=new ExponentPortalEntities();
        public ActionResult FillParts(int DroneId)

        {
            var DroneParts = (from MSTR_Parts in ctx.MSTR_Parts
            select new
            {
                MSTR_Parts.PartsId,
                MSTR_Parts.Model,
                MSTR_Parts.PartsName,
            });



            return Json(DroneParts, JsonRequestBehavior.AllowGet);
        }

        private object Toint(int? supplierId)
        {
            throw new NotImplementedException();
        }


        // GET: DroneService/Delete/5

        public string Delete([Bind(Prefix = "ID")]int DroneServiceID = 0)
        {
            try
            {
                // TODO: Add delete logic here

                String SQL = "";
                Response.ContentType = "text/json";
                if (!exLogic.User.hasAccess("SERVICE.DELETE"))
                  
                    return Util.jsonStat("ERROR", "Access Denied");

                //Delete the drone from database if there is no flights are created
              

                SQL = "DELETE FROM [M2M_DroneServiceParts] WHERE ServiceId = " + DroneServiceID;
                Util.doSQL(SQL);

                SQL = "DELETE FROM [MSTR_DRONESERVICE] WHERE ServiceId = " + DroneServiceID;
                Util.doSQL(SQL);

                return Util.jsonStat("OK");

               
            }
            catch
            {
                return"error";
            }
        }
    }
}
