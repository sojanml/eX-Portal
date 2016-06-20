using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace eX_Portal.exLogic {

  public class qView {
    private bool _isFilterByTop = true;
    private ExponentPortalEntities ctx;
    public String SQL { get; set; }
    public List<String> ColumDef = new List<String>();
    public List<String> FilterColumns = new List<String>();
    public String onRowClick { get; set; }
    public HttpResponse Response = HttpContext.Current.Response;
    public HttpRequest Request = HttpContext.Current.Request;
    private Dictionary<String, String> ColumnMapping = new Dictionary<string, string>();
    private int _SortColumn = 0;
    private String _SortOrder = "desc";

    public int SortColumn {
      get { return _SortColumn; }
      set { _SortColumn = value; }
    }

    public String SortOrder {
      get { return _SortOrder; }
      set { _SortOrder = value; }
    }

    public int _TotalRecords {
      get; set;
    }
    public bool isFilterByTop {
      get {
        return _isFilterByTop;
      }
      set {
        _isFilterByTop = value;
      }
    }

    public bool HasRows { get; set; }
    public string TotalSQL { get; internal set; }

    private List<qViewMenu> qViewMenus = new List<qViewMenu>();
    private bool IsPrimaryKey = false;
    public bool IsFormatDate = true;

    public qView(String sSQL = "", bool isFilterByTop = true) {
      if (sSQL != "") SQL = sSQL;
      _TotalRecords = 0;
      ctx = new ExponentPortalEntities();
      _isFilterByTop = isFilterByTop;
      if (!String.IsNullOrEmpty(SQL)) setColumDef();
    } //qView

    public bool addMenu(String Caption, String URL, String Icon = "") {
      qViewMenu Menu = new qViewMenu {
        Caption = Caption,
        URL = URL,
        Icon = Icon
      };
      qViewMenus.Add(Menu);
      return true;
    }

    private StringBuilder getMenu() {
      StringBuilder Menus = new StringBuilder();
      int i = 0;
      foreach (var Menu in qViewMenus) {
        i = i + 1;
        if (i > 1) Menus.AppendLine(",");
        Menus.AppendLine("{");
        Menus.AppendLine("\"caption\": \"" + Menu.Caption + "\",");
        Menus.AppendLine("\"url\": \"" + Menu.URL + "\",");
        Menus.AppendLine("\"class\": \"" + Menu.ClassName + "\"");
        Menus.Append("}");
      }
      return Menus;
    }

    private void setColumnMapping() {
      SimpleParser DataColumns = new SimpleParser();
      DataColumns.Parse("ALTER VIEW X as\n" + SQL);

      foreach (var col in DataColumns.aColumnInfoList.ColumnList) {
        String ColName = col.Alias;
        String FieldName = (col.TableAlias == "" ? "" : col.TableAlias + ".") + col.TableColumnName;
        if (String.IsNullOrEmpty(FieldName)) FieldName = col.Expression;
        //                   (String.IsNullOrEmpty(col.TableColumnName) ? col.Alias : );
        ColumnMapping.Add(ColName, FieldName);
      }
    }

    private String getQueryFilter() {
      String Filter = "";

      var SearchTerm = Request["search[value]"].ToString();
      if (SearchTerm != "") {

        foreach (var Column in FilterColumns) {
          if (!String.IsNullOrWhiteSpace(ColumnMapping[Column])) {
            if (Filter != "") Filter += " OR ";
            Filter += ColumnMapping[Column] + " LIKE '%" + SearchTerm + "%'";
          }
        }
        if (Filter != "") Filter = " (" + Filter + ")";
      }
      return Filter;
    }

    private String getQueryOrder() {
      String OrderColumnKey = "order[0][column]";
      String OrderDirKey = "order[0][dir]";

      int OrderColumn = 0;
      String OrderDir = Request[OrderDirKey];
      Int32.TryParse(Request[OrderColumnKey], out OrderColumn);
      if (OrderDir != "asc") OrderDir = "desc";

      return ColumDef.ElementAt(OrderColumn) + " " + OrderDir;

    }

    private String setOrderFilterPaging(String SQL) {
      //http://www.codeproject.com/Articles/32524/SQL-Parser

      setColumnMapping();

      String SearchFilter = getQueryFilter();
      String SearchOrderBy = getQueryOrder();
      Parser.SqlParser myParser = new Parser.SqlParser();
      myParser.Parse(SQL);


      //Add Required Filter
      if (SearchFilter != "") {
        if (string.IsNullOrEmpty(myParser.WhereClause)) {
          myParser.WhereClause = SearchFilter;
        } else {
          myParser.WhereClause += " AND " + SearchFilter;
        }//if string.IsNullOrEmpty(myParser.WhereClause)
      }//if (SearchFilter != "")


      //Set Paging for SQL Query
      String OrderColumn = Request["order[0][column]"];
      String OrderDir = Request["order[0][dir]"];
      if (OrderDir != "asc") OrderDir = "desc";
      String OrderField = ColumDef.ElementAt(Util.toInt(OrderColumn));
      int StartAt = Util.toInt(Request["start"]);
      int RowLength = Util.toInt(Request["length"]);
      myParser.OrderByClause = "";
      SQL = myParser.ToText();

      //Replace only first instance of SQL with RowNumber()
      var regex = new Regex(Regex.Escape("SELECT"));
      String RowNumber = "SELECT ROW_NUMBER() OVER (ORDER BY " + ColumnMapping[OrderField] + " " + OrderDir + ") AS _RowNumber,\n";
      SQL = regex.Replace(SQL, RowNumber, 1);

      String newSQL = "SELECT * FROM (\n" +
        SQL +
        ") as QueryTable\n" +
        "WHERE\n" +
        "  _RowNumber > " + StartAt + " AND _RowNumber <= " + (RowLength + StartAt) + "\n" +
        "ORDER BY\n" +
        SearchOrderBy;

      return newSQL;
      /*
      //Add Order BY
      if (SearchOrderBy != "") {
        myParser.OrderByClause = SearchOrderBy;
      }

      return myParser.ToText();
      */
    }


    public String getDataJson() {
      int x = 0;

      StringBuilder Row = new StringBuilder();
      StringBuilder Data = new StringBuilder();

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = setOrderFilterPaging(SQL);
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
              String DisplayValue = getFieldVal(reader, i);
              if (i > 0) Columns.AppendLine(",");
              Columns.Append("\"" + reader.GetName(i) + "\"");
              Columns.Append(" : ");
              Columns.Append("\"" + Util.toSQL(DisplayValue) + "\"");
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
      Data.AppendLine("\"searchDelay\" : 1000,");
      Data.AppendLine("\"data\" : [");
      Data.Append(Row);
      Data.AppendLine("]");
      Data.AppendLine("}");


      return Data.ToString();
    }//getData


    private String getFieldVal(DbDataReader reader, int i) {
      String FieldValue = "";
      switch (reader.GetFieldType(i).ToString()) {
      case "System.DateTime":
      try {
        DateTime Dt = reader.GetDateTime(i);
        if (String.IsNullOrEmpty(Dt.ToString())) {
          FieldValue = "Invalid";
        } else {
          if (IsFormatDate) {
            if (Dt.Hour == 0 && Dt.Minute == 0 && Dt.Second == 0) {
              FieldValue = String.Format("{0:dd-MMM-yyyy}", Dt);
            } else {
              FieldValue = String.Format("{0:dd-MMM-yyyy HH:mm}", Dt);
            }
          } else {
            FieldValue = String.Format("{0:dd-MMM-yyyy HH:mm:ss}", Dt);
          }
        }
      } catch {
        FieldValue = "Invalid";
      }
      break;
      case "System.Decimal":
      FieldValue = reader.GetDecimal(i).ToString("###,##0.00");
      break;
      default:
      FieldValue = reader.GetValue(i).ToString();
      break;
      }
      return FieldValue;
    }


    public String getDataTable(
      bool isIncludeData = false,
      bool isIncludeFooter = true,
      String qDataTableID = "qViewTable") {
      StringBuilder Table = new StringBuilder();
      StringBuilder THead = new StringBuilder();

      THead.AppendLine("<tr>");
      foreach (var Column in ColumDef) {
        THead.Append("<th>");
        THead.Append(Util.toCaption(Column));
        THead.AppendLine("</th>");
      }//foreach
      if (IsPrimaryKey) THead.Append("<th class=\"menu\">&nbsp;</th>");
      THead.AppendLine("</tr>");

      Table.AppendLine("<table id=\"" + qDataTableID + "\" class=\"report\">");
      Table.AppendLine("<thead>");
      Table.Append(THead);

      Table.AppendLine("</thead>");

      if (isIncludeData) {
        Table.AppendLine("<tbody>");
        Table.Append(getTableRows());
        Table.AppendLine("</tbody>");
      }

      //add total row to the sql
      if (!String.IsNullOrEmpty(TotalSQL)) {
        Dictionary<String, Object> Row = Util.getDBRow(TotalSQL);
        StringBuilder TotalRow = new StringBuilder();
        TotalRow.AppendLine("<tfoot>");
        TotalRow.AppendLine("<tr>");
        foreach (var Column in Row) {
          switch (Column.Key.ToLower()) {
          case "hasrows":
          break;
          default:
          TotalRow.AppendLine("<th>" + Column.Value.ToString() + "</th>");
          break;
          }
        }
        if (IsPrimaryKey) TotalRow.AppendLine("<th></th>");
        TotalRow.AppendLine("</tr>");
        TotalRow.AppendLine("</tfoot>");
        Table.Append(TotalRow);
      }


      if (isIncludeFooter) {
        Table.AppendLine("<tfoot>");
        Table.Append(THead);
        Table.AppendLine("</tfoot>");
      }

      Table.AppendLine("</table>");
      return Table.ToString();
    }//getDataTable()


    public StringBuilder getTableRows() {

      StringBuilder Rows = new StringBuilder();
      bool isEven = false;

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = SQL;
        using (var reader = cmd.ExecuteReader()) {
          //For each row
          while (reader.Read()) {
            Rows.AppendLine("<tr class=\"" + (isEven ? "even" : "odd") + "\">");
            for (int i = 0; i < reader.FieldCount; i++) {
              String DisplayValue = getFieldVal(reader, i);
              switch (reader.GetName(i).ToLower()) {
              case "_pkey":
              Rows.AppendLine("<td class=\"menu\"><img data-pkey=\"" + DisplayValue + "\" class=\"row-button\" src=\"/images/drop-down.png\"></td>");
              break;
              default:
              Rows.AppendLine("<td>" + DisplayValue + "</td>");
              break;
              }//switch
            }//for
            Rows.AppendLine("</tr>");
            isEven = !isEven;
          } //while
        }
      }
      return Rows;
    }

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
      if (IsPrimaryKey) ScriptColumns.AppendLine("\n, { \"data\": null, \"defaultContent\": \"<img class=button src=/images/drop-down.png>\",  \"orderable\": false,  className: \"menu\" }");

      ScriptColumns.AppendLine("]");

      Scripts.AppendLine("var qViewDataTable = null;");
      Scripts.AppendLine("var qViewMenu =  [");
      Scripts.Append(getMenu());
      Scripts.AppendLine("];");

      Scripts.AppendLine("$(document).ready(function() {");
      Scripts.AppendLine("  qViewDataTable = $('#qViewTable').DataTable( {");
      Scripts.AppendLine("    \"processing\": true,");
      Scripts.AppendLine("    \"serverSide\": true, ");
      Scripts.AppendLine("    \"searchDelay\": 1000, ");
      Scripts.AppendLine("    \"iDisplayLength\": 50, ");
      Scripts.AppendLine("    \"fnFooterCallback\": _fnFooterCallback, ");
      Scripts.AppendLine("    \"fnDrawCallback\": _fnDrawCallback, ");
      Scripts.AppendLine("    \"order\": [[" + _SortColumn.ToString() + ",\"" + _SortOrder + "\"]], ");
      Scripts.AppendLine("    \"ajax\": \"" + HttpContext.Current.Request.RequestContext.HttpContext.Request.Url + "\",");
      Scripts.Append(ScriptColumns);

      Scripts.AppendLine("  } );");
      Scripts.AppendLine("} );");

      return Scripts.ToString();
    }


    private void setColumDef() {
      int ColumnCounter = 0;
      String mySQL = SQL;  

      if(isFilterByTop) { 
        string pattern = @"SELECT[\s\n]";
        Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        
        mySQL = rgx.Replace(SQL, "SELECT TOP 1 ");
      }


      /*
      //can not use the none filter, we need to find is there any rows
      //exists to build the list

      //http://www.codeproject.com/Articles/32524/SQL-Parser

      String NowRowFilter = "1 = 0";
      Parser.SqlParser myParser = new Parser.SqlParser();
      myParser.Parse(SQL);
      if(string.IsNullOrEmpty(myParser.WhereClause)) {
        myParser.WhereClause = NowRowFilter;
      } else {
        myParser.WhereClause += " AND " + NowRowFilter;
      }
      String mySQL = myParser.ToText();
      */

      var cmd = ctx.Database.Connection.CreateCommand();
      DataTable schemaTable;
      ctx.Database.Connection.Open();
      cmd.CommandText = mySQL;

      DbDataReader myReader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
      HasRows = myReader.HasRows;
      schemaTable = myReader.GetSchemaTable();
      /*
      //Retrieve column schema into a DataTable.
      foreach (DataRow myField in schemaTable.Rows) {
        Response.Write("<P>");
        foreach (DataColumn Field in schemaTable.Columns) {
          Response.Write(Field.ColumnName + ": " + myField[Field] + "<br>\n");
        }
        Response.Write("</P>");
      }
      */

      //For each field in the table...
      foreach (DataRow myField in schemaTable.Rows) {
        ColumnCounter++;

        //For each property of the field...
        //Columns.Add(myField["BaseTableName"] + "." + myField["BaseColumnName"]);      
        String ColumnName = myField["ColumnName"].ToString();
        String FieldType = myField["DataType"].ToString();
        if (FieldType == "System.String") {
          FilterColumns.Add(ColumnName);
        }
        if (ColumnCounter == 1 && FieldType == "System.String") {
          _SortOrder = "asc";
        }


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

  public class qViewMenu {
    private String pCaption = "";
    public String Caption {
      get {
        return pCaption;
      }//set
      set {
        pCaption = value;
        if (String.IsNullOrEmpty(ClassName)) ClassName = "_" + pCaption.ToLower().Replace(" ", "_");
      }//set
    }//property Caption
    public String URL { get; set; }
    public String ClassName { get; set; }
    public String Icon { get; set; }
  }
}//namespace eX_Portal.exLogic