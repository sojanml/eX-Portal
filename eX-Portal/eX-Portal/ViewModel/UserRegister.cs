using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel
{
    public class UserRegister
    {
       
        public IEnumerable<SelectListItem> AccountList { get; set; }
        public string AccountName { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public Nullable<int> AccountId { get; set; }
        public string RPASPermitNo { get; set; }
        public string GeneratedPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}