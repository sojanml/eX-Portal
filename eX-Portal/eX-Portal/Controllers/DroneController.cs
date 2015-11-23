using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers
{
    public class DroneController : Controller
    {
      
        // GET: Drone
        public ActionResult Index()
        {

            return View();
        }

        // GET: Drone/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Drone/Create
        public ActionResult Create()
        {

            var viewModel = new ViewModel.DroneView
            {
                Drone = new MSTR_Drone(),
                OwnerList = Util.GetDropDowntList("Owner", "Name", "Code", "usp_Portal_GetDroneDropDown"),
                UAVTypeList = Util.GetDropDowntList("Owner", "Name", "Code", "usp_Portal_GetDroneDropDown"),
                ManufactureList = Util.GetDropDowntList("Owner", "Name", "Code", "usp_Portal_GetDroneDropDown")
                //PartsGroupList = Util.GetDropDowntList();
            } ;

            return View(viewModel);
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

        // POST: Drone/Create
        [HttpPost]
        public ActionResult Create(ViewModel.DroneView DroneView)
        {
            try
            {
                // TODO: Add insert logic here
                var Drone = new MSTR_Drone
                {
                    DroneName = DroneView.Drone.DroneName,
                    OwnerId=DroneView.Drone.OwnerId

                };
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
