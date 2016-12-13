using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exponent.ADSB {
  public class FlightPosition {
    public String FlightID { get; set; }
    public Double Heading { get; set; }
    public string TailNumber { get; set; }
    public string FlightSource { get; set; }
    public string CallSign { get; set; }
    public Double Lon { get; set; }
    public Double Lat { get; set; }
    public Double Speed { get; set; }
    public Double Altitude { get; set; }
    public DateTime ADSBDate { get; set; }

    public List<Double> History { get; set; }

    public FlightPosition() {
      History = new List<Double>();
    }

  }

  public class FlightDistance {
    public String FromFlightID { get; set; }
    public String ToFlightID { get; set; }
    public Double vDistance { get; set; }
    public Double hDistance { get; set; }
  }


}
