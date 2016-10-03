using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class AdsbData {

   
    public string FlightId { get; set; }
    public string heading { get; set; }
    public string ident { get; set; }
    public string FlightSource { get; set; }
    public string CallSign { get; set; }
    public Nullable<double> longitude { get; set; }
    public Nullable<double> latitude { get; set; }
    public Nullable<double> speed { get; set; }
    public Nullable<double> altitude { get; set; }
    public DateTime AdsbsDate { get; set; }
    public  DateTime CreatedDate { get; set; }
  }
}