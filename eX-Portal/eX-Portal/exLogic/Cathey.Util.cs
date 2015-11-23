using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.exLogic
{
    public partial class Util
    {
        static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();

        static string connection = ConfigurationManager.ConnectionStrings["ExponentPortalSql"].ConnectionString;
        public static IEnumerable<SelectListItem> GetDropDowntList(string TypeField, string NameField, string ValueField, string SPName)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();
                SqlDataReader myReader = null;

                SqlCommand myCommand = new SqlCommand("usp_Portal_GetDroneDropDown", conn);
                SqlParameter[] Param = new SqlParameter[1];
                Param[0] = new SqlParameter("@Type", TypeField);
                myCommand.Parameters.Add(Param[0]);

                myCommand.CommandType = CommandType.StoredProcedure;
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    //DropDownList.Add(new SelectListItem { Text = myReader["Tournament"].ToString(), Value = myReader["Id"].ToString() });
                    //DropDownList.
                    //tournament.Add(new SelectListItem { Text = myReader["Tournament"].ToString(), Value = myReader["Id"].ToString() });
                }

                conn.Close();
                return DropDownList; //return the list objects

            }

        }
    }
}