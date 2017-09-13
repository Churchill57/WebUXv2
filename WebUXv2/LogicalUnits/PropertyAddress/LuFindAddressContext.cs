using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.PropertyAddress
{
    [ComponentTitle("Find Address Context")]
    //[SingletonComponent]
    //[LaunchableComponent("temporary")]
    public class LuFindAddressContext : LogicalUnit, ISingletonComponent
    {

        [ComponentState]
        private string NextStep { get; set; } = "CheckAddressContext";

        [ComponentState]
        private int? SelectedAddressId { get; set; } = null;

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        private IEntityContextManager _cm = SingletonService.Instance.EntityContextManager;
      public LuFindAddressContext()
        {
            InitialiseState();
        }
        public void InitialiseState()
        {
            NextStep = "CheckAddressContext";
        }


        public override Component GetNextComponent()
        {
            var addressContext = _cm.ResolveContext("address");
            if (addressContext != null)
            {
                SelectedAddressId = addressContext.Id;
                return null;
            }

            var customerContext = _cm.ResolveContext("customer");
            if (customerContext != null)
            {
                // TODO: 
            }

            SingletonService.Instance.UserMessage = "Address context indeterminate";
            return null;

            //if (NextStep == "CheckAddressContext")
            //{
            //    var addressContext = _cm.ResolveContext("address");
            //    if (addressContext != null)
            //    {
            //        SelectedAddressId = addressContext.Id;
            //        return null;
            //    }

            //}

            //return null;
        }


        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }

        public override void ComponentCompleted(Component comp)
        {
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
        }

        public bool AddressExists(int? id)
        {
            CustomerDbContext db = new CustomerDbContext();
            var address = db.PropertyAddresses.Find(id);
            return (address != null);
        }


    }
}