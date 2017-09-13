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
    [ComponentTitle("Create Policy (excl find)")]
    [LaunchableComponent("customer add policy insert create record")]
    [SecondaryActionController("CustomerPolicies","Policy")]

    public class LuAddCustomerPolicy : LogicalUnit
    {

        [LaunchInput]
        [ComponentInput("customer")]
        [ComponentState]
        public int ? CustomerId { get; set; } = null;

        [ComponentState]
        public int? PolicyId { get; set; } = null;

        [ComponentState]
        private string NextClientRef { get; set; } = "Policy";

        [ComponentState]
        private int? uxAddCustomerPolicyTaskId { get; set; } = null;

        public LuAddCustomerPolicy()
        {
            if (!CustomerId.HasValue) SingletonService.Instance.UserMessage = "Choose annuitant for new policy";
        }
        public override Component GetNextComponent()
        {
            //if (!PolicyId.HasValue)
            if (NextClientRef == "Policy")
            {
                if (CustomerId.HasValue) SingletonService.Instance.UserMessage = "";
                var uxAddCustomerPolicy = GetUserExperience<UxAddCustomerPolicy>("Policy", InitializeAddCustomerPolicy);
                uxAddCustomerPolicyTaskId = uxAddCustomerPolicy.TaskId;
                return uxAddCustomerPolicy;
            }
            return null;
        }

        private void InitializeAddCustomerPolicy(UxAddCustomerPolicy uxAddCustomerPolicy)
        {
            uxAddCustomerPolicy.AnnuitantId = CustomerId;
        }

        public override Component GetPrevComponent()
        {
            if (NextClientRef == "Policy")
            {
                PolicyId = null;
                if (CustomerId.HasValue) SingletonService.Instance.UserMessage = "";
                return GetUserExperience<UxAddCustomerPolicy>("Policy");
            }
            SingletonService.Instance.UserMessage = "";
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Policy")
            {
                PolicyId = ((UxAddCustomerPolicy)comp).PolicyId;
                SingletonService.Instance.UserMessage = "";
                NextClientRef = null;
            }

            var luLauncher = comp as LuLauncher;
            if (luLauncher == null) return;

            if (luLauncher.ReturnTaskId == uxAddCustomerPolicyTaskId)
            {
                if (luLauncher.ComponentName == typeof(LuFindCustomer).Name)
                {
                    var luFindCustomer = luLauncher.GetRootComponent() as LuFindCustomer;

                    var uxAddCustomerPolicy = GetUserExperience<UxAddCustomerPolicy>("Policy");
                    if (luLauncher.ReturnTaskRef == "annuitant")
                    {
                        uxAddCustomerPolicy.AnnuitantId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Annuitant changed";
                    }
                    if (luLauncher.ReturnTaskRef == "dependant")
                    {
                        uxAddCustomerPolicy.DependantId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Dependant changed";
                    }
                    if (luLauncher.ReturnTaskRef == "beneficiary")
                    {
                        uxAddCustomerPolicy.BeneficiaryId = luFindCustomer.SelectedCustomerContext.Id;
                        SingletonService.Instance.UserMessage = "Beneficiary changed";
                    }
                    uxAddCustomerPolicy.Save();
                }
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (eventArgs.Component.ClientRef == "Policy")
                {
                    NextClientRef = null;
                }
                if (eventArgs.Component is LuFindCustomerContext)
                {
                    
                }
            }
        }

    }
}