using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Interfaces;

namespace WebUXv2.UserExperiences.Customer
{
    [ComponentTitle("Enter Customer Advanced Search Criteria")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("CaptureAdvancedSearchCriteria", "Customer")]
    public class UxCustomerAdvSearchCriteria : UserExperience, IUxPerformSearch<Models.Customer>
    {

        //[ComponentInput("customer")]
        //public int? CustomerId { get; set; }
        [ComponentState]
        public CustomerAdvancedSearchCriteria Criteria { get; set; } = new CustomerAdvancedSearchCriteria();

        private EntityContext _selectedCustomerContext;
        //private int? _selectedCustomerId;

        [ComponentState]
        public EntityContext SelectedCustomerContext
        {
            get { return _selectedCustomerContext; }
            set
            {
                _selectedCustomerContext = value;
                if (_selectedCustomerContext!=null) CtxMan.SetContext(_selectedCustomerContext);
            }
        }

        [ComponentState]
        public bool ShowSwitchToBasic { get; set; } = false;

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";


        public void ResetFields()
        {
            Criteria = new CustomerAdvancedSearchCriteria();
        }

        public ComponentEventArgs SetAdvancedSearch(bool advancedSearch)
        {
            return new CustomerSearchSwitchAdvancedEventArgs(this, advancedSearch);
        }

        public IEnumerable<Models.Customer> PerformSearch()
        {
            var db = new CustomerDbContext();
            if (String.IsNullOrEmpty(Criteria.FirstName) && String.IsNullOrEmpty(Criteria.Surname))
            {
                return db.Customers.ToList();
            }
            return db.Customers.Where(c => c.FirstName.Contains(Criteria.FirstName)
                                || c.LastName.Contains(Criteria.Surname)).ToList();
        }

        //public BackEventArgs Back()
        //{
        //    return new BackEventArgs(this);
        //}

    }
}