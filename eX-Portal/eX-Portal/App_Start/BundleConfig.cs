﻿using System.Web;
using System.Web.Optimization;

namespace eX_Portal {
  public class BundleConfig {
    // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
    public static void RegisterBundles(BundleCollection bundles) {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                  "~/Scripts/jquery-1.11.3.js",
                  "~/jquery-ui/jquery-ui.min.js"
                  ));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));

      bundles.Add(new ScriptBundle("~/bundles/dataTableScript").Include(
                "~/dataTable/js/jquery.dataTables.js"
                ));
      bundles.Add(new StyleBundle("~/bundles/dataTableCss").Include(
                "~/dataTable/css/jquery.dataTables.custom.css",
                "~/jquery-ui/jquery-ui.min.css"
                ));

      bundles.Add(new StyleBundle("~/bundles/eXPortalWidget").Include(
                "~/jquery-ui/jquery-ui.min.css",
                "~/Content/eXPortalWidget.css"
                ));
      bundles.Add(new StyleBundle("~/bundles/ADSB").Include(
          "~/jquery-ui/jquery-ui.min.css",
          "~/Content/ADSB.css"
          ));


      bundles.Add(new ScriptBundle("~/bundles/exPortalScript").Include(
                  "~/Scripts/jquery-1.11.3.js",
                  "~/Scripts/jquery.validate*",
                  "~/jquery-ui/jquery-ui.min.js",
                "~/dataTable/js/jquery.dataTables.js",
                "~/Scripts/exPortal.js"
                ));
      bundles.Add(new StyleBundle("~/bundles/exPortalCss").Include(
                "~/dataTable/css/jquery.dataTables.custom.css",
                "~/jquery-ui/jquery-ui.min.css",
                 "~/Content/exPortal.css"
                ));

      bundles.Add(new StyleBundle("~/bundles/exPortalCssV3").Include(
                "~/dataTable/css/jquery.dataTables.custom.css",
                "~/jquery-ui/jquery-ui.min.css",
                 "~/Content/exPortal_V3.css"
                ));

      bundles.Add(new ScriptBundle("~/bundles/exPortalScriptV3").Include(
                  "~/Scripts/jquery-1.11.3.js",
                  "~/Scripts/jquery.validate*",
                  "~/jquery-ui/jquery-ui.min.js",
                "~/dataTable/js/jquery.dataTables.js",
                "~/Scripts/exPortal_V3.js"
                ));

    }
  }
}
