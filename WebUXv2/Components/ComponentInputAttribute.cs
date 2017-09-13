using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace WebUXv2.Components
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ComponentInputAttribute : Attribute
    {
        public ComponentInputAttribute(string contextName = null)
        {
            ContextName = contextName;
        }

        public string ContextName { get; }
    }
}