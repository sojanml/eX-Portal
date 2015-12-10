using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;
using System.Data;

namespace eX_Portal.Controllers
{
    public class MapController : Controller
    {
        // GET: Map
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult FightMap()
        {
            return View();
        }

        public ActionResult FlightData(int id=0)
        {
            ViewBag.FlightID = id;
            return View();
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult GetDrones()
        {
            IList<LiveDrone> LiveDrones = Util.GetLiveDrones("SELECT [DroneID],[DroneHex] ,[LastLatitude] ,[LastLongitude] FROM [ExponentPortal].[dbo].[LiveDrone] ");
            //  LiveDrones.SQL = ;
            //string JsonData=Json(LiveDrones)
            // return Json(JsonData);
            return Json(LiveDrones, JsonRequestBehavior.AllowGet);
           // return LiveDrones;GetMessage
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult GetFlightData(int id=0)
        {

            IList<DroneData> DroneDataList = Util.GetDroneData();
            //  LiveDrones.SQL = ;
            //string JsonData=Json(LiveDrones)
            // return Json(JsonData);
            return Json(DroneDataList, JsonRequestBehavior.AllowGet);
            // return LiveDrones;
        }


        public string Send()
        {
            /*
            SqlConnection con = new SqlConnection();
            DataSet dsLogin = new DataSet();
            Connection conSonrai = new Connection();
            // Response.Write("OK");
            string reuestquery = Request.QueryString["Message"];
            string[] data = reuestquery.Split('|');

            string strInsQry = "INSERT INTO [dbo].[WorkOrderTemp] ([QueueMessage],Workordernumber,[Lat] ,[Lon] ,[Alt] ,[Speed] ,[FixQuality] ,[Satellites]  ,[Timstamp] ,[Pitch]   ,[Roll]    ,[Heading]  ,[TotalFlightTime])"
            + "VALUES ('" + reuestquery + "','" + data[0] + "','" + data[1] + "','" + data[2] + "','" + data[3] + "','" + data[4] + "','" + data[5] + "','" + data[6] + "','" + data[7] + "','" + data[8] + "','" + data[9] + "','" + data[10] + "','" + data[11] + "')";

            con = conSonrai.getSonraiConnection();
            con.Open();
            dsLogin = SqlHelper.ExecuteDataset(con, CommandType.Text, strInsQry);
            con.Close();*/
            return "";
        }
    }
}