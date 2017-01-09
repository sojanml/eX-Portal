using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.ViewModel
{
    public class TrafficViewModel
    {
        public MSTR_Drone MSTR_Drone { get; set; }
        public DroneFlight DroneFlight { get; set; }
        public DroneFlightVideo DroneFlightVideo { get; set; }
    }
}