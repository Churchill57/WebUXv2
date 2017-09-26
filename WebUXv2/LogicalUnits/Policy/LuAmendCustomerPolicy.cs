using WebUXv2.Components;
using WebUXv2.Events.TaskManager;
using WebUXv2.LogicalUnits.Customer;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.UserExperiences.Policy;

namespace WebUXv2.LogicalUnits.Policy
{
    [ComponentTitle("Amend Policy")]
    [LaunchableComponent("customer amend policy change edit modify")]
    [SecondaryActionController("CustomerPolicies","Policy")]

    public class LuAmendCustomerPolicy : LogicalUnit
    {

        [LaunchInput]
        [ComponentInput("policy")]
        [ComponentState]
        public EntityContext PolicyContext { get; set; }

        [ComponentState]
        private string NextClientRef { get; set; } = "Policy";


        [ComponentState]
        private int? uxAmendCustomerPolicyTaskId { get; set; } = null;

        public override Component GetNextComponent()
        {
            if (NextClientRef == "Policy")
            {
                var uxAmendCustomerPolicy = GetUserExperience<UxAmendCustomerPolicy>("Policy");
                uxAmendCustomerPolicyTaskId = uxAmendCustomerPolicy.TaskId;
                return uxAmendCustomerPolicy;
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            if (NextClientRef == "Policy")
            {
                if (PolicyContext == null) return null;

                return GetUserExperience<UxAmendCustomerPolicy>("Policy");
            }
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Policy")
            {
                PolicyContext = ((UxAmendCustomerPolicy)comp).PolicyContext;
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
                }
                if (eventArgs.Component is LuFindCustomerContext)
                {
                    
                }
            }
        }

    }
}