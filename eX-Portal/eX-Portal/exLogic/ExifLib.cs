using System.Linq;
using System.Web;

using System;
using System.Collections.Generic;
using System.IO;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/*
Add the Following reference to the projcet
  1. PresentationCore
  2. WindowsBase
*/

namespace eX_Portal.exLogic {





  public class GPSInfo {
    public GPSInfo() {
      Latitude = 0;
      Longitude = 0;
      Altitude = 0;
    }
    public Double Latitude { get; set; }
    public Double Longitude { get; set; }
    public Double Altitude { get; set; }


    public string getInfo () {
      string Direction = "";
      double UnformattedLatitude = Latitude;
      double UnformattedLongitude = Longitude;

      if (Latitude > 0) {
        Direction = "N";
      } else {
        UnformattedLatitude = UnformattedLatitude * -1;
        Direction = "S";
      }
      string GPSString = UnformattedLatitude.ToString("0.0000") + Direction;

      if (Longitude > 0) {
        Direction = "E";
      } else {
        UnformattedLongitude = UnformattedLongitude * -1;
        Direction = "W";
      }
      GPSString = GPSString +  " " + UnformattedLongitude.ToString("0.0000") + Direction;
      return GPSString;
    }

  }

  class ExifLib {


    private String _InputFileName = "";
    private String _OutputFileName = "";
    public ExifLib(String InputFileName, String OutputFileName = "") {
      _InputFileName = InputFileName;
      _OutputFileName = OutputFileName;
    }

    public String setThumbnail(int Width = 120, String ExtPrefix="t") {
      Image image = Image.FromFile(_InputFileName);
      int X = image.Width;
      int Y = image.Height;
      int Height = (int)((Width * Y) / X);
      if (String.IsNullOrWhiteSpace(_OutputFileName)) _OutputFileName = _InputFileName;
      String ThumbnailImage = Path.ChangeExtension(_OutputFileName, ExtPrefix + ".png");

      Image thumb = image.GetThumbnailImage(Width, Height, () => false, IntPtr.Zero);      
      thumb.Save(ThumbnailImage);
            image.Dispose();
      return ThumbnailImage;
    }
    public GPSInfo getGPS() {
      var myGPS = new GPSInfo();
      //_InputFileName = InputFileName;
      Image image = Image.FromFile(_InputFileName);
      myGPS.Latitude = GetLatitude(image);
      myGPS.Longitude = GetLongitude(image);
      image.Dispose();
      return myGPS;
    }

    public float GetLatitude(Image targetImg) {
      try {
        //Property Item 0x0001 - PropertyTagGpsLatitudeRef
        PropertyItem propItemRef = targetImg.GetPropertyItem(1);
        //Property Item 0x0002 - PropertyTagGpsLatitude
        PropertyItem propItemLat = targetImg.GetPropertyItem(2);
        return ExifGpsToFloat(propItemRef, propItemLat);
      } catch (ArgumentException) {
        return 0;
      }
    }
    public float GetLongitude(Image targetImg) {
      try {
        ///Property Item 0x0003 - PropertyTagGpsLongitudeRef
        PropertyItem propItemRef = targetImg.GetPropertyItem(3);
        //Property Item 0x0004 - PropertyTagGpsLongitude
        PropertyItem propItemLong = targetImg.GetPropertyItem(4);
        return ExifGpsToFloat(propItemRef, propItemLong);
      } catch (ArgumentException) {
        return 0;
      }
    }
    private static float ExifGpsToFloat(PropertyItem propItemRef, PropertyItem propItem) {
      uint degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
      uint degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
      float degrees = degreesNumerator / (float)degreesDenominator;

      uint minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
      uint minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
      float minutes = minutesNumerator / (float)minutesDenominator;

      uint secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
      uint secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
      float seconds = secondsNumerator / (float)secondsDenominator;

      float coorditate = degrees + (minutes / 60f) + (seconds / 3600f);
      string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
      if (gpsRef == "S" || gpsRef == "W")
        coorditate = 0 - coorditate;
      return coorditate;
    }


