﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eX_Portal.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class ExponentPortalEntities : DbContext
    {
        public ExponentPortalEntities()
            : base("name=ExponentPortalEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<DroneData> DroneDatas { get; set; }
        public virtual DbSet<DroneDefinition> DroneDefinitions { get; set; }
        public virtual DbSet<LUP_Drone> LUP_Drone { get; set; }
        public virtual DbSet<LUP_Product> LUP_Product { get; set; }
        public virtual DbSet<M2M_DroneDetails> M2M_DroneDetails { get; set; }
        public virtual DbSet<M2M_DroneParts> M2M_DroneParts { get; set; }
        public virtual DbSet<M2M_DroneServiceParts> M2M_DroneServiceParts { get; set; }
        public virtual DbSet<M2M_UserAccount> M2M_UserAccount { get; set; }
        public virtual DbSet<MSTR_DroneService> MSTR_DroneService { get; set; }
        public virtual DbSet<MSTR_Menu> MSTR_Menu { get; set; }
        public virtual DbSet<MSTR_Parts> MSTR_Parts { get; set; }
        public virtual DbSet<MSTR_Product> MSTR_Product { get; set; }
        public virtual DbSet<MSTR_Profile> MSTR_Profile { get; set; }
        public virtual DbSet<ProductDefinition> ProductDefinitions { get; set; }
        public virtual DbSet<ProductTransaction> ProductTransactions { get; set; }
        public virtual DbSet<ProductHistory> ProductHistories { get; set; }
        public virtual DbSet<DroneCheckList> DroneCheckList { get; set; }
        public virtual DbSet<MSTR_DroneCheckList> MSTR_DroneCheckList { get; set; }
        public virtual DbSet<MSTR_DroneCheckListItems> MSTR_DroneCheckListItems { get; set; }
        public virtual DbSet<DroneCheckListItem> DroneCheckListItems { get; set; }
        public virtual DbSet<DroneCheckListValidation> DroneCheckListValidations { get; set; }
        public virtual DbSet<LiveDrone> LiveDrones { get; set; }
        public virtual DbSet<M2M_ProfileMenu> M2M_ProfileMenu { get; set; }
        public virtual DbSet<DroneDocument> DroneDocuments { get; set; }
        public virtual DbSet<BlackBoxData> BlackBoxDatas { get; set; }
        public virtual DbSet<MSTR_User_Pilot> MSTR_User_Pilot { get; set; }
        public virtual DbSet<MSTR_User_Pilot_Certification> MSTR_User_Pilot_Certification { get; set; }
        public virtual DbSet<MSTR_User_Pilot_ExponentUAS> MSTR_User_Pilot_ExponentUAS { get; set; }
        public virtual DbSet<MSTR_Pilot_Log> MSTR_Pilot_Log { get; set; }
        public virtual DbSet<DroneFlight> DroneFlights { get; set; }
        public virtual DbSet<MSTR_Account> MSTR_Account { get; set; }
        public virtual DbSet<MSTR_User> MSTR_User { get; set; }
        public virtual DbSet<PayLoadData> PayLoadDatas { get; set; }
        public virtual DbSet<PayLoadMapData> PayLoadMapDatas { get; set; }
        public virtual DbSet<PayLoadYard> PayLoadYards { get; set; }
        public virtual DbSet<PayLoadFlight> PayLoadFlights { get; set; }
        public virtual DbSet<PayLoadImageData> PayLoadImageDatas { get; set; }
        public virtual DbSet<PayLoadYardGrid> PayLoadYardGrids { get; set; }
        public virtual DbSet<GCA_Approval> GCA_Approval { get; set; }
        public virtual DbSet<PortalAlert> PortalAlerts { get; set; }
        public virtual DbSet<PayLoad_AutoFix> PayLoad_AutoFix { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<DroneFlightVideo> DroneFlightVideos { get; set; }
        public virtual DbSet<PortalAuth> PortalAuths { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<MSTR_Drone_Setup> MSTR_Drone_Setup { get; set; }
        public virtual DbSet<PortalAlertEmail> PortalAlertEmails { get; set; }
        public virtual DbSet<PortalAlertRegistration> PortalAlertRegistrations { get; set; }
        public virtual DbSet<FlightInfo> FlightInfoes { get; set; }
        public virtual DbSet<MSTR_RPAS_User> MSTR_RPAS_User { get; set; }
        public virtual DbSet<MSTR_BlackBox> MSTR_BlackBox { get; set; }
        public virtual DbSet<BlackBoxTransaction> BlackBoxTransactions { get; set; }
        public virtual DbSet<FlightMapData> FlightMapDatas { get; set; }
        public virtual DbSet<MSTR_Drone> MSTR_Drone { get; set; }
        public virtual DbSet<BlackBoxCost> BlackBoxCosts { get; set; }
        public virtual DbSet<SMSTable> SMSTables { get; set; }
    
        public virtual ObjectResult<usp_Portal_CreateDrone_Result> usp_Portal_CreateDrone(Nullable<int> ownerID, Nullable<int> manufacturerID, Nullable<int> uAVTypeID, Nullable<System.DateTime> commissionDate)
        {
            var ownerIDParameter = ownerID.HasValue ?
                new ObjectParameter("OwnerID", ownerID) :
                new ObjectParameter("OwnerID", typeof(int));
    
            var manufacturerIDParameter = manufacturerID.HasValue ?
                new ObjectParameter("ManufacturerID", manufacturerID) :
                new ObjectParameter("ManufacturerID", typeof(int));
    
            var uAVTypeIDParameter = uAVTypeID.HasValue ?
                new ObjectParameter("UAVTypeID", uAVTypeID) :
                new ObjectParameter("UAVTypeID", typeof(int));
    
            var commissionDateParameter = commissionDate.HasValue ?
                new ObjectParameter("CommissionDate", commissionDate) :
                new ObjectParameter("CommissionDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_Portal_CreateDrone_Result>("usp_Portal_CreateDrone", ownerIDParameter, manufacturerIDParameter, uAVTypeIDParameter, commissionDateParameter);
        }
    
        public virtual ObjectResult<usp_Portal_GetDroneDropDown_Result> usp_Portal_GetDroneDropDown(string type)
        {
            var typeParameter = type != null ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_Portal_GetDroneDropDown_Result>("usp_Portal_GetDroneDropDown", typeParameter);
        }
    
        public virtual ObjectResult<usp_Portal_DroneNameList_Result> usp_Portal_DroneNameList()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_Portal_DroneNameList_Result>("usp_Portal_DroneNameList");
        }
    
        public virtual ObjectResult<usp_Portal_DroneServiceType_Result> usp_Portal_DroneServiceType(string type)
        {
            var typeParameter = type != null ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_Portal_DroneServiceType_Result>("usp_Portal_DroneServiceType", typeParameter);
        }
    
        public virtual ObjectResult<usp_Portal_GetDroneParts_Result> usp_Portal_GetDroneParts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_Portal_GetDroneParts_Result>("usp_Portal_GetDroneParts");
        }
    
        public virtual int Recover_Dropped_Objects_Proc(string database_Name, Nullable<System.DateTime> date_From, Nullable<System.DateTime> date_To)
        {
            var database_NameParameter = database_Name != null ?
                new ObjectParameter("Database_Name", database_Name) :
                new ObjectParameter("Database_Name", typeof(string));
    
            var date_FromParameter = date_From.HasValue ?
                new ObjectParameter("Date_From", date_From) :
                new ObjectParameter("Date_From", typeof(System.DateTime));
    
            var date_ToParameter = date_To.HasValue ?
                new ObjectParameter("Date_To", date_To) :
                new ObjectParameter("Date_To", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("Recover_Dropped_Objects_Proc", database_NameParameter, date_FromParameter, date_ToParameter);
        }
    
        public virtual int sp_Recover_Dropped_Objects(string database_Name, Nullable<System.DateTime> date_From, Nullable<System.DateTime> date_To)
        {
            var database_NameParameter = database_Name != null ?
                new ObjectParameter("Database_Name", database_Name) :
                new ObjectParameter("Database_Name", typeof(string));
    
            var date_FromParameter = date_From.HasValue ?
                new ObjectParameter("Date_From", date_From) :
                new ObjectParameter("Date_From", typeof(System.DateTime));
    
            var date_ToParameter = date_To.HasValue ?
                new ObjectParameter("Date_To", date_To) :
                new ObjectParameter("Date_To", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_Recover_Dropped_Objects", database_NameParameter, date_FromParameter, date_ToParameter);
        }
    
        public virtual int sp_alterdiagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_alterdiagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_creatediagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_creatediagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_dropdiagram(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_dropdiagram", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagramdefinition_Result> sp_helpdiagramdefinition(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagramdefinition_Result>("sp_helpdiagramdefinition", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagrams_Result> sp_helpdiagrams(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagrams_Result>("sp_helpdiagrams", diagramnameParameter, owner_idParameter);
        }
    
        public virtual int sp_renamediagram(string diagramname, Nullable<int> owner_id, string new_diagramname)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var new_diagramnameParameter = new_diagramname != null ?
                new ObjectParameter("new_diagramname", new_diagramname) :
                new ObjectParameter("new_diagramname", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_renamediagram", diagramnameParameter, owner_idParameter, new_diagramnameParameter);
        }
    
        public virtual int sp_upgraddiagrams()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_upgraddiagrams");
        }
    
        public virtual int usp_PayLoad_AutoCorrectGrid(string flightUniqueID, Nullable<int> yardID)
        {
            var flightUniqueIDParameter = flightUniqueID != null ?
                new ObjectParameter("FlightUniqueID", flightUniqueID) :
                new ObjectParameter("FlightUniqueID", typeof(string));
    
            var yardIDParameter = yardID.HasValue ?
                new ObjectParameter("YardID", yardID) :
                new ObjectParameter("YardID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_PayLoad_AutoCorrectGrid", flightUniqueIDParameter, yardIDParameter);
        }
    
        public virtual int usp_Portal_GetFlightChartData(string accountID, Nullable<int> isAccess)
        {
            var accountIDParameter = accountID != null ?
                new ObjectParameter("AccountID", accountID) :
                new ObjectParameter("AccountID", typeof(string));
    
            var isAccessParameter = isAccess.HasValue ?
                new ObjectParameter("IsAccess", isAccess) :
                new ObjectParameter("IsAccess", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_Portal_GetFlightChartData", accountIDParameter, isAccessParameter);
        }
    
        public virtual int usp_Portal_GetLastFlightChartData(string accountID, Nullable<int> isAccess)
        {
            var accountIDParameter = accountID != null ?
                new ObjectParameter("AccountID", accountID) :
                new ObjectParameter("AccountID", typeof(string));
    
            var isAccessParameter = isAccess.HasValue ?
                new ObjectParameter("IsAccess", isAccess) :
                new ObjectParameter("IsAccess", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_Portal_GetLastFlightChartData", accountIDParameter, isAccessParameter);
        }
    
        public virtual int usp_Portal_GetPilotChartData(string accountID, Nullable<int> isAccess)
        {
            var accountIDParameter = accountID != null ?
                new ObjectParameter("AccountID", accountID) :
                new ObjectParameter("AccountID", typeof(string));
    
            var isAccessParameter = isAccess.HasValue ?
                new ObjectParameter("IsAccess", isAccess) :
                new ObjectParameter("IsAccess", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_Portal_GetPilotChartData", accountIDParameter, isAccessParameter);
        }
    }
}
