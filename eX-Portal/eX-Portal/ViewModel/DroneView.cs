using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel
{
    public class DroneView
    {
        public IEnumerable<SelectListItem> OwnerList { get; set; }
        public MSTR_Drone Drone { get; set; }

        public IEnumerable<SelectListItem> ManufactureList { get; set; }
        public IEnumerable<SelectListItem> UAVTypeList { get; set; }
        public IEnumerable<SelectListItem> PartsGroupList { get; set; }
        


    }
}