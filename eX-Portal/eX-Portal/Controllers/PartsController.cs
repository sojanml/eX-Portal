using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using System.Data.Entity;
using eX_Portal.exLogic;
namespace eX_Portal.Controllers
{
    public class PartsController : Controller
    {
        public ExponentPortalEntities db = new ExponentPortalEntities();
        // GET: Parts
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult PartsList()
        {

            if (!exLogic.User.hasAccess("PARTS.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "UAS Parts";

            string SQL = "   select a.PartsName,\n    " +
                            "   a.Model,         \n      " +
                            "   b.name            \n     " +
                            "   as ComapnyName ,    \n    " +
                            "  Count(*) Over() as _TotalRecords,\n" +
                            "  [PartsId] as _PKey\n" +
                            "   from mstr_parts a   \n   " +
                            "   left              \n     " +
                            "   join                \n   " +
                            "   MSTR_Account b   \n      " +                            
                            "   on a.SupplierId = b.AccountId   \n  ";
                       
           

            qView nView = new qView(SQL);
             nView.addMenu("Detail", Url.Action("Detail", new { ID = "_PKey" }));
              nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
          nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));

            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }//Partslist

        public ActionResult Detail([Bind(Prefix = "ID")] int PartsId)
        {
            if (!exLogic.User.hasAccess("PARTS.VIEW")) return RedirectToAction("NoAccess", "Home");

            Models.MSTR_Parts Parts = db.MSTR_Parts.Find(PartsId);
            if (Parts == null) return RedirectToAction("Error", "Home");
            ViewBag.Title = Parts.PartsId;
            return View(Parts);
        }//AccountDe
        // GET: Parts/Details/5
        public String PartsDetails(int id)
        {
            if (!exLogic.User.hasAccess("PARTS.VIEW"))   return Util.jsonStat("ERROR", "Access Denied");
            string SQL = "   select a.PartsName,\n    " +
                       "   a.Model,         \n      " +
                        "   a.description,      \n     " +
                       "   b.name            \n     " +
                      
                       "   as ComapnyName     \n    " +
                       "   from mstr_parts a   \n   " +
                       "   left              \n     " +
                       "   join                \n   " +
                       "   MSTR_Account b   \n      " +
                       "   on a.SupplierId = b.AccountId   \n  " +
                       "   where  a.PartsId=" + id;

            qDetailView nView = new qDetailView(SQL);
            ViewBag.Message = nView.getTable();

            return nView.getTable();
        }

        // GET: Parts/Create
        public ActionResult Create()
        {
            if (!exLogic.User.hasAccess("PARTS.CREATE")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Create Parts";
            MSTR_Parts Parts = new MSTR_Parts();
         
            return View(Parts);
           
        }

        // POST: Parts/Create
        [HttpPost]
        public ActionResult Create(MSTR_Parts Parts)
        {
            try
            {
                if (!exLogic.User.hasAccess("PARTS.CREATE")) return RedirectToAction("NoAccess", "Home");
                if (Parts.SupplierId < 1 || Parts.SupplierId == null) ModelState.AddModelError("SupplierId", "You must select a Company");


                if (ModelState.IsValid)
                {
                    int ID = 0;
                    Parts.CreatedBy = Util.getLoginUserID();
                    Parts.CreatedOn = DateTime.Now;
                    db.MSTR_Parts.Add(Parts);

                    db.SaveChanges();

                    db.Dispose();

                    return RedirectToAction("Detail", "Parts", new { ID = Parts.PartsId});
                }
                else
                {
                    ViewBag.Title = "Create Drone Flight";
                    return View(Parts);
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Parts/Edit/5
        public ActionResult Edit(int id)
        {
            if (!exLogic.User.hasAccess("PARTS.EDIT")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "Edit Parts";
            ExponentPortalEntities db = new ExponentPortalEntities();
          MSTR_Parts Parts = db.MSTR_Parts.Find(id);
           
            return View(Parts);
                       
        }

        // POST: Parts/Edit/5
        [HttpPost]
        public ActionResult Edit(MSTR_Parts Parts)
        {
           // try
            {
                // TODO: Add update logic here

                if (!exLogic.User.hasAccess("PARTS.EDIT")) return RedirectToAction("NoAccess", "Home");

                ViewBag.Title = "Edit Parts";
                Parts.ModifiedBy = Util.getLoginUserID();
                Parts.ModifiedOn = DateTime.Now;
                db.Entry(Parts).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PartsList", "Parts");

               
            }
           // catch
            {
              //  return View(Parts);
            }
        }

 
        public string Delete([Bind(Prefix = "ID")]int PartsId = 0)
        {
            try
            {
                // TODO: Add delete logic here

                if (!exLogic.User.hasAccess("PARTS.DELETE")) return Util.jsonStat("ERROR", "Access Denied");
                string    SQL = "select count(*) from M2M_DroneParts where PartsId=" + PartsId;
                if (Util.getDBInt(SQL) != 0)
                    return Util.jsonStat("ERROR", "You can not delete  the Parts Attached to UAS ");


                SQL = "select COUNT(*) from M2M_DroneServiceParts where PartsId=" + PartsId;
                if (Util.getDBInt(SQL) != 0)
                    return Util.jsonStat("ERROR", "You can not delete  the Parts Attached UAS Service ");

                SQL = "Delete from mstr_parts where partsid=" + PartsId;
                Util.doSQL(SQL);
                return Util.jsonStat("OK");
            }
            catch
            {
                return Util.jsonStat("error");
            }
        }
    }
}
