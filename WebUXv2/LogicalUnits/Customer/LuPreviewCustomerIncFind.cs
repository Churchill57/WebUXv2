using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("* Preview Customer (inc find)")]
    [LaunchableComponent("customer preview")]
    public class LuPreviewCustomerIncFind : LogicalUnit
    {

        [ComponentState]
        internal string NextStep { get; set; } = "Find";

        [LaunchInput]
        //[ComponentInput("customer")]
        [ComponentState]
        public EntityContext SelectedCustomerContext { get; set; }
        //public int? SelectedCustomerId { get; set; } = null;

        [ComponentState]
        internal EntityContext PreviewedCustomerContext { get; set; }

        //internal int? PreviewedCustomerId { get; set; } = null;

        public override Component GetNextComponent()
        {
            if (PreviewedCustomerContext!=null) return null;
            if (SelectedCustomerContext!=null) NextStep = "Preview";

            if (NextStep == "Find")
            {
                return FindCustomer();
            }

            if (NextStep == "Preview")
            {
                return PreviewCustomer();
            }

            return null;
        }

        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }

        private LuFindCustomer FindCustomer()
        {
            var luFindCustomer = GetLogicalUnit<LuFindCustomer>("Find", InitializeFindCustomer);
            //luFindCustomer.SelectedCustomerId = SelectedCustomerId;
            luFindCustomer.SelectButtonText = "Preview";
            luFindCustomer.ShowSearchBackButton = false;
            luFindCustomer.SelectBackButtonText = "Try a different search";
            return luFindCustomer;
        }

        private void InitializeFindCustomer(LuFindCustomer luFindCustomer)
        {
            luFindCustomer.InitialiseState();
        }


        private UxPreviewCustomer PreviewCustomer()
        {
            var uxPreviewCustomer = GetUserExperience<UxPreviewCustomer>("Preview");
            uxPreviewCustomer.DoneButtonText = "Done";
            uxPreviewCustomer.ShowBackButton = true;
            uxPreviewCustomer.PreviewDifferentCustomer = false;
            uxPreviewCustomer.BackButtonAsLink = true;
            uxPreviewCustomer.BackButtonText = "Select a different customer";
            uxPreviewCustomer.CustomerContext = SelectedCustomerContext;
            return uxPreviewCustomer;
        }
        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Find")
            {
                SelectedCustomerContext = ((LuFindCustomer)comp).SelectedCustomerContext;
                NextStep = "Preview";
            }

            if (comp.ClientRef == "Preview")
            {
                PreviewedCustomerContext = ((UxPreviewCustomer)comp).CustomerContext;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (eventArgs.Component.ClientRef == "Find")
                {
                    NextStep = null;
                }

                if (eventArgs.Component.ClientRef == "Preview")
                {
                    SelectedCustomerContext = null;
                    NextStep = "Find";
                }
            }
        }

    }
}