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

namespace eX_Portal.Controllers {
  public class ReportController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Report
    public ActionResult Index() {
      return View();
    }

    public String GoogleMap([Bind(Prefix = "ID")]int FlightID = 0) {
      var GoogleAPI = ConfigurationManager.AppSettings["GoogleAPI"];
      var GeoPoints = (from n in db.FlightMapDatas
                       where n.FlightID == FlightID
                       orderby n.FlightMapDataID
                       select new GeoLocation { 
                         Latitude = (Double)n.Latitude,
                         Longitude = (Double)n.Longitude }                                             
                    ).ToList();
      var FirstPoint = GeoPoints.First();
      var GoogleURL = "https://maps.googleapis.com/maps/api/staticmap" +
      "?center=" + FirstPoint.Latitude + "," + FirstPoint.Longitude +
      "&zoom=15&size=640x400" +
      "&path=color:0x0000ff|weight:2|enc:" + gEncode(GeoPoints);

      return GoogleURL;

    }//public ActionResult GoogleMap

    public String Boundary([Bind(Prefix = "ID")]int FlightID = 0) {
      /*select Coordinates, InnerBoundaryCoord  from GCA_Approval where droneid=50*/
      List<GeoLocation> 
        Inner = new List<GeoLocation>(), 
        Outer = new List<GeoLocation>(), 
        Polygon = new List<GeoLocation>();

      var GeoPoints = (from t1 in db.GCA_Approval
                       join t2 in db.DroneFlights on t1.DroneID equals t2.DroneID
                       where t2.ID == FlightID
                       select new {
                         Outer = t1.Polygon.AsText(),
                         Inner = t1.InnerBoundaryCoord
                       }).ToList();

      foreach(var Row in GeoPoints) {
        Outer = toPoints(Row.Outer);
        Inner = toPoints(Row.Inner);
        //Inner.RemoveAt(Inner.Count - 1);

        //Polygon.Add(Outer.FirstOrDefault());
        //Polygon.AddRange(Inner));
        for(var i = Inner.Count - 1; i >= 0; i--) {
          Polygon.Add(Inner[i]);
        }
        Polygon.AddRange(Outer);

        break;

      }
      var FirstPoint = Outer.FirstOrDefault();

      var GoogleURL = "https://maps.googleapis.com/maps/api/staticmap" +
      "?center=" + FirstPoint.Latitude + "," + FirstPoint.Longitude +
      "&zoom=15&size=640x400" +
      "&path=color:0x0000ff|weight:2|enc:" + gEncode(Polygon);

      //remove the last point from Inner Polygon


      return GoogleURL;

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
        public ActionResult FlightReport([Bind(Prefix = "ID")]int FlightID = 0)
        {
            // if (!exLogic.User.hasAccess("FLIGHT.MAP")) return RedirectToAction("NoAccess", "Home");
            ViewBag.FlightID = FlightID;
            var GeoPoints = (from n in db.FlightMapDatas
                             where n.FlightID == FlightID
                             orderby n.FlightMapDataID
                             select new GeoLocation
                             {
                                 Latitude = (Double)n.Latitude,
                                 Longitude = (Double)n.Longitude
                             }).ToList();

            var FlightData = (from n in db.DroneFlights
                              where n.ID == FlightID
                              select new FlightViewModel
                              {
                                  ID = n.ID,
                                  PilotID = n.PilotID,
                                  GSCID = n.GSCID,
                                  FlightDate = n.FlightDate,
                                  FlightHours = n.FlightHours
                              }).ToList();


            return View();
        }

        public FilePathResult GetPdf()
        {
            var doc = new Document();
            var pdf = Server.MapPath("");

            PdfWriter.GetInstance(doc, new FileStream(pdf, FileMode.Create));
            doc.Open();

            doc.Add(new Paragraph("Dashboard"));
            var image = Image.GetInstance(Chart());
            image.ScalePercent(75f);
            doc.Add(image);
            doc.Close();

            return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] Chart()
        {
            var query = from o in db.FlightMapDatas
                        where o.FlightID == 284
                        orderby o.FlightMapDataID
                        select new
                        {
                            Altitude = o.Altitude,
                            Roll = o.Roll,
                            Satellites = o.Satellites,
                            Speed = o.Speed,
                            ReadTime = o.ReadTime
                        };


            var chart = new Chart
            {
                Width = 1000,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Values");
            chart.Titles[0].Font = new Font("Arial", 16f);


            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Value";
            chart.ChartAreas[0].AxisY.Title = "ReadTime";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
            chart.ChartAreas[0].BackColor = Color.White;

            chart.Series.Add("Altitude");
            chart.Series.Add("Speed");

            chart.Series[0].ChartType = SeriesChartType.Line;
            chart.Series[1].ChartType = SeriesChartType.Line;
            IList<FlightMapData> fl = query.ToList()
                                     .Select(x => new FlightMapData()
                                     {
                                         Altitude = x.Altitude,
                                         Roll = x.Roll,
                                         Satellites = x.Satellites,
                                         Speed = x.Speed,
                                         ReadTime = x.ReadTime
                                     }).ToList();
            for (int i = 0; i < fl.Count; i++)
            {
                chart.Series["Altitude"].Points.AddXY(fl[i].ReadTime, fl[i].Altitude);
                chart.Series["Speed"].Points.AddXY(fl[i].ReadTime, fl[i].Speed);
            }
            using (var chartimage = new MemoryStream())
            {
                // chart.RenderControl();
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                //System.Drawing.Image returnImage = System.Drawing.Image.FromStream(chartimage);
                // returnImage.Save("samp.png");
                return chartimage.GetBuffer();
            }
        }


        public FileContentResult GetChart([Bind(Prefix = "ID")]int FlightID = 0)
        {
            return File(Chart(), "image/png");
        }

    }//public class ReportController
}//namespace eX_Portal.Controllers