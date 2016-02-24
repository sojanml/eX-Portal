using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using eX_Portal.exLogic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers
{
    public class SummaryController : Controller
    {
        public ExponentPortalEntities ctx = new ExponentPortalEntities();

        public ActionResult UASSummary()
        {
            using (var ctx = new ExponentPortalEntities())
            {

                List<Summary> oList = new List<Summary>();
                Summary odata = new Summary();
                //Count
                var UASCount = ctx.Database.SqlQuery<int>("select count(DroneId) from MSTR_Drone").ToList();
                var value =Convert.ToInt16((from p in UASCount select p).Single());
                int UAS_count =value;
                odata.UASCount = value;
                //FlightTime
                var HighestFlightTime =ctx.Database.SqlQuery<int>("select Max(TotalFlightTime) from BlackBoxData ");
                var value1 = Convert.ToInt16((from p in HighestFlightTime select p).Single());
                int FlightTime = value1;
                odata.HighestFlightTime = value1;
                //Last10daysCount
                var Last10days = ctx.Database.SqlQuery<int>("SELECT COUNT(DroneId) FROM  DroneFlight WHERE Flightdate >= DATEADD(day, -10, getdate())and   Flightdate <= getdate()").ToList();
                var data = Convert.ToInt32((from p in Last10days select p).Single());
                int day = data;
                odata.Last10days = data;
                //Last30days
                var Last30days = ctx.Database.SqlQuery<int>("SELECT COUNT(DroneId) FROM  DroneFlight WHERE Flightdate >= DATEADD(day, -30, getdate())and   Flightdate <= getdate()").ToList();
                var data1 = Convert.ToInt32((from p in Last30days select p).Single());
                int day1 = data1;
                odata.Last30days = data1;
                //TotalFlightTime
                var TotalFlightTime = ctx.Database.SqlQuery<int>("select sum(TotalFlightTime) from BlackBoxData");
                var value4 = Convert.ToInt32((from p in TotalFlightTime select p).Single());
                int TFT = value4;
                odata.TotalFlightTime = value4;
                oList.Add(odata);
                return View(oList);
            }
        }
        public ActionResult UASFlightSummary([Bind(Prefix = "ID")] int DroneID=0 )
        {
            using (var ctx = new ExponentPortalEntities())
            {
                string f = Convert.ToString(ViewData["DroneID"]);
                List<Summary> FList = new List<Summary>();
                Summary fdata = new Summary();
                //Count
                string query = "select count(distinct(FlightID)) from FLIGHTMAPDATA where droneID = " + DroneID;
                var TotalFlightcount = ctx.Database.SqlQuery<int?>(query).ToList();
                var value = 0;
                if (TotalFlightcount.Count != 0)
                {
                   value = Convert.ToInt32((from p in TotalFlightcount select p).Single());
                }
                int Flightcount = value;
                fdata.TotalFlightcount = value;
                //TotalFlightHour
                string query2 = "SELECT FlightHour FROM MSTR_DroneService  where droneID = " + DroneID;
                var TotalFlightHour = ctx.Database.SqlQuery<int?>(query2).ToList();
                var value1 = 0;
                if (TotalFlightHour.Count != 0)
                  {
                    value1 = Convert.ToInt32((from p in TotalFlightHour select p).Single());
                  }
                int TotalHour = value1;
                fdata.TotalFlightHour = value1;
                string query3 = "select datediff(dd,commissiondate,getdate())  from mstr_drone where droneid=" + DroneID;
                var UASAge = ctx.Database.SqlQuery<int>(query3).ToList();
                var value2 = 0;
                if (UASAge.Count != 0)
                {
                    value2 = Convert.ToInt16((from p in UASAge select p).Single());
                }
                int age = value2;
                fdata.UASAge = value2;


                FList.Add(fdata);



                return View(FList);
            }
        }
        public ActionResult FlightHistorySummary()
        {
            using (var ctx = new ExponentPortalEntities())
            {
                List<Summary> FHList = new List<Summary>();
                Summary fhdata = new Summary();
                List<DateTime> dlist = new List<DateTime>();
                //count
                var Total_Flight_count = ctx.Database.SqlQuery<int>("select count(distinct(FlightID)) from FLIGHTMAPDATA").ToList();
                var value = Convert.ToInt16((from p in Total_Flight_count select p).Single());
                int Flightcount = value;
                fhdata.Total_Flight_count = value;
                //TotalFlightHour
                var Total_Flight_Hour = ctx.Database.SqlQuery<int>("select sum(flightHour) from MSTR_DroneService").ToList();
                var value1 = Convert.ToInt16((from p in Total_Flight_Hour select p).Single());
                int TotalHour = value1;
                fhdata.Total_Flight_Hour = value1;
                //last flight date

                var Last_Flight_Date = ctx.Database.SqlQuery<DateTime>("select  top 1 (FlightDate)  from DroneFlight order by FlightDate desc  ").ToList();
                DateTime value2 = Convert.ToDateTime((from p in Last_Flight_Date select p).Single());
                DateTime flightdate = value2;
                fhdata.Last_Flight_Date = flightdate;

                //last UAS ID USED BASED on FightDate
                var Last_UAS_ID = ctx.Database.SqlQuery<int>("select top 1(droneId)from droneflight order by   flightdate desc").ToList();
                var value3 = Convert.ToInt16((from p in Last_UAS_ID select p).Single());
                int UASID = value3;
                fhdata.Last_UAS_ID = value3;
                

                FHList.Add(fhdata);
                return View(FHList);

            }
        }
        public ActionResult UserList()
        {
            using (var ctx = new ExponentPortalEntities()) {

                List<Summary> UserList = new List<Summary>();
                Summary ulist = new Summary();
                //Count
                var UserCount = ctx.Database.SqlQuery<int>("SELECT COUNT(USERID) FROM MSTR_USER").ToList();
                var value = Convert.ToInt16((from p in UserCount select p).Single());
                int Ucount = value;
                ulist.UserCount = value;
                //InActiveUser
                var InActive_User = ctx.Database.SqlQuery<int>("SELECT COUNT(USERID) FROM MSTR_USER WHERE ISACTIVE='0'").ToList();
                var value1 = Convert.ToInt16((from p in InActive_User select p).Single());
                int Icount = value1;
                ulist.InActive_User = value1;
                UserList.Add(ulist);
                return View(UserList);

            }

        }
        public ActionResult PilotList()
        {
            using (var ctx = new ExponentPortalEntities())
            {
                List<Summary> Plist = new List<Summary>();
                Summary plist = new Summary();
                //Count
                var PilotCount = ctx.Database.SqlQuery<int>("SELECT count( [UserId]) FROM [ExponentPortal].[dbo].[MSTR_User_Pilot]").ToList();
                var value = Convert.ToInt16((from p in PilotCount select p).Single());
                int pcount = value;
                plist.PilotCount = value;
               

                var Certificatecount = ctx.Database.SqlQuery<int>("  select  count(distinct userid)   from [ExponentPortal].[dbo].[MSTR_User_Pilot_Certification]").ToList();
                var value1 = Convert.ToInt16((from p in Certificatecount select p).Single());
                int ccount = value1;
                plist.Certificatecount = value1;

                Plist.Add(plist);

                return View(Plist);
            }


        }

        public ActionResult UASServiceHistory()

        {
            using (var ctx = new ExponentPortalEntities())
            {
                List<Summary> ServiceList = new List<Summary>();
                Summary shlist = new Summary();



                return View();


            }
        }
    }


}