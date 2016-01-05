using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using System.Data.Entity;
using eX_Portal.exLogic;
using System.Text;

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
              
                db.Dispose();
                return RedirectToAction("Detail", new { ID = PilotLog.PilotId });
            }
            else
            {
                ViewBag.Title = "Create Drone Flight";
                return View(PilotLog);
            }

        }

        // GET: PilotLog/Edit/5
        public ActionResult Edit([Bind(Prefix = "ID")] int LogId = 0)
        {

            ViewBag.Title = "Edit Pilot Log";
            ExponentPortalEntities db = new ExponentPortalEntities();
            MSTR_Pilot_Log PilotLog = db.MSTR_Pilot_Log.Find(LogId);
            return View(PilotLog);

        }

        // POST: PilotLog/Edit/5
        [HttpPost]
        public ActionResult Edit(MSTR_Pilot_Log PilotLog)
        {
            try
            {
                ViewBag.Title = "Edit Pilot Log";
                ExponentPortalEntities db = new ExponentPortalEntities();
                db.Entry(PilotLog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Detail", new { ID = PilotLog.PilotId });


            }
            catch
            {
                return View();
            }
        }

    

        // POST: PilotLog/Delete/5
        [HttpPost]
        public string Delete([Bind(Prefix = "ID")]int LogId = 0)
        {
            try
            {
                // TODO: Add delete logic here


                string SQL = "DELETE FROM MSTR_PILOT_LOG WHERE ID=" + LogId;
                Util.doSQL(SQL);

                return Util.jsonStat("OK");
            }
            catch
            {
                return Util.jsonStat("Error");
            }
        }


        public ActionResult Detail([Bind(Prefix = "ID")] int UserID)
        {
           

            ExponentPortalEntities db = new ExponentPortalEntities();
            Models.MSTR_User User = db.MSTR_User.Find(UserID);
            if (User == null) return RedirectToAction("Error", "Home");
            ViewBag.Title = User.FirstName;
            return View(User);

        }//UserDetail()
        public string PilotLogTotal([Bind(Prefix = "ID")] int PilotID)
        {


            string SQL= "SELECT \n" +
      " sum(EngineLand) as EngineLand \n" +
      "  ,sum(Night) as Night \n" +
      "  ,sum(ActualInstrument) as ActualInstrument \n" +
      "  ,sum(SimulatedInstrument) as SimulatedInstrument \n" +
      "  ,sum(AsflightInstructor) as AsflightInstructor \n" +
      "  ,sum(DualRecieved) as DualRecieved \n" +
      "  ,sum(FloatingCommand) as FloatingCommand \n" +
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
                TFooter.AppendLine("---------Total---------");
                TFooter.AppendLine("</td>");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["EngineLand"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["Night"].ToString());
                        TFooter.AppendLine("</td>");

                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["ActualInstrument"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["SimulatedInstrument"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["AsflightInstructor"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["DualRecieved"].ToString());
                        TFooter.AppendLine("</td>");
                        TFooter.AppendLine("<td>");
                        TFooter.AppendLine(reader["FloatingCommand"].ToString());
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
        }
        public string PilotLogDetails([Bind(Prefix = "ID")] int PilotID)
        {


            string SQL = "SELECT \n" +
                        " CONVERT(NVARCHAR, a.Date, 103) AS Date\n   " +   
                        " ,b.DroneName as UASName " +
                        " ,a.RouteFrom " +
                        ",a.RouteTo    " +
                        " ,a.Remarks   " +
                        " ,a.EngineLand " +
                        ",a.Night" +
                        " ,a.ActualInstrument " +
                        " ,a.SimulatedInstrument " +
                        " ,a.AsflightInstructor " +
                        " ,a.DualRecieved  " +
                        " ,a.FloatingCommand " +                      
                        " , a.DualRecieved + a.FloatingCommand as TotalDuration " +
                        " , a.Id as _PKey" +
                        " FROM MSTR_Pilot_Log  " +
                        "a left join mstr_drone b  " +
                        "on a.DroneId = b.DroneId  " +
                        " where a.PilotId=" + PilotID;

            qView nView = new qView(SQL);
            if (nView.HasRows)
            {
                nView.isFilterByTop = false;
                return
                  "<h2>Pilot Log Details</h2>\n" +
                  nView.getDataTable(isIncludeData: true, isIncludeFooter: false, qDataTableID: "PilotLogDetails");
            }

            return "";


        }
    }
    }
