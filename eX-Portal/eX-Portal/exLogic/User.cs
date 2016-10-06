using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using System.Web.Mvc;
using System.Web.SessionState;

namespace eX_Portal.exLogic {
  public class User {
    public static ExponentPortalEntities db = new ExponentPortalEntities();

    public static bool hasAccess(String PermissionID) {
      HttpSessionState Session = HttpContext.Current.Session;
      int UserID = 0;
      if (Session["UserID"] != null) Int32.TryParse(Session["UserID"].ToString(), out UserID);
      String SQL =
      "select\n" +
      "  Count(MSTR_Menu.MenuID) as PermissionCount\n" +
      "FROM\n" +
      "  MSTR_User,\n" +
      "  MSTR_Profile,\n" +
      "  M2M_ProfileMenu,\n" +
      "  MSTR_Menu\n" +
      "WHERE\n" +
      "  MSTR_User.UserID = " + UserID + " AND\n" +
      "  MSTR_User.UserProfileId = MSTR_Profile.ProfileId AND\n" +
      "  M2M_ProfileMenu.ProfileID = MSTR_Profile.ProfileId AND\n" +
      "  MSTR_Menu.MenuId = M2M_ProfileMenu.MenuID AND\n" +
      "  MSTR_Menu.PermissionId = '" + PermissionID + "'";
      int PermissionCount = Util.getDBInt(SQL);

      return (PermissionCount > 0);
    }

    public static bool hasDrone(int DroneID) {
      if (exLogic.User.hasAccess("DRONE.MANAGE")) return true;

      String SQL = "SELECT Count(*) FROM\n" +
        "MSTR_Drone\n" +
        "WHERE\n" +
        "  DroneID=" + DroneID + " AND\n" +
        "  AccountID=" + Util.getAccountID();
      int Count = Util.getDBInt(SQL);
      if (Count > 0) return true;
      return false;

    }

    public static int UserValidation(String UserName, String Password) {
           
            string PasswordCrypto = Util.GetEncryptedPassword(Password);
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        var _objuserdetail = (from data in ctx.MSTR_User
                              where ((data.UserName.Equals(UserName)
                              && (data.Password.Equals(PasswordCrypto)))
                              || (data.GeneratedPassword.Equals(PasswordCrypto)))
                              select data).ToList();
                if (_objuserdetail.Count > 0)
                {
                    if (_objuserdetail[0].GeneratedPassword == PasswordCrypto)
                    {
                        string updatesql = "update MSTR_User set Password='" + PasswordCrypto + "',GeneratedPassword='' where UserId=" + _objuserdetail[0].UserId;
                        Util.doSQL(updatesql);
                    }
                    else { }
                }
                result = _objuserdetail.Count;
      }
      return result;


    }

    public static int UserIsActive(String UserName, String Password)
    {
        try
        {
            string PasswordCrypto = Util.GetEncryptedPassword(Password);
            int result = 0;
                
            using (var ctx = new ExponentPortalEntities())
            {
                var _objuserdetail = (from data in ctx.MSTR_User
                                      where (data.UserName == UserName
                                      && data.Password == PasswordCrypto)
                                      select data.IsActive).ToList();
                    if (_objuserdetail.Count > 0)
                    {
                        string isactive = _objuserdetail[0].ToString();
                        if (isactive == "True")
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                    else
                    {
                        result = 0;
                    }
            }
            return result;

        }
        catch (Exception ex)
        {
            Util.ErrorHandler(ex);
            System.Web.HttpContext.Current.Response.Write("<script>alert('Please Check Database Connection');</script>");
            return -1;
        }
   }


        public static int UserExist(String UserName) {
      try {

        int result = 0;
        using (var ctx = new ExponentPortalEntities()) {
          var _objuserdetail = (from data in ctx.MSTR_User
                                where data.UserName == UserName


                                select data);

          result = _objuserdetail.Count();
        }

        return result;

      } catch (Exception ex) {
        Util.ErrorHandler(ex);
        System.Web.HttpContext.Current.Response.Write("<script>alert('Please Check Database Connection');</script>");
        return -1;
      }
    }

        public static int EmailExist(String EmailID)
        {
            try
            {

                int result = 0;
                using (var ctx = new ExponentPortalEntities())
                {
                    var _objuserdetail = (from data in ctx.MSTR_User
                                          where data.EmailId == EmailID


                                          select data);

                    result = _objuserdetail.Count();
                }

                return result;

            }
            catch (Exception ex)
            {
                Util.ErrorHandler(ex);
                System.Web.HttpContext.Current.Response.Write("<script>alert('Please Check Database Connection');</script>");
                return -1;
            }
        }

        public static int GetUserId(String UserName) {
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        String SQL = "select UserId from MSTR_User" +
        " where UserName='" + UserName + "'";
        int UserId = ctx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
        result = UserId;
      }

      return result;
    }


