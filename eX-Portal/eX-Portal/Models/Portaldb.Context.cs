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
        public virtual DbSet<MSTR_Account> MSTR_Account { get; set; }
        public virtual DbSet<MSTR_Drone> MSTR_Drone { get; set; }
        public virtual DbSet<MSTR_DroneService> MSTR_DroneService { get; set; }
        public virtual DbSet<MSTR_Menu> MSTR_Menu { get; set; }
        public virtual DbSet<MSTR_Parts> MSTR_Parts { get; set; }
        public virtual DbSet<MSTR_Product> MSTR_Product { get; set; }
        public virtual DbSet<MSTR_Profile> MSTR_Profile { get; set; }
        public virtual DbSet<MSTR_User> MSTR_User { get; set; }
        public virtual DbSet<ProductDefinition> ProductDefinitions { get; set; }
        public virtual DbSet<ProductTransaction> ProductTransactions { get; set; }
    }
}