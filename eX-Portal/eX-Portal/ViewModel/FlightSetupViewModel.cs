using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.ViewModel
{
    public class FlightSetupViewModel
    {
        public MSTR_Drone_Setup DroneSetup { get; set; }
        public GCA_Approval GcaApproval { get; set; }
    }
}