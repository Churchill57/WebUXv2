using System;
using System.Linq;
using System.Reflection.Emit;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.LogicalUnits.Customer;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Customer;
using WebUXv2.UserExperiences.Interfaces;

namespace WebUXv2.Controllers
{
    //[Authorize]
    public class CustomerController : Controller
    {
        private readonly ITaskManager _tm;
        private readonly ITaskManagerController _tmc;

        public CustomerController() : this( new TaskManager(new TaskDbContext()), new TaskManagerController()) { }
        public CustomerController( ITaskManager tm, ITaskManagerController tmc)
        {
            _tm = tm;
            _tmc = tmc;
        }

        ~CustomerController() { ((IDisposable)_tmc).Dispose(); }

        /// <summary>
        /// Provides a local reference to the task manager controller which has the necessary controller context setup.
        /// </summary>
        /// <returns></returns>
        private ITaskManagerController Tmc()
        {
            if (_tmc.ControllerContext == null)
            {
                // Essential to make User context (for example) available to the task manager controller.
                _tmc.ControllerContext = new ControllerContext(this.Request.RequestContext, (ControllerBase) _tmc);

                // Ideally the controller context would be passed from the constructor of this controller into
                // the task manager controller constructor. However, the 'this' keyword cannot be passed as a
                // parameter in this way.
                // i.e. public CustomerController() : this( new TaskManager(), new TaskManagerController(this))
                // does not allow the passing of 'this' -------------------------------------------------^ 
            }
            return _tmc;
        }
                         
        public ActionResult SwitchToAdvancedSearch(int uxTaskId)
        {
            var uxSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerSearchCriteria;
            if (uxSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var uxEvent = uxSearchForCustomer.SetAdvancedSearch(true);

            return Tmc().RedirectAfterComponentEvent(uxEvent);
        }

        public ActionResult SwitchToBasicSearch(int uxTaskId)
        {
            var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerAdvSearchCriteria;
            if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var uxEvent = uxAdvancedSearchForCustomer.SetAdvancedSearch(false);

            return Tmc().RedirectAfterComponentEvent(uxEvent);
        }

        public ActionResult CaptureAdvancedSearchCriteria(int uxTaskId, [Bind(Include = "Firstname,Surname,Age,NINO,Town,PostCode")] CustomerAdvancedSearchCriteria customerAdvancedSearchCriteria)
        {
            var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerAdvSearchCriteria;
            if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (customerAdvancedSearchCriteria.FirstName != null) uxAdvancedSearchForCustomer.Criteria.FirstName = customerAdvancedSearchCriteria.FirstName;
            if (customerAdvancedSearchCriteria.Surname != null) uxAdvancedSearchForCustomer.Criteria.Surname = customerAdvancedSearchCriteria.Surname;
            if (customerAdvancedSearchCriteria.Age != null) uxAdvancedSearchForCustomer.Criteria.Age = customerAdvancedSearchCriteria.Age;
            if (customerAdvancedSearchCriteria.NINO != null) uxAdvancedSearchForCustomer.Criteria.NINO = customerAdvancedSearchCriteria.NINO;
            if (customerAdvancedSearchCriteria.Town != null) uxAdvancedSearchForCustomer.Criteria.Town = customerAdvancedSearchCriteria.Town;
            if (customerAdvancedSearchCriteria.PostCode != null) uxAdvancedSearchForCustomer.Criteria.PostCode = customerAdvancedSearchCriteria.PostCode;

            uxAdvancedSearchForCustomer.Save();

            var model = new CustomerAdvancedSearchCriteria
            {
                FirstName = uxAdvancedSearchForCustomer.Criteria.FirstName,
                Surname = uxAdvancedSearchForCustomer.Criteria.Surname,
                Age = uxAdvancedSearchForCustomer.Criteria.Age,
                NINO = uxAdvancedSearchForCustomer.Criteria.NINO,
                Town = uxAdvancedSearchForCustomer.Criteria.Town,
                PostCode = uxAdvancedSearchForCustomer.Criteria.PostCode,
            };

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowSwitchToBasic = uxAdvancedSearchForCustomer.ShowSwitchToBasic;
            ViewBag.ShowBackButton = uxAdvancedSearchForCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxAdvancedSearchForCustomer.BackButtonText;

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult SaveAdvancedSearchCriteria(int? uxTaskId, [Bind(Include = "Firstname,Surname,Age,NINO,Town,PostCode")] CustomerAdvancedSearchCriteria customerAdvancedSearchCriteria)
        {
            if (uxTaskId != null)
            {
                var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId.Value) as UxCustomerAdvSearchCriteria;
                if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

                uxAdvancedSearchForCustomer.Criteria.FirstName = customerAdvancedSearchCriteria.FirstName;
                uxAdvancedSearchForCustomer.Criteria.Surname = customerAdvancedSearchCriteria.Surname;
                uxAdvancedSearchForCustomer.Criteria.Age = customerAdvancedSearchCriteria.Age;
                uxAdvancedSearchForCustomer.Criteria.NINO = customerAdvancedSearchCriteria.NINO;
                uxAdvancedSearchForCustomer.Criteria.Town = customerAdvancedSearchCriteria.Town;
                uxAdvancedSearchForCustomer.Criteria.PostCode = customerAdvancedSearchCriteria.PostCode;

                uxAdvancedSearchForCustomer.Save();
            }

            return new EmptyResult();
        }

        public ActionResult ResetAdvancedSearchCriteria(int uxTaskId)
        {
            var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerAdvSearchCriteria;
            if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAdvancedSearchForCustomer.ResetFields();
            uxAdvancedSearchForCustomer.Save();

            return Tmc().RedirectToUserExperience(uxAdvancedSearchForCustomer);
        }

        // POST: CustomerSearch/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult CaptureAdvancedSearchCriteria(int uxTaskId, [Bind(Include = "Firstname,Surname,Age,NINO,Town,PostCode")] CustomerAdvancedSearchCriteria customerAdvancedSearchCriteria, FormCollection form)
        {
            if (!ModelState.IsValid) return View(customerAdvancedSearchCriteria);

            var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerAdvSearchCriteria;
            if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAdvancedSearchForCustomer.SelectedCustomerContext = null;
            uxAdvancedSearchForCustomer.Criteria.FirstName = customerAdvancedSearchCriteria.FirstName;
            uxAdvancedSearchForCustomer.Criteria.Surname = customerAdvancedSearchCriteria.Surname;
            uxAdvancedSearchForCustomer.Criteria.Age = customerAdvancedSearchCriteria.Age;
            uxAdvancedSearchForCustomer.Criteria.NINO = customerAdvancedSearchCriteria.NINO;
            uxAdvancedSearchForCustomer.Criteria.Town = customerAdvancedSearchCriteria.Town;
            uxAdvancedSearchForCustomer.Criteria.PostCode = customerAdvancedSearchCriteria.PostCode;

            uxAdvancedSearchForCustomer.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxAdvancedSearchForCustomer);
        }

