using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class ChartViewModel
    {
        public string DroneName { set; get;}
        public int TotalFightTime { set; get;}
        public int CurrentFlightTime { set; get;}
        public string PilotName { set; get; }
        public int PilotTotalHrs { set; get; }
        public int PilotCurrentMonthHrs { set; get; }
    }
}