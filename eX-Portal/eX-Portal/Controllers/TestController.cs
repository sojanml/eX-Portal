using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.Text;
using eX_Portal.ViewModel;
using Microsoft.Reporting.WebForms;


namespace eX_Portal.Controllers {
  public class TestController : Controller {
    // GET: Test
    public ActionResult Index(int id = 9) {
      GeoGrid Info = new GeoGrid(id);
      return View(Info);
    }

    public ActionResult GCA_Approval(int id = 9) {
      ExponentPortalEntities DB = new ExponentPortalEntities();
      var Row = (from m in DB.GCA_Approval
                where m.ApprovalID == id
                select m).First();
      return View(Row);
    }

    public ActionResult Video() {
      return View();
    }

    public ActionResult CompareGraph(int From = 0, int To = 0) {
      String SQL =
      @"SELECT BasePoint.RFID,
        IsNull(BasePoint.Latitude,0) AS bLat,
        IsNull(BasePoint.Longitude,0) AS bLon,
        IsNull(ComparePoint.Latitude,0) AS cLat,
        IsNull(ComparePoint.Longitude,0) AS cLon
      FROM PayLoadMapData AS BasePoint
      LEFT JOIN PayLoadMapData AS ComparePoint
        ON ComparePoint.RFID = BasePoint.RFID
          AND ComparePoint.FlightUniqueID = (
            SELECT FlightUniqueID
            FROM PayLoadFlight
            WHERE PayLoadFlightID = " + To + @"
            )
      WHERE BasePoint.FlightUniqueID = (
          SELECT FlightUniqueID
          FROM PayLoadFlight
          WHERE PayLoadFlightID = " + From + @"
          )";
      ViewBag.Json = Util.getDBRowsJson(SQL);
      return View();
    }

    public ActionResult Analysis(String OrgLoc, String CmpLoc) {
      StringBuilder JSon = new StringBuilder();
      String SQL = @"Select
        OrgLoc.Latitude,
        OrgLoc.Longitude,
        OrgLoc.RFID,
        OrgLoc.RSSI,
        OrgLoc.ReadCount
      FROM
        PayLoadMapData  as OrgLoc
      WHERE
        OrgLoc.FlightUniqueID = '" + OrgLoc + "'";

      StringBuilder SB = new StringBuilder();
      StringBuilder sRow = new StringBuilder();
      List<Dictionary<String, Object>> Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        if (SB.Length > 0) SB.AppendLine(",");
        sRow.Clear();
        foreach (var Key in Row.Keys) {
          if (sRow.Length > 0) sRow.Append(", ");
          sRow.Append("\"");
          sRow.Append(Key);
          sRow.Append("\": ");
          if (!Util.IsNumber(Row[Key])) sRow.Append("\"");
          sRow.Append(Row[Key]);
          if (!Util.IsNumber(Row[Key])) sRow.Append("\"");
        }
        SB.Append("{");
        SB.Append(sRow);
        SB.Append(", \"Reads\": [");
        SB.Append(getReads(Row["RFID"].ToString(), CmpLoc));
        SB.Append("]}");
      }

      ViewBag.Json = SB.ToString();
      return View();




    }

    public ActionResult BlackBox() {
      BlackBox BB = new BlackBox();
      var theList = new List<List<BlackBoxCostCalucation>>();
      theList.Add(BB.getBlackBoxCost(10));
      theList.Add(BB.getBlackBoxCost(15));
      theList.Add(BB.getBlackBoxCost(20));

      return View(theList);
    }

    private String getReads(String RFID, String uKey) {
      String SQL = @"select
        Latitude,
        Longitude,
        RFID,
        RSSI,
        ReadCount
      FROM
        PayLoadData as OrgLocation
      WHERE
        FlightUniqueID = '" + uKey + @"' AND
        RFID='" + RFID + @"'
      ORDER BY
        CreatedTime";
     return Util.getDBRowsJson(SQL);
    }

    public ActionResult pdf() {
      var myReport = new ReportData();
      LocalReport localReport = new LocalReport();

      ReportParameter[] TheParams = {
        new ReportParameter("ReportFilterInfo", "Generate The Report From System", false)
      };
      localReport.ReportPath = Server.MapPath("~/ReportsManager/FlightReport.rdlc");
      ReportDataSource reportDataSource = new ReportDataSource("FlightReportData", myReport.getFlightReportData());
      localReport.SetParameters(TheParams);

      localReport.DataSources.Add(reportDataSource);
      string reportType = "PDF";
      string mimeType;
      string encoding;
      string fileNameExtension;

      //The DeviceInfo settings should be changed based on the reportType
      //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
      string deviceInfo =
      "<DeviceInfo>" +
      "  <OutputFormat>PDF</OutputFormat>" +
      "  <PageWidth>11.69in</PageWidth>" +
      "  <PageHeight>8.27in</PageHeight>" +
      "  <MarginTop>0.5in</MarginTop>" +
      "  <MarginLeft>0.5in</MarginLeft>" +
      "  <MarginRight>0.5in</MarginRight>" +
      "  <MarginBottom>0.5in</MarginBottom>" +
      "</DeviceInfo>";

      Warning[] warnings;
      string[] streams;
      byte[] renderedBytes;

      //Render the report
      renderedBytes = localReport.Render(
          reportType,
          deviceInfo,
          out mimeType,
          out encoding,
          out fileNameExtension,
          out streams,
          out warnings);
      //Response.AddHeader("content-disposition", "attachment; filename=NorthWindCustomers." + fileNameExtension);
      return File(renderedBytes, mimeType);
    }
  

    public ActionResult dt() {
      return View();
    }
  }

}