    public static string GetFirstName(String UserName) {
      string result;
      using (var ctx = new ExponentPortalEntities()) {
        String SQL = "select FirstName from MSTR_User" +
        " where UserName='" + UserName + "'";
        string FirstName = ctx.Database.SqlQuery<string>(SQL).FirstOrDefault<string>();
        result = FirstName;
      }

      return result;
    }

    public static UserInfo getInfo(String UserName) {
      UserInfo thisUser = new UserInfo();
      String SQL = "select\n" +
      "  MSTR_User.UserID," +
      "  FirstName + ISNull(' ' + MiddleName, '') + ISNull(' ' +LastName, '') as FullName,\n" +
      "  MSTR_Account.AccountID,\n" +
      "  MSTR_Account.BrandColor,\n" +
      "  MSTR_Account.BrandLogo\n" +
      "from\n" +
      "  MSTR_User\n" +
      "left Join MSTR_Account On\n" +
      "  MSTR_Account.AccountId = MSTR_User.AccountId\n" +
      "where\n" +
      "  MSTR_User.UserName = '" + UserName + "'\n";
      var Result = Util.getDBRow(SQL);
      thisUser.UserID = int.Parse(Result["UserID"].ToString());
      thisUser.AccountID =string.IsNullOrEmpty(Convert.ToString(Result["AccountID"]))? 0 : int.Parse(Convert.ToString(Result["AccountID"]));
      thisUser.FullName = Result["FullName"].ToString();
      thisUser.BrandColor = Result["BrandColor"].ToString();
      thisUser.BrandLogo = Result["BrandLogo"].ToString();
      thisUser.UserName = UserName;
      if (thisUser.BrandColor == "") thisUser.BrandColor = "#ea050e";
      if (thisUser.BrandLogo == "") thisUser.BrandLogo = "exponent-logo.png";
      return thisUser;
    }


    public static IList<MenuModel> BuildMenu() {

      IList<MenuModel> mmList = new List<MenuModel>();
      try {

        var menu_items = db.MSTR_Menu;
        var UserId = System.Web.HttpContext.Current.Session["UserId"] == null ? 0 : System.Web.HttpContext.Current.Session["UserId"];


        String SQL = "select * from MSTR_Menu where Visible=1 and  MenuId" +
            " in(select b.MenuId from  MSTR_Profile a left join   M2M_ProfileMenu b" +
             "  on a.ProfileId = b.ProfileId left join MSTR_User c on b.ProfileId = c.UserProfileId" +
             "   where c.UserId =" + UserId + ") order by SortOrder asc ";
        var MenuName = db.Database.SqlQuery<MSTR_Menu>(SQL);


        foreach (MSTR_Menu mnu in MenuName) {
          MenuModel model = new MenuModel();
          model.Id = mnu.MenuId;
          model.Name = mnu.MenuName;
          model.ParentId = mnu.ParentId.GetValueOrDefault();
          model.SortOrder = mnu.SortOrder.GetValueOrDefault();
          model.PageUrl = mnu.PageUrl;

          mmList.Add(model);

          if(mnu.MenuName == "Home") {
            AddCmsMenuTo(mmList, mnu.MenuId);
          }
        }

      } catch (Exception ex) {
        Util.ErrorHandler(ex);
      }
      return mmList;
    }

    private static void AddCmsMenuTo(IList<MenuModel> mmList, int ParentID) {
      using(var ctx = new ExponentPortalEntities()) {
        var CmsItems = from cms in ctx.ContentManagements
                       where cms.IsShowInMenu == 1
                       orderby cms.PageTitle
                       select new MenuModel {
                         Id = cms.CmsID,
                         Name = cms.MenuTitle,
                         ParentId = ParentID,
                         SortOrder = cms.CmsID,
                         PageUrl = "/home/demo/" + cms.CmsRefName
                       };
        foreach(var menu in CmsItems.ToList()) {
          mmList.Add(menu);
        }
      }
    }

    }//class

  public class UserInfo {
    public int UserID { get; set; }
    public String FullName { get; set; }
    public String BrandLogo { get; set; }
    public String BrandColor { get; set; }
    public String UserName { get; set; }
    public int AccountID { get; set; }
  }
}//namespace