    public GPSInfo getGPS(int FlightID, DateTime FileCreatedOn) {
        StringBuilder GPSInfo = new StringBuilder();
      String FromTime = Util.toSQLDate(FileCreatedOn.AddMinutes(-5).ToUniversalTime());
      String ToTime = Util.toSQLDate(FileCreatedOn.AddMinutes(5).ToUniversalTime());
      String ThisTime = Util.toSQLDate(FileCreatedOn.ToUniversalTime());
      String SQL = @"Select TOP 1 
        Latitude,
        Longitude,
        Altitude,
        ABS(DATEDIFF(SECOND, ReadTime,'" + ThisTime + @"')) as SortTime
      from 
        FlightMapData 
      where 
        flightid=" + FlightID + @" AND
        ReadTime >=  '" + FromTime + @"' AND
        ReadTime <=  '" + ToTime + @"'
      ORDER BY
        SortTime ASC,
        ReadTime DESC";
      var Row = Util.getDBRow(SQL);
      var theGPS = new GPSInfo();
      if (Row["hasRows"].ToString() == "True") {
        theGPS.Latitude = Util.toDouble(Row["Latitude"]);
        theGPS.Longitude = Util.toDouble(Row["Longitude"]);
        theGPS.Altitude = Util.toDouble(Row["Altitude"]);
      }
      return theGPS;
    }
     

