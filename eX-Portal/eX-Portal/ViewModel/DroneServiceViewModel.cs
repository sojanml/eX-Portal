﻿using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel
{
    public class DroneServiceViewModel
    {
        public IEnumerable<SelectListItem> ServiceType { get; set; }
        public IEnumerable<SelectListItem> DroneList { get; set; }
        public MSTR_DroneService DroneService { get; set; }
    }
}