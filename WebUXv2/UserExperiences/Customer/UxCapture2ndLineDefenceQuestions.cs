using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Customer
{
    //[Authorize(Roles = "Admin")] // Also use Authorise attribute on AmendCustomer action method in CustomerController
    [LaunchableComponent("capture record save customer 2nd second line second-line defence defense question quest")]
    [ComponentTitle("Record answers to 2nd Line Defence Questions")]
    [PrimaryActionController("Capture2ndLineDefenceQuestions", "Customer")]
    public class UxCapture2ndLineDefenceQuestions : UserExperience
    {

        [ComponentInput("customer")]
        [ComponentState]
        public EntityContext CustomerContext { get; set; }
        //public int? CustomerId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        //public Models.Customer LoadCustomer(int customerId)
        //{
        //    var db = new CustomerDbContext();
        //    var customer = db.Customers.Find(customerId);
        //    if (customer != null) CtxMan.SetContext(customer.Id, "customer", customer.FullName);
        //    return customer;
        //}


        public Models.SecondLineDefenceQuestion PrepareNewQuestion()
        {
            var question = new SecondLineDefenceQuestion()
            {
                CustomerId = CustomerContext.Id
               ,DateAsked = DateTime.Now
                ,
            };
            return question;
        }

        public void SaveQuestion(Models.SecondLineDefenceQuestion question)
        {
            var db = new CustomerDbContext();
            db.Entry(question).State = EntityState.Added;
            db.SaveChanges();
            var customerContext = CtxMan.SetContext(question.CustomerId, "customer", null);
            var questionContext = CtxMan.SetContext(question.Id, "2ndLineDefenceQuestion", question.Description);
            CtxMan.SetDirectRelationship(customerContext, "2ndLineDefenceQs", questionContext);

            SingletonService.Instance.UserMessage = $"Answers to 2nd line defence questions {questionContext.Description} were recorded";

        }
    }
}