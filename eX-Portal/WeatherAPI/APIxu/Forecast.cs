using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Exponent.APIxu {
  public static class IP {
    public static String Fix(String IPAddress) {
      switch (IPAddress) {
      case "::1":
        return "86.98.146.170";
      }
      return IPAddress; 
    }
  }

  public static class Pipe {
    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Dictionary<int, String> OpenWeaterMapCode = new Dictionary<int, String>{
      {1000,"01d"},
      {1003,"01n"},
      {1006,"03d"},
      {1009,"03d"},
      {1030,"50d"},
      {1063,"10d"},
      {1066,"13n"},
      {1069,"13d"},
      {1072,"13d"},
      {1087,"11n"},
      {1114,"13d"},
      {1117,"50d"},
      {1135,"50d"},
      {1147,"50d"},
      {1150,"10d"},
      {1153,"10d"},
      {1168,"10d"},
      {1171,"13d"},
      {1180,"10d"},
      {1183,"10d"},
      {1186,"10d"},
      {1189,"10d"},
      {1192,"10d"},
      {1195,"10d"},
      {1198,"10d"},
      {1201,"10d"},
      {1204,"13n"},
      {1207,"13d"},
      {1210,"13n"},
      {1213,"13n"},
      {1216,"13n"},
      {1219,"13n"},
      {1222,"13d"},
      {1225,"13d"},
      {1237,"13d"},
      {1240,"10d"},
      {1243,"10d"},
      {1246,"10d"},
      {1249,"13d"},
      {1252,"13d"},
      {1255,"13d"},
      {1258,"13d"},
      {1261,"13d"},
      {1264,"13d"},
      {1273,"11d"},
      {1276,"11d"},
      {1279,"11d"},
      {1282,"11d"}
    };


    public static void SetToday(Current Condition, WeatherCondition TheDay) { 
      TheDay.ConditionCode = OpenWeaterMapCode.ContainsKey(Condition.condition.code) ?
       OpenWeaterMapCode[Condition.condition.code] : "01d";      
      TheDay.ConditionText = Condition.condition.text;
      TheDay.Temperature = Condition.temp_c;
      TheDay.Humidity = Condition.humidity;
      TheDay.Pressure = Condition.pressure_mb;
      TheDay.Visibility = Condition.vis_km;
      TheDay.WindDirection = Condition.wind_degree;
      TheDay.WindSpeed = Condition.wind_kph;
    }

    public static List<WeatherCondition> GetForcast(Forecast AllForcast) {
      List<WeatherCondition> AllConditions = new List<WeatherCondition>();
      foreach (Forecastday ThisForcastDay in AllForcast.forecastday) {
        WeatherCondition ThisCondition = new WeatherCondition();

        ThisCondition.ConditionCode = OpenWeaterMapCode.ContainsKey(ThisForcastDay.day.condition.code) ?
          OpenWeaterMapCode[ThisForcastDay.day.condition.code] : "01d";
        ThisCondition.ConditionDate = epoch.AddSeconds(ThisForcastDay.date_epoch);
        ThisCondition.ConditionText = ThisForcastDay.day.condition.text;
        ThisCondition.High = ThisForcastDay.day.maxtemp_c;
        ThisCondition.Humidity = ThisForcastDay.day.avghumidity;
        ThisCondition.Low = ThisForcastDay.day.mintemp_c;
        ThisCondition.Pressure = ThisForcastDay.hour[0].pressure_mb;
        ThisCondition.Sunrise = ThisForcastDay.astro.sunrise;
        ThisCondition.Sunset = ThisForcastDay.astro.sunset;
        ThisCondition.Temperature = ThisForcastDay.day.avgtemp_c;
        ThisCondition.WindDirection = ThisForcastDay.hour[0].wind_degree;
        ThisCondition.WindSpeed = ThisForcastDay.hour[0].wind_kph;
        AllConditions.Add(ThisCondition);
      }
      return AllConditions;
    }


  }

  public class Location {
    public string name { get; set; }
    public string region { get; set; }
    public string country { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
    public string tz_id { get; set; }
    public int localtime_epoch { get; set; }
    public string localtime { get; set; }
  }

  public class Condition {
    public string text { get; set; }
    public string icon { get; set; }
    public int code { get; set; }
  }

  public class Current {
    public int last_updated_epoch { get; set; }
    public string last_updated { get; set; }
    public double temp_c { get; set; }
    public double temp_f { get; set; }
    public Condition condition { get; set; }
    public double wind_mph { get; set; }
    public double wind_kph { get; set; }
    public int wind_degree { get; set; }
    public string wind_dir { get; set; }
    public double pressure_mb { get; set; }
    public double pressure_in { get; set; }
    public double precip_mm { get; set; }
    public double precip_in { get; set; }
    public int humidity { get; set; }
    public int cloud { get; set; }
    public double feelslike_c { get; set; }
    public double feelslike_f { get; set; }

    public double vis_km { get; set; }
    public double vis_miles { get; set; }
  }



  public class Day {
    public double maxtemp_c { get; set; }
    public double maxtemp_f { get; set; }
    public double mintemp_c { get; set; }
    public double mintemp_f { get; set; }
    public double avgtemp_c { get; set; }
    public double avgtemp_f { get; set; }
    public double maxwind_mph { get; set; }
    public double maxwind_kph { get; set; }
    public double totalprecip_mm { get; set; }
    public double avgvis_km { get; set; }
    public double avgvis_miles { get; set; }
    public double avghumidity { get; set; }
    public double totalprecip_in { get; set; }

    public Condition condition { get; set; }
  }

  public class Astro {
    public string sunrise { get; set; }
    public string sunset { get; set; }
    public string moonrise { get; set; }
    public string moonset { get; set; }
  }



  public class Hour {
    public int time_epoch { get; set; }
    public string time { get; set; }
    public double temp_c { get; set; }
    public double temp_f { get; set; }
    public Condition condition { get; set; }
    public double wind_mph { get; set; }
    public double wind_kph { get; set; }
    public int wind_degree { get; set; }
    public string wind_dir { get; set; }
    public double pressure_mb { get; set; }
    public double pressure_in { get; set; }
    public double precip_mm { get; set; }
    public double precip_in { get; set; }
    public int humidity { get; set; }
    public int cloud { get; set; }
    public double feelslike_c { get; set; }
    public double feelslike_f { get; set; }
    public double windchill_c { get; set; }
    public double windchill_f { get; set; }
    public double heatindex_c { get; set; }
    public double heatindex_f { get; set; }
    public double dewpoint_c { get; set; }
    public double dewpoint_f { get; set; }
    public int will_it_rain { get; set; }
    public int will_it_snow { get; set; }
  }

  public class Forecastday {
    public string date { get; set; }
    public int date_epoch { get; set; }
    public Day day { get; set; }
    public Astro astro { get; set; }
    public List<Hour> hour { get; set; }
  }

  public class Forecast {
    public List<Forecastday> forecastday { get; set; }
  }

  public class WeatherModel {
    public Location location { get; set; }
    public Current current { get; set; }
    public Forecast forecast { get; set; }
  }
}
