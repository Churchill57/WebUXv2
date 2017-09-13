using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;
using WebUXv2.UserExperiences.Policy;

namespace WebUXv2.LogicalUnits.Policy
{
    [ComponentTitle("Find Customer by Policy")]
    //[SingletonComponent]
    [LaunchableComponent("temporary")]
    public class LuFindCustomerByPolicy : LogicalUnit//, ISingletonComponent
    {

        [ComponentInput("policy")]
        [ComponentState]
        public int? PolicyId { get; set; } = null;

        private bool _showSearchBackButton = true;

        [ComponentState]
        public EntityContext SelectedCustomerContext { get; internal set; } = null;
        //public int? SelectedCustomerId { get; internal set; } = null;

        [ComponentState]
        internal bool CriteriaCaptured { get; set; } = false;

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

        //public LuFindCustomerByPolicy()
        //{
        //    InitialiseState();
        //}
        [ComponentState]
        public bool ShowSearchBackButton
        {
            get { return _showSearchBackButton; }
            set
            {
                _showSearchBackButton = value;
                CustomerSearchCriteria().Save(); // Cascades ux ShowBackButton property
            }
        }

        public override Component GetNextComponent()
        {
            if (SelectedCustomerContext !=null) return null;

            if (PolicyId.HasValue)
            {
                var customerSearchCriteria = CustomerSearchCriteria();
                customerSearchCriteria.Criteria.Id = PolicyId;
                customerSearchCriteria.Save();

                SingletonService.Instance.UserMessage = $"...from policy {PolicyId}";

                SearchTaskId = customerSearchCriteria.TaskId;
                //PolicyId = null;
                CriteriaCaptured = true;
                Save();

                return SelectCustomer();
            }
            //if (!CriteriaCaptured)
            //{
            //    return CustomerSearchCriteria();
            //}
            //if (!SelectedCustomerId.HasValue)
            //{
            //    return SelectCustomer();
            //}
            return null;
        }

        private void InitializeSelectCustomer(UxSelectCustomer uxSelectCustomer)
        {
            uxSelectCustomer.ShowBackButton = true;
            uxSelectCustomer.BackButtonAsLink = true;
            uxSelectCustomer.BackButtonText = "Try a different search";
        }

        private UxCustomerByPolicySearchCriteria CustomerSearchCriteria()
        {
            var uxCustomerByPolicySearchCriteria = GetUserExperience<UxCustomerByPolicySearchCriteria>("Criteria");
            uxCustomerByPolicySearchCriteria.ShowBackButton = ShowSearchBackButton;
            return uxCustomerByPolicySearchCriteria;
        }

        private UxSelectCustomer SelectCustomer()
        {
            var uxSelectCustomer = GetUserExperience<UxSelectCustomer>("Select", InitializeSelectCustomer);
            uxSelectCustomer.SearchTaskId = SearchTaskId;
            return uxSelectCustomer;
        }

        public override Component GetPrevComponent()
        {
            if (SelectedCustomerContext!=null)
            {
                SelectedCustomerContext = null;
                return SelectCustomer();
            }
            //if (CriteriaCaptured)
            //{
            //    CriteriaCaptured = false;
            //    SearchTaskId = null;
            //    return CustomerSearchCriteria();
            //}
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Criteria")
            {
                CriteriaCaptured = true;
                SearchTaskId = comp.TaskId;
            }
            if (comp.ClientRef == "Select")
            {
                SelectedCustomerContext = ((UxSelectCustomer)comp).SelectedCustomerContext;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                //BackEvent logic effectively handled in GetPrevComponent
            }
        }


        //public void InitialiseState()
        //{
        //    PolicyId = null;
        //    SelectedCustomerId = null;
        //    CriteriaCaptured = false;
        //    SearchTaskId = null;
        //}
    }
}