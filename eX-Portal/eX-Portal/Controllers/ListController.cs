using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
namespace eX_Portal.Controllers
{
    public class ListController : Controller
    {
        // GET: List
        public ActionResult Index()
        {
            return View();
        }

        // GET: List/Details/5
        public ActionResult Details(string Name)
        {




            String SQL = "select name, Count(*) Over() as _TotalRecords,TypeId as _PKey from LUP_Drone where type=" + Name;
            qView nView = new qView(SQL);

          
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
           
        }

        // GET: List/Create
        public ActionResult Create()

        {

            var viewModel = new ViewModel.ListViewModel
            {
               Typelist = Util.LUPTypeList()

            };
            return View(viewModel);
       
        }

        // POST: List/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: List/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: List/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: List/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: List/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
