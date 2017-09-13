using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebUXv2.Models;

namespace WebUXv2.Controllers
{
    public class LuTaskController : Controller
    {
        private TaskDbContext db = new TaskDbContext();

        // GET: LuTask
        public ActionResult Index()
        {
            var luTasks = db.LuTasks.Include(l => l.ParentLuTask);
            return View(luTasks.ToList());
        }

        // GET: LuTask/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LuTask luTask = db.LuTasks.Find(id);
            if (luTask == null)
            {
                return HttpNotFound();
            }
            luTask.ParentLuTask = db.LuTasks.Find(luTask.ParentLuTaskId);

            return View(luTask);
        }

        // GET: LuTask/Create
        public ActionResult Create()
        {
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name");
            return View();
        }

        // POST: LuTask/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,ClientRef,State,Status,ParentLuTaskId")] LuTask luTask)
        {
            if (ModelState.IsValid)
            {
                db.LuTasks.Add(luTask);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", luTask.ParentLuTaskId);
            return View(luTask);
        }

        // GET: LuTask/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LuTask luTask = db.LuTasks.Find(id);
            if (luTask == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", luTask.ParentLuTaskId);
            return View(luTask);
        }

        // POST: LuTask/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,ClientRef,State,Status,ParentLuTaskId")] LuTask luTask)
        {
            if (ModelState.IsValid)
            {
                db.Entry(luTask).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentLuTaskId = new SelectList(db.LuTasks, "Id", "Name", luTask.ParentLuTaskId);
            return View(luTask);
        }

        // GET: LuTask/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LuTask luTask = db.LuTasks.Find(id);
            if (luTask == null)
            {
                return HttpNotFound();
            }
            return View(luTask);
        }

        // POST: LuTask/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LuTask luTask = db.LuTasks.Find(id);
            db.LuTasks.Remove(luTask);
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
