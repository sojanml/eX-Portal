﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using System.Data.Entity;
using eX_Portal.exLogic;
using System.Text;

namespace eX_Portal.Controllers {
  public class PilotLogController : Controller {
    // GET: PilotLog
    public ActionResult Index() {
      return View();
    }

    // GET: PilotLog/Details/5
    public ActionResult Details(int id) {
      return View();
    }

    // GET: PilotLog/Create
    public ActionResult Create([Bind(Prefix = "ID")] int PilotID = 0) {
      if (!exLogic.User.hasAccess("PILOTLOG.CREATE"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Create Pilot Log";
      MSTR_Pilot_Log PilotLog = new MSTR_Pilot_Log();
      PilotLog.PilotId = PilotID;
      PilotLog.Date = DateTime.Now;

      return View(PilotLog);

    }

    // POST: PilotLog/Create
    [HttpPost]
    public ActionResult Create(MSTR_Pilot_Log PilotLog) {
      if (!exLogic.User.hasAccess("PILOTLOG.CREATE"))
        return RedirectToAction("NoAccess", "Home");
      if (PilotLog.DroneId < 1 || PilotLog.DroneId == null)
        ModelState.AddModelError("DroneID", "You must select a UAS.");


      if (ModelState.IsValid) {
        ExponentPortalEntities db = new ExponentPortalEntities();
        db.MSTR_Pilot_Log.Add(PilotLog);
        db.SaveChanges();

        db.Dispose();
        return RedirectToAction("UserDetail", "User", new { ID = PilotLog.PilotId });
      } else {
        ViewBag.Title = "Create Drone Flight";
        return View(PilotLog);
      }

    }

    // GET: PilotLog/Edit/5
    public ActionResult Edit([Bind(Prefix = "ID")] int LogId = 0) {
      if (!exLogic.User.hasAccess("PILOTLOG.EDIT"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.Title = "Edit Pilot Log";
      ExponentPortalEntities db = new ExponentPortalEntities();
      MSTR_Pilot_Log PilotLog = db.MSTR_Pilot_Log.Find(LogId);
      return View(PilotLog);

    }

    // POST: PilotLog/Edit/5
    [HttpPost]
    public ActionResult Edit(MSTR_Pilot_Log PilotLog) {
      try {
        if (!exLogic.User.hasAccess("PILOTLOG.EDIT"))
          return RedirectToAction("NoAccess", "Home");
        ViewBag.Title = "Edit Pilot Log";
        ExponentPortalEntities db = new ExponentPortalEntities();
        db.Entry(PilotLog).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("UserDetail", "User", new { ID = PilotLog.PilotId });


      } catch {
        return View();
      }
    }



    // POST: PilotLog/Delete/5

    public string Delete([Bind(Prefix = "ID")]int LogId = 0) {
      try {
        // TODO: Add delete logic here
        if (!exLogic.User.hasAccess("PILOTLOG.DELETE"))
          return "Access Denied";

        string SQL = "DELETE FROM MSTR_PILOT_LOG WHERE ID=" + LogId;
        Util.doSQL(SQL);

        return Util.jsonStat("OK");
      } catch {
        return Util.jsonStat("Error");
      }
    }


    public ActionResult Detail([Bind(Prefix = "ID")] int UserID) {
      if (!exLogic.User.hasAccess("PILOTLOG.VIEW"))
        return RedirectToAction("NoAccess", "Home");

      ExponentPortalEntities db = new ExponentPortalEntities();
      Models.MSTR_User User = db.MSTR_User.Find(UserID);
      if (User == null)
        return RedirectToAction("Error", "Home");
      ViewBag.Title = User.FirstName;

      return View(User);

    }//UserDetail()
    public string PilotLogTotal([Bind(Prefix = "ID")] int PilotID) {
      return "";
      /*
            if (!exLogic.User.hasAccess("PILOTLOG.VIEW")) return "Access Denied";

            string SQL= "SELECT \n" +
      " sum(FixedWing) as FixedWing \n" +
      "  ,sum(MultiDashRotor) as MultiDashRotor \n" +      
      "  ,sum(SimulatedInstrument) as Simulator \n" +
      "  ,sum(AsflightInstructor) as AsflightInstructor \n" +
      "  ,sum(DualRecieved) as DualRecieved \n" +
      "  ,sum(FloatingCommand) as FloatInCommand \n" +
      " ,SUM( DualRecieved + FloatingCommand) as total \n" +
      " FROM[MSTR_Pilot_Log] \n" +
                "where PilotId=" + PilotID;
          
            StringBuilder Table = new StringBuilder();
            StringBuilder TFooter = new StringBuilder();
            String qDataTableID = "qViewTable";
            ExponentPortalEntities ctx = new ExponentPortalEntities();
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = SQL;
                TFooter.AppendLine("<td>");
                TFooter.AppendLine("---------Total-----------");
                TFooter.AppendLine("</td>");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["FixedWing"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["MultiDashRotor"].ToString());
                        TFooter.AppendLine("</td>");

                       
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["Simulator"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["AsflightInstructor"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["DualRecieved"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["FloatInCommand"].ToString());
                        TFooter.AppendLine("</td>");                       
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["Total"].ToString());
                        TFooter.AppendLine("</td>");
                    }



                }


            }
           

            Table.AppendLine("<table id=\"" + qDataTableID + "\" class=\"report\">");
            
            Table.AppendLine("<tfoot>");
            Table.Append(TFooter);
            Table.AppendLine("</tfoot>");
            Table.AppendLine("</table>");
            return Table.ToString();
      */
    }
    public string PilotLogDetails([Bind(Prefix = "ID")] int PilotID) {

      if (!exLogic.User.hasAccess("PILOTLOG.VIEWDETAIL"))
        return "Access Denied";
      string SQL =
      @"SELECT REPLACE(CONVERT(NVARCHAR, a.DATE, 106), ' ', '-') AS DATE,
       b.DroneName AS RPAS,
       a.FlightID,
       a.FixedWing,
       a.MultiDashRotor,
       a.SimulatedInstrument AS Simulator,
       a.AsflightInstructor,
       a.DualRecieved as DualReceived,
       a.FloatingCommand AS PilotInCommand,
       a.Id AS _PKey
      FROM MSTR_Pilot_Log a
      LEFT JOIN mstr_drone b
       ON a.DroneId = b.DroneId
      WHERE a.PilotId = " + +PilotID;

      string TotalSQL = @"SELECT 
        'Total' AS DATE,
        '' AS UASName,
        '' AS FlightID,
        sum(FixedWing) AS FixedWing,
        sum(MultiDashRotor) AS MultiDashRotor,
        sum(SimulatedInstrument) AS Simulator,
        sum(AsflightInstructor) AS AsflightInstructor,
        sum(DualRecieved) AS DualRecieved,
        sum(FloatingCommand) AS PilotInCommandm
      FROM [MSTR_Pilot_Log]
      WHERE PilotId = " + PilotID;

      qView nView = new qView(SQL);
      nView.TotalSQL = TotalSQL;
      nView.ClassName += " dataTable";
      if (nView.HasRows) {
        
        nView.isFilterByTop = false;
        return
          "<h2>Pilot Log Details</h2>\n" +
        nView.getDataTable(
          isIncludeData: true,
          isIncludeFooter: false,
          qDataTableID: "PilotLogDetails"
        );
      }
      return "";
    }
  }
}
