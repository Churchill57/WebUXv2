using System;
using System.Collections.Generic;
using System.Linq;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.LogicalUnits.Policy;
using WebUXv2.Models;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    [ComponentTitle("Find Customer Context")]
    //[SingletonComponent]
    //[LaunchableComponent("temporary")]
    public class LuFindCustomerContext : LogicalUnit, ISingletonComponent, IContextFinder
    {

        [ComponentState]
        public bool AdvancedSearch { get; set; } = true;

        private bool _showSearchBackButton = true;
        private bool _showSelectBackButton = true;

        [ComponentState]
        public EntityContext SelectedCustomerContext { get; internal set; }

        //[ComponentState]
        //public int? SelectedCustomerId { get; internal set; } = null;

        [ComponentState]
        internal bool CriteriaCaptured { get; set; } = false;

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

        //[ComponentState]
        internal EntityContext SearchCustomerContext { get; set; } // A recent customer selected in the advanced search.
        //internal int? SearchCustomerId { get; set; } = null; // A recent customer selected in the advanced search.

        [ComponentState]
        private string SearchCriteriaUserMessage { get; set; } = null;

        //[ComponentState]
        private string NextStep { get; set; } = "CheckContext";
        private IEntityContextManager _cm = SingletonService.Instance.EntityContextManager;

        [ComponentState]
        private bool _forceNextContextSearch { get; set; } = false;

        [ComponentInput()]
        [ComponentState]
        public bool ShowSearchBackButton
        {
            get { return _showSearchBackButton; }
            set
            {
                _showSearchBackButton = value;
                CustomerBasicSearchCriteria().Save(); // Cascades ux ShowBackButton property
                CustomerAdvancedSearchCriteria().Save(); // Cascades ux ShowBackButton property
            }
        }

        [ComponentState]
        public bool ShowSelectBackButton
        {
            get { return _showSelectBackButton; }
            set
            {
                _showSelectBackButton = value;
                SelectCustomer().Save(); // Cascades ux ShowBackButton property
            }
        }

        //public LuFindCustomerContext()
        //{
        //    InitialiseState();
        //}
        public void InitialiseState()
        {
            NextStep = "CheckContext";
            SelectedCustomerContext = null;
            //SelectedCustomerId = null;
            CriteriaCaptured = false;
            SearchTaskId = null;
        }

        public bool CustomerExists(int id)
        {
            return EntityExists(id);
        }

        public override Component GetNextComponent()
        {
            if (NextStep == "CheckContext")
            {
                var policyContext = _cm.ResolveContext("policy");
                if (policyContext != null)
                {
                    var relatedCustomerContexts = _cm.GetDirectRelationships(policyContext, null, "customer").ToList();
                    if (relatedCustomerContexts.Count == 1 && !_forceNextContextSearch)
                    {
                        SelectedCustomerContext = relatedCustomerContexts.First().EntityContext as EntityContext;
                        //SelectedCustomerId = relatedCustomerContexts.First().EntityContext.Id;
                        CtxMan.SetContext(relatedCustomerContexts.First().EntityContext);
                        return null;
                    }

                    var lu = GetLogicalUnit<LuFindCustomerByPolicy>("-LuFindCustomerByPolicy");
                    lu.PolicyId = policyContext.Id;
                    return lu;
                }


                var addressContext = _cm.ResolveContext("address");
                if (addressContext != null)
                {
                    var relatedCustomerContexts = _cm.GetDirectRelationships(addressContext, null, "customer").ToList();
                    if (relatedCustomerContexts.Count == 1 && !_forceNextContextSearch)
                    {
                        SelectedCustomerContext = relatedCustomerContexts.First().EntityContext as EntityContext;
                        //SelectedCustomerId = relatedCustomerContexts.First().EntityContext.Id;
                        CtxMan.SetContext(relatedCustomerContexts.First().EntityContext);
                        return null;
                    }
                }



            }

            _forceNextContextSearch = false;

            if (SearchCustomerContext!=null)
            {
                SearchCriteriaUserMessage = null;
                return null;
            }

            if (SelectedCustomerContext !=null)
            {
                SearchCriteriaUserMessage = null;
                return null;
            }

            if (!CriteriaCaptured)
            {
                SearchCriteriaUserMessage = SingletonService.Instance.UserMessage;
                return CustomerSearchCriteria();
            }

            SingletonService.Instance.UserMessage = SearchCriteriaUserMessage;
            return SelectCustomer();
        }

        private void InitializeSelectCustomer(UxSelectCustomer uxSelectCustomer)
        {
            uxSelectCustomer.ShowBackButton = _showSelectBackButton;
            uxSelectCustomer.BackButtonAsLink = true;
            uxSelectCustomer.BackButtonText = "Try a different search";
        }

        private UserExperience CustomerSearchCriteria()
        {
            if (AdvancedSearch) return CustomerAdvancedSearchCriteria();
            return CustomerBasicSearchCriteria();
        }

        private UserExperience CustomerBasicSearchCriteria()
        {
            var uxCustomerSearchCriteria = GetUserExperience<UxCustomerSearchCriteria>("BasicCriteria", InitializeBasicSearch);
            uxCustomerSearchCriteria.ShowBackButton = ShowSearchBackButton;
            return uxCustomerSearchCriteria;
        }

        private void InitializeBasicSearch(UxCustomerSearchCriteria uxCustomerSearchCriteria)
        {
            uxCustomerSearchCriteria.ShowSwitchToAdvanced = true;
            //uxCustomerSearchCriteria.ShowBackButton = ShowSearchBackButton;
        }
        private UserExperience CustomerAdvancedSearchCriteria()
        {
            var uxCustomerAdvSearchCriteria = GetUserExperience<UxCustomerAdvSearchCriteria>("AdvancedCriteria", InitializeAdvancedSearch);
            uxCustomerAdvSearchCriteria.ShowBackButton = ShowSearchBackButton;
            uxCustomerAdvSearchCriteria.SelectedCustomerContext = null;
            return uxCustomerAdvSearchCriteria;
        }
        private void InitializeAdvancedSearch(UxCustomerAdvSearchCriteria uxCustomerAdvSearchCriteria)
        {
            uxCustomerAdvSearchCriteria.ShowSwitchToBasic = true;
            //uxCustomerAdvSearchCriteria.ShowBackButton = ShowSearchBackButton;
        }
        private UserExperience SelectCustomer()
        {
            var uxSelectCustomer = GetUserExperience<UxSelectCustomer>("Select", InitializeSelectCustomer);
            uxSelectCustomer.ShowBackButton = ShowSelectBackButton;
            uxSelectCustomer.SearchTaskId = SearchTaskId;
            return uxSelectCustomer;
        }

        public override Component GetPrevComponent()
        {
            if (SearchCustomerContext!=null || NextStep== "CheckContext")
            {
                SearchCustomerContext = null;
                SelectedCustomerContext = null;
                SingletonService.Instance.UserMessage = SearchCriteriaUserMessage;
                return CustomerSearchCriteria();
            }
            if (SelectedCustomerContext!=null)
            {
                SelectedCustomerContext = null;
                SingletonService.Instance.UserMessage = SearchCriteriaUserMessage;
                return SelectCustomer();
            }
            if (CriteriaCaptured)
            {
                CriteriaCaptured = false;
                SearchTaskId = null;
                SearchCustomerContext = null;
                SelectedCustomerContext = null;
                SingletonService.Instance.UserMessage = SearchCriteriaUserMessage;
                return CustomerSearchCriteria();
            }

            SearchCriteriaUserMessage = null;
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "-LuFindCustomerByPolicy") // TODO; Hash ClientRefs to make them totally unique.
            {
                SelectedCustomerContext = ((LuFindCustomerByPolicy) comp).SelectedCustomerContext;
                NextStep = null;
            }
            NextStep = "CustomerSearch";
            if (comp.ClientRef == "BasicCriteria")
            {
                CriteriaCaptured = true;
                SearchTaskId = ((UxCustomerSearchCriteria)comp).TaskId;
            }
            if (comp.ClientRef == "AdvancedCriteria")
            {
                SearchCustomerContext = ((UxCustomerAdvSearchCriteria)comp).SelectedCustomerContext;
                if (SearchCustomerContext!=null)
                {
                    SelectedCustomerContext = SearchCustomerContext;
                }
                else
                {
                    CriteriaCaptured = true;
                    SearchTaskId = ((UxCustomerAdvSearchCriteria)comp).TaskId;
                }
            }
            if (comp.ClientRef == "Select")
            {
                SelectedCustomerContext = ((UxSelectCustomer)comp).SelectedCustomerContext;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            var args = eventArgs as CustomerSearchSwitchAdvancedEventArgs;
            if (args != null)
            {
                AdvancedSearch = args.AdvancedSearch;
                SingletonService.Instance.UserMessage = SearchCriteriaUserMessage;
            }
            if (eventArgs is BackEventArgs)
            {
                // TODO: Bubbles up back event. Need a more general mechanism for this
                eventArgs.Component = this;
            }
        }

        public void ForceNextContextSearch()
        {
            _forceNextContextSearch = true;
        }


        public bool EntityExists(int id)
        {
            var db = new CustomerDbContext();
            var customer = db.Customers.Find(id);
            return (customer != null);
        }
    }
}