using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
 
namespace eX_Portal.Controllers {
  public class AutoCompleteController : Controller {
    // GET: AutoComplete
    public String Parts(String Term = "") {
      String SQL =
      "select\n" +
      "  PartsId as id,\n" +
      "  CONCAT(PartsName, ' - ', Model, ' - ', MSTR_User.FirstName) as value,\n" +
      "  PartsName,\n" +
      "  Model,\n" +
      "  ISNULL(MSTR_User.FirstName, '') as Supplier\n" +
      "from\n" +
      "  MSTR_Parts\n" +
      "LEFT JOIN MSTR_User On\n" +
      "  MSTR_User.UserId = MSTR_Parts.SupplierId\n";

      if (Term != "") {
        SQL += "WHERE\n" +
        "MSTR_Parts.PartsName LIKE '" + Term + "%' OR\n" +
        "MSTR_Parts.Model LIKE '" + Term + "%' OR\n" +
        "MSTR_User.FirstName LIKE '" + Term + "%'";
      }


      return toJSon(SQL);
    }//function Parts()

    private String toJSon(String SQL) {
      bool isFirstRow = true;
      StringBuilder Data = new StringBuilder();

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = SQL;
        using (var reader = cmd.ExecuteReader()) {
          Data.AppendLine("[");
          //For each row
          while (reader.Read()) {
            if (!isFirstRow) Data.Append(",");
            Data.AppendLine("{");
            for (int i = 0; i < reader.FieldCount; i++) {
              if (i > 0) Data.AppendLine(",");
              Data.Append("\"" + reader.GetName(i) + "\"");
              Data.Append(" : ");
              Data.Append("\"" + reader.GetValue(i).ToString() + "\"");
            }//for
            Data.Append("\n}");
            isFirstRow = false;
          }//while
          Data.AppendLine("]");

        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities

      return Data.ToString();

    }//function toJSon

  }//class
}//namespace