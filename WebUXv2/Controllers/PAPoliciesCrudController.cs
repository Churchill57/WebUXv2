using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Models;

namespace WebUXv2.Controllers
{
    public class PAPoliciesCrudController : Controller
    {
        private CustomerDbContext db = new CustomerDbContext();

        // GET: PAPoliciesCrud
        public ActionResult Index()
        {
            var pAPolicies = db.PAPolicies.Include(p => p.Annuitant).Include(p => p.Beneficiary).Include(p => p.Dependant);
            return View(pAPolicies.ToList());
        }

        // GET: PAPoliciesCrud/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PAPolicy pAPolicy = db.PAPolicies.Find(id);
            if (pAPolicy == null)
            {
                return HttpNotFound();
            }
            return View(pAPolicy);
        }

        // GET: PAPoliciesCrud/Create
        public ActionResult Create()
        {
            ViewBag.AnnuitantId = new SelectList(db.Customers, "Id", "FullName");
            ViewBag.BeneficiaryId = new SelectList(db.Customers, "Id", "FullName");
            ViewBag.DependantId = new SelectList(db.Customers, "Id", "FullName");
            return View();
        }

        // POST: PAPoliciesCrud/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Premium,AnnuitantId,DependantId,BeneficiaryId")] PAPolicy pAPolicy)
        {
            if (ModelState.IsValid)
            {
                db.PAPolicies.Add(pAPolicy);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AnnuitantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.AnnuitantId);
            ViewBag.BeneficiaryId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.BeneficiaryId);
            ViewBag.DependantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.DependantId);
            return View(pAPolicy);
        }

        // GET: PAPoliciesCrud/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PAPolicy pAPolicy = db.PAPolicies.Find(id);
            if (pAPolicy == null)
            {
                return HttpNotFound();
            }
            ViewBag.AnnuitantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.AnnuitantId);
            ViewBag.BeneficiaryId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.BeneficiaryId);
            ViewBag.DependantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.DependantId);
            return View(pAPolicy);
        }

        // POST: PAPoliciesCrud/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Premium,AnnuitantId,DependantId,BeneficiaryId")] PAPolicy pAPolicy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pAPolicy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AnnuitantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.AnnuitantId);
            ViewBag.BeneficiaryId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.BeneficiaryId);
            ViewBag.DependantId = new SelectList(db.Customers, "Id", "FullName", pAPolicy.DependantId);
            return View(pAPolicy);
        }

        // GET: PAPoliciesCrud/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PAPolicy pAPolicy = db.PAPolicies.Find(id);
            if (pAPolicy == null)
            {
                return HttpNotFound();
            }
            return View(pAPolicy);
        }

        // POST: PAPoliciesCrud/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PAPolicy pAPolicy = db.PAPolicies.Find(id);
            db.PAPolicies.Remove(pAPolicy);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
