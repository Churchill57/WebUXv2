using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("Preview Customer (excl find)")]
    [LaunchableComponent("customer preview")]

    public class LuPreviewCustomerExclFind : LogicalUnit
    {

        [ComponentState]
        internal string NextStep { get; set; } = "Criteria";

        [LaunchInput]
        [ComponentInput("customer")]
        [ComponentState]
        public EntityContext PreviewCustomerContext { get; set; }
        //public int? PreviewCustomerId { get; set; } = null;

        [ComponentState]
        internal EntityContext PreviewedCustomerContext { get; set; }
        //internal int? PreviewedCustomerId { get; set; } = null;

        public override Component GetNextComponent()
        {
            if (PreviewedCustomerContext==null)
            {
                return PreviewCustomer();
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            if (PreviewedCustomerContext!=null)
            {
                PreviewedCustomerContext = null;
                return PreviewCustomer();
            }
            return null;
        }

        private UxPreviewCustomer PreviewCustomer()
        {
            var uxPreviewCustomer = GetUserExperience<UxPreviewCustomer>("Preview");

            uxPreviewCustomer.DoneButtonText = "Done";
            uxPreviewCustomer.ShowBackButton = false;
            uxPreviewCustomer.PreviewDifferentCustomer = true;

            return uxPreviewCustomer;
        }


        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Preview")
            {
                PreviewedCustomerContext = ((UxPreviewCustomer)comp).CustomerContext;
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