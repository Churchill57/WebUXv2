using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("* Manage Customer (inc find)")]
    [LaunchableComponent("customer manage review")]
    public class LuManageIncSearchCustomer : LogicalUnit
    {

        [ComponentState]
        internal EntityContext SelectedCustomerContext { get; set; }
        //internal int? SelectedCustomerId { get; set; }= null;

        [ComponentState]
        internal EntityContext ManagedCustomerContext { get; set; }
        //internal int? ManagedCustomerId { get; set; }= null;

        [ComponentState]
        private string NextClientRef { get; set; } = "Find";


        public override Component GetNextComponent()
        {
            if (NextClientRef == "Find") return GetLogicalUnit<LuFindCustomer>("Find", InitializeFindCustomer);
            if (NextClientRef == "Manage") return GetUserExperience<UxManageCustomer>("Manage");
            return null;
        }

        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }

        private void InitializeFindCustomer(LuFindCustomer luFindCustomer)
        {
            luFindCustomer.SelectButtonText = "Manage";
            luFindCustomer.AdvancedSearch = false;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Find")
            {
                SelectedCustomerContext = ((LuFindCustomer)comp).SelectedCustomerContext;
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
                    NextClientRef = "Find";
                }
            }
        }

    }
}