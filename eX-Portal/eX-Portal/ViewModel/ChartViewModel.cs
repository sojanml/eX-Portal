using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class ChartViewModel {
    public string DroneName { set; get; }
    public string AccountName { set; get; }
    public string ChartColor { set; get; }
    public string ShortName { set; get; }
    public int AccountID { set; get; }
    public int UserID { set; get; }
    public int DroneID { set; get; }
    public double TotalFightTime { set; get; }
    public double CurrentFlightTime { set; get; }
    public string PilotName { set; get; }
    public double PilotTotalHrs { set; get; }
    public double PilotLastFlightHrs { set; get; }
    public double PilotCurrentMonthHrs { set; get; }
    public double LastFlightTime { set; get; }


    public double LastMultiDashHrs { set; get; }
    public double LastFixedwingHrs { set; get; }
    public double LastMonthMultiDashHrs { set; get; }
    public double LastMonthFixedwingHrs { set; get; }
    public double TotalMultiDashHrs { set; get; }
    public double TotalFixedWingHrs { set; get; }

        public double M1 { set; get; }
        public double M2 { set; get; }
        public double M3 { set; get; }
        public double M4 { set; get; }
        public double M5 { set; get; }
        public double M6 { set; get; }
        public double M7 { set; get; }
        public double M8 { set; get; }
        public double M9 { set; get; }
        public double M10 { set; get; }
        public double M11 { set; get; }
        public double M12 { set; get; }
       
    }
}