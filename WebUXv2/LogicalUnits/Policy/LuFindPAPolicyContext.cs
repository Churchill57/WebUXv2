using System;
using System.Collections.Generic;
using WebUXv2.Components;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Policy;

namespace WebUXv2.LogicalUnits.Policy
{
    [ComponentTitle("Find Policy Context")]
    //[SingletonComponent]
    [LaunchableComponent("temporary")]
    public class LuFindPAPolicyContext : LogicalUnit, ISingletonComponent
    {

        [ComponentState]
        public bool AdvancedSearch { get; set; } = false;

        private bool _showSearchBackButton = true;
        private bool _showSelectBackButton = true;

        [ComponentState]
        public int? SelectedPolicyId { get; internal set; } = null;

        [ComponentState]
        internal bool CriteriaCaptured { get; set; } = false;

        [ComponentState]
        internal int? SearchTaskId { get; set; } = null;

        [ComponentState]
        internal int? SearchPolicyId { get; set; } = null; // A recent Policy selected in the advanced search.


        [ComponentState]
        public bool ShowSearchBackButton
        {
            get { return _showSearchBackButton; }
            set
            {
                _showSearchBackButton = value;
                PolicyBasicSearchCriteria().Save(); // Cascades ux ShowBackButton property
                PolicyAdvancedSearchCriteria().Save(); // Cascades ux ShowBackButton property
            }
        }
        public bool ShowSelectBackButton
        {
            get { return _showSelectBackButton; }
            set
            {
                _showSelectBackButton = value;
                SelectPolicy().Save(); // Cascades ux ShowBackButton property
            }
        }


        public LuFindPAPolicyContext()
        {
            InitialiseState();
        }
        public void InitialiseState()
        {
            SelectedPolicyId = null;
            CriteriaCaptured = false;
            SearchTaskId = null;
        }

        public bool PolicyExists(int? id)
        {
            CustomerDbContext db = new CustomerDbContext();
            var policy = db.PAPolicies.Find(id);
            return (policy != null);
        }

        public override Component GetNextComponent()
        {
            if (SearchPolicyId.HasValue) return null;

            if (SelectedPolicyId.HasValue) return null;

            if (!CriteriaCaptured) return PolicySearchCriteria();

            return SelectPolicy();
        }

        private void InitializeSelectPolicy(UxSelectPAPolicy uxSelectPolicy)
        {
            uxSelectPolicy.ShowBackButton = true;
            uxSelectPolicy.BackButtonAsLink = true;
            uxSelectPolicy.BackButtonText = "Try a different search";
        }

        private UserExperience PolicySearchCriteria()
        {
            if (AdvancedSearch) return PolicyAdvancedSearchCriteria();
            return PolicyBasicSearchCriteria();
        }

        private UserExperience PolicyBasicSearchCriteria()
        {
            var uxPolicySearchCriteria = GetUserExperience<UxPAPolicyByIdSearchCriteria>("BasicCriteria", InitializeBasicSearch);
            uxPolicySearchCriteria.ShowBackButton = ShowSearchBackButton;
            uxPolicySearchCriteria.SelectedPolicyId = null;
            return uxPolicySearchCriteria;
        }

        private void InitializeBasicSearch(UxPAPolicyByIdSearchCriteria uxPAPolicyByIdSearchCriteria)
        {
            uxPAPolicyByIdSearchCriteria.ShowSwitchToAdvanced = false;
            uxPAPolicyByIdSearchCriteria.ShowBackButton = ShowSearchBackButton;
        }
        private UserExperience PolicyAdvancedSearchCriteria()
        {
            var uxPolicyAdvSearchCriteria = GetUserExperience<UxPAPolicyAdvSearchCriteria>("AdvancedCriteria", InitializeAdvancedSearch);
            uxPolicyAdvSearchCriteria.ShowBackButton = ShowSearchBackButton;
            uxPolicyAdvSearchCriteria.SelectedPolicyId = null;
            return uxPolicyAdvSearchCriteria;
        }
        private void InitializeAdvancedSearch(UxPAPolicyAdvSearchCriteria uxPAPolicyAdvSearchCriteria)
        {
            uxPAPolicyAdvSearchCriteria.ShowSwitchToBasic = false;
            uxPAPolicyAdvSearchCriteria.ShowBackButton = ShowSearchBackButton;
        }
        private UserExperience SelectPolicy()
        {
            var uxSelectPolicy = GetUserExperience<UxSelectPAPolicy>("Select", InitializeSelectPolicy);
            uxSelectPolicy.ShowBackButton = ShowSelectBackButton;
            uxSelectPolicy.SearchTaskId = SearchTaskId;
            return uxSelectPolicy;
        }

        public override Component GetPrevComponent()
        {
            if (SearchPolicyId.HasValue)
            {
                SearchPolicyId = null;
                SelectedPolicyId = null;
                return PolicySearchCriteria();
            }
            if (SelectedPolicyId.HasValue)
            {
                SelectedPolicyId = null;
                return SelectPolicy();
            }
            if (CriteriaCaptured)
            {
                CriteriaCaptured = false;
                SearchTaskId = null;
                SearchPolicyId = null;
                SelectedPolicyId = null;
                return PolicySearchCriteria();
            }
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "BasicCriteria")
            {
                SearchPolicyId = ((UxPAPolicyByIdSearchCriteria)comp).SelectedPolicyId;
                if (SearchPolicyId.HasValue)
                {
                    SelectedPolicyId = SearchPolicyId;
                }
                else
                {
                    CriteriaCaptured = true;
                    SearchTaskId = ((UxPAPolicyByIdSearchCriteria)comp).TaskId;
                }
            }
            if (comp.ClientRef == "AdvancedCriteria")
            {
                SearchPolicyId = ((UxPAPolicyAdvSearchCriteria)comp).SelectedPolicyId;
                if (SearchPolicyId.HasValue)
                {
                    SelectedPolicyId = SearchPolicyId;
                }
                else
                {
                    CriteriaCaptured = true;
                    SearchTaskId = ((UxPAPolicyAdvSearchCriteria)comp).TaskId;
                }
            }
            if (comp.ClientRef == "Select")
            {
                SelectedPolicyId = ((UxSelectPAPolicy)comp).SelectedPolicyId;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            var args = eventArgs as PAPolicySearchSwitchAdvancedEventArgs;
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