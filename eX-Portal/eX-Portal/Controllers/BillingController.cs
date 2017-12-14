using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace eX_Portal.Controllers {

  public class BillingController : Controller {


    private ExponentPortalEntities db = new ExponentPortalEntities();

    public ActionResult AllGroups() {
      var Groups = db.BillingRulesGroup.ToList();
      return View(Groups);
    }


    public ActionResult AllRules() {
      var Rules = db.BillingRules.ToList();
      return View(Rules);
    }

        // GET: Billing
    [HttpGet]
    public ActionResult Rules([Bind(Prefix="ID")]int RuleID = 0) {
      Models.BillingRules billingRules = null;
      if (RuleID > 0) {
        billingRules = db.BillingRules.Where(w => w.RuleID == RuleID).FirstOrDefault();
      }
      if (billingRules == null)
        billingRules = new BillingRules();
      return View(billingRules);
    }

    [HttpPost]
    public async Task<ActionResult> Rules(Models.BillingRules billingRules) {
      if (ModelState.IsValid) {
        String ValidationErrors;
        var BillingModule = new BillingModule.RulesManager();
        if (String.Equals(billingRules.CalculateField, "Custom")) {
          billingRules.CalculateField = Request.Form["CalculateFieldCustom"];
        }
        var IsValid = BillingModule.IsValid(billingRules, out ValidationErrors);
        if(!IsValid) { 
          ModelState.AddModelError("", ValidationErrors);
          return View(billingRules);
        }
        if(billingRules.ApplyCondition == null)
          billingRules.ApplyCondition = String.Empty;

        if(billingRules.RuleID > 0) {
          db.Entry(billingRules).State = EntityState.Modified;
        } else {
          db.BillingRules.Add(billingRules);
        }        
        await db.SaveChangesAsync();

        return RedirectToAction("AllRules");
      }
      ModelState.AddModelError("", "Invalid Model State. Please try again...");
      return View(billingRules);
    }

    [HttpGet]
    public ActionResult Group([Bind(Prefix ="ID")]int GroupID = 0) {
      BillingModule.BillingGroup grp = new BillingModule.BillingGroup(GroupID);
      return View(grp);
    }

    [HttpPost]
    public async Task<ActionResult> Group(BillingModule.BillingGroupForm group) {
      await group.Save();
      BillingModule.BillingGroup grp = new BillingModule.BillingGroup(group.GroupID);
      return View(grp);
    }

    public ActionResult Init() {
      var Settiings = new {
        ListItems = BillingModule.Settings.TablesAndFields
      };
      return Json(Settiings, JsonRequestBehavior.AllowGet);
    }

  }//public class BillingController : Controller
}//namespace eX_Portal.Controllers