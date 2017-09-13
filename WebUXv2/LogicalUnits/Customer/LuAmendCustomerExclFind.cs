using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    //[Authorize(Roles = "Admin")]
    [ComponentTitle("Amend Customer (excl find)")]
    [LaunchableComponent("customer amend change update edit alter")]
    //[SecondaryUserExperience("customer amend change update edit alter")]
    public class LuAmendCustomerExclFind : LogicalUnit
    {

        //[ContextInput("customer")]
        //public int? CustomerIdToAmend { get; set; }= null;
        [ComponentState]
        internal EntityContext AmendedCustomer { get; set; }
        //internal int? AmendedCustomerId { get; set; }= null;

        [ComponentState]
        internal EntityContext ManagedCustomerContext { get; set; }
        //internal int? ManagedCustomerId { get; set; }= null;

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