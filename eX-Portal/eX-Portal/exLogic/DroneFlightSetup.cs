using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.exLogic
{
    public class DroneFlightSetup
    {
        static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();
        static IEnumerable<SelectListItem> DDoptions = Enumerable.Empty<SelectListItem>();
        
        public static IEnumerable<SelectListItem> GetDdListDrone(int accountid)
        {
            //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {

                    ctx.Database.Connection.Open();

                    SqlParameter parameter = new SqlParameter("@accountid", SqlDbType.Int);
                    parameter.Value = accountid;
                    cmd.Parameters.Add(parameter);

                    cmd.CommandText = "Select [DroneName],[DroneId] from [MSTR_Drone] where [AccountID]=" + accountid;
                    cmd.CommandType = CommandType.Text;
                   
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            SelectList.Add(new SelectListItem { Text = reader["DroneName"].ToString(), Value = reader["DroneId"].ToString() });

                        }
                    }
                    DropDownList = SelectList.ToList();
                    ctx.Database.Connection.Close();
                    return DropDownList; //return the list objects

                }
            }
        }

        public static IEnumerable<SelectListItem> GetOptions()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
            //SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
            SelectList.Add(new SelectListItem { Text = "Good", Value = "Good" });
            SelectList.Add(new SelectListItem { Text = "Bad", Value = "Bad" });
            SelectList.Add(new SelectListItem { Text = "Ok", Value = "Ok" });

            //SelectList.Add(new SelectListItem { Text = "Good", Value = "Good" });        
            //SelectList.Add(new SelectListItem { Text = "Bad", Value = "Bad" });
            //SelectList.Add(new SelectListItem { Text = "Ok", Value = "Ok" });

            DDoptions = SelectList.ToList();
            return DDoptions; //return the list objects
        }

    }
}