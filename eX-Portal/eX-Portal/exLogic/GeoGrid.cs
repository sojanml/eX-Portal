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

  public class Yard {
    public GeoBox Bounds { get; set; }
    public String VechileOrientation { get; set; }
    public int VechileLength { get; set; }
    public int VechileWidth { get; set; }
    public int YardID { get; set; }
    public int AccountID { get; set; }
    public String YardName { get; set; }

    public String isChecked(String Orientation) {
      if (VechileOrientation == Orientation) return "checked=\"checked\"";
      if (String.IsNullOrEmpty(VechileOrientation) && Orientation == "H") return "checked=\"checked\"";
      if (Orientation == "new" && YardID <= 0) return "checked=\"checked\"";
      return "";
    }
  }

  public class GeoGrid {
    private int _YardID = 0;
    private String _FlightUniqueID = String.Empty;
    public GeoGrid(int _pYardID = 0) {
      _YardID = _pYardID;
    }

    public GeoGrid(String FlightUniqueID) {
      _YardID = getYardID(FlightUniqueID);
      _FlightUniqueID = FlightUniqueID;
    }

    public int YardID { 
      get {
        return _YardID;
      } 
      set {
        _YardID = value;
      } 
    }

    public Yard getYard(int thisYardID = 0) {
      if (thisYardID == 0) thisYardID = _YardID;
      String SQL = @"SELECT
        [AccountID],
        [YardName],
        [TopLeftLat],
        [TopLeftLon],
        [TopRightLat],
        [TopRightLon],
        [BottomLeftLat],
        [BottomLeftLon],
        [BottomRightLat],
        [BottomRightLon],
        [VechileOrientation],
        [VechileLength],
        [VechileWidth],
        [CreatedBy],
        [CreatedOn]
      FROM 
        [PayLoadYard]
      WHERE
        YardID=" + thisYardID;
      var Row = Util.getDBRow(SQL);
      var theYard = new Yard();
      if(Row["hasRows"].ToString() == "True") theYard = new Yard {
        YardID = thisYardID,
        AccountID = Util.toInt(Row["AccountID"]),
        YardName = Row["YardName"].ToString(),
        VechileOrientation = Row["VechileOrientation"].ToString(),
        VechileLength = Util.toInt(Row["VechileLength"]),
        VechileWidth = Util.toInt(Row["VechileWidth"]),
        Bounds = new GeoBox {
          TopLeft = new MapPoint {
            Latitude = Util.toDouble(Row["TopLeftLat"]),
            Longitude = Util.toDouble(Row["TopLeftLon"])
          },
          TopRight = new MapPoint {
            Latitude = Util.toDouble(Row["TopRightLat"]),
            Longitude = Util.toDouble(Row["TopRightLon"])
          },
          BottomRight = new MapPoint {
            Latitude = Util.toDouble(Row["BottomRightLat"]),
            Longitude = Util.toDouble(Row["BottomRightLon"])
          },
          BottomLeft = new MapPoint {
            Latitude = Util.toDouble(Row["BottomLeftLat"]),
            Longitude = Util.toDouble(Row["BottomLeftLon"])
          }
        }
      };
      return theYard;
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

    public Object getGridKML() {
      String VerticalSQL = @"SELECT 
       LeftPoint.RowNumber, 
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

      String HorizondalSQL = @"SELECT 
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

      String BoundarySQL = "SELECT * FROM PayLoadYard WHERE YardID=" + _YardID;

      StringBuilder XML = new StringBuilder();
      XML.AppendLine(@"<kml xmlns=""http://earth.google.com/kml/2.1"">");
      XML.AppendLine("<Document>");
      XML.AppendLine(@"<name>Chicago Transit Map</name>
        <description>Payload Lines</description>
      <Style id=""GridInLine"">
        <LineStyle>
        <color>ffff0000</color>
        <width>1</width>
        </LineStyle>
      </Style>
      <Style id=""GridBorder"">
        <LineStyle>
        <color>ff000000</color>
        <width>1</width>
        </LineStyle>
      </Style>");

      //Adding Lines
      XML.Append(BuildCordinates(VerticalSQL));
      XML.Append(BuildCordinates(HorizondalSQL));
      XML.Append(BuildBoundary(BoundarySQL));
      XML.Append(getPayloadItems());

      //Closing Document
      XML.AppendLine("</Document>");
      XML.AppendLine("</kml>");

      return XML;

    }

    public StringBuilder getPayloadItems() {
      String SQL = @"SELECT
        PayloadMapData.RFID,
        (TopLeftLat + BottomRightLat)/2 as Lat,
        (TopLeftLon + BottomRightLon)/2 as Lon,
        MSTR_Product.VIN,
        MSTR_Product.CarMake,
        MSTR_Product.Model,
        MSTR_Product.[Year],
        MSTR_Product.Color
      FROM
        PayloadMapData
      LEFT JOIN PayLoadYardGrid ON
        PayLoadYardGrid.YardID = PayloadMapData.YardID AND
        PayLoadYardGrid.RowNumber = (PayloadMapData.RowNumber - 1) AND
        PayLoadYardGrid.ColumnNumber = (PayloadMapData.ColumnNumber - 1)
      LEFT JOIN MSTR_Product ON
        MSTR_Product.RFID = PayloadMapData.RFID
      WHERE
        PayloadMapData.FlightUniqueID = '" + _FlightUniqueID +  @"' 
      ORDER BY
        PayloadMapData.RowNumber,
        PayloadMapData.ColumnNumber
      ";
      StringBuilder SB = new StringBuilder();
      var Data = Util.getDBRows(SQL);
      foreach(var Row in Data) {
        SB.Append(@"<Placemark>");
        SB.AppendFormat("<name>" + Row["RFID"] + "</name>");
        SB.Append(@"<description>");
        SB.Append("<![CDATA[");
        SB.AppendLine("VIN: <b>" + Row["VIN"] + "</b><br>");
        SB.AppendLine("Make: <b>" + Row["CarMake"] + "</b><br>");
        SB.AppendLine("Year: <b>" + Row["Year"] + "</b><br>");
        SB.AppendLine("Color: <b>" + Row["Color"] + "</b>");
        SB.Append("]]>");
        SB.Append(@"</description>");
        SB.Append(@"<Point>
        <coordinates>");
        SB.AppendFormat("{0},{1}\n", Row["Lon"], Row["Lat"]);
        SB.Append(@"</coordinates>
        </Point>
        </Placemark>");
      }
      return SB;


    }
    
    public StringBuilder BuildCordinates(String SQL) {
      StringBuilder SB = new StringBuilder();
      var Data = Util.getDBRows(SQL);
      foreach(var Row in Data) {
        SB.AppendLine(@"
      <Placemark>
        <name>Blue Line</name>
        <styleUrl>#GridInLine</styleUrl>
        <LineString>
        <altitudeMode>relative</altitudeMode>
        <coordinates>");
        SB.AppendFormat("{0},{1},0 {2},{3},0\n", Row["sLon"], Row["sLat"], Row["eLon"], Row["eLat"]);
        SB.AppendLine(@"</coordinates>
        </LineString>
      </Placemark>");
      }
      return SB;
    }

    public StringBuilder BuildBoundary(String SQL) {
      StringBuilder SB = new StringBuilder();
      var Data = Util.getDBRows(SQL);
      foreach(var Row in Data) {
        SB.AppendLine(@"
      <Placemark>
        <name>Blue Line</name>
        <styleUrl>#GridBorder</styleUrl>
        <LineString>
        <altitudeMode>relative</altitudeMode>
        <coordinates>");
        SB.AppendFormat("{0},{1},0\n", Row["TopLeftLon"], Row["TopLeftLat"]);
        SB.AppendFormat("{0},{1},0\n", Row["TopRightLon"], Row["TopRightLat"]);
        SB.AppendFormat("{0},{1},0\n", Row["BottomRightLon"], Row["BottomRightLat"]);
        SB.AppendFormat("{0},{1},0\n", Row["BottomLeftLon"], Row["BottomLeftLat"]);
        SB.AppendFormat("{0},{1},0\n", Row["TopLeftLon"], Row["TopLeftLat"]); 
        SB.AppendLine(@"</coordinates>
        </LineString>
      </Placemark>");
      }
      return SB;
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

    public String getDistance(String UniqueFlightID) {
      String SQL = @"Select 
        RowDistance,
        ColDistance,
        ColPointLat,
        ColPointLon,
        RowPointLat,
        RowPointLon,
        Latitude,
        Longitude,
        RowNumber,
        ColumnNumber
      from 
        PayLoadMapData 
      WHERE 
        FlightUniqueID='" + UniqueFlightID + "'";
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