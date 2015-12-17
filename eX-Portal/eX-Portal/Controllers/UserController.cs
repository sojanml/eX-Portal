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
            if (!exLogic.User.hasAccess("USER.VIEW")) return RedirectToAction("NoAccess", "Home");
            ViewBag.Title = "User View";
            string SQL = "select a.UserName,a.FirstName,a.MobileNo,b.ProfileName, Count(*) Over() as _TotalRecords ,  a.UserId as _PKey " +
                " from MSTR_User a left join MSTR_Profile b on a.UserProfileId = b.ProfileId  ";

           
            qView nView = new qView(SQL);
            if (exLogic.User.hasAccess("USER.VIEW")) nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
             if (exLogic.User.hasAccess("USER.DELETE")) nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
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
            if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");

            var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {
                User = new MSTR_User(),

                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList()

               

            };
            return View(viewModel);
        }




        // GET: DroneService/Edit/5
        public ActionResult Edit(int id)
        {

            if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
            var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {


                
                User = db.MSTR_User.Find(id),

                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList()
            };
            return View(viewModel);
        }



        [HttpPost]
        public ActionResult Edit(MSTR_User User)
        {         
            if (!exLogic.User.hasAccess("USER.EDIT")) return RedirectToAction("NoAccess", "Home");
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
                   
                    string Password = Util.GetEncryptedPassword(User.Password).ToString();
                    string SQL = "UPDATE MSTR_USER SET UserName='" + User.UserName +
                             "',Password='" + Password + "',UserProfileId=" + User.UserProfileId + ",FirstName='" + User.FirstName +
                             "', Remarks='" +
                             User.Remarks + "',MobileNo='" + User.MobileNo + "',EmailId='" +
                             User.EmailId + "', CountryId="+ User.CountryId + ",AccountId=" + User.AccountId +" where UserId=" + User.UserId;
                        int id = Util.doSQL(SQL);

                        return RedirectToAction("UserList");
                    }
               
            }
            catch (Exception ex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return View("InternalError", ex);
            }
            //if model not valid

            var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {



                User = db.MSTR_User.Find(User.UserId),

                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList()
            };
            return View(viewModel);
           
        }//ActionEdit()


        [HttpPost]
        public ActionResult Create(MSTR_User User)
        {
            if (!exLogic.User.hasAccess("USER.CREATE")) return RedirectToAction("NoAccess", "Home");
            if (ModelState.IsValid)
            {

                if (exLogic.User.UserExist(User.UserName)>0)
                {
                    ViewBag.UserExists = "User Exist Please Try Another !";

                    var viewModel = new ViewModel.UserViewModel.LoginViewModel.UserLogon
                    {
                       User = new MSTR_User(),
                        ProfileList = Util.GetProfileList(),
                        CountryList= Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                        AccountList=Util.GetAccountList()

                    };
                    return View(viewModel);
                   
                }
                else
                {

                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }
                               

                  
                    User.IsActive = true;
                    User.CreatedBy = Util.toInt(Session["UserID"].ToString());
                    User.CreatedOn = DateTime.Now;
                  
                    //  db.MSTR_User.Add(User);
                    //  db.SaveChanges();

                 
                    string Password = Util.GetEncryptedPassword(User.Password).ToString();

              String    SQL = "insert into MSTR_User(UserName, Password, FirstName, CreatedBy," +
                         "UserProfileId, Remarks, MobileNo, EmailId, CountryId, IsActive, CreatedOn,AccountId)" +
                         " values('" + User.UserName + "','" + Password + "','" + User.FirstName + "'," +
                         Session["UserId"] + "," + User.UserProfileId + ",'" + User.Remarks + "','" + User.MobileNo +
                         "','" + User.EmailId + "'," + User.CountryId + ",'" + User.IsActive + "','" + DateTime.Now.ToString("yyyy - MM - dd") +
                         "'," + User.AccountId + ")";

                    int id = Util.InsertSQL(SQL);

                    return RedirectToAction("UserList");
                }
            }

            var viewModelUser = new ViewModel.UserViewModel.LoginViewModel.UserLogon
            {
                User = new MSTR_User(),
                ProfileList = Util.GetProfileList(),
                CountryList = Util.GetCountryLists("Country", "CountryName", "Code", "sp"),
                AccountList = Util.GetAccountList()

            };
            return View(viewModelUser);

           
           
        }




        public String Delete([Bind(Prefix = "ID")]int UserID = 0)
        {
            if (!exLogic.User.hasAccess("USER.DELETE"))

              return Util.jsonStat("ERROR", "Access Denied");
            String SQL = "";
            Response.ContentType = "text/json";
           

            //Delete the drone from database if there is no user createdby
           SQL = "SELECT Count(*) FROM MSTR_User where CreatedBy = " + UserID;

          if (Util.getDBInt(SQL) != 0)
            return Util.jsonStat("ERROR", "You can not delete a the User Attached to another user");

            SQL = "select count(*) from Mstr_Drone where CreatedBy=" + UserID;
            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to Drone Creation");
            SQL = "select count(*) from Mstr_DroneService where CreatedBy=" + UserID;
            if (Util.getDBInt(SQL) != 0)
                return Util.jsonStat("ERROR", "You can not delete a the User Attached to DroneService Creation");



            SQL = "DELETE FROM [MSTR_USER] WHERE UserId = " + UserID;
            Util.doSQL(SQL);           

            return Util.jsonStat("OK");
        }





    } //class
}//namespace

