using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class TaskTypeCrudController : Controller
    {
        private TaskDbContext db = new TaskDbContext();

        // GET: TaskType
        public ActionResult Index()
        {
            return View(db.TaskTypes.ToList());
        }

        // GET: TaskType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskType taskType = db.TaskTypes.Find(id);
            if (taskType == null)
            {
                return HttpNotFound();
            }
            return View(taskType);
        }

        // GET: TaskType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaskType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Host,Name,RootComponentName,SearchTags")] TaskType taskType)
        {
            if (ModelState.IsValid)
            {
                db.TaskTypes.Add(taskType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(taskType);
        }

        // GET: TaskType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskType taskType = db.TaskTypes.Find(id);
            if (taskType == null)
            {
                return HttpNotFound();
            }
            return View(taskType);
        }

        // POST: TaskType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Host,Name,RootComponentName,SearchTags,TaskInputs")] TaskType taskType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(taskType).State = EntityState.Modified;
                foreach (var input in taskType.TaskInputs)
                {
                    db.Entry(input).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskType);
        }

        // GET: TaskType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskType taskType = db.TaskTypes.Find(id);
            if (taskType == null)
            {
                return HttpNotFound();
            }
            return View(taskType);
        }

        // POST: TaskType/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TaskType taskType = db.TaskTypes.Find(id);
            db.TaskTypes.Remove(taskType);
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
