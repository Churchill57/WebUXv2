using System;
using System.Linq;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.LogicalUnits.Customer;
using WebUXv2.Models;
using WebUXv2.UserExperiences.PropertyAddress;

namespace WebUXv2.Controllers
{
    //[Authorize]
    public class PropertyAddressController : Controller
    {
        private readonly ITaskManager _tm;
        private readonly ITaskManagerController _tmc;

        public PropertyAddressController() : this(new TaskManager(), new TaskManagerController()) { }
        public PropertyAddressController(ITaskManager tm, ITaskManagerController tmc)
        {
            _tm = tm;
            _tmc = tmc;
        }

        ~PropertyAddressController() { ((IDisposable)_tmc).Dispose(); }

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

        //[Authorize(Roles = "Admin")]
        public ActionResult AddCustomerAddress(int uxTaskId)
        {
            var uxAddCustomerAddress = _tm.GetUserExperience(uxTaskId) as UxAddCustomerAddress;
            if (uxAddCustomerAddress == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            if (uxAddCustomerAddress.CustomerContext == null) return HttpNotFound($"User Experience with id {uxTaskId} has a null customer context.");

            var model = new PropertyAddress() { CustomerId = uxAddCustomerAddress.CustomerContext.Id };

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.customerName = uxAddCustomerAddress.CustomerContext.Description;
            ViewBag.ShowBackButton = uxAddCustomerAddress.ShowBackButton;
            ViewBag.BackButtonText = uxAddCustomerAddress.BackButtonText;

            return View(model);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddCustomerAddress(int uxTaskId, [Bind(Include = "Id,Line1,Line2,Line3,PostCode,CustomerId")] PropertyAddress propertyAddress)
        {
            if (!ModelState.IsValid) return View(propertyAddress);

            var uxAddCustomerAddress = _tm.GetUserExperience(uxTaskId) as UxAddCustomerAddress;
            if (uxAddCustomerAddress == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAddCustomerAddress.AddAddress(propertyAddress);

            uxAddCustomerAddress.AddressId = propertyAddress.Id;
            uxAddCustomerAddress.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxAddCustomerAddress);
        }

        public ActionResult CustomerAddresses(string componentName, string rootComponentName)
        {

            var cm = SingletonService.Instance.EntityContextManager;
            var customerContext = cm.ResolveContext("customer");
            if (customerContext == null) return new EmptyResult();

            var db = new CustomerDbContext();
            var addresses = db.PropertyAddresses.Where(a => a.CustomerId == customerContext.Id);

            ViewBag.ComponentName = componentName;
            ViewBag.RootComponentName = rootComponentName;
            ViewBag.CustomerContext = customerContext;

            return PartialView(addresses);
        }

        public ActionResult AmendCustomerAddress(int uxTaskId)
        {
            var uxAmendCustomerAddress = _tm.GetUserExperience(uxTaskId) as UxAmendCustomerAddress;
            if (uxAmendCustomerAddress == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            var addressId = uxAmendCustomerAddress.AddressContext.Id;
            var model = uxAmendCustomerAddress.LoadAddress(addressId);

            var customer = uxAmendCustomerAddress.LoadCustomer(model.CustomerId);
            ViewBag.customerName = customer?.FullName ?? "unknown";

            ViewBag.uxTaskId = uxTaskId;
            ViewBag.ShowBackButton = uxAmendCustomerAddress.ShowBackButton;
            ViewBag.BackButtonText = uxAmendCustomerAddress.BackButtonText;

            return View(model);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AmendCustomerAddress(int uxTaskId, [Bind(Include = "Id,Line1,Line2,Line3,PostCode,CustomerId")] PropertyAddress propertyAddress)
        {
            if (!ModelState.IsValid) return View(propertyAddress);

            var uxAmendCustomerAddress = _tm.GetUserExperience(uxTaskId) as UxAmendCustomerAddress;
            if (uxAmendCustomerAddress == null) return HttpNotFound($"User Experience with id {uxTaskId} not found.");

            uxAmendCustomerAddress.AmendAddress(propertyAddress);

            uxAmendCustomerAddress.AddressContext = new EntityContext(propertyAddress.Id,"address",propertyAddress.CommasSeparated);
            uxAmendCustomerAddress.Save();

            return Tmc().RedirectAfterUserExperienceCompleted(uxAmendCustomerAddress);
        }

    }
}
