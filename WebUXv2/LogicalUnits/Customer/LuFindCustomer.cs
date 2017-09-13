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
    [ComponentTitle("Find Customer")]
    //[LaunchableComponent("temporary")]
    public class LuFindCustomer : LogicalUnit, ISingletonComponent
    {

        [ComponentState]
        public bool AdvancedSearch { get; set; } = false;

        [ComponentState]
        public bool ShowSearchBackButton { get; set; } = true;

        [ComponentState]
        public bool ShowSelectBackButton { get; set; } = true;

        [ComponentState]
        public string SelectButtonText { get; set; } = "Select";

        [ComponentState]
        public string SelectBackButtonText { get; set; } = "Back";

        [ComponentState]
        public bool ShowSelectPreviewLink { get; set; } = false;

        //[ContextInput("customer")]
        [ComponentState]
        public EntityContext SelectedCustomerContext { get; internal set; }
        //public int? SelectedCustomerId { get; internal set; } = null;

        [ComponentState]
        internal bool CriteriaCaptured { get; set; } = false;

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

        [ComponentState]
        internal EntityContext SearchCustomerContext { get; set; } // A recent customer selected in the advanced search.
        //internal int? SearchCustomerId { get; set; } = null; // A recent customer selected in the advanced search.

        public LuFindCustomer() { }

        public override Component GetNextComponent()
        {
            if (SearchCustomerContext!=null) return null;

            if (SelectedCustomerContext!=null) return null;

            if (!CriteriaCaptured)
            {
                var luCustomerSearchCriteria = GetLogicalUnit<LuCustomerSearchCriteria>("Criteria");
                luCustomerSearchCriteria.InitialiseState();
                luCustomerSearchCriteria.AdvancedSearch = AdvancedSearch;
                luCustomerSearchCriteria.ShowSearchBackButton = ShowSearchBackButton;
                return luCustomerSearchCriteria;
            }

            var uxSelectCustomer = SelectCustomer();
            uxSelectCustomer.SearchTaskId = SearchTaskId;
            return uxSelectCustomer;
        }
        private UxSelectCustomer SelectCustomer()
        {
            var uxSelectCustomer = GetUserExperience<UxSelectCustomer>("Select");
            uxSelectCustomer.ShowBackButton = ShowSelectBackButton;
            uxSelectCustomer.BackButtonAsLink = true;
            uxSelectCustomer.BackButtonText = SelectBackButtonText;

            uxSelectCustomer.SelectButtonText = SelectButtonText;
            uxSelectCustomer.ShowPreviewLink = ShowSelectPreviewLink;

            return uxSelectCustomer;
        }

        public override Component GetPrevComponent()
        {
            if (SearchCustomerContext!=null)
            {
                SearchCustomerContext = null;
                SelectedCustomerContext = null;
                return GetLogicalUnit<LuCustomerSearchCriteria>("Criteria");
            }
            if (SelectedCustomerContext!=null)
            {
                SelectedCustomerContext = null;
                return SelectCustomer();
            }
            if (CriteriaCaptured)
            {
                CriteriaCaptured = false;
                SearchTaskId = null;
                SearchCustomerContext = null;
                SelectedCustomerContext = null;
                return GetLogicalUnit<LuCustomerSearchCriteria>("Criteria");
            }
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Criteria")
            {
                SearchCustomerContext = ((LuCustomerSearchCriteria)comp).SearchCustomerContext as EntityContext;
                if (SearchCustomerContext!=null)
                {
                    SelectedCustomerContext = SearchCustomerContext;
                }
                else
                {
                    CriteriaCaptured = true;
                    SearchTaskId = ((LuCustomerSearchCriteria)comp).SearchTaskId;
                }
                AdvancedSearch = ((LuCustomerSearchCriteria) comp).AdvancedSearch;
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

        public void InitialiseState()
        {
            AdvancedSearch = false;
            ShowSearchBackButton = true;
            ShowSelectBackButton = true;
            SelectButtonText = "Select";
            SelectBackButtonText = "Back";
            ShowSelectPreviewLink = false;
            SelectedCustomerContext = null;
            CriteriaCaptured = false;
            SearchTaskId = null;
            SearchCustomerContext = null;
        }
    }
}