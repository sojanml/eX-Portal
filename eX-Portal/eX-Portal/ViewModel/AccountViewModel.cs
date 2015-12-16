using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
namespace eX_Portal.ViewModel
{
    public class AccountViewModel
    {
        public MSTR_Account Account { get; set; }
        public IEnumerable<SelectListItem> CountryList { get; set; }
    }
}