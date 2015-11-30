using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.Common;
using System.Text.RegularExpressions;
using eX_Portal.Models;
using System.Web.Mvc;

namespace eX_Portal.exLogic {
  public partial class Util {


    public static String toCaption(String Title) {
      Title = Regex.Replace(Title, "([A-Z][a-z])", m => " " + m.Groups[1]);
      Title = Regex.Replace(Title, "_", " ");
      Title = Regex.Replace(Title, "\\s+", " ");
      return Title.Trim().ToString();
    }

    public static int doSQL(String SQL) {
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        result = ctx.Database.ExecuteSqlCommand(SQL);
      }
      return result;
    }

    public static int InsertSQL(String SQL) {
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          cmd.ExecuteNonQuery();

          cmd.CommandText = "SELECT scope_identity()";
          result = Int32.Parse(cmd.ExecuteScalar().ToString());
           
        }
      }
      return result;
    }

    public static string getDBVal(String SQL) {
      String Result = "";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          Result = cmd.ExecuteScalar().ToString();
        }
      }
      return Result;
    }

<<<<<<< HEAD
    public String getQ(String FieldName) {
=======
    public static Dictionary<String, Object> getDBRow(String SQL) {
      var Result = new Dictionary<String, Object>();
      Result["hasRows"] = false;
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            if (reader.Read()) {
              Result["hasRows"] = true;
              for (int i = 0; i < reader.FieldCount; i++) {
                Result[reader.GetName(i)] = reader.GetValue(i);
              }//for
            }//if
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities
      return Result;
    }//function

    public static String getQ(String FieldName) {
>>>>>>> cbd8da4440f926610cb438281d69a9c01c1704bc
      HttpRequest Request = HttpContext.Current.Request;
      return Request[FieldName].ToString();
    }

    public static String toSQL(String FieldValue) {
      String sVal = FieldValue;
      sVal = sVal.Replace("\\", "\\\\");
      sVal = sVal.Replace("'", "\\'");
      return sVal;
    }

    public static IEnumerable<SelectListItem> GetDropDowntList(String TypeOfList) {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

      String SQL = "SELECT 0 as Value, 'Not Available' as Name";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          switch (TypeOfList.ToLower()) {
            case "drone":
              SQL = "SELECT [DroneId] as Value  ,[DroneName] as Name FROM [MSTR_Drone] ORDER BY [DroneName]";
              break;
            case "pilot":
            case "gsc":
              SQL = "SELECT UserID as Value, FirstName as Name FROM MSTR_User ORDER BY FirstName";
              break;
          }
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

  }//class Util
}