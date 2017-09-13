using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Interfaces;

namespace WebUXv2.UserExperiences.Policy
{
    [ComponentTitle("Enter Policy Advanced Search Criteria")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("CaptureAdvancedSearchCriteria", "Policy")]
    public class UxPAPolicyAdvSearchCriteria : UserExperience, IUxPerformSearch<Models.PAPolicy>
    {

        //[ComponentInput("Policy")]
        //public int? PolicyId { get; set; }
        [ComponentState]
        public PAPolicyAdvancedSearchCriteria Criteria { get; set; } = new PAPolicyAdvancedSearchCriteria();

        private int? _selectedPolicyId;

        [ComponentState]
        public int? SelectedPolicyId
        {
            get { return _selectedPolicyId; }
            set
            {
                _selectedPolicyId = value;
                if (_selectedPolicyId.HasValue) CtxMan.SetContext(_selectedPolicyId.Value, "policy", null);
            }
        }

        [ComponentState]
        public bool ShowSwitchToBasic { get; set; } = false;

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        public void ResetFields()
        {
            Criteria = new PAPolicyAdvancedSearchCriteria();
        }

        public ComponentEventArgs SetAdvancedSearch(bool advancedSearch)
        {
            return new PAPolicySearchSwitchAdvancedEventArgs(this, advancedSearch);
        }

        public IEnumerable<Models.PAPolicy> PerformSearch()
        {
            var db = new CustomerDbContext();
            return db.PAPolicies.ToList();
        }

    }
}