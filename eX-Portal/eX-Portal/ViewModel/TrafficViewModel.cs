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

    public class TrafficDashboard {
        private String _Drone = String.Empty;
        private String _Pilot = String.Empty;
        private String _GSC = String.Empty;
        public int FlightID { get; set; }
        public String Drone {
            get {
                if (String.IsNullOrWhiteSpace(_Drone)) return "No Drone";
                return _Drone;
            }
            set {
                _Drone = value;
            }
        }
        public String Pilot {
            get {
                if (String.IsNullOrWhiteSpace(_Pilot)) return "Not Available";
                return _Pilot;
            }
            set {
                _Pilot = value;
            }
        }
        public String GSC {
            get {
                if (String.IsNullOrWhiteSpace(_GSC)) return "Not Available";
                return _GSC;
            }
            set {
                _GSC = value;
            }
        }
        public String FlightDate { get; set; }
        public String FlightVideo { get; set; }

        public Double Lat { get; set; }
        public Double Lng { get; set; }

        public String DroneIDShort {
            get {
                if (String.IsNullOrWhiteSpace(_Drone)) return "N/A";
                int Index = _Drone.LastIndexOf('-');
                return _Drone.Substring(Index + 1);
            }
        }

    }
    public class TrafficMonitorViewModel
    {
        public MSTR_Drone MSTR_Drone { get; set; }
        public Int64 FlightID { get; set; }
        
    }
}


