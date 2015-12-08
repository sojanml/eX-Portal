using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class DroneController : Controller {
    public ExponentPortalEntities ctx = new ExponentPortalEntities();
    // GET: Drone
    public ActionResult Index() {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");

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
      if (exLogic.User.hasAccess("DRONE.EDIT")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("FLIGHT.CREATE")) nView.addMenu("Create Flight", Url.Action("Create", "DroneFlight", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("FLIGHT")) nView.addMenu("Flights", Url.Action("Index", "DroneFlight", new { ID = "_Pkey" }));
      if (exLogic.User.hasAccess("DRONE.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)

    }

    // GET: Drone/Details/5
    public ActionResult Detail([Bind(Prefix = "ID")] int DroneID) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
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


    public ActionResult DroneParts(int ID = 0) {
      if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
      List<String> Parts = new List<String>();
      Parts = Listing.DroneListing(ID);
      return PartialView(Parts);
    }
    // GET: Drone/Details/5
    public String DroneDetail([Bind(Prefix = "ID")]  int DroneID) {
      if (!exLogic.User.hasAccess("DRONE")) return "Access Denied";
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
      if (!exLogic.User.hasAccess("DRONE.CREATE")) return RedirectToAction("NoAccess", "Home");
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
      if (!exLogic.User.hasAccess("DRONE.CREATE")) return RedirectToAction("NoAccess", "Home");
      try {
        // TODO: Add insert logic here

        MSTR_Drone Drone = DroneView.Drone;

        int DroneId = Util.InsertSQL("INSERT INTO MSTR_DRONE(OWNERID,MANUFACTUREID,UAVTYPEID,COMMISSIONDATE,DRONEDEFINITIONID,ISACTIVE) VALUES('" + Drone.OwnerId + "','" + Drone.ManufactureId + "','" + Drone.UavTypeId + "','" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") + "',11,'True');");

        if (DroneView.SelectItemsForParts != null) {
          for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
            string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
            int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
            string SQL = "Insert into M2M_DroneParts (DroneId,PartsId,Quantity) values(" + DroneId + "," + PartsId + "," + Qty + ");";
            int ID = Util.doSQL(SQL);
          }

        }

        return RedirectToAction("Index");
      } catch (Exception ex) {
        Util.ErrorHandler(ex);
        return View(DroneView);
      }
    }


    // GET: Drone/Edit/5
    public ActionResult Edit(int id) {
      if (!exLogic.User.hasAccess("DRONE.EDIT")) return RedirectToAction("NoAccess", "Home");
      ViewBag.DroneId = id;
      ExponentPortalEntities db = new ExponentPortalEntities();
      var viewModel = new ViewModel.DroneView {
        Drone = db.MSTR_Drone.Find(id),
        OwnerList = Util.GetDropDowntList("Owner", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        UAVTypeList = Util.GetDropDowntList("UAV Type", "Name", "Code", "usp_Portal_GetDroneDropDown"),
        ManufactureList = Util.GetDropDowntList("Manufacturer", "Name", "Code", "usp_Portal_GetDroneDropDown")
        //PartsGroupList = Util.GetDropDowntList();
      };
      return View(viewModel);
    }

    // POST: Drone/Edit/5
    [HttpPost]
    public ActionResult Edit(ViewModel.DroneView DroneView) {
      if (!exLogic.User.hasAccess("DRONE.EDIT")) return RedirectToAction("NoAccess", "Home");
      try {
        // TODO: Add update logic here
        if (ModelState.IsValid) {
          MSTR_Drone Drone = DroneView.Drone;
          //master updating

          string SQL = "UPDATE MSTR_DRONE SET OWNERID ='" + Drone.OwnerId + "'," +
                       "MANUFACTUREID ='" + Drone.ManufactureId + "',UAVTYPEID ='" + Drone.UavTypeId +
                       "',COMMISSIONDATE ='" + Drone.CommissionDate.Value.ToString("yyyy-MM-dd") +
                        "',DRONEDEFINITIONID = 11,ISACTIVE ='True' WHERE DroneId =" + Drone.DroneId;
          int DroneId = Util.doSQL(SQL);

          //Parts updating

          SQL = "delete from M2M_DroneParts where DroneId=" + Drone.DroneId;
          int Id = Util.doSQL(SQL);
          if (DroneView.SelectItemsForParts != null) {
            for (var count = 0; count < DroneView.SelectItemsForParts.Count(); count++) {
              string PartsId = ((string[])DroneView.SelectItemsForParts)[count];
              int Qty = Util.toInt(Request["SelectItemsForParts_" + PartsId]);
              SQL = "Insert into M2M_DroneParts (DroneId,PartsId,Quantity) values(" + Drone.DroneId + "," + PartsId + "," + Qty + ");";
              int ID = Util.doSQL(SQL);
            }

          }

        }
        return RedirectToAction("Index");
      } catch {
        return View();
      }
    }

    // GET: Drone/Delete/5
    public String Delete([Bind(Prefix = "ID")]int DroneID = 0) {
      String SQL = "";
      Response.ContentType = "text/json";
      if (!exLogic.User.hasAccess("DRONE.DELETE"))
        return Util.jsonStat("ERROR", "Access Denied");

      //Delete the drone from database if there is no flights are created
      SQL = "SELECT Count(*) FROM [DroneFlight] WHERE DroneID = " + DroneID;
      if(Util.getDBInt(SQL) != 0)
        return Util.jsonStat("ERROR", "You can not delete a drone with a flight attached");

      SQL = "DELETE FROM [M2M_DroneParts] WHERE DroneID = " + DroneID;
      Util.doSQL(SQL);

      SQL = "DELETE FROM [DroneFlight] WHERE DroneID = " + DroneID;
      Util.doSQL(SQL);

      return Util.jsonStat("OK");
    }

    
    public ActionResult FillParts(int DroneId) {
      var DroneParts = (from MSTR_Parts in ctx.MSTR_Parts
                        select new {
                          MSTR_Parts.PartsId,
                          MSTR_Parts.Model,
                          MSTR_Parts.PartsName,
                        });

      return Json(DroneParts, JsonRequestBehavior.AllowGet);
    }

    private object Toint(int? supplierId) {
      throw new NotImplementedException();
    }

  }
}
