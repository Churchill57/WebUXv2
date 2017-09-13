using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Pending Tasks")]
    [PrimaryActionController("PendingTasks", "TaskManager")]
    public class UxPendingTasks : UserExperience{}
}