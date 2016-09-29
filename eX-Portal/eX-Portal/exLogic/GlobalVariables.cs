using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public static class Exponent { 
    public const String WorldWeatherOnlineKey = "ebb2026088a34f1089a65727162609";
    public const String WeatherAPI = "http://api.worldweatheronline.com/premium/v1/weather.ashx" + 
      "?key=" + Exponent.WorldWeatherOnlineKey +
      "&format=json&num_of_days=5";
  }
}