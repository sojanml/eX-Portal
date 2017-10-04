using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public class ProximityInfo {

    public Double Lat { get; set; }
    public Double Lng { get; set; }
    public Double Distance { get; set; }
    public Double Altitude { get; set; }

    public int FlightID { get; set; }
    public int PilotID { get; set; }
    public int AccountID { get; set; } = 0;
    public String DroneName { get; set; }
    public String AccountName { get; set; }
    public String PilotName { get; set; }
    public String Location { get; set; }

    public bool IsProximityWarning { get; set; } = false;
    public bool IsProximityCritical { get; set; } = false;

  }
}