    // North or South Latitude 
    // ASCII 2    // Latitude        
    private const string GPSLatitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=1}";
    // RATIONAL 3  // East or West Longitude 
    private const string GPSLatitudeQuery = "/app1/ifd/gps/subifd:{ulong=2}";
    // ASCII 2 // Longitude 
    private const string GPSLongitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=3}";
    // RATIONAL 3 // Altitude reference 
    private const string GPSLongitudeQuery = "/app1/ifd/gps/subifd:{ulong=4}";
    // BYTE 1 // Altitude 
    private const string GPSAltitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=5}";
    // RATIONAL 1
    private const string GPSAltitudeQuery = "/app1/ifd/gps/subifd:{ulong=6}";

    public bool setGPS(GPSInfo theGPS) {

      double latitude = theGPS.Latitude;
      double longitude = theGPS.Longitude;
      double altitude = theGPS.Altitude;

      //original image file
      string originalPath = _InputFileName;
      //image file after adding the GPS tags
      string outputPath = _OutputFileName;

      BitmapCreateOptions createOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
      uint paddingAmount = 2048;

      //open the image file
      using (Stream originalFile = File.Open(originalPath, FileMode.Open, FileAccess.Read)) {
        BitmapDecoder original = BitmapDecoder.Create(originalFile, createOptions, BitmapCacheOption.None);

        //this becomes the new image that contains new metadata
        JpegBitmapEncoder output = new JpegBitmapEncoder();

        if (original.Frames[0] != null && original.Frames[0].Metadata != null) {
          //clone the metadata from the original input image so that it can be modified
          BitmapMetadata metadata = original.Frames[0].Metadata.Clone() as BitmapMetadata;

          //pad the metadata so that it can be expanded with new tags
          metadata.SetQuery("/app1/ifd/PaddingSchema:Padding", paddingAmount);
          metadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", paddingAmount);
          metadata.SetQuery("/xmp/PaddingSchema:Padding", paddingAmount);


          GPSRational latitudeRational = new GPSRational(latitude);
          GPSRational longitudeRational = new GPSRational(longitude);
          metadata.SetQuery(GPSLatitudeQuery, latitudeRational.bytes);
          metadata.SetQuery(GPSLongitudeQuery, longitudeRational.bytes);
          if (latitude > 0) metadata.SetQuery(GPSLatitudeRefQuery, "N");
          else metadata.SetQuery(GPSLatitudeRefQuery, "S");
          if (longitude > 0) metadata.SetQuery(GPSLongitudeRefQuery, "E");
          else metadata.SetQuery(GPSLongitudeRefQuery, "W");

          //denoninator = 1 for Rational
          Rational altitudeRational = new Rational((int)altitude, 1);
          metadata.SetQuery(GPSAltitudeQuery, altitudeRational.bytes);

          //create the output image using the image data, thumbnail, and metadata from the original image as modified above
          output.Frames.Add(
              BitmapFrame.Create(original.Frames[0], original.Frames[0].Thumbnail, metadata, original.Frames[0].ColorContexts));
        }//if (original.Frames[0] != null)

        //save the output image
        using (Stream outputFile = File.Open(outputPath, FileMode.Create, FileAccess.ReadWrite)) {
          output.Save(outputFile);
        }//using (Stream outputFile)

        //Delete the source file
        //System.IO.File.Delete(originalPath);
      }//using (Stream originalFile)

      return true;
    }//public bool GPS)

  }//class ExifLib


  //EXIF Rational Type (pack 4-byte numerator and 4-byte denominator into 8 bytes
  public class Rational {
    public Int32 _num;     //numerator of exif rational
    public Int32 _denom;   //denominator of exif rational
    public byte[] bytes;   //8 bytes that form the exif rational value

    //form rational from a given 4-byte numerator and denominator
    public Rational(Int32 _Num, Int32 _Denom) {
      _num = _Num;
      _denom = _Denom;

      bytes = new byte[8];  //create a byte array with 8 bytes
      BitConverter.GetBytes(_num).CopyTo(bytes, 0);  //copy 4 bytes of num to location 0 in the byte array
      BitConverter.GetBytes(_denom).CopyTo(bytes, 4);  //copy 4 bytes of denom to location 4 in the byte array
    }

    //form rational from an array of 8 bytes
    public Rational(byte[] _bytes) {
      byte[] n = new byte[4];
      byte[] d = new byte[4];
      //copy 4 bytes from bytes-array into n-array starting at index 0 from bytes and 0 from n 
      Array.Copy(_bytes, 0, n, 0, 4);
      //copy 4 bytes from bytes-array into d-array starting at index 4 from bytes and 0 from d 
      Array.Copy(_bytes, 4, d, 0, 4);
      //convert the 4 bytes from n into a 4-byte int (becomes the numerator of the rational)
      _num = BitConverter.ToInt32(n, 0);
      //convert the 4 bytes from d into a 4-byte int (becomes the denonimator of the rational)
      _denom = BitConverter.ToInt32(d, 0);

    }

    //convert the exif rational into a double value
    public double ToDouble() {
      //round the double value to 5 digits
      return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 5);
    }
  }

  //special rational class to handle the GPS three rational values  (degrees, minutes, seconds)
  public class GPSRational {
    public Rational _degrees;
    public Rational _minutes;
    public Rational _seconds;
    public byte[] bytes;  //becomes an array of 24 bytes that represent hrs, minutes, seconds as 3 rationals
    double angleInDegrees;  //latitude or longitude as decimal degrees

    //form the 3-rational exif value from an angle in decimal degrees
    public GPSRational(double angleInDeg) {
      //convert angle in decimal degrees to three rationals (deg, min, sec) with denominator of 1
      //NOTE:  this formulation results in a descretization of about 100 ft in the lat/lon position
      double absAngleInDeg = Math.Abs(angleInDeg);
      int degreesInt = (int)(absAngleInDeg);
      absAngleInDeg -= degreesInt;
      int minutesInt = (int)(absAngleInDeg * 60.0);
      absAngleInDeg -= minutesInt / 60.0;
      int secondsInt = (int)(absAngleInDeg * 3600.0 + 0.50);

      //form a rational using "1" as the denominator
      int denominator = 1;
      _degrees = new Rational(degreesInt, denominator);
      _minutes = new Rational(minutesInt, denominator);
      _seconds = new Rational(secondsInt, denominator);

      angleInDegrees = _degrees.ToDouble() + _minutes.ToDouble() / 60.0 + _seconds.ToDouble() / 3600.0;

      // form the 24-byte array representing the 3 rationals
      bytes = new byte[24];
      BitConverter.GetBytes(degreesInt).CopyTo(bytes, 0);
      BitConverter.GetBytes(denominator).CopyTo(bytes, 4);
      BitConverter.GetBytes(minutesInt).CopyTo(bytes, 8);
      BitConverter.GetBytes(denominator).CopyTo(bytes, 12);
      BitConverter.GetBytes(secondsInt).CopyTo(bytes, 16);
      BitConverter.GetBytes(denominator).CopyTo(bytes, 20);
    }

    //Form the GPSRational object from an array of 24 bytes
    public GPSRational(byte[] _bytes) {
      byte[] degBytes = new byte[8]; byte[] minBytes = new byte[8]; byte[] secBytes = new byte[8];

      //form the hours, minutes, seconds rational values from the input 24 bytes
      // first 8 are hours, second 8 are the minutes, third 8 are the seconds
      Array.Copy(_bytes, 0, degBytes, 0, 8); Array.Copy(_bytes, 8, minBytes, 0, 8); Array.Copy(_bytes, 16, secBytes, 0, 8);

      _degrees = new Rational(degBytes);
      _minutes = new Rational(minBytes);
      _seconds = new Rational(secBytes);

      angleInDegrees = _degrees.ToDouble() + _minutes.ToDouble() / 60.0 + _seconds.ToDouble() / 3600.0;
      bytes = new byte[24];
      _bytes.CopyTo(bytes, 0);
    }
  }

}//namespace eX_Portal.exLogic