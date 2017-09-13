using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;

namespace WebUXv2.Controllers
{
    public class EntityContextController : Controller
    {
        // GET: EntityContext
        public ActionResult Contexts()
        {
            var ecm = SingletonService.Instance.EntityContextManager;
            var model = ecm.GetContexts();
            return View("Contexts", model);
        }

        public ActionResult DirectRelationships(string key)
        {
            var ecm = SingletonService.Instance.EntityContextManager;
            var model = ecm.GetDirectRelationships(ecm.GetContext(key));

            ViewBag.Key = key;
            return View(model);
        }

        public ActionResult RecentCustomerList(string selectAction, string selectController)
        {
            var ecm = SingletonService.Instance.EntityContextManager;
            var recentCustomerContexts = ecm.GetContexts().Where(x => x.Name == "customer" && !String.IsNullOrEmpty(x.Description)).OrderByDescending(x => x.WhenSet).ToList();

            ViewBag.SelectAction = selectAction;
            ViewBag.SelectController = selectController;
            return PartialView(recentCustomerContexts);
        }

        public ActionResult RecentContextList(string caption, string contextName, string selectAction, string selectController)
        {
            var ecm = SingletonService.Instance.EntityContextManager;
            var recentContexts = ecm.GetContexts().Where(x => x.Name == contextName && !String.IsNullOrEmpty(x.Description)).OrderByDescending(x => x.WhenSet).ToList();

            ViewBag.Caption = caption;
            ViewBag.ContextName = contextName;
            ViewBag.SelectAction = selectAction;
            ViewBag.SelectController = selectController;
            return PartialView(recentContexts);
        }

        public ActionResult DeleteAllContexts()
        {
            SingletonService.Instance.EntityContextManager.Clear();
            return Contexts();
        }

    }
}