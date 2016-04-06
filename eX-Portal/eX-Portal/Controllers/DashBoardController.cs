﻿using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using eX_Portal.ViewModel;
using System.Text;
using System.IO;
using System.Xml;

namespace eX_Portal.Controllers
{
    public class DashBoardController : Controller
    {
    // GET: DashBoard
        public ActionResult Default()
        {
      return View();
    }

        public ActionResult Dewa()
        {
      String SQL = @"Select 
        Max(DroneDocuments.ID) as LastID,
        DroneDocuments.DocumentType,
        Max(DroneDocuments.UploadedDate) as LastDate
      FROM
        DroneDocuments,
        MSTR_Drone
      WHERE
        MSTR_Drone.DroneID = DroneDocuments.DroneID AND
        MSTR_Drone.AccountID = " + Util.getAccountID() + @" AND
        DroneDocuments.DocumentType IN (
          'Power Line Inspection',
          'Water Sampling',
          'Red Tide and Oil Spill Detection',
          'Power Plants Surveillance',
            'Vehicle Tracking - QR Codes',
            'RFID Inventory Tracking',
'Port Facility Surveillance'
        ) 
      Group BY
        DroneDocuments.DocumentType
      ORDER BY
        LastDate";
      var Rows = Util.getDBRows(SQL);
      return View(Rows);
    }

        public ActionResult SubSection(int DocumentID = 0)
        {
      var DB = new ExponentPortalEntities();
      var Doc = DB.DroneDocuments.Find(DocumentID);

      return View(Doc);
    }



        [System.Web.Mvc.HttpGet]
        public JsonResult getUASLastFlightChartData()
        {
            string DroneName;
            List<object> iData = new List<object>();
            List<string> labels = new List<string>();
            List<string> labelsShort = new List<string>();
            List<string> labelsLastDate = new List<string>();
            List<int> lst_dataItem_2 = new List<int>();
            List<int> lst_dataItem_1 = new List<int>();
            List<int> lst_dataItem_3 = new List<int>();
            List<int> lst_dataItem_4 = new List<int>();
            List<int> lst_dataItem_5 = new List<int>();


            IList<ChartViewModel> ChartList = Util.getUASLastFlightChartData();
            foreach (ChartViewModel FMD in ChartList)
            {
                DroneName = FMD.DroneName;
                labels.Add(FMD.DroneName);
                labelsShort.Add(DroneName.Split('-').Last());
                lst_dataItem_1.Add(Convert.ToInt32(FMD.TotalFightTime / 60));
                labelsLastDate.Add(FMD.LastFlightTime.ToString());



            }
            iData.Add(labels);
            iData.Add(lst_dataItem_1);
            iData.Add(labelsShort);
           
            //  iData.Add(lst_dataItem_2);
            //iData.Add(lst_dataItem_3);
            //iData.Add(lst_dataItem_4);
            //iData.Add(lst_dataItem_5);

            return Json(iData, JsonRequestBehavior.AllowGet);

        }

        

        [System.Web.Mvc.HttpGet]
        public JsonResult getCurrentFlightChartData()
        {
            string DroneName;
            List<object> iData = new List<object>();
            List<string> labels = new List<string>();
            List<string> labelsShort = new List<string>();
            
            List<int> lst_dataItem_2 = new List<int>();
            List<int> lst_dataItem_1 = new List<int>();
            List<int> lst_dataItem_3 = new List<int>();
            List<int> lst_dataItem_4 = new List<int>();
            List<int> lst_dataItem_5 = new List<int>();


            IList<ChartViewModel> ChartList = Util.getCurrentFlightChartData();
            foreach (ChartViewModel FMD in ChartList)
            {
                //labels.Add(FMD.DroneName);
                DroneName = FMD.DroneName;
                labels.Add(FMD.DroneName);
                labelsShort.Add(DroneName.Split('-').Last());
                lst_dataItem_1.Add(Convert.ToInt32(FMD.TotalFightTime / 60));
                lst_dataItem_2.Add(Convert.ToInt32(FMD.CurrentFlightTime / 60));


            }
            iData.Add(labels);
            
            iData.Add(lst_dataItem_1);
            iData.Add(lst_dataItem_2);
            iData.Add(labelsShort);
            //iData.Add(lst_dataItem_3);
            //iData.Add(lst_dataItem_4);
            //iData.Add(lst_dataItem_5);

            return Json(iData, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Mvc.HttpGet]
        public JsonResult getCurrentPilotChartData()
        {
            List<object> iData = new List<object>();
            List<string> labels = new List<string>();
            List<int> lst_dataItem_2 = new List<int>();
            List<int> lst_dataItem_1 = new List<int>();
            List<int> lst_dataItem_3 = new List<int>();
            List<int> lst_dataItem_4 = new List<int>();
            List<int> lst_dataItem_5 = new List<int>();


            IList<ChartViewModel> ChartList = Util.getCurrentPilotChartData();
            foreach (ChartViewModel FMD in ChartList)
            {
                labels.Add(FMD.PilotName);
                lst_dataItem_1.Add(Convert.ToInt32(FMD.PilotTotalHrs));
                lst_dataItem_2.Add(Convert.ToInt32(FMD.PilotCurrentMonthHrs));


            }
            iData.Add(labels);
            iData.Add(lst_dataItem_1);
            iData.Add(lst_dataItem_2);
            //iData.Add(lst_dataItem_3);
            //iData.Add(lst_dataItem_4);
            //iData.Add(lst_dataItem_5);

            return Json(iData, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Mvc.HttpGet]
        public JsonResult getCurrentTime()
        {
          //  TimeSpan t = new TimeSpan(0, addMinutes, 0);
            return Json(System.DateTime.Now.ToString(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Internal()

        {
            string City, Lat = null, Lng = null, woeid;
            if (Session["Lat"] != null)
            {
                Lat = Session["Lat"].ToString();
            }
            else
            {
                Lat = "25.2048";
            }
            if (Session["Long"] != null)
            {
                Lng = Session["Long"].ToString();
            }
            else
            {
                Lng = "55.2708";
            }

            WeatherViewModel Weather = new WeatherViewModel();
            //getting the exact place from lat and long
            City = Util.GetLocation(Lat, Lng);
            //getting the woeid from yahoo api
            woeid = Util.GetWOEID(City);
            //getting the weather information from  woeid           
            Weather = Util.GetCurrentConditions(woeid);
            return View(Weather);
        }



        public ActionResult ChartDetails()
        {
            String SQL = @"select t.DroneId,
                           v.DroneName,
                            max(T.TotalFlightTime) as TotalFlightTime,
                           CASE WHEN  max(T.TotalFlightTime) - min(k.FlightTime)IS NULL or 
                            max(T.TotalFlightTime) - min(k.FlightTime) = 0 
                            THEN max(T.TotalFlightTime) ELSE max(T.TotalFlightTime) - min(k.FlightTime) END as CurrentFlightTime,
                            Count(*) Over() as _TotalRecords,
                            t.DroneId as _PKey
                            from MSTR_Drone v
                            join FlightMapData t on v.DroneId = t.DroneId 
                            left join(select u.DroneId, min(u.ReadTime) as ReadTime,
                            max(u.TotalFlightTime) as FlightTime
                            from FlightMapData u
                            where
                            convert(nvarchar(30), u.ReadTime, 120)
                            BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) 
                            AND  GETDATE()
                            group by  u.DroneId )k on t.DroneId = k.DroneId
                            group by t.DroneId,v.DroneName";

            qView nView = new qView(SQL);
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text /javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }
  }


}
}