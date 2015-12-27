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
namespace eX_Portal.exLogic
{
    public partial class Util
    {

        static IEnumerable<SelectListItem> DropDownLists = Enumerable.Empty<SelectListItem>();
        //private static ExponentPortalEntities cotx;


        public static IEnumerable<SelectListItem>LUPTypeList()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
           

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();
                    SQL=  "SELECT MIN(x.id)as value,x.type  as name FROM LUP_Drone x  JOIN(SELECT p.type " +
                        "FROM LUP_Drone p  GROUP BY p.type) y ON y.type = x.type GROUP BY x.type";

                           
                           
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDownt

     public   static string DecToBin(int dec)
        {
            if (dec < 0) return ""; // can't be negative
            List<char> list = new List<char>();

            do
            {
                int div = dec / 2;
                int rem = dec % 2;
               
                dec = div;
                list.Add((char)(rem + 48));
            }
            while (dec > 0);

            list.Reverse();
            return new string(list.ToArray());
        }


        public static IEnumerable<SelectListItem> GetAccountList()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();

                    SQL = "SELECT [AccountID] as Value  ,[Name] as Name FROM [MSTR_Account] ORDER BY [Name]";


                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntL
        public static IEnumerable<SelectListItem> GetProfileList()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();
                   
                            SQL = "SELECT [ProfileId] as Value  ,[ProfileName] as Name FROM [MSTR_Profile] ORDER BY [ProfileName]";
                          
                 
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Value"].ToString() });
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntList


        public static Int32 toInt(String sItem)
        {
            int Temp;
            if (Int32.TryParse(sItem, out Temp))
            {
                return Int32.Parse(sItem);
            }
            else
            {
                return 0;
            }
        }



        public static IEnumerable<SelectListItem> GetCountryLists(string TypeField, string NameField, string ValueField, string SPName)
        {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });


            using (var cotx = new ExponentPortalEntities())
            {
                using (var cmd = cotx.Database.Connection.CreateCommand())
                {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetDroneDropDown";
                    DbParameter Param = cmd.CreateParameter();
                    Param.ParameterName = "@Type";
                    Param.Value = TypeField;
                    cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });

                        }
                    }
                    DropDownList = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownList; //return the list objects

                }
            }
        }
        public static IEnumerable<SelectListItem> GetDropDowntLists(string TypeField, string NameField, string ValueField, string SPName)
        {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
            using (var cotx = new ExponentPortalEntities())
            {
                using (var cmd = cotx.Database.Connection.CreateCommand())
                {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_DroneServiceType";
                    DbParameter Param = cmd.CreateParameter();
                    Param.ParameterName = "@Type";
                    Param.Value = TypeField;
                    cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                                SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });
                           
                        }
                    }
                    DropDownList = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownList; //return the list objects

                }
            }
        }


        /*Populating the drop down from Table Mst_Drone using sp usp_Portal_DroneNameList */
        public static IEnumerable<SelectListItem> DroneList(string SPName)
        {
            //  ctx=new ExponentPortalEntities();

            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
            using (var cotx = new ExponentPortalEntities())
            {
                using (var cmd = cotx.Database.Connection.CreateCommand())
                {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_DroneNameList";
                    // DbParameter Param = cmd.CreateParameter();


                    //cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                                SelectList.Add(new SelectListItem { Text = reader["DroneName"].ToString(), Value = reader["DroneId"].ToString() });
                           
                        }
                    }
                    DropDownLists = SelectList.ToList();
                    cotx.Database.Connection.Close();
                    return DropDownLists; //return the list objects

                }
            }
        }


        public static IEnumerable<SelectListItem> DronePartsList(string SPName)
        {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            using (var cotx = new ExponentPortalEntities())
            {
                using (var cmd = cotx.Database.Connection.CreateCommand())
                {

                    cotx.Database.Connection.Open();


                    cmd.CommandText = "usp_Portal_GetDroneParts";
                    // DbParameter Param = cmd.CreateParameter();


                    //cmd.Parameters.Add(Param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
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



        public static int GetTotalFlightTime(int DronId)
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL =" select Top 1 TotalFlightTime from FlightMapData \n" +
                    "where DroneId ="+ DronId +   " order by ReadTime desc";
             string Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>().ToString();
                result =Util.toInt( Id);
            }

            return result;
        }
        public static int GetDroneIdFromService(int ServiceId)
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL =  "select DroneId from MSTR_DroneService where  ServiceId=" + ServiceId;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }


        public static String GetEncryptedPassword(String Password)
        {
            
           String EncrptedPassword;
            String SQL = "SELECT CONVERT(NVARCHAR(32), HASHBYTES('MD5', '" + Password + "'),2)";
            using (var cotx = new ExponentPortalEntities())
            {

                // String SQL = "select Max(TypeId)+1 from LUP_Drone where  Type='" + TypeName + "'";
                EncrptedPassword = cotx.Database.SqlQuery<String>(SQL).FirstOrDefault<String>();
                
            }

            return EncrptedPassword; ;
        }
        public static int GetTypeId(String TypeName)
        {
            int result = 0;

              String SQL = "select Max(TypeId)+1 from LUP_Drone where  Type='" + TypeName +"'";
              int Id = Util.getDBInt(SQL);
            if (Id < 1) Id = 1;
              result = Id;

            return result;
        }

        public static int GetTypeIdFromId(int ID)
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select TypeId from LUP_Drone where  Id=" + ID ;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }
        public static int GetTypeOfIdFromService(int ServiceId)
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select TypeOfServiceId from MSTR_DroneService where  ServiceId=" + ServiceId;
                int Id = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = Id;
            }

            return result;
        }



        public static int GetServiceId()
        {
            int result = 0;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select MAX(ServiceId)as ServiceId from MSTR_DroneService";
                int ServiceId = cotx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = ServiceId;
            }

            return result;
        }

        public static string GetCompanyName(string AccId)
        {
            string result;
            using (var cotx = new ExponentPortalEntities())
            {
                String SQL = "select name from MSTR_Account where  AccountId=" + AccId;
                string ServiceId = cotx.Database.SqlQuery<string>(SQL).FirstOrDefault<string>();
                result = ServiceId;
            }

            return result;
        }

    }
}


