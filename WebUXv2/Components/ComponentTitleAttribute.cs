using System;

namespace WebUXv2.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentTitleAttribute : Attribute
    {
        public ComponentTitleAttribute(string title)
        {
            Title = title;
        }
        public string Title { get; }
    }
}