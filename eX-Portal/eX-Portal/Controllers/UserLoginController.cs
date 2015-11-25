using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
namespace eX_Portal.Controllers
{
    public class UserLoginController :Controller
    {
        // GET: UserLogin
        public ActionResult Login()
        {
            UserLogin _objuserlogin = new UserLogin();
            return View(_objuserlogin);
        }
        [HttpPost]
        public ActionResult Login(UserLogin _objuserlogin)
        {
            /*Create instance of entity model*/
             ExponentPortalEntities objentity = new ExponentPortalEntities();
            /*Getting data from database for user validation*/

            var _objuserdetail = (from data in objentity.MSTR_User
                                  where data.UserName == _objuserlogin.UserName                                  
                                  && data.Password == _objuserlogin.Password
                                  select data);
                         if (_objuserdetail.Count() > 0)
                             {
                /*Redirect user to success apge after successfull login*/
                                  ViewBag.Message = 1;

                String SQL = "select UserId from MSTR_User" +
               " where UserName='" + _objuserlogin.UserName + "'";
                int UserId = objentity.Database.SqlQuery<int>( SQL).FirstOrDefault<int>();
                // var UserId = objentity.Database.SqlQuery<string>(SQL);

                Session["UserId"] = UserId;

                return RedirectToAction("Index", "Home");

                               }
                             else
                             {
                                 ViewBag.Message = 0;
                             }
                             return View(_objuserlogin);
        }
    }
}