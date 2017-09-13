using System.Web.Mvc;

namespace WebUXv2.Components
{
    public class PreserveQueryStringAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var redirectResult = filterContext.Result as RedirectToRouteResult;
            if (redirectResult == null)
            {
                return;
            }

            var query = filterContext.HttpContext.Request.QueryString;
            foreach (string key in query.Keys)
            {
                if (redirectResult.RouteValues.ContainsKey(key))
                {
                    //Querystring values take precedence
                    redirectResult.RouteValues[key] = query[key];
                }
                else
                {
                    redirectResult.RouteValues.Add(key, query[key]);
                }
            }
        }

    }
}