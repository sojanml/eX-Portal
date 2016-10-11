using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace eX_Portal.exLogic {

  public class NoFlyZoneMap {
    private int ID = 0;
    public List<FlyZone> NoFlyZone = new List<FlyZone>();
    public StringBuilder GetKML() {
      StringBuilder XML = new StringBuilder();
      XML.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
      XML.AppendLine(@"<kml xmlns=""http://earth.google.com/kml/2.1"">");
      XML.AppendLine("<Document>");
      XML.AppendLine(@"<name>Dubai No Fly Zone</name>
        <description>Zones</description>
      <Style id=""RedPoly"">
                <LineStyle> 
                        <color>55000000</color> 
                        <width>1</width> 
                </LineStyle>
        <PolyStyle>
          <color>550000ff</color>
          <colorMode>normal</colorMode>
          <fill>1</fill>
          <outline>1</outline>
        </PolyStyle>
      </Style>
      <Style id=""GreenPoly"">
                <LineStyle> 
                        <color>55000000</color> 
                        <width>1</width> 
                </LineStyle>
        <PolyStyle>
          <colorMode>normal</colorMode>
          <color>5500ff00</color>
          <fill>1</fill>
          <outline>1</outline>
        </PolyStyle>
      </Style>");

      foreach (var Zone in NoFlyZone) {
        XML.Append(Zone.GetKML(String.Format("Poly{0}", ID++)));
      }



      //Closing Document
      XML.AppendLine("</Document>");
      XML.AppendLine("</kml>");
      return XML;
    }
  }

  public class FlyZone {

    public List<MapPoint> Coordinates;
    public String FillColor;

    public FlyZone(String DefaultFillColor, String AllCoordinates) {
      FillColor = String.IsNullOrEmpty(DefaultFillColor) ?
        "#FF0000" :
        DefaultFillColor;
      Coordinates = new List<MapPoint>();
      foreach (var Point in AllCoordinates.Split(',')) {
        Double Lat = 0, Lng = 0;
        var LatLng = Point.Split(' ');
        Double.TryParse(LatLng[0], out Lat);
        Double.TryParse(LatLng[1], out Lng);
        Coordinates.Add(new MapPoint { Latitude = Lat, Longitude = Lng });
      }
    }

    public StringBuilder GetKML(String ID) {
      StringBuilder KML = new StringBuilder();
      String StyleURL = FillColor.Equals("Green") ? "GreenPoly" : "RedPoly";
      KML.Append(@"<Placemark>
          <name>No FlyZone</name>
          <visibility>1</visibility>
          <styleUrl>#" + StyleURL + @"</styleUrl>
          <Polygon id=""#" + ID + @""">
            <outerBoundaryIs>
              <LinearRing>
                <coordinates>");
      foreach (var Poly in Coordinates) {
        KML.AppendLine(String.Format("{1},{0},0", Poly.Latitude, Poly.Longitude));
      }
      //Add the first point again
      var FirstPoint = Coordinates.First();
      KML.AppendLine(String.Format("{1},{0},0", FirstPoint.Latitude, FirstPoint.Longitude));

      //Adding Lines
      KML.Append(@"</coordinates>
              </LinearRing>
            </outerBoundaryIs>
          </Polygon>
        </Placemark>");
      return KML;
    }
  }


}
