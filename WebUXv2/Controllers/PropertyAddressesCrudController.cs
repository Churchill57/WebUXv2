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
    public class PropertyAddressesCrudController : Controller
    {
        private CustomerDbContext db = new CustomerDbContext();

        // GET: PropertyAddressesCrud
        public ActionResult Index()
        {
            var propertyAddresses = db.PropertyAddresses.Include(p => p.Customer);
            return View(propertyAddresses.ToList());
        }

        // GET: PropertyAddressesCrud/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyAddress propertyAddress = db.PropertyAddresses.Find(id);
            if (propertyAddress == null)
            {
                return HttpNotFound();
            }
            return View(propertyAddress);
        }

        // GET: PropertyAddressesCrud/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Title");
            return View();
        }

        // POST: PropertyAddressesCrud/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Line1,Line2,Line3,PostCode,CustomerId")] PropertyAddress propertyAddress)
        {
            if (ModelState.IsValid)
            {
                db.PropertyAddresses.Add(propertyAddress);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Title", propertyAddress.CustomerId);
            return View(propertyAddress);
        }

        // GET: PropertyAddressesCrud/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyAddress propertyAddress = db.PropertyAddresses.Find(id);
            if (propertyAddress == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Title", propertyAddress.CustomerId);
            return View(propertyAddress);
        }

        // POST: PropertyAddressesCrud/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Line1,Line2,Line3,PostCode,CustomerId")] PropertyAddress propertyAddress)
        {
            if (ModelState.IsValid)
            {
                db.Entry(propertyAddress).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Title", propertyAddress.CustomerId);
            return View(propertyAddress);
        }

        // GET: PropertyAddressesCrud/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyAddress propertyAddress = db.PropertyAddresses.Find(id);
            if (propertyAddress == null)
            {
                return HttpNotFound();
            }
            return View(propertyAddress);
        }

        // POST: PropertyAddressesCrud/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PropertyAddress propertyAddress = db.PropertyAddresses.Find(id);
            db.PropertyAddresses.Remove(propertyAddress);
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
