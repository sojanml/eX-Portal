using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
namespace eX_Portal.Controllers
{
    public class DesignController : Controller
    {
        // GET: Design

        public DesignController()
        {
            ViewBag.Menu = BuildMenu();
        }
       


        private ExponentPortalEntities db = new ExponentPortalEntities();
        private IList<MenuModel> BuildMenu()
        {

            IList<MenuModel> mmList = new List<MenuModel>();
            var menu_items = db.MSTR_Menu;
            foreach (MSTR_Menu mnu in menu_items)
            {

                MenuModel model = new MenuModel();

                model.Id = mnu.MenuId;
                model.Name = mnu.MenuName;
                model.ParentId = mnu.ParentId.GetValueOrDefault();
                model.SortOrder = mnu.SortOrder.GetValueOrDefault();
                mmList.Add(model);
            }




          

            return mmList;
        }
    }
}