using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
namespace eX_Portal.Controllers
{
    public class PilotLogController : Controller
    {
        // GET: PilotLog
        public ActionResult Index()
        {
            return View();
        }

        // GET: PilotLog/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PilotLog/Create
        public ActionResult Create([Bind(Prefix = "ID")] int DroneID = 0)
        {
            ViewBag.Title = "Create Pilot Log";
            // InitialData = new DroneFlight();
          //  InitialData.DroneID = DroneID;
           // return View(InitialData);
            return View();
        }

        // POST: PilotLog/Create
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

        // GET: PilotLog/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PilotLog/Edit/5
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

        // GET: PilotLog/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PilotLog/Delete/5
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
