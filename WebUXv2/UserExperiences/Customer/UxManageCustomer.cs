using System;
using System.Collections.Generic;
using System.Dynamic;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Customer
{
    [ComponentTitle("Manage Customer")]
    [LaunchableComponent("temporary")]
    [PrimaryActionController("ManageCustomer", "Customer")]
    public class UxManageCustomer : UserExperience
    {

        [ComponentInput("customer")]
        [ComponentState]
        public EntityContext CustomerContext { get; set; }
        //public int? CustomerId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = false;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        //public override ExpandoObject ActionParams()
        //{
        //    dynamic result = new ExpandoObject();
        //    result.customerId = CustomerId;
        //    return result;
        //}

        public Models.Customer LoadCustomer(int id)
        {
            var db = new CustomerDbContext();
            var result = db.Customers.Find(id);

            CustomerContext = CtxMan.SetContext(result.Id, "customer", result.FullName) as EntityContext;

            return result;
        }

    }
}