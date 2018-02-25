using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace eX_Portal.BillingModule {

  public class BillingGroupForm {
    private Models.ExponentPortalEntities ctx = new Models.ExponentPortalEntities();


    public String GroupName { get; set; }
    public int GroupID { get; set; }
    public String RulesIndex { get; set; }
    public List<BillingModule.BillingGroupRuleForm> Rule { get; set; }

    public async Task<bool> Save() {
      //if GroupID found - update it
      var Group = ctx.BillingRulesGroup.Where(w => w.GroupID == GroupID).FirstOrDefault();
      if(Group == null) {
        Group = new Models.BillingRulesGroup {
          GroupName = GroupName
        };
        ctx.BillingRulesGroup.Add(Group);
      } else {
        Group.GroupName = GroupName;
        ctx.Entry(Group).State = EntityState.Modified;
      }
      await ctx.SaveChangesAsync();

      //Delete the rules specified in Database first
      await ctx.Database.ExecuteSqlCommandAsync("DELETE FROM BillingRulesCost WHERE GroupID = {0}", GroupID);

      //Save Billing Rules
      var RuleIDs = RulesIndex.Split(',').Select(s => int.Parse(s)).ToList();
      foreach(var rule in Rule) {
        var Seq = RuleIDs.IndexOf(rule.RuleID) * 10;
        var dbRule = new Models.BillingRulesCost {
          RuleID = rule.RuleID,
          Seq = Seq,
          GroupID = Group.GroupID,
          CostDividedBy = rule.CostDividedBy,
          CostMultipliedBy = rule.CostMultipliedBy,
          IsSkipRest = (rule.IsActive ? (byte)0 : (byte)1)
        };
        ctx.BillingRulesCost.Add(dbRule);
      }
      await ctx.SaveChangesAsync();

      return true;
    }
  }


  public class BillingGroupRuleForm {
    public int RuleID { get; set; }
    public Decimal CostDividedBy { get; set; }
    public Decimal CostMultipliedBy { get; set; }
    public bool IsActive { get; set; }
    public int Seq { get; set; }
  }

  public class BillingGroup {
    Models.ExponentPortalEntities ctx = new Models.ExponentPortalEntities();

    public BillingGroup(int GroupID = 0) {
      if (GroupID > 0) {
        var Group = ctx.BillingRulesGroup.Where(w => w.GroupID == GroupID).FirstOrDefault();
        GroupName = Group.GroupName;
        this.GroupID = GroupID;
      } else {
        
      }
      LoadRues(GroupID);

    }//public BillingGroup

    public BillingGroup() {
    }//public BillingGroup


    public String GroupName { get; set; }
    public int GroupID { get; set; }

    public List<BillingGroupRule> Rules { get; private set; }
    public List<int> RulesIndex { get; private set; }

    public String RulesIndexString {
      get {
        return String.Join(",", RulesIndex.ToArray());
      }
    }


    private void LoadRues(int GroupID = 0) {
      var Query =
        from rule in ctx.BillingRules
        join grp in ctx.BillingRulesCost.Where(w => w.GroupID == GroupID) on
          rule.RuleID equals grp.RuleID into grpLeft
        from grpSelect in grpLeft.DefaultIfEmpty()
        orderby
          grpSelect.Seq
        select new BillingGroupRule {
          RuleID = rule.RuleID,
          RuleName = rule.RuleName,
          CalculateOn = rule.CalculateOn,
          CalculateField = rule.CalculateField,
          Description = rule.RuleDescription,
          CostDividedBy = grpSelect.CostDividedBy,
          CostMultipliedBy = grpSelect.CostMultipliedBy,
          ApplyCondition = rule.ApplyCondition,
          IsActive = (grpSelect.IsSkipRest == 0),
          RuleDescription = rule.RuleDescription
        };
      Rules = Query.ToList();
      RulesIndex = Rules.Select(s => s.RuleID).ToList();
    }


    public async Task<List<BillingGroupRule>> GenerateBilling(BillingNOC noc, Models.DroneFlight flight = null) {
      using (var db = new Models.ExponentPortalEntities()) {
        await db.Database.Connection.OpenAsync();
        await CreateTempTableFor(noc, db);        
        foreach (var rule in Rules.Where(w => w.IsActive && w.CalculateOn == "NOC_Details")) {
          await rule.ApplyNoc(noc, db);
        }
        foreach (var rule in Rules.Where(w => w.IsActive && w.CalculateOn == "DroneFlight")) {
          await rule.ApplyDroneFlight(flight, db);
        }
      }
      //if no flight return only the NOC Rules
      if(flight == null)
        return Rules.Where(w => w.IsActive && w.CalculateOn == "NOC_Details").ToList();
      
      //Return all active ruels applied
      return Rules.Where(w => w.IsActive).ToList();
    }


    public async Task<List<BillingGroupRule>> GenerateEstimate(BillingNOC noc) {
      using (var db = new Models.ExponentPortalEntities()) {
        await db.Database.Connection.OpenAsync();
        await CreateTempTableFor(noc, db);
        foreach (var rule in Rules.Where(w => w.IsActive && w.CalculateOn == "NOC_Details")) {
          await rule.ApplyNoc(noc, db);
        }
        foreach (var rule in Rules.Where(w => w.IsActive && w.CalculateOn == "DroneFlight")) {
          await rule.ApplyFlightEstimate(noc, db);
        }
      }

      //Return all active ruels applied
      return Rules.Where(w => w.IsActive).ToList();
    }


    private async Task CreateTempTableFor(BillingNOC noc, Models.ExponentPortalEntities db) {
      String SQL1 = @"
CREATE TABLE [#NOC_Details](
	[PilotID] [int] NOT NULL,
	[DroneID] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[MinAltitude] [int] NOT NULL,
	[MaxAltitude] [int] NOT NULL,
	[Coordinates] [text] NOT NULL,
	[LOS] [char](5) NULL,
	[BillingDays] [int] NOT NULL,
	[BillingTotalMinutes] [int] NOT NULL,
	[BillingPeakMinutes] [int] NOT NULL,
	[BillingOffPeakMinutes] [int] NOT NULL,
	[BillingArea] decimal NOT NULL,
	[BillingVolume] decimal NOT NULL
)";

      String SQL2 = 
        $@"INSERT INTO [#NOC_Details](
      [PilotID]
     ,[DroneID]
     ,[StartDate]
     ,[EndDate]
     ,[StartTime]
     ,[EndTime]
     ,[MinAltitude]
     ,[MaxAltitude]
     ,[Coordinates]
     ,[LOS]
     ,[BillingDays]
     ,[BillingTotalMinutes]
     ,[BillingPeakMinutes]
     ,[BillingOffPeakMinutes]
     ,[BillingArea]
     ,[BillingVolume]
) VALUES(
    0
   ,0
   ,'{noc.StartDate.ToString("yyyy-MM-dd")}'
   ,'{noc.EndDate.ToString("yyyy-MM-dd")}'
   ,'{noc.StartTime.ToString()}'
   ,'{noc.EndTime.ToString()}'
   ,{noc.MinAltitude}
   ,{noc.MaxAltitude}
   ,'{noc.Coordinates}'
   ,'{noc.LOS}'
   ,{noc.BillingDays}
   ,{noc.BillingTotalMinutes}
   ,{noc.BillingPeakMinutes}
   ,{noc.BillingOffPeakMinutes}
   ,{noc.BillingArea}
   ,{noc.BillingVolume}
)";

      await db.Database.ExecuteSqlCommandAsync(SQL1);
      await db.Database.ExecuteSqlCommandAsync(SQL2);

    }


  }

  public class BillingGroupRule {
    private Decimal _CostDividedBy = 0;
    private Decimal _CostMultipliedBy = 0;
    private Decimal _CalculatedCost = 0;

    public int RuleID { get; set; }
    public String RuleName { get; set; }
    public String Description { get; set; }
    public String CalculateOn { get; set; }
    public String CalculateField { get; set; }
    public String ApplyCondition { get; set; }
    public String RuleDescription { get; set; }

    public Decimal? CostDividedBy {
      get { return _CostDividedBy; } 
      set {
        _CostDividedBy = (value == null ? 0 : (Decimal)value);
        if (_CostDividedBy == 0)
          _CostDividedBy = 1;
      }
    }
    public Decimal? CostMultipliedBy {
      get { return _CostMultipliedBy; }
      set { _CostMultipliedBy = (value == null ? 0 : (Decimal)value); }
    }
    public bool IsActive {get; set;}
    public Decimal CalculatedCost {
      get { return _CalculatedCost;}
    }

    public async Task ApplyNoc(BillingNOC noc, Models.ExponentPortalEntities db) {
      String _CField = CalculateField.Replace("NOC_Details.", "#NOC_Details.");
      String SQL =
        $@"SELECT {_CField} * {CostMultipliedBy} / {_CostDividedBy} FROM #NOC_Details";
      if(!String.IsNullOrWhiteSpace(ApplyCondition)) {
        String _ACondition = ApplyCondition.Replace("NOC_Details.", "#NOC_Details.");
        SQL += $" WHERE ({_ACondition})";
      }

      using (var cmd = db.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var Result = await cmd.ExecuteScalarAsync();
        if (Result == null)
          _CalculatedCost = 0;
        else
          Decimal.TryParse(Result.ToString(), out _CalculatedCost);
      }//using ctx.Database.Connection.CreateCommand

    }


    public async Task ApplyFlightEstimate(BillingNOC noc, Models.ExponentPortalEntities db) {
      String SQL =
        $@"SELECT 
          {CalculateField} * {CostMultipliedBy} / {_CostDividedBy} 
        FROM #NOC_Details";
      if (!String.IsNullOrWhiteSpace(ApplyCondition)) {
        String _ACondition = ApplyCondition.Replace("DroneFlight.", "#NOC_Details.");
        SQL += $" WHERE ({_ACondition})";

      }

      using (var cmd = db.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var Result = await cmd.ExecuteScalarAsync();
        if (Result == null)
          _CalculatedCost = 0;
        else
          Decimal.TryParse(Result.ToString(), out _CalculatedCost);
      }//using ctx.Database.Connection.CreateCommand
    }

    public async Task ApplyDroneFlight(Models.DroneFlight flight, Models.ExponentPortalEntities db) {
      if (flight == null)
        return;

      String SQL =
        $@"SELECT 
          {CalculateField} * {CostMultipliedBy} / {_CostDividedBy} 
        FROM DroneFlight
          WHERE ID={flight.ID}";
      if (!String.IsNullOrWhiteSpace(ApplyCondition)) {
        SQL += $" AND ({ApplyCondition})";
      }

      using (var cmd = db.Database.Connection.CreateCommand()) {
        cmd.CommandText = SQL;
        var Result = await cmd.ExecuteScalarAsync();
        if (Result == null)
          _CalculatedCost = 0;
        else
          Decimal.TryParse(Result.ToString(), out _CalculatedCost);
      }//using ctx.Database.Connection.CreateCommand
    }
  }
}