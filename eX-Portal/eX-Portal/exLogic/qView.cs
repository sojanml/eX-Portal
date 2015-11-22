using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;

namespace eX_Portal.exLogic {

  public class qView {
    private ExponentPortalEntities ctx;
    public String SQL { get; set; }
    public List<String> ColumDef = new List<String>();


    public qView(String sSQL = "") {
      if (sSQL != "") SQL = sSQL;
      ctx = new ExponentPortalEntities();
      setColumDef();
    } //qView


    public String getDataJson() {
      int x = 0;
      String _TotalRecords = "";
      StringBuilder Row = new StringBuilder();
      StringBuilder Data = new StringBuilder();

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = SQL;
        using (var reader = cmd.ExecuteReader()) {

          //For each row
          while (reader.Read()) {

            if (x > 0) {
              Row.AppendLine(",");
            } else {
              _TotalRecords = reader["_TotalRecords"].ToString();
            }
            StringBuilder Columns = new StringBuilder();
            for (int i = 0; i < reader.FieldCount; i++) {
              if (i > 0) Columns.AppendLine(",");
              Columns.Append("\"" + reader.GetName(i) + "\"");
              Columns.Append(" : ");
              Columns.Append("\"" + reader.GetValue(i).ToString() + "\"");
            }
            Row.Append("{");
            Row.Append(Columns);
            Row.Append("}");
            x = x + 1;
          }//while


        }
      }

      Data.AppendLine("{");
      Data.AppendLine("\"recordsTotal\" : " + _TotalRecords + ",");
      Data.AppendLine("\"recordsFiltered\" : " + _TotalRecords + ",");
      Data.AppendLine("\"data\" : [");
      Data.Append(Row);
      Data.AppendLine("]");
      Data.AppendLine("}");


      return Data.ToString();
    }//getData

    public String getDataTable() {
      StringBuilder Table = new StringBuilder();
      StringBuilder THead = new StringBuilder();

      THead.AppendLine("<tr>");
      foreach (var Column in ColumDef) {
        THead.Append("<th>");
        THead.Append(Util.toCaption(Column));
        THead.AppendLine("</th>");
      }//foreach
      THead.AppendLine("</tr>");

      Table.AppendLine("<table id=\"qViewTable\" class=\"report\">");
      Table.AppendLine("<thead>");
      Table.Append(THead);
      Table.AppendLine("</thead>");

      Table.AppendLine("<tfoot>");
      Table.Append(THead);
      Table.AppendLine("</tfoot>");
      Table.AppendLine("</table>");
      return Table.ToString();
    }//getDataTable()


    public String getScripts() {

      StringBuilder Scripts = new StringBuilder();
      StringBuilder ScriptColumns = new StringBuilder();
      bool isFirstColumn = true;
      ScriptColumns.AppendLine("\"columns\": [");
      foreach (var Column in ColumDef) {
        if (!isFirstColumn) ScriptColumns.AppendLine(",");
        ScriptColumns.Append("{ \"data\": \"" + Column + "\" }");
        isFirstColumn = false;
      }
      ScriptColumns.AppendLine("]");

      Scripts.AppendLine("$(document).ready(function() {");
      Scripts.AppendLine("    $('#qViewTable').DataTable( {");
      Scripts.AppendLine("    \"processing\": true,");
      Scripts.AppendLine("        \"serverSide\": true, ");
      Scripts.AppendLine("        \"ajax\": \"" + HttpContext.Current.Request.RequestContext.HttpContext.Request.Url + "\",");
      //Scripts.AppendLine("        \"ajax\": \"/Product/Index2\",");
      Scripts.Append(ScriptColumns);
      Scripts.AppendLine("  } );");
      Scripts.AppendLine("} );");

      return Scripts.ToString();
    }


    private void setColumDef() {
      String mySQL = SQL.Replace("SELECT ", "SELECT TOP 0 ");


      var cmd = ctx.Database.Connection.CreateCommand();
      DataTable schemaTable;
      ctx.Database.Connection.Open();
      cmd.CommandText = mySQL;

      DbDataReader myReader = cmd.ExecuteReader(CommandBehavior.KeyInfo);

      //Retrieve column schema into a DataTable.
      schemaTable = myReader.GetSchemaTable();

      //For each field in the table...
      foreach (DataRow myField in schemaTable.Rows) {
        //For each property of the field...
        //Columns.Add(myField["BaseTableName"] + "." + myField["BaseColumnName"]);      
        String ColumnName = myField["ColumnName"].ToString();


        switch (ColumnName) {
          case "_TotalRecords":
            break;
          default:
            ColumDef.Add(ColumnName);
            break;
        } //switch

      }//foreach

      myReader.Close();
      cmd.Dispose();
      ctx.Dispose();

    }//setColumDef()




  }//class qView
}//namespace eX_Portal.exLogic