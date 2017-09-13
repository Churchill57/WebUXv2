using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.PropertyAddress
{
    [ComponentTitle("Amend Address Details")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("AmendCustomerAddress", "PropertyAddress")]
    public class UxAmendCustomerAddress : UserExperience
    {

        [ComponentInput()]
        [ComponentState]
        public EntityContext CustomerContext { get; set; }
        //public int? CustomerId { get; set; }

        [ComponentInput("address")]
        [ComponentState]
        public EntityContext AddressContext { get; internal set; }
        //public int? AddressId { get; internal set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";


        public Models.Customer LoadCustomer(int customerId)
        {
            var db = new CustomerDbContext();
            var customer = db.Customers.Find(customerId);
            if (customer != null) CtxMan.SetContext(customer.Id, "customer", customer.FullName);
            return customer;
        }

        public Models.PropertyAddress LoadAddress(int addressId)
        {
            var db = new CustomerDbContext();
            var address = db.PropertyAddresses.Find(addressId);
            if (address != null)
            {
                var addressContext = CtxMan.SetContext(address.Id, "address", address.CommasSeparated);
                var customerContext = CtxMan.SetContext(address.CustomerId, "customer", null);
                CtxMan.SetDirectRelationship(customerContext, "domicile", addressContext);
            }

            return address;
        }

        public void AmendAddress(Models.PropertyAddress address)
        {
            var db = new CustomerDbContext();
            db.Entry(address).State = EntityState.Modified;
            db.SaveChanges();

            //var customerContext = CtxMan.SetContext(address.CustomerId, "customer", null);
            var addressContext =  CtxMan.SetContext(address.Id, "address", address.CommasSeparated);
            //CtxMan.SetDirectRelationship(customerContext, "main domicile", addressContext);

            SingletonService.Instance.UserMessage = $"Address {address.CommasSeparated} was amended";

        }

    }
}