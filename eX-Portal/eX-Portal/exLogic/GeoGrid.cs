﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {

  public class MapPoint {
    public double Longitude { get; set; } // In Degrees
    public double Latitude { get; set; } // In Degrees
  }
  
  public class GeoBox {
    public MapPoint TopLeft { get; set; }
    public MapPoint TopRight { get; set; }
    public MapPoint BottomLeft { get; set; }
    public MapPoint BottomRight { get; set; }
  }

  public class GeoGrid {
    private int _YardID = 0;
    public GeoGrid(int _pYardID = 0) {
      _YardID = _pYardID;
    }

    public int YardID { 
      get {
        return _YardID;
      } 
      set {
        _YardID = value;
      } 
    }

    public String getBox() {
      String SQL = "SELECT * FROM PayLoadYard WHERE YardID=" + _YardID;
      return Util.getDBRowsJson(SQL);
    }

    public String getRowLines() {
      String SQL = @"SELECT 
       LeftPoint.RowNumber, 
       LeftPoint.ColumnNumber as ColLeft,
       RightPoint.ColumnNumber as ColRight,
       LeftPoint.TopLeftLat as sLat, 
       LeftPoint.TopLeftLon as sLon,
       RightPoint.TopRightLat as eLat, 
       RightPoint.TopRightLon as eLon
      FROM 
        PayLoadYardGrid as LeftPoint
      LEFT JOIN PayLoadYardGrid as RightPoint ON
        RightPoint.ColumnNumber = (SELECT MAX(ColumnNumber) FROM PayLoadYardGrid WHERE YardID=" + _YardID + @") AND
        RightPoint.RowNumber = LeftPoint.RowNumber AND
        RightPoint.YardID=" + _YardID + @"
      WHERE 
        LeftPoint.ColumnNumber = 0 AND
        LeftPoint.RowNumber > 0 AND
        LeftPoint.YardID=" + _YardID + @"
      ORDER BY 
        LeftPoint.RowNumber";
      return Util.getDBRowsJson(SQL);
    }

    public String getColumnLines() {
      String SQL = @"SELECT 
         LeftPoint.RowNumber as RowTop, 
         RightPoint.RowNumber as RowBottom, 
         LeftPoint.ColumnNumber,
         LeftPoint.TopLeftLat as sLat, 
         LeftPoint.TopLeftLon as sLon,
         RightPoint.BottomLeftLat as eLat, 
         RightPoint.BottomLeftLon as eLon
        FROM 
          PayLoadYardGrid as LeftPoint
        LEFT JOIN PayLoadYardGrid as RightPoint ON
          RightPoint.RowNumber = (SELECT MAX(RowNumber) FROM PayLoadYardGrid WHERE YardID=" + _YardID + @") AND
          RightPoint.ColumnNumber = LeftPoint.ColumnNumber AND
          RightPoint.YardID=" + _YardID + @"
        WHERE 
          LeftPoint.RowNumber = 0 AND
          LeftPoint.ColumnNumber > 0 AND
          LeftPoint.YardID=" + _YardID + @"
        ORDER BY 
          LeftPoint.ColumnNumber;";
      return Util.getDBRowsJson(SQL);
    }

    public String getCords() {
      String SQL = "SELECT\n" +
      "  [TopLeftLat],\n" +
      "  [TopLeftLon],\n" +
      "  [TopRightLat],\n" +
      "  [TopRightLon],\n" +
      "  [BottomLeftLat],\n" +
      "  [BottomLeftLon],\n" +
      "  [BottomRightLat],\n" +
      "  [BottomRightLon],\n" +
      "  CellNumber, RowNumber, ColumnNumber\n" +
      "    FROM\n" +
      "  [PayLoadYardGrid]";
      return Util.getDBRowsJson(SQL);
    }

    // degrees to radians
    private static double Deg2rad(double degrees) {
      return Math.PI * degrees / 180.0;
    }

    // radians to degrees
    private static double Rad2deg(double radians) {
      return 180.0 * radians / Math.PI;
    }

    // Earth radius at a given latitude, according to the WGS-84 ellipsoid [m]
    private static double WGS84EarthRadius(double lat) {
      // Semi-axes of WGS-84 geoidal reference
      double WGS84_a = 6378137.0; // Major semiaxis [m]
      double WGS84_b = 6356752.3; // Minor semiaxis [m]
      // http://en.wikipedia.org/wiki/Earth_radius
      var An = WGS84_a * WGS84_a * Math.Cos(lat);
      var Bn = WGS84_b * WGS84_b * Math.Sin(lat);
      var Ad = WGS84_a * Math.Cos(lat);
      var Bd = WGS84_b * Math.Sin(lat);
      return Math.Sqrt((An * An + Bn * Bn) / (Ad * Ad + Bd * Bd));
    }
    





  }
}