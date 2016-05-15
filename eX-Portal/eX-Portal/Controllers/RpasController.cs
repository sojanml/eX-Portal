using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers
{
    public class RpasController : Controller
    {
        private ExponentPortalEntities db = new ExponentPortalEntities();

        // GET: Rpas
        public ActionResult Index()
        {
            string SQL = "SELECT MSTR_RPAS_User.Name as [FullName],\n"+
                         "LUP_Drone.Name AS Nationality,\n"+
                         "MSTR_RPAS_User.EmiratesId as [EmiratesID],\n"+
                         "MSTR_RPAS_User.EmailId as [Email],\n"+
                         "MSTR_RPAS_User.MobileNo as [MobileNo],\n"+
                         "MSTR_RPAS_User.Status,\n"+
                         "Count(*) Over() as _TotalRecords,\n"+
                         "RpasId as _PKey\n"+
                         "FROM MSTR_RPAS_User INNER JOIN LUP_Drone\n"+
                         "ON MSTR_RPAS_User.NationalityId = LUP_Drone.TypeId\n"+
                         "where LUP_Drone.Type = 'Country'";

            qView nView = new qView(SQL);
            //if (exLogic.User.hasAccess("PILOTLOG.VIEW"))
            nView.addMenu("Create User", Url.Action("Create", "User", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)
            //return View(db.MSTR_RPAS_User.ToList());
        }

        // GET: Rpas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Rpas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(MSTR_RPAS_User mSTR_RPAS_User)
        {
            if (mSTR_RPAS_User.NationalityId == null) ModelState.AddModelError("NationalityId", "Please select your nationality..");
            if (ModelState.IsValid)
            {               
                mSTR_RPAS_User.Status = "New User Request";
                mSTR_RPAS_User.CreatedBy = Convert.ToInt32(Session["UserId"].ToString());
                mSTR_RPAS_User.CreatedOn = System.DateTime.Now;
                db.MSTR_RPAS_User.Add(mSTR_RPAS_User);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_RPAS_User);
        }

        // POST: Rpas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RpasId,Name,NationalityId,EmiratesId,EmailId,MobileNo,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] MSTR_RPAS_User mSTR_RPAS_User)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mSTR_RPAS_User).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mSTR_RPAS_User);
        }

        // GET: Rpas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            if (mSTR_RPAS_User == null)
            {
                return HttpNotFound();
            }
            return View(mSTR_RPAS_User);
        }

        // POST: Rpas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MSTR_RPAS_User mSTR_RPAS_User = db.MSTR_RPAS_User.Find(id);
            db.MSTR_RPAS_User.Remove(mSTR_RPAS_User);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
