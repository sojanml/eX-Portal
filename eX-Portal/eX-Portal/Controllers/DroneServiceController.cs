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
            ViewBag.Title = "Drone Listing";

            String SQL = "";
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
                DroneList = Util1.DroneList("usp_Portal_DroneNameList")


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

                MSTR_DroneService DroneService= DroneServiceView.DroneService;
               

                string SQL = "INSERT INTO MSTR_DRONESERVICE(Description,CreatedBy,CreatedOn,DroneId,TypeOfServiceId,TypeOfService,DateOfService) VALUES('"
                          + DroneService.Description + "'," + Session["UserId"]
                         + ",'" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") +
                         "','" + DroneService.DroneId + "'," + DroneService.ServiceId + ",'" + DroneService.TypeOfService + "','" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "'); ";
                int ID = Util.InsertSQL(SQL);

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
