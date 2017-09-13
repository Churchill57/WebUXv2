using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Policy
{
    [ComponentTitle("Enter Policy Details")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("AddCustomerPolicy", "Policy")]
    public class UxAddCustomerPolicy : UserExperience
    {

        [ComponentInput("customer")]
        public int? CustomerId { get; set; }

        [ComponentState]
        public int? AnnuitantId { get; set; }

        [ComponentInput()]
        [ComponentState]
        public int? DependantId { get; set; }

        [ComponentInput()]
        [ComponentState]
        public int? BeneficiaryId { get; set; }

        [ComponentState]
        private decimal Premium { get; set; }


        internal const string CmdClearDependant = "ClearDep";
        internal const string CmdClearBeneficiary = "ClearBen";

        [ComponentInput()]
        public string Command
        {
            get { return null; }
            set
            {
                if (value.StartsWith(CmdClearDependant, StringComparison.InvariantCultureIgnoreCase))
                {
                    DependantId = null;
                }
                if (value.StartsWith(CmdClearBeneficiary, StringComparison.InvariantCultureIgnoreCase))
                {
                    BeneficiaryId = null;
                }
            }
        }

        [ComponentState]
        public int? PolicyId { get; internal set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        private CustomerDbContext _db = new CustomerDbContext();
        public PAPolicy NewPolicy()
        {
            var annuitantId = AnnuitantId ?? CustomerId;

            var newPolicy = new PAPolicy()
            {
                AnnuitantId = annuitantId.Value
               ,Annuitant = _db.Customers.Find(annuitantId.Value)
               ,DependantId = DependantId
               ,Dependant = _db.Customers.Find(DependantId)
               ,BeneficiaryId = BeneficiaryId
               ,Beneficiary = _db.Customers.Find(BeneficiaryId)
               ,Premium = Premium
            };

            return newPolicy;
        }

        public void SetInterimPolicy(PAPolicy currentPolicy)
        {
            AnnuitantId = currentPolicy.AnnuitantId;
            DependantId = currentPolicy.DependantId;
            BeneficiaryId = currentPolicy.BeneficiaryId;
            Premium = currentPolicy.Premium;
        }


        public void AddPolicy(Models.PAPolicy paPolicy)
        {
            _db.Entry(paPolicy).State = EntityState.Added;
            _db.SaveChanges();

            SetContexts(paPolicy);

            SingletonService.Instance.UserMessage = $"Policy {paPolicy.Id} was created";
        }

        private void SetContexts(PAPolicy paPolicy)
        {
            var ecm = SingletonService.Instance.EntityContextManager;
            var policyContext = ecm.SetContext(paPolicy.Id, "policy", paPolicy.Description);

            // N.B. customers are added to entity context in the order which leaves the annuitant as the latest customer context!

            if (paPolicy.BeneficiaryId != null)
            {
                var beneficiary = _db.Customers.Find(paPolicy.BeneficiaryId);
                var beneficiaryContext = ecm.SetContext(beneficiary.Id, "customer", beneficiary.FullName);
                ecm.RemoveDirectRelationship(paPolicy.Id, "policy", "Beneficiary");
                ecm.SetDirectRelationship(policyContext, "Beneficiary", beneficiaryContext);
            }

            if (paPolicy.DependantId != null)
            {
                var dependant = _db.Customers.Find(paPolicy.DependantId);
                var dependantContext = ecm.SetContext(dependant.Id, "customer", dependant.FullName);
                ecm.RemoveDirectRelationship(paPolicy.Id, "policy", "Dependant");
                ecm.SetDirectRelationship(policyContext, "Dependant", dependantContext);
            }

            var annuitant = _db.Customers.Find(paPolicy.AnnuitantId);
            var annuitantContext = ecm.SetContext(annuitant.Id, "customer", annuitant.FullName);
            ecm.RemoveDirectRelationship(paPolicy.Id, "policy", "Annuitant");
            ecm.SetDirectRelationship(policyContext, "Annuitant", annuitantContext);

            // Policy context rules since this UX is about amending the policy.
            ecm.SetContext(policyContext);

        }


    }
}