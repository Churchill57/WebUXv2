using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("My Apps")]
    [PrimaryActionController("MyApps", "TaskManager")]
    public class UxMyApps : UserExperience
    {
        public override string Title()
        {
            if ("app".Equals(Host, StringComparison.CurrentCultureIgnoreCase)) return base.Title();
            return $"{Host} - Secondary Tasks";
        }

        [ComponentState]
        public string Host { get; set; }

        public IEnumerable<TaskType> GetApps()
        {
            return TaskMan.TaskDbContext.TaskTypes.Where(t => t.Host == Host).ToList();
        }



    }
}