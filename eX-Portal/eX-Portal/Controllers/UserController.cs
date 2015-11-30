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
    public class UserController :Controller
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

            
                         if (eX_Portal.exLogic.User.UserValidation(_objuserlogin.UserName , _objuserlogin.Password )> 0)
                             {
                /*Redirect user to success apge after successfull login*/
                                  ViewBag.Message = 1;

               
                int UserId = eX_Portal.exLogic.User.GetUserId(_objuserlogin.UserName);
                string UserFirstName = eX_Portal.exLogic.User.GetFirstName(_objuserlogin.UserName);
                Session["FirstName"] = UserFirstName;
                Session["UserId"] = UserId;
                Session["UserName"] = _objuserlogin.UserName;
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