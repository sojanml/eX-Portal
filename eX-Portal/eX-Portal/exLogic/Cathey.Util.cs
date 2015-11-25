using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.exLogic
{
    public partial class Util
    {
        static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();
        private static ExponentPortalEntities ctx;
        //static string connection = ConfigurationManager.ConnectionStrings["ExponentPortalSql"].ConnectionString;
        public static IEnumerable<SelectListItem> GetDropDowntList(string TypeField, string NameField, string ValueField, string SPName)
        {
          //  ctx=new ExponentPortalEntities();
            List<SelectListItem> SelectList = new List<SelectListItem>();
            using (var ctx = new ExponentPortalEntities())
            { 
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {

                ctx.Database.Connection.Open();


                cmd.CommandText = "usp_Portal_GetDroneDropDown";
                DbParameter Param = cmd.CreateParameter();
                Param.ParameterName = "@Type";
                Param.Value = TypeField;
                //  Param[0] = new DbParameter("@Type", TypeField);
                cmd.Parameters.Add(Param);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });

                    }
                }
                DropDownList = SelectList.ToList();
                ctx.Database.Connection.Close();
                return DropDownList; //return the list objects

            }
            }
        }
        public static int InsertSQL(String SQL,string[] Parameter)
        {
            int result = 0;
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();
                    cmd.CommandText = SQL;
                    cmd.Parameters.Add(Parameter.ToList());
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT scope_identity()";
                    result = Int32.Parse(cmd.ExecuteScalar().ToString());

                }
            }
            return result;
        }
    }
}