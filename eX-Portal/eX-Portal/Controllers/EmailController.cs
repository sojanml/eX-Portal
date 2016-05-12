﻿using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class EmailController : Controller {
    private ExponentPortalEntities ctx = new ExponentPortalEntities();
    // GET: Email
    public ActionResult Index() {
      return View();
    }
    public ActionResult ForgotPassword(
      [Bind(Prefix = "ID")] int UserID = 0,
      String NewPassword = "null"
    ) {
      var User = ctx.MSTR_User.Find(UserID);

      ViewBag.Title = "Recover Password";
      ViewBag.NewPassword = NewPassword;
      return View(User);
    }//ActionResult ForgotPassword


    public ActionResult FlightReport([Bind(Prefix = "ID")] int FlightID = 0) {


      var FlightData = (
        from n in ctx.DroneFlights
        where n.ID == FlightID
        select new FlightViewModel {
          ID = n.ID,
          PilotID = n.PilotID,
          GSCID = n.GSCID,
          FlightDate = n.FlightDate,
          FlightHours = n.FlightHours,
          FlightDistance = n.FlightDistance,
          DroneID = n.DroneID,
          CreatedOn = n.CreatedOn
        }).ToList().FirstOrDefault();

      if (FlightData != null) {

        FlightData.PilotName = (
          from n in ctx.MSTR_User
          where n.UserId == FlightData.PilotID
          select n.FirstName).FirstOrDefault();

        FlightData.GSCName = (
          from n in ctx.MSTR_User
          where n.UserId == FlightData.GSCID
          select n.FirstName).FirstOrDefault();

        FlightData.DroneName = (
          from n in ctx.MSTR_Drone
          where n.DroneId == FlightData.DroneID
          select n.DroneName).FirstOrDefault();

        FlightData.Info = (
          from n in ctx.FlightInfoes
          where n.FlightID == FlightID
          select n).FirstOrDefault();

      }//if (FlightData != null)
      ViewBag.Title = "Post Flight Report";
      return View(FlightData);
    }//ActionResult FlightReport

  }//public class EmailController
}//namespace eX_Portal.Controllers