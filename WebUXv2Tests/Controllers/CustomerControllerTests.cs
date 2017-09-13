using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using WebUXv2.Components;
using WebUXv2.Controllers;
using WebUXv2.Events;
using WebUXv2.Events.Customer;
using WebUXv2.UserExperiences;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2Tests.Controllers
{
    [TestClass()]
    public class CustomerControllerTests
    {
        private ITaskManager _tm;
        private ITaskManagerController _tmc;
        private CustomerController _target;

        [TestInitialize()]
        public void Initialise()
        {
            _tm = Substitute.For<ITaskManager>();
            _tmc = Substitute.For<ITaskManagerController>();
            _target = new CustomerController(_tm, _tmc);
        }

        [TestMethod()]
        public void SwitchToAdvancedSearchEventRaisedFromBasicSearch()
        {
            var dontcare = 0;
            _tm.GetUserExperience(dontcare).ReturnsForAnyArgs(x => new UxCustomerSearchCriteria());
            _target.SwitchToAdvancedSearch(dontcare);
            _tmc.Received().RedirectAfterComponentEvent(Arg.Is<CustomerSearchSwitchAdvancedEventArgs>(e => e.AdvancedSearch == true));
        }

        [TestMethod()]
        public void SwitchToBasicSearchEventRaisedFromAdvancedSearch()
        {
            var dontcare = 0;
            _tm.GetUserExperience(dontcare).ReturnsForAnyArgs(x => new UxCustomerAdvSearchCriteria());
            _target.SwitchToBasicSearch(dontcare);
            _tmc.Received().RedirectAfterComponentEvent(Arg.Is<CustomerSearchSwitchAdvancedEventArgs>(e => e.AdvancedSearch == false));
        }

        //[TestMethod()]
        //public void SwitchToBasicSearchTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CaptureAdvancedSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SaveAdvancedSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ResetAdvancedSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CaptureAdvancedSearchCriteriaTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CaptureSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SaveSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ResetSearchCriteriaTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CaptureSearchCriteriaTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SelectCustomerFromSearchResultTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SelectedCustomerFromSearchResultTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void PreviewCustomerTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CustomerPreviewedTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ManageCustomerTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CustomerManagedTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void IndexTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DetailsTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CreateTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CreateTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void EditTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void EditTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void AmendCustomerTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void AmendCustomerTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ResetAmendedCustomerTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DeleteConfirmedTest()
        //{
        //    Assert.Fail();
        //}
    }
}