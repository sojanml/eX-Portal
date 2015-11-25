using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
namespace eX_Portal.Controllers
{
    public class DesignController : Controller
    {
        // GET: Design


        public DesignController()
        {
            ViewBag.Menu = BuildMenu();
       
       

          

        }
       

        public ActionResult SystemMenu()
        {

            var MenuList = BuildMenu();
            return View(MenuList);
        }

        private ExponentPortalEntities db = new ExponentPortalEntities();
        private IList<MenuModel> BuildMenu()
        {

            IList<MenuModel> mmList = new List<MenuModel>();
            var menu_items = db.MSTR_Menu;

           

          
                var UserId = System.Web.HttpContext.Current.Session["UserId"] == null ? 0 : System.Web.HttpContext.Current.Session["UserId"];
          
                
            String SQL = "select * from MSTR_Menu where MenuId" +
                " in(select b.MenuId from  MSTR_Profile a left join   M2M_UserProfile b" +
                 "  on a.ProfileId = b.ProfileId left join MSTR_User c on b.ProfileId = c.UserProfileId" +
                 "   where c.UserId ="+ UserId + ")";
          var studentName = db.Database.SqlQuery<MSTR_Menu>(SQL);
          

            foreach (MSTR_Menu mnu in studentName)
            {

                MenuModel model = new MenuModel();

                model.Id = mnu.MenuId;
                model.Name = mnu.MenuName;
                model.ParentId = mnu.ParentId.GetValueOrDefault();
                model.SortOrder = mnu.SortOrder.GetValueOrDefault();
                model.PageUrl = mnu.PageUrl;

                mmList.Add(model);
            }




          

            return mmList;
        }
    }
}