using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {
  public class DroneView {

    private List<SelectListItem> SelectListItems = new List<SelectListItem> {
      new SelectListItem {Value = "", Text="Please Select..."},
      new SelectListItem {Value = "DCAA", Text="DCAA"},
      new SelectListItem {Value = "DOT", Text="DOT"},
      new SelectListItem {Value = "GCAA", Text="GCAA"}
    };

    public IEnumerable<SelectListItem> OwnerList { get; set; }
    public MSTR_Drone Drone { get; set; }   
    public IEnumerable<SelectListItem> ManufactureList { get; set; }
    public IEnumerable<SelectListItem> UAVTypeList { get; set; }
    public IEnumerable<SelectListItem> PartsGroupList { get; set; }
    public IEnumerable<SelectListItem> DronePartsList { get; set; }
    public IEnumerable<string> SelectItemsForParts { set; get; }
    public MSTR_Parts DroneParts { get; set; }
    public string Name {get;set;}
    public IEnumerable<SelectListItem> RegistrationAuthority {
      get {
        return SelectListItems;
      }
    }
  }

}
