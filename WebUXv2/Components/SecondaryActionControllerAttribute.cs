using System;

namespace WebUXv2.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SecondaryActionControllerAttribute : Attribute
    {
        public SecondaryActionControllerAttribute(string actionName, string controllerName)
        {
            ActionName = actionName;
            ControllerName = controllerName;
        }
        public string ActionName { get; }
        public string ControllerName { get; }

    }
}