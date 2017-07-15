﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Web.Mvc;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
namespace eX_Portal.exLogic {


    public partial class Util {
        public ExponentPortalEntities db = new ExponentPortalEntities();
        static IEnumerable<SelectListItem> ChartList = Enumerable.Empty<SelectListItem>();
        static IEnumerable<SelectListItem> DropDownLists = Enumerable.Empty<SelectListItem>();
        //private static ExponentPortalEntities cotx;


        public static IEnumerable<SelectListItem> LUPTypeList() {
            List<SelectListItem> SelectList = new List<SelectListItem>();


            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();
                    SQL = "SELECT MIN(x.id)as value,x.type  as name FROM LUP_Drone x  JOIN(SELECT p.type " +
                        "FROM LUP_Drone p  GROUP BY p.type) y ON y.type = x.type GROUP BY x.type";



                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDownt

        public static string DecToBin(int dec) {
            if (dec < 0) return ""; // can't be negative
            List<char> list = new List<char>();

            do {
                int div = dec / 2;
                int rem = dec % 2;

                dec = div;
                list.Add((char)(rem + 48));
            }
            while (dec > 0);

            list.Reverse();
            return new string(list.ToArray());
        }



        //getting the flightid using date and time for geo tagging
        public static int getFlightID(int DroneID, DateTime FileCreatedOn)
        {

            int FlightID = 0;

            String SQL = @"Select TOP 1 
                                 FlightID       
                                 from 
                                 FlightMapData 
                                             where 
                                DroneID=" + DroneID + @" AND
                                ReadTime >=  '" + Util.toSQLDate(FileCreatedOn.ToUniversalTime()) + @"' AND
                                ReadTime <=  '" + Util.toSQLDate(FileCreatedOn.AddMinutes(10).ToUniversalTime()) + @"'
                                ORDER BY
                                ReadTime DESC";
            var Row = Util.getDBRow(SQL);
            var theGPS = new GPSInfo();
            if (Row["hasRows"].ToString() == "True")
            {
                FlightID = Util.toInt(Row["FlightID"]);

          
                  

            }
            else {
                FlightID = Util.getOtherFlightID(DroneID, FileCreatedOn);       
                 }
            return FlightID;
        }

    public static int getOtherFlightID(int DroneID, DateTime FileCreatedOn) {

      int FlightID = 0;

      String SQL = @"Select TOP 1 
                                ID       
                                 from 
                                 DroneFlight
                                             where 
                                DroneID=" + DroneID + @" AND
                                FlightDate <=  '" + Util.toSQLDate(FileCreatedOn.ToUniversalTime()) + @"'                               
                                ORDER BY                              
                                FlightDate DESC";
      var Row = Util.getDBRow(SQL);
      var theGPS = new GPSInfo();
      if (Row["hasRows"].ToString() == "True") {
        FlightID = Util.toInt(Row["ID"]);




      }
      return FlightID;
    }


    //for new chart pilot data
    public static IList<ChartViewModel> getCurrentPilotData()
        {
            IList<ChartViewModel> ChartList = new List<ChartViewModel>();
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();

                    cmd.CommandText = "usp_Portal_GetPilotData";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";

                   
                  
                        if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                        {

                            Param2.Value = 1;
                        }
                        else
                        {
                            Param2.Value = 0;
                        }
                   
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartViewModel dd = new ChartViewModel();
                            dd.PilotName = reader["FirstName"].ToString();
                            dd.UserID = Util.toInt( reader["UserID"].ToString());
                            dd.TotalMultiDashHrs = Util.toDouble(reader["TotalMultiDashHrs"].ToString());
                            dd.TotalFixedWingHrs = Util.toDouble(reader["TotalFixedWingHrs"].ToString());
                            dd.LastMultiDashHrs = Util.toDouble(reader["LastMultiDashHrs"].ToString());
                            dd.LastFixedwingHrs = Util.toDouble(reader["LastFixedwingHrs"].ToString());
                            dd.LastMonthFixedwingHrs = Util.toDouble(reader["LastMonthFixedwingHrs"].ToString());
                            dd.LastMonthMultiDashHrs = Util.toDouble(reader["LastMonthMultiDashHrs"].ToString());

                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
            //return the list objects
        }

        public static IList<ChartViewModel> getCurrentPilotChartData() {

            IList<ChartViewModel> ChartList = new List<ChartViewModel>();

            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();

                    cmd.CommandText = "usp_Portal_GetPilotChartData";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";

                    if (exLogic.User.hasAccess("PILOT"))
                    {
                        Param2.Value = 0;
                    }
                    else
                    {
                        if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                        {

                            Param2.Value = 1;
                        }
                        else
                        {
                            Param2.Value = 0;
                        }
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;


                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            ChartViewModel dd = new ChartViewModel();
                            dd.PilotName = reader["FirstName"].ToString();
                            dd.PilotTotalHrs = Util.toInt(reader["TotalPilotHours"].ToString());
                            dd.PilotCurrentMonthHrs = Util.toInt(reader["LastMonthPilotHours"].ToString());
                            dd.PilotLastFlightHrs = Util.toInt(reader["LastPilottHours"].ToString());

                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
            //return the list objects
        }

        public static IList<ChartViewModel> getUASLastFlightChartData() {

            IList<ChartViewModel> ChartList = new List<ChartViewModel>();

            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();

                    cmd.CommandText = "usp_Portal_GetLastFlightChartData";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";
                    if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {

                        Param2.Value = 1;
                    } else {
                        Param2.Value = 0;
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;



                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            ChartViewModel dd = new ChartViewModel();
                            dd.DroneName = reader["DroneName"].ToString();
                            dd.TotalFightTime = Util.toInt(reader["LastFlightHours"].ToString());

                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
            //return the list objects
        }


        public static IList<ChartViewModel> getCurrentFlightChartData() {

            IList<ChartViewModel> ChartList = new List<ChartViewModel>();

            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetFlightChartData";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";
                    if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
                        Param2.Value = 1;
                    } else {
                        Param2.Value = 0;
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {

                            ChartViewModel dd = new ChartViewModel();
                            dd.DroneName = reader["DroneName"].ToString();
                            dd.TotalFightTime = Util.toInt(reader["TotalFlightHours"].ToString());
                            dd.CurrentFlightTime = Util.toInt(reader["LastMonthHours"].ToString());
                            dd.LastFlightTime = Util.toInt(reader["LastFlightHours"].ToString());
                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
            //return the list objects
        }

        public static IList<ChartViewModel> getRecentFlightChartData()
        {

            IList<ChartViewModel> ChartList = new List<ChartViewModel>();

            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetFlightChartData";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";
                    if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                    {
                        Param2.Value = 1;
                    }
                    else
                    {
                        Param2.Value = 0;
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            ChartViewModel dd = new ChartViewModel();
                            dd.DroneID= Util.toInt(reader["DroneID"].ToString());
                            dd.DroneName = reader["DroneName"].ToString();
                            dd.ShortName = dd.DroneName.Split('-').Last();
                            dd.AccountID = Util.toInt(reader["AccountID"].ToString());
                            dd.AccountName = reader["AccountName"].ToString();
                            dd.ChartColor= reader["ChartColor"].ToString();
                            dd.TotalFightTime = Util.toDouble(reader["TotalFlightHours"].ToString());
                            dd.TotalFightTime = Math.Round((dd.TotalFightTime / 60), 2);
                            dd.CurrentFlightTime = Util.toDouble(reader["LastMonthHours"].ToString());
                            dd.CurrentFlightTime = Math.Round((dd.CurrentFlightTime / 60), 2);
                            dd.LastFlightTime = Util.toDouble(reader["LastFlightHours"].ToString());
                            dd.LastFlightTime = Math.Round((dd.LastFlightTime / 60), 2);
                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
        }
        //return the list objects

//chart for getting the qumulative graph for the last 12 months
        public static IList<ChartViewModel> getFlightHoursByAccount()
        {

            IList<ChartViewModel> ChartList = new List<ChartViewModel>();

            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();


                    cmd.CommandText = "Report_FlightHours_By_Account";
                    DbParameter Param1 = cmd.CreateParameter();
                    Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";
                    if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                    {
                        Param2.Value = 1;
                    }
                    else
                    {
                        Param2.Value = 0;
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            ChartViewModel dd = new ChartViewModel();
                            
                           
                            dd.AccountID = Util.toInt(reader["AccountID"].ToString());
                            dd.AccountName = reader["AccountName"].ToString();
                            dd.ChartColor = reader["ChartColor"].ToString();
                            dd.M1 = Util.toDouble(reader["M1"].ToString());
                            dd.M1 = Math.Round((dd.M1 / 60), 2);
                            dd.M2 = Util.toDouble(reader["M2"].ToString());
                            dd.M2 = Math.Round((dd.M2 / 60), 2);
                            dd.M3 = Util.toDouble(reader["M3"].ToString());
                            dd.M3 = Math.Round((dd.M3 / 60), 2);
                            dd.M4 = Util.toDouble(reader["M4"].ToString());
                            dd.M4 = Math.Round((dd.M4 / 60), 2);
                            dd.M5 = Util.toDouble(reader["M5"].ToString());
                            dd.M5 = Math.Round((dd.M5 / 60), 2);
                            dd.M6 = Util.toDouble(reader["M6"].ToString());
                            dd.M6 = Math.Round((dd.M6 / 60), 2);
                            dd.M7 = Util.toDouble(reader["M7"].ToString());
                            dd.M7 = Math.Round((dd.M7 / 60), 2);
                            dd.M8 = Util.toDouble(reader["M8"].ToString());
                            dd.M8 = Math.Round((dd.M8 / 60), 2);
                            dd.M9 = Util.toDouble(reader["M9"].ToString());
                            dd.M9 = Math.Round((dd.M9 / 60), 2);
                            dd.M10 = Util.toDouble(reader["M10"].ToString());
                            dd.M10 = Math.Round((dd.M10 / 60), 2);
                            dd.M11 = Util.toDouble(reader["M11"].ToString());
                            dd.M11 = Math.Round((dd.M11 / 60), 2);
                            dd.M12 = Util.toDouble(reader["M12"].ToString());
                            dd.M12 = Math.Round((dd.M12 / 60), 2);
                            ChartList.Add(dd);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
        }

//end chart for getting the qumulative graph for the last 12 months


        public static IList<ChartAlertViewModel> getAlertData()
        {

            IList<ChartAlertViewModel> ChartList = new List<ChartAlertViewModel>();

            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetAlertChartData";
                       DbParameter Param1 = cmd.CreateParameter();
                     Param1.ParameterName = "@AccountID";
                    Param1.Value = Util.getAccountID();
                    DbParameter Param2 = cmd.CreateParameter();
                    Param2.ParameterName = "@IsAccess";
                    if (!exLogic.User.hasAccess("DRONE.VIEWALL"))
                    {
                        Param2.Value = 1;
                    }
                    else
                    {
                        Param2.Value = 0;
                    }
                    cmd.Parameters.Add(Param1);
                    cmd.Parameters.Add(Param2);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())                        {

                            ChartAlertViewModel alerts = new ChartAlertViewModel();
                            alerts.AlertType = reader["AlertCategory"].ToString();
                            alerts.TotalAlert = Util.toInt(reader["TotalAlerts"].ToString());
                            alerts.CurrentMonthAlert = Util.toInt(reader["ThisMonthAlert"].ToString());
                            alerts.LastFlightAlert = Util.toInt(reader["LastFlightAlert"].ToString());
                            ChartList.Add(alerts);

                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return ChartList;
            //return the list objects
        }





        public static IList<GeoTagReport> getAllGeoTag(DateTime FromDate, DateTime ToDate, int IsCompany, int DroneID)

        {

            IList<GeoTagReport> GeoList = new List<GeoTagReport>();

            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();
                    string SQLFilter = "";
                    if (DroneID != 0)
                    {

                        SQLFilter = @"select  o.Latitude,
                        o.Longitude,      
                        o.Altitude,
                        o.DocumentName,
                        o.FlightID,
                        o.DocumentDate,
                        o.ID,
                        d.DroneName
                            from
                          DroneDocuments o
                             left join mstr_drone d on o.DroneId = d.DroneId
                      where o.DocumentType = 'GEO Tag' and
                          o.DroneID=" + DroneID + " and" +
                     "   (o.DocumentDate >= '" + FromDate + "' and  o.DocumentDate <='" + ToDate + "')";
                    }
                    else
                    {
                        SQLFilter = @"select  o.Latitude,
                        o.Longitude,      
                        o.Altitude,
                        o.DocumentName,
                        o.FlightID,
                        o.DocumentDate,
                        o.ID,
                        d.DroneName
                            from
                          DroneDocuments o
                             left join mstr_drone d on o.DroneId = d.DroneId
                      where o.DocumentType = 'GEO Tag' and
                         (o.DocumentDate >= '" + FromDate + "' and  o.DocumentDate <='" + ToDate + "')";
                    }
                    if (IsCompany == 1)
                    {
                        if (SQLFilter != "")
                            SQLFilter += " AND";
                        SQLFilter += " \n" +
                          "  d.AccountID=" + Util.getAccountID();
                    }

                    cmd.CommandText = SQLFilter;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {


                            GeoTagReport lst = new GeoTagReport();
                            lst.Latitude = Util.toDecimal(reader["Latitude"].ToString());
                            lst.Longitude = Util.toDecimal(reader["Longitude"].ToString());
                            lst.Altitude = Util.toDecimal(reader["Altitude"].ToString());
                            lst.DocumentName = reader["DocumentName"].ToString();
                            lst.FlightID = Util.toInt(reader["FlightID"]);
                            lst.UpLoadedDate = Util.toDate(reader["DocumentDate"].ToString());
                            lst.ID = Util.toInt(reader["ID"]);
                            lst.DroneName = reader["DroneName"].ToString();
                            GeoList.Add(lst);


                        }
                    }

                    ctx.Database.Connection.Close();


                }


            }



            return GeoList;
            //return the list objects
        }
        public static IEnumerable<SelectListItem> GetLookup(string type) {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();

                    SQL = "SELECT Type,TypeID,Code,Name from dbo.LUP_Drone where IsActive = 1 and Type ='" + type + "'";


                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["TypeID"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntL
        public static IEnumerable<SelectListItem> GetAccountList() {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();

                    SQL = "SELECT [AccountID] as Value  ,[Name] as Name FROM [MSTR_Account] ORDER BY [Name]";


                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntL
        public static IEnumerable<SelectListItem> GetProfileList() {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();

                    SQL = "SELECT [ProfileId] as Value  ,[ProfileName] as Name FROM [MSTR_Profile] ORDER BY [ProfileName]";

                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntList

        public static string GetWOEID(string City) {
            string WeatherWOEID = null, HttpSQL;
            HttpSQL = " http://query.yahooapis.com/v1/public/yql?q=select woeid from geo.places where text='" + City + "'";
            XmlDocument doc = new XmlDocument();
            doc.Load(HttpSQL);
            WeatherWOEID = doc.GetElementsByTagName("woeid")[0].InnerText;
            return WeatherWOEID;
        }

        public static string GetLocation(string lat, string lng) {
            string AddStart = null;
            System.Net.HttpWebRequest request = default(HttpWebRequest);
            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            string json = null;

            try {
                //Create the web request
                string Data= "https://maps.googleapis.com/maps/api/geocode/json?key=AIzaSyBSdl9aWjP5rHiceUgBVRxBqdQbc29cWKk&latlng=" + lat + "," + lng + "&sensor=false";
                //string data = "http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&sensor=false";
                request = (HttpWebRequest)WebRequest.Create(Data);
                //Get response   
                response = (HttpWebResponse)request.GetResponse();
                //Get the response stream into a reader   
                reader = new StreamReader(response.GetResponseStream());
                json = reader.ReadToEnd();
                response.Close();
                // textBox1.Text = json;
                if (json.Contains("ZERO_RESULTS")) {
                    // CurrentAddress.Text = "No Address Available";
                };
                if (json.Contains("formatted_address")) {
                    //CurrentAddress.Text = "Address Available";
                    int start = json.IndexOf("formatted_address");
                    int end = json.IndexOf(", USA");
                    AddStart = json.Substring(start + 21);
                    // string EndStart = json.Substring(end);
                    //  FinalAddress = AddStart.Replace(EndStart, ""); //Gives Full Address, One Line

                    //  CurrentAddress.Text = FinalAddress;

                };
            } catch (Exception ex) {
                string Message = "Error: " + ex.ToString();

                throw ex;
            } finally {
                if ((response != null))
                    response.Close();
            }
            if (AddStart != null) {
                AddStart = AddStart.Split(',')[0];
                // s.Replace("\"", "");
                AddStart = AddStart.Replace("\"", "");
                // AddStart = AddStart.Replace("\n", String.Empty);
                // AddStart = AddStart.TrimEnd('\r', '\n');
                // AddStart= AddStart.TrimStart('\r', '\n');
            } else {
                AddStart = "";
            }

            return AddStart;

        }
        public static WeatherViewModel GetCurrentConditions(string Location) {
            //code for parsing the api xml data from yahoo

            bool isInGeoLongitude = false, isInGeoLattitude = false;
            string temp;
            int rising, count = 0;

            IList<Forcast> ForcastList = new List<Forcast>();

            WeatherViewModel Weather = new WeatherViewModel();
            WeatherViewModel WeatherFromFile = new WeatherViewModel();
            // string forecastUrl = "http://weather.yahooapis.com/forecastrss?&" + "p=" + Location + "&u=c";
            //  string forecastUrl = "http://weather.yahooapis.com/forecastrss?w=" + Location + "&u=c";
            string forecastUrl = "https://query.yahooapis.com/v1/public/yql?q=select * from weather.forecast where woeid=" + Location + " AND u='f'";
            // open a XmlTextReader object using the constructed url
            XmlTextReader reader = new XmlTextReader(forecastUrl);
            // loop through xml result node by node

            while (reader.Read()) {
                // decide which type of node us currently being read
                switch (reader.NodeType) {
                    // xml start element
                    case XmlNodeType.Element:
                        // read the tag name and decide which objects to load
                        if (reader.Name.ToLower() == "yweather:location") {
                            Weather.City = reader.GetAttribute("city");
                            Weather.Region = reader.GetAttribute("region");
                            Weather.Country = reader.GetAttribute("country");
                        }
                        if (reader.Name.ToLower() == "yweather:units") {
                            // store in temporary variable
                            temp = reader.GetAttribute("temperature").ToLower();
                            // put it into the correct units

                            if (temp == "c") {
                                Weather.TemperatureUnit = "&deg;C";
                            } else {
                                Weather.TemperatureUnit = "ºF";
                            }

                            temp = reader.GetAttribute("distance");
                            if (temp == "km") {
                                Weather.DistanceUnit = "Kilometeres";
                            } else {
                                Weather.DistanceUnit = "Miles";
                            }

                            temp = reader.GetAttribute("pressure");
                            if (temp == "mb") {
                                Weather.PressureUnit = "Millibars";
                            } else {
                                Weather.PressureUnit = "PoundsPerSquareInch";
                            }

                            temp = reader.GetAttribute("speed");
                            if (temp == "kph") {
                                Weather.SpeedUnit = "KilometersPerHour";

                            } else {
                                Weather.SpeedUnit = "MilesPerHour";
                            }
                        }
                        if (reader.Name.ToLower() == "yweather:wind") {
                            Weather.Chill = reader.GetAttribute("chill").ToString();
                            Weather.Direction = reader.GetAttribute("direction").ToString();
                            Weather.Speed = reader.GetAttribute("speed").ToString();
                        }
                        if (reader.Name.ToLower() == "yweather:atmosphere") {
                            Weather.Humidity = reader.GetAttribute("humidity").ToString();
                            Weather.Visibility = Math.Round((double.Parse(reader.GetAttribute("visibility")) / 1.60934), 0);

                            Weather.Pressure = Math.Round(double.Parse(reader.GetAttribute("pressure")) * 0.0295301, 0);
                            rising = Convert.ToInt32(reader.GetAttribute("rising"));

                            rising = Convert.ToInt32(reader.GetAttribute("rising"));

                            if (rising == 0) {
                                Weather.PressureStatus = "Steady";
                            } else if (rising == 1) {
                                Weather.PressureStatus = "Rising";
                            } else if (rising == 2) {
                                Weather.PressureStatus = "Falling";
                            }

                        }
                        if (reader.Name.ToLower() == "yweather:astronomy") {
                            Weather.Sunrise = reader.GetAttribute("sunrise");
                            Weather.Sunset = reader.GetAttribute("sunset");
                        }
                        if (reader.Name.ToLower() == "yweather:condition") {
                            Weather.ConditionText = reader.GetAttribute("text");
                            Weather.ConditionCode = reader.GetAttribute("code");
                            //  Weather.c = (ConditionCodes)conditionCode;
                            Weather.ConditionTemperature = reader.GetAttribute("temp");
                            Weather.ConditionDate = reader.GetAttribute("date");
                        }
                        if (reader.Name.ToLower() == "yweather:forecast") {
                            Forcast fcast = new Forcast();
                            fcast.Date = reader.GetAttribute("date");
                            fcast.TempLow = reader.GetAttribute("low");
                            fcast.TempHigh = reader.GetAttribute("high");
                            fcast.status = reader.GetAttribute(6);
                            fcast.Code = int.Parse(reader.GetAttribute("code"));
                            fcast.Date = fcast.Date.Remove(fcast.Date.LastIndexOf(" "));
                            if (count <= 4) {
                                ForcastList.Add(fcast);
                            }
                            // code = (ConditionCodes)Convert.ToInt32(reader.GetAttribute("code"));
                            count++;

                        }
                        // set the flag if we're in the geo:long tag


                        if (reader.Name.ToLower() == "geo:long") {
                            isInGeoLongitude = true;
                        }
                        // set the flag if we're in the geo:lat tag
                        if (reader.Name.ToLower() == "geo:lat") {
                            isInGeoLattitude = true;
                        }
                        break;
                    // xml element text
                    case XmlNodeType.Text:
                        // if we're currently in the geo:lat tag
                        if (isInGeoLattitude) {
                            // read the value from the node
                            Weather.Lattitude = reader.Value;
                        }
                        // if we're currently in the geo:long tag
                        if (isInGeoLongitude) {
                            // read the value from the node
                            Weather.Longitude = reader.Value;
                        }
                        break;
                    // xml end element
                    case XmlNodeType.EndElement:
                        // clear the flag once we leave the geo:long tag
                        if (reader.Name.ToLower() == "geo:long") {
                            isInGeoLongitude = false;
                        }
                        // clear the flag once we leave the geo:lat tag
                        if (reader.Name.ToLower() == "geo:lat") {
                            isInGeoLattitude = false;
                        }
                        break;
                }
            }

            Weather.Forecast = ForcastList;
            //Checking the records if exist write to the file
            if (ForcastList.Count > 0) {
                //WriteToFile(Weather, Location);
            } else {
                //not exist read from the file and display that
                // WeatherFromFile = ReadFromFile(Location);
                WeatherFromFile = null;
                if (WeatherFromFile != null) {
                    Weather = WeatherFromFile;
                }
            }


            return Weather;

        }

        public static int getPilotProfileID(int AccountID)
        {
            String SQL = "SELECT PilotProfileID From MSTR_Account WHERE AccountID=" + AccountID;
            int PilotProfileID = Int32.Parse(getDBVal(SQL));

            return PilotProfileID;
        }

        public static string WriteToFile(WeatherViewModel Weather, string FileName) {
            string Path = HttpContext.Current.Server.MapPath("/Upload/" + FileName + ".txt");

            // String UploadPath = HttpContext.Current.Server.MapPath(Url.Content(RootUploadDir) + UserID + "/");
            //string SubPath = "~/Weather/";
            //string SubPath = HttpContext.Current.Server.MapPath("~/Weather/");


            //bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(SubPath));
            // if (!exists)
            //  System.IO.Directory.CreateDirectory(SubPath);
            //  if (File.Exists(Path))
            // {
            //   File.Delete(Path);
            //  }


            JavaScriptSerializer jss = new JavaScriptSerializer();

            string output = jss.Serialize(Weather);

            System.IO.File.WriteAllText(@Path, output);
            //  System.IO.File.Create(Path).Close();

            return output;
        }


        public static WeatherViewModel ReadFromFile(string FileName) {
            try {
                string Path = HttpContext.Current.Server.MapPath("/Upload/" + FileName + ".txt");

                StreamReader sr = new StreamReader(Path);
                string jsonString = sr.ReadToEnd();
                JavaScriptSerializer ser = new JavaScriptSerializer();
                WeatherViewModel Weather = ser.Deserialize<WeatherViewModel>(jsonString);

                return Weather;
            } catch {
                return null;
            }

        }

        public static Int32 toInt(String sItem) {
            int Temp;
            if (Int32.TryParse(sItem, out Temp)) {
                return Int32.Parse(sItem);
            } else {
                return 0;
            }
        }



        public static IEnumerable<SelectListItem> GetCountryLists(string TypeField, string NameField, string ValueField, string SPName) {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });

            using (var cotx = new ExponentPortalEntities()) {
                using (var cmd = cotx.Database.Connection.CreateCommand()) {
                    cotx.Database.Connection.Open();
                    cmd.CommandText = "usp_Portal_GetDroneDropDown";
                    DbParameter Param = cmd.CreateParameter();
                    Param.ParameterName = "@Type";
                    Param.Value = TypeField;
                    cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });
                        }
                    }
                    DropDownList = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownList; //return the list objects
                }
            }
        }
        public static IEnumerable<SelectListItem> GetLists(string Type) {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();
                    SQL = "SELECT [Name] as Value  ,[Name] as code FROM[LUP_Drone] where type = '" + Type + "' ";
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["code"].ToString(), Value = reader["Value"].ToString() });
                        }
                        ctx.Database.Connection.Close();
                    }


                }

                return SelectList; //return the list objects


            }
        }
        public static IEnumerable<SelectListItem> GetDashboardLists() {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();
                    SQL = "SELECT [Name] as Value  ,[Name] as code FROM [LUP_Drone] where name in('DpWorld','Default','Internal','Dewa','RPAS','UserDashboard')  order by Code asc";
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            SelectList.Add(new SelectListItem { Text = reader["code"].ToString(), Value = reader["Value"].ToString() });
                        }
                        ctx.Database.Connection.Close();
                    }


                }

                return SelectList; //return the list objects


            }
        }
        public static IEnumerable<SelectListItem> GetDropDowntLists(string TypeField, string NameField, string ValueField, string SPName) {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
            using (var cotx = new ExponentPortalEntities()) {
                using (var cmd = cotx.Database.Connection.CreateCommand()) {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_DroneServiceType";
                    DbParameter Param = cmd.CreateParameter();
                    Param.ParameterName = "@Type";
                    Param.Value = TypeField;
                    cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {

                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Name"].ToString() });

                        }
                    }
                    DropDownList = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownList; //return the list objects

                }
            }
        }


        /*Populating the drop down from Table Mst_Drone using sp usp_Portal_DroneNameList */
        public static IEnumerable<SelectListItem> DroneList(string SPName) {
            //  ctx=new ExponentPortalEntities();

            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
            using (var cotx = new ExponentPortalEntities()) {
                using (var cmd = cotx.Database.Connection.CreateCommand()) {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_DroneNameList";
                    // DbParameter Param = cmd.CreateParameter();


                    //cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {

                            SelectList.Add(new SelectListItem { Text = reader["DroneName"].ToString(), Value = reader["DroneId"].ToString() });

                        }
                    }
                    DropDownLists = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownLists; //return the list objects

                }
            }
        }


        public static IEnumerable<SelectListItem> DronePartsList(string SPName) {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            using (var cotx = new ExponentPortalEntities()) {
                using (var cmd = cotx.Database.Connection.CreateCommand()) {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetDroneParts";
                    // DbParameter Param = cmd.CreateParameter();


                    //cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            string SupplierId = reader["SupplierId"].ToString();


                            SelectList.Add(new SelectListItem { Text = reader["PartsName"].ToString() + "::" + reader["Model"].ToString() + "::" + GetCompanyName(SupplierId), Value = reader["PartsId"].ToString() });

                        }
                    }
                    DropDownLists = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownLists; //return the list objects

                }
            }
        }
        public static int GetAccountIDFromDrone(int DroneId) {
            int result = 0;

            String SQL = "select AccountID from MSTR_Drone where  DroneId=" + DroneId;
            result = Util.toInt(Util.getDBVal(SQL));

            return result;
        }

        public static int GetDroneIdFromFlight(int FlightId) {
            int result = 0;

            String SQL = "select DroneId from DroneFlight where  ID=" + FlightId;
            result = Util.toInt(Util.getDBVal(SQL));

            return result;
        }


    public static bool CheckUserIsPilot(int UserId) {
      bool result = false;

      using (var ctx = new ExponentPortalEntities()) {
        
          var list = ctx.MSTR_User.Where(e => e.IsPilot == true && e.UserId == UserId).FirstOrDefault();
          if (list != null) {
            result = true;
          }//if

          return result;
        }//using
      
    }

    public static string GetUASFromFlight(int FlightId) {
            string result = "";

            String SQL = @"SELECT
                          MSTR_Drone.DroneName as UAS
                      FROM
                        DroneFlight
                      LEFT JOIN MSTR_Drone ON
                        MSTR_Drone.DroneId = DroneFlight.DroneID
                      LEFT JOIN MSTR_User as tblPilot ON
                        tblPilot.UserID = DroneFlight.PilotID
                      LEFT JOIN MSTR_User as tblGSC ON
                        tblGSC.UserID = DroneFlight.GSCID
                      LEFT JOIN MSTR_User as tblCreated ON
                        tblCreated.UserID = DroneFlight.CreatedBy
                        where DroneFlight.ID = " + FlightId;
            result = Util.getDBVal(SQL);



            return result;
        }

        public static string GetPilotFromFlight(int FlightId) {
            string result = "";

            String SQL = @"SELECT
                          tblPilot.FirstName + ' ' + tblPilot.LastName as PilotName
                      FROM
                        DroneFlight
                      LEFT JOIN MSTR_Drone ON
                        MSTR_Drone.DroneId = DroneFlight.DroneID
                      LEFT JOIN MSTR_User as tblPilot ON
                        tblPilot.UserID = DroneFlight.PilotID
                      LEFT JOIN MSTR_User as tblGSC ON
                        tblGSC.UserID = DroneFlight.GSCID
                      LEFT JOIN MSTR_User as tblCreated ON
                        tblCreated.UserID = DroneFlight.CreatedBy
                        where DroneFlight.ID = " + FlightId;
            result = Util.getDBVal(SQL);



            return result;
        }
        public static int GetPilotIdFromFlight(int FlightId) {
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = "select PilotID from DroneFlight where  ID=" + FlightId;
                result = Util.toInt(Util.getDBVal(SQL));

            }

            return result;
        }

        public static int GetLastFlightFromDrone(int DroneID)
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = @"SELECT Top 1
                                          a.[ID] 
                            FROM droneflight a
                            left join MSTR_Drone b
                            on a.DroneID = b.DroneId
                            WHERE a.DroneId = " + DroneID +
                            " ORDER BY a.FlightDate DESC"; 
                result = Util.toInt(Util.getDBVal(SQL));

            }

            return result;
        }

        public static int GetTotalFlightTime(int DronId) {
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = " select Top 1 TotalFlightTime from FlightMapData \n" +
                    "where DroneId =" + DronId + " order by ReadTime desc";
                string Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>().ToString();
                result = Util.toInt(Id);
            }

            return result;
        }
        public static int GetDroneIdFromService(int ServiceId) {
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = "select DroneId from MSTR_DroneService where  ServiceId=" + ServiceId;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }


        public static String GetEncryptedPassword(String Password) {
            if (String.IsNullOrEmpty(Password)) Password = "";
            String EncrptedPassword;
            String SQL = "SELECT CONVERT(NVARCHAR(32), HASHBYTES('MD5', '" + Password.ToLower() + "'),2)";
            using (var cotx = new ExponentPortalEntities()) {

                // String SQL = "select Max(TypeId)+1 from LUP_Drone where  Type='" + TypeName + "'";
                EncrptedPassword = cotx.Database.SqlQuery<String>(SQL).FirstOrDefault<String>();

            }

            return EncrptedPassword; ;
        }
        public static int GetTypeId(String TypeName) {
            int result = 0;

            String SQL = "select Max(TypeId)+1 from LUP_Drone where  Type='" + TypeName + "'";
            int Id = Util.getDBInt(SQL);
            if (Id < 1) Id = 1;
            result = Id;

            return result;
        }

        public static int GetTypeIdFromId(int ID) {
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = "select TypeId from LUP_Drone where  Id=" + ID;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }
        public static int GetTypeOfIdFromService(int ServiceId) {
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = "select TypeOfServiceId from MSTR_DroneService where  ServiceId=" + ServiceId;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }

        public static bool IsGcaApproved(int ApprovalID)
        {
            bool result = false;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select count(*) as Count from GCA_Approval where approvalID='" + ApprovalID + "'  and (ApprovalStatus='Approved' or ApprovalStatus='Reject' )";
                int Count = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                if (Count > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }


            }

            return result;

        }
        public static int GetUserIdCertificate(int CertID, string TableName) {
            
            int result = 0;
            using (var cotx = new ExponentPortalEntities()) {
                String SQL = "select UserId from " + TableName + " where Id='" + CertID + "'";
                int UserId = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = UserId;
            }

            return result;
        }



        public static int ExportFlightDataCSV(HttpResponseBase Response,int FlightID)
        {
            int Result = 1;
            string Path = HttpContext.Current.Server.MapPath("/Upload/FightData.csv");
            //    String SQL =
            //  "SELECT \n" +
            //  "  FlightMapDataID,\n" +
            //  "  ReadTime,\n" +
            //  "  Latitude,\n" +
            //  "  Longitude,\n" +
            //  "  Altitude,\n" +
            //  "  Speed,\n" +
            //  "  FixQuality,\n" +
            //  "  Satellites,\n" +
            //  "  Pitch,\n" +
            //  "  Roll,\n" +
            //  "  Heading,\n" +
            //  "  TotalFlightTime,\n" +
            //  "  Count(*) OVER() as _TotalRecords\n" +
            //  "FROM\n" +
            //  "  FlightMapData\n" +
            //  "WHERE\n" +
            //  "  FlightID=" + "761";




            using (var ctx = new ExponentPortalEntities())
            {
                var FlightData = (
                 from o in ctx.FlightMapDatas
                 where
                o.FlightID == FlightID
                 select new
                 {
                     FlightMapDataID = o.FlightMapDataID,
                     ReadTime = o.ReadTime,
                     Latitude = o.Latitude,
                     Longitude = o.Longitude,
                     Altitude = o.Altitude,
                     Speed = o.Speed,
                     FixQuality = o.FixQuality,
                     Satellites = o.Satellites,
                     Pitch = o.Pitch,
                     Roll = o.Roll,
                     Heading = o.Heading                    
                 }).ToList();

               



                string attachment = "attachment; filename=FlightData.csv";
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                HttpContext.Current.Response.ContentType = "text/csv";
                HttpContext.Current.Response.AddHeader("Pragma", "public");
                WriteColumnName();
                foreach (var FlighInfo  in FlightData)
                {

                    StringBuilder stringBuilder = new StringBuilder();
                   
                    AddComma(FlighInfo.ReadTime.ToString(), stringBuilder);
                    AddComma(FlighInfo.Latitude.ToString(), stringBuilder);
                    AddComma(FlighInfo.Longitude.ToString(),stringBuilder);
                    AddComma(FlighInfo.Altitude.ToString(), stringBuilder);
                    AddComma(FlighInfo.Speed.ToString(), stringBuilder);
                    AddComma(FlighInfo.FixQuality.ToString(), stringBuilder);
                    AddComma(FlighInfo.Satellites.ToString(), stringBuilder);
                    AddComma(FlighInfo.Pitch.ToString(), stringBuilder);
                    AddComma(FlighInfo.Roll.ToString(), stringBuilder);
                    AddComma(FlighInfo.Heading.ToString(), stringBuilder);
                  
                    HttpContext.Current.Response.Write(stringBuilder.ToString());
                    HttpContext.Current.Response.Write(Environment.NewLine);
                    
                }
                HttpContext.Current.Response.End();
            }

            return Result;

        }
        private static void WriteUserInfo(FlightMapData FlightInfo)
        {
            
        }

        private static void AddComma(string value, StringBuilder stringBuilder)
        {
            stringBuilder.Append(value.Replace(',', ' '));
            stringBuilder.Append(", ");
        }
        private static void WriteColumnName()
        {
            string columnNames = " ReadTime,Latitude, Longitude,Altitude,Speed,FixQuality,Satellites,Pitch,Roll,Heading";
         
            HttpContext.Current.Response.Write(columnNames);
            HttpContext.Current.Response.Write(Environment.NewLine);
        }


        public static  bool IsAssignToDrone(Nullable<int> DroneID, Nullable<int> BlackBoxID)
        {
            bool Result = false;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select count(*) as Count from mstr_drone where DroneID='" + DroneID + "' and BlackBoxID='" + BlackBoxID + "'";
                int Count = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                if (Count > 0)
                {
                    Result = true;
                }
            }

            return Result;
        }


















        public static int GetServiceId() {
      int result = 0;
      using (var cotx = new ExponentPortalEntities()) {
        String SQL = "select MAX(ServiceId)as ServiceId from MSTR_DroneService";
        int ServiceId = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
        result = ServiceId;
      }

      return result;
    }
        

        public static string FirstLetterToUpper(string str)
        {
       
            if (str==null)
            {
                return null;
            } 
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
      
    }
        public static string GetCompanyName(string AccId) {
      string result;
      using (var cotx = new ExponentPortalEntities()) {
        String SQL = "select name from MSTR_Account where  AccountId=" + AccId;
        string ServiceId = cotx.Database.SqlQuery<string>(SQL).FirstOrDefault<string>();
        result = ServiceId;
      }

      return result;
    }

  }
}


