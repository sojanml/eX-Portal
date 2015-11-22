using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace eX_Portal.Controllers {
  public class ProductController : Controller {
    // GET: Product


    public ActionResult Index() {

      ViewBag.Title = "Product Listing";

      String SQL = "SELECT \n" +
        "  ProductId, \n" +
        "  LEFT(RFID,5) as RFID,\n" +
        "  DecimalCode,\n" +
        "  BinaryCode,\n" +
        "  IsAssigned,\n" +
        "  IsActive,\n" +
        "  RecordType,\n" +
        "  Product_Name,\n" +
        "  SerialNo,\n" +
        "  AccountId,\n" +
        "  Count(*) Over() as _TotalRecords\n" +
        "FROM\n" +
        "  MSTR_Product";
      qView nView = new qView(SQL);
      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)


    }//ActionResult Index()


  }//class ProductController
}//namespace eX_Portal.Controllers