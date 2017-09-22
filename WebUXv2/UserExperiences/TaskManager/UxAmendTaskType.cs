using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Amend App")]
    [PrimaryActionController("AmendTaskType", "TaskManager")]
    public class UxAmendTaskType : UserExperience
    {
        public override string Title()
        {
            string title = base.Title();
            if ("app".Equals(ComponentHost, StringComparison.CurrentCultureIgnoreCase)) return title;
            if (!String.IsNullOrEmpty(ComponentHost))
            {
                var hostTitle = TaskMan.GetComponentTitle(TaskMan.GetType(ComponentHost));
                title = $"{title} - {hostTitle}";
            }
            return title;

            //if ("app".Equals(ComponentHost, StringComparison.CurrentCultureIgnoreCase)) return base.Title();
            //return $"Amend Secondary Task - {ComponentHost}";
        }

        [ComponentState]
        public string ComponentHost { get; set; }

        [ComponentState]
        public int? ComponentId { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        [ComponentState]
        public string DoneButtonText { get; set; } = "Amend";

        public TaskType LoadTaskType(int id)
        {
            return TaskMan.TaskDbContext.TaskTypes.Find(id);
        }
        public void SaveTaskType(TaskType taskType)
        {

            TaskMan.TaskDbContext.Entry(taskType).State = EntityState.Modified;
            if (taskType.TaskInputs != null)
            {
                foreach (var taskInput in taskType.TaskInputs)
                {
                    TaskMan.TaskDbContext.Entry(taskInput).State = EntityState.Modified;
                }
            }

        }

    }
}