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
using static eX_Portal.ViewModel.UserDashboardModel;

namespace eX_Portal.exLogic {
  public partial class Util {
    static IEnumerable<SelectListItem> DropDownList = Enumerable.Empty<SelectListItem>();
    public static IEnumerable<SelectListItem> getListSQL(string SQL) {
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
      using (var ctx = new ExponentPortalEntities()) {
        using(var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using(var reader = cmd.ExecuteReader()) {
            while(reader.Read()) {
              SelectList.Add(new SelectListItem {
                Text = reader[0].ToString(),
                Value = reader[1].ToString()
              });
            }//while
          }//using reader
        }//using command
      }//using database
      return SelectList;
    }//function


    //static string connection = ConfigurationManager.ConnectionStrings["ExponentPortalSql"].ConnectionString;
    public static IEnumerable<SelectListItem> GetDropDowntList(string TypeField, string NameField, string ValueField, string SPName) {
      //  ctx=new ExponentPortalEntities();
      List<SelectListItem> SelectList = new List<SelectListItem>();
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value ="0" });
      using (var ctx = new ExponentPortalEntities()) {
        using(var cmd = ctx.Database.Connection.CreateCommand()) {

          ctx.Database.Connection.Open();


          cmd.CommandText = "usp_Portal_GetDroneDropDown";
          DbParameter Param = cmd.CreateParameter();
          Param.ParameterName = "@Type";
          Param.Value = TypeField;
          //  Param[0] = new DbParameter("@Type", TypeField);
          cmd.Parameters.Add(Param);
          cmd.CommandType = CommandType.StoredProcedure;
          using(var reader = cmd.ExecuteReader()) {
            while(reader.Read()) {
              SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });
            }
          }
          if(TypeField == "Camera")
                        SelectList.Add(new SelectListItem { Text = "Other", Value = "1" });

