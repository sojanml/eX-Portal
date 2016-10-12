using System;
using System.Collections.Generic;

namespace Exponent {

  public class CityInfo {
    public String Country { get; set; }
    public String City { get; set; }
    public Double Lat { get; set; }
    public Double Lng { get; set; }
  }
  public class WeatherForcast {

    public string City { get; set; }
    public string Country { get; set; }
    public WeatherCondition Today { get; set; }
    public List<WeatherCondition> Forecast { get; set; }

    public WeatherForcast() {
      Today = new WeatherCondition();
      Forecast = new List<WeatherCondition>();
    }
  }

  public class WeatherCondition {
    public DateTime ConditionDate { get; set; }
    public string ConditionCode { get; set; }
    public string ConditionText { get; set; }
    public Double Humidity { get; set; }
    public Double Visibility { get; set; }
    public Double Pressure { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
    public Double Temperature { get; set; }
    public Double WindDirection { get; set; }
    public Double WindSpeed { get; set; }
    public Double High { get; set; }
    public Double Low { get; set; }
  }
}
