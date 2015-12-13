﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace eX_Portal {
  public class MvcApplication : System.Web.HttpApplication {


    protected void Application_Start() {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    public override string GetVaryByCustomString(HttpContext context, string arg) {
      if (arg.Equals("User", StringComparison.InvariantCultureIgnoreCase)) {
        return context.Session["UserID"] == null ? "" : context.Session["UserID"].ToString();
      }
      return base.GetVaryByCustomString(context, arg);
    }
  }
}
