using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Data.Entity;

namespace eX_Portal.Controllers {

  public class UserController : Controller {
        // GET: UserLogin
        public ExponentPortalEntities db = new ExponentPortalEntities();

        public object EntityState { get; private set; }

        public ActionResult Index() {
      ViewBag.Title = "Login";
      UserLogin _objuserlogin = new UserLogin();
      return View(_objuserlogin);
    }//Login()


    [HttpPost]
    public ActionResult Index(UserLogin _objuserlogin) {
      ViewBag.Title = "Login";

      /*Create instance of entity model*/
      ExponentPortalEntities objentity = new ExponentPortalEntities();
      /*Getting data from database for user validation*/

      if (eX_Portal.exLogic.User.UserValidation(_objuserlogin.UserName, _objuserlogin.Password) > 0) {
        /*Redirect user to success apge after successfull login*/
        ViewBag.Message = 1;

        int UserId = eX_Portal.exLogic.User.GetUserId(_objuserlogin.UserName);
        string UserFirstName = eX_Portal.exLogic.User.GetFirstName(_objuserlogin.UserName);
        Session["FirstName"] = UserFirstName;
        Session["UserId"] = UserId;
        Session["UserName"] = _objuserlogin.UserName;
        return RedirectToAction("Index", "Home");

      } else {
        ViewBag.Message = 0;
      }
      return View(_objuserlogin);
    }//HttpPost Login()

        public ActionResult UserList()
        {
          
            ViewBag.Title = "User View";
            string SQL = "select a.UserName,a.FirstName,a.MobileNo,b.ProfileName, Count(*) Over() as _TotalRecords ,  a.UserId as _PKey " +
                " from MSTR_User a left join MSTR_Profile b on a.UserProfileId = b.ProfileId ";

           
            qView nView = new qView(SQL);
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



        }


        public ActionResult Create()

        {
            ViewBag.Title = "Create User";

            var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {
                User = new MSTR_User(),

                ProfileList = Util.GetProfileList(),
              
                //   DronePartsList=Util1.DronePartsList("usp_Portal_GetDroneParts")

            };
            return View(viewModel);
        }




        // GET: DroneService/Edit/5
        public ActionResult Edit(int id)
        {

            
            var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {


                
                User = db.MSTR_User.Find(id),

                ProfileList = Util.GetProfileList(),
            };
            return View(viewModel);
        }



        [HttpPost]
        public ActionResult Edit(MSTR_User User)
        {
            ViewBag.Title = "Edit Account";
            try
            {
                if (ModelState.IsValid)
                {


                  
                        if (Session["UserId"] == null)
                        {
                            Session["UserId"] = -1;
                        }
                        User.LastModifiedBy = Util.toInt(Session["UserID"].ToString());
                        User.LastModifiedOn = DateTime.Now;

                        string SQL = "UPDATE MSTR_USER SET UserName='" + User.UserName +
                             "',Password='" + User.Password + "',UserProfileId=" + User.UserProfileId + ",FirstName='" + User.FirstName +
                             "',UserProfileId=" + User.UserProfileId + ", Remarks='" +
                             User.Remarks + "',MobileNo='" + User.MobileNo + "',EmailId='" +
                             User.EmailId + "' where UserId=" + User.UserId;
                        int id = Util.doSQL(SQL);

                        return RedirectToAction("UserList");
                    }
               
            }
            catch (Exception ex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return View("InternalError", ex);
            }
            return View(User);
        }//ActionEdit()


        [HttpPost]
        public ActionResult Create(MSTR_User User)
        {
            if (ModelState.IsValid)
            {

                if (exLogic.User.UserExist(User.UserName)>0)
                {
                    ViewBag.UserExists = "User Exist Please Try Another !";

                    var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
                    {
                       User = new MSTR_User(),

                        ProfileList = Util.GetProfileList(),
                       
                        

                    };
                    return View(viewModel);
                   
                }
                else
                {

                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }
                    User.CreatedBy = Util.toInt(Session["UserID"].ToString());
                    User.CreatedOn = DateTime.Now;
                    db.MSTR_User.Add(User);
                    db.SaveChanges();
                    return RedirectToAction("UserList");
                }
            }
            return View(User);
        }




        public String Delete([Bind(Prefix = "ID")]int UserID = 0)
        {
            String SQL = "";
            Response.ContentType = "text/json";
           // if (!exLogic.User.hasAccess("USER.DELETE"))
             //   return Util.jsonStat("ERROR", "Access Denied");

            //Delete the drone from database if there is no flights are created
           // SQL = "SELECT Count(*) FROM [DroneFlight] WHERE DroneID = " + DroneID;
          //  if (Util.getDBInt(SQL) != 0)
           //     return Util.jsonStat("ERROR", "You can not delete a drone with a flight attached");

            SQL = "DELETE FROM [M2M_USER] WHERE UserId = " + UserID;
            Util.doSQL(SQL);           

            return Util.jsonStat("OK");
        }





    } //class
}//namespace

