using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class ChartViewModel {
    public string DroneName { set; get; }
    public double TotalFightTime { set; get; }
    public double CurrentFlightTime { set; get; }
        public string PilotName { set; get; }
        public double PilotTotalHrs { set; get; }
        public double PilotLastFlightHrs { set; get; }
        public double PilotCurrentMonthHrs { set; get; }
        public double LastFlightTime { set; get; }
    }
}