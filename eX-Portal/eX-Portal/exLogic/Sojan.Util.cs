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
using System.Globalization;
using System.IO;
using System.Web.SessionState;
using System.Text;
using System.Security.Cryptography;
using System.ComponentModel;

namespace eX_Portal.exLogic {
  public partial class Util {
    public static int EmailQue(
      int UserID,
      String ToAddress,
      String EmailSubject,
      String EmailURL) {
      int QueID = 0;
      using (var ctx = new ExponentPortalEntities()) {
        var Email = new PortalAlertEmail { 
          EmailSubject = EmailSubject,
          EmailURL = EmailURL,
          Body = null,
          FromAddress = "info@exponent-ts.com",
          ToAddress = ToAddress,
          UserID = UserID,
          //default values set for items
          CreatedOn = DateTime.Now,
          IsSend = 0,
          SendOn = null,
          SendStatus = "In Queue",
          SendType = "EMAIL"
        };
        ctx.PortalAlertEmails.Add(Email);
        ctx.SaveChanges();
        QueID = Email.EmailID;
      }

      return QueID;
    }


    public static int SMSQue(
      int UserID,
      String ToAddress,
      String Body) {
      int QueID = 0;
      using (var ctx = new ExponentPortalEntities()) {
        var Email = new PortalAlertEmail {
          EmailSubject = "SMS to " + ToAddress,
          EmailURL = null,
          Body = Body,
          FromAddress = "info@exponent-ts.com",
          ToAddress = ToAddress,
          UserID = UserID,
          //default values set for items
          CreatedOn = DateTime.Now,
          IsSend = 0,
          SendOn = null,
          SendStatus = "In Queue",
          SendType = "SMS"
        };
        ctx.PortalAlertEmails.Add(Email);
        ctx.SaveChanges();
        QueID = Email.EmailID;
      }

      return QueID;
    }
    public static String toCaption(String Title) {
      Title = Regex.Replace(Title, "([A-Z][a-z])", m => " " + m.Groups[1]);
      Title = Regex.Replace(Title, "_", " ");
      Title = Regex.Replace(Title, "\\s+", " ");
      return Title.Trim().ToString();
    }

    public static int doSQL(String SQL) {
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        result = ctx.Database.ExecuteSqlCommand(SQL);
      }
      return result;
    }

    public static int doSQL(String SQL, ExponentPortalEntities ctx) {
      return ctx.Database.ExecuteSqlCommand(SQL);
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



    public static String getDBVal(String SQL, ExponentPortalEntities ctx) {
      String Result = "";
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var oResult = cmd.ExecuteScalar();
        if (oResult != null) Result = oResult.ToString();
      }
      return Result;
    }//getDBVal()

