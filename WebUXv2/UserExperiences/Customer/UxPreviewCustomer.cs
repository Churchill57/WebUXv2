using System;
using System.Collections.Generic;
using System.Dynamic;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Customer
{
    [ComponentTitle("Preview Customer")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("PreviewCustomer", "Customer")]
    public class UxPreviewCustomer : UserExperience
    {

        [ComponentInput("customer")]
        [ComponentState]
        public EntityContext CustomerContext { get; set; }
        //public int? CustomerId { get; set; }

        [ComponentState]
        public bool PreviewDifferentCustomer { get; set; } = false;

        [ComponentState]
        public bool ShowBackButton { get; set; } = false;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public bool BackButtonAsLink { get; set; } = false;

        [ComponentState]
        public string DoneButtonText { get; set; } = "Close";

        //public override ExpandoObject ActionParams()
        //{
        //    dynamic result = new ExpandoObject();
        //    result.customerId = CustomerId; // TODO: Consider doing via reflection and the ContextInputAttribute.
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