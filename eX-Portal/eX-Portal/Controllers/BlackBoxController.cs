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
      return View();
    }//Index()

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
      Value = Util.toSQL(Value);
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