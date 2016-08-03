using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class TestViewModel
    {
        public string DroneName { set; get; }
      
        public double LastMultiDashHrs { set; get; }
        public double LastFixedwingHrs    { set; get; }
        public double LastMonthMultiDashHrs     { set; get; }
        public double LastMonthFixedwingHrs { set; get; }
        public double TotalMultiDashHrs { set; get; }
        public double TotalFixedWingHrs { set; get; }

        public string PilotName { set; get; }
     
    }
}