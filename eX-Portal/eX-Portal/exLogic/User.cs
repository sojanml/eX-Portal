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
      return true;
      HttpSessionState Session = HttpContext.Current.Session;
      int UserID = 0;
      if(Session["UserID"] != null) Int32.TryParse(Session["UserID"].ToString(), out UserID);
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
      String PermissionCount = Util.getDBVal(SQL);

      return (PermissionCount == "1");
    }

    public static int UserValidation(String UserName, String Password) {
      try {
        int result = 0;
        using (var ctx = new ExponentPortalEntities()) {
          var _objuserdetail = (from data in ctx.MSTR_User
                                where data.UserName == UserName
                                && data.Password == Password

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


    public static IList<MenuModel> BuildMenu() {

      IList<MenuModel> mmList = new List<MenuModel>();
      try {

        var menu_items = db.MSTR_Menu;
        var UserId = System.Web.HttpContext.Current.Session["UserId"] == null ? 0 : System.Web.HttpContext.Current.Session["UserId"];


        String SQL = "select * from MSTR_Menu where Visible=1 and  MenuId" +
            " in(select b.MenuId from  MSTR_Profile a left join   M2M_ProfileMenu b" +
             "  on a.ProfileId = b.ProfileId left join MSTR_User c on b.ProfileId = c.UserProfileId" +
             "   where c.UserId =" + UserId + ")order by SortOrder asc ";
        var MenuName = db.Database.SqlQuery<MSTR_Menu>(SQL);


        foreach (MSTR_Menu mnu in MenuName) {

          MenuModel model = new MenuModel();

          model.Id = mnu.MenuId;
          model.Name = mnu.MenuName;
          model.ParentId = mnu.ParentId.GetValueOrDefault();
          model.SortOrder = mnu.SortOrder.GetValueOrDefault();
          model.PageUrl = mnu.PageUrl;

          mmList.Add(model);
        }

      } catch (Exception ex) {
        Util.ErrorHandler(ex);
      }
      return mmList;
    }
  }//class
}//namespace