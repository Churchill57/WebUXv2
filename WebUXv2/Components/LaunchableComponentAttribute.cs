using System;

namespace WebUXv2.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LaunchableComponentAttribute : Attribute
    {
        public LaunchableComponentAttribute(string searchTags)
        {
            SearchTags = searchTags;
        }
        public string SearchTags { get; }

    }
}