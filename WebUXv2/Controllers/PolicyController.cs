using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.LogicalUnits.Policy;
using WebUXv2.Models;
using WebUXv2.UserExperiences.Customer;
using WebUXv2.UserExperiences.Policy;

namespace WebUXv2.Controllers
{
    public class PolicyController : Controller
    {

        private readonly ITaskManager _tm;
        private readonly ITaskManagerController _tmc;

        public PolicyController() : this( new TaskManager(new TaskDbContext()), new TaskManagerController()) { }
        public PolicyController(ITaskManager tm, ITaskManagerController tmc)
        {
            _tm = tm;
            _tmc = tmc;
        }

        ~PolicyController() { ((IDisposable)_tmc).Dispose(); }

        /// <summary>
        /// Provides a local reference to the task manager controller which has the necessary controller context setup.
        /// </summary>
        /// <returns></returns>
        private ITaskManagerController Tmc()
        {
            if (_tmc.ControllerContext == null)
            {
                // Essential to make User context (for example) available to the task manager controller.
                _tmc.ControllerContext = new ControllerContext(this.Request.RequestContext, (ControllerBase)_tmc);

                // Ideally the controller context would be passed from the constructor of this controller into
                // the task manager controller constructor. However, the 'this' keyword cannot be passed as a
                // parameter in this way.
                // i.e. public CustomerController() : this( new TaskManager(), new TaskManagerController(this))
                // does not allow the passing of 'this' -------------------------------------------------^ 
            }
            return _tmc;
        }

