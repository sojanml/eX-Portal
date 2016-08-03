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
      //If local server set it to dubai address
      if(UserIP == "::1") UserIP = "80.227.122.178";
      MaxMind.GeoIP2.Responses.CityResponse city;
      var WeatherView = new WeatherViewModel();
      WeatherView.Forecast = new List<Forcast>();

      using(var reader = new DatabaseReader(@"C:\Web\eX-Portal\eX-Portal\eX-Portal\GeoLiteCity\GeoLite2-City.mmdb")) {
        // Replace "City" with the appropriate method for your database, e.g.,
        // "Country".
        try { 
          city = reader.City(UserIP);
        } catch {
          city = reader.City("80.227.122.178");
        }
        WeatherView.Country = city.Country.Name; // 'United States'
        WeatherView.City = city.City.Name;
        String CacheFile = getWeatherFile(city.Country.IsoCode, city.City.Name);
        String JSonContent = String.Empty;

        if(!File.Exists(CacheFile)) {
          JSonContent = DownloadWeatherInfo(city.Country.IsoCode, city.City.Name, CacheFile);
        } else {
          JSonContent = File.ReadAllText(CacheFile);
        }

        dynamic WeatherInfo = System.Web.Helpers.Json.Decode(JSonContent);
        var WeatherNow = WeatherInfo.data.current_condition[0];
        WeatherView.ConditionText = WeatherNow.weatherDesc[0].value;
        WeatherView.ConditionCode = WeatherNow.weatherCode;
        WeatherView.Speed = WeatherNow.windspeedKmph;
        WeatherView.ConditionTemperature = WeatherNow.temp_C;
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
      String WebURL = "http" + 
        "://api.worldweatheronline.com/premium/v1/weather.ashx?key=fea5347e9616488ab8760228163107" +
        "&q=" + City + "," + Country + "&format=json&num_of_days=5";
      using(var webClient = new System.Net.WebClient()) {
        webClient.Encoding = System.Text.Encoding.UTF8;
        var json2 = webClient.DownloadString(WebURL);

        String PathOnly = Path.GetDirectoryName(CacheFile);
        if(!Directory.Exists(PathOnly)) Directory.CreateDirectory(PathOnly);

        using(System.IO.StreamWriter file = new System.IO.StreamWriter(CacheFile, false)) {
          file.WriteLine(json2);
        }//using(System.IO.StreamWriter)

        return json2;


      }//using(var webClient)

    }

    private static String getWeatherFile(String Country, String City) {
      String CacheFile = DateTime.Now.ToString("yyyy-MM-dd-Z") + (DateTime.Now.Hour / 4);

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