using WebUXv2.Components;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("Amend Customer")]
    [LaunchableComponent("customer cust amend change update edit alter")]
    public class LuAmendCustomerExclFind : LogicalUnit
    {

        [ComponentState]
        internal EntityContext AmendedCustomer { get; set; }

        [ComponentState]
        internal EntityContext ManagedCustomerContext { get; set; }

        [ComponentState]
        private string NextClientRef { get; set; } = "Amend";

        public override Component GetNextComponent()
        {
            if (NextClientRef == "Amend") return GetUserExperience<UxAmendCustomer>("Amend", InitializeAmendCustomer);
            if (NextClientRef == "Manage") return GetUserExperience<UxManageCustomer>("Manage", InitializeManageCustomer);
            return null;
        }

        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }


        private void InitializeAmendCustomer(UxAmendCustomer uxAmendCustomer)
        {
            uxAmendCustomer.ShowBackButton = false;
        }

        private void InitializeManageCustomer(UxManageCustomer uxManageCustomer)
        {
            uxManageCustomer.ShowBackButton = false;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Amend")
            {
                AmendedCustomer = ((UxAmendCustomer)comp).CustomerContext;
                NextClientRef = "Manage";

            }
            if (comp.ClientRef == "Manage")
            {
                ManagedCustomerContext = ((UxManageCustomer)comp).CustomerContext;
                NextClientRef = null;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (eventArgs.Component.ClientRef == "Manage")
                {
                    NextClientRef = "Amend";
                }
                if (eventArgs.Component.ClientRef == "Amend")
                {
                    NextClientRef = null;
                }
            }
        }

    }
}