        public ActionResult CaptureSearchCriteriaById(int uxTaskId, int? id)
        {
            var uxPAPolicyByIdSearchCriteria = _tm.GetUserExperience(uxTaskId) as UxPAPolicyByIdSearchCriteria;
            if (uxPAPolicyByIdSearchCriteria == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var model = new PAPolicySearchCriteria
            {
                Id = uxPAPolicyByIdSearchCriteria.PolicyId,
            };

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxPAPolicyByIdSearchCriteria.ShowBackButton;
            ViewBag.BackButtonText = uxPAPolicyByIdSearchCriteria.BackButtonText;
            ViewBag.SearchButtonText = uxPAPolicyByIdSearchCriteria.SearchButtonText;

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult CaptureSearchCriteriaById(int uxTaskId, int? id, FormCollection form)
        {
            var uxPAPolicyByIdSearchCriteria = _tm.GetUserExperience(uxTaskId) as UxPAPolicyByIdSearchCriteria;
            if (uxPAPolicyByIdSearchCriteria == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxPAPolicyByIdSearchCriteria.ShowBackButton;
            ViewBag.BackButtonText = uxPAPolicyByIdSearchCriteria.BackButtonText;
            ViewBag.SearchButtonText = uxPAPolicyByIdSearchCriteria.SearchButtonText;

            if (!ModelState.IsValid) return View();

            if (!uxPAPolicyByIdSearchCriteria.SetSelectedPolicyId(id)) return View();

            uxPAPolicyByIdSearchCriteria.PolicyId = id;
            uxPAPolicyByIdSearchCriteria.SelectedPolicyId = id;
            uxPAPolicyByIdSearchCriteria.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxPAPolicyByIdSearchCriteria);
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult AddCustomerPolicy(int uxTaskId)
        {
            var uxAddCustomerPolicy = _tm.GetUserExperience(uxTaskId) as UxAddCustomerPolicy;
            if (uxAddCustomerPolicy == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var paPolicy = uxAddCustomerPolicy.NewPolicy();

            ViewBag.uxTaskId = uxTaskId;

            ViewBag.ShowBackButton = uxAddCustomerPolicy.ShowBackButton;
            ViewBag.BackButtonText = uxAddCustomerPolicy.BackButtonText;

            return View(paPolicy);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddCustomerPolicy(int uxTaskId, [Bind(Include = "Premium, AnnuitantId, DependantId, BeneficiaryId")] PAPolicy paPolicy, string redirectUrl)
        {
            if (!ModelState.IsValid) { return View(paPolicy);}

            var uxAddCustomerPolicy = _tm.GetUserExperience(uxTaskId) as UxAddCustomerPolicy;
            if (uxAddCustomerPolicy == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                uxAddCustomerPolicy.SetInterimPolicy(paPolicy);
                uxAddCustomerPolicy.Save();
                return Redirect(redirectUrl);
            }
            else
            {
                uxAddCustomerPolicy.AddPolicy(paPolicy);
                uxAddCustomerPolicy.PolicyId = paPolicy.Id;
                uxAddCustomerPolicy.Save();
            }

            return Tmc().RedirectAfterUserExperienceCompleted(uxAddCustomerPolicy);
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult AmendCustomerPolicy(int uxTaskId)
        {
            var uxAmendCustomerPolicy = _tm.GetUserExperience(uxTaskId) as UxAmendCustomerPolicy;
            if (uxAmendCustomerPolicy == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAmendCustomerPolicy.SetInterimPolicy(null);
            var paPolicy = uxAmendCustomerPolicy.GetInterimPolicy();
            uxAmendCustomerPolicy.Save();

            ViewBag.uxTaskId = uxTaskId;

            ViewBag.ShowBackButton = uxAmendCustomerPolicy.ShowBackButton;
            ViewBag.BackButtonText = uxAmendCustomerPolicy.BackButtonText;

            return View(paPolicy);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AmendCustomerPolicy(int uxTaskId, [Bind(Include = "Id, Premium, AnnuitantId, DependantId, BeneficiaryId")] PAPolicy paPolicy, string redirectUrl)
        {
            if (!ModelState.IsValid) { return View(paPolicy); }

            var uxAmendCustomerPolicy = _tm.GetUserExperience(uxTaskId) as UxAmendCustomerPolicy;
            if (uxAmendCustomerPolicy == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                uxAmendCustomerPolicy.SetInterimPolicy(paPolicy);
                uxAmendCustomerPolicy.Save();
                return Redirect(redirectUrl);
            }
            else
            {
                uxAmendCustomerPolicy.AmendPolicy(paPolicy);
                //uxAmendCustomerPolicy.PolicyId = paPolicy.Id;
                uxAmendCustomerPolicy.Save();
            }

            return Tmc().RedirectAfterUserExperienceCompleted(uxAmendCustomerPolicy);
        }


        public ActionResult CustomerPolicies(string componentName, string rootComponentName)
        {

            var cm = SingletonService.Instance.EntityContextManager;
            var customerContext = cm.ResolveContext("customer");
            if (customerContext == null) return new EmptyResult();

            var db = new CustomerDbContext();
            var customerPolicies = new List<PAPolicy>();
            foreach (var policy in db.PAPolicies)
            {
                if (policy.AnnuitantId == customerContext.Id)
                {
                    customerPolicies.Add(policy);
                    policy.Cargo = "(annuitant)";
                }
                if (policy.DependantId == customerContext.Id)
                {
                    customerPolicies.Add(policy);
                    policy.Cargo = "(dependant)";
                }
                if (policy.BeneficiaryId == customerContext.Id)
                {
                    customerPolicies.Add(policy);
                    policy.Cargo = "(beneficiary)";
                }
            }

            ViewBag.ComponentName = componentName;
            ViewBag.RootComponentName = rootComponentName;

            return PartialView(customerPolicies);
        }


        public ActionResult RecentPolicySelectedFromSearchById(int uxTaskId, int policyId)
        {
            var uxPAPolicyByIdSearchCriteria = _tm.GetUserExperience(uxTaskId) as UxPAPolicyByIdSearchCriteria;
            if (uxPAPolicyByIdSearchCriteria == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxPAPolicyByIdSearchCriteria.SelectedPolicyId = policyId;
            uxPAPolicyByIdSearchCriteria.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxPAPolicyByIdSearchCriteria);
        }

    }
}
