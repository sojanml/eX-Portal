using System;
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

namespace eX_Portal.exLogic {


  public partial class Util {
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

    public static IList<ChartViewModel> getCurrentPilotChartData() {

      IList<ChartViewModel> ChartList = new List<ChartViewModel>();

      using (var ctx = new ExponentPortalEntities()) {
                using (var cmd = ctx.Database.Connection.CreateCommand()) {
                    ctx.Database.Connection.Open();
                    string SQL;
                    SQL = @"     select a.PilotId,c.FirstName,max(d.LastHours) as Plast,
                      sum(a.FixedWing) + sum(a.multiDashRotor) as Ptotal
                      , CASE WHEN
                           MIN(b.PCurrentMonth) IS NULL then 0
                         else min(b.PCurrentMonth) end as pCurrent
                        from MSTR_User c 
                              left  join MSTR_Pilot_Log a on a.PilotId = c.UserId
                              left  join(SELECT PilotId, LastHours
                                          FROM
                                          (select PilotId,
                                               fixedwing + multidashrotor as LastHours,                                 
                                                ROW_NUMBER() OVER(PARTITION BY PilotId
                                                 ORDER BY date DESC
                                            ) AS RN
                                          from MSTR_Pilot_Log

                                          )Sub   WHERE rn = 1   )d on a.PilotId = d.PilotId
                           left join(select PilotId, sum(FixedWing) + sum(multiDashRotor) as PCurrentMonth
                                    from MSTR_Pilot_Log
                                     where
                                      convert(nvarchar(30), date, 120)
                                      BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                                      AND GETDATE()
                                      group by  PilotId )  b on a.PilotId = b.PilotId

                              group by a.PilotId ,c.FirstName
                           having   sum(a.FixedWing) + sum(a.multiDashRotor) > 0";

                    if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                                  {
                        SQL = @"     select a.PilotId,c.FirstName,max(d.LastHours) as Plast,
                      sum(a.FixedWing) + sum(a.multiDashRotor) as Ptotal
                      , CASE WHEN
                           MIN(b.PCurrentMonth) IS NULL then 0
                         else min(b.PCurrentMonth) end as pCurrent
                        from MSTR_User c 
                              left  join MSTR_Pilot_Log a on a.PilotId = c.UserId
                              left  join(SELECT PilotId, LastHours
                                          FROM
                                          (select PilotId,
                                               fixedwing + multidashrotor as LastHours,                                 
                                                ROW_NUMBER() OVER(PARTITION BY PilotId
                                                 ORDER BY date DESC
                                            ) AS RN
                                          from MSTR_Pilot_Log

                                          )Sub   WHERE rn = 1   )d on a.PilotId = d.PilotId
                           left join(select PilotId, sum(FixedWing) + sum(multiDashRotor) as PCurrentMonth
                                    from MSTR_Pilot_Log
                                     where
                                      convert(nvarchar(30), date, 120)
                                      BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                                      AND GETDATE()
                                      group by  PilotId )  b on a.PilotId = b.PilotId "+
                               " where c.accountId='" + Util.getAccountID() + "'  "                          
                             + "  group by a.PilotId ,c.FirstName "
                             +  "  having   sum(a.FixedWing) + sum(a.multiDashRotor) > 0";

                    }

                        //SQL = @" select a.PilotId,c.FirstName,sum(a.FixedWing) + sum(a.multiDashRotor) as Ptotal
                        //               , CASE WHEN 
                        //                 MIN(b.PCurrentMonth) IS NULL then 0
                        //                 else min(b.PCurrentMonth) end as pCurrent
                        //                 from MSTR_User c left
                        //                 join MSTR_Pilot_Log a on a.PilotId = c.UserId
                        //                 left join(select PilotId,sum(FixedWing) + sum(multiDashRotor) as PCurrentMonth
                        //                 from MSTR_Pilot_Log
                        //                 where
                        //                 convert(nvarchar(30), date, 120)
                        //                  BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) 
                        //                  AND GETDATE()
                        //                  group by  PilotId )
                        //                  b on a.PilotId = b.PilotId  group by a.PilotId ,c.FirstName
                        //                 having   sum(a.FixedWing) + sum(a.multiDashRotor) > 0";
                        //          if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                        //          {
                        //              SQL = @" select a.PilotId,c.FirstName,sum(a.FixedWing) + sum(a.multiDashRotor) as Ptotal
                        //               , CASE WHEN 
                        //                 MIN(b.PCurrentMonth) IS NULL then 0
                        //                 else min(b.PCurrentMonth) end as pCurrent
                        //                 from MSTR_User c left
                        //                 join MSTR_Pilot_Log a on a.PilotId = c.UserId
                        //                 left join(select PilotId,sum(FixedWing) + sum(multiDashRotor) as PCurrentMonth
                        //                 from MSTR_Pilot_Log
                        //                 where
                        //                 convert(nvarchar(30), date, 120)
                        //                  BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) 
                        //                  AND GETDATE()
                        //                  group by  PilotId )
                        //                  b on a.PilotId = b.PilotId  where c.accountId='"+ Util.getAccountID() + "' group by a.PilotId ,c.FirstName " +
                        //               "  having   sum(a.FixedWing) + sum(a.multiDashRotor) > 0";
                        //          }


                        cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              ChartViewModel dd = new ChartViewModel();
              dd.PilotName = reader["FirstName"].ToString();
              dd.PilotTotalHrs = Util.toInt(reader["Ptotal"].ToString());
              dd.PilotCurrentMonthHrs = Util.toInt(reader["PCurrent"].ToString());
              dd.PilotLastFlightHrs = Util.toInt(reader["Plast"].ToString());
                         
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
          string SQL;
                    SQL = @"select MAX(d.flightHours) as LastFlightHours,                            
                            d.DroneId,c.DroneName
                            from MSTR_Drone c
                            join
                           (SELECT DroneId, flighthours
                              FROM
                               (select a.DroneId,
                                 b.flighthours as flighthours,
                                 a.ReadTime,
                                 ROW_NUMBER() OVER(PARTITION BY a.DroneId
                                ORDER BY b.flightdate DESC
                             ) AS RN
                          from flightmapdata a
                         left
                          join
                         droneflight b on a.DroneId = b.DroneID

                         )Sub
                        WHERE rn = 1) d
                            on c.DroneId = d.DroneId
                            group by
                            d.DroneId,c.DroneName";


                    if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                        {
                        SQL = @"select MAX(d.flightHours) as LastFlightHours,                            
                            d.DroneId,c.DroneName
                            from MSTR_Drone c
                            join
                           (SELECT DroneId, flighthours
                              FROM
                               (select a.DroneId,
                                 b.flighthours as flighthours,
                                 a.ReadTime,
                                 ROW_NUMBER() OVER(PARTITION BY a.DroneId
                                ORDER BY b.flightdate DESC
                             ) AS RN
                          from flightmapdata a
                         left
                          join
                         droneflight b on a.DroneId = b.DroneID

                         )Sub
                        WHERE rn = 1) d
                            on c.DroneId = d.DroneId  where c.AccountID='" + Util.getAccountID() + "'" +
                           " group by "+
                          "  d.DroneId,c.DroneName";
                    }

                        //SQL = @"select MAX(b.ReadTime) as ReadTime,
                        //                  max(b.TotalFlightTime)as TotalFlightTime,
                        //                  b.DroneId,a.DroneName 
                        //                  from MSTR_Drone a
                        //                  join 
                        //                  FlightMapData b 
                        //                  on a.DroneId=b.DroneId 
                        //                  group by 
                        //                  b.DroneId,a.DroneName";
                        //          if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                        //          {
                        //              SQL = @"select MAX(b.ReadTime) as ReadTime,
                        //                  max(b.TotalFlightTime) as TotalFlightTime,
                        //                  b.DroneId,a.DroneName
                        //                  from MSTR_Drone a
                        //                  join
                        //                  FlightMapData b
                        //                  on a.DroneId = b.DroneId
                        //                   where a.AccountID='" + Util.getAccountID() +
                        //                 "' group by " +
                        //                 " b.DroneId,a.DroneName  ";
                        //          }

                        cmd.CommandText = SQL;
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
          string SQL;

          SQL= @"select     t.DroneId,MAX(j.flighthours) as LastFlightTime,
                            v.DroneName,max(t.ReadTime) as readtime ,
                            max(T.TotalFlightTime) as TotalFlightTime,
                            min(k.FlightTime) as Monthtime, 
                            CASE WHEN  max(T.TotalFlightTime) - min(k.FlightTime)IS NULL or
                            max(T.TotalFlightTime) - min(k.FlightTime) = 0
                            THEN max(T.TotalFlightTime) ELSE max(T.TotalFlightTime) - min(k.FlightTime) END as CurrentFlightTime
                 from MSTR_Drone v
                 join
                          FlightMapData t on v.DroneId = t.DroneId
                 left  join  
                            (SELECT DroneId, flighthours
                              FROM 
                               (select a.DroneId,
                                 b.flighthours as flighthours,
                                 a.ReadTime,
                                 ROW_NUMBER() OVER(PARTITION BY a.DroneId
                                ORDER BY b.flightdate DESC
                             ) AS RN
                          from flightmapdata a
                         left  join 
                              droneflight b on a.DroneId = b.DroneID

                         )Sub
                        WHERE rn = 1 )j on t.DroneId = j.DroneId
                        left join(select u.DroneId, min(u.ReadTime) as ReadTime,
                        max(u.TotalFlightTime) as FlightTime
                        from FlightMapData u
                           where
                            u.ReadTime
                            BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                            AND  GETDATE()
                            group by  u.DroneId )k on t.DroneId = k.DroneId

                  group by  t.DroneId,v.DroneName";


                    if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                    {
                        SQL = @"select     t.DroneId,MAX(j.flighthours) as LastFlightTime,
                            v.DroneName,max(t.ReadTime) as readtime ,
                            max(T.TotalFlightTime) as TotalFlightTime,
                            min(k.FlightTime) as Monthtime, 
                            CASE WHEN  max(T.TotalFlightTime) - min(k.FlightTime)IS NULL or
                            max(T.TotalFlightTime) - min(k.FlightTime) = 0
                            THEN max(T.TotalFlightTime) ELSE max(T.TotalFlightTime) - min(k.FlightTime) END as CurrentFlightTime
                 from MSTR_Drone v
                 join
                          FlightMapData t on v.DroneId = t.DroneId
                 left  join  
                            (SELECT DroneId, flighthours
                              FROM 
                               (select a.DroneId,
                                 b.flighthours as flighthours,
                                 a.ReadTime,
                                 ROW_NUMBER() OVER(PARTITION BY a.DroneId
                                ORDER BY b.flightdate DESC
                             ) AS RN
                          from flightmapdata a
                         left  join 
                              droneflight b on a.DroneId = b.DroneID

                         )Sub
                        WHERE rn = 1 )j on t.DroneId = j.DroneId
                        left join(select u.DroneId, min(u.ReadTime) as ReadTime,
                        max(u.TotalFlightTime) as FlightTime
                        from FlightMapData u
                           where
                            u.ReadTime
                            BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                            AND  GETDATE()
                            group by  u.DroneId )k on t.DroneId = k.DroneId" +
                           "  where v.AccountID='" + Util.getAccountID() + "'"+
                         " group by  t.DroneId,v.DroneName";

                    }

                        //SQL = @"select t.DroneId,
                        //                 v.DroneName,max(t.ReadTime) as readtime ,
                        //                  max(T.TotalFlightTime) as TotalFlightTime,
                        //                  min(k.FlightTime)as Monthtime,
                        //                  CASE WHEN  max(T.TotalFlightTime) - min(k.FlightTime)IS NULL or 
                        //                  max(T.TotalFlightTime) - min(k.FlightTime) = 0 
                        //                  THEN max(T.TotalFlightTime) ELSE max(T.TotalFlightTime) - min(k.FlightTime) END as CurrentFlightTime
                        //                  from MSTR_Drone v
                        //                  join FlightMapData t on v.DroneId = t.DroneId 
                        //                  left join(select u.DroneId, min(u.ReadTime) as ReadTime,
                        //                  max(u.TotalFlightTime) as FlightTime
                        //                  from FlightMapData u
                        //                  where
                        //                   u.ReadTime
                        //                  BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) 
                        //                  AND  GETDATE()
                        //                  group by  u.DroneId )k on t.DroneId = k.DroneId
                        //                  group by t.DroneId,v.DroneName";
                        //          if (!exLogic.User.hasAccess("DRONE.MANAGE"))
                        //          {
                        //              SQL = @"select t.DroneId,
                        //                 v.DroneName,max(t.ReadTime) as readtime ,
                        //                  max(T.TotalFlightTime) as TotalFlightTime,
                        //                  min(k.FlightTime)as Monthtime,
                        //                  CASE WHEN  max(T.TotalFlightTime) - min(k.FlightTime)IS NULL or 
                        //                  max(T.TotalFlightTime) - min(k.FlightTime) = 0 
                        //                  THEN max(T.TotalFlightTime) ELSE max(T.TotalFlightTime) - min(k.FlightTime) END as CurrentFlightTime
                        //                  from MSTR_Drone v
                        //                  join FlightMapData t on v.DroneId = t.DroneId 
                        //                  left join(select u.DroneId, min(u.ReadTime) as ReadTime,
                        //                  max(u.TotalFlightTime) as FlightTime
                        //                  from FlightMapData u
                        //                  where    u.ReadTime
                        //                  BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)  
                        //                  AND  GETDATE() 
                        //                  group by  u.DroneId )k on t.DroneId = k.DroneId 
                        //                  where v.AccountID='"+ Util.getAccountID() +"' group by t.DroneId,v.DroneName";

                        //          }


                        cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              ChartViewModel dd = new ChartViewModel();
              dd.DroneName = reader["DroneName"].ToString();
              dd.TotalFightTime = Util.toInt(reader["TotalFlightTime"].ToString());
              dd.CurrentFlightTime = Util.toInt(reader["CurrentFlightTime"].ToString());
              dd.LastFlightTime = Util.toInt(reader["LastFlightTime"].ToString());
              ChartList.Add(dd);

            }
          }

          ctx.Database.Connection.Close();


        }


      }



