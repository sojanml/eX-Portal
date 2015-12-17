using eX_Portal.exLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class BlackBoxController : Controller {
    static String RootUploadDir = "~/Upload/BlackBox/";
    // GET: BlackBox

    public ActionResult Index() {
      //if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");

      ViewBag.Title = "Black Box Data";

      String SQL = "SELECT \n" +
        "  MSTR_Drone.DroneName,\n" +
        "  BBFlightID,\n" +
        "  Min([BlackBoxData].ReadTime) as StartTime,\n" +
        "  Max([BlackBoxData].ReadTime) as EndTime,\n" +
        "  Max(Speed) as MaxSpeed,\n" +
        "  Max(TotalFlightTime) as FlightTime,\n" +
        "  Count(*) Over() as _TotalRecords,\n"+
        "  Cast(MSTR_Drone.DroneId as varchar) + ',' + Cast(BBFlightID as varchar) as _Pkey\n" +
        "FROM\n" +
        "  [BlackBoxData]\n" +
        "LEFT JOIN MSTR_Drone ON\n" +
        "  MSTR_Drone.DroneId = [BlackBoxData].DroneId\n" +
        "WHERE\n" +
        "  Speed > 0.00\n" +
        "GROUP BY\n" +
        "  MSTR_Drone.DroneID,\n" +
        "  MSTR_Drone.DroneName,\n" +
        "  BBFlightID\n";

      qView nView = new qView(SQL);
      nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }//Index()


        public ActionResult Live()
        {
            //if (!exLogic.User.hasAccess("BLACKBOX.VIEW")) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = "Black Box Data";

            string SQL = "SELECT [DroneDataId] " +
     ", [DroneId]" +
      ",[DroneRFID]" +
      ",[Latitude]" +
     " ,[Longitude]" +
     " ,[ProductRFID]" +
     " ,[ProductQrCode]" +
     " ,[ProductRSSI]" +
      ",[ReadTime]" +
      ",[CreatedTime]" +
      ",[RecordType]" +
      ",[IsActive]" +
      ",[ProductId]" +
      ",[Altitude]" +
      ",[Speed]" +
     " ,[FixQuality]" +
     " ,[Satellites]" +
      ",[Pitch]" +
      ",[Roll]" +
     " ,[Heading]" +
      ",[TotalFlightTime]" +
     " ,[BBFlightID]" +
      ",[IsProcessed]" +
     " ,[QueueMessage],Count(*) Over() as _TotalRecords,[DroneDataId] as _PKey" +
  " FROM  [DroneData]   ";

          

            qView nView = new qView(SQL);
            //nView.addMenu("Detail", Url.Action("Detail", new { ID = "_Pkey" }));

            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }//Index()
        public ActionResult Detail([Bind(Prefix = "ID")] String DroneID_BBFlightID = "") {
      String[] SplitData = DroneID_BBFlightID.Split(',');
      if (SplitData.Length != 2) return RedirectToAction("Error");
      int DroneID = Util.toInt(SplitData[0]);
      int BBFlightID = Util.toInt(SplitData[1]);
      if(DroneID < 1 || BBFlightID < 1) return RedirectToAction("Error");
      ViewBag.Title = "Blackbox  Data";
      ViewBag.DroneID = DroneID;

      String SQL =
       "SELECT \n" +
       "  RecordNumber,\n"+
       "  ReadTime,\n" +
       "  Latitude,\n" +
       "  Longitude,\n" +
       "  Altitude,\n" +
       "  Speed,\n" +
       "  FixQuality,\n" +
       "  Satellites,\n" +
       "  Pitch,\n" +
       "  Roll,\n" +
       "  Heading,\n" +
       "  TotalFlightTime,\n" +
       "  Count(*) OVER() as _TotalRecords\n" +
       "FROM\n" +
       "  BlackBoxData\n" +
       "WHERE\n" +
       "  DroneID=" + DroneID + " AND\n" +
       "  BBFlightID=" + BBFlightID;

      qView nView = new qView(SQL);

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    public ActionResult Upload() {
      ViewBag.Title = "Upload";
      return View();
    }//upload()



    public String Delete([Bind(Prefix = "file")] String FileName) {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      StringBuilder JsonText = new StringBuilder();
      String FullName = UploadPath + FileName;

      Response.ContentType = "text/json";
      try {
        System.IO.File.Delete(FullName);
        JsonText.Append("{");
        JsonText.Append(this.Pair("status", "ok", true));
        JsonText.Append(this.Pair("message", "Deleted", false));
        JsonText.Append("}");
      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(this.Pair("status", "error", true));
        JsonText.Append(this.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }

    public String Save() {
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      //send information in JSON Format always
      StringBuilder JsonText = new StringBuilder();
      Response.ContentType = "text/json";

      //when there are files in the request, save and return the file information
      try {
        var TheFile = Request.Files[0];
        String FullName = UploadPath + TheFile.FileName;

        if (!Directory.Exists(UploadPath)) Directory.CreateDirectory(UploadPath);
        TheFile.SaveAs(FullName);
        JsonText.Append("{");
        JsonText.Append(this.Pair("status", "success", true));
        JsonText.Append("\"addFile\":[");
        JsonText.Append(getFileInfo(FullName));
        JsonText.Append("]}");
      } catch (Exception ex) {
        JsonText.Clear();
        JsonText.Append("{");
        JsonText.Append(this.Pair("status", "error", true));
        JsonText.Append(this.Pair("message", ex.Message, false));
        JsonText.Append("}");
      }//catch
      return JsonText.ToString();
    }//Save()

    public String getFiles() {
      StringBuilder JsonText = new StringBuilder();
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      JsonText.Append("{");
      JsonText.Append(this.Pair("status", "success", true));
      JsonText.Append("\"addFile\":[");
      bool isAddComma = false;
      foreach (string file in Directory.EnumerateFiles(UploadPath, "*.*")) {
        if (isAddComma) JsonText.Append(",\n");
        JsonText.Append(getFileInfo(file));
        isAddComma = true;
      }
      JsonText.Append("]}");
      return JsonText.ToString();
    }//getFiles()

    public String Import(String file) {
      String Status = "ok";
      String StatusMessage = "Completed Successfully";
      String UploadPath = Server.MapPath(Url.Content(RootUploadDir));
      BlackBox Process = new BlackBox();
      StringBuilder JsonText = new StringBuilder();
      int ImportedRows = 0;
      //String newFile = Process.FixImportFile(UploadPath + file);
      try {
        ImportedRows = Process.BulkInsert(UploadPath + file);
        StatusMessage = "Imported " + ImportedRows + " Rows";
      } catch(Exception ex) {
        Status = "error";
        StatusMessage = ex.Message;
      }

      JsonText.AppendLine("{");
      JsonText.AppendLine(Pair("status", Status, true));
      JsonText.AppendLine(Pair("message", StatusMessage, false));
      JsonText.AppendLine("}");

      return JsonText.ToString();
    }

    private string Pair(String Name, String Value, bool IsAddComma = false) {
      Value = Value.Replace("\\", "\\\\") ;
      Value = Value.Replace("\r\n", "\\n");
      return "\"" + Name + "\" : \"" + Value + "\"" + (IsAddComma ? "," : "") + "\n";
    }

    private String getFileInfo(String FileName) {

      FileInfo oFileInfo = new FileInfo(FileName);
      return "{" +
        Pair("name", oFileInfo.Name, true) +
        Pair("created", oFileInfo.CreationTime.ToString("dd-MMM-yyyy hh:mm:ss tt [zzz]"), true) +
        Pair("ext", oFileInfo.Extension, true) +
        Pair("records", getLineCount(FileName).ToString(), true) +
        Pair("size", (oFileInfo.Length / 1024).ToString("N0"), false) +
        "}";
    }//getFileInfo()

    private int getLineCount(String FileName) {
      var lineCount = 0;
      using (var reader = System.IO.File.OpenText(FileName)) {
        while (reader.ReadLine() != null) {
          lineCount++;
        }//while
      }//using
      return lineCount;
    }//getLineCount()

  }//class
}//namespace