using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class AdsbData {

   
    public string FlightId { get; set; }
    public string Heading { get; set; }
    public string TailNumber { get; set; }
    public string FlightSource { get; set; }
    public string CallSign { get; set; }
    public Nullable<double> Lon { get; set; }
    public Nullable<double> Lat { get; set; }
    public Nullable<double> Speed { get; set; }
    public Nullable<double> Altitude { get; set; }
    public string Adsbsdate { get; set; }
    public  string CreatedDate { get; set; }
  }
}