      return ChartList;
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
      HttpSQL = " http://query.yahooapis.com/v1/public/yql?q=select woeid from geo.places where text='" + City +"'";
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
        request = (HttpWebRequest)WebRequest.Create("http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&sensor=false");
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
      string forecastUrl = "https://query.yahooapis.com/v1/public/yql?q=select * from weather.forecast where woeid=" + Location + " AND u='c'";
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
            Weather.TemperatureUnit = "°C";
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
          Weather.Visibility = reader.GetAttribute("visibility").ToString();
          Weather.Pressure = reader.GetAttribute("pressure").ToString();
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
          if (count <= 5) {
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
            if (ForcastList.Count>0)
            {
                WriteToFile(Weather,Location);
            }
            else
            {
                //not exist read from the file and display that
                WeatherFromFile = ReadFromFile(Location);
                if(WeatherFromFile!= null)
                {
                    Weather = WeatherFromFile;
                }
            }
            
           
      return Weather;

    }

        

        public static string WriteToFile(WeatherViewModel  Weather,string FileName)
        {
            string Path = HttpContext.Current.Server.MapPath("/Upload/" + FileName+".txt");
          
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


        public static WeatherViewModel ReadFromFile(string FileName)
        {
            try {
                string Path = HttpContext.Current.Server.MapPath("/Upload/" + FileName + ".txt");

                StreamReader sr = new StreamReader(Path);
                string jsonString = sr.ReadToEnd();
                JavaScriptSerializer ser = new JavaScriptSerializer();
                WeatherViewModel Weather = ser.Deserialize<WeatherViewModel>(jsonString);

                return Weather;
            }
            catch  {
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
    public static IEnumerable<SelectListItem> GetDashboardLists() {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
      String SQL = "SELECT 0 as Value, 'Not Available' as Name";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          SQL = "SELECT [Name] as Value  ,[Name] as code FROM [LUP_Drone] where name in('DpWorld','Default','Internal','Dewa') ";
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
      using (var cotx = new ExponentPortalEntities()) {
        String SQL = "select AccountID from MSTR_Drone where  DroneId=" + DroneId;
        result = Util.toInt(Util.getDBVal(SQL));

      }

      return result;
    }

    public static int GetDroneIdFromFlight(int FlightId) {
      int result = 0;
      using (var cotx = new ExponentPortalEntities()) {
        String SQL = "select DroneId from DroneFlight where  ID=" + FlightId;
        result = Util.toInt(Util.getDBVal(SQL));

      }

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

    public static int GetUserIdCertificate(int CertID, string TableName) {
      int result = 0;
      using (var cotx = new ExponentPortalEntities()) {
        String SQL = "select UserId from " + TableName + " where Id='" + CertID + "'";
        int UserId = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
        result = UserId;
      }

      return result;
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


