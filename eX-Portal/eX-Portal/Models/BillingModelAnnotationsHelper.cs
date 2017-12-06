using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eX_Portal.Models {
  public class BillingModelAnnotationsHelper {
  }

  [MetadataType(typeof(BillingRulesHelper))]
  public partial class BillingRules { }

  public class BillingRulesHelper {
    [Required(ErrorMessage = "Rule name is required for reference in future")]
    [Display(Name = "Provide a name for the Rule")]
    public string RuleName { get; set; }

    [Required(ErrorMessage = "Description is required ")]
    [Display(Name = "Desciption")]
    public string RuleDescription { get; set; }

    [Required(ErrorMessage = "Please select where the rule is applied")]
    [Display(Name = "Rule Applied On")]
    public string CalculateOn { get; set; }

    [Required(ErrorMessage = "Select the field of Rule should be based on")]
    [Display(Name = "Rule Based On the Value of")]
    public string CalculateField { get; set; }

    [Display(Name = "Boolean Condition based on the \"Rule Applied On\" Table (Advanced)")]
    public string ApplyCondition { get; set; }
  }//MSTR_AccountHelper
}