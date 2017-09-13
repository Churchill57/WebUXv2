using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Interfaces;

namespace WebUXv2.UserExperiences.Customer
{
    [ComponentTitle("Enter Customer Search Criteria")]
    //[Authorize(Roles = "Admin, SuperUser")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("CaptureSearchCriteria", "Customer")]
    public class UxCustomerSearchCriteria : UserExperience, IUxPerformSearch<Models.Customer>
    {

        //[ComponentInput("customer")]
        //public int? CustomerId { get; set;}
        [ComponentState]
        public string CustomerName { get; set; }

        [ComponentState]
        public DateTime? CustomerDateOfBirth { get; set; }

        [ComponentState]
        public bool ShowSwitchToAdvanced { get; set; } = false;

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public string SearchButtonText { get; set; } = "Search";

        public void ResetFields()
        {
            CustomerName = null;
            CustomerDateOfBirth = null;
        }

        public CustomerSearchSwitchAdvancedEventArgs SetAdvancedSearch(bool advancedSearch)
        {
            return new CustomerSearchSwitchAdvancedEventArgs(this, advancedSearch);
        }

        public IEnumerable<Models.Customer> PerformSearch()
        {
            var db = new CustomerDbContext();
            if (String.IsNullOrEmpty(CustomerName))
            {
                return db.Customers.ToList();
            }

            var result = new List<Models.Customer>();
            foreach (var c in db.Customers)
            {
                if (c.FullName.ToLower().Contains(CustomerName.ToLower()))
                {
                    result.Add(c);
                }
            }
            return result;
        }

        //public BackEventArgs Back()
        //{
        //    return new BackEventArgs(this);
        //}

    }
}