using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.Common;
using System.Text.RegularExpressions;
using eX_Portal.Models;

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

    public String getQ(String FieldName) {
      HttpRequest Request = HttpContext.Current.Request;
      return Request[FieldName].ToString();
    }

    public static String toSQL(String FieldValue) {
      String sVal = FieldValue;
      sVal = sVal.Replace("\\", "\\\\");
      sVal = sVal.Replace("'", "\\'");
      return sVal;
    }

  }//class Util
}