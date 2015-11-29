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

            String SQL = " select  ROW_NUMBER() Over (Order by a.ServiceId) As SNo,  b.DroneName as DroneName,c.Name as ServiceType,a.DateOfService as DateOfService,a.FlightHour , a.Description,Count(*) Over() as _TotalRecords  " +
                " from [ExponentPortal].[dbo].MSTR_DroneService a left join  [ExponentPortal].[dbo].MSTR_Drone b on a.DroneId = b.DroneId left join [ExponentPortal].[dbo].LUP_Drone c on a.TypeOfServiceId " +
                "= c.TypeId where c.Type = 'ServiceType'"; 
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

        // GET: DroneService/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DroneService/Create
        public ActionResult Create()
        {
            var viewModel = new ViewModel.DroneServiceViewModel
            {
                DroneService = new MSTR_DroneService(),

                ServiceType = Util1.GetDropDowntList("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util1.DroneList("usp_Portal_DroneNameList"),
                DronePartsList=Util1.DronePartsList("usp_Portal_GetDroneParts")

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
                             "','" + DroneService.DroneId + "'," + DroneService.TypeOfService + ",'" + DroneService.TypeOfService + "','" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "'," + DroneService.FlightHour + "); ";
                    int ID = Util.InsertSQL(SQL);

                    int ServiceId = Util1.GetServiceId();
                    for (var count=0;count< DroneServiceView.SelectItems.Count();count++)
                    {
                        //int PartsId = Int32.Parse((DroneServiceView.SelectItems)[2])

                       string PartsId=    ((string[])DroneServiceView.SelectItems)[count];
                       SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId) values(" + ServiceId +"," + PartsId + ");";

                        ID = Util.InsertSQL(SQL);
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
