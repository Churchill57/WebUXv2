using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebUXv2.Components;

namespace WebUXv2Tests.Components
{
    [TestClass()]
    public class EntityContextManagerTests
    {
        private IEntityContextManager _target;

        [TestInitialize()]
        public void Initialise()
        {
            _target = new EntityContextManager();
        }

        [TestMethod()]
        public void GetNonExiststentContextShouldReturnNull()
        {
            var contextGet = _target.GetContext(123999, "blahblah");

            Assert.IsNull(contextGet);
        }

        [TestMethod()]
        public void InitialCurrentContextShouldReturnNull()
        {
            var contextGet = _target.GetCurrentContext;

            Assert.IsNull(contextGet);
        }

        [TestMethod()]
        public void SetThenGetContextShouldBeTheSame()
        {
            var contextSet = _target.SetContext(1, "customer", "Mr Fred Smith1");

            var contextGet = _target.GetContext(1, "Customer");

            Assert.AreEqual(contextSet, contextGet);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void SetThenGetCurrentContextShouldBeTheSame()
        {
            var contextSet = _target.SetContext(2, "Customer", "Mr Fred Smith2");

            var contextCurrent = _target.GetCurrentContext;

            Assert.AreEqual(contextSet, contextCurrent);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void SetUpdatedContextThenGetContextShouldBeTheSame()
        {
            _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var contextSet = _target.SetContext(1, "Customer", "Mr Fred Smith1b"); // Description was updated.

            var contextGet = _target.GetContext(1, "Customer");

            Assert.AreEqual(contextSet, contextGet);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void SetUpdatedContextThenGetCurrentContextShouldBeTheSame()
        {
            _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var contextSet = _target.SetContext(1, "Customer", "Mr Fred Smith1b"); // Description was updated.

            var contextCurrent = _target.GetCurrentContext;

            Assert.AreEqual(contextSet, contextCurrent);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void GetCurrentContextShouldBeLastContextSet()
        {
            _target.SetContext(1, "Customer", "Mr Fred Smith1");
            _target.SetContext(2, "Customer", "Mr Fred Smith2");
            _target.SetContext(1001, "Address", "123 Aciacia Avenue");
            _target.SetContext(9000, "Policy", "GIFL Policy 9000");
            var contextSet = _target.SetContext(567, "FundElement", "£9,275");

            var contextCurrent = _target.GetCurrentContext;

            Assert.AreEqual(contextSet, contextCurrent);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void ReturnContextsLatestFirst()
        {
            var context1 = _target.SetContext(1, "Customer", "Mr Fred Smith1");
            Thread.Sleep(1); // Ensure sufficient delay between internally set context timestamps
            var context2 = _target.SetContext(2, "Customer", "Mr Fred Smith2");
            Thread.Sleep(1);
            var context3 = _target.SetContext(1001, "Address", "123 Aciacia Avenue");
            Thread.Sleep(1);
            var context4 = _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            var setContexts = new List<IEntityContext> {context4, context3, context2, context1};

            var getContexts = _target.GetContexts().ToList();

            CollectionAssert.AreEqual(setContexts, getContexts);
        }

        [TestMethod()]
        public void NoContextsFollowingClear()
        {
            _target.SetContext(1, "Customer", "Mr Fred Smith1");
            _target.SetContext(2, "Customer", "Mr Fred Smith2");
            _target.SetContext(1001, "Address", "123 Aciacia Avenue");
            _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            _target.Clear();

            var getContexts = _target.GetContexts();
            var contextCurrent = _target.GetCurrentContext;

            Assert.AreEqual(0, getContexts.Count());
            Assert.IsNull(contextCurrent);
        }

        [TestMethod()]
        public void SetContextWithNullDescriptionStillsSetsContext()
        {
            var contextSet = _target.SetContext(1, "customer", null);

            var contextGet = _target.GetContext(1, "Customer");

            Assert.AreEqual(contextSet, contextGet);
            Assert.IsTrue(contextSet.WhenSet <= DateTime.Now);
        }

        [TestMethod()]
        public void SetContextWithNullDescriptionDoesNotOverwriteExistingContext()
        {
            var contextSet1 = _target.SetContext(1, "customer", "Mr Fred Smith");
            var contextSet2 = _target.SetContext(1, "customer", null);

            var contextGet = _target.GetContext(1, "Customer");

            Assert.AreEqual(contextSet1, contextGet);
            Assert.IsTrue(contextSet1.WhenSet <= DateTime.Now);
        }


        [TestMethod()]
        public void SetThenGetContextRelationshipShouldBeTheSame()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000");
            var relationshipSet = _target.SetDirectRelationship(ctxPolicy, "policyholder", ctxCustomer);

            var relationshipGet = _target.GetDirectRelationships(ctxPolicy).First();
            Assert.AreEqual(relationshipSet.Name, relationshipGet.Name);
            Assert.AreEqual(relationshipSet.EntityContext.Name, relationshipGet.EntityContext.Name);
            Assert.AreEqual(relationshipSet.EntityContext.Id, relationshipGet.EntityContext.Id);
            Assert.AreEqual(relationshipSet.EntityContext.Description, relationshipGet.EntityContext.Description);
        }

        [TestMethod()]
        public void GetNamedRelationships()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            var dependantSet = _target.SetDirectRelationship(ctxPolicy, "dependanT", ctxCustomer);
            var beneficiarySet = _target.SetDirectRelationship(ctxPolicy, "beneficiary", ctxCustomer);

            var dependantGet = _target.GetDirectRelationships(ctxPolicy, "dependant").First();
            Assert.AreEqual(dependantSet.Name, dependantGet.Name);
            Assert.AreEqual(dependantSet.EntityContext.Name, dependantGet.EntityContext.Name);
            Assert.AreEqual(dependantSet.EntityContext.Id, dependantGet.EntityContext.Id);
            Assert.AreEqual(dependantSet.EntityContext.Description, dependantGet.EntityContext.Description);

            var beneficiaryGet = _target.GetDirectRelationships(ctxPolicy, "Beneficiary").First();
            Assert.AreEqual(beneficiarySet.Name, beneficiaryGet.Name);
            Assert.AreEqual(beneficiarySet.EntityContext.Name, beneficiaryGet.EntityContext.Name);
            Assert.AreEqual(beneficiarySet.EntityContext.Id, beneficiaryGet.EntityContext.Id);
            Assert.AreEqual(beneficiarySet.EntityContext.Description, beneficiaryGet.EntityContext.Description);
        }

        [TestMethod()]
        public void GetNamedRelatedContext()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var ctxFundElement = _target.SetContext(567, "FundElement", "£9,275");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            var dependantSet = _target.SetDirectRelationship(ctxPolicy, "dependanT", ctxCustomer);
            _target.SetDirectRelationship(ctxPolicy, "fundelement", ctxFundElement);
            var beneficiarySet = _target.SetDirectRelationship(ctxPolicy, "beneficiary", ctxCustomer);

            var dependantGet = _target.GetDirectRelationships(ctxPolicy, null, "customer").First();
            Assert.AreEqual(dependantSet.Name, dependantGet.Name);
            Assert.AreEqual(dependantSet.EntityContext.Name, dependantGet.EntityContext.Name);
            Assert.AreEqual(dependantSet.EntityContext.Id, dependantGet.EntityContext.Id);
            Assert.AreEqual(dependantSet.EntityContext.Description, dependantGet.EntityContext.Description);

            var beneficiaryGet = _target.GetDirectRelationships(ctxPolicy, null, "customer").Skip(1).First();
            Assert.AreEqual(beneficiarySet.Name, beneficiaryGet.Name);
            Assert.AreEqual(beneficiarySet.EntityContext.Name, beneficiaryGet.EntityContext.Name);
            Assert.AreEqual(beneficiarySet.EntityContext.Id, beneficiaryGet.EntityContext.Id);
            Assert.AreEqual(beneficiarySet.EntityContext.Description, beneficiaryGet.EntityContext.Description);
        }

        [TestMethod()]
        public void ContextRelationshipShouldBeConsistentInReverse()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            var relationshipSet = _target.SetDirectRelationship(ctxPolicy, "policyholder", ctxCustomer);

            var relationshipGetReverse = _target.GetDirectRelationships(ctxCustomer).First();
            Assert.AreEqual(relationshipGetReverse.Name, relationshipSet.Name);
            Assert.AreEqual(relationshipGetReverse.EntityContext.Id, 9000);
            Assert.AreEqual(relationshipGetReverse.EntityContext.Name, "policy");
            Assert.AreEqual(relationshipGetReverse.EntityContext.Description, "GIFL Policy 9000");
        }

        [TestMethod()]
        public void UpdatingContextDescriptionShouldNotAffectRelationship()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1a");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000");

