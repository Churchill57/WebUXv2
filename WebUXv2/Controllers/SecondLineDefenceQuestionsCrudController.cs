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
    public class SecondLineDefenceQuestionsCrudController : Controller
    {
        private CustomerDbContext db = new CustomerDbContext();

        // GET: SecondLineDefenceQuestionsCrud
        public ActionResult Index()
        {
            var secondLineDefenceQuestions = db.SecondLineDefenceQuestions.Include(s => s.Customer);
            return View(secondLineDefenceQuestions.ToList());
        }

        // GET: SecondLineDefenceQuestionsCrud/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SecondLineDefenceQuestion secondLineDefenceQuestion = db.SecondLineDefenceQuestions.Find(id);
            if (secondLineDefenceQuestion == null)
            {
                return HttpNotFound();
            }
            return View(secondLineDefenceQuestion);
        }

        // GET: SecondLineDefenceQuestionsCrud/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FullName");
            return View();
        }

        // POST: SecondLineDefenceQuestionsCrud/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DateAsked,BenefitOption,ContactMethod,PensionsGuidance,RegulatedAdvice,RiskWarning,ScriptName,CustomerId")] SecondLineDefenceQuestion secondLineDefenceQuestion)
        {
            if (ModelState.IsValid)
            {
                db.SecondLineDefenceQuestions.Add(secondLineDefenceQuestion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FullName", secondLineDefenceQuestion.CustomerId);
            return View(secondLineDefenceQuestion);
        }

        // GET: SecondLineDefenceQuestionsCrud/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SecondLineDefenceQuestion secondLineDefenceQuestion = db.SecondLineDefenceQuestions.Find(id);
            if (secondLineDefenceQuestion == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FullName", secondLineDefenceQuestion.CustomerId);
            return View(secondLineDefenceQuestion);
        }

        // POST: SecondLineDefenceQuestionsCrud/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DateAsked,BenefitOption,ContactMethod,PensionsGuidance,RegulatedAdvice,RiskWarning,ScriptName,CustomerId")] SecondLineDefenceQuestion secondLineDefenceQuestion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(secondLineDefenceQuestion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FullName", secondLineDefenceQuestion.CustomerId);
            return View(secondLineDefenceQuestion);
        }

        // GET: SecondLineDefenceQuestionsCrud/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SecondLineDefenceQuestion secondLineDefenceQuestion = db.SecondLineDefenceQuestions.Find(id);
            if (secondLineDefenceQuestion == null)
            {
                return HttpNotFound();
            }
            return View(secondLineDefenceQuestion);
        }

        // POST: SecondLineDefenceQuestionsCrud/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SecondLineDefenceQuestion secondLineDefenceQuestion = db.SecondLineDefenceQuestions.Find(id);
            db.SecondLineDefenceQuestions.Remove(secondLineDefenceQuestion);
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
