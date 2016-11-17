using MaxMind.GeoIP2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace  Exponent  {
  public sealed class WeatherAPI {
    private const String APIKey = "e925e5f9b310f96d54cb45e01bfb3fe0";
    private const String APIUrl = "http://api.openweathermap.org/data/2.5/";
    private String ApplicationPah = String.Empty;
    private System.Net.WebClient webClient ;
    

    public WeatherAPI() {
      //ApplicationPah = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);      
      //ApplicationPah = Path.Combine(ApplicationPah.Replace("file:\\", ""), "OpenWeatherMap");
      ApplicationPah = @"C:\WWW\OpenWeatherMap";

    }

    public WeatherForcast GetByLocation(Double Lat, Double Lng) {
      WeatherForcast ThisWeather = new WeatherForcast();
      try {
        DateTime LastCashedOn = getLastProcessedDateTime(Lat, Lng);
        DateTime MaxCashedTime = LastCashedOn.AddMinutes(30);

        if (MaxCashedTime > DateTime.Now) {
          return CashedWeather(Lat, Lng);
        }

        String URL = APIUrl + "forecast/daily?APIKey=" + APIKey + "&units=metric&cnt=5&lat=" + Lat + "&lon=" + Lng;
        String JsonData = getWeatherJson(URL);
        if (String.IsNullOrEmpty(JsonData)) {
          return CashedWeather(Lat, Lng);
        } else {
          dynamic WeatherJson = JObject.Parse(JsonData);
          SetWeatherInfo(ThisWeather, WeatherJson);
          ThisWeather.City = WeatherJson.city.name;
          ThisWeather.Country = WeatherJson.city.country;
          SetWeatherStation(ThisWeather.Today, Lat, Lng);
          String LatLngFolder = String.Format("#{0}#{1}", Convert.ToInt32(Lat), Convert.ToInt32(Lng));
          SaveWeatherCashe(ThisWeather, LatLngFolder);
        }
      } catch (Exception ex) {
        ThisWeather.Today.ConditionText = "Error";
      }
      return ThisWeather;
    }

    public WeatherForcast GetByIP(String IPAddress) {
      WeatherForcast ThisWeather = new WeatherForcast();
        CityInfo ThisCity = GetCityByIP(IPAddress);
 
        DateTime LastCashedOn = getLastProcessedDateTime(ThisCity.Country, ThisCity.City);
        DateTime MaxCashedTime = LastCashedOn.AddMinutes(30);
        if (MaxCashedTime > DateTime.Now) {
          return CashedWeather(ThisCity.Country, ThisCity.City);
        }

        //Get todays weather and forecast
        String URL = APIUrl + "forecast/daily?APIKey=" + APIKey + "&cnt=5&units=metric&q=" + ThisCity.City + "," + ThisCity.Country;
        ThisWeather.Country = ThisCity.Country;
        ThisWeather.City = ThisCity.City;
        String JsonData = getWeatherJson(URL);
        if(String.IsNullOrEmpty(JsonData)) {
          return CashedWeather(ThisCity.City, ThisCity.Country);
        } else { 
          dynamic WeatherJson = JObject.Parse(JsonData);
          SetWeatherInfo(ThisWeather, WeatherJson);
          SetWeatherStation(ThisWeather.Today, ThisCity.Lat, ThisCity.Lng);
        }
        SaveWeatherCashe(ThisWeather);

      return ThisWeather;
    }


    private WeatherForcast CashedWeather(String Country, String City) {
      WeatherForcast tmp = new WeatherForcast();
      String LastCachedFile = Path.Combine(ApplicationPah, Country, City, "CachedWeather.json");
      if (File.Exists(LastCachedFile)) {
        String JSonText = File.ReadAllText(LastCachedFile);
        try {
          tmp = JsonConvert.DeserializeObject<WeatherForcast>(JSonText);
        } catch {
          //nothing
        }
      }
      return tmp;
    }

    private WeatherForcast CashedWeather(Double Lat, Double Lng) {
      WeatherForcast tmp = new WeatherForcast();
      String LatLngFolder = String.Format("#{0}#{1}", Convert.ToInt32(Lat), Convert.ToInt32(Lng));
      String LastCachedFile = Path.Combine(ApplicationPah, LatLngFolder, "CachedWeather.json");
      if (File.Exists(LastCachedFile)) {
        String JSonText = File.ReadAllText(LastCachedFile);
        try { 
          tmp = JsonConvert.DeserializeObject<WeatherForcast>(JSonText);
        } catch {
          //nothing
        }
      }
      return tmp;
    }
    private void SaveWeatherCashe(WeatherForcast ThisWeather, String LatLngFolder = "") {
      //save the cached file
      String Country = ThisWeather.Country.Replace(" ", "");
      String City = ThisWeather.City.Replace(" ", "");

      String FileDir = String.IsNullOrEmpty(LatLngFolder) ?
        Path.Combine(ApplicationPah, Country, City) :
        Path.Combine(ApplicationPah, LatLngFolder);


      String LastCachedFileRef = Path.Combine(FileDir, "CachedDate.txt");
      String LastCachedFile = Path.Combine(FileDir, "CachedWeather.json");
     
      if (!Directory.Exists(FileDir)) Directory.CreateDirectory(FileDir);
      if (File.Exists(LastCachedFileRef)) File.Delete(LastCachedFileRef);
      if (File.Exists(LastCachedFile)) File.Delete(LastCachedFile);

      String TheTime = DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss");
      System.IO.File.WriteAllText(LastCachedFileRef, TheTime);

      //Save Json
      var json = JsonConvert.SerializeObject(ThisWeather);
      System.IO.File.WriteAllText(LastCachedFile, json);

    }

    private DateTime getLastProcessedDateTime(String Country, String City) {
      DateTime LastDate = DateTime.MinValue;
      String LastCachedFileRef = Path.Combine(ApplicationPah, Country, City, "CachedDate.txt");
      if(File.Exists(LastCachedFileRef)) {
        String sLastDate = File.ReadAllText(LastCachedFileRef);
        DateTime.TryParse(sLastDate, out LastDate);
      }
      return LastDate;
    }

    private DateTime getLastProcessedDateTime(Double Lat, Double Lng) {
      DateTime LastDate = DateTime.MinValue;
      String LatLngFolder = String.Format("#{0}#{1}", Convert.ToInt32(Lat), Convert.ToInt32(Lng));
      String LastCachedFileRef = Path.Combine(ApplicationPah, LatLngFolder, "CachedDate.txt");
      if (File.Exists(LastCachedFileRef)) {
        String sLastDate = File.ReadAllText(LastCachedFileRef);
        DateTime.TryParse(sLastDate, out LastDate);
      }
      return LastDate;
    }

    private void SetWeatherStation(WeatherCondition Today, Double Lat, Double Lng) {

      //Setup todays wether information
      String URL = APIUrl + "station/find?APIKey=" + APIKey + "&units=metric&lat=" + Lat + "&lon=" + Lng + "&cnt=4";
      String WeatherStation = getWeatherJson(URL);
      dynamic WeatherStationJson = JObject.Parse("{\"data\":" + WeatherStation + "}");

      var Data = WeatherStationJson.data[0];      
      Today.Visibility = Data.last.visibility.distance / 1000;


      //get current weather data
      URL = APIUrl + "weather?APIKey=" + APIKey + "&units=metric&lat=" + Lat + "&lon=" + Lng + "&cnt=4";
      String CurrentWeather = getWeatherJson(URL);
      dynamic CurrentWeatherJson = JObject.Parse(CurrentWeather);
      Today.ConditionCode = CurrentWeatherJson.weather[0].icon;
      Today.ConditionText = CurrentWeatherJson.weather[0].main;
      Today.WindDirection = CurrentWeatherJson.wind.deg ==  null ? 0 : CurrentWeatherJson.wind.deg;
      Today.WindSpeed = CurrentWeatherJson.wind.speed * 3.6;

      Today.ConditionDate = DateTime.Now.ToUniversalTime();
      Today.Humidity = CurrentWeatherJson.main.humidity;
      Today.Pressure = CurrentWeatherJson.main.pressure;
      Today.Temperature = CurrentWeatherJson.main.temp;
      Today.High = CurrentWeatherJson.main.temp_max;
      Today.Low = CurrentWeatherJson.main.temp_min;

    }

    private void SetWeatherInfo(WeatherForcast ThisWeather, dynamic WeatherJson) {

      var WeatherInfo = WeatherJson.list;
      DateTime LastForcast = DateTime.Now;
      for (var i = 0; i < WeatherInfo.Count; i++) {
        WeatherCondition ThisCondition = new WeatherCondition();
        try {
          // Unix timestamp is seconds past epoch
          Double UnixTimeStamp = WeatherInfo[i].dt;
          System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
          ThisCondition.ConditionDate = dtDateTime.AddSeconds(UnixTimeStamp).ToLocalTime();
          ThisCondition.ConditionText = WeatherInfo[i].weather[0].main;
          ThisCondition.ConditionCode = WeatherInfo[i].weather[0].icon;
          ThisCondition.Humidity = WeatherInfo[i].humidity;
          ThisCondition.Visibility = 0;
          ThisCondition.Pressure = WeatherInfo[i].pressure;
          ThisCondition.Temperature = WeatherInfo[i].temp.day;
          ThisCondition.High = WeatherInfo[i].temp.max;
          ThisCondition.Low = WeatherInfo[i].temp.min;
          ThisCondition.WindDirection = WeatherInfo[i].deg;
          ThisCondition.WindSpeed = WeatherInfo[i].speed * 3.6;
        } catch (Exception e) {
          ThisCondition.ConditionText = e.Message;
        }
        ThisWeather.Forecast.Add(ThisCondition);
        if (i >= 5) return;
      }
    }

    private String getWeatherJson(String WebURL) {
      String json2 = String.Empty;
      using (System.Net.WebClient wc = new System.Net.WebClient()) {
        json2 = wc.DownloadString(WebURL);
      }
      if (String.IsNullOrEmpty(json2)) return String.Empty;
      return json2;
    }

    private CityInfo GetCityByIP(String IPAddress) {
      //Weather Information
      CityInfo ThisCity = new CityInfo();
      ThisCity.Lat = 25.2048;
      ThisCity.Lng = 55.2708;
      //If local server set it to dubai address
      MaxMind.GeoIP2.Responses.CityResponse city = new MaxMind.GeoIP2.Responses.CityResponse();
      String DatabaseLocation = HttpContext.Current.Server.MapPath("/GeoLiteCity/GeoLite2-City.mmdb");

      using (var reader = new DatabaseReader(DatabaseLocation)) {
        // Replace "City" with the appropriate method for your database, e.g.,
        // "Country".
        try {
          city = reader.City(IPAddress);
          
          ThisCity.Country = city.Country.Name; // 'United States'
          ThisCity.City = city.City.Name;
          ThisCity.Lat = city.Location.Latitude == null ? 0 : (Double)city.Location.Latitude;
          ThisCity.Lng = city.Location.Longitude == null ? 0 : (Double)city.Location.Longitude;

        } catch {
          //do any error processing
        }
      }//using(var reader)

      if (String.IsNullOrEmpty(ThisCity.Country))
        ThisCity.Country = "United Arab Emirates";
      if (String.IsNullOrEmpty(ThisCity.City))
        ThisCity.City = "Dubai";

      return ThisCity;
    }
  }


}
