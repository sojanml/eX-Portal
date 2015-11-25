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
            ViewBag.Title = "Drone Listing";

            String SQL = "SELECT \n" +
                "D.[DroneId] ,D.[DroneName],D.[DroneIdHexa]  ,D.[CommissionDate] ,D.[DroneSerialNo], O.Name as OwnerName, M.Name as ManufactureName, U.Name as UAVType, Count(*) Over() as _TotalRecords \n" +
                "FROM [ExponentPortal].[dbo].[MSTR_Drone] D \n" +
                "inner join[ExponentPortal].[dbo].LUP_Drone  O on OwnerID = O.TypeID and O.Type = 'Owner' " +
                "inner join[ExponentPortal].[dbo].LUP_Drone M on ManufactureID = M.TypeID and M.Type='Manufacturer' " +
                "inner join [ExponentPortal].[dbo].LUP_Drone U on UAVTypeID = U.TypeID and U.Type= 'UAV Type' ";
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
                UAVTypeList = Util.GetDropDowntList("UAV Type", "Name", "Code", "usp_Portal_GetDroneDropDown"),
                ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
                //PartsGroupList = Util.GetDropDowntList();
            } ;

            return View(viewModel);
        }

       

        // POST: Drone/Create
        [HttpPost]
       
        public ActionResult Create(ViewModel.DroneView DroneView)
        {
            try
            {
                // TODO: Add insert logic here
                MSTR_Drone Drone = DroneView.Drone;

                int ID=Util.InsertSQL("INSERT INTO MSTR_DRONE(OWNERID,MANUFACTUREID,UAVTYPEID,COMMISSIONDATE) VALUES('"+Drone.OwnerId+"','"+Drone.ManufactureId+"','"+Drone.UavTypeId+"','"+Drone.CommissionDate.Value.ToString("yyyy-MM-dd")+"');");
                return RedirectToAction("Index");
            }
            catch(Exception ex)
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
