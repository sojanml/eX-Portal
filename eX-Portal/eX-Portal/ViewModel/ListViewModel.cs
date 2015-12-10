using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
namespace eX_Portal.ViewModel
{
    public class ListViewModel
    {
        public IEnumerable<SelectListItem> Typelist { get; set; }
        public LUP_Drone List{get; set; }
    }
}