using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.Controllers
{
    public class DroneController : Controller
    {
     //   private Models.MSTR_Drone db=new M
        // GET: Drone
        public ActionResult Index()
        {
            var Drones = this.db.Albums.Include(a => a.Genre).Include(a => a.Artist)
            .OrderBy(a => a.Price);

            return this.View(albums.ToList());
           
        }

        // GET: Drone/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Drone/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Drone/Create
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

        // GET: Drone/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Drone/Edit/5
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

        // GET: Drone/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Drone/Delete/5
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
