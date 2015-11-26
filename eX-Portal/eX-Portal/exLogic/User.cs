using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using System.Web.Mvc;
namespace eX_Portal.exLogic
{
    public class User
    {
        public static ExponentPortalEntities db = new ExponentPortalEntities();



        public static int UserValidation(String UserName,String Password)
        {
            int result = 0;
            using (var ctx = new ExponentPortalEntities())
            {
                var _objuserdetail = (from data in ctx.MSTR_User
                                      where data.UserName == UserName
                                      && data.Password == Password

                                      select data);

                result = _objuserdetail.Count();
            }

            return result;
        }


        public static int GetUserId(String UserName)
        {
            int result = 0;
            using (var ctx = new ExponentPortalEntities())
            {
                String SQL = "select UserId from MSTR_User" +
                " where UserName='" + UserName + "'";
                int UserId = ctx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
                result = UserId;
            }

            return result;
        }


        public static string GetFirstName(String UserName)
        {
            string result;
            using (var ctx = new ExponentPortalEntities())
            {
                String SQL = "select FirstName from MSTR_User" +
                " where UserName='" + UserName + "'";
                string FirstName = ctx.Database.SqlQuery<string>(SQL).FirstOrDefault<string>();
                result = FirstName;
            }

            return result;
        }


        public static IList<MenuModel> BuildMenu()
        {

            IList<MenuModel> mmList = new List<MenuModel>();

            var menu_items = db.MSTR_Menu;




            var UserId = System.Web.HttpContext.Current.Session["UserId"] == null ? 0 : System.Web.HttpContext.Current.Session["UserId"];


            String SQL = "select * from MSTR_Menu where MenuId" +
                " in(select b.MenuId from  MSTR_Profile a left join   M2M_UserProfile b" +
                 "  on a.ProfileId = b.ProfileId left join MSTR_User c on b.ProfileId = c.UserProfileId" +
                 "   where c.UserId =" + UserId + ")order by SortOrder asc ";
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