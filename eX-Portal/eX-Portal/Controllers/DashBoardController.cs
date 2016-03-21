using eX_Portal.exLogic;
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

namespace eX_Portal.Controllers {
  public class DashBoardController : Controller {
    // GET: DashBoard
    public ActionResult Default() {
      return View();
    }

    public ActionResult Dewa() {
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

    public ActionResult SubSection(int DocumentID = 0) {
      var DB = new ExponentPortalEntities();
      var Doc = DB.DroneDocuments.Find(DocumentID);

      return View(Doc);
    }
        [System.Web.Mvc.HttpGet]
        public void getWeatherData(string Location)
        {

            
        Util.GetCurrentConditions("AEXX0004" );


        }

        [System.Web.Mvc.HttpGet]
        public JsonResult getCurrentFlightChartData()
        {
            List<object> iData = new List<object>();
            List<string> labels = new List<string>();
            List<int> lst_dataItem_2 = new List<int>();
            List<int> lst_dataItem_1 = new List<int>();
            List<int> lst_dataItem_3 = new List<int>();
            List<int> lst_dataItem_4 = new List<int>();
            List<int> lst_dataItem_5 = new List<int>();
          
           
            IList<ChartViewModel> ChartList = Util.getCurrentFlightChartData();
            foreach (ChartViewModel FMD in ChartList)
            {
                labels.Add(FMD.DroneName);
                lst_dataItem_1.Add(Convert.ToInt32(FMD.TotalFightTime/60));
                lst_dataItem_2.Add(Convert.ToInt32(FMD.CurrentFlightTime/60));              


            }
            iData.Add(labels);
            iData.Add(lst_dataItem_1);
            iData.Add(lst_dataItem_2);
            //iData.Add(lst_dataItem_3);
            //iData.Add(lst_dataItem_4);
            //iData.Add(lst_dataItem_5);

            return Json(iData, JsonRequestBehavior.AllowGet);

        }



        public ActionResult Internal()

        {
            string City,Lat=null,Lng=null,woeid;
            if (Session["Lat"]!=null)
            {
                Lat = Session["Lat"].ToString();
            }
            else
            {
                Lat = "25.2048";
            }
            if (Session["Long"]!=null)
            {
                Lng = Session["Long"].ToString();
            }
            else
            {
                Lng= "55.2708";
            }
          
            WeatherViewModel Weather= new WeatherViewModel();
            //getting the exact place from lat and long
            City = Util.GetLocation(Lat, Lng);
            //getting the woeid from yahoo api
            woeid = Util.GetWOEID(City);
            //getting the weather information from  woeid           
           Weather= Util.GetCurrentConditions(woeid);
            return View(Weather);
        }

  }


    
}