using MaxMind.GeoIP2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Exponent {
  public class WeatherAPI {
    private const String APIKey = "e925e5f9b310f96d54cb45e01bfb3fe0";
    private const String APIUrl = "http://api.openweathermap.org/data/2.5/";

    private void getStation(Double Lat, Double Lng) {
      String URL = "station/find?lat=55&lon=37&cnt=30&APIKey=";
    }

    public WeatherForcast GetByIP(String IPAddress) {
      WeatherForcast ThisWeather = new WeatherForcast();
      CityInfo ThisCity = GetCityByIP(IPAddress);
      String URL = APIUrl + "forecast/daily?APIKey=" + APIKey + "&q=" + ThisCity.City + "," + ThisCity.Country;
      ThisWeather.Country = ThisCity.Country;
      ThisWeather.City = ThisCity.City;
      String JsonData = getWeatherJson(URL);
      dynamic WeatherJson = JObject.Parse(JsonData);
      SetWeatherInfo(ThisWeather, WeatherJson);


      //Setup todays wether information
      URL = APIUrl + "station/find?APIKey=" + APIKey + "&lat=" + ThisCity.Lat + "&lon=" + ThisCity.Lng + "&cnt=4";
      String WeatherStation = getWeatherJson(URL);
      dynamic WeatherStationJson = JObject.Parse("{\"data\":" + WeatherStation + "}");
      SetWeatherStation(ThisWeather.Today, WeatherStationJson);
      return ThisWeather;
    }

    private void SetWeatherStation(WeatherCondition Today, dynamic WeatherJson) {
      var Data = WeatherJson.data[0];
      
      Today.Humidity = Data.last.main.humidity;
      Today.Visibility = Data.last.visibility.distance;
      Today.Pressure = Data.last.main.pressure;
      Today.WindDirection = Data.last.wind.deg;
      Today.WindSpeed = Data.last.wind.speed;
    }

    private void SetWeatherInfo(WeatherForcast ThisWeather, dynamic WeatherJson) {
      try {
        var WeatherInfo = WeatherJson.list;
        DateTime LastForcast = DateTime.Now;
        for (var i = 0; i < WeatherInfo.Count; i++) {
          WeatherCondition ThisCondition = new WeatherCondition();

          // Unix timestamp is seconds past epoch
          Double UnixTimeStamp = WeatherInfo[i].dt;
          System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
          ThisCondition.ConditionDate = dtDateTime.AddSeconds(UnixTimeStamp).ToLocalTime();
          ThisCondition.ConditionText = WeatherInfo[i].weather[0].main;
          ThisCondition.ConditionCode = WeatherInfo[i].weather[0].icon;
          ThisCondition.Humidity = WeatherInfo[i].humidity;
          ThisCondition.Visibility = 0;
          ThisCondition.Pressure = WeatherInfo[i].pressure;
          ThisCondition.Sunrise = "";
          ThisCondition.Sunset = "";
          ThisCondition.Temperature = WeatherInfo[i].temp.day;
          ThisCondition.High = WeatherInfo[i].temp.min;
          ThisCondition.Low = WeatherInfo[i].temp.max;
          ThisCondition.WindDirection = WeatherInfo[i].deg;
          ThisCondition.WindSpeed = WeatherInfo[i].speed;

          if(i == 0) {
            ThisWeather.Today = ThisCondition;
          } else {
            ThisWeather.Forecast.Add(ThisCondition);
          }
          if (i >= 5) return;
        }
      } catch(Exception e) {
        
      }


    }

    private  String getWeatherJson(String WebURL) {
      String json2 = String.Empty;
      try {
        using (var webClient = new System.Net.WebClient()) {
          webClient.Encoding = System.Text.Encoding.UTF8;
          json2 = webClient.DownloadString(WebURL);
          if (String.IsNullOrEmpty(json2)) return String.Empty;
        }//using(var webClient)
      } catch {
        //nothing - do not show the error
      }
      return json2;
    }

    private CityInfo GetCityByIP(String IPAddress) {
      //Weather Information
      CityInfo ThisCity = new CityInfo();

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
