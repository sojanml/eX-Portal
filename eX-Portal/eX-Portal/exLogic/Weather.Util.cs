using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using eX_Portal.ViewModel;
using MaxMind.Db;
using MaxMind.GeoIP2;


namespace eX_Portal.exLogic {
  public static class Weather {
    public static Dictionary<int, String> WeatherIcons = new Dictionary<int, String>() {
     {0, "&#xf056;" }, {10, "&#xf019;" },  {20, "&#xf014;" }, {30, "&#xf00c;" },   {40, "&#xf01a;" },
     {1, "&#xf01d;" }, {11, "&#xf01a;" },  {21, "&#xf0b6;" }, {31, "&#xf02e;" },   {41, "&#xf01b;" },
     {2, "&#xf073;" }, {12, "&#xf01a;" },  {22, "&#xf062;" }, {32, "&#xf00d;" },   {42, "&#xf01b;" },
     {3, "&#xf01e;" }, {13, "&#xf01b;" },  {23, "&#xf085;" }, {33, "&#xf02e;" },   {43, "&#xf01b;" },
     {4, "&#xf01e;" }, {14, "&#xf01b;" },  {24, "&#xf021;" }, {34, "&#xf00d;" },   {44, "&#xf013;" },
     {5, "&#xf017;" }, {15, "&#xf064;" },  {25, "&#xf076;" }, {35, "&#xf015;" },   {45, "&#xf01e;" },
     {6, "&#xf0b5;" }, {16, "&#xf01b;" },  {26, "&#xf013;" }, {36, "&#xf072;" },   {46, "&#xf01b;" },
     {7, "&#xf0b5;" }, {17, "&#xf015;" },  {27, "&#xf086;" }, {37, "&#xf01e;" },   {47, "&#xf01d;" },
     {8, "&#xf04e;" }, {18, "&#xf0b5;" },  {28, "&#xf002;" }, {38, "&#xf01e;" },
     {9, "&#xf04e;" }, {19, "&#xf063;" },  {29, "&#xf083;" }, {39, "&#xf01e;" },
     
     {3200, "&#xf075;" }     
    };
    

    public static WeatherViewModel getLocalWeather(String UserIP) {
      String JSonContent = String.Empty;
      int DelayHours = 0;
      //If local server set it to dubai address
      MaxMind.GeoIP2.Responses.CityResponse city = new MaxMind.GeoIP2.Responses.CityResponse();
      var WeatherView = new WeatherViewModel();
      WeatherView.Forecast = new List<Forcast>();

      String DatabaseLocation = HttpContext.Current.Server.MapPath("/GeoLiteCity/GeoLite2-City.mmdb");

      using(var reader = new DatabaseReader(DatabaseLocation)) {
        // Replace "City" with the appropriate method for your database, e.g.,
        // "Country".
        try {
          city = reader.City(UserIP);
          WeatherView.Country = city.Country.Name; // 'United States'
          WeatherView.City = city.City.Name;
        } catch {
          //do any error processing
        }
      }//using(var reader)

      if(String.IsNullOrEmpty(WeatherView.Country))
        WeatherView.Country = "United Arab Emirates";
      if(String.IsNullOrEmpty(WeatherView.City))
        WeatherView.City = "Dubai";

      do {
        String CacheFile = getWeatherFile(WeatherView.Country, WeatherView.City, DelayHours);
        if (!File.Exists(CacheFile)) {
          JSonContent = DownloadWeatherInfo(WeatherView.Country, WeatherView.City, CacheFile);
        } else {
          JSonContent = File.ReadAllText(CacheFile);
        }
        DelayHours = DelayHours - 4;
      } while (JSonContent.Length <= 10);

      dynamic WeatherInfo = System.Web.Helpers.Json.Decode(JSonContent);
      if(WeatherInfo != null) { 
        var WeatherNow = WeatherInfo.data.current_condition[0];
        WeatherView.ConditionText = WeatherNow.weatherDesc[0].value;
        WeatherView.ConditionCode = WeatherNow.weatherCode;
        WeatherView.Speed = WeatherNow.windspeedKmph;
        WeatherView.ConditionTemperature = Util.toDouble(WeatherNow.temp_C).ToString("0.0");
        if (WeatherView.ConditionTemperature == "0.0") {
          Double Temp = (Util.toDouble(WeatherNow.temp_F) - 32) / 1.8;
          WeatherView.ConditionTemperature = Temp.ToString("0.0");
        }
        WeatherView.TemperatureUnit = "&deg;C";
        WeatherView.Pressure = Util.toDouble(WeatherNow.pressure);
        WeatherView.Wind = WeatherNow.windspeedKmph;
        WeatherView.Direction = WeatherNow.winddirDegree;
        WeatherView.Visibility = Util.toDouble(WeatherNow.visibility);
        WeatherView.Humidity = WeatherNow.humidity;

        foreach(var ForcastDay in WeatherInfo.data.weather) {
          var thisForcast = new Forcast {
            Code = Util.toInt(ForcastDay.hourly[3].weatherCode),
            Date = (DateTime.Parse(ForcastDay.date)).ToString("dd MMM"),
            status = ForcastDay.hourly[3].weatherDesc[0].value,
            TempHigh = ForcastDay.maxtempC,
            TempLow = ForcastDay.mintempC
          };
          WeatherView.Forecast.Add(thisForcast);
        }
      }
      //var intAddress = BitConverter.ToInt64(IPAddress.Parse(UserIP).GetAddressBytes(), 0);

      return WeatherView;
    }
     
    private static String DownloadWeatherInfo(String Country, String City, String CacheFile) {
      String WebURL = Exponent.WeatherAPI + "&q=" + City + "," + Country;
      String json2 = String.Empty;
      try { 
        using (var webClient = new System.Net.WebClient()) {
          webClient.Encoding = System.Text.Encoding.UTF8;
          json2 = webClient.DownloadString(WebURL);
          if (String.IsNullOrEmpty(json2)) return String.Empty;

          String PathOnly = Path.GetDirectoryName(CacheFile);
          if(!Directory.Exists(PathOnly)) Directory.CreateDirectory(PathOnly);

          using(System.IO.StreamWriter file = new System.IO.StreamWriter(CacheFile, false)) {
            file.Write(json2);
          }//using(System.IO.StreamWriter)
        }//using(var webClient)
      } catch {
        //nothing - do not show the error
      }
      return json2;
    }

    private static String getWeatherFile(String Country, String City, int DelayHours = 0) {
      DateTime CasheFileTile = DateTime.Now.AddHours(DelayHours);
      String CacheFile = CasheFileTile.ToString("yyyy-MM-dd-Z") + (CasheFileTile.Hour / 4);

      String FolderLocation = Path.Combine(
        HttpContext.Current.Server.MapPath("/Upload/WeatherCache"),
        Country,
        City,
        CacheFile + ".json"
      );

      return FolderLocation;
    }

  }



}