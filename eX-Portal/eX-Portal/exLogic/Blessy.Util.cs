using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public partial class Util {
    public static string RandomPassword() {
      string paswd = System.Web.Security.Membership.GeneratePassword(7, 1).ToString();
      return paswd;
    }
  }
}