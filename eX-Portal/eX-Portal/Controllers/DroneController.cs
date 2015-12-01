using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class DroneController : Controller {

    // GET: Drone
    public ActionResult Index() {
      ViewBag.Title = "Drone Listing";

      String SQL = "SELECT \n" +
          "  D.[DroneId],\n" +
          "  D.[DroneName],\n" +
          "  D.[DroneIdHexa],\n" +
          "  D.[CommissionDate],\n" +
          "  D.[DroneSerialNo],\n" +
          "  O.Name as OwnerName,\n" +
          "  M.Name as ManufactureName,\n" +
          "  U.Name as UAVType,\n" +
          "  Count(*) Over() as _TotalRecords,\n" +
          "  D.[DroneId] as _PKey\n" +
          "FROM\n" +
          "  [ExponentPortal].[dbo].[MSTR_Drone] D\n" +
          "inner join LUP_Drone  O on\n" +
          "  OwnerID = O.TypeID and\n" +
          "  O.Type = 'Owner' " +
          "inner join LUP_Drone M on\n" +
          "  ManufactureID = M.TypeID and\n" +
          "  M.Type='Manufacturer' " +
          "inner join LUP_Drone U on\n" +
          "  UAVTypeID = U.TypeID and\n" +
          "  U.Type= 'UAV Type' ";
      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));
      nView.addMenu("Create Flight", Url.Action("Create", "DroneFlight", new { ID = "_Pkey" }));
      nView.addMenu("Flights", Url.Action("Index", "DroneFlight", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }

    // GET: Drone/Details/5
    public ActionResult Detail([Bind(Prefix = "ID")] int DroneID) {
      String SQL = "SELECT \n" +
           "  D.[DroneName] + ' - ' +  DroneIdHexa as DroneName\n" +
           "FROM\n" +
           "  [ExponentPortal].[dbo].[MSTR_Drone] D\n" +
           "WHERE\n" +
           "  D.[DroneId]=" + DroneID;
      ViewBag.Title = Util.getDBVal(SQL);
      ViewBag.DroneID = DroneID;
      return View();
    }

    // GET: Drone/Details/5
    public String DroneDetail([Bind(Prefix = "ID")]  int DroneID) {
      String SQL = "SELECT \n" +
          "  D.[DroneId],\n" +
          "  D.[DroneName],\n" +
          "  D.[DroneIdHexa],\n" +
          "  D.[CommissionDate],\n" +
          "  D.[DroneSerialNo],\n" +
          "  O.Name as OwnerName,\n" +
          "  M.Name as ManufactureName,\n" +
          "  U.Name as UAVType,\n" +
          "  Count(*) Over() as _TotalRecords,\n" +
          "  D.[DroneId] as _PKey\n" +
          "FROM\n" +
          "  [ExponentPortal].[dbo].[MSTR_Drone] D\n" +
          "inner join LUP_Drone  O on\n" +
          "  OwnerID = O.TypeID and\n" +
          "  O.Type = 'Owner' " +
          "inner join LUP_Drone M on\n" +
          "  ManufactureID = M.TypeID and\n" +
          "  M.Type='Manufacturer' " +
          "inner join LUP_Drone U on\n" +
          "  UAVTypeID = U.TypeID and\n" +
          "  U.Type= 'UAV Type'\n" +
          "WHERE\n" +
          "  D.[DroneId]=" + DroneID;
      qDetailView nView = new qDetailView(SQL);
      return nView.getTable();
    }

    // GET: Drone/Create
    public ActionResult Create() {

      var viewModel = new ViewModel.DroneView {
        Drone = new MSTR_Drone(),
        OwnerList = Util.GetDropDowntList("Owner", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        UAVTypeList = Util.GetDropDowntList("UAV Type", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };

      return View(viewModel);
    }



    // POST: Drone/Create
    [HttpPost]

    public ActionResult Create(ViewModel.DroneView DroneView) {
      try {
        // TODO: Add insert logic here

        MSTR_Drone Drone = DroneView.Drone;

        int ID = Util.InsertSQL("INSERT INTO MSTR_DRONE(OWNERID,MANUFACTUREID,UAVTYPEID,COMMISSIONDATE,DRONEDEFINITIONID,ISACTIVE) VALUES('" + Drone.OwnerId + "','" + Drone.ManufactureId + "','" + Drone.UavTypeId + "','" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',11,'True');");
        return RedirectToAction("Index");
      } catch (Exception ex) {
        return View(DroneView);
      }
    }


    // GET: Drone/Edit/5
    public ActionResult Edit(int id) {
      return View();
    }

    // POST: Drone/Edit/5
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection) {
      try {
        // TODO: Add update logic here

        return RedirectToAction("Index");
      } catch {
        return View();
      }
    }

    // GET: Drone/Delete/5
    public ActionResult Delete(int id) {
      return View();
    }

    // POST: Drone/Delete/5
    [HttpPost]
    public ActionResult Delete(int id, FormCollection collection) {
      try {
        // TODO: Add delete logic here

        return RedirectToAction("Index");
      } catch {
        return View();
      }
    }





  }
}
