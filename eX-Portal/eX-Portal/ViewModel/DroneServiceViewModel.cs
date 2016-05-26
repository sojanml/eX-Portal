using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel
{
    public class DroneServiceViewModel
    {
        public string DroneID { set; get; }
        public IEnumerable<string> SelectItemsForRefurbished { set; get; }
       public IEnumerable<string> SelectItemsForReplaced { set; get; }
        public IEnumerable<SelectListItem> ServiceType { get; set; }
        public IEnumerable<SelectListItem> DroneList { get; set; }
        public IEnumerable<SelectListItem> DronePartsList { get; set; }
        public MSTR_DroneService DroneService { get; set; }
        public MSTR_Parts DroneParts { get; set; }
    }
}