using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace eX_Portal.exLogic {
  public class qDetailView {
    public String SQL { get; set; }
    public int Columns { get; set; }
    public   Dictionary<string, string> FormatCols =
                new Dictionary<string, string>();
       
        public qDetailView(String SQLQuery, int Cols = 3, Dictionary<string, string> FormatCol=null) {
      SQL = SQLQuery;
      Columns = Cols;
      if (FormatCols == null) FormatCol = new Dictionary<string, string>();


        }
    public String getTable() {
      int Col = 0;
            
            StringBuilder Table = new StringBuilder();

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = SQL;
        using (var reader = cmd.ExecuteReader()) {

          //For each row
          while (reader.Read()) {
            Table.AppendLine("<table class=\"qDetailView\">");
                       
                        for (int i = 0; i < reader.FieldCount; i++) {

                        
                           
              Col = Col + 1;
              if (Col == 1) Table.AppendLine("<tr>");
              Table.Append("<td>");
              Table.Append("<span class=\"caption\">");
              Table.Append(Util.toCaption(reader.GetName(i)));
              Table.Append(":</span>");
              Table.Append("<span class=\"value\">");
                            //checking whether the column required link
                            if (FormatCols.ContainsKey(reader.GetName(i)))
                            {
                                Table.Append(FormatCols[reader.GetName(i)]);
                            }
                            else
                            {
                                Table.Append("$" + reader.GetName(i).ToString());
                               
                            }
              Table.Append("</span>");                            
              Table.Append("</td>");
              if (Col >= Columns) {
                Col = 0;
                Table.AppendLine("</tr>");
              }
            } //for

   
            for(int i = 0; i < reader.FieldCount;  i++ ) {
                 Table.Replace("$" + reader.GetName(i), reader.GetValue(i).ToString());
            }
           

             

            //add additional columns if required
            if (Col > 0) {
              for (int i = Col + 1; i <= Columns; i++) {
                Table.AppendLine("<td></td>");
              }
              Table.AppendLine("</tr>");
            }


            Table.AppendLine("</table>");
          }//while

        }//using reader
      }//using database
      return Table.ToString();
    }//getTable()
  }//class
}//namespace
            