            var relationshipSet = _target.SetDirectRelationship(ctxCustomer, "policyholder", ctxPolicy);

            ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith1b");

            var relationshipGet = _target.GetDirectRelationships(ctxCustomer).First();
            Assert.AreEqual(relationshipGet.Name, relationshipSet.Name);
            Assert.AreEqual(relationshipGet.EntityContext.Id, 9000);
            Assert.AreEqual(relationshipGet.EntityContext.Name, "policy");
            Assert.AreEqual(relationshipGet.EntityContext.Description, "GIFL Policy 9000");
        }

        [TestMethod()]
        public void ResolveContextMatchesCurrentContext()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith");

            var ctxResolved = _target.ResolveContext("customer");

            Assert.AreEqual(ctxCustomer.Name, ctxResolved.Name);
            Assert.AreEqual(ctxCustomer.Id, ctxResolved.Id);
            Assert.AreEqual(ctxCustomer.Description, ctxResolved.Description);

        }

        [TestMethod()]
        public void ResolveContextDirectlyRelatedCurrentContext()
        {
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith");
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000"); // <-- Current context is always the last one set!
            _target.SetDirectRelationship(ctxCustomer, "policyholder", ctxPolicy);

            var ctxResolved = _target.ResolveContext("customer");

            Assert.AreEqual(ctxCustomer.Name, ctxResolved.Name);
            Assert.AreEqual(ctxCustomer.Id, ctxResolved.Id);
            Assert.AreEqual(ctxCustomer.Description, ctxResolved.Description);

        }

        [TestMethod()]
        public void ResolveContextIndirectlyRelatedCurrentContext()
        {
            var ctxAddress = _target.SetContext(99, "address", "123 Acacia Drive");
            var ctxCustomer = _target.SetContext(1, "Customer", "Mr Fred Smith");
            _target.SetDirectRelationship(ctxCustomer, "livesat", ctxAddress);
            var ctxPolicy = _target.SetContext(9000, "Policy", "GIFL Policy 9000"); // <-- Current context is always the last one set!
            _target.SetDirectRelationship(ctxCustomer, "policyholder", ctxPolicy);

            var ctxResolved = _target.ResolveContext("address");

            Assert.AreEqual(ctxAddress.Name, ctxResolved.Name);
            Assert.AreEqual(ctxAddress.Id, ctxResolved.Id);
            Assert.AreEqual(ctxAddress.Description, ctxResolved.Description);

        }

    }
}