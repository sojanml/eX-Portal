using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class ChartAlertViewModel
    {
        public string AlertType { set; get; }
        public double TotalAlert { set; get; }
        public double CurrentMonthAlert{ set; get; }
        public double LastFlightAlert { set; get; }
    }
}