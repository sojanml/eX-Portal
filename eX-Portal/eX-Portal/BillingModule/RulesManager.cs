using System;
using System.Collections.Generic;
using eX_Portal.Models;

namespace eX_Portal.BillingModule {
  public class RulesManager {

    ExponentPortalEntities ctx = new ExponentPortalEntities();
    public bool IsValid(BillingRules Rule, out String ValidationError) {
      String SQL;
      ValidationError = String.Empty;
      //Check the table and field is defined
      SQL = $"SELECT {Rule.CalculateField} FROM {Rule.CalculateOn} WHERE 1 = 0";

      if (!String.IsNullOrWhiteSpace(Rule.ApplyCondition))
        SQL += $" AND ({Rule.ApplyCondition})";

      try {
        ctx.Database.Connection.Open();
        var result = ctx.Database.ExecuteSqlCommand(SQL);
      } catch(Exception e) {
        ValidationError = e.Message;
        return false;
      }

      return true;
    }
  }
}
