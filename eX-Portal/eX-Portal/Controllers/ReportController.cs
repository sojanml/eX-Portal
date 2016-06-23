using eX_Portal.exLogic;
using eX_Portal.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.Mvc;
using static eX_Portal.exLogic.Util;

using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using System.Web.UI.DataVisualization;
using System.Web.UI.DataVisualization.Charting;
using eX_Portal.ViewModel;
using System.Text;
using iTextSharp.tool.xml;
using Microsoft.Reporting.WebForms;

namespace eX_Portal.Controllers {
  public class ReportController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Report
    public ActionResult Index() {
      return View();
    }


    public ActionResult Flights(FlightReportFilter ReportFilter) {
      if (!exLogic.User.hasAccess("REPORT.FLIGHTS")) return RedirectToAction("NoAccess", "Home");
      var theReport = new exLogic.Report();            
      qView nView = new qView(theReport.getFlightReportSQL(ReportFilter));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        ViewBag.ReportFilter = ReportFilter;
        return View(nView);
      }//if(IsAjaxRequest)

    }
    /*
    public ActionResult Alerts(FlightReportFilter ReportFilter) {
      if (!exLogic.User.hasAccess("REPORT.ALERTS")) return RedirectToAction("NoAccess", "Home");
      var theReport = new exLogic.Report();
      qView nView = new qView(theReport.getAlertReportSQL(ReportFilter));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        ViewBag.ReportFilter = ReportFilter;
        return View(nView);
      }//if(IsAjaxRequest)

    }
    */


    public ActionResult FlightsPDF(FlightReportFilter ReportFilter) {
      var myReport = new ReportData();
      LocalReport localReport = new LocalReport();

      ReportParameter[] TheParams = {
        new ReportParameter("ReportFilterInfo", ReportFilter.getReadableFilter(), false)
      };
      localReport.ReportPath = Server.MapPath("~/ReportsManager/FlightReport.rdlc");
      ReportDataSource reportDataSource = new ReportDataSource("FlightReportData", myReport.getFlightReportData(ReportFilter));
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
      Response.AddHeader("content-disposition", "attachment; filename=FlightReport." + fileNameExtension);
      return File(renderedBytes, mimeType);
    }
    public ActionResult ReportFilter(FlightReportFilter ReportFilter) {
      return View(ReportFilter);
    }

    public String getPilots(String Term = "") {
      var theReport = new exLogic.Report();
      return theReport.getPilots(Term);
    }

    public String getUAS(String Term = "") {
      var theReport = new exLogic.Report();
      return theReport.getUAS(Term);
    }


    public String GoogleMap([Bind(Prefix = "ID")]int FlightID = 0, int ApprovalID=0) {
      String GoogleURL = String.Empty;
      var GoogleAPI = ConfigurationManager.AppSettings["GoogleAPI"];
      var GeoPoints = (from n in db.FlightMapDatas
                       where n.FlightID == FlightID 
                       orderby n.FlightMapDataID
                       select new GeoLocation {
                         Latitude = (Double)n.Latitude,
                         Longitude = (Double)n.Longitude
                       }
                    ).ToList();
      if(GeoPoints.Count < 1) {
        GoogleURL = "/images/world.jpg";
      } else { 
        var FirstPoint = GeoPoints.First();
        GoogleURL = "https://maps.googleapis.com/maps/api/staticmap" +
        "?key=" + GoogleAPI + 
        "&center=" + FirstPoint.Latitude + "," + FirstPoint.Longitude +
        "&size=640x400" +
        getGoogleBoundary(FlightID, ApprovalID) +
        "&path=color:0x000000|weight:1|enc:" + gEncode(GeoPoints);
      }

      return GoogleURL;

    }//public ActionResult GoogleMap

    public String getGoogleBoundary([Bind(Prefix = "ID")]int FlightID = 0, int ApprovalID=0) {
      /*select Coordinates, InnerBoundaryCoord  from GCA_Approval where droneid=50*/
      StringBuilder GoogleURL = new StringBuilder();
      List<GeoLocation>
        Inner = new List<GeoLocation>(),
        Outer = new List<GeoLocation>(),
        Polygon = new List<GeoLocation>();

      var GeoPoints = (
        from t1 in db.GCA_Approval
        join t2 in db.DroneFlights on t1.DroneID equals t2.DroneID
        where t2.ID == FlightID &&
        t2.FlightDate >= t1.StartDate &&
        t2.FlightDate <= t1.EndDate &&
        t1.ApprovalID == ApprovalID
        select new {
          Outer = t1.Polygon.AsText(),
          Inner = t1.InnerBoundaryCoord
        });

      foreach (var Row in GeoPoints.ToList()) {
        Outer = toPoints(Row.Outer);
        Inner = toPoints(Row.Inner);
        //Inner.RemoveAt(Inner.Count - 1);

        //Polygon.Add(Outer.FirstOrDefault());
        //Polygon.AddRange(Inner));
        for (var i = Inner.Count - 1; i >= 0; i--) {
          Polygon.Add(Inner[i]);
        }
        Polygon.AddRange(Outer);


        GoogleURL.Append("&path=fillcolor:0xFF9B5255|weight:0|enc:" + gEncode(Polygon));
        GoogleURL.Append("&path=fillcolor:0x5AD74655|weight:0|enc:" + gEncode(Outer));
        GoogleURL.Append("&path=color:0xF42D2DAA|weight:1|enc:" + gEncode(Inner));

      }

      //remove the last point from Inner Polygon

      return GoogleURL.ToString();

    }//public ActionResult GoogleMap

    private List<GeoLocation> toPoints(String PointList) {
      var AllPoints = new List<GeoLocation>();
      PointList = PointList.Replace("POLYGON ((", "");
      PointList = PointList.Replace("))", "");

      foreach (String Cord in PointList.Split(',')) {
        var LatLnt = Cord.Trim().Split(' ');
        var thisCord = new GeoLocation {
          Latitude = Util.toDouble(LatLnt[0]),
          Longitude = Util.toDouble(LatLnt[1])
        };
        AllPoints.Add(thisCord);
      }//foreach

      return AllPoints;
    }
    public ActionResult FlightReport([Bind(Prefix = "ID")]int FlightID = 0) {
      // if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      ViewBag.FlightID = FlightID;

      String SQL = @"
      SELECT TOP 1
       GCA_Approval.ApprovalID,
       GCA_Approval.MaxAltitude,
       GCA_Approval.MinAltitude,
       GEOGRAPHY::Point((
         SELECT TOP 1 Cast(Longitude AS VARCHAR)
         FROM FlightMapData
         WHERE FlightID = " + FlightID + @"
         ORDER BY CreatedTime DESC
         ), (
         SELECT TOP 1 Cast(Latitude AS VARCHAR)
         FROM FlightMapData
         WHERE FlightID = " + FlightID + @"
         ORDER BY CreatedTime DESC
         ), 4326).STIntersects(InnerBoundary) AS IsInside
      FROM GCA_Approval
      WHERE DroneID = (
        SELECT DroneID
        FROM DroneFlight
        WHERE ID = " + FlightID + @"
        )
      ORDER BY
        IsInside DESC
      ";
      var thisApproval = Util.getDBRow(SQL);

      var FlightData = (
        from n in db.DroneFlights
        where n.ID == FlightID
        select new FlightViewModel {
          ID = n.ID,
          PilotID = n.PilotID,
          GSCID = n.GSCID,
          FlightDate = n.FlightDate,
          FlightHours = n.FlightHours,
          FlightDistance = n.FlightDistance,
          DroneID = n.DroneID,
          CreatedOn = n.CreatedOn,
        }).ToList().FirstOrDefault();
      if (FlightData.FlightHours == null) FlightData.FlightHours = 0;
      FlightData.PilotName = (
        from n in db.MSTR_User
        where n.UserId == FlightData.PilotID
        select n.FirstName + " " +  n.LastName).FirstOrDefault();

      FlightData.GSCName = (
        from n in db.MSTR_User
        where n.UserId == FlightData.GSCID
        select n.FirstName + " " + n.LastName).FirstOrDefault();

      FlightData.DroneName = (
        from n in db.MSTR_Drone
        where n.DroneId == FlightData.DroneID
        select n.DroneName).FirstOrDefault();

      FlightData.PortalAlerts = (
        from n in db.PortalAlerts
        where n.FlightID == FlightID
        select n).ToList();

      //set Alert message for Report
      setReportMessages(FlightData.PortalAlerts, thisApproval);

      FlightData.Videos = (
        from n in db.DroneFlightVideos
        where n.FlightID == FlightID
        select n)
        .OrderBy(o => o.CreatedDate)
        .ToList();


      FlightData.Approvals = (
        from n in db.GCA_Approval
        where FlightData.FlightDate >= n.StartDate &&
              FlightData.FlightDate <= n.EndDate &&
              n.DroneID == FlightData.DroneID
        select n
      ).ToList();

      FlightData.Info = (
        from n in db.FlightInfoes
        where n.FlightID == FlightID
        select n).FirstOrDefault();


      if ((bool)thisApproval["hasRows"])
        ViewBag.ApprovalID = ((bool)thisApproval["IsInside"] ? thisApproval["ApprovalID"] : 0);
     return View(FlightData);
    }


    private void setReportMessages(IList<PortalAlert> Messages, Dictionary<String, Object> thisApproval) {
      
      foreach(var Message in Messages) {
      if((bool)thisApproval["hasRows"]) { 
        var FlightInfo = (
          from f in db.FlightMapDatas
          where f.FlightMapDataID == Message.FlightDataID
          select f
        ).FirstOrDefault();
        switch (Message.AlertCategory)  {
        case "Height":
          Message.AlertMessage = "UAS is above proposed height of " + thisApproval["MaxAltitude"] + " Meter at " + Message.Altitude + " Meter";
        break;
        case "Boundary":
          Message.AlertCategory = "Perimeter";
          Message.AlertMessage = "UAS is outside approved perimeter at " + fmtGPS((Double)Message.Latitude, (Double)Message.Longitude);
        break;
        case "Proximity":
          Message.AlertMessage = getProximityMessage(FlightInfo.OtherFlightIDs, (int)FlightInfo.FlightID);
        break;

        }//switch
        } else {
          //nothing.
        }
      }//foreach
    }//private void setReportMessages

    private String getProximityMessage(String OtherFlightIDs, int FlightID) {
      StringBuilder SB = new StringBuilder();
      foreach(var OtherFlight in OtherFlightIDs.Split('|')) {
        var ThisInfo = OtherFlight.Split(',');
        var nFlightID = Util.toInt(ThisInfo[0]);
        var nLat = Util.toDouble(ThisInfo[1]);
        var nLng = Util.toDouble(ThisInfo[2]);
        var nDist = Util.toDouble(ThisInfo[3]);
        if(FlightID != nFlightID) {
          String TheMessage = "Flight " + nFlightID + " is close in proximity of " + nDist.ToString("###") + " Meter";
          if (SB.Length > 0) SB.Append(", ");
          SB.Append(TheMessage);
        }
      }
      return SB.ToString();
    }

    private String fmtGPS(Double Lat, Double Lng) {
      StringBuilder TheStr = new StringBuilder();
      TheStr.Append(Lat.ToString("###.###"));
      TheStr.Append(Lat >= 0 ? "N" : "S");
      TheStr.Append(" ");
      TheStr.Append(Lng.ToString("###.###"));
      TheStr.Append(Lng >= 0 ? "E" : "W");
      return TheStr.ToString();
    }

    public ActionResult FlightPDF([Bind(Prefix = "ID")]int FlightID = 0) {
      var FlightData = (from n in db.DroneFlights
                        where n.ID == FlightID
                        select new FlightViewModel {
                          ID = n.ID,
                          PilotID = n.PilotID,
                          GSCID = n.GSCID,
                          FlightDate = n.FlightDate,
                          FlightHours = n.FlightHours
                        }).ToList();
      int userid = FlightData[0].PilotID.Value;
      string Pilotname = (from n in db.MSTR_User
                          where n.UserId == userid
                          select n.FirstName).FirstOrDefault();
      userid = FlightData[0].GSCID.Value;
      string GSCname = (from n in db.MSTR_User
                        where n.UserId == userid
                        select n.FirstName).FirstOrDefault();
      FlightData[0].PilotName = Pilotname;
      FlightData[0].GSCName = GSCname;
      return Pdf("test", "FlightReport", FlightData[0]);
    }

    public string DroneCheckWarning([Bind(Prefix = "ID")]int FlightID = 0) {
      String CheckListMessage = "";

      //int DroneId, UserId;

      //string UASFormat, PilotFormat;
      String UploadedDocs = "";

      String SQL = @"
      SELECT 
        Count([DroneCheckList].[ID]) as FlightCheckList
      FROM 
        [DroneCheckList]
      LEFT JOIN [MSTR_DroneCheckList] ON
        [MSTR_DroneCheckList].ID = [DroneCheckListID]
      WHERE
        [MSTR_DroneCheckList].[CheckListTitle]='Pre-Flight Checklist' AND
        [DroneCheckList].[FlightID]=" + FlightID;
      int CheckListCount = Util.getDBInt(SQL);
      if (CheckListCount >= 3) {
        CheckListMessage = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "CheckList Completed</div>";
      } else if (CheckListCount >= 1) {
        CheckListMessage = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "CheckList Incomplete</div>";
      } else {
        CheckListMessage = "<div class=\"invalid\"><span class=\"icon\">&#xf071;</span>" +
        "No CheckList</div>";
      }

      //Check the documents for GCA is uploaded
      SQL = "SELECT\n" +
      "  Count(*)\n" +
      "FROM\n" +
      "  [DroneDocuments]\n" +
      "WHERE\n" +
      "  FlightID = " + FlightID.ToString() + " and\n" +
      "  DocumentType = 'GCA Approval'\n";
      int TheCount = Util.getDBInt(SQL);
      if (TheCount < 1) {
        UploadedDocs = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "Please upload your DCAA Authorisation document before the flight</div>";
      } else {
        UploadedDocs = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "Your DCAA Authorization: " + getUploadedDocs(FlightID) +
        "</div>";
      }
      return UploadedDocs + CheckListMessage;
    }
    private String getUploadedDocs(int FlightID) {
      StringBuilder theList = new StringBuilder();
      String DroneName = "";
      ExponentPortalEntities db = new ExponentPortalEntities();
      List<DroneDocument> Docs = (from r in db.DroneDocuments
                                  where (int)r.FlightID == FlightID
                                  select r).ToList();
      theList.Append("<UL>");
      foreach (var Doc in Docs) {
        if (DroneName == "") DroneName = Util.getDroneName(Doc.DroneID);
        theList.AppendLine("<LI><span class=\"icon\">&#xf0f6;</span> <a href=\"/upload/Drone/" + DroneName + "/" + FlightID +
        "/" + Doc.DocumentName + "\">" + Util.getFilePart(Doc.DocumentName) + "</a></LI>");
      }
      theList.Append("</UL>");
      return theList.ToString();
    }


    private Byte[] Chart(int FlightID) {
      var query = from o in db.FlightMapDatas
                  where o.FlightID == FlightID
                  orderby o.FlightMapDataID
                  select new {
                    Altitude = o.Altitude,
                    Roll = o.Roll,
                    Satellites = o.Satellites,
                    Speed = o.Speed,
                    ReadTime = o.ReadTime,
                    Pitch = o.Pitch
                  };


      var chart = new Chart {
        Width = 1000,
        Height = 220,
        RenderType = RenderType.ImageTag,
        AntiAliasing = AntiAliasingStyles.All,
        TextAntiAliasingQuality = TextAntiAliasingQuality.High
      };


      //chart.Titles.Add("Values");
      //chart.Titles[0].Font = new Font("Arial", 16f);


      chart.ChartAreas.Add("");
      //chart.ChartAreas[0].AxisX.Title = "Time";
      //chart.ChartAreas[0].AxisY.Title = "Value";
      //chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
      //chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
      //chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
      //chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
      chart.ChartAreas[0].BackColor = Color.White;
      chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Silver;
      chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Silver;


      String[] Series = { "Altitude", "Roll", "Pitch", "Satellites", "Speed" };
      foreach(var Item in Series) {
        chart.Series.Add(Item);
        chart.Series[Item].ChartType = SeriesChartType.Line;
        chart.Series[Item].BorderWidth = 2;
      }

      chart.Series["Altitude"].Color = Color.DarkRed;
      chart.Series["Speed"].Color = Color.DeepPink;
      chart.Series["Roll"].Color = Color.DarkTurquoise;
      chart.Series["Pitch"].Color = Color.Coral;
      chart.Series["Satellites"].Color = Color.DarkViolet;

      IList<FlightMapData> fl = query.ToList()
                               .Select(x => new FlightMapData() {
                                 Altitude = x.Altitude,
                                 Roll = x.Roll,
                                 Pitch = x.Pitch,
                                 Satellites = x.Satellites,
                                 Speed = x.Speed,
                                 ReadTime = x.ReadTime
                               }).ToList();
      for (int i = 0; i < fl.Count; i++) {
        String Label = fl[i].ReadTime.Value.ToString("HH:mm");
        chart.Series["Altitude"].Points.AddXY(Label, fl[i].Altitude);
        chart.Series["Speed"].Points.AddXY(Label, fl[i].Speed);
        chart.Series["Roll"].Points.AddXY(Label, fl[i].Roll);
        chart.Series["Pitch"].Points.AddXY(Label, fl[i].Pitch);
        chart.Series["Satellites"].Points.AddXY(Label, fl[i].Satellites); 
      }
      using (var chartimage = new MemoryStream()) {
        // chart.RenderControl();
        chart.SaveImage(chartimage, ChartImageFormat.Png);
        //System.Drawing.Image returnImage = System.Drawing.Image.FromStream(chartimage);
        // returnImage.Save("samp.png");
        return chartimage.GetBuffer();
      }
    }


    public FileContentResult GetChart([Bind(Prefix = "ID")]int FlightID = 0) {
      return File(Chart(FlightID), "image/png");
    }

    protected ActionResult Pdf() {
      return Pdf(null, null, null);
    }

    protected ActionResult Pdf(string fileDownloadName) {
      return Pdf(fileDownloadName, null, null);
    }

    public ActionResult Pdf(string fileDownloadName, string viewName) {
      return Pdf(fileDownloadName, viewName, null);
    }

    protected ActionResult Pdf(object model) {
      return Pdf(null, null, model);
    }

    protected ActionResult Pdf(string fileDownloadName, object model) {
      return Pdf(fileDownloadName, null, model);
    }

    protected ActionResult Pdf(string fileDownloadName, string viewName, object model) {
      // Based on View() code in Controller base class from MVC
      if (model != null) {
        ViewData.Model = model;
      }
      PdfResult pdf = new PdfResult() {
        FileDownloadName = fileDownloadName,
        ViewName = viewName,
        ViewData = ViewData,
        TempData = TempData,
        ViewEngineCollection = ViewEngineCollection
      };
      return pdf;
    }
  }//public class ReportController

  public class PdfResult : PartialViewResult {
    // Setting a FileDownloadName downloads the PDF instead of viewing it
    public string FileDownloadName { get; set; }

    public override void ExecuteResult(ControllerContext context) {
      if (context == null) {
        throw new ArgumentNullException("context");
      }

      // Set the model and data
      context.Controller.ViewData.Model = Model;
      ViewData = context.Controller.ViewData;
      TempData = context.Controller.TempData;


      // Get the view name
      if (string.IsNullOrEmpty(ViewName)) {
        ViewName = context.RouteData.GetRequiredString("action");
      }

      // Get the view
      ViewEngineResult viewEngineResult = null;
      if (View == null) {
        viewEngineResult = FindView(context);
        View = viewEngineResult.View;
      }

      // Render the view
      StringBuilder sb = new StringBuilder();
      using (TextWriter tr = new StringWriter(sb)) {
        ViewContext viewContext = new ViewContext(context, View, ViewData, TempData, tr);
        View.Render(viewContext, tr);
      }
      if (viewEngineResult != null) {
        viewEngineResult.ViewEngine.ReleaseView(context, View);
      }

      // Create a PDF from the rendered view content

      var workStream = new MemoryStream();
      var document = new Document();
      PdfWriter writer = PdfWriter.GetInstance(document, workStream);
      writer.CloseStream = false;
      document.Open();
      Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

      XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, stream, Encoding.UTF8);
      document.Close();

      // Save the PDF to the response stream
      FileContentResult result = new FileContentResult(workStream.ToArray(), "application/pdf") {
        FileDownloadName = FileDownloadName
      };

      result.ExecuteResult(context);
    }
  }
}//namespace eX_Portal.Controllers