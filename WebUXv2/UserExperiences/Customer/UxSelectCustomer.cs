using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;

namespace WebUXv2.UserExperiences.Customer
{
    [ComponentTitle("Select Customer")]
    ////[LaunchableComponent("temporary")]
    [PrimaryActionController("SelectCustomerFromSearchResult", "Customer")]
    public class UxSelectCustomer : UserExperience
    {

        [ComponentState]
        public int? SearchTaskId { get; set; }

        [ComponentState]
        internal EntityContext SelectedCustomerContext { get; set; }

        //[ComponentState]
        //internal int? SelectedCustomerId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public bool BackButtonAsLink { get; set; } = false;

        [ComponentState]
        public string BackButtonText { get; set; } = "Back";

        [ComponentState]
        public string SelectButtonText { get; set; } = "Select";

        [ComponentState]
        public bool ShowPreviewLink { get; set; } = false;

        public void SetCustomer(int id, string fullName)
        {
            //SelectedCustomerId = id;
            SelectedCustomerContext = CtxMan.SetContext(id, "customer", fullName) as EntityContext;
        }

    }
}