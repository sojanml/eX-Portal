using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Reflection;

namespace eX_Portal.exLogic {
  public class UserDones {
    public int DroneID { get; set; }
    public String DroneName { get; set; }
    public String Manufacture { get; set; }
    public String UavTypeGroup { get; set; }
    public String UavType { get; set; }
    public int? LinkDroneID { get; set; }

    public String Checked {
      get {
        return (DroneID == LinkDroneID ? "checked" : "");
      }
    }
  }


  public static class OrganisationAdmin {
    public static OrganisationAdminModel GetOrganisationDashboard(int UserID) {
      Util ut = new Util();
      ViewModel.UserDashboardModel UserDashboard = ut.GetUserDetails(UserID, true);

      OrganisationAdminModel ThisDashboard = new OrganisationAdminModel(UserDashboard);
      ThisDashboard.OrganisationPilots = GetOrganizationPilots();
      return ThisDashboard;
    }

    private  static List<Models.MSTR_User> GetOrganizationPilots() {
      var DB = new Models.ExponentPortalEntities();
      int AccountID = Util.getAccountID();
      var Query = from u in DB.MSTR_User
                  where u.AccountId == AccountID
                  orderby u.FirstName
                  select u;
      var TheList =  Query.ToList();
      //If no photo image, skip it
      foreach(var Usr in TheList) {
        if (String.IsNullOrEmpty(Usr.PhotoUrl)) {
          Usr.PhotoUrl = "/images/PilotImage.png";
        } else {
          Usr.PhotoUrl = $"/Upload/User/{Usr.UserId}/{Usr.PhotoUrl}";
          if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(Usr.PhotoUrl)))
            Usr.PhotoUrl = "/images/PilotImage.png";
        }
      }//foreach(var Usr in TheList)

      return TheList;
    }

  }

  public class OrganisationAdminModel : ViewModel.UserDashboardModel {
    public List<Models.MSTR_User> OrganisationPilots = new List<Models.MSTR_User>();

    public OrganisationAdminModel(ViewModel.UserDashboardModel userDashboardModel) {
      // Iterate the Properties of the destination instance and  
      // populate them from their source counterparts  
      PropertyInfo[] destinationProperties = this.GetType().GetProperties();
      foreach (PropertyInfo destinationPi in destinationProperties) {
        PropertyInfo sourcePi = userDashboardModel.GetType().GetProperty(destinationPi.Name);
        destinationPi.SetValue(this, sourcePi.GetValue(userDashboardModel, null), null);
      }
    }

  }

  
}