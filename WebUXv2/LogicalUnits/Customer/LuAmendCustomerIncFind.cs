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
    [ComponentTitle("Amend Customer (inc find)")]
    [LaunchableComponent("customer amend change update edit alter")]
    public class LuAmendCustomerIncFind : LogicalUnit
    {

        //[ContextInput("customer")]
        [ComponentState]
        public EntityContext CustomerToAmend { get; set; }
        //public int? CustomerIdToAmend { get; set; }= null;

        [ComponentState]
        internal EntityContext AmendedCustomer { get; set; }
        //internal int? AmendedCustomerId { get; set; }= null;

        [ComponentState]
        internal EntityContext ManagedCustomerContext { get; set; }
        //internal int? ManagedCustomerId { get; set; }= null;

        public override Component GetNextComponent()
        {
            if (CustomerToAmend==null)
            {
                return GetLogicalUnit<LuFindCustomer>("Find", InitializeFindCustomer);
            }
            if (AmendedCustomer==null)
            {
                var uxAmendCustomer = GetUserExperience<UxAmendCustomer>("Amend");
                uxAmendCustomer.CustomerContext = CustomerToAmend;
                return uxAmendCustomer;
            }
            if (ManagedCustomerContext==null)
            {
                var uxManageCustomer = GetUserExperience<UxManageCustomer>("Manage", InitializeManageCustomer);
                //uxManageCustomer.CustomerId = AmendedCustomerId; // Get value from context
                return uxManageCustomer;
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            if (ManagedCustomerContext!=null)
            {
                ManagedCustomerContext = null;
                return GetUserExperience<UxManageCustomer>("Manage");
            }
            if (AmendedCustomer!=null)
            {
                AmendedCustomer = null;
                return GetUserExperience<UxAmendCustomer>("Amend");
            }
            if (CustomerToAmend!=null)
            {
                CustomerToAmend = null;
                return GetLogicalUnit<LuFindCustomer>("Find");
            }
            return null;
        }

        private void InitializeFindCustomer(LuFindCustomer luFindCustomer)
        {
            luFindCustomer.SelectButtonText = "Amend";
            luFindCustomer.AdvancedSearch = true;
            luFindCustomer.ShowSelectPreviewLink = true;
        }

        private void InitializeManageCustomer(UxManageCustomer uxManageCustomer)
        {
            uxManageCustomer.ShowBackButton = true;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Find")
            {
                CustomerToAmend = ((LuFindCustomer)comp).SelectedCustomerContext;
            }
            if (comp.ClientRef == "Amend")
            {
                AmendedCustomer = ((UxAmendCustomer)comp).CustomerContext;
            }
            if (comp.ClientRef == "Manage")
            {
                ManagedCustomerContext = ((UxManageCustomer)comp).CustomerContext;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                //BackEvent logic effectively handled in GetPrevComponent
            }
        }

    }
}