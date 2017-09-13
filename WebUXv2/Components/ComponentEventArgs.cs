using System;

namespace WebUXv2.Components
{
    public class ComponentEventArgs : EventArgs
    {
        public Component Component { get; set; }

        public ComponentEventArgs(Component component)
        {
            Component = component;
        }
    }
}