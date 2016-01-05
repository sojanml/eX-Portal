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
        public ActionResult Create([Bind(Prefix = "ID")] int PilotID = 0)
        {
            ViewBag.Title = "Create Pilot Log";
            MSTR_Pilot_Log PilotLog = new MSTR_Pilot_Log();
          PilotLog.PilotId = PilotID;
            return View(PilotLog);
         
        }

        // POST: PilotLog/Create
        [HttpPost]
        public ActionResult Create(MSTR_Pilot_Log PilotLog)
        {
            if (PilotLog.DroneId < 1 || PilotLog.DroneId == null) ModelState.AddModelError("DroneID", "You must select a Drone.");
          

            if (ModelState.IsValid)
            {
                int ID = 0;
                ExponentPortalEntities db = new ExponentPortalEntities();
                db.MSTR_Pilot_Log.Add(PilotLog);
                db.SaveChanges();
                ID = PilotLog.Id;
                db.Dispose();
                return RedirectToAction("Detail", new { ID = ID });
            }
            else
            {
                ViewBag.Title = "Create Drone Flight";
                return View(PilotLog);
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