    public static int getDBInt(String SQL, ExponentPortalEntities ctx) {
      int Result = 0;
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var oResult = cmd.ExecuteScalar();
        if (oResult != null) Int32.TryParse(oResult.ToString(), out Result);
      }
      return Result;
    }//getDBInt()


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
    }//getDBVal()

    public static DateTime toDate(String StrDate) {
      DateTime dt = DateTime.Now;
      bool isSuccess = false;
      try {
        //10/12/2015 07:17:53
        dt = DateTime.ParseExact(StrDate, "yyyy/M/dd HH:mm:ss", CultureInfo.InvariantCulture);
        isSuccess = true;
      } catch {
        //nothing
      }
      try {
        //10/12/2015 07:17:53
        dt = DateTime.ParseExact(StrDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        isSuccess = true;
      } catch {
        //nothing
      }
      if (!isSuccess)
        try {
          dt = DateTime.ParseExact(StrDate, "dd MM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
          isSuccess = true;
        } catch {

        }
      if (!isSuccess)
        try {
          dt = DateTime.ParseExact(StrDate, "MM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
          isSuccess = true;
        } catch {

        }

      return dt;
    }

    public static decimal toDecimal(String Num) {
      Decimal theNum = 0;
      Decimal.TryParse(Num, out theNum);
      return theNum;
    }

    public static Double toDouble(Object Num) {
      Double theNum = 0;
      Double.TryParse(Num.ToString(), out theNum);
      return theNum;
    }

    public static int toInt(Object Str) {
      if (Str == null) Str = String.Empty;
      var theStr = Str.ToString();
      return toInt(theStr);
    }

    public static String toFix(String Num) {
      Double dNum = 0;
      Double.TryParse(Num, out dNum);
      return dNum.ToString("#0.00");
    }

    public static String toFix(Double ?Num) {
      Double dNum = Num == null ? 0 : (Double)Num;
      return dNum.ToString("#0.00");
    }
    public static String toFix(Decimal? Num) {
      Double dNum = Num == null ? 0 : (Double)Num;
      return dNum.ToString("#0.00");
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


    public static String getDBRowsJson(String SQL) {
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        return getDBRowsJson(SQL, ctx);
      }
    }

    public static String getDBRowsJson(String SQL, ExponentPortalEntities ctx) {
      StringBuilder SB = new StringBuilder();
      StringBuilder sRow = new StringBuilder();
      List<Dictionary<String, Object>> Rows = getDBRows(SQL, ctx);
      foreach (var Row in Rows) {
        if (SB.Length > 0) SB.AppendLine(",");
        sRow.Clear();
        foreach (var Key in Row.Keys) {
          if (sRow.Length > 0) sRow.Append(", ");
          sRow.Append("\"");
          sRow.Append(Key);
          sRow.Append("\": ");
          if (!IsNumber(Row[Key])) sRow.Append("\"");
          sRow.Append(Row[Key]);
          if (!IsNumber(Row[Key])) sRow.Append("\"");
        }
        SB.Append("{");
        SB.Append(sRow);
        SB.Append("}");
      }
      return SB.ToString();

    }


    public static bool IsNumber(object value) {
      return value is sbyte
              || value is byte
              || value is short
              || value is ushort
              || value is int
              || value is uint
              || value is long
              || value is ulong
              || value is float
              || value is double
              || value is decimal;
    }

    public static List<Dictionary<String, Object>> getDBRows(String SQL) {
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        return getDBRows(SQL, ctx);
      }
    }
    public static List<Dictionary<String, Object>> getDBRows(String SQL, ExponentPortalEntities ctx) {
      var Rows = new List<Dictionary<String, Object>>();
      var Result = new Dictionary<String, Object>();

      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        //ctx.Database.Connection.Open();
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
      sVal = sVal.Replace("\n", "\\n");
      sVal = sVal.Replace("\r", "\\r");
      return sVal;
    }

    public static IEnumerable<SelectListItem> GetDropDowntList(String TypeOfList, bool IsStrictFilter = false) {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });

      String SQL = "SELECT 0 as Value, 'Not Available' as Name";
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          switch (TypeOfList.ToLower()) {
          case "drone":
          SQL = "SELECT [DroneId] as Value, Convert(nvarchar(20),DroneId)+'-'+[DroneName] as Name FROM [MSTR_Drone] where IsActive=1";
          if (IsStrictFilter || !exLogic.User.hasAccess("DRONE.MANAGE")) {
            SQL += "\n" +
              " AND\n " +
              "  MSTR_Drone.AccountID=" + Util.getAccountID();
          }
          SQL += "\n ORDER BY [DroneName]";
          break;
          case "pilot":
          SQL = "SELECT UserID as Value, FirstName as Name FROM MSTR_User  WHERE \n";
          if (IsStrictFilter || !exLogic.User.hasAccess("DRONE.MANAGE")) {
            SQL += "\n" +

              "  MSTR_User.AccountID=" + Util.getAccountID() +
             " and ";
          }
          SQL += "\n MSTR_User.IsPilot=1 ORDER BY FirstName";
          break;
          case "gsc":
          SQL = "SELECT UserID as Value, FirstName as Name FROM MSTR_User";
          if (IsStrictFilter || !exLogic.User.hasAccess("DRONE.MANAGE")) {
            SQL += "\n" +
              "WHERE\n" +
              "  MSTR_User.AccountID=" + Util.getAccountID();
          }
          SQL += "\nORDER BY FirstName";
          break;
          }
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

    public static String getDroneName(int DroneID) {
      String SQL = "SELECT \n" +
     "  D.[DroneName]\n" +
     "FROM\n" +
     "  [MSTR_Drone] D\n" +
     "WHERE\n" +
     "  D.[DroneId]=" + DroneID;
      return getDBVal(SQL);
    }

    public static String getDroneRegistrationDocuments(int DroneID) {
      StringBuilder AllDocs = new StringBuilder();
      using(var ctx = new ExponentPortalEntities()) {
        List<DroneDocument> Docs = (
        from o in ctx.DroneDocuments
        where
        o.DocumentType == "UAS-Registration" &&
        o.DroneID == DroneID
        select o)
        .OrderBy(o => o.DocumentTitle)
        .OrderBy(o => o.DocumentName)
        .ToList();

        if(Docs.Count < 1) return "";

        String DroneName = getDroneName(DroneID);
        AllDocs.Append(@"<div class=""authorise"">Your Regulatory Authorization: <ul>");
        foreach(var Document in Docs) {
          String URL = Document.getURL();

          if(File.Exists(HttpContext.Current.Server.MapPath("~" + URL))) {
            if(Document.DocumentTitle == "Other")
              Document.DocumentTitle = Document.getMoreInfo();
            
            AllDocs.Append(@"<li><span class=""icon"">&#xf0f6;</span>");
            AllDocs.Append(@" <span class=""DocumentTitle"">");
            AllDocs.Append(Document.DocumentTitle);
            AllDocs.Append(" - </span>");
            AllDocs.Append(@"<a href=""" + URL + @""">" + Document.getName() + "</a></li>");

          }

        }
        AllDocs.Append("</ul></div>");
        return AllDocs.ToString();
      }
    }

    public static String getDroneNameByFlight(int FlightID) {
      String SQL = "SELECT \n" +
     "  [DroneName]\n" +
     "FROM\n" +
     "  [MSTR_Drone], DroneFlight\n" +
     "WHERE\n" +
     "  [MSTR_Drone].[DroneId]= DroneFlight.DroneID AND\n" +
     "  DroneFlight.ID=" + FlightID;
      return getDBVal(SQL);
    }
        public static String getDroneNameByApplicationID(int ApplicationID)
        {
            string SQL = @"select mstr_drone.DroneName from GCA_approval left join
                        mstr_drone on GCA_approval.droneid = mstr_drone.droneid
                        where GCA_approval.ApprovalID =" + ApplicationID;
            return getDBVal(SQL);
        }


        public static string getDroneIDByApplicationID(int ApplicationID)
        {
            string SQL = @"select GCA_approval.DroneID from GCA_approval where GCA_approval.ApprovalID =" + ApplicationID;
            return getDBVal(SQL);
        }
        public static void ErrorHandler(Exception ex = null) {
      /*
      //Reference - http://stackoverflow.com/questions/3328990/c-sharp-get-line-number-which-threw-exception
      // Get stack trace for the exception with source file information
      var st = new System.Diagnostics.StackTrace(ex, true);
      // Get the top stack frame
      var frame = st.GetFrame(0);
      // Get the line number from the stack frame
      var line = frame.GetFileLineNumber();
      */
    }

    public static String jsonStat(String Status, String Message = "") {
      return "{\n" +
        "\"status\":\"" + toSQL(Status) + "\",\n" +
        "\"message\":\"" + toSQL(Message) + "\"\n" +
        "}";
    }


    public static String getFileInfo(String FileName, String SaveFileName = null) {
      if (String.IsNullOrEmpty(SaveFileName)) SaveFileName = FileName;
      FileInfo oFileInfo = new FileInfo(FileName);
      return "{" +
        Pair("name", oFileInfo.Name, true) +
        Pair("savename", SaveFileName, true) +
        Pair("created", oFileInfo.CreationTime.ToString("dd-MMM-yyyy hh:mm:ss tt [zzz]"), true) +
        Pair("ext", oFileInfo.Extension, true) +
        Pair("records", getLineCount(FileName).ToString(), true) +
        Pair("size", (oFileInfo.Length / 1024).ToString("N0"), false) +
        "}";
    }//getFileInfo()

    public static string Pair(String Name, String Value, bool IsAddComma = false) {
      Value = Value.Replace("\\", "\\\\");
      Value = Value.Replace("\r\n", "\\n");
      return "\"" + Name + "\" : \"" + Value + "\"" + (IsAddComma ? "," : "") + "\n";
    }//Pair()

    public static int getLineCount(String FileName) {
      var lineCount = 0;
      using (var reader = System.IO.File.OpenText(FileName)) {
        while (reader.ReadLine() != null) {
          lineCount++;
        }//while
      }//using
      return lineCount;
    }//getLineCount()


    public static int getLoginUserID() {
      HttpSessionState Session = HttpContext.Current.Session;
      var UserID = 0;
      if (Session["UserID"] != null) int.TryParse(Session["UserID"].ToString(), out UserID);
      return UserID;
    }

    public static int getAccountID() {
      HttpSessionState Session = HttpContext.Current.Session;
      int AccountID = 0;
      if (Session["AccountID"] != null) int.TryParse(Session["AccountID"].ToString(), out AccountID);
      return AccountID;
    }

    public static String getDashboard() {
      String SQL = "SELECT DashBoard From MSTR_User WHERE UserID=" + getLoginUserID();
      String DashBoard = getDBVal(SQL);
      if (String.IsNullOrEmpty(DashBoard)) DashBoard = "Default";
      return DashBoard;
    }

    public static String getProfileImage(int UserID = 0) {
      if (UserID == 0) UserID = getLoginUserID();
      String SQL = "SELECT PhotoUrl From MSTR_User WHERE UserID=" + UserID;
      String ProfileImage = Util.getDBVal(SQL).ToString();
      if (ProfileImage == "") {
        return "/Upload/User/none.png";
      } else {
        return "/Upload/User/" + UserID + "/" + ProfileImage;
      }

    }

    public static String fmtDt(String theDt, bool isAddTime = true) {
      DateTime dt;
      DateTime.TryParse(theDt, out dt);
      if (isAddTime) {
        return dt.ToString("dd-MMM-yyyy hh:mm tt");
      } else {
        return dt.ToString("dd-MMM-yyyy");
      }
    }//fmtDt()


    public static String fmtDt(DateTime ?theDt, bool isAddTime = true) {
      DateTime dt = (theDt == null ? DateTime.MinValue : (DateTime)theDt);
      if (isAddTime) {
        return dt.ToString("dd-MMM-yyyy hh:mm tt");
      } else {
        return dt.ToString("dd-MMM-yyyy");
      }
    }//fmtDt()

    public static String toSQLDate(String sDate) {
      String sTheDate = "2001-01-01 00:00:01";
      try {
        DateTime theDate = DateTime.ParseExact(sDate, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        sTheDate = theDate.ToString("yyyy-MM-dd HH:mm:ss");
      } catch {
        //nothing
      }
      return sTheDate;
    }
    public static String toSQLDate(DateTime theDate) {
      return theDate.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static String getFilePart(String FullFile) {
      int TildAt = FullFile.IndexOf("~");
      if (TildAt > 0) return FullFile.Substring(TildAt + 1);
      return FullFile;
    }


    public static String getAllYardsAsOptions(String YardID) {
      String SQL = "SELECT YardID, YardName FROM PayLoadYard ORDER BY YardName";
      StringBuilder Options = new StringBuilder();
      var Rows = getDBRows(SQL);
      foreach (var row in Rows) {
        Options.Append("<option");
        if (YardID == row["YardID"].ToString()) Options.Append(" selected");
        Options.Append(" value=\"");
        Options.Append(row["YardID"].ToString());
        Options.Append("\">");
        Options.Append(row["YardName"].ToString());
        Options.AppendLine("</option>");
      }
      return Options.ToString();
    }



    public class GeoLocation {
      public Double Latitude { get; set; }
      public Double Longitude { get; set; }
    }


    public static string gEncode(IEnumerable<GeoLocation> points) {
      var str = new StringBuilder();

      var encodeDiff = (Action<int>)(diff => {
        int shifted = diff << 1;
        if (diff < 0)
          shifted = ~shifted;
        int rem = shifted;
        while (rem >= 0x20) {
          str.Append((char)((0x20 | (rem & 0x1f)) + 63));
          rem >>= 5;
        }
        str.Append((char)(rem + 63));
      });

      int lastLat = 0;
      int lastLng = 0;
      foreach (var point in points) {
        int lat = (int)Math.Round(point.Latitude * 1E5);
        int lng = (int)Math.Round(point.Longitude * 1E5);
        if(lat != lastLat && lng != lastLng) { 
          encodeDiff(lat - lastLat);
          encodeDiff(lng - lastLng);
        }
        lastLat = lat;
        lastLng = lng;
      }
      return str.ToString();
    }//static string gEncode

    public static String MD5(String HasStr) {

      // byte array representation of that string
      byte[] encodedPassword = new UTF8Encoding().GetBytes(HasStr);

      // need MD5 to calculate the hash
      byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

      // string representation (similar to UNIX format)
      string encoded = BitConverter.ToString(hash)
         // without dashes
         .Replace("-", string.Empty)
         // make lowercase
         .ToLower();
      return encoded;

    }


    public static T toEntityModel<T>(DbDataReader row) where T : new() {
      //reference: stackoverflow.com/questions/19286246/how-to-convert-dataset-to-entity-in-c
      var entity = new T();
      var properties = typeof(T).GetProperties();

      foreach (var property in properties) {
        //Get the description attribute
        var descriptionAttribute = (DescriptionAttribute)property.GetCustomAttributes(typeof(DescriptionAttribute), true).SingleOrDefault();
        if (descriptionAttribute == null)
          continue;

        property.SetValue(entity, row[descriptionAttribute.Description]);
      }

      return entity;
    }//toEntityModel

  }//class Util

}//eX_Portal.exLogic
