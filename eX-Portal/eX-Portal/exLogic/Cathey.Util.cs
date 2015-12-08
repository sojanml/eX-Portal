using eX_Portal.Models;
using eX_Portal.ViewModel;
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

        public static  IList<LiveDrone> GetLiveDrones(string SQL)
        {

            List<LiveDrone> LiveDroneList = new List<LiveDrone>();

            
            using (var ctx = new ExponentPortalEntities())
            {
                using (var cmd = ctx.Database.Connection.CreateCommand())
                {
                    ctx.Database.Connection.Open();
                    cmd.CommandText = SQL;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LiveDroneList.Add(new LiveDrone{DroneID=Convert.ToInt32(reader["DroneID"]),DroneHex=reader["DroneHex"].ToString(),LastLatitude=Convert.ToDouble(reader["LastLatitude"]),LastLongitude= Convert.ToDouble(reader["LastLongitude"])});
                        }
                    }

                    ctx.Database.Connection.Close();
                } //using Database.Connection
            }//using ExponentPortalEntities;
            return LiveDroneList; //return the list objects
        }//function GetDropDowntList

        public static SqlConnection getSonraiSQLServer()
        {
            string strConnection = ConfigurationManager.ConnectionStrings["SonraiConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(strConnection);
            return con;
        }

        

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 90;
            bool mustCloseConnection = false;
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            cmd.Connection = connection;
           // PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        public static IList<DroneData> GetDroneData()
        {
            IList<DroneData> ddList = new List<DroneData>();
            SqlConnection con = new SqlConnection();
          //  Connection conSonrai = new Connection();
            con =Util.getSonraiSQLServer();
            string query = "Select  TOP 30 WorkorderTempID as SNO,[Lat],[Lon] ,[Alt]  ,[Speed]  ,[FixQuality]  ,[Satellites] ,[Timstamp] ,[Pitch] ,[Roll] ,[Heading]  ,[TotalFlightTime]  FROM [sonrai001].[dbo].[WorkOrderTemp] order by  1 desc";

            DataSet ds = Util.ExecuteDataset(con, CommandType.Text, query);
            if(ds.Tables.Count>0)
            { 
                if(ds.Tables[0].Rows.Count>0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        DroneData dd = new DroneData();
                        dd.Altitude = dr["Alt"].ToString();
                        dd.Latitude = dr["Lat"].ToString();
                        dd.Longitude = dr["Lon"].ToString();
                        dd.Roll = dr["Roll"].ToString();
                        dd.TotalFlightTime = dr["TotalFlightTime"].ToString();
                        dd.DroneDataId = Convert.ToInt32(dr["SNO"].ToString());
                        if(IsDate(dr["Timstamp"].ToString()))
                        dd.ReadTime = Convert.ToDateTime(dr["Timstamp"].ToString());                        
                        dd.Satellites = dr["Satellites"].ToString();
                        dd.Speed = dr["Speed"].ToString();
                        dd.FixQuality = dr["FixQuality"].ToString();
                        dd.Pitch = dr["Pitch"].ToString();
                        dd.Heading = dr["Heading"].ToString();
                        ddList.Add(dd);
                    }
                }
                
            }
            return ddList;

        }

        public static  bool IsDate(string Timestamp)
        {
            DateTime tempDate;
            return  DateTime.TryParse(Timestamp, out tempDate) ? true : false;
        }
    }
}