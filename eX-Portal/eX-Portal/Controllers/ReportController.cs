using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static eX_Portal.exLogic.Util;

namespace eX_Portal.Controllers {
  public class ReportController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();
    // GET: Report
    public ActionResult Index() {
      return View();
    }

    public String GoogleMap(int FlightID = 0) {
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

    public String Boundary(int FlightID = 0) {
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

  }//public class ReportController
}//namespace eX_Portal.Controllers