using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using eX_Portal.Models;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {
  public class UserViewModel {

    public MSTR_User User { get; set; }
    public IEnumerable<SelectListItem> ProfileList { get; set; }
    public IEnumerable<SelectListItem> CountryList { get; set; }
    public IEnumerable<SelectListItem> AccountList { get; set; }

  }


}