using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.Text;
using eX_Portal.ViewModel;
using Microsoft.Reporting.WebForms;
using System.Data.Common;
using System.Data;

namespace eX_Portal.Controllers {
  public class TestController : Controller {

    public ActionResult Test() {
      Exponent.WeatherAPI TheWeather = new Exponent.WeatherAPI();
      var Today = TheWeather.GetByLocation(25.2048, 55.2708);
      return View(Today);
    }

  }
}