          DropDownList = SelectList.ToList();
          ctx.Database.Connection.Close();
          return DropDownList; //return the list objects

        }
      }
    }

    public static int InsertSQL(String SQL, string[] Parameter) {
      int result = 0;
      using(var ctx = new ExponentPortalEntities()) {
        using(var cmd = ctx.Database.Connection.CreateCommand()) {
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

    public static IList<LiveDrone> GetLiveDrones(string SQL) {

      List<LiveDrone> LiveDroneList = new List<LiveDrone>();


      using(var ctx = new ExponentPortalEntities()) {
        using(var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using(var reader = cmd.ExecuteReader()) {
            while(reader.Read()) {
              LiveDroneList.Add(new LiveDrone { DroneID = Convert.ToInt32(reader["DroneID"]), DroneHex = reader["DroneHex"].ToString(), LastLatitude = Convert.ToDouble(reader["LastLatitude"]), LastLongitude = Convert.ToDouble(reader["LastLongitude"]) });
            }
          }

          ctx.Database.Connection.Close();
        } //using Database.Connection
      }//using ExponentPortalEntities;
      return LiveDroneList; //return the list objects
    }//function GetDropDowntList

    public static SqlConnection getSonraiSQLServer() {
      string strConnection = ConfigurationManager.ConnectionStrings["SonraiConnectionString"].ConnectionString;
      SqlConnection con = new SqlConnection(strConnection);
      return con;
    }



    public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText) {
      if(connection == null)
        throw new ArgumentNullException("connection");

      // Create a command and prepare it for execution
      SqlCommand cmd = new SqlCommand();
      cmd.CommandTimeout = 90;
      bool mustCloseConnection = false;
      cmd.CommandText = commandText;
      cmd.CommandType = commandType;
      cmd.Connection = connection;
      // PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

      // Create the DataAdapter & DataSet
      using(SqlDataAdapter da = new SqlDataAdapter(cmd)) {
        DataSet ds = new DataSet();

        // Fill the DataSet using default values for DataTable names, etc
        da.Fill(ds);

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();

        if(mustCloseConnection)
          connection.Close();

        // Return the dataset
        return ds;
      }
    }

    public static IList<DroneData> GetDroneData() {

      IList<DroneData> ddList = new List<DroneData>();
      SqlConnection con = new SqlConnection();
      //  Connection conSonrai = new Connection();
      con = Util.getSonraiSQLServer();
      string query = "Select  TOP 30 DroneID,Latitude,[Longitude],[ReadTime],[Altitude],[Speed],[FixQuality],[Satellites],[Pitch] ,[Roll],[Heading],[TotalFlightTime],[FlightID],BBFlightID  FROM [sonrai001].[dbo].[WorkOrderTemp] order by  1 desc";

      DataSet ds = Util.ExecuteDataset(con, CommandType.Text, query);
      if(ds.Tables.Count > 0) {
        if(ds.Tables[0].Rows.Count > 0) {
          foreach(DataRow dr in ds.Tables[0].Rows) {
            DroneData dd = new DroneData();
            dd.Altitude = dr["Alt"].ToString();
            dd.Latitude = dr["Lat"].ToString();
            dd.Longitude = dr["Lon"].ToString();
            dd.Roll = dr["Roll"].ToString();
            dd.TotalFlightTime = dr["TotalFlightTime"].ToString();
            dd.DroneDataId = Convert.ToInt32(dr["SNO"].ToString());
            //if (IsDate(dr["Timstamp"].ToString()))
            //  dd.ReadTime = Convert.ToDateTime(dr["Timstamp"].ToString());                        
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
    public static IList<FlightMapData> GetDroneData(int FlID, int LastFlightID, int MaxRecords = 1, int Replay = 0) {

      IList<FlightMapData> FlightMapDataList;
      using(ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        FlightMapDataList = (from FlightMapData in ctx.FlightMapDatas
                             where FlightMapData.FlightID == FlID &&
                                   FlightMapData.FlightMapDataID > LastFlightID &&
                                   ((Replay == 1 && FlightMapData.Speed > 0) || Replay == 0)
                             select FlightMapData).OrderBy(x => x.FlightMapDataID).Take(MaxRecords).ToList();

      }


      return FlightMapDataList; //return the list objects
    }//function GetDropDowntList

    public static IList<FlightMapData> GetFlightChartData(int FlID, int LastFlightID = 0, int MaxRecords = 0) {
      int MaxDataCount = 25;      
      int DataCount = 0;
      int mode = 0;
      IList<FlightMapData> FlightMapDataList = new List<FlightMapData>();

      using(ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        var AllFlightData = (
          from FlightMapData in ctx.FlightMapDatas
          where FlightMapData.FlightID == FlID
          select FlightMapData
          ).OrderBy(x => x.FlightMapDataID)
           .ToList();
        DataCount = AllFlightData.Count;

        if(DataCount < MaxDataCount) {
          FlightMapDataList = AllFlightData;
        } else {
          mode = DataCount / MaxDataCount;
          FlightMapDataList = AllFlightData.Where((x, i) => i % mode == 0).ToList();
        }
      }
      return FlightMapDataList;
      //return the list objects
    }


    public static bool IsDate(string Timestamp) {
      DateTime tempDate;
      return DateTime.TryParse(Timestamp, out tempDate) ? true : false;
    }
    public static IList<PayLoadMapData> GetPayLoadData(string FlID, int LastFlightID, int MaxRecords = 1) {

      IList<PayLoadMapData> PayLoadMapDataList;
      using(ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        PayLoadMapDataList = (from PayLoadMapData in ctx.PayLoadMapDatas
                              where PayLoadMapData.FlightUniqueID == FlID &&
                                   PayLoadMapData.PayLoadDataMapID > LastFlightID
                              select PayLoadMapData).OrderBy(x => x.RFID).Take(MaxRecords).ToList();

      }


      return PayLoadMapDataList; //return the list objects
    }//function GetDropDowntList

    public static bool CheckSessionValid(int UserId)
    {
            string updateSQL= @"update UserLog set IsSessionEnd=1,SessionEndTime=GETDATE()
                                where DATEDIFF(HOUR, SessionStartTime, GETDATE())> 23";
            Util.doSQL(updateSQL);

            string sql = "select count(*) from UserLog where UserID= " + UserId + " and IsSessionEnd=0";
            int count=Util.getDBInt(sql);
            if (count < 100)
                return true;
            else
                return false;
    }

        public UserDashboardModel GetUserDetails(int UserID)
        {
            Models.MSTR_User User = db.MSTR_User.Find(UserID);
            UserDashboardModel UserDashboard = new UserDashboardModel();
            UserDashboardModel.PilotDetail PDetail = new UserDashboardModel.PilotDetail();
            List<UserDashboardModel.RPASDetail> RPASDet = new List<UserDashboardModel.RPASDetail>();
            List<GCA_Approval> AppList = new List<GCA_Approval>();
            string PilotSQL = $@"SELECT a.[UserName]
                          ,a.[FirstName] 
                         ,a.[MiddleName]  
                         ,a.[LastName]   
                         ,a.[MobileNo]   
                         ,a.[OfficeNo]  
                         ,a.[HomeNo] 
                          ,a.[EmailId]  as [Email ID] 
                         ,b.[PassportNo] 
                          ,CONVERT(NVARCHAR, b.[DateOfExpiry], 106) AS DateOfExpiry  
                         ,b.[Department]
                          ,b.[EmiratesId]  as [Emirates ID]
                       ,b.[Title] as JobTitle
                         ,a.[RPASPermitNo] as [RPAS Permit No.] 
                         ,a.[PermitCategory] as [Permit Category] 
                         ,ISNULL(a.PhotoURL, '') as PilotImage,
                           c.Name as CompanyName,
                           a.Nationality,
                           a.DOI_RPASPermit,
                           a.DOE_RPASPermit
                          FROM [MSTR_User] a
                          left join mstr_user_pilot b
                         on a.UserId=b.UserId
                         left join MSTR_Account c  
                         on a.AccountId=c.AccountId  
                         left join MSTR_Profile d 
                         on a.UserProfileId=d.ProfileId
                         where a.userid={ UserID}";
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = PilotSQL;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PDetail.UserName = reader.GetValue(0).ToString();
                        PDetail.FirstName = reader.GetValue(1).ToString();
                        PDetail.MiddleName = reader.GetValue(2).ToString();
                        PDetail.LastName = reader.GetValue(3).ToString();
                        PDetail.MobileNo = reader.GetValue(4).ToString();

                        PDetail.OfficeNo = reader.GetValue(5).ToString();
                        PDetail.HomeNo = reader.GetValue(6).ToString();
                        PDetail.EmailId = reader.GetValue(7).ToString();
                        PDetail.PassportNo = reader.GetValue(8).ToString();
                        PDetail.DateOfExpiry = Util.toDate(reader.GetValue(8).ToString());
                        PDetail.EmiratesID = reader.GetValue(9).ToString();
                        PDetail.Title = reader.GetValue(10).ToString();
                        PDetail.RPASPermitNo = reader.GetValue(13).ToString();
                        PDetail.PermitCategory = reader.GetValue(14).ToString();
                        PDetail.PilotImage = reader.GetValue(15).ToString();
                       
                        PDetail.CompanyName= reader.GetValue(16).ToString();
                        PDetail.Nationality = reader.GetValue(17).ToString();
                        PDetail.DOI_RPASPermit =reader.GetDateTime(18);
                        PDetail.DOE_RPASPermit  = reader.GetDateTime(19);
                        PDetail.UserId = UserID;
                    }

                }
            }
            if (String.IsNullOrEmpty(PDetail.PilotImage))
            {
                PDetail.PilotImage = "/images/PilotImage.png";
            }
            else
            {
                PDetail.PilotImage = $"/Upload/User/{UserID}/{PDetail.PilotImage}";
                if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(PDetail.PilotImage)))
                    PDetail.PilotImage = "/images/PilotImage.png";
            }

            string ApprovalSQL = $@"Select
                ApprovalID,
                ApprovalName,
                StartDate,
                EndDate,
                StartTime,
                EndTime,
                MaxAltitude,
                MinAltitude,        
                case IsUseCamara when 1 then 'Yes' else 'No' end as Camara,       
                ApprovalStatus as Status,
                Count(*) Over() as _TotalRecords,
                ApprovalID as _PKey
              FROM
                GCA_Approval
              LEFT JOIN MSTR_User ON
                MSTR_User.UserID = GCA_Approval.CreatedBy
               LEFT JOIN MSTR_Drone  on GCA_Approval.DroneId= MSTR_Drone.DroneId  
                where GCA_Approval.[PilotUserId]={UserID}
             Order By
                StartDate DESC
";
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = ApprovalSQL;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GCA_Approval GCADet = new GCA_Approval();
                        GCADet.ApprovalID = Util.toInt(reader[0]);
                        GCADet.ApprovalName = reader[1].ToString();
                        GCADet.ApprovalStatus = reader[9].ToString();
            GCADet.StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
            GCADet.EndDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
            GCADet.StartTime = reader["StartTime"].ToString();
            GCADet.EndTime = reader["EndTime"].ToString();
            AppList.Add(GCADet);
                    }
                }
            }

            string RPASSql = $@"SELECT 
                          D.[DroneName] as RPAS,
                          D.[ModelName] as Description,
                          D.[CommissionDate],
                          O.Name as Authority,
                          M.Name as Manufacture,
                          U.GroupName as RPASType,
                          Count(*) Over() as _TotalRecords,
                          D.[DroneId] as _PKey
                        FROM
                          [MSTR_Drone] D
                        Left join MSTR_Account  O on
                          D.AccountID = O.AccountID Left join LUP_Drone M on
                          ManufactureID = M.TypeID and
                          M.Type='Manufacturer' Left join LUP_Drone U on
                          UAVTypeID = U.TypeID and
                          U.Type= 'UAVType'
                        WHERE
                          D.AccountID={User.AccountId}";
            
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = RPASSql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RPASDetail RPAS = new RPASDetail();
                        RPAS.DroneName = reader.GetValue(0).ToString();
                        RPAS.CommissionDate = Util.toDate(reader.GetValue(2).ToString());
                        RPAS.DroneId = Util.toInt( reader.GetValue(7));
                        RPAS.ManufactureName = reader.GetValue(4).ToString();
                        RPAS.UavTypeName = reader.GetValue(5).ToString();
                        RPASDet.Add(RPAS);
                    }
                }
            }
            UserDashboard.Pilot = PDetail;
            UserDashboard.ApprovalList = AppList;
            UserDashboard.RPASList = RPASDet;
            return UserDashboard;
        }
       
        public string getOuterPolygon(String coordinates,int BoundaryInMeters)
        {
            string innerpoly = "";
            string sql = $@"select 
                        case 
                        when geography::STGeomFromText('POLYGON((' + dbo.ToogleGeo('{coordinates}') + '))', 4326).STArea() > 999999999999
                        then
                        dbo.ToogleGeo(geography::STGeomFromText('POLYGON((' + dbo.ToogleGeo('{coordinates}') + '))', 4326).ReorientObject().MakeValid().STBuffer({BoundaryInMeters}).ToString())
                        else
                        dbo.ToogleGeo(geography::STGeomFromText('POLYGON((' + dbo.ToogleGeo('{coordinates}') + '))', 4326).STBuffer({BoundaryInMeters}).ToString())
                        end";
            innerpoly = Util.getDBVal(sql);

            return innerpoly;
        }
        //return Json(PDetail, JsonRequestBehavior.AllowGet); 
  }
}