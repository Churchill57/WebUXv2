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
using WebUXv2.LogicalUnits.TaskType;
using WebUXv2.Models;
using WebUXv2.UserExperiences.TaskType;

namespace WebUXv2.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class TaskTypeController : Controller
    {
        private TaskDbContext db = new TaskDbContext();

        private readonly ITaskManager _tm;
        private readonly ITaskManagerController _tmc;
        private readonly IEntityContextManager _ctxMan;

        public TaskTypeController() : this( new TaskManager(), new TaskManagerController(), Singleton.Instance.EntityContextManager) { }
        public TaskTypeController(ITaskManager tm, ITaskManagerController tmc, IEntityContextManager ctxMan)
        {
            _tm = tm;
            _tmc = tmc;
            _ctxMan = ctxMan;
        }

        ~TaskTypeController() { ((IDisposable)_tmc).Dispose(); }

        /// <summary>
        /// Provides a local reference to the task manager controller which has the necessary controller context setup.
        /// </summary>
        /// <returns></returns>
        private ITaskManagerController Tmc()
        {
            if (_tmc.ControllerContext == null)
            {
                // Essential to make User context (for example) available to the task manager controller.
                _tmc.ControllerContext = new ControllerContext(this.Request.RequestContext, (ControllerBase)_tmc);

                // Ideally the controller context would be passed from the constructor of this controller into
                // the task manager controller constructor. However, the 'this' keyword cannot be passed as a
                // parameter in this way.
                // i.e. public CustomerController() : this( new TaskManager(), new TaskManagerController(this))
                // does not allow the passing of 'this' -------------------------------------------------^ 
            }
            return _tmc;
        }

        [Authorize]
        public ActionResult Bootstrap()
        {
            return RedirectToAction("StartTask", "TaskManager", new { componentName = typeof(LuRunAndManageApps).Name });
        }

        public ActionResult SearchAndRunApps(int? uxTaskId)
        {
            // Ensures only one instance of uxSearchAndRunApps runs per user session.
            // TODO: Consider introducing a Singletom attribute for LUs to do this automatically!!
            int taskId = 0;
            if (uxTaskId.HasValue)
            {
                taskId = uxTaskId.Value;
                HttpContext.Session["SearchAndRunAppsTaskId"] = taskId;
            }
            else
            {
                taskId = (int)HttpContext.Session["SearchAndRunAppsTaskId"];
                return RedirectToAction("SearchAndRunApps", new { uxTaskId = taskId, componentName = typeof(UxSearchAndRunApps).Name });
            }

            // If the task does not exists start a new one.
            var uxSearchAndRunApps = _tm.GetUserExperience(taskId) as UxSearchAndRunApps;
            if (uxSearchAndRunApps == null) return Tmc().StartTask(typeof(LuRunAndManageApps).Name);

            var searchText = uxSearchAndRunApps.SearchText;

            ViewBag.uxTaskId = uxTaskId;

            return View("SearchAndRunApps", model: searchText); // N.B. without explicitly naming model (string) parameter, MVC can get its View method overloads in a mucking fuddle.
        }

        public ActionResult SearchMatchingApps(int uxTaskId, string searchText)
        {
            var uxSearchAndRunApps = _tm.GetUserExperience(uxTaskId) as UxSearchAndRunApps;
            if (uxSearchAndRunApps == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxSearchAndRunApps.SearchText = searchText;
            uxSearchAndRunApps.Save();

            var showAll = String.IsNullOrEmpty(searchText) || searchText.Trim() == "*";
            var model = (
                from assy in AppDomain.CurrentDomain.GetAssemblies()
                from type in assy.GetTypes()
                let launchAttr = type.GetCustomAttribute<LaunchableComponentAttribute>()
                where Attribute.IsDefined(type, typeof(LaunchableComponentAttribute))
                let titleAttr = type.GetCustomAttribute<ComponentTitleAttribute>()
                let matches = StringMatch(titleAttr.Title + " " + launchAttr.SearchTags, searchText)
                where matches > 0 || showAll
                orderby matches descending 
                select new TaskType() { Id = 0, RootComponentName = type.Name, Name = titleAttr.Title, SearchTags = launchAttr.SearchTags }
            ).ToList();

            return PartialView("MatchingAppsResults", model);
        }

        private int StringMatch(string subject, string find)
        {
            // The more space delimited parts of the 'find' string in the 'subject' string, the higher the match weight.
            // The earlier the 'find' string appears in the 'subject' string, the higher the match weight.
            int matchWeight = 0;
            foreach (var part in find.Split(' '))
            {
                var matchIndex = subject.IndexOf(part, StringComparison.InvariantCultureIgnoreCase);
                if (matchIndex != -1) matchWeight += part.Length * (subject.Length - matchIndex);
            }
            return matchWeight;
        }

        //public ActionResult AutoCompleteTaskType(string term)
        //{
        //    var items = new[] {"Apple", "Pear", "Banana", "Pineapple", "Peach"};

        //    var filteredItems = items.Where(
        //        item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
        //        );
        //    return Json(filteredItems, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult SecondaryTasks(string host)
        //{
        //    return View(db.TaskTypes.Where(t => t.Host == host).ToList());
        //}

        public ActionResult MyApps()
        {
            return View(db.TaskTypes.Where(t => t.Host == "app").ToList());
        }

        public ActionResult LaunchableComponents()
        {
            var model = (
                from assy in AppDomain.CurrentDomain.GetAssemblies()
                from type in assy.GetTypes()
                let titleAttr = type.GetCustomAttribute<ComponentTitleAttribute>()
                let launchAttr = type.GetCustomAttribute<LaunchableComponentAttribute>()
                where Attribute.IsDefined(type, typeof(LaunchableComponentAttribute))
                orderby titleAttr.Title
                select
                new TaskType() {Id = 0, RootComponentName = type.Name, Name = titleAttr.Title, SearchTags = launchAttr.SearchTags}
            ).ToList();

            return View(model);
        }

        public ActionResult AddToMyApps(string componentName)
        {
            var componentType = _tm.GetType(componentName);

            var titleAttr = componentType.GetCustomAttribute<ComponentTitleAttribute>();
            var launchAttr = componentType.GetCustomAttribute<LaunchableComponentAttribute>();
            var model = new TaskType()
            {
                Id = 0,
                Host =  "app",
                RootComponentName = componentName,
                Name = titleAttr.Title,
                SearchTags = launchAttr.SearchTags
            };

            var component = _tm.CreateInstanceOfTypeName(componentName);

            model.TaskInputs = (
                from pi in componentType.GetProperties()
                from attr in pi.GetCustomAttributes(typeof(LaunchInputAttribute), false)
                select new TaskInput() {Id = 0, Name = pi.Name, Value = pi.GetValue(component)?.ToString() }
            ).ToList();

            return View(model);
        }

        // POST: TaskType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToMyApps([Bind(Include = "Id,Host,Name,RootComponentName,SearchTags,TaskInputs")] TaskType taskType)
        {
            if (ModelState.IsValid)
            {
                db.TaskTypes.Add(taskType);
                db.SaveChanges();
                return RedirectToAction("MyApps");
            }
            return View(taskType);
        }

    }
}
