using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("Customer Search (temp)")]
    //[LaunchableComponent("temporary")]
    public class LuCustomerSearchCriteria : LogicalUnit
    {

        //[LaunchInput]
        [ComponentState]
        public bool AdvancedSearch { get; set; } = false;

        [ComponentState]
        public bool ShowSearchBackButton { get; set; } = true;

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

        [ComponentState]
        internal EntityContext SearchCustomerContext { get; set; } = null;
        //internal int? SearchCustomerId { get; set; } = null;

        public void InitialiseState()
        {
            AdvancedSearch = false;
            ShowSearchBackButton = true;
            SearchTaskId = null;
            SearchCustomerContext = null;
        }
        public override Component GetNextComponent()
        {
            if (SearchCustomerContext!=null)
            {
                return null;
            }
            if (!SearchTaskId.HasValue)
            {
                return GetSearchUserExperience();
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            if (SearchTaskId.HasValue)
            {
                SearchCustomerContext = null;
                SearchTaskId = null;
                return GetSearchUserExperience();
            }
            return null;
        }

        private UserExperience GetSearchUserExperience()
        {
            if (AdvancedSearch)
            {
                var uxCustomerAdvSearchCriteria = GetUserExperience<UxCustomerAdvSearchCriteria>("AdvancedSearch", InitializeAdvancedSearch);
                uxCustomerAdvSearchCriteria.SelectedCustomerContext = null; // Important to remember that back and forward navigation will yield a persisted ux which may have selected customer previously set!
                return uxCustomerAdvSearchCriteria;
            }
            return GetUserExperience<UxCustomerSearchCriteria>("BasicSearch", InitialiseBasicSearch);
        }

        private void InitializeAdvancedSearch(UxCustomerAdvSearchCriteria uxCustomerAdvSearch)
        {
            uxCustomerAdvSearch.ShowSwitchToBasic = true;
            uxCustomerAdvSearch.ShowBackButton = ShowSearchBackButton;
        }
        private void InitialiseBasicSearch(UxCustomerSearchCriteria uxBasicSearch)
        {
            uxBasicSearch.ShowSwitchToAdvanced = true;
            uxBasicSearch.ShowBackButton = ShowSearchBackButton;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "BasicSearch")
            {
                SearchTaskId = comp.TaskId;
            }
            if (comp.ClientRef == "AdvancedSearch")
            {
                SearchCustomerContext = ((UxCustomerAdvSearchCriteria)comp).SelectedCustomerContext as EntityContext;
                if (SearchCustomerContext==null)
                {
                    SearchTaskId = comp.TaskId;
                }
            }
        }

        // TODO: Is HandleComponentEvent really necessary? Probably yes for switching searches.
        // But not sure if BackEvent is necessary?
        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            var args = eventArgs as CustomerSearchSwitchAdvancedEventArgs;
            if (args != null)
            {
                AdvancedSearch = args.AdvancedSearch;
            }
            if (eventArgs is BackEventArgs)
            {
            }
        }

    }
}