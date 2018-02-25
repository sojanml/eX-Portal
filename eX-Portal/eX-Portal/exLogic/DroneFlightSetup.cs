using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using eX_Portal.Models;
using System.Data.Common;

namespace eX_Portal.exLogic {
  public class DroneFlightSetup {
    static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();
    static IEnumerable<SelectListItem> DDoptions = Enumerable.Empty<SelectListItem>();
    static ExponentPortalEntities ctx = new ExponentPortalEntities();

    public static String GetSMSNumber(int UserID) {
      var Query = from u in ctx.MSTR_User
                  where u.UserId == UserID
                  select u.MobileNo;
      String Mobile = Query.FirstOrDefault().ToString();
      return Mobile;

    }

    public static IEnumerable<SelectListItem> GetDdListDroneForUser(int UserID) {
      var Query = from d in ctx.MSTR_Drone
                  join u in ctx.M2M_Drone_User.Where(t => t.UserID == UserID) on
                    d.DroneId equals u.DroneID
                  orderby d.DroneName
                  select new SelectListItem {
                    Text = d.DroneName,
                    Value = d.DroneId.ToString()
                  };
      return Query.ToList();

    }

    public static IEnumerable<SelectListItem> GetDdListDrone(int accountid) {

      var Query = from d in ctx.MSTR_Drone
                  where d.AccountID == accountid
                  orderby d.DroneName
                  select new SelectListItem {
                    Text = d.DroneName,
                    Value = d.DroneId.ToString()
                  };
      return Query.ToList();

    }

    public static IEnumerable<SelectListItem> GetFlightType(int AccountID) {
      var ctx = new ExponentPortalEntities();
      var query = from t in ctx.LUP_Drone
                  where
                    t.Type == "FlightType" &&
                    t.Name != "Other"
                  orderby
                    t.Name
                  select new SelectListItem {
                    Text = t.Name,
                    Value = t.TypeId.ToString()
                  };

      return query.ToList();
    }

    public static IEnumerable<SelectListItem> getAllUsers(Object accountid) {

      int iAccountID = 0;
      int.TryParse(accountid.ToString(), out iAccountID);

      var ctx = new ExponentPortalEntities();
      List<SelectListItem> SelectList = (
        from m in ctx.MSTR_User
        where m.AccountId == iAccountID
        orderby m.FirstName ascending
        select new SelectListItem {
          Text = m.FirstName + " " + m.LastName,
          Value = m.UserId.ToString()
        }
      ).ToList();

      return SelectList;
    }


        public static IEnumerable<SelectListItem> getPilotAllUsers(Object accountid)
        {

            int iAccountID = 0;
            int.TryParse(accountid.ToString(), out iAccountID);
            List<SelectListItem> SelectList = new List<SelectListItem>();
            var ctx = new ExponentPortalEntities();
            if (Convert.ToInt32(accountid)==1)
            {              
                 SelectList = (
                  from m in ctx.MSTR_User
                  where m.IsPilot == true
                  orderby m.FirstName ascending
                  select new SelectListItem
                  {
                      Text = m.FirstName + " " + m.LastName,
                      Value = m.UserId.ToString()
                  }
                ).ToList();
            }
            else
            {
                SelectList = (
                from m in ctx.MSTR_User
                where m.AccountId == iAccountID && m.IsPilot == true
                orderby m.FirstName ascending
                select new SelectListItem
                {
                    Text = m.FirstName + " " + m.LastName,
                    Value = m.UserId.ToString()
                }
                ).ToList();
            }                     
            return SelectList;
        }

        public static IEnumerable<SelectListItem> GetOptions() {
      List<SelectListItem> SelectList = new List<SelectListItem>();

      SelectList.Add(new SelectListItem { Text = "Good", Value = "Good" });
      SelectList.Add(new SelectListItem { Text = "Bad", Value = "Bad" });
      SelectList.Add(new SelectListItem { Text = "Ok", Value = "Ok" });

      DDoptions = SelectList.ToList();
      return DDoptions; //return the list objects
    }



    public static IEnumerable<SelectListItem> GetDdListNationality(string TypeField, string NameField, string ValueField, string SPName) {
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
  }
}