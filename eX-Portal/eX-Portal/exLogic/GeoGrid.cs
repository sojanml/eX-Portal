using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public GeoGrid(String FlightUniqueID) {
      _YardID = getYardID(FlightUniqueID);
    }

    public int YardID { 
      get {
        return _YardID;
      } 
      set {
        _YardID = value;
      } 
    }

    public String getTable(String FlightUniqueID) {
      int MaxRow = 0;
      int MaxCol = 0;
      Dictionary<String, String> Rows = new Dictionary<String, String>();

      String SQL = @"select 
        Max(RowNumber) as MaxRow,
        Max(ColumnNumber) as MaxCol
      from
        PayLoadMapData
      where
        FlightUniqueID = '" + FlightUniqueID + "'";
      var Max = Util.getDBRow(SQL);
      int.TryParse(Max["MaxRow"].ToString(), out MaxRow);
      int.TryParse(Max["MaxCol"].ToString(), out MaxCol);

      SQL = @"select 
        RowNumber,
        ColumnNumber,
        Count(PayLoadDataMapID) as Items
      from 
        PayLoadMapData  
      where 
        FlightUniqueID='" + FlightUniqueID + @"'
      GROUP BY
        RowNumber,
        ColumnNumber
      Order By
        RowNumber,
        ColumnNumber";

    //Add all reference to rows      
    for(var Row = 1; Row <= MaxRow; Row++) {
      for(var Col = 1; Col <= MaxCol; Col++) {
          String ThisRef = Row + "." + Col;
          Rows[ThisRef] = "";
      }
    }

      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              String ThisRef = reader["RowNumber"].ToString() + "." + 
                               reader["ColumnNumber"].ToString();
              String RFID = getRFID(
                ctx,
                FlightUniqueID, 
                reader["RowNumber"].ToString(), 
                reader["ColumnNumber"].ToString());
              //Rows[ThisRef] = int.Parse(reader["Items"].ToString());
              Rows[ThisRef] = RFID;
            }//while
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities


      StringBuilder TableRows = new StringBuilder();
      for (var Row = 1; Row <= MaxRow; Row++) {
        for (var Col = 1; Col <= MaxCol; Col++) {
          String ThisRef = Row + "." + Col;
          if (TableRows.Length > 0) TableRows.Append(",");
          TableRows.Append("\"");
          TableRows.Append(ThisRef);
          TableRows.Append("\": \"");
          TableRows.Append(Rows[ThisRef]);
          TableRows.Append("\"");
        }
        if (TableRows.Length > 0) TableRows.AppendLine("");
      }

      if (TableRows.Length > 0) TableRows.Append(",");
      TableRows.Append("\"Rows\":");
      TableRows.Append(MaxRow);
      TableRows.Append(",\"Cols\":");
      TableRows.Append(MaxCol);

      return TableRows.ToString();
    }

    private String getRFID(
      ExponentPortalEntities DB,
      String FlightUniqueID, 
      String sRow, String sCol) {
      int Row = -100;  
      int Col = -100;
      int.TryParse(sRow, out Row);
      int.TryParse(sCol, out Col);
      

      String SQL = "SELECT RFID From PayLoadMapData WHERE\n" +
      "  FlightUniqueID='" + FlightUniqueID + "' AND\n" +
      "  RowNumber='" + Row + "' AND\n" +
      "  ColumnNumber='" + Col + "'";
      String RFID = "";
      using (var cmd = DB.Database.Connection.CreateCommand()) {
        //DB.Database.Connection.Open();
        cmd.CommandText = SQL;
        using (var reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if (RFID != "") RFID += ",";
            RFID += reader["RFID"].ToString().Substring(4,6);
          }//while
        }//using reader
      }//using ctx.Database.Connection.CreateCommand

      return RFID;
    }

    public String getBox() {
      String SQL = "SELECT * FROM PayLoadYard WHERE YardID=" + _YardID;
      String JSon = Util.getDBRowsJson(SQL);
      if (String.IsNullOrWhiteSpace(JSon)) JSon = "[]";
      return JSon;
    }

    private int getYardID(String FlightUniqueID) {
      String SQL = @"SELECT YardID From PayLoadFlight WHERE FlightUniqueID='" + FlightUniqueID + "'";
      return Util.getDBInt(SQL);
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

    public String getGrid(String FlightUniqueID, bool IsReturnJSon = false) {
      _YardID = getYardID(FlightUniqueID);
      String SQL = "SELECT MAX(RowNumber) as Rows,  MAX(ColumnNumber) as Cols FROM PayLoadYardGrid WHERE YardID=" + _YardID;
      var GridSpec = Util.getDBRow(SQL);

      SQL = @"SELECT 
        PayLoadYardGrid.ColumnNumber ColNum,
        PayLoadYardGrid.RowNumber as RowNum,
        Concat(
        '[', PayLoadYardGrid.[TopLeftLat], ',',PayLoadYardGrid.[TopLeftLon], '],',
        '[', PayLoadYardGrid.[TopRightLat], ',',  PayLoadYardGrid.[TopRightLon], '],',
        '[', PayLoadYardGrid.[BottomLeftLat],',', PayLoadYardGrid.[BottomLeftLon], '],',
        '[', PayLoadYardGrid.[BottomRightLat],',', PayLoadYardGrid.[BottomRightLon], ']'
        ) as Grid, 
        (SELECT Count(*) FROM 
          PayLoadMapData
        WHERE
          PayLoadMapData.RowNumber = PayLoadYardGrid.RowNumber and
          PayLoadMapData.ColumnNumber = PayLoadYardGrid.ColumnNumber and
          PayLoadMapData.FlightUniqueID = '" + FlightUniqueID + @"'    
        ) as Products
      FROM 
         PayLoadYardGrid 
      WHERE 
        PayLoadYardGrid.YardID=" + _YardID + @"
      ORDER BY
        RowNum,
        ColNum";

      StringBuilder Grid = new StringBuilder();
      StringBuilder GridRow = new StringBuilder(); 
      int lastRow = -1, Row = 0, ProductCount = 0;

      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              Row = reader.GetInt32(reader.GetOrdinal("RowNum"));
              ProductCount = reader.GetInt32(reader.GetOrdinal("Products"));
              if (Row != lastRow && GridRow.Length > 0) {
                if (Grid.Length > 0) Grid.AppendLine(",");
                Grid.Append("[");
                Grid.Append(GridRow);
                Grid.Append("]");
                GridRow.Clear();
              }
              if (GridRow.Length > 0) GridRow.Append(",");
              GridRow.Append("{\"grid\":[");
              GridRow.Append(reader.GetValue(reader.GetOrdinal("Grid")).ToString());
              GridRow.Append("], \"items\":");
              GridRow.Append(ProductCount);
              GridRow.Append("}");
              lastRow = Row;
            }//while
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities

      //adding the last row
      if (Grid.Length > 0) Grid.AppendLine(",");
      Grid.Append("[");
      Grid.Append(GridRow);
      Grid.Append("]");
      GridRow.Clear();

      if(IsReturnJSon) {
        StringBuilder JsonGrid = new StringBuilder();
        JsonGrid.Append("[");
        JsonGrid.Append(Grid);
        JsonGrid.Append("]");
        return JsonGrid.ToString();
      }
      return Grid.ToString();


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