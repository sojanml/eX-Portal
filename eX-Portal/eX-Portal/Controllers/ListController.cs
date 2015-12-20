using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Text;

namespace eX_Portal.Controllers {
  public class ListController : Controller {
    // GET: List
    public ActionResult Index() {
      return View();
    }

    public PartialViewResult ListNames(ViewModel.ListViewModel list, String TypeName) {
      ExponentPortalEntities db = new ExponentPortalEntities();
      var result = from r in db.LUP_Drone
                   where r.Type == TypeName & (bool)r.IsActive
                   orderby r.Name
                   select r;
      list.NameList = result;
      list.TypeCopy = TypeName;
      return PartialView("LookupDetails", list);
    }

    // GET: List/Details/5
    public ActionResult Details(string TypeName) {
      String SQL = "select name, Count(*) Over() as _TotalRecords,TypeId as _PKey from LUP_Drone where type='" + TypeName + "'";
      qView nView = new qView(SQL);

      if (Request.IsAjaxRequest()) {
        Response.ContentType = "text/javascript";
        return PartialView("qViewData", nView);
      } else {
        return View(nView);
      }//if(IsAjaxRequest)
    }

    // GET: List/Create
    public ActionResult Create() {
      if (!exLogic.User.hasAccess("lOOKUP.CREATE")) return RedirectToAction("NoAccess", "Home");
      var viewModel = new ViewModel.ListViewModel {
        Typelist = Util.LUPTypeList()
      };
      return View(viewModel);
    }

    // POST: List/Create
    [HttpPost]
    public String Update(ViewModel.ListViewModel ListView) {
      try {
        ListView.TypeCopy = ListView.TypeCopy.Replace(" ", "");
        ExponentPortalEntities db = new ExponentPortalEntities();
        int TypeId = Util.GetTypeId(ListView.TypeCopy);
        string BinaryCode = Util.DecToBin(TypeId);
        //checking for upadation or new record creation
        if (ListView.Id == 0) {
          if (!exLogic.User.hasAccess("lOOKUP.CREATE")) return Util.jsonStat("error", "Access Denied");
          string SQL = "INSERT INTO LUP_DRONE(\n" +
            "  Type,\n" +
            "  Code,\n" +
            "  TypeId,\n" +
            "  BinaryCode,\n" +
            "  Name,\n" +
            "  CreatedBy,\n" +
            "  CreatedOn,\n" +
            "  IsActive\n" +
            ") VALUES(\n" +
            "  '" + ListView.TypeCopy + "',\n" +
            "  '" + ListView.Code + "',\n" +
            "  " + TypeId + ",\n" +
            "  '" + BinaryCode + "',\n" +
            "  '" + ListView.Name + "',\n" +
            "  " + Util.getLoginUserID() + ",\n" +
            "  GETDATE(),\n" +
            "  'True'" +
            ")";
          int ListId = Util.InsertSQL(SQL);
          return JSonData(ListView);
        } else {
          if (!exLogic.User.hasAccess("lOOKUP.EDIT")) return Util.jsonStat("error", "Access Denied");
          string SQL = "UPDATE LUP_DRONE SET\n" +
            "  Type='" + ListView.TypeCopy + "',\n" +
            "  Code='" + ListView.Code + "',\n" +
            "  Name='" + ListView.Name + "',\n" +
            "  ModifiedBy=" + Util.getLoginUserID() + ",\n" +
            "  ModifiedOn=GETDATE()\n" +
            "where\n" +
            "  Id=" + ListView.Id;
          int ListId = Util.doSQL(SQL);
          return JSonData(ListView);
        }
        // return RedirectToAction("Listnames");

      } catch (Exception ex) {
        return Util.jsonStat("error", ex.Message);
      }
    }

    private String JSonData(ViewModel.ListViewModel ListView) {
      StringBuilder JSON = new StringBuilder();
      JSON.Append("{");
      JSON.AppendLine("\"status\": \"ok\",");
      JSON.AppendLine("\"id\": \"" + ListView.Id.ToString() + "\",");
      JSON.AppendLine("\"code\": \"" + ListView.Code.ToString() + "\",");
      JSON.AppendLine("\"name\": \"" + ListView.Name.ToString() + "\"");
      JSON.Append("}");
      return JSON.ToString();
    }
    // GET: List/Edit/5
    public ActionResult Edit(int id) {
      return View();
    }

    // POST: List/Edit/5
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection) {
      try {
        // TODO: Add update logic here
        return RedirectToAction("Index");
      } catch {
        return View();
      }
    }

    // GET: List/Delete/5
    public String Delete([Bind(Prefix = "ID")]int LupID = 0) {
      try {
        //if (!exLogic.User.hasAccess("lOOKUP.DELETE")) return RedirectToAction("NoAccess", "Home");
        String SQL = "UPDATE [LUP_Drone] SET IsActive = 0 WHERE Id = " + LupID;
        Util.doSQL(SQL);
        Response.ContentType = "text/json";
        return Util.jsonStat("OK");

      } catch (Exception ex) {
        return Util.jsonStat("ERROR", ex.Message);
      }
    }

  }
}
