using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace eX_Portal {
  public class MvcApplication : System.Web.HttpApplication {


    protected void Application_Start() {
      //SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    public override string GetVaryByCustomString(HttpContext context, string arg) {
      if (arg.Equals("User", StringComparison.InvariantCultureIgnoreCase)) {
        String SessionKey = "uid";
        var TheCookies = context.Request.Cookies;
        if (TheCookies.AllKeys.Contains(SessionKey)) return TheCookies[SessionKey].Value;
        return "0";
      }
      return base.GetVaryByCustomString(context, arg);
    }

    protected void Session_Start(Object sender, EventArgs e) {
      if (Session["uid"] != null) {
        //  Console.WriteLine("Session Started");
      }
    }

    protected void Session_End(Object sender, EventArgs e) {
      if (Session["uid"] != null) {
        string sql = "update userlog set IsSessionEnd=1 where ID='" + Session["uid"] + "'";
        exLogic.Util.doSQL(sql);
      }
    }
  }
}
