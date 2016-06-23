using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers
{
    public class GpsController : Controller
    {

        ExponentPortalEntities db = new ExponentPortalEntities();
        // GET: Gps
        public ActionResult Index()
        {


            ViewBag.Title = "Drone Service Listing";

            String SQL = @"SELECT
       [DroneId]
      ,[DroneRFID]
      ,[Latitude]
      ,[Longitude]
      ,[ProductRFID]
      ,[ProductQrCode]
      ,[ProductRSSI]
      ,[ReadTime]
      ,[CreatedTime]
      ,[RecordType]
      ,[IsActive]
      ,[ProductId]
      ,[Altitude]
      ,[Speed]
      ,[FixQuality]
      ,[Satellites]
      ,[Pitch]
      ,[Roll]
      ,[Heading]
      ,[TotalFlightTime]
      ,[BBFlightID]
      ,[IsProcessed]      
      ,[CreatedDate]
      ,[Voltage],
       Count(*) Over() as _TotalRecords,
       [DroneGPSLogId] as _PKey
       FROM[DroneGPSLog]";




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

        // GET: Gps/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Gps/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Gps/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Gps/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Gps/Edit/5
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

        // GET: Gps/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Gps/Delete/5
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