        public ActionResult RecentCustomerSelectedFromAdvancedSearch(int uxTaskId, int customerId)
        {
            var uxAdvancedSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerAdvSearchCriteria;
            if (uxAdvancedSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAdvancedSearchForCustomer.SelectedCustomerContext = new EntityContext(customerId,"customer",null);
            //uxAdvancedSearchForCustomer.SelectedCustomerId = customerId;
            uxAdvancedSearchForCustomer.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxAdvancedSearchForCustomer);
        }

        public ActionResult CaptureSearchCriteria(int uxTaskId, [Bind(Include = "Name,DOB")] CustomerSearchCriteria customerSearchCriteria) //, CustomerSearchCriteria customerSearchCriteria)
        {
            var uxSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerSearchCriteria;
            if (uxSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (customerSearchCriteria.Name != null) uxSearchForCustomer.CustomerName = customerSearchCriteria.Name;
            if (customerSearchCriteria.DOB.HasValue) uxSearchForCustomer.CustomerDateOfBirth = customerSearchCriteria.DOB;

            uxSearchForCustomer.Save();

            var model = new CustomerSearchCriteria
            {
                Name = uxSearchForCustomer.CustomerName,
                DOB = uxSearchForCustomer.CustomerDateOfBirth
            };

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowSwitchToAdvanced = uxSearchForCustomer.ShowSwitchToAdvanced;
            ViewBag.ShowBackButton = uxSearchForCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxSearchForCustomer.BackButtonText;
            ViewBag.SearchButtonText = uxSearchForCustomer.SearchButtonText;

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult SaveSearchCriteria(int? uxTaskId, [Bind(Include = "Name,DOB")] CustomerSearchCriteria customerSearchCriteria)
        {
            if (uxTaskId != null)
            {
                var uxSearchForCustomer = _tm.GetUserExperience(uxTaskId.Value) as UxCustomerSearchCriteria;
                if (uxSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

                uxSearchForCustomer.CustomerName = customerSearchCriteria.Name;
                uxSearchForCustomer.CustomerDateOfBirth = customerSearchCriteria.DOB;

                uxSearchForCustomer.Save();
            }

            return new EmptyResult();
        }

        public ActionResult ResetSearchCriteria(int uxTaskId)
        {
            var uxSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerSearchCriteria;
            if (uxSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxSearchForCustomer.ResetFields();
            uxSearchForCustomer.Save();

            return Tmc().RedirectToUserExperience(uxSearchForCustomer);
        }

        // POST: CustomerSearch/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult CaptureSearchCriteria(int uxTaskId,[Bind(Include = "Name,DOB")] CustomerSearchCriteria customerSearchCriteria, FormCollection form)
        {
            if (!ModelState.IsValid) return View(customerSearchCriteria);

            var uxSearchForCustomer = _tm.GetUserExperience(uxTaskId) as UxCustomerSearchCriteria;
            if (uxSearchForCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxSearchForCustomer.CustomerName = customerSearchCriteria.Name;
            uxSearchForCustomer.CustomerDateOfBirth = customerSearchCriteria.DOB;

            uxSearchForCustomer.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxSearchForCustomer);
        }

        public ActionResult SelectCustomerFromSearchResult(int uxTaskId)
        {
            var uxSelectCustomer = _tm.GetUserExperience(uxTaskId) as UxSelectCustomer;
            if (uxSelectCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var searchTaskId = uxSelectCustomer.SearchTaskId;
            if (searchTaskId == null) return HttpNotFound($"User Experience with id {uxTaskId} has a null SearchTaskId. No customer search Performed.");

            var uxPerformCustomerSearch = _tm.GetUserExperience(searchTaskId.Value) as IUxPerformSearch<Customer>;
            if (uxPerformCustomerSearch == null) return  HttpNotFound($"User Experience with id {searchTaskId} does not implement IPerformCustomerSearch.");

            var model = uxPerformCustomerSearch.PerformSearch().ToList();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxSelectCustomer.ShowBackButton;
            ViewBag.BackButtonAsLink = uxSelectCustomer.BackButtonAsLink;
            ViewBag.BackButtonText = uxSelectCustomer.BackButtonText;
            ViewBag.SelectButtonText = uxSelectCustomer.SelectButtonText;
            ViewBag.ShowPreviewLink = uxSelectCustomer.ShowPreviewLink;

            return View(model);
        }

        public ActionResult SelectedCustomerFromSearchResult(int uxTaskId, int customerId, string fullName)
        {
            var uxSelectCustomer = _tm.GetUserExperience(uxTaskId) as UxSelectCustomer;
            if (uxSelectCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxSelectCustomer.SetCustomer(customerId, fullName);
            uxSelectCustomer.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxSelectCustomer);
        }

        public ActionResult PreviewCustomer(int uxTaskId)
        {
            var uxPreviewCustomer = _tm.GetUserExperience(uxTaskId) as UxPreviewCustomer;
            if (uxPreviewCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (uxPreviewCustomer.CustomerContext==null) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer context.");
            //if (!uxPreviewCustomer.CustomerId.HasValue) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer id.");

            var customerId = uxPreviewCustomer.CustomerContext.Id;

            var model = uxPreviewCustomer.LoadCustomer(customerId);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.PreviewDifferentCustomer = uxPreviewCustomer.PreviewDifferentCustomer;
            ViewBag.ShowBackButton = uxPreviewCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxPreviewCustomer.BackButtonText;
            ViewBag.BackButtonAsLink = uxPreviewCustomer.BackButtonAsLink;
            ViewBag.DoneButtonText = uxPreviewCustomer.DoneButtonText;

            return View(model);
        }

        public ActionResult ManageCustomer(int uxTaskId)
        {
            var uxManageCustomer = _tm.GetUserExperience(uxTaskId) as UxManageCustomer;
            if (uxManageCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (uxManageCustomer.CustomerContext==null) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer context.");

            // Customer context description may have changed since uxManageCustomer was last hydrated & run
            //TODO: Consider doing this automatically whenever any component is hydrated and has entity context properties which may have out of date descriptions.
            var uxManageCustomerContextKey = EntityContextManager.ContextKey(uxManageCustomer.CustomerContext);
            var cm = SingletonService.Instance.EntityContextManager;
            var prevailingContext = cm.GetContext(uxManageCustomerContextKey);
            var prevailingContextKey = EntityContextManager.ContextKey(cm.GetContext(uxManageCustomerContextKey));
            if (uxManageCustomerContextKey == prevailingContextKey)
            {
                uxManageCustomer.CustomerContext.Description = prevailingContext.Description;
                uxManageCustomer.Save();
            }

            var model = uxManageCustomer.LoadCustomer(uxManageCustomer.CustomerContext.Id);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxManageCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxManageCustomer.BackButtonText;

            return View(model);
        }

        public ActionResult ResetCustomerContextInput(int uxTaskId)
        {
            var ux = _tm.GetUserExperience(uxTaskId);
            if (ux == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            foreach (var propertyInfo in ux.GetType().GetProperties())
            {
                var attr = propertyInfo.GetCustomAttributes(typeof(ComponentInputAttribute), false);
                if (attr.Length == 1)
                {
                    propertyInfo.SetValue(ux, null);
                    ux.Save();
                }
            }

            // TODO: Need to raise a context changhed event?
            return Tmc().ResumeTask(uxTaskId, true);

        }

        //[Authorize(Roles = "Admin")] // Also use Authorise attribute on UxAmendCustomer class
        public ActionResult AddCustomer(int uxTaskId)
        {
            var uxAddCustomer = _tm.GetUserExperience(uxTaskId) as UxAddCustomer;
            if (uxAddCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = uxAddCustomer.NewCustomer();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxAddCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxAddCustomer.BackButtonText;

            return View(model);
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddCustomer(int uxTaskId, [Bind(Include = "Id,Title,FirstName,LastName,DOB,NINO")] Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            var uxAddCustomer = _tm.GetUserExperience(uxTaskId) as UxAddCustomer;
            if (uxAddCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAddCustomer.AddCustomer(customer);

            return Tmc().RedirectAfterUserExperienceCompleted(uxAddCustomer);
        }


        [Authorize(Roles = "Admin")] // Also use Authorise attribute on UxAmendCustomer class
        public ActionResult AmendCustomer(int uxTaskId)
        {
            var uxAmendCustomer = _tm.GetUserExperience(uxTaskId) as UxAmendCustomer;
            if (uxAmendCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (uxAmendCustomer.CustomerContext==null) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer context.");
            //if (!uxAmendCustomer.CustomerId.HasValue) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer id.");

            var customerId = uxAmendCustomer.CustomerContext.Id;

            var model = uxAmendCustomer.LoadCustomer(customerId);

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxAmendCustomer.ShowBackButton;
            ViewBag.BackButtonText = uxAmendCustomer.BackButtonText;

            return View(model);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AmendCustomer(int uxTaskId, [Bind(Include = "Id,Title,FirstName,LastName,DOB,NINO")] Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            var uxAmendCustomer = _tm.GetUserExperience(uxTaskId) as UxAmendCustomer;
            if (uxAmendCustomer == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAmendCustomer.AmendCustomer(customer);

            return Tmc().RedirectAfterUserExperienceCompleted(uxAmendCustomer);
        }

        public ActionResult Capture2ndLineDefenceQuestions(int uxTaskId)
        {
            var uxCapture2ndLineDefenceQuestions = _tm.GetUserExperience(uxTaskId) as UxCapture2ndLineDefenceQuestions;
            if (uxCapture2ndLineDefenceQuestions == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (uxCapture2ndLineDefenceQuestions.CustomerContext==null) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer context.");
            //if (!uxCapture2ndLineDefenceQuestions.CustomerId.HasValue) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer id.");

            //var customerContext = uxCapture2ndLineDefenceQuestions.CustomerContext;

            var model = uxCapture2ndLineDefenceQuestions.PrepareNewQuestion();

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxCapture2ndLineDefenceQuestions.ShowBackButton;
            ViewBag.BackButtonText = uxCapture2ndLineDefenceQuestions.BackButtonText;

            //var customer = uxCapture2ndLineDefenceQuestions.LoadCustomer(customerContext.Id);
            //ViewBag.customerContext = customerContext;

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Capture2ndLineDefenceQuestions(int uxTaskId, SecondLineDefenceQuestion secondLineDefenceQuestion)
        {

            if (!ModelState.IsValid) return View(secondLineDefenceQuestion);

            var uxCapture2ndLineDefenceQuestions = _tm.GetUserExperience(uxTaskId) as UxCapture2ndLineDefenceQuestions;
            if (uxCapture2ndLineDefenceQuestions == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxCapture2ndLineDefenceQuestions.SaveQuestion(secondLineDefenceQuestion);

            return Tmc().RedirectAfterUserExperienceCompleted(uxCapture2ndLineDefenceQuestions);

        }

        public ActionResult CustomerSecondLineDefenceQuestions(string componentName, string rootComponentName)
        {
            var cm = SingletonService.Instance.EntityContextManager;
            var customerContext = cm.ResolveContext("customer");
            if (customerContext == null) return new EmptyResult();

            var db = new CustomerDbContext();
            var addresses = db.SecondLineDefenceQuestions.Where(a => a.CustomerId == customerContext.Id);

            ViewBag.ComponentName = componentName;
            ViewBag.RootComponentName = rootComponentName;

            return PartialView(addresses);
        }

    }
}
