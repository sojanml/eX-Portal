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



    public static IEnumerable<SelectListItem> getAllUsers(Object accountid) {
      
      int iAccountID = 0;
      int.TryParse(accountid.ToString(), out iAccountID);

      var ctx=new ExponentPortalEntities();
      List<SelectListItem> SelectList = (
        from m in ctx.MSTR_User
        where m.AccountId == iAccountID
        select new SelectListItem {
          Text = m.FirstName + " " + m.LastName,
          Value = m.UserId.ToString()
        }
      ).ToList();

      return SelectList;
      }
    

    public static IEnumerable<SelectListItem> GetOptions()
        {
            List<SelectListItem> SelectList = new List<SelectListItem>();
        
            SelectList.Add(new SelectListItem { Text = "Good", Value = "Good" });
            SelectList.Add(new SelectListItem { Text = "Bad", Value = "Bad" });
            SelectList.Add(new SelectListItem { Text = "Ok", Value = "Ok" });        
          
            DDoptions = SelectList.ToList();
            return DDoptions; //return the list objects
        }

    }
}