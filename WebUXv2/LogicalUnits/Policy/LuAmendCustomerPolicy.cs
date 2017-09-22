using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.LogicalUnits.Customer;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Policy;
using WebUXv2.UserExperiences.PropertyAddress;

namespace WebUXv2.LogicalUnits.Policy
{
    [ComponentTitle("Amend Policy (excl find)")]
    [LaunchableComponent("customer amend policy change edit modify")]
    [SecondaryActionController("CustomerPolicies","Policy")]

    public class LuAmendCustomerPolicy : LogicalUnit
    {

        [LaunchInput]
        [ComponentInput("policy")]
        [ComponentState]
        public EntityContext PolicyContext { get; set; }
        //public int? PolicyId { get; set; } = null;

        [ComponentState]
        private string NextClientRef { get; set; } = "Policy";


        [ComponentState]
        private int? uxAmendCustomerPolicyTaskId { get; set; } = null;

        public override Component GetNextComponent()
        {
            //if (!PolicyId.HasValue)
            if (NextClientRef == "Policy")
            {
                //if (PolicyId.HasValue) SingletonService.Instance.UserMessage = "";
                var uxAmendCustomerPolicy = GetUserExperience<UxAmendCustomerPolicy>("Policy", InitializeAmendCustomerPolicy);
                uxAmendCustomerPolicyTaskId = uxAmendCustomerPolicy.TaskId;
                return uxAmendCustomerPolicy;
            }
            return null;
        }

        private void InitializeAmendCustomerPolicy(UxAmendCustomerPolicy uxAmendCustomerPolicy)
        {
            //uxAmendCustomerPolicy.AnnuitantId = CustomerId;
        }

        public override Component GetPrevComponent()
        {
            if (NextClientRef == "Policy")
            {
                if (PolicyContext == null) return null;

                //PolicyId = null;
                //if (PolicyId.HasValue) SingletonService.Instance.UserMessage = "";
                return GetUserExperience<UxAmendCustomerPolicy>("Policy");
            }
            //SingletonService.Instance.UserMessage = "";
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Policy")
            {
                PolicyContext = ((UxAmendCustomerPolicy)comp).PolicyContext;
                //SingletonService.Instance.UserMessage = "";
                NextClientRef = null;
            }

            var luLauncher = comp as LuLauncher;
            if (luLauncher == null) return;

            if (luLauncher.ReturnTaskId == uxAmendCustomerPolicyTaskId)
            {
                if (luLauncher.ComponentName == typeof(LuFindCustomer).Name)
                {
                    var luFindCustomer = luLauncher.GetRootComponent() as LuFindCustomer;

                    var uxAmendCustomerPolicy = GetUserExperience<UxAmendCustomerPolicy>("Policy");
                    if (luLauncher.ReturnTaskRef == "annuitant")
                    {
                        uxAmendCustomerPolicy.AnnuitantId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Annuitant changed";
                    }
                    if (luLauncher.ReturnTaskRef == "dependant")
                    {
                        uxAmendCustomerPolicy.DependantId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Dependant changed";
                    }
                    if (luLauncher.ReturnTaskRef == "beneficiary")
                    {
                        uxAmendCustomerPolicy.BeneficiaryId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Beneficiary changed";
                    }
                    uxAmendCustomerPolicy.Save();
                }
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (eventArgs.Component.ClientRef == "Policy")
                {
                    PolicyContext = null;
                    //NextClientRef = null;
                }
                if (eventArgs.Component is LuFindCustomerContext)
                {
                    
                }
            }
        }

    }
}