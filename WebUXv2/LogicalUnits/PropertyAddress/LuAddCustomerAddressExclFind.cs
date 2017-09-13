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
    [ComponentTitle("Add Customer Address (excl find)")]
    [LaunchableComponent("address add customer insert create record")]
    [SecondaryActionController("CustomerAddresses","PropertyAddress")]

    public class LuAddCustomerAddressExclFind : LogicalUnit
    {

        //[LaunchInput]
        //[ComponentInput("customer")]
        //[ComponentState]
        //public int ? CustomerId { get; set; } = null;

        //[LaunchInput]
        //[ComponentInput("customer")]
        //[ComponentState]
        //public EntityContext CustomerContext { get; set; }

        [ComponentState]
        public int? AddressId { get; set; } = null;
        public override Component GetNextComponent()
        {
            if (!AddressId.HasValue)
            {
                return GetUserExperience<UxAddCustomerAddress>("Address");
                //var uxAddCustomerAddress = GetUserExperience<UxAddCustomerAddress>("Address");
                //uxAddCustomerAddress.CustomerContext = CustomerContext;
                //return uxAddCustomerAddress;
            }
            return null;
        }

        public override Component GetPrevComponent()
        {
            if (AddressId.HasValue)
            {
                AddressId = null;
                return GetUserExperience<UxAddCustomerAddress>("Address");
            }
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Address")
            {
                AddressId = ((UxAddCustomerAddress)comp).AddressId;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                //BackEvent logic effectively handled in GetPrevComponent
            }
        }

    }
}