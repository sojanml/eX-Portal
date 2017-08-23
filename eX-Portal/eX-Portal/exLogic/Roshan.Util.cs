using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.exLogic {
  public partial class Util {
    public static IEnumerable<SelectListItem> GetBlackBox() {
      List<SelectListItem> SelectList = new List<SelectListItem>();

      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {

          ctx.Database.Connection.Open();

          cmd.CommandText = "SELECT BlackBoxID,BlackBoxName,BlackBoxSerial from MSTR_BlackBox where CurrentStatus='OUT' and IsActive = 1";
          cmd.CommandType = CommandType.Text;


          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              SelectList.Add(new SelectListItem { Text = reader["BlackBoxSerial"].ToString() + "-" + reader["BlackBoxName"].ToString(), Value = reader["BlackBoxID"].ToString() });
            }
          }
          DropDownList1 = SelectList.ToList();
          ctx.Database.Connection.Close();
          return DropDownList1; //return the list objects

        }
      }
    }

    public static IEnumerable<SelectListItem> GetRAPSDroneList() {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
      String SQL = "SELECT 0 as Value, 'Not Available' as Name";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();

          SQL = "SELECT [DroneId] as Value, [RPASSerialNo] as Name FROM [MSTR_Drone] where IsActive=1";
          SQL += "\n" +
            " AND\n " +
            "  MSTR_Drone.CreatedBy =" + Util.getLoginUserID();

          SQL += "\n ORDER BY [DroneName]";
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              SelectList.Add(new SelectListItem {
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

    public static IEnumerable<SelectListItem> GetYeNoList() {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
      SelectList.Add(new SelectListItem { Text = "Yes", Value = "1" });
      SelectList.Add(new SelectListItem { Text = "No", Value = "0" });
      return SelectList; //return the list objects
    }//function GetDropDowntList

    public static IEnumerable<SelectListItem> GetBoolList(bool NeedSelect) {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      if (NeedSelect)
        SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
      SelectList.Add(new SelectListItem { Text = "True", Value = "1" });
      SelectList.Add(new SelectListItem { Text = "False", Value = "2" });
      return SelectList; //return the list objects
    }//function GetDropDowntList

    public static IEnumerable<SelectListItem> GetApporveRejectList() {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      //SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "" });
      SelectList.Add(new SelectListItem { Text = "Approved", Value = "Approved" });
      SelectList.Add(new SelectListItem { Text = "Rejected", Value = "Rejected" });
      SelectList.Add(new SelectListItem { Text = "Amended", Value = "Amended" });

      return SelectList; //return the list objects
    }//function GetDropDowntList

    public static IEnumerable<SelectListItem> GetDropDowntList(string TypeField, string NameField, string ValueField, string SPName, bool OtherRquired) {
      //  ctx=new ExponentPortalEntities();
      List<SelectListItem> SelectList = new List<SelectListItem>();
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {

          ctx.Database.Connection.Open();


          cmd.CommandText = "usp_Portal_GetDroneDropDown";
          DbParameter Param = cmd.CreateParameter();
          Param.ParameterName = "@Type";
          Param.Value = TypeField;
          //  Param[0] = new DbParameter("@Type", TypeField);
          cmd.Parameters.Add(Param);
          cmd.CommandType = CommandType.StoredProcedure;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });
            }
          }
          if (OtherRquired)
            SelectList.Add(new SelectListItem { Text = "Other", Value = "-1" });

          DropDownList = SelectList.ToList();
          ctx.Database.Connection.Close();
          return DropDownList; //return the list objects

        }
      }
    }

    public static String getDroneNameByDroneID(int DroneID) {
      String SQL = "SELECT \n" +
     "  [DroneName]\n" +
     "FROM\n" +
     "  [MSTR_Drone]\n" +
     "WHERE\n" +
     "  MSTR_Drone.DroneId=" + DroneID;
      return getDBVal(SQL);
    }
  }
}