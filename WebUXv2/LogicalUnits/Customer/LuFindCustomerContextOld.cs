//using System;
//using System.Collections.Generic;
//using WebUXv2.Components;
//using WebUXv2.Events;
//using WebUXv2.Events.TaskManager;
//using WebUXv2.UserExperiences;
//using WebUXv2.UserExperiences.Customer;

//namespace WebUXv2.LogicalUnits.Customer
//{
//    [ComponentTitle("Find Customer Context")]
//    //[SingletonComponent]
//    //[LaunchableComponent("temporary")]
//    public class LuFindCustomerContext : LogicalUnit, ISingletonComponent
//    {
//        private bool _showSearchBackButton = true;
//        public int? SelectedCustomerId { get; internal set; } = null;

//        internal bool CriteriaCaptured { get; set; } = false;
//        internal int? SearchTaskId { get; set; } = null;

//        public bool ShowSearchBackButton
//        {
//            get { return _showSearchBackButton; }
//            set
//            {
//                _showSearchBackButton = value;
//                CustomerSearchCriteria().Save(); // Cascades ux ShowBackButton property
//            }
//        }

//        internal const string CMD_SHOW_SEARCH_BACK_BUTTON = "ShowSearchBackButton";


//        public LuFindCustomerContext()
//        {
//            InitialiseState();
//        }
//        public void InitialiseState()
//        {
//            SelectedCustomerId = null;
//            CriteriaCaptured = false;
//            SearchTaskId = null;
//        }

//        public override Component GetNextComponent()
//        {
//            if (!CriteriaCaptured)
//            {
//                return CustomerSearchCriteria();
//            }
//            if (!SelectedCustomerId.HasValue)
//            {
//                var uxSelectCustomer = GetUserExperience<UxSelectCustomer>("Select", InitializeSelectCustomer);
//                uxSelectCustomer.SearchTaskId = SearchTaskId;
//                return uxSelectCustomer;
//            }
//            return null;
//        }

//        private void InitializeSelectCustomer(UxSelectCustomer uxSelectCustomer)
//        {
//            uxSelectCustomer.ShowBackButton = true;
//            uxSelectCustomer.BackButtonAsLink = true;
//            uxSelectCustomer.BackButtonText = "Try a different search";
//        }

//        private UserExperience CustomerSearchCriteria()
//        {
//            var uxCustomerSearchCriteria = GetUserExperience<UxCustomerSearchCriteria>("Criteria");
//            uxCustomerSearchCriteria.ShowBackButton = ShowSearchBackButton;
//            return uxCustomerSearchCriteria;
//        }

//        public override Component GetPrevComponent()
//        {
//            if (SelectedCustomerId.HasValue)
//            {
//                SelectedCustomerId = null;
//                return GetUserExperience<UxSelectCustomer>("Select");
//            }
//            if (CriteriaCaptured)
//            {
//                CriteriaCaptured = false;
//                SearchTaskId = null;
//                return GetUserExperience<UxCustomerSearchCriteria>("Criteria");
//            }
//            return null;
//        }

//        public override void ComponentCompleted(Component comp)
//        {
//            if (comp.ClientRef == "Criteria")
//            {
//                CriteriaCaptured = true;
//                SearchTaskId = comp.TaskId;
//            }
//            if (comp.ClientRef == "Select")
//            {
//                SelectedCustomerId = ((UxSelectCustomer)comp).SelectedCustomerId;
//            }
//        }

//        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
//        {
//            if (eventArgs is BackEventArgs)
//            {
//                //BackEvent logic effectively handled in GetPrevComponent
//            }
//        }

//        public override Dictionary<string, object> State
//        {
//            get
//            {
//                return new Dictionary<string, object>
//                {
//                    {"CriteriaCaptured", CriteriaCaptured}
//                   ,{"SearchTaskId", SearchTaskId}
//                   ,{"SelectedCustomerId", SelectedCustomerId}
//                   ,{"ShowSearchBackButton", ShowSearchBackButton}
//                };
//            }
//            set
//            {
//                CriteriaCaptured = Convert.ToBoolean(value["CriteriaCaptured"]);
//                ShowSearchBackButton = Convert.ToBoolean(value["ShowSearchBackButton"]);

//                if (value["SearchTaskId"] == null)
//                {
//                    SearchTaskId = null;
//                }
//                else
//                {
//                    SearchTaskId = Convert.ToInt32(value["SearchTaskId"]);
//                }

//                if (value["SelectedCustomerId"] == null)
//                {
//                    SelectedCustomerId = null;
//                }
//                else
//                {
//                    SelectedCustomerId = Convert.ToInt32(value["SelectedCustomerId"]);
//                }

//            }
//        }

//    }
//}