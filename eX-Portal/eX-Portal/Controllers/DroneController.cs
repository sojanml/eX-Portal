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

      String SQL =
      "SELECT \n" +
      "  D.[DroneId] , \n" +
      "  D.[DroneName], \n" +
      "  D.[DroneIdHexa] as DroneHex , \n" +
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
      "  UAVTypeID = U.TypeID and U.Type= 'UAV Type' ";
      qView nView = new qView(SQL);
      nView.addMenu("Details", Url.Action("Details", new { ID = "_PKey" }));
      nView.addMenu("Edit", Url.Action("Details", new { ID = "_PKey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }

    // GET: Drone/Details/5
    public ActionResult Details([Bind(Prefix = "ID")] int DroneID) {
      ViewBag.Title = Util.getDBVal("SELECT DroneName FROM MSTR_Drone WHERE DroneID=" + DroneID);
      ViewBag.DroneID = DroneID;
      return View();
    }


    public String DroneDetail(int ID = 0) {
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
    }//DroneFlightDetail ()

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

        int ID = Util.InsertSQL("INSERT INTO MSTR_DRONE(OWNERID,MANUFACTUREID,UAVTYPEID,COMMISSIONDATE) VALUES('" + Drone.OwnerId + "','" + Drone.ManufactureId + "','" + Drone.UavTypeId + "','" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "');");
        return RedirectToAction("Index");
      } catch (Exception ex) {
        return View();
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
