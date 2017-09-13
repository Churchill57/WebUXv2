using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using WebGrease.Css.Extensions;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.LogicalUnits;
using WebUXv2.LogicalUnits.Customer;
using WebUXv2.LogicalUnits.Policy;
using WebUXv2.LogicalUnits.PropertyAddress;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Customer;
using WebUXv2.UserExperiences.TaskManager;

namespace WebUXv2.Controllers
{
    //TODO: Extract full interface. Eventually!
    public class TaskManagerController : Controller, ITaskManagerController
    {

        private TaskDbContext _db;
        private ITaskManager _taskMan;
        private IEntityContextManager _ctxMan;

        //public TaskManagerController() : this(new TaskDbContext(), new TaskManager(), Singleton.Instance.EntityContextManager) {}

        public TaskManagerController()
        {
            var taskDbContext = new TaskDbContext();
            InitializeTaskManagerController(taskDbContext, new TaskManager(taskDbContext), SingletonService.Instance.EntityContextManager);
        }


        public TaskManagerController(TaskDbContext taskDbContext, ITaskManager taskMan, IEntityContextManager ctxMan)
        {
            InitializeTaskManagerController(taskDbContext, taskMan, ctxMan);
        }

        private void InitializeTaskManagerController(TaskDbContext taskDbContext, ITaskManager taskMan, IEntityContextManager ctxMan)
        {
            _db = taskDbContext;
            _taskMan = taskMan;
            _ctxMan = ctxMan;
        }

        public void SetControllerContext(ControllerContext controllerContext)
        {
            this.ControllerContext = controllerContext;
        }

        public ActionResult DeleteAllTasks()
        {
            _db.UxTasks.RemoveRange(_db.UxTasks);
            _db.LuTasks.RemoveRange(_db.LuTasks);
            _db.SaveChanges();

            return Redirect(Request.UrlReferrer?.AbsolutePath);
        }

        public ActionResult Back(int uxTaskId)
        {
            var ux = _taskMan.GetUserExperience(uxTaskId);
            if (ux == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var uxEvent = new BackEventArgs(ux);

            return RedirectAfterComponentEvent(uxEvent);
        }

        [PreserveQueryString]
        public ActionResult RunTaskType(int taskTypeId, int? returnTaskId = null, bool forceContextSearch = false, string userMessage = null)
        {
            if (userMessage != null) SingletonService.Instance.UserMessage = userMessage;

            string url = Request?.UrlReferrer?.AbsoluteUri ?? "";

            //Check specified task type exists.
            var taskType = _db.TaskTypes.Find(taskTypeId);
            if (taskType == null) return Redirect(url);

            if (_taskMan.ComponentIsSingleton(taskType.RootComponentName))
            {
                var redirectToSingletonComponent = RedirectToSingletonComponent(taskType.RootComponentName, returnTaskId, returnTaskId, forceContextSearch);
                if (redirectToSingletonComponent != null) return redirectToSingletonComponent;
            }

            var rootComponentName = taskType.RootComponentName;
            var userName = GetUserName();
            var luLauncher = new LuLauncher(rootComponentName, userName, url)
            {
                Status = LogicalUnitStatusEnum.Started,
                ReturnTaskId = returnTaskId,
                //Title = taskType.Name,
                TaskMan = _taskMan
            };

            var launcherTask = new LuTask(userName)
            {
                Name = luLauncher.GetType().Name,
                ClientRef = rootComponentName,
                State = _taskMan.ObjectToJson(luLauncher.State),
                Status = LogicalUnitStatusEnum.Started
            };
            _db.LuTasks.Add(launcherTask);
            _db.SaveChanges();

            luLauncher.TaskId = launcherTask.Id;

            luLauncher.InitialiseRootComponentAndInputs(taskType);

            var result = ResumeTask(launcherTask.Id, forceContextSearch);

            if (result == null) return Redirect(url);

            return result;
        }

        [PreserveQueryString]
        public ActionResult RedirectToSingletonComponent(string componentName, int? returnTaskId = null, int? parentTaskId = null, bool forceContextSearch = false, string searchText = null, string returnTaskRef = null)
        {
            // TODO: Use searchText to initialise any context inputs.
            //if (_taskMan.ComponentIsSingleton(componentName))
            //{
                var luTask = _db.LuTasks.FirstOrDefault(t => t.Name == componentName);
                if (luTask != null)
                {
                    if (luTask.ParentLuTaskId.HasValue)
                    {
                        // Singleton reuse means it has a new parent.
                        // Iterate through parents to first LuLauncher.
                        // TODO : Iterate through parents to first LuLauncher - currently assuming one level above!
                        var luParentTask = _db.LuTasks.Find(luTask.ParentLuTaskId);
                        luParentTask.ParentLuTaskId = parentTaskId;
                        _db.Entry(luParentTask).State = EntityState.Modified;
                        _db.SaveChanges();
                        //if (returnTaskId != null && luParentTask.Name == typeof(LuLauncher).Name)
                        if (luParentTask.Name == typeof(LuLauncher).Name)
                        {
                            var luLauncher = (LuLauncher)_taskMan.GetLogicalUnit(luParentTask);
                            luLauncher.ReturnTaskId = returnTaskId;
                            luLauncher.ReturnTaskRef = returnTaskRef;
                            luLauncher.Save();
                        }
                    }

                    //if (parentTaskId.HasValue)
                    //{
                    //    // Singleton reuse means it has a new parent.
                    //    // Iterate through parents to first LuLauncher.
                    //    // TODO : Iterate through parents to first LuLauncher - currently assuming one level above!
                    //    var luParentTask = _db.LuTasks.Find(luTask.ParentLuTaskId);
                    //    if (luParentTask.ParentLuTaskId != parentTaskId)
                    //    {
                    //        luParentTask.ParentLuTaskId = parentTaskId;
                    //        _db.Entry(luParentTask).State = EntityState.Modified;
                    //        _db.SaveChanges();
                    //    }
                    //}
                    var lu = _taskMan.GetLogicalUnit(luTask);

                    var singletonComponent = lu as ISingletonComponent;
                    singletonComponent?.InitialiseState();

                    if (forceContextSearch)
                    {
                        var contextFinder = lu as IContextFinder;
                        contextFinder?.ForceNextContextSearch();
                    }

                    lu?.Save();

                    return ResumeTask(luTask.Id, forceContextSearch, searchText);
                }
                if (_taskMan.ComponentIsSingleton(componentName))
                {
                    return ResumeNewComponent(componentName,returnTaskId,parentTaskId,forceContextSearch,searchText,returnTaskRef);
                }
                var uxTask = _db.UxTasks.FirstOrDefault(t => t.Name == componentName);
                if (uxTask != null) return ResumeTask(uxTask.Id, forceContextSearch, searchText);
            //}
            return null;
        }

        [PreserveQueryString]
        public ActionResult RunTask(string componentName, int? returnTaskId = null, int? parentTaskId = null, bool forceContextSearch = false, string searchText = null, string returnTaskRef = null, string userMessage = null)
        {
            if (userMessage != null) SingletonService.Instance.UserMessage = userMessage;

            if (_taskMan.ComponentIsSingleton(componentName))
            {
                return RedirectToSingletonComponent(componentName, returnTaskId, parentTaskId, forceContextSearch, searchText, returnTaskRef);
                //var redirectToSingletonComponent = RedirectToSingletonComponent(componentName, returnTaskId, parentTaskId, forceContextSearch, searchText, returnTaskRef);
                //if (redirectToSingletonComponent != null) return redirectToSingletonComponent;
            }

            var resumeAction = ResumeNewComponent(componentName, returnTaskId, parentTaskId, forceContextSearch, searchText, returnTaskRef);

            if (resumeAction != null) return resumeAction;

            return Redirect(Request.UrlReferrer.AbsoluteUri);

        }

        private ActionResult ResumeNewComponent(string componentName, int? returnTaskId, int? parentTaskId, bool forceContextSearch, string searchText, string returnTaskRef)
        {
            Component component = _taskMan.CreateInstanceOfTypeName(componentName) as Component;
            if (component == null)
            {
                return HttpNotFound($"Component {componentName} not found.");
            }

            string url = Request?.UrlReferrer?.AbsoluteUri ?? "";
            var userName = GetUserName();
            var luLauncher = new LuLauncher(componentName, userName, url)
            {
                Status = LogicalUnitStatusEnum.Started,
                ReturnTaskId = returnTaskId,
                ReturnTaskRef = returnTaskRef,
                //Title = _taskMan.GetComponentTitle(component),
                TaskMan = _taskMan
            };

            var launcherTask = new LuTask(userName)
            {
                Name = luLauncher.GetType().Name,
                ClientRef = componentName,
                State = _taskMan.ObjectToJson(luLauncher.State),
                Status = LogicalUnitStatusEnum.Started
            };

            if (parentTaskId.HasValue)
            {
                launcherTask.ParentLuTaskId = parentTaskId.Value;
                //launcherTask.ParentLuTask = _taskMan.GetLuTask(parentTaskId.Value);
            }
            _db.LuTasks.Add(launcherTask);
            _db.SaveChanges();

            return ResumeTask(launcherTask.Id, forceContextSearch, searchText);
        }

        private string GetUserName()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userName = identity.FindFirstValue(ClaimTypes.GivenName) ?? identity.GetUserName();
            if (String.IsNullOrEmpty(userName)) userName = "anonymous";
            return userName;
        }

        [PreserveQueryString]
        public ActionResult ResumeTask(int componentTaskId, bool forceContextSearch = false, string searchText = null)
        {
            Component nextComp = _taskMan.GetUserExperience(componentTaskId);

            if (nextComp == null)
            {
                nextComp = _taskMan.GetLogicalUnit(componentTaskId);
                if (nextComp != null)
                {
                    var lu = nextComp as LogicalUnit;

                    if (_taskMan.ComponentIsSingleton(lu))
                    {
                        ((ISingletonComponent)lu).InitialiseState();
                    }



                    LuLauncher launcher = lu as LuLauncher;
                    if (launcher != null)
                    {
                        if (launcher.ResumeTaskId != null) return ResumeTask(launcher.ResumeTaskId.Value);
                        launcher.ReturnUrl = Request?.UrlReferrer?.AbsoluteUri;
                        launcher.Save();
                    }

                    if (_taskMan.UserHasAccessToComponent(User, lu))
                    {
                        //var routeParams = _taskMan.GetComponentInputParams(lu);

                        var routeParams = new RouteValueDictionary();
                        //var routeParams = _taskMan.GetComponentInputParams(ux); TODO: Remove _taskMan.GetComponentInputParams ??
                        // Retain any querystring parameters going forward for the next Url, except uxTaskId
                        foreach (string key in Request.QueryString.Keys)
                        {
                            if (string.Equals(key, "uxTaskId", StringComparison.InvariantCultureIgnoreCase)) continue;
                            if (routeParams.ContainsKey(key))
                            {
                                //Querystring values take precedence
                                routeParams[key] = Request.QueryString[key];
                            }
                            else
                            {
                                routeParams.Add(key, Request.QueryString[key]);
                            }
                        }
                        //routeParams["uxTaskId"] = ux.TaskId;
                        //routeParams.Add("uxTaskId", ux.TaskId.ToString());

                        routeParams.Add("luTaskId", lu.TaskId.ToString());

                        var redirectToResolveContextInputs = RedirectToResolveComponentInputs(lu, routeParams, forceContextSearch, searchText);
                        if (redirectToResolveContextInputs != null) return redirectToResolveContextInputs;

                        nextComp = lu.GetNextComponent();
                        lu.Save();
                    }
                    else
                    {
                        var routeParams = new RouteValueDictionary();
                        foreach (string key in Request.QueryString.Keys)
                        {
                            routeParams.Add(key, Request.QueryString[key]);
                        }
                        routeParams["luTaskId"] = lu.TaskId;
                        nextComp = lu.GetNextComponent();
                        lu.Save();
                        nextComp?.Save();
                        routeParams.Add("nextTaskId", nextComp?.TaskId);
                        routeParams.Add("returnUrl", Request.UrlReferrer?.LocalPath);
                        routeParams.Add("loginReturnUrl", Request.Url?.PathAndQuery);

                        return RedirectToAction("AccessDenied", "TaskManager", routeParams);
                    }
                }
            }

            nextComp?.Save();

            if (nextComp is LogicalUnit)
            {
                var lu = nextComp as LogicalUnit;
                return ResumeTask(lu.TaskId, forceContextSearch, searchText);
            }

            if (nextComp is UserExperience)
            {
                var ux = nextComp as UserExperience;
                return RedirectToUserExperience(ux, forceContextSearch, searchText);
            }


            return null;
        }

        // TODO: This method really needs refactoring.
        public ActionResult RedirectAfterComponentEvent(ComponentEventArgs eventArgs, bool forceContextSearch = false)
        {
            Component uxSource = eventArgs.Component;
            LogicalUnit luParent = null;
            Component nextComp = null;
            LuTask luParentTask = null;

            var uxTask = _db.UxTasks.FirstOrDefault(x => x.Id == uxSource.TaskId);
            if (uxTask != null)
            {
                luParent = _taskMan.GetLogicalUnit(uxTask.ParentLuTaskId);
                luParent.HandleComponentEvent(eventArgs);
                luParentTask = uxTask.ParentLuTask;
            }
            else
            {
                // TODO: YAGNI ALERT -  Not sure if a Logical Unit will ever raise an event.
                var luTask = _db.LuTasks.FirstOrDefault(x => x.Id == uxSource.TaskId);
                if (luTask != null)
                {
                    luParent = _taskMan.GetLogicalUnit(luTask.ParentLuTaskId.Value);
                    luParent.HandleComponentEvent(eventArgs);
                    luParentTask = luTask.ParentLuTask;
                }
            }

            if (luParent == null) return HttpNotFound($"Component with id {uxSource.TaskId} not found.");

            do
            {
                if (eventArgs is BackEventArgs)
                {
                    nextComp = luParent.GetPrevComponent();
                    nextComp?.Save();
                    luParent.Status = LogicalUnitStatusEnum.Started;
                }
                else
                {
                    nextComp = luParent.GetNextComponent();
                    nextComp?.Save();
                    if (nextComp == null) luParent.Status = LogicalUnitStatusEnum.Completed;
                }


                luParentTask.State = _taskMan.ObjectToJson(luParent.State);
                luParentTask.Status = luParent.Status;
                _db.Entry(luParentTask).State = EntityState.Modified;
                _db.SaveChanges();

                if (nextComp is LogicalUnit)
                {
                    luParent = nextComp as LogicalUnit;
                    luParentTask = _db.LuTasks.Find(nextComp.TaskId);
                }


            } while (nextComp is LogicalUnit);

            LogicalUnit luParentParent;
            LuTask luParentParentTask;

            while (!(luParent is LuLauncher))
            {
                if (nextComp is UserExperience)
                {
                    var ux = nextComp as UserExperience;
                    return RedirectToUserExperience(ux, forceContextSearch);
                }

                while (luParentTask.ParentLuTaskId.HasValue)
                {

                    if (nextComp is UserExperience)
                    {
                        var ux = nextComp as UserExperience;
                        return RedirectToUserExperience(ux, forceContextSearch);
                    }

                    luParentParent = _taskMan.GetLogicalUnit(luParentTask.ParentLuTaskId.Value);

                    // TODO: Fix horrible kludge which allows cancelling from a secondary app returning to the primary app where it was launched
                    // Consider using the null entity pattern to return UxLast or UxFirst from Lu.GetNext/PrevComponent.
                    if (eventArgs is BackEventArgs)
                    {
                        if (luParent is LuLauncher && luParentParent is LuLauncher)
                        {
                            if (((LuLauncher) luParent).ReturnTaskId == ((LuLauncher) luParentParent).ComponentTaskId)
                            {
                                break;
                            }
                        }
                    }

                    luParentParent.HandleComponentEvent(eventArgs);
                    luParentParentTask = _db.LuTasks.Find(luParentParent.TaskId);

                    if (eventArgs is BackEventArgs)
                    {
                        nextComp = luParentParent.GetPrevComponent();
                        nextComp?.Save();
                        luParentParent.Status = LogicalUnitStatusEnum.Started;
                    }
                    else
                    {
                        nextComp = luParentParent.GetNextComponent();
                        nextComp?.Save();
                        if (nextComp == null) luParentParent.Status = LogicalUnitStatusEnum.Completed;
                    }

                    luParentParentTask.State = _taskMan.ObjectToJson(luParentParent.State);
                    luParentParentTask.Status = luParentParent.Status;
                    luParentParentTask.Title = luParentParent.Title();
                    _db.Entry(luParentParentTask).State = EntityState.Modified;
                    _db.SaveChanges();

                    luParent = luParentParent;
                    luParentTask = luParentParentTask;
                }

            }

            var luLauncher = luParent as LuLauncher;
            return luLauncher.ReturnTaskId.HasValue ? ResumeTask(luLauncher.ReturnTaskId.Value, forceContextSearch) : Redirect(luLauncher.ReturnUrl);
            // TODO: cater for back button processing. Perhaps we need a ResumeBackTask method?
        }

        public ActionResult RedirectAfterUserExperienceCompleted(UserExperience uxCompleted, bool forceContextSearch = false)
        {
            LogicalUnit luParent = null;
            Component nextComp = null;

            var uxTask = _db.UxTasks.FirstOrDefault(x => x.Id == uxCompleted.TaskId);
            if (uxTask != null)
            {
                luParent = _taskMan.GetLogicalUnit(uxTask.ParentLuTaskId);
                luParent.ComponentCompleted(uxCompleted);
                nextComp = luParent.GetNextComponent();
                nextComp?.Save();

                var luParentTask = uxTask.ParentLuTask;
                luParentTask.State = _taskMan.ObjectToJson(luParent.State);
                if (nextComp == null) luParentTask.Status = LogicalUnitStatusEnum.Completed;
                _db.Entry(luParentTask).State = EntityState.Modified;
                _db.SaveChanges();

                LogicalUnit luParentParent;
                LuTask luParentParentTask;

                while (!(luParent is LuLauncher))
                //while (luParentTask.ParentLuTaskId.HasValue)
                {

                    while (nextComp == null && luParentTask.ParentLuTaskId.HasValue)
                    {
                        var routeParams = new RouteValueDictionary();
                        luParentParent = _taskMan.GetLogicalUnit(luParentTask.ParentLuTaskId.Value);

                        routeParams = new RouteValueDictionary();
                        // Retain any querystring parameters going forward for the next Url, except uxTaskId
                        foreach (string key in Request.QueryString.Keys)
                        {
                            if (string.Equals(key, "uxTaskId", StringComparison.InvariantCultureIgnoreCase)) continue;
                            if (routeParams.ContainsKey(key))
                            {
                                //Querystring values take precedence
                                routeParams[key] = Request.QueryString[key];
                            }
                            else
                            {
                                routeParams.Add(key, Request.QueryString[key]);
                            }
                        }

                        var redirectToResolveContextInputs = RedirectToResolveComponentInputs(luParent, routeParams, forceContextSearch);
                        if (redirectToResolveContextInputs != null) return redirectToResolveContextInputs;

                        luParentParent.ComponentCompleted(luParent);
                        AssignContextInputProperties(luParentParent, routeParams, forceContextSearch);

                        redirectToResolveContextInputs = RedirectToResolveComponentInputs(luParentParent, routeParams, forceContextSearch);
                        if (redirectToResolveContextInputs != null)
                        {
                            return redirectToResolveContextInputs;
                        }

                        nextComp = luParentParent.GetNextComponent();
                        nextComp?.Save();

                        luParentParentTask = _db.LuTasks.Find(luParentParent.TaskId);
                        luParentParentTask.State = _taskMan.ObjectToJson(luParentParent.State);
                        luParentParentTask.Title = luParentParent.Title();
                        if (nextComp == null) luParentParentTask.Status = LogicalUnitStatusEnum.Completed;
                        _db.Entry(luParentParentTask).State = EntityState.Modified;
                        _db.SaveChanges();

                        luParent = luParentParent;
                        luParentTask = luParentParentTask;
                    }

                    if (nextComp is UserExperience)
                    {
                        var ux = nextComp as UserExperience;
                        return RedirectToUserExperience(ux, forceContextSearch);
                    }

                    if (nextComp is LogicalUnit)
                    {
                        var lu = nextComp as LogicalUnit;

                        var resumeTask = ResumeTask(lu.TaskId, forceContextSearch);
                        if (resumeTask != null) return resumeTask;

                        // A spawned LuLauncher may have just completed, so resume lu which spawned it.
                        return ResumeTask(luParent.TaskId, forceContextSearch);
                    }

                }

                var luLauncher = luParent as LuLauncher;
                return luLauncher.ReturnTaskId.HasValue ? ResumeTask(luLauncher.ReturnTaskId.Value, forceContextSearch) : Redirect(luLauncher.ReturnUrl);
            }

            return null;
        }

        private bool AssignContextInputProperties(Component comp, RouteValueDictionary routeValues, bool forceContextSearch = false, string searchText = null)
        {
            var componentInputProperties = _taskMan.ComponentInputProperties(comp);
            var parsedSearchText = _taskMan.ParseSearchText(searchText);

            bool atLeastOneInputWasResolved = false;
            bool atLeastOneInputWasNotResolved = false;
            foreach (var componentInputProperty in componentInputProperties)
            {
                // Look for route param which matches component property name.
                if (routeValues.ContainsKey(componentInputProperty.Key))
                {
                    //If there is a corresponding route value, use it to assign property value.
                    var propertyInfo = comp.GetType().GetProperty(componentInputProperty.Key);
                    Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    // TODO: Major dilemma. Should a context property be overwritten if its value is to change, or only set if currently null?
                    if (propertyInfo.GetValue(comp) == null)
                    {
                        if (propertyInfo.PropertyType == typeof(EntityContext))
                        {
                            var entityContextRouteValue = routeValues[componentInputProperty.Key] as EntityContext;
                            if (entityContextRouteValue != null)
                            {
                                propertyInfo.SetValue(comp, Convert.ChangeType(routeValues[componentInputProperty.Key], typeof(EntityContext)), null);
                            }
                            else
                            {
                                entityContextRouteValue = _taskMan.JsonToObject<EntityContext>(routeValues[componentInputProperty.Key].ToString());
                                propertyInfo.SetValue(comp, Convert.ChangeType(entityContextRouteValue, typeof(EntityContext)), null);
                            }

                        }
                        else
                        {
                            propertyInfo.SetValue(comp, Convert.ChangeType(routeValues[componentInputProperty.Key], propertyType), null);
                        }

                    }

                    //string currentValue = (propertyInfo.GetValue(comp) ?? "").ToString();
                    //string newValue = (string)routeValues[componentInputProperty.Key];
                    //if (!String.Equals(currentValue, newValue, StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    propertyInfo.SetValue(comp, Convert.ChangeType(routeValues[componentInputProperty.Key], propertyType), null);
                    //}

                    // Route poaram used so no need to proliferate further. 
                    routeValues.Remove(componentInputProperty.Key);
                    atLeastOneInputWasResolved = true;
                }
                // Look for context value which matches component property name.
                else if (componentInputProperty.Value != null && parsedSearchText.ContainsKey(componentInputProperty.Value))
                {
                    int contextId = (int)parsedSearchText[componentInputProperty.Value];
                    //If there is a corresponding context value, use it to assign property value.
                    var propertyInfo = comp.GetType().GetProperty(componentInputProperty.Key);
                    Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                    if (String.Equals(componentInputProperty.Value, "customer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var luFindCustomerContext = new LuFindCustomerContext();
                        // TODO: Replace luFindCustomerContext.CustomerExists with a IContextFinder interface and EntityExists method. 
                        if (!luFindCustomerContext.CustomerExists(contextId))
                        {
                            SingletonService.Instance.UserMessage = $"Customer {contextId} does not exist";
                            propertyInfo.SetValue(comp, null, null);
                            atLeastOneInputWasNotResolved = true;
                            continue;
                        }
                    }

                    if (String.Equals(componentInputProperty.Value, "policy", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var luFindPAPolicyContext = new LuFindPAPolicyContext();
                        // TODO: Replace luFindPAPolicyContext.PolicyExists with a IContextFinder interface and EntityExists method. 
                        if (!luFindPAPolicyContext.PolicyExists(contextId))
                        {
                            SingletonService.Instance.UserMessage = $"Policy {contextId} does not exist";
                            propertyInfo.SetValue(comp, null, null);
                            atLeastOneInputWasNotResolved = true;
                            continue;
                        }
                    }

                    if (String.Equals(componentInputProperty.Value, "address", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var luFindAddressContext = new LuFindAddressContext();
                        // TODO: Replace luFindAddressContext.AddressExists with a IContextFinder interface and EntityExists method. 
                        if (!luFindAddressContext.AddressExists(contextId))
                        {
                            SingletonService.Instance.UserMessage = $"Address {contextId} does not exist";
                            propertyInfo.SetValue(comp, null, null);
                            atLeastOneInputWasNotResolved = true;
                            continue;
                        }
                    }

                    propertyInfo.SetValue(comp, Convert.ChangeType(parsedSearchText[componentInputProperty.Value], propertyType), null);

                    //_ctxMan.SetContext((int)parsedSearchText[componentInputProperty.Value], componentInputProperty.Value, null);

                    atLeastOneInputWasResolved = true;
                }
                else
                {
                    // Try to resolve any context input attributed properties on the component by examining the current entity context situation.
                    var contextName = componentInputProperty.Value;
                    if (!String.IsNullOrEmpty(contextName))
                    {
                        //var resolvedContext = _ctxMan.ResolveContext(contextName);
                        var resolvedContext = _ctxMan.GetCurrentContext;
                        if (resolvedContext?.Name != contextName) resolvedContext = null;
                        if (resolvedContext != null && !forceContextSearch)
                        {
                            var propertyInfo = comp.GetType().GetProperty(componentInputProperty.Key);
                            if (propertyInfo.GetValue(comp) == null)
                            {
                                if (propertyInfo.PropertyType == typeof(EntityContext))
                                {
                                    propertyInfo.SetValue(comp, Convert.ChangeType(resolvedContext, typeof(EntityContext)), null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(comp, Convert.ChangeType(resolvedContext.Id, typeof(int)), null);
                                }

                            }

                            //_ctxMan.SetContext(resolvedContext.Id, contextName, null);

                            atLeastOneInputWasResolved = true;
                        }
                    }
                }
            }
            if (atLeastOneInputWasResolved) comp.Save();
            return !atLeastOneInputWasNotResolved;
        }

        public ActionResult RedirectToResolveComponentInputs(Component comp, RouteValueDictionary routeValues, bool forceContextSearch = false, string searchText = null)
        {
            if (!AssignContextInputProperties(comp, routeValues, forceContextSearch, searchText))
            {
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            // If no input attributed properties on the component are null, then happy days (no redirection is necessary).
            //var nullAttributedProperties = ComponentInputProperties(comp);
            var nullAttributedProperties = _taskMan.NullComponentInputProperties(comp);
            if (!nullAttributedProperties.Any()) return null;

            bool atLeastOneInputWasResolved = false;
            foreach (var nullProperty in nullAttributedProperties)
            {
                if (routeValues.ContainsKey(nullProperty.Key))
                {
                    //If there is a corresponding route value, use it to assign property value.
                    var propertyInfo = comp.GetType().GetProperty(nullProperty.Key);
                    Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    propertyInfo.SetValue(comp, Convert.ChangeType(routeValues[nullProperty.Key], propertyType), null);
                    routeValues.Remove(nullProperty.Key); // Route poaram used so no need to proliferate further. 
                    atLeastOneInputWasResolved = true;
                }
                else
                {
                    // Try to resolve any context input attributed properties on the component by examining the current entity context situation.
                    var contextName = nullProperty.Value;
                    if (!String.IsNullOrEmpty(contextName))
                    {
                        //var resolvedContext = _ctxMan.ResolveContext(contextName);
                        var resolvedContext = _ctxMan.GetCurrentContext;
                        if (resolvedContext?.Name != contextName) resolvedContext = null;
                        if (resolvedContext != null && !forceContextSearch)
                        {
                            //routeValues[nullProperty.Key] = resolvedContext.Id;
                            var propertyInfo = comp.GetType().GetProperty(nullProperty.Key);
                            propertyInfo.SetValue(comp, Convert.ChangeType(resolvedContext.Id, typeof(int)), null);
                            _ctxMan.SetContext(resolvedContext.Id, contextName, null);
                            atLeastOneInputWasResolved = true;
                        }
                        else
                        {
                            // TODO: determine which lu/ux to display to user to find context.
                            // This can be made configurable between context name and a lu/ux name.
                            if (String.Equals(contextName, "customer", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // TODO: Consider including parentlutaskid property on Component class to avoid these extra db access
                                int returnTaskId = comp.TaskId;
                                if (comp is UserExperience)
                                {
                                    returnTaskId = _taskMan.GetUxTask(comp.TaskId).ParentLuTaskId;
                                }

                                var customerContextAction = RunTask(typeof(LuFindCustomerContext).Name, comp.TaskId, returnTaskId, forceContextSearch, searchText);
                                // Customer context may have been established without redirecting to a search/selection componentr
                                if (customerContextAction != null)
                                {
                                    LuFindCustomerContext luFindCustomerContext =
                                        _taskMan.GetSingletonComponent<LuFindCustomerContext>();
                                    luFindCustomerContext.ShowSearchBackButton = false;
                                    luFindCustomerContext.Save();
                                    return customerContextAction;
                                }
                            }

                            if (String.Equals(contextName, "address", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // TODO: Consider including parentlutaskid property on Component class to avoid these extra db access
                                int returnTaskId = comp.TaskId;
                                if (comp is UserExperience)
                                {
                                    returnTaskId = _taskMan.GetUxTask(comp.TaskId).ParentLuTaskId;
                                }

                                var addressContextAction = RunTask(typeof(LuFindAddressContext).Name, comp.TaskId, returnTaskId, forceContextSearch, searchText);
                                // Address context may have been established without redirecting to a search/selection componentr
                                if (addressContextAction != null)
                                {
                                    LuFindAddressContext luFindAddressContext = _taskMan.GetSingletonComponent<LuFindAddressContext>();
                                    luFindAddressContext.ShowBackButton = false;
                                    luFindAddressContext.Save();
                                    return addressContextAction;
                                }
                            }

                            if (String.Equals(contextName, "policy", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // TODO: Consider including parentlutaskid property on Component class to avoid these extra db access
                                int returnTaskId = comp.TaskId;
                                if (comp is UserExperience)
                                {
                                    returnTaskId = _taskMan.GetUxTask(comp.TaskId).ParentLuTaskId;
                                }

                                var policyContextAction = RunTask(typeof(LuFindPAPolicyContext).Name, comp.TaskId, returnTaskId, forceContextSearch, searchText);
                                // Policy context may have been established without redirecting to a search/selection componentr
                                if (policyContextAction != null)
                                {
                                    LuFindPAPolicyContext luFindCustomerContext = _taskMan.GetSingletonComponent<LuFindPAPolicyContext>();
                                    luFindCustomerContext.ShowSearchBackButton = false;
                                    luFindCustomerContext.Save();
                                    return policyContextAction;
                                }
                            }
                        }
                    }
                }
            }
            if (atLeastOneInputWasResolved) comp.Save();

            // In case some context was established without resorting to a user experience being displayed to the user.
            AssignContextInputProperties(comp, routeValues, false, searchText);

            return null;
        }

        [PreserveQueryString]
        public ActionResult RedirectToUserExperience(UserExperience ux, bool forceContextSearch = false, string searchText = null)
        {
            var routeParams = new RouteValueDictionary();
            //var routeParams = _taskMan.GetComponentInputParams(ux); TODO: Remove _taskMan.GetComponentInputParams ??
            // Retain any querystring parameters going forward for the next Url, except uxTaskId
            foreach (string key in Request.QueryString.Keys)
            {
                if (string.Equals(key, "uxTaskId", StringComparison.InvariantCultureIgnoreCase)) continue;
                if (routeParams.ContainsKey(key))
                {
                    //Querystring values take precedence
                    routeParams[key] = Request.QueryString[key];
                }
                else
                {
                    routeParams.Add(key, Request.QueryString[key]);
                }
            }
            //routeParams["uxTaskId"] = ux.TaskId;
            routeParams.Add("uxTaskId", ux.TaskId.ToString());

            //AssignContextInputProperties(ux, routeParams, forceContextSearch);

            ActionResult uxActionResult = RedirectToResolveComponentInputs(ux, routeParams, forceContextSearch, searchText);
            if (uxActionResult != null) return uxActionResult;

            if (_taskMan.UserHasAccessToComponent(User, ux))
            {
                uxActionResult = RedirectToAction(ux.GetType().GetCustomAttribute<PrimaryActionControllerAttribute>().ActionName, ux.GetType().GetCustomAttribute<PrimaryActionControllerAttribute>().ControllerName, routeParams);
            }
            else
            {
                //// Retain any querystring parameters going forward for the next Url
                //foreach (string key in Request.QueryString.Keys)
                //{
                //    if (routeParams.ContainsKey(key))
                //    {
                //        //Querystring values take precedence
                //        routeParams[key] = Request.QueryString[key];
                //    }
                //    else
                //    {
                //        routeParams.Add(key, Request.QueryString[key]);
                //    }
                //}
                //routeParams["uxTaskId"] = ux.TaskId;
                var urlHelper = new UrlHelper(Request.RequestContext);
                var nextUrl = urlHelper.Action(ux.GetType().GetCustomAttribute<PrimaryActionControllerAttribute>().ActionName, ux.GetType().GetCustomAttribute<PrimaryActionControllerAttribute>().ControllerName, routeParams);
                routeParams.Add("nextUrl", nextUrl);

                string returnUrl = Request.UrlReferrer?.AbsoluteUri ?? "";
                routeParams.Add("returnUrl", returnUrl);

                uxActionResult = RedirectToAction("AccessDenied", "TaskManager", routeParams);
            }
            //return new HttpUnauthorizedResult(@"You are not authorised to access the User Experience {ux.Name}");
            return uxActionResult;
        }

        public ActionResult AccessDenied(string nextUrl, int? nextTaskId, string returnUrl, string loginReturnUrl)
        {
            ViewBag.NextUrl = nextUrl;
            ViewBag.NextTaskId = nextTaskId;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginReturnUrl = loginReturnUrl;
            return View();
        }

        public ActionResult ConfirmHandOff(string returnUrl, int resumeTaskId)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ResumeTaskId = resumeTaskId;
            return View();
        }

        public ActionResult HandOffTask(int uxTaskId)
        {
            return SuspendUxTask(uxTaskId);
        }

        public ActionResult Done(int uxTaskId)
        {
            var ux = _taskMan.GetUserExperience(uxTaskId);
            if (ux == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");
            return RedirectAfterUserExperienceCompleted(ux);
        }

        public ActionResult GetUserExperienceTaskBreadcrumbs()
        {
            int uxTaskId;
            //int.TryParse(Request.RequestContext.RouteData.Values["uxTaskId"]?.ToString(), out uxTaskId);
            int.TryParse(Request.QueryString["uxTaskId"], out uxTaskId);
            if (uxTaskId == 0) return new EmptyResult();
            var breadcrumbs = _taskMan.GetUserExperienceTaskBreadcrumbs(uxTaskId);
            return Content(String.Join(" > ",breadcrumbs.Reverse()));
        }
        //public ActionResult GetUltimateParentLogicalUnitTitle()
        //{
        //    int uxTaskId;
        //    //int.TryParse(Request.RequestContext.RouteData.Values["uxTaskId"]?.ToString(), out uxTaskId);
        //    int.TryParse(Request.QueryString["uxTaskId"], out uxTaskId);
        //    if (uxTaskId == 0) return new EmptyResult();
        //    var title = _taskMan.GetUltimateParentLogicalUnitTitle(uxTaskId);
        //    return Content(title);
        //}

        //public ActionResult GetUltimateParentLogicalUnitId()
        //{
        //    int uxTaskId;
        //    //int.TryParse(Request.RequestContext.RouteData.Values["uxTaskId"]?.ToString(), out uxTaskId);
        //    int.TryParse(Request.QueryString["uxTaskId"], out uxTaskId);
        //    if (uxTaskId == 0) return new EmptyResult();
        //    var id = _taskMan.GetUltimateParentLogicalUnitTask(uxTaskId).Id;
        //    return Content(id.ToString());
        //}

        public ActionResult GetExecutingUserExperienceTitle()
        {
            int uxTaskId;
            //int.TryParse(Request.RequestContext.RouteData.Values["uxTaskId"]?.ToString(), out uxTaskId);
            int.TryParse(Request.QueryString["uxTaskId"], out uxTaskId);
            if (uxTaskId == 0) return new EmptyResult();
            var title = _taskMan.GetUxTask(uxTaskId)?.Title;
            return Content(title);
        }

        public ActionResult SecondaryTasks(int uxTaskId)
        {
            var ux = _taskMan.GetUserExperience(uxTaskId);
            if (ux == null) return new EmptyResult();

            var luRunAndManageApps = _taskMan.GetSingletonComponent<LuRunAndManageApps>();
            if (luRunAndManageApps == null) return new EmptyResult();

            //var host = ux.GetType().Name;
            //var host = ux.Title();
            //var host = _taskMan.GetComponentTitle(ux);
            //if (string.IsNullOrEmpty(host)) host = ux.GetType().Name;

            var uxMy2ndApps = luRunAndManageApps.UxMy2ndApps;
            uxMy2ndApps.Host = ux.GetType().Name;
            uxMy2ndApps.Save();

            var model = uxMy2ndApps.GetApps();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.Host = uxMy2ndApps.Host;
            ViewBag.HideInstructions = uxMy2ndApps.HideInstructions;

            return PartialView("SecondaryTasks", model);
        }





        [Authorize]
        public ActionResult StartHere()
        {
            return RedirectToAction("RunTask", "TaskManager", new { componentName = typeof(LuRunAndManageApps).Name, Command = "SearchAndRunApps" }); // + "|"
        }

        public ActionResult SearchAndRunApps(int uxTaskId)
        {
            var uxSearchAndRunApps = _taskMan.GetUserExperience(uxTaskId) as UxSearchAndRunApps;
            if (uxSearchAndRunApps == null) return RunTask(typeof(LuRunAndManageApps).Name);

            var model = new SearchAndRunAppViewModel()
            {
                SearchText = uxSearchAndRunApps.SearchText,
                HideInstructions = uxSearchAndRunApps.HideInstructions,
                RunBestMatch = uxSearchAndRunApps.RunBestMatch
            };

            ViewBag.uxTaskId = uxTaskId;

            return View("SearchAndRunApps", model);
        }

        public ActionResult SearchAppsToRun(int uxTaskId, SearchAndRunAppViewModel criteria)
        {
            var uxSearchAndRunApps = _taskMan.GetUserExperience(uxTaskId) as UxSearchAndRunApps;
            if (uxSearchAndRunApps == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxSearchAndRunApps.SearchText = criteria.SearchText;
            uxSearchAndRunApps.RunBestMatch = criteria.RunBestMatch;
            uxSearchAndRunApps.HideInstructions = criteria.HideInstructions;
            uxSearchAndRunApps.Save();

            var model = uxSearchAndRunApps.SearchResults();

            ViewBag.SearchText = criteria.SearchText;

            return PartialView("MatchingAppsToRun", model);
        }

        public ActionResult SearchAndAddApp(int uxTaskId)
        {
            var uxSearchAndAddApp = _taskMan.GetUserExperience(uxTaskId) as UxSearchAndAddApp;
            if (uxSearchAndAddApp == null) return RunTask(typeof(LuRunAndManageApps).Name);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.AppHost = uxSearchAndAddApp.AppHost;

            return View("SearchAndAddApp", model: uxSearchAndAddApp.SearchText); // N.B. without explicitly naming model (string) parameter, MVC can get its View method overloads in a mucking fuddle.
        }

        public ActionResult SearchAppsToAdd(int uxTaskId, string searchText)//, string appHost)
        {
            var uxUxSearchAndAddApp = _taskMan.GetUserExperience(uxTaskId) as UxSearchAndAddApp;
            if (uxUxSearchAndAddApp == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxUxSearchAndAddApp.SearchText = searchText;
            uxUxSearchAndAddApp.Save();

            var model = uxUxSearchAndAddApp.SearchResults();

            ViewBag.AppHost = uxUxSearchAndAddApp.AppHost;

            return PartialView("MatchingAppsToAdd", model);
        }


        public ActionResult MyApps(int uxTaskId)
        {
            var uxMyApps = _taskMan.GetUserExperience(uxTaskId) as UxMyApps;
            if (uxMyApps == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = uxMyApps.GetApps();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.Host = uxMyApps.Host;

            return View("MyApps", model);

        }

        public ActionResult My2ndApps(int uxTaskId, bool hideInstructions = false)
        {
            var uxMy2ndApps = _taskMan.GetUserExperience(uxTaskId) as UxMy2ndApps;
            if (uxMy2ndApps == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxMy2ndApps.HideInstructions = hideInstructions;
            uxMy2ndApps.Save();

            var model = uxMy2ndApps.GetApps();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.Host = uxMy2ndApps.Host;

            return View("My2ndApps", model);

        }


        public ActionResult ActiveTasks(int uxTaskId)
        {
            var uxActiveTasks = _taskMan.GetUserExperience(uxTaskId) as UxActiveTasks;
            if (uxActiveTasks == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = uxActiveTasks.GetActiveTasks();

            ViewBag.uxTaskId = uxTaskId;

            return View("ActiveTasks", model);

        }

        public ActionResult CompletedTasks(int uxTaskId)
        {
            var uxCompletedTasks = _taskMan.GetUserExperience(uxTaskId) as UxCompletedTasks;
            if (uxCompletedTasks == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = uxCompletedTasks.GetCompletedTasks();

            ViewBag.uxTaskId = uxTaskId;

            return View("CompletedTasks", model);

        }


        //public ActionResult AddComponentToMyApps(string componentName)
        //{
        //    var luRunAndManageApps = _taskMan.GetSingletonComponent<LuRunAndManageApps>();
        //    if (luRunAndManageApps == null) return HttpNotFound($"Singleton component {typeof(LuRunAndManageApps).Name} not found.");

        //    luRunAndManageApps.AddComponentToMyApps(componentName);
        //    var nextComponent = luRunAndManageApps.GetNextComponent();
        //    luRunAndManageApps.Save();
        //    nextComponent.Save();

        //    return ResumeTask(nextComponent.TaskId);
        //}

        public ActionResult AddTaskType(int uxTaskId)
        {
            var uxAddTaskType = _taskMan.GetUserExperience(uxTaskId) as UxAddTaskType;
            if (uxAddTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = uxAddTaskType.PrepareComponentTaskType();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxAddTaskType.ShowBackButton;
            ViewBag.BackButtonText = uxAddTaskType.BackButtonText;
            ViewBag.DoneButtonText = uxAddTaskType.DoneButtonText;

            return View("SaveTaskType", model);

        }

        // POST: TaskType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddTaskType(int uxTaskId, [Bind(Include = "Id,Host,Name,RootComponentName,SearchTags,TaskInputs")] TaskType taskType)
        {
            if (!ModelState.IsValid) return View("SaveTaskType", taskType);

            var uxAddTaskType = _taskMan.GetUserExperience(uxTaskId) as UxAddTaskType;
            if (uxAddTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAddTaskType.SaveTaskType(taskType);

            return RedirectAfterUserExperienceCompleted(uxAddTaskType);

        }

        public ActionResult AmendTaskType(int uxTaskId)
        {
            var uxAmendTaskType = _taskMan.GetUserExperience(uxTaskId) as UxAmendTaskType;
            if (uxAmendTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (!uxAmendTaskType.ComponentId.HasValue) return HttpNotFound($"User Experience with id {uxTaskId} has a null component id.");

            var componentId = uxAmendTaskType.ComponentId.Value;

            var model = uxAmendTaskType.LoadTaskType(componentId);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxAmendTaskType.ShowBackButton;
            ViewBag.BackButtonText = uxAmendTaskType.BackButtonText;
            ViewBag.DoneButtonText = uxAmendTaskType.DoneButtonText;

            return View("SaveTaskType", model);

        }

        // POST: TaskType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AmendTaskType(int uxTaskId, [Bind(Include = "Id,Host,Name,RootComponentName,SearchTags,TaskInputs")] TaskType taskType)
        {
            if (!ModelState.IsValid) return View("SaveTaskType", taskType);

            var uxAmendTaskType = _taskMan.GetUserExperience(uxTaskId) as UxAmendTaskType;
            if (uxAmendTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAmendTaskType.SaveTaskType(taskType);

            return RedirectAfterUserExperienceCompleted(uxAmendTaskType);

        }

        public ActionResult DeleteTaskType(int uxTaskId)
        {
            var uxDeleteTaskType = _taskMan.GetUserExperience(uxTaskId) as UxDeleteTaskType;
            if (uxDeleteTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (!uxDeleteTaskType.ComponentId.HasValue) return HttpNotFound($"User Experience with id {uxTaskId} has a null component id.");

            var componentId = uxDeleteTaskType.ComponentId.Value;

            var model = uxDeleteTaskType.LoadTaskType(componentId);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxDeleteTaskType.ShowBackButton;
            ViewBag.BackButtonText = uxDeleteTaskType.BackButtonText;
            ViewBag.DoneButtonText = uxDeleteTaskType.DoneButtonText;

            return View("DeleteTaskType", model);

        }

        // POST: TaskType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteTaskType(int uxTaskId, [Bind(Include = "Id,Host,Name,RootComponentName,SearchTags,TaskInputs")] TaskType taskType)
        {
            var uxDeleteTaskType = _taskMan.GetUserExperience(uxTaskId) as UxDeleteTaskType;
            if (uxDeleteTaskType == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxDeleteTaskType.DeleteTaskType(taskType);

            return RedirectAfterUserExperienceCompleted(uxDeleteTaskType);

        }

        public ActionResult SuspendUxTask(int uxTaskId)
        {
            // TODO: Should really keep luTopLauncherTask.ResumeTaskId updated on every redirect in the system because suspension is always implicit.
            var luTopLauncherTask = _taskMan.GetUltimateParentLauncherLogicalUnitTask(uxTaskId);
            if (luTopLauncherTask != null)
            {
                var luLauncher = (LuLauncher)_taskMan.GetLogicalUnit(luTopLauncherTask);
                luLauncher.ResumeTaskId = uxTaskId;
                luLauncher.Save();
            }
            return RedirectToAction("RunTask", "TaskManager", new { componentName = typeof(LuRunAndManageApps).Name, Command = "ActiveTasks" });
        }

    }
}