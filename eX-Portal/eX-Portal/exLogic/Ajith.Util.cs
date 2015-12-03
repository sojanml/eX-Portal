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
        private static ExponentPortalEntities cotx;

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

        public static IEnumerable<SelectListItem> GetDropDowntLists(string TypeField, string NameField, string ValueField, string SPName)
        {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();

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
                    return DropDownLists; //return the list objects

                }
            }
        }


        /*Populating the drop down from Table Mst_Drone using sp usp_Portal_DroneNameList */
        public static IEnumerable<SelectListItem> DroneList(string SPName)
        {
            //  ctx=new ExponentPortalEntities();

            List<SelectListItem> SelectList = new List<SelectListItem>();
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

