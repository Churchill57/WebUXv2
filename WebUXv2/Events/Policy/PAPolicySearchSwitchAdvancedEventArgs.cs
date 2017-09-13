
using WebUXv2.Components;

namespace WebUXv2.Events.Customer
{
    public class PAPolicySearchSwitchAdvancedEventArgs : ComponentEventArgs
    {
        public bool AdvancedSearch { get; set; }
        public PAPolicySearchSwitchAdvancedEventArgs(Component sourceComponent, bool advancedSearch) : base(sourceComponent)
        {
            AdvancedSearch = advancedSearch;
        }
    }
}