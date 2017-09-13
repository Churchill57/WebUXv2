﻿using System;
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
    [Authorize(Roles = "Admin")] // Also use Authorise attribute on AmendCustomer action method in CustomerController
    [ComponentTitle("Amend Customer Details")]
    [PrimaryActionController("AmendCustomer", "Customer")]
    ////[LaunchableComponent("temporary")]
    public class UxAmendCustomer : UserExperience
    {

        [ComponentInput("customer")]
        [ComponentState]
        public EntityContext CustomerContext { get; set; }
        //public int? CustomerId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        //public override ExpandoObject ActionParams() // TODO: Do we need ExpandoObject ActionParams() anymore?
        //{
        //    dynamic result = new ExpandoObject();
        //    result.customerId = CustomerId;
        //    return result;
        //}

        public Models.Customer LoadCustomer(int id)
        {
            var db = new CustomerDbContext();
            var result = db.Customers.Find(id);
            if (result !=null) CtxMan.SetContext(result.Id, "customer", result.FullName);
            return result;
        }

        public void AmendCustomer(Models.Customer customer)
        {
            var db = new CustomerDbContext();
            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();
            CustomerContext = CtxMan.SetContext(customer.Id, "customer", customer.FullName) as EntityContext;

            SingletonService.Instance.UserMessage = $"Customer {customer.FullName} was amended";
        }

        //public BackEventArgs Back()
        //{
        //    return new BackEventArgs(this);
        //}

    }
}