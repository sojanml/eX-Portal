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
using System.Threading.Tasks;
using System.Data.Entity;

namespace eX_Portal.Controllers {

  public class ReportController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Report
    public ActionResult Index() {
      return View();
    }


    public ActionResult Flights(FlightReportFilter ReportFilter) {
      if (!exLogic.User.hasAccess("REPORT.FLIGHTS"))
        return RedirectToAction("NoAccess", "Home");
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

    public ActionResult Alert(FlightReportFilter ReportFilter) {
      //if(!exLogic.User.hasAccess("REPORT.ALERT"))
      //  return RedirectToAction("NoAccess", "Home");
      var theReport = new exLogic.Report();
      qView nView = new qView(theReport.getAlertSQL(ReportFilter));
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        ViewBag.ReportFilter = ReportFilter;
        return View(nView);
      }//if(IsAjaxRequest)
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ReportFilter"></param>
    /// <returns></returns>
    public ActionResult GeoTag(FlightReportFilter ReportFilter) {
      if (!exLogic.User.hasAccess("REPORT.FLIGHTS"))
        return RedirectToAction("NoAccess", "Home");
      ViewBag.ReportFilter = ReportFilter;
      int DroneID = ReportFilter.UAS;
      int IsCompany = 0;
      DateTime FromDate = DateTime.Parse(ReportFilter.From);
      DateTime ToDate = DateTime.Parse(ReportFilter.To);

      ToDate = ToDate.AddHours(24);

      List<DroneDocument> Docs = new List<DroneDocument>();
      IList<GeoTagReport> DocsGeo = new List<GeoTagReport>();
      ExponentPortalEntities Db = new ExponentPortalEntities();

      if (!exLogic.User.hasAccess("DRONE.VIEWALL")) {
        IsCompany = 1;

      } else {
        IsCompany = 0;
      }
      DocsGeo = Util.getAllGeoTag(FromDate, ToDate, IsCompany, DroneID);
      return View(DocsGeo);
    }

    public ActionResult GeoReportFilter(FlightReportFilter ReportFilter) {
      return View(ReportFilter);
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




    public ActionResult AlertPDF(FlightReportFilter ReportFilter) {
      var myReport = new ReportData();
      LocalReport localReport = new LocalReport();

      ReportParameter[] TheParams = {
        new ReportParameter("ReportFilterInfo", ReportFilter.getReadableFilter(), false)
      };
      localReport.ReportPath = Server.MapPath("~/ReportsManager/AlertReport.rdlc");
      ReportDataSource reportDataSource = new ReportDataSource("AlertReportDataSet", myReport.getAlertReportData(ReportFilter));
      //localReport.SetParameters(TheParams);
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
      Response.AddHeader("content-disposition", "attachment; filename=AlertReport." + fileNameExtension);
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



    public String GoogleMap([Bind(Prefix = "ID")]int FlightID = 0, int ApprovalID = 0) {
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
      if (GeoPoints.Count < 1) {
        GoogleURL = "/images/world.jpg";
      } else {
        var FirstPoint = GeoPoints.First();
        GoogleURL = "https://maps.googleapis.com/maps/api/staticmap" +
        "?key=" + GoogleAPI +
        "&size=640x480" +
        /*getGoogleBoundary(FlightID, ApprovalID) +*/
        "&path=color:0xFF0000FF|weight:1|enc:" + gEncode(GeoPoints);
      }

      return GoogleURL;

    }//public ActionResult GoogleMap



    public String getGoogleBoundary([Bind(Prefix = "ID")]int FlightID = 0, int ApprovalID = 0) {
      /*select Coordinates, InnerBoundaryCoord  from GCA_Approval where droneid=50*/
      StringBuilder GoogleURL = new StringBuilder();
      List<GeoLocation>
        Inner = new List<GeoLocation>(),
        Outer = new List<GeoLocation>(),
        Polygon = new List<GeoLocation>();

      var GeoPoints = (
        from t1 in db.GCA_Approval
        where t1.ApprovalID == ApprovalID
        select new {
          Outer = t1.Polygon.AsText(),
          Inner = t1.InnerBoundary.AsText()
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
        //GoogleURL.Append("&path=color:0xF42D2DAA|weight:1|enc:" + gEncode(Inner));

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
          Latitude = Util.toDouble(LatLnt[1]),
          Longitude = Util.toDouble(LatLnt[0])
        };
        AllPoints.Add(thisCord);
      }//foreach

      return AllPoints;
    }

    public ActionResult PostFlightReport([Bind(Prefix = "ID")] int FlightID = 0) {
      //check if the PDF is generated
      string ReportPath = ConfigurationManager.AppSettings["ReportFolder"].ToString();
      // String FullPath1 = System.IO.Path.Combine("C:\\Reports", String.Format("{0}.pdf", FlightID));
      // String FullPath2 = System.IO.Path.Combine("C:\\Reports_DCAA", String.Format("{0}.pdf", FlightID));
      String FullPath3 = System.IO.Path.Combine(ReportPath, String.Format("{0}.pdf", FlightID));
      String FullPath = String.Empty;
      //if(System.IO.File.Exists(FullPath1)) {
      //  FullPath = FullPath1;
      //} else if(System.IO.File.Exists(FullPath2)) {
      //  FullPath = FullPath2;
      //}
      //else
      if (System.IO.File.Exists(FullPath3)) {
        FullPath = FullPath3;
      } else {
        ViewBag.Title = "Report not found";
        return View();
      }

      var ReturnData = System.IO.File.ReadAllBytes(FullPath);
      var ReturnType = System.Web.MimeMapping.GetMimeMapping(FullPath);
      var cd = new System.Net.Mime.ContentDisposition {
        FileName = "FlightReport-" + FlightID + DateTime.Now.ToString("-yyyymmddhhMMss") + ".pdf",
        Inline = false,
      };
      Response.AppendHeader("Content-Disposition", cd.ToString());
      return File(ReturnData, ReturnType);

    }

    public async Task<ActionResult> FlightReport([Bind(Prefix = "ID")]int FlightID = 0) {
      // if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
      ViewBag.FlightID = FlightID;

      var FlightData = await (
        from n in db.DroneFlight
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
          ApprovalID = n.ApprovalID
        }).FirstOrDefaultAsync();
      if (FlightData == null)
        return HttpNotFound();

      if (FlightData.FlightHours == null)
        FlightData.FlightHours = 0;
      FlightData.PilotName = await (
        from n in db.MSTR_User
        where n.UserId == FlightData.PilotID
        select n.FirstName + " " + n.LastName).FirstOrDefaultAsync();

      FlightData.GSCName = await (
        from n in db.MSTR_User
        where n.UserId == FlightData.GSCID
        select n.FirstName + " " + n.LastName).FirstOrDefaultAsync();

      FlightData.DroneName = await (
        from n in db.MSTR_Drone
        where n.DroneId == FlightData.DroneID
        select n.DroneName).FirstOrDefaultAsync();

      FlightData.PortalAlerts = await (
        from n in db.PortalAlerts
        where n.FlightID == FlightID
        select n).ToListAsync();


      var thisApproval =
        from n in db.GCA_Approval
        where n.ApprovalID == FlightData.ApprovalID
        select n;
      FlightData.Approval = await thisApproval.FirstOrDefaultAsync();

      //set Alert message for Report
      //setReportMessages(FlightData.PortalAlerts, FlightData.Approval);

      FlightData.MapData = await (
        from n in db.FlightMapDatas
        where n.FlightID == FlightID
        orderby n.FlightMapDataID
        select new LatLng{
          Lat = (Decimal)n.Latitude,
          Lng =(Decimal)n.Longitude
          }
        ).ToListAsync();

      FlightData.Videos = await (
        from n in db.DroneFlightVideos
        where n.FlightID == FlightID
        select n)
        .OrderBy(o => o.CreatedDate)
        .ToListAsync();


      FlightData.Approvals = await (
        from n in db.GCA_Approval
        where FlightData.FlightDate >= n.StartDate &&
              FlightData.FlightDate <= n.EndDate &&
              n.DroneID == FlightData.DroneID
        select n
      ).ToListAsync();

      FlightData.Info = await (
        from n in db.FlightInfoes
        where n.FlightID == FlightID
        select n).FirstOrDefaultAsync();
      if(FlightData.Info == null) {
        FlightData.Info = new Models.FlightInfo();
        LatLng FirstPoint = FlightData.MapData.FirstOrDefault();
        Exponent.WeatherAPI ReportWeather = new Exponent.WeatherAPI();      
        Exponent.WeatherForcast Condition = ReportWeather.GetByLocation((Double)FirstPoint.Lat, (Double)FirstPoint.Lng);
        FlightData.Info.Condition = Condition.Today.ConditionText;
        FlightData.Info.WindSpeed = Condition.Today.WindSpeed.ToString("0.0");
        FlightData.Info.Humidity = Condition.Today.Humidity.ToString("0");
        FlightData.Info.Visibility = (Decimal)Condition.Today.Visibility;
        FlightData.Info.Pressure = (Decimal)Condition.Today.Pressure;
        FlightData.Info.Temperature = Condition.Today.Temperature.ToString("0.0");
      }


      //Billing 
      int BillingGroupID = 1;
      BillingModule.BillingGroup grp = new BillingModule.BillingGroup(BillingGroupID);
      BillingModule.BillingNOC noc = new BillingModule.BillingNOC();
      Models.DroneFlight flight = await noc.LoadNocForFlight(FlightID);
      //await noc.GenerateFields();
      FlightData.Billing = await grp.GenerateBilling(noc, flight);


      return View(FlightData);
    }


    private void setReportMessages(IList<PortalAlert> Messages, GCA_Approval thisApproval) {

      foreach (var Message in Messages) {
        if (thisApproval != null) {
          var FlightInfo = (
            from f in db.FlightMapDatas
            where f.FlightMapDataID == Message.FlightDataID
            select f
          ).FirstOrDefault();
          switch (Message.AlertCategory) {
          case "Height":
            Message.AlertMessage = "RPAS is above proposed height of " + thisApproval.MaxAltitude + " Meter at " + Message.Altitude + " Meter";
            break;
          case "Boundary":
            Message.AlertCategory = "Perimeter";
            Message.AlertMessage = "RPAS is outside approved perimeter at " + fmtGPS((Double)Message.Latitude, (Double)Message.Longitude);
            break;
          case "Proximity":
            //Message.AlertMessage = getProximityMessage(FlightInfo.OtherFlightIDs, (int)FlightInfo.FlightID);
            Message.AlertMessage = "Proximity Error";
            break;

          }//switch
        } else {
          //nothing.
        }
      }//foreach
    }//private void setReportMessages

    private String getProximityMessage(String OtherFlightIDs, int FlightID) {
      StringBuilder SB = new StringBuilder();
      foreach (var OtherFlight in OtherFlightIDs.Split('|')) {
        var ThisInfo = OtherFlight.Split(',');
        var nFlightID = Util.toInt(ThisInfo[0]);
        var nLat = Util.toDouble(ThisInfo[1]);
        var nLng = Util.toDouble(ThisInfo[2]);
        var nDist = Util.toDouble(ThisInfo[3]);
        if (FlightID != nFlightID) {
          String TheMessage = "Flight " + nFlightID + " is close in proximity of " + nDist.ToString("###") + " Meter";
          if (SB.Length > 0)
            SB.Append(", ");
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
      var FlightData = (from n in db.DroneFlight
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
      "  DocumentType = 'Regulator Approval'\n";
      int TheCount = Util.getDBInt(SQL);
      if (TheCount < 1) {
        UploadedDocs = "<div class=\"warning\"><span class=\"icon\">&#xf071;</span>" +
        "Please upload your Regulatory Authorisation document before the flight</div>";
      } else {
        UploadedDocs = "<div class=\"authorise\"><span class=\"icon\">&#xf214;</span>" +
        "Your Regulatory Authorization: " + getUploadedDocs(FlightID) +
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
        if (DroneName == "")
          DroneName = Util.getDroneName(Doc.DroneID);
        theList.AppendLine("<LI><span class=\"icon\">&#xf0f6;</span> <a href=\"/upload/Drone/" + DroneName + "/" + FlightID +
        "/" + Doc.DocumentName + "\">" + Util.getFilePart(Doc.DocumentName) + "</a></LI>");
      }
      theList.Append("</UL>");
      return theList.ToString();
    }




    public FileContentResult GetChart([Bind(Prefix = "ID")]int FlightID = 0, String ChartType="all") {
      return File(Chart(FlightID, ChartType), "image/png");
    }


    private Byte[] Chart(int FlightID, String ChartType) {
      var cWidth = 700;
      var cHeight = 100;
      var chart = new Chart {
        Width = cWidth,
        Height = cHeight,
        RenderType = RenderType.ImageTag,
        AntiAliasing = AntiAliasingStyles.All,
        TextAntiAliasingQuality = TextAntiAliasingQuality.High,
        
      };

      chart.ChartAreas.Add("");
      chart.ChartAreas[0].BackColor = Color.White;

      chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Silver;
      chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Silver;
      chart.ChartAreas[0].AxisX.IsMarginVisible = false;
      chart.ChartAreas[0].AxisY.IsMarginVisible = false;
      chart.ChartAreas[0].AxisX2.IsMarginVisible = false;
      chart.ChartAreas[0].AxisY2.IsMarginVisible = false;
      chart.ChartAreas[0].Position.X = 0;
      chart.ChartAreas[0].Position.Width = 100;
      chart.ChartAreas[0].Position.Height = 100;
      chart.ChartAreas[0].Position.Y = 0;

      foreach(var FildType in new String[]{ "Altitude", "Pitch", "Roll", "Speed", "Satellite"}) { 
        chart.Series.Add(FildType);
        chart.Series[FildType].ChartType = SeriesChartType.Line;
        chart.Series[FildType].BorderWidth = 3;
      }
      switch (ChartType) {
      case "Ptich":
        chart.Series["Pitch"].Color = Color.FromArgb(255, 89, 0);
        break;
      case "Roll":
        chart.Series["Roll"].Color = Color.FromArgb(153, 131, 199);
        break;
      case "Speed":
        chart.Series["Speed"].Color = Color.FromArgb(11, 144, 118);
        break;
      case "Satellite":
        chart.Series["Satellite"].Color = Color.FromArgb(101, 186, 25);
        break;
      case "Altitude":
        chart.Series["Altitude"].Color = Color.FromArgb(219, 211, 1);
        chart.ChartAreas[0].AxisX.Minimum = -1;
        break;
      }
               

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
        String Label = fl[i].ReadTime.Value.ToString("HH:mm:ss");
        switch (ChartType) {
        case "Ptich":
          chart.Series["Pitch"].Points.AddXY(Label, fl[i].Pitch);
          break;
        case "Roll":
          chart.Series["Roll"].Points.AddXY(Label, fl[i].Roll);
          break;
        case "Speed":
          chart.Series["Speed"].Points.AddXY(Label, fl[i].Speed);
          break;
        case "Satellite":
          chart.Series["Satellite"].Points.AddXY(Label, fl[i].Satellites);
          break;
        case "Altitude":
          chart.Series["Altitude"].Points.AddXY(Label, fl[i].Altitude);
          break;
        }
      }
      using (var chartimage = new MemoryStream()) {
        // chart.RenderControl();
        chart.SaveImage(chartimage, ChartImageFormat.Png);
        //System.Drawing.Image returnImage = System.Drawing.Image.FromStream(chartimage);
        // returnImage.Save("samp.png");
        return chartimage.GetBuffer();
      }
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
    public String GoogleMapApproval([Bind(Prefix = "ID")]int DroneID = 0, int ApprovalID = 0) {
      String GoogleURL = String.Empty;
      var GoogleAPI = ConfigurationManager.AppSettings["GoogleAPI"];


      GoogleURL = "https://maps.googleapis.com/maps/api/staticmap" +
      "?key=" + GoogleAPI +
      "&size=640x400" +
      getGoogleBoundary(0, ApprovalID);


      return GoogleURL;

    }//public ActionResult GoogleMap
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