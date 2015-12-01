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
            ViewBag.Title = "Drone Service Listing";

            String SQL = " select  ROW_NUMBER() Over (Order by a.ServiceId) As SNo, " +
                " b.DroneName as DroneName,c.Name as ServiceType,a.DateOfService as DateOfService,a.FlightHour"+
                " , a.Description,Count(*) Over() as _TotalRecords ,  a.ServiceId as _PKey "  +
                " from [ExponentPortal].[dbo].MSTR_DroneService a left join" +
                "[ExponentPortal].[dbo].MSTR_Drone b on a.DroneId = b.DroneId" +
                " left join [ExponentPortal].[dbo].LUP_Drone c on a.TypeOfServiceId " +
                "= c.TypeId where c.Type = 'ServiceType'"; 
            qView nView = new qView(SQL);
          
            nView.addMenu("Detail", Url.Action("Details", new { ID = "_PKey" }));
            nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
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

           
            String  SQL = "select DroneId from MSTR_DroneService where  ServiceId=" + id;

            ViewBag.FlightID = Util.getDBVal(SQL);
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



        public String DroneDetail(int ID = 0)
        {
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
            var viewModel = new ViewModel.DroneServiceViewModel
            {
                DroneService = new MSTR_DroneService(),

                ServiceType = Util1.GetDropDowntList("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util1.DroneList("usp_Portal_DroneNameList"),
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
                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {

                    MSTR_DroneService DroneService = DroneServiceView.DroneService;

                    
                    string SQL = "INSERT INTO MSTR_DRONESERVICE(Description,CreatedBy,CreatedOn,DroneId,TypeOfServiceId,TypeOfService,DateOfService,FlightHour) VALUES('"
                              + DroneService.Description + "'," + Session["UserId"]
                             + ",'" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") +
                             "','" + DroneServiceView.DroneID + "'," + DroneService.TypeOfService + ",'" + DroneService.TypeOfService + "','" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "'," + DroneService.FlightHour + "); ";
                    int ID = Util.InsertSQL(SQL);

                    int ServiceId = Util1.GetServiceId();
                    if (DroneServiceView.SelectItemsForReplaced != null)
                    {
                        if (DroneServiceView.SelectItemsForReplaced != null)
                        {
                            for (var count = 0; count < DroneServiceView.SelectItemsForReplaced.Count(); count++)
                            {


                                string PartsId = ((string[])DroneServiceView.SelectItemsForReplaced)[count];
                                int Qty = Util1.toInt(Request["SelectItemsForReplaced_" + PartsId]);
                                SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REP',"+ Qty + ");";

                                

                                ID = Util.InsertSQL(SQL);
                            }
                        }
                    }
                   

                    if (DroneServiceView.SelectItemsForRefurbished != null)
                    { 
                        for (var count = 0; count < DroneServiceView.SelectItemsForRefurbished.Count(); count++)
                        { //int PartsId = Int32.Parse((DroneServiceView.SelectItems)[2])

                            string PartsId = ((string[])DroneServiceView.SelectItemsForRefurbished)[count];
                            int Qty = Util1.toInt(Request["SelectItemsForRefurbished_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REF'," + Qty + " );";
                             
                            ID = Util.InsertSQL(SQL);
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
            return View();
        }

        // POST: DroneService/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DroneService/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }



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


        // POST: DroneService/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
