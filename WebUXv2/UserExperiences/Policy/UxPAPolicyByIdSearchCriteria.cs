using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Interfaces;

namespace WebUXv2.UserExperiences.Policy
{
    [ComponentTitle("Enter Policy Search Criteria")]
    //[Authorize(Roles = "Admin, SuperUser")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("CaptureSearchCriteriaById", "Policy")]
    public class UxPAPolicyByIdSearchCriteria : UserExperience, IUxPerformSearch<Models.PAPolicy>
    {

        //[ComponentInput("Policy")]
        //public int? PolicyId { get; set;}
        [ComponentState]
        public int? PolicyId { get; set; }

        [ComponentState]
        public int? SelectedPolicyId { get; set; }

        [ComponentState]
        public bool ShowSwitchToAdvanced { get; set; } = false;

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public string SearchButtonText { get; set; } = "Search";

        public bool SetSelectedPolicyId(int? id)
        {
            if (id != null)
            {
                var policy = LoadPolicy(id);
                if (policy != null)
                {
                    SelectedPolicyId = id;
                    PolicyId = id;
                    CtxMan.SetContext(policy.Id, "policy", policy.Description);
                    return true;
                }
                else
                {
                    SingletonService.Instance.UserMessage = $"Policy {id} does not exist";
                }
            }
            return false;
        }

        public PAPolicySearchSwitchAdvancedEventArgs SetAdvancedSearch(bool advancedSearch)
        {
            return new PAPolicySearchSwitchAdvancedEventArgs(this, advancedSearch);
        }

        public IEnumerable<PAPolicy> PerformSearch()
        {
            var policy = LoadPolicy(PolicyId);
            var result = new[] { policy };
            return result;
        }

        private PAPolicy LoadPolicy(int? id)
        {
            var db = new CustomerDbContext();
            var result = db.PAPolicies.Find(id);
            return result;
        }

        //}

    }
}