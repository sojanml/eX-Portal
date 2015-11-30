using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;
using System.Web.Mvc;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

    public partial class Util1
{

    static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();
    private static ExponentPortalEntities ctx;

    public static IEnumerable<SelectListItem> GetDropDowntList(string TypeField, string NameField, string ValueField, string SPName)
    {
        //  ctx=new ExponentPortalEntities();
        List<SelectListItem> SelectList = new List<SelectListItem>();
      
        using (var ctx = new ExponentPortalEntities())
        {
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {

                ctx.Database.Connection.Open();


                cmd.CommandText = "usp_Portal_DroneServiceType";
                DbParameter Param = cmd.CreateParameter();
                Param.ParameterName = "@Type";
                Param.Value = TypeField;
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


    /*Populating the drop down from Table Mst_Drone using sp usp_Portal_DroneNameList */
    public static IEnumerable<SelectListItem> DroneList(string SPName)
    {
        //  ctx=new ExponentPortalEntities();
        
        List<SelectListItem> SelectList = new List<SelectListItem>();
        using (var ctx = new ExponentPortalEntities())
        {
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {

                ctx.Database.Connection.Open();


                cmd.CommandText = "usp_Portal_DroneNameList";
               // DbParameter Param = cmd.CreateParameter();
               
              
                //cmd.Parameters.Add(Param);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        SelectList.Add(new SelectListItem { Text = reader["DroneName"].ToString(), Value = reader["DroneId"].ToString() });

                    }
                }
                DropDownList = SelectList.ToList();
                ctx.Database.Connection.Close();
                return DropDownList; //return the list objects

            }
        }
    }


    public static IEnumerable<SelectListItem> DronePartsList(string SPName)
    {
        //  ctx=new ExponentPortalEntities();
        List<SelectListItem> SelectList = new List<SelectListItem>();
        using (var ctx = new ExponentPortalEntities())
        {
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {

                ctx.Database.Connection.Open();


                cmd.CommandText = "usp_Portal_GetDroneParts";
                // DbParameter Param = cmd.CreateParameter();


                //cmd.Parameters.Add(Param);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string SupplierId = reader["SupplierId"].ToString();

                        
                        SelectList.Add(new SelectListItem { Text = reader["PartsName"].ToString()+"::" +reader["Model"].ToString()+ "::" + GetCompanyName(SupplierId) , Value = reader["PartsId"].ToString() });

                    }
                }
                DropDownList = SelectList.ToList();
                ctx.Database.Connection.Close();
                return DropDownList; //return the list objects

            }
        }
    }


   




    public static int GetServiceId()
    {
        int result = 0;
        using (var ctx = new ExponentPortalEntities())
        {
            String SQL = "select MAX(ServiceId)as ServiceId from MSTR_DroneService";
            int ServiceId = ctx.Database.SqlQuery<int>(SQL).FirstOrDefault<int>();
            result = ServiceId;
        }

        return result;
    }

    public static string GetCompanyName(string AccId)
    {
        string result;
        using (var ctx = new ExponentPortalEntities())
        {
            String SQL = "select name from MSTR_Account where  AccountId="+ AccId;
            string ServiceId = ctx.Database.SqlQuery<string>(SQL).FirstOrDefault<string>();
            result = ServiceId;
        }

        return result;
    }

}

