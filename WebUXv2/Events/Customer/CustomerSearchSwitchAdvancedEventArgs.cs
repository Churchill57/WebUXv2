
using WebUXv2.Components;

namespace WebUXv2.Events.Customer
{
    public class CustomerSearchSwitchAdvancedEventArgs : ComponentEventArgs
    {
        public bool AdvancedSearch { get; set; }
        public CustomerSearchSwitchAdvancedEventArgs(Component sourceComponent, bool advancedSearch) : base(sourceComponent)
        {
            AdvancedSearch = advancedSearch;
        }
    }
}