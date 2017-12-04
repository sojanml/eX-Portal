using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;

namespace eX_Portal.Controllers {
  public class BillingRulesController : Controller {
    private ExponentPortalEntities db = new ExponentPortalEntities();

    // GET: BillingRules
    public async Task<ActionResult> Index() {
      return View(await db.BillingRules.ToListAsync());
    }

    // GET: BillingRules/Details/5
    public async Task<ActionResult> Details(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      BillingRules billingRules = await db.BillingRules.FindAsync(id);
      if (billingRules == null) {
        return HttpNotFound();
      }
      return View(billingRules);
    }

    // GET: BillingRules/Create
    public ActionResult Create() {
      return View();
    }

    // POST: BillingRules/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Include = "RuleID,RuleName,RuleDescription,CalculateField,ApplyCondition")] BillingRules billingRules) {
      if (ModelState.IsValid) {
        db.BillingRules.Add(billingRules);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      return View(billingRules);
    }

    // GET: BillingRules/Edit/5
    public async Task<ActionResult> Edit(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      BillingRules billingRules = await db.BillingRules.FindAsync(id);
      if (billingRules == null) {
        return HttpNotFound();
      }
      return View(billingRules);
    }

    // POST: BillingRules/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "RuleID,RuleName,RuleDescription,CalculateField,ApplyCondition")] BillingRules billingRules) {
      if (ModelState.IsValid) {
        db.Entry(billingRules).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      return View(billingRules);
    }

    // GET: BillingRules/Delete/5
    public async Task<ActionResult> Delete(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      BillingRules billingRules = await db.BillingRules.FindAsync(id);
      if (billingRules == null) {
        return HttpNotFound();
      }
      return View(billingRules);
    }

    // POST: BillingRules/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id) {
      BillingRules billingRules = await db.BillingRules.FindAsync(id);
      db.BillingRules.Remove(billingRules);
      await db.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
