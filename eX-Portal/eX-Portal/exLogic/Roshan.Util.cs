using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.exLogic
{
    public partial class Util
    {
        public static IEnumerable<SelectListItem> GetRAPSDroneList()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
            String SQL = "SELECT 0 as Value, 'Not Available' as Name";
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();

                    SQL = "SELECT [DroneId] as Value, [DroneName] as Name FROM [MSTR_Drone] where IsActive=1";
                        SQL += "\n" +
                          " AND\n " +
                          "  MSTR_Drone.CreatedBy =" + Util.getLoginUserID();

                    SQL += "\n ORDER BY [DroneName]";
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SelectList.Add(new SelectListItem
                            {
                                Text = reader["Name"].ToString(),
                                Value = reader["Value"].ToString()
                            });
                        }//while
                    }//using

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return SelectList; //return the list objects
        }//function GetDropDowntList

        public static IEnumerable<SelectListItem> GetBoolList()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
            SelectList.Add(new SelectListItem { Text = "True", Value = "1" });
            SelectList.Add(new SelectListItem { Text = "false", Value = "0" });
            return SelectList; //return the list objects
        }//function GetDropDowntList


    }
}