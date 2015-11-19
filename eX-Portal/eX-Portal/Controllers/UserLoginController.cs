using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
namespace eX_Portal.Controllers
{
    public class UserLoginController : Controller
    {
        // GET: UserLogin
        public ActionResult Index()
        {
            UserLogin _objuserlogin = new UserLogin();
            return View(_objuserlogin);
        }
        [HttpPost]
        public ActionResult Index(UserLogin _objuserlogin)
        {
            /*Create instance of entity model*/
          
            /*Getting data from database for user validation*/
            var _objuserdetail = (from data in objentity.UserDetails
                                  where data.UserId == _objuserloginmodel.UserId
                                  && data.Password == _objuserloginmodel.Password
                                  select data);
            if (_objuserdetail.Count() > 0)
            {
                /*Redirect user to success apge after successfull login*/
                ViewBag.Message = 1;
            }
            else
            {
                ViewBag.Message = 0;
            }
            return View(objuserloginmodel);
        }
    }
}