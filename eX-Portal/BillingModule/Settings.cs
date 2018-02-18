﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BillingModule {
  public static class Settings {
    public static List<BillingRuleTableAndFields> TablesAndFields {
      get {
        var NocFields = new List<NameValuePair> {
          new NameValuePair { Name = "Flat Rate", Value = "1" },
          new NameValuePair { Name = "Daily", Value = "NOC_Details.BillingDays" },
          new NameValuePair { Name = "Total NOC Time (Minutes)", Value = "  BillingTotalMinutes" },
          new NameValuePair { Name = "Peak Time (Minutes)", Value = "BillingPeakMinutes" },
          new NameValuePair { Name = "Off Peak Time (Minutes)", Value = "BillingOffPeakMinutes" },
          new NameValuePair { Name = "Total Area (Meter²)", Value = "BillingArea" },
          new NameValuePair { Name = "Volume (Meter³)", Value = "BillingVolume" },
          new NameValuePair { Name = "Custom", Value = "" }
        };

        return new List<BillingRuleTableAndFields>() {          
          new BillingRuleTableAndFields{
            TableName = new NameValuePair{ Name = "NOC", Value="NOC_Details" },
            Fields = NocFields
          }
        };
      }//get;
    }//public static List<BillingRuleTableAndFields> TablesAndFields
  }//public static class Settings


  public class BillingRuleTableAndFields {
    public NameValuePair TableName { get; set; }
    public List<NameValuePair> Fields { get; set; }
  }

  public class NameValuePair {
    public String Name { get; set; }
    public String Value { get; set; }
  }

}