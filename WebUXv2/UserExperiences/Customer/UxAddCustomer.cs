using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Customer
{
    //[Authorize(Roles = "Admin")] // Also use Authorise attribute on AmendCustomer action method in CustomerController
    [ComponentTitle("* Add Customer Details (ux)")]
    [PrimaryActionController("AddCustomer", "Customer")]
    [LaunchableComponent("temporary")]
    public class UxAddCustomer : UserExperience
    {
        [ComponentState]
        public int? CustomerId { get; set; }

        [ComponentInput()]
        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentInput()]
        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        public Models.Customer NewCustomer()
        {
            return new Models.Customer();
        }

        public void AddCustomer(Models.Customer customer)
        {
            var db = new CustomerDbContext();
            db.Entry(customer).State = EntityState.Added;
            db.SaveChanges();
            CtxMan.SetContext(customer.Id, "customer", customer.FullName);

            SingletonService.Instance.UserMessage = $"Customer {customer.FullName} was added";
        }

    }
}