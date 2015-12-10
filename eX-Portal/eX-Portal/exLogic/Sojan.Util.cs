﻿using System;
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

    public static String getDBVal(String SQL) {
      String Result = "";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          var oResult = cmd.ExecuteScalar();
          if (oResult != null)
            Result = oResult.ToString();
        }
      }
      return Result;
    }

    public static int getDBInt(String SQL) {
      int Result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          var oResult = cmd.ExecuteScalar();
          if (oResult != null) Int32.TryParse(oResult.ToString(), out Result);
        }
      }
      return Result;
    }

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


    public static List<Dictionary<String, Object>> getDBRows(String SQL) {      
      var Rows = new List<Dictionary<String, Object>>();
      var Result = new Dictionary<String, Object>();
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              Result = new Dictionary<String, Object>();
              Result["hasRows"] = true;
              for (int i = 0; i < reader.FieldCount; i++) {
                Result[reader.GetName(i)] = reader.GetValue(i);
              }//for
              Rows.Add(Result);
            }//while
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities
      return Rows;
    }//function

    public static String getQ(String FieldName) {
      HttpRequest Request = HttpContext.Current.Request;
      String FieldValue = Request[FieldName];
      return FieldValue == null ? "" : FieldValue.ToString();
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

    public static String getDroneName(int DroneID) {
      String SQL = "SELECT \n" +
     "  D.[DroneName] + ' - ' +  DroneIdHexa as DroneName\n" +
     "FROM\n" +
     "  [ExponentPortal].[dbo].[MSTR_Drone] D\n" +
     "WHERE\n" +
     "  D.[DroneId]=" + DroneID;
      return getDBVal(SQL);
    }

    public static void ErrorHandler(Exception ex) {
      //Reference - http://stackoverflow.com/questions/3328990/c-sharp-get-line-number-which-threw-exception
      // Get stack trace for the exception with source file information
      var st = new System.Diagnostics.StackTrace(ex, true);
      // Get the top stack frame
      var frame = st.GetFrame(0);
      // Get the line number from the stack frame
      var line = frame.GetFileLineNumber();
    }

    public static String jsonStat(String Status, String Message = "") {
      return "{\n" +
        "\"status\":\"" + toSQL(Status) + "\",\n" +
        "\"message\":\"" + toSQL(Message) + "\"\n" +
        "}";
    }


  }//class Util
}