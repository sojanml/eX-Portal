using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class WeatherViewModel
    {



        public string TemperatureUnit { get; set; }
        public string City { get; set; }
        public string DistanceUnit { get; set; }
        public string  PressureUnit { get; set; }
        public string SpeedUnit { get; set; }
        public string Chill{ get; set; }
        public string Direction { get; set; }
        public string Speed { get; set; }
        public string Humidity { get; set; }
        public string Visibility { get; set; }
        public string Condition { get; set; }
        public string Pressure { get; set; }
        public string PressureStatus { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string TempF { get; set; }

        public string ConditionCode { get; set; }
        public string ConditionText { get; set; }
        public string ConditionTemperature { get; set; }
        public string ConditionDate{ get; set; }
        public string TempC { get; set; }

       public string Lattitude { get; set; }
       public string Longitude { get; set; }
       public string Region { get; set; }
       public string Country { get; set; }







        public string Wind { get; set; }
        

        public string DayOfWeek { get; set; }

       



        public string High { get; set; }




        public string Low { get; set; }
        public IList<Forcast> Forecast { get; set; }

        //  public ForcastCollection Days = new ForcastCollection();

       

    }
}