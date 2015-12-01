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

        public ActionResult FlightData()
        {
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
           // return LiveDrones;
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult GetFlightData()
        {

            IList<DroneData> DroneDataList = Util.GetDroneData();
            //  LiveDrones.SQL = ;
            //string JsonData=Json(LiveDrones)
            // return Json(JsonData);
            return Json(DroneDataList, JsonRequestBehavior.AllowGet);
            // return LiveDrones;
        }




    }
}