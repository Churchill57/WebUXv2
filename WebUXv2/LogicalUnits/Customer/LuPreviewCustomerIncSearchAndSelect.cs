using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("Preview Customer (inc search & select)")]
    [LaunchableComponent("customer preview")]
    public class LuPreviewCustomerIncSearchAndSelect: LogicalUnit
    {

        [ComponentState]
        internal string NextStep { get; set; } = "Criteria";

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

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

            if (NextStep == "Criteria")
            {
                return CustomerSearchCriteria();
            }

            if (NextStep == "Select")
            {
                var uxSelectCustomer = SelectCustomer();
                uxSelectCustomer.SearchTaskId = SearchTaskId;
                return uxSelectCustomer;
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

        private UxPreviewCustomer PreviewCustomer()
        {
            var uxPreviewCustomer = GetUserExperience<UxPreviewCustomer>("Preview");
            uxPreviewCustomer.DoneButtonText = "Finish >";
            uxPreviewCustomer.ShowBackButton = true;
            uxPreviewCustomer.BackButtonText = "< Prev";
            uxPreviewCustomer.CustomerContext = SelectedCustomerContext;
            return uxPreviewCustomer;
        }
        private UxCustomerSearchCriteria CustomerSearchCriteria()
        {
            var uxCustomerSearchCriteria = GetUserExperience<UxCustomerSearchCriteria>("Criteria");
            uxCustomerSearchCriteria.SearchButtonText = "Next >";
            uxCustomerSearchCriteria.BackButtonText = "< Prev";
            return uxCustomerSearchCriteria;
        }

        private UxSelectCustomer SelectCustomer()
        {
            var uxSelectCustomer = GetUserExperience<UxSelectCustomer>("Select");
            uxSelectCustomer.SelectButtonText = "Next >";
            uxSelectCustomer.BackButtonText = "< Prev";
            return uxSelectCustomer;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Criteria")
            {
                SearchTaskId = comp.TaskId;
                NextStep = "Select";
            }

            if (comp.ClientRef == "Select")
            {
                SelectedCustomerContext = ((UxSelectCustomer)comp).SelectedCustomerContext;
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
                if (eventArgs.Component.ClientRef == "Criteria")
                {
                    SearchTaskId = null;
                    NextStep = null;
                }

                if (eventArgs.Component.ClientRef == "Select")
                {
                    NextStep = "Criteria";
                }

                if (eventArgs.Component.ClientRef == "Preview")
                {
                    SelectedCustomerContext = null;
                    NextStep = "Select";
                }
            }
        }

    }
}