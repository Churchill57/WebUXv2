using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences.Customer;

namespace WebUXv2.LogicalUnits.Customer
{
    //[Authorize(Roles = "Admin")] // Also use Authorise attribute on AmendCustomer action method in CustomerController
    [ComponentTitle("Capture Customer Second Line Defence Questions")]
    //[LaunchableComponent("capture record save customer 2nd second line second-line defence defense question quest")]
    [SecondaryActionController("CustomerSecondLineDefenceQuestions", "Customer")]

    // TODO: Is LuCapture2ndLineDefenceQuestions pointless considering it only calls UxCapture2ndLineDefenceQuestions?
    public class LuCapture2ndLineDefenceQuestions : LogicalUnit
    {

        //[ComponentInput("customer")] N.B. this attribute is on UxCapture2ndLineDefenceQuestions.CustomerId
        [ComponentState]
        public int? CustomerId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        public override Component GetNextComponent()
        {
            if (!CustomerId.HasValue)
            {
                return GetUserExperience<UxCapture2ndLineDefenceQuestions>("Capture", InitializeCaptureQuestions);
            }
            return null;
        }

        private void InitializeCaptureQuestions(UxCapture2ndLineDefenceQuestions uxCapture2ndLineDefenceQuestions)
        {
            uxCapture2ndLineDefenceQuestions.ShowBackButton = ShowBackButton;
            uxCapture2ndLineDefenceQuestions.BackButtonText = BackButtonText;
        }

        public override Component GetPrevComponent()
        {
            if (CustomerId.HasValue)
            {
                CustomerId = null;
                return GetUserExperience<UxCapture2ndLineDefenceQuestions>("Capture");
            }
            return null;
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == "Capture")
            {
                CustomerId = ((UxCapture2ndLineDefenceQuestions)comp).CustomerContext.Id;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                //BackEvent logic effectively handled in GetPrevComponent
            }
        }

    }
}