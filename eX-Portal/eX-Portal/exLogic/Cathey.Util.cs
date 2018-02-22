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
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
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
      SelectList.Add(new SelectListItem { Text = "Please Select...", Value = "0" });
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {

          ctx.Database.Connection.Open();


          cmd.CommandText = "usp_Portal_GetDroneDropDown";
          DbParameter Param = cmd.CreateParameter();
          Param.ParameterName = "@Type";
          Param.Value = TypeField;
          //  Param[0] = new DbParameter("@Type", TypeField);
          cmd.Parameters.Add(Param);
          cmd.CommandType = CommandType.StoredProcedure;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
              SelectList.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["Code"].ToString() });
            }
          }
          if (TypeField == "Camera")
            SelectList.Add(new SelectListItem { Text = "Other", Value = "1" });

          DropDownList = SelectList.ToList();
          ctx.Database.Connection.Close();
          return DropDownList; //return the list objects

        }
      }
    }

    public static int InsertSQL(String SQL, string[] Parameter) {
      int result = 0;
      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
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


      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd = ctx.Database.Connection.CreateCommand()) {
          ctx.Database.Connection.Open();
          cmd.CommandText = SQL;
          using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
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
      if (connection == null)
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
      using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
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

    public static IList<DroneData> GetDroneData() {

      IList<DroneData> ddList = new List<DroneData>();
      SqlConnection con = new SqlConnection();
      //  Connection conSonrai = new Connection();
      con = Util.getSonraiSQLServer();
      string query = "Select  TOP 30 DroneID,Latitude,[Longitude],[ReadTime],[Altitude],[Speed],[FixQuality],[Satellites],[Pitch] ,[Roll],[Heading],[TotalFlightTime],[FlightID],BBFlightID  FROM [sonrai001].[dbo].[WorkOrderTemp] order by  1 desc";

      DataSet ds = Util.ExecuteDataset(con, CommandType.Text, query);
      if (ds.Tables.Count > 0) {
        if (ds.Tables[0].Rows.Count > 0) {
          foreach (DataRow dr in ds.Tables[0].Rows) {
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
      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
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

      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        var AllFlightData = (
          from FlightMapData in ctx.FlightMapDatas
          where FlightMapData.FlightID == FlID
          select FlightMapData
          ).OrderBy(x => x.FlightMapDataID)
           .ToList();
        DataCount = AllFlightData.Count;

        if (DataCount < MaxDataCount) {
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
      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        PayLoadMapDataList = (from PayLoadMapData in ctx.PayLoadMapDatas
                              where PayLoadMapData.FlightUniqueID == FlID &&
                                   PayLoadMapData.PayLoadDataMapID > LastFlightID
                              select PayLoadMapData).OrderBy(x => x.RFID).Take(MaxRecords).ToList();

      }


      return PayLoadMapDataList; //return the list objects
    }//function GetDropDowntList

    public static bool CheckSessionValid(int UserId) {
      string updateSQL = @"update UserLog set IsSessionEnd=1,SessionEndTime=GETDATE()
                                where DATEDIFF(HOUR, SessionStartTime, GETDATE())> 23";
      Util.doSQL(updateSQL);

      string sql = "select count(*) from UserLog where UserID= " + UserId + " and IsSessionEnd=0";
      int count = Util.getDBInt(sql);
      if (count < 100)
        return true;
      else
        return false;
    }

    public UserDashboardModel GetUserDetails(int UserID, bool IsOrganisationAdmin = false) {
      Models.MSTR_User User = db.MSTR_User.Find(UserID);
      UserDashboardModel UserDashboard = new UserDashboardModel();
      UserDashboardModel.PilotDetail PDetail = new UserDashboardModel.PilotDetail();
      List<UserDashboardModel.RPASDetail> RPASDet = new List<UserDashboardModel.RPASDetail>();
      List<NOC_Details> AppList = new List<NOC_Details>();
      string PilotSQL = 
        $@"SELECT 
          a.[UserName]
          ,a.[FirstName] 
          ,a.[MiddleName]  
          ,a.[LastName]   
          ,a.[MobileNo]   
          ,a.[OfficeNo]  
          ,a.[HomeNo] 
          ,a.[EmailId]  
          ,b.[PassportNo] 
          ,CONVERT(NVARCHAR, b.[DateOfExpiry], 106) AS DateOfExpiry  
          ,b.[Department]
          ,b.[EmiratesId] 
          ,b.[Title] as JobTitle
          ,a.[RPASPermitNo] 
          ,a.[PermitCategory] 
          ,ISNULL(a.PhotoURL, '') as PilotImage,
          c.Name as CompanyName,
          a.Nationality,
          a.DOI_RPASPermit,
          a.DOE_RPASPermit,
          a.AccountId
        FROM 
          [MSTR_User] a
        left join mstr_user_pilot b
          on a.UserId=b.UserId
        left join MSTR_Account c  
          on a.AccountId=c.AccountId  
        left join MSTR_Profile d 
          on a.UserProfileId=d.ProfileId
        where 
          a.userid={ UserID}";
      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = PilotSQL;
        using (var reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            PDetail.UserName = RS(reader, "UserName");
            PDetail.FirstName = RS(reader, "FirstName");
            PDetail.MiddleName = RS(reader, "MiddleName");
            PDetail.LastName = RS(reader, "LastName");
            PDetail.MobileNo = RS(reader, "MobileNo");

            PDetail.OfficeNo = RS(reader, "OfficeNo");
            PDetail.HomeNo = RS(reader, "HomeNo");
            PDetail.EmailId = RS(reader, "EmailId");
            PDetail.PassportNo = RS(reader, "PassportNo");
            PDetail.DateOfExpiry = Util.toDate(RS(reader, "DateOfExpiry"));
            PDetail.EmiratesID = RS(reader, "EmiratesID");
            PDetail.Title = RS(reader, "JobTitle");
            PDetail.RPASPermitNo = RS(reader, "RPASPermitNo");
            PDetail.PermitCategory = RS(reader, "PermitCategory");
            PDetail.PilotImage = RS(reader, "PilotImage");

            PDetail.CompanyName = RS(reader, "CompanyName");
            PDetail.Nationality = RS(reader, "Nationality");

            PDetail.DOI_RPASPermit = Util.toDate(RS(reader, "DOI_RPASPermit"));
            PDetail.DOE_RPASPermit = Util.toDate(RS(reader, "DOE_RPASPermit"));

            PDetail.AccountId = Util.toInt(RS(reader, "AccountId"));

            PDetail.UserId = UserID;
            
          }

        }
      }
      if (String.IsNullOrEmpty(PDetail.PilotImage)) {
        PDetail.PilotImage = "/images/PilotImage.png";
      } else {
        PDetail.PilotImage = $"/Upload/User/{UserID}/{PDetail.PilotImage}";
        if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(PDetail.PilotImage)))
          PDetail.PilotImage = "/images/PilotImage.png";
      }

      string ApprovalSQL = $@"Select
        NOCID,
        NOCName,
        NOC_Details.StartDate,
        NOC_Details.EndDate,
        StartTime,
        EndTime,
        MaxAltitude,
        MinAltitude,        
        case IsUseCamara when 1 then 'Yes' else 'No' end as Camara,       
        Status,
        Count(*) Over() as _TotalRecords,
        NOCID as _PKey
      FROM
        NOC_Details
		join MSTR_NOC on NOC_Details.NOCApplicationID=MSTR_NOC.NOCApplicationID
      LEFT JOIN MSTR_User ON
        MSTR_User.UserID = NOC_Details.PilotID
      LEFT JOIN MSTR_Drone  on 
        NOC_Details.DroneId= MSTR_Drone.DroneId";
      if(IsOrganisationAdmin) {
        ApprovalSQL += $@"
        where 
          MSTR_Drone.[AccountID]={PDetail.AccountId}";
      } else {
        ApprovalSQL += $@"
        where 
          NOC_Details.[PilotID]={UserID}";
      }
      ApprovalSQL += "\nOrder By NOC_Details.StartDate DESC";


            var ct = new ExponentPortalEntities();
            
                var NocList = from noc in ct.NOC_Details
                              join user in ct.MSTR_User on noc.PilotID equals user.UserId
                              join drone in ct.MSTR_Drone on noc.DroneID equals drone.DroneId
                              select noc;
                if (IsOrganisationAdmin)
                {
                    NocList = NocList.Where(x => x.MSTR_NOC.AccountID == PDetail.AccountId);

                }
                else
                {
                    NocList = NocList.Where(x => x.PilotID == UserID);
                }
                NocList = NocList.OrderByDescending(x => x.StartDate).Select(x => x);
                List<NOC_Details> NOCList= NocList.ToList();

                AppList = NOCList;

            
           
       String SQLVideo = 
        $@"CASE 
          WHEN 
            D.LastFlightID =(Select Top 1  DroneFlight.ID from DroneFlight  where DroneFlight.DroneId=D.DroneID order by DroneFlight.ID desc) and
            D.FlightTime > DATEADD(MINUTE, -1, GETDATE())
          THEN 
            '<a href=/FlightMap/View/'+ TRY_CONVERT(nvarchar(10), D.LastFlightID) +'><span class=green_icon>&#xf04b;</span></a>'                             
          ELSE 
            ''
          END AS LiveStatus";

      String RPASSql = 
        $@"SELECT 
            D.[DroneName] as RPAS,
            D.[ModelName] as Description,
            D.[CommissionDate],
            M.Name as Manufacture,
            U.GroupName as RPASType,
            {SQLVideo},
            D.[DroneId]
          FROM
            [MSTR_Drone] D
          Left join LUP_Drone M on
            ManufactureID = M.TypeID and
            M.Type='Manufacturer' 
          Left join LUP_Drone U on
            UAVTypeID = U.TypeID and
            U.Type= 'UAVType'";
      if (IsOrganisationAdmin) {
        RPASSql += $@"
        where 
          D.[AccountID]={PDetail.AccountId}";
      } else {
        RPASSql += $@"
        LEFT JOIN M2M_Drone_User ON
         M2M_Drone_User.DroneID = D.DroneID AND
         M2M_Drone_User.UserID = {PDetail.UserId}
        where 
          D.[AccountID]={PDetail.AccountId} AND
          M2M_Drone_User.DroneID IS NOT NULL"; 
      }

      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = RPASSql;
        using (var reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            RPASDetail RPAS = new RPASDetail();
            RPAS.DroneName = RS(reader, "RPAS");
            RPAS.CommissionDate = Util.toDate(RS(reader, "CommissionDate"));
            RPAS.DroneId = Util.toInt(RS(reader, "DroneId"));
            RPAS.ManufactureName = RS(reader, "Manufacture");            
            RPAS.UavTypeName = RS(reader, "RPASType");
            RPAS.LiveStatus = RS(reader, "LiveStatus");
            RPASDet.Add(RPAS);
          }
        }
      }
      UserDashboard.Pilot = PDetail;
      UserDashboard.ApprovalList = AppList;
      UserDashboard.RPASList = RPASDet;
      return UserDashboard;
    }

    private String RS(DbDataReader RS, String FieldName) {
      int FieldID =  RS.GetOrdinal(FieldName);
      return RS.IsDBNull(FieldID) ? String.Empty : RS[FieldName].ToString();      
    }

    public string getOuterPolygon(String coordinates, int BoundaryInMeters) {
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