using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {


  public class BlackBoxViewModel {
    public IEnumerable<SelectListItem> BlackBoxList { get; set; }
    public BlackBoxTransaction BBTransaction { get; set; }

    public IEnumerable<SelectListItem> CollectionMode { get; set; }

  }


}