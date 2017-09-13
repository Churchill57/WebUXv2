using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;

namespace WebUXv2.UserExperiences.Policy
{
    [ComponentTitle("Select Policy")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("SelectPolicyFromSearchResult", "Policy")]
    public class UxSelectPAPolicy : UserExperience
    {

        private int? _selectedPolicyId;

        [ComponentState]
        public int? SearchTaskId { get; set; }

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
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public bool BackButtonAsLink { get; set; } = false;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public string SelectButtonText { get; set; } = "Select";

        [ComponentState]
        public bool ShowPreviewLink { get; set; } = false;

        public void SetCustomer(int id, string fullName)
        {
            _selectedPolicyId = id;
            CtxMan.SetContext(id, "policy", fullName);
        }

    }
}