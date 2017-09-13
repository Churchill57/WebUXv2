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
using System.Data.Entity;

namespace WebUXv2.UserExperiences.Policy
{
    [ComponentTitle("Customer Search by Policy")]
    //[Authorize(Roles = "Admin, SuperUser")]
    //[LaunchableComponent("temporary")]
    [PrimaryActionController("CaptureSearchCriteriaByPolicy", "Policy")]
    // TODO: Strictly this should be under the policy/PolicyController
    public class UxCustomerByPolicySearchCriteria : UserExperience, IUxPerformSearch<Models.Customer>
    {

        [ComponentState]
        public PAPolicySearchCriteria Criteria { get; set; } = new PAPolicySearchCriteria();

        //[ComponentInput("policy")]
        //public int? PolicyId
        //{
        //    get { return Criteria.Id; }
        //    set { Criteria.Id = value; }
        //}

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public string SearchButtonText { get; set; } = "Search";

        public bool PolicyExists(int? id)
        {
            CustomerDbContext db = new CustomerDbContext();
            var policy = db.PAPolicies.Find(id);
            return (policy != null);
        }

        public IEnumerable<Models.Customer> PerformSearch()
        {
            CustomerDbContext db = new CustomerDbContext();
            var policy = db.PAPolicies.Find(Criteria.Id);

            // TODO: Strictly shouldn't need to test for policy existence here?
            if (policy == null)
            {
                SingletonService.Instance.UserMessage = $"Policy {Criteria.Id} does not exist";
                return null;
            }

            var customers = new List<Models.Customer>();

            //var ecm = SingletonService.Instance.EntityContextManager;
            //var policyContext = ecm.SetContext(policy.Id, "policy", policy.Description);

            // N.B. customers are added to entity context in the order which leaves the annuitant as the latest!
            // But customers are returned with annuitant first

            if (policy.Beneficiary != null)
            {
                //var beneficiaryContext = ecm.SetContext(policy.Beneficiary.Id, "customer", policy.Beneficiary.FullName);
                //ecm.SetDirectRelationship(policyContext, "Beneficiary", beneficiaryContext);
                policy.Beneficiary.Cargo = "Beneficiary";
                customers.Insert(0, policy.Beneficiary);
            }

            if (policy.Dependant != null)
            {
                //var dependantContext = ecm.SetContext(policy.Dependant.Id, "customer", policy.Dependant.FullName);
                //ecm.SetDirectRelationship(policyContext, "Dependant", dependantContext);
                policy.Dependant.Cargo = "Dependant" ;
                customers.Insert(0, policy.Dependant);
            }

            //var annuitantContext = ecm.SetContext(policy.Annuitant.Id, "customer", policy.Annuitant.FullName);
            //ecm.SetDirectRelationship(policyContext, "Annuitant", annuitantContext);
            policy.Annuitant.Cargo = "Annuitant";
            customers.Insert(0, policy.Annuitant);

            return customers;
        }

        //public BackEventArgs Back()
        //{
        //    return new BackEventArgs(this);
        //}

    }
}