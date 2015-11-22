using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace eX_Portal.exLogic {
  public partial class Util {
    public static SqlConnection getDB() {
      String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ExponentPortalEntities"].ConnectionString;
      return new SqlConnection(DSN);
    }//getDB()


    public static String toCaption(String Title) {
      Title = Regex.Replace(Title, "([A-Z][a-z])", m => " " + m.Groups[1]);
      Title = Regex.Replace(Title, "_", " ");
      Title = Regex.Replace(Title, "\\s+", " ");
      return Title.Trim().ToString();
    }



  }//class Util
}