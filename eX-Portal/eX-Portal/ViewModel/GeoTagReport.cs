using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class GeoTagReport
    {
      public Nullable< decimal> Latitude { get; set; }
       public Nullable<decimal> Longitude { get; set; }
       public Nullable<decimal> Altitude { get; set; }                   
      public string DocumentName { get; set; }
      public Nullable<int> FlightID { get; set; }
      public   Nullable<System.DateTime> UpLoadedDate { get; set; }                          
      public string DroneName { set; get; }
      public int ID { set; get; }
    }
}