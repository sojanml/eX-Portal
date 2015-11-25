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
    public String onRowClick { get; set; }

    public int _TotalRecords {
      get; set;
    }

    public bool HasRows { get; set; }
    
    private List<qViewMenu> qViewMenus = new List<qViewMenu>();
    private bool IsPrimaryKey = false;

    public qView(String sSQL = "") {
      if (sSQL != "") SQL = sSQL;
      _TotalRecords = 0;
      ctx = new ExponentPortalEntities();
      setColumDef();
    } //qView

    public bool addMenu(String Caption, String URL) {
      qViewMenu Menu = new qViewMenu{
        Caption= Caption,
        URL= URL
      };
      qViewMenus.Add(Menu);
      return true;
    }

    private StringBuilder getMenu() {
      StringBuilder Menus = new StringBuilder();
      int i = 0;
      foreach(var Menu in qViewMenus) {
        i = i + 1;
        if (i > 1) Menus.AppendLine(",");
        Menus.AppendLine("{");
        Menus.AppendLine("\"caption\": \"" + Menu.Caption +  "\",");
        Menus.AppendLine("\"url\": \"" + Menu.URL + "\"");
        Menus.Append("}");
      }
      return Menus;
    }

    public String getDataJson() {
      int x = 0;

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
              _TotalRecords = Int32.Parse(reader["_TotalRecords"].ToString());
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
      if(IsPrimaryKey) THead.Append("<th class=\"menu\">&nbsp;</th>");
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
        String ColumnName = Column;
        if (!isFirstColumn) ScriptColumns.AppendLine(",");
        ScriptColumns.Append("{ \"data\": \"" + ColumnName + "\"");
        ScriptColumns.Append("}");
        isFirstColumn = false;
      }
      if(IsPrimaryKey) ScriptColumns.AppendLine("\n, { \"data\": null, \"defaultContent\": \"<img class=button src=/images/drop-down.png>\", className: \"menu\" }");

      ScriptColumns.AppendLine("]");

      Scripts.AppendLine("var qViewDataTable = null;");
      Scripts.AppendLine("var qViewMenu =  [");
      Scripts.Append(getMenu());
      Scripts.AppendLine("];");

      Scripts.AppendLine("$(document).ready(function() {");
      Scripts.AppendLine("  qViewDataTable = $('#qViewTable').DataTable( {");
      Scripts.AppendLine("    \"processing\": true,");
      Scripts.AppendLine("    \"serverSide\": true, ");
      Scripts.AppendLine("    \"ajax\": \"" + HttpContext.Current.Request.RequestContext.HttpContext.Request.Url + "\",");
      Scripts.Append(ScriptColumns);

      Scripts.AppendLine("  } );");
      Scripts.AppendLine("} );");




      return Scripts.ToString();
    }


    private void setColumDef() {
      String mySQL = SQL.Replace("SELECT ", "SELECT TOP 1 ");
      HttpResponse Response = HttpContext.Current.Response;

      var cmd = ctx.Database.Connection.CreateCommand();
      DataTable schemaTable;
      ctx.Database.Connection.Open();
      cmd.CommandText = mySQL;


      DbDataReader myReader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
      HasRows = myReader.HasRows;

      //Retrieve column schema into a DataTable.
      schemaTable = myReader.GetSchemaTable();
      /*
      foreach (DataRow myField in schemaTable.Rows) {
        Response.Write("<P>");
        foreach (DataColumn Field in schemaTable.Columns) {
          Response.Write(Field + ": " + myField[Field] + "<br>\n");
        }
        Response.Write("</P>");
      }
      */

      //For each field in the table...
      foreach (DataRow myField in schemaTable.Rows) {
        //For each property of the field...
        //Columns.Add(myField["BaseTableName"] + "." + myField["BaseColumnName"]);      
        String ColumnName = myField["ColumnName"].ToString();

        if (myField["IsHidden"].ToString() == "False") {
          switch (ColumnName.ToLower()) {
            case "_totalrecords":
              break;
            case "_pkey":
              IsPrimaryKey = true;
              break;
            default:
              ColumDef.Add(ColumnName);
              break;
          } //switch
        }

      }//foreach

      myReader.Close();
      cmd.Dispose();
      ctx.Dispose();

    }//setColumDef()


  }//class qView

  public class qViewMenu{
    public String Caption { get; set; }
    public String URL { get; set; }
  }
}//namespace eX_Portal.exLogic