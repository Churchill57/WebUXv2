using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebUXv2.Models;

namespace WebUXv2.Controllers
{
    public class UxTaskController : Controller
    {
        private TaskDbContext db = new TaskDbContext();

        // GET: UxTask
        public ActionResult Index()
        {
            var uxTasks = db.UxTasks.Include(u => u.ParentLuTask);
            return View(uxTasks.ToList());
        }

        // GET: UxTask/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UxTask uxTask = db.UxTasks.Find(id);
            if (uxTask == null)
            {
                return HttpNotFound();
            }
            return View(uxTask);
        }

        // GET: UxTask/Create
        public ActionResult Create()
        {
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name");
            return View();
        }

        // POST: UxTask/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,ClientRef,State,ParentLuTaskId")] UxTask uxTask)
        {
            if (ModelState.IsValid)
            {
                db.UxTasks.Add(uxTask);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", uxTask.ParentLuTaskId);
            return View(uxTask);
        }

        // GET: UxTask/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UxTask uxTask = db.UxTasks.Find(id);
            if (uxTask == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", uxTask.ParentLuTaskId);
            return View(uxTask);
        }

        // POST: UxTask/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,ClientRef,State,ParentLuTaskId")] UxTask uxTask)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uxTask).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", uxTask.ParentLuTaskId);
            return View(uxTask);
        }

        // GET: UxTask/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UxTask uxTask = db.UxTasks.Find(id);
            if (uxTask == null)
            {
                return HttpNotFound();
            }
            return View(uxTask);
        }

        // POST: UxTask/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UxTask uxTask = db.UxTasks.Find(id);
            db.UxTasks.Remove(uxTask);
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
