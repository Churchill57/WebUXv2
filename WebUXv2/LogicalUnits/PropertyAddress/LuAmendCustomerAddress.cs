using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.PropertyAddress;

namespace WebUXv2.LogicalUnits.PropertyAddress
{
    [ComponentTitle("Amend Address")]
    [LaunchableComponent("address amend update change")]
    [SecondaryActionController("CustomerAddresses", "PropertyAddress")]

    public class LuAmendCustomerAddress : LogicalUnit
    {
        [LaunchInput]
        [ComponentInput("address")]
        [ComponentState]
        public EntityContext AddressContext { get; set; }
        //public int? AddressId { get; set; } = null;

        [ComponentState]
        private string NextStep { get; set; } = "Address";

        public override Component GetNextComponent()
        {
            if (NextStep == "Address")
            {
                if (AddressContext!=null)
                {
                    return GetUserExperience<UxAmendCustomerAddress>("Address");
                    //var uxAmendCustomerAddress = GetUserExperience<UxAmendCustomerAddress>("Address");
                    //uxAmendCustomerAddress.AddressId = AddressId;
                    //return uxAmendCustomerAddress;
                   
                }
                //SingletonService.Instance.UserMessage = "No address context"; // TODO: Probably should be done automatically instead of calling GetNextComponent
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Address")
            {
                NextStep = null;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (eventArgs.Component.ClientRef == "Address")
                {
                    NextStep = null;
                }
                //BackEvent logic effectively handled in GetPrevComponent
            }
        }

    }
}