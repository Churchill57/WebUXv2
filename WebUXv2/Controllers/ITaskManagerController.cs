using System;
using System.Web.Mvc;
using System.Web.Routing;
using WebUXv2.Components;

namespace WebUXv2.Controllers
{
    public interface ITaskManagerController
    {
        ControllerContext ControllerContext { get; set; }
        ActionResult Done(int uxTaskId);
        ActionResult Back(int uxTaskId);
        ActionResult DeleteAllTasks();
        //UserExperience GetUserExperience(LogicalUnit parentLu, string uxTypeName, string clientRef);
        ActionResult AccessDenied(string nextUrl, int? nextTaskId, string returnUrl, string loginReturnUrl);
        ActionResult RedirectAfterComponentEvent(ComponentEventArgs eventArgs, bool forceContextSearch = false);
        ActionResult RedirectAfterUserExperienceCompleted(UserExperience uxCompleted, bool forceContextSearch = false);
        ActionResult RedirectToUserExperience(UserExperience ux, bool forceContextSearch = false, string searchText = null);
        ActionResult RedirectToSingletonComponent(string componentName, int? returnTaskId = null, int? parentTaskId = null, bool forceContextSearch = false, string searchText = null, string returnTaskRef = null);
        ActionResult RedirectToResolveComponentInputs(Component comp, RouteValueDictionary routeValues, bool forceContextSearch = false, string searchText = null);
        ActionResult ResumeTask(int componentTaskId, bool forceContextSearch = false, string searchText = null);
        ActionResult RunTask(string componentName, int? returnTaskId = null, int? parentTaskId = null, bool forceContextSearch = false, string searchText = null, string returnTaskRef = null, string userMessage = null);
        ActionResult RunTaskType(int taskTypeId, int? returnTaskId = null, bool forceContextSearch = false, string userMessage = null);
    }
}