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
    [ComponentTitle("Remove App")]
    [PrimaryActionController("DeleteTaskType", "TaskManager")]
    public class UxDeleteTaskType : UserExperience
    {
        public override string Title()
        {
            if ("app".Equals(ComponentHost, StringComparison.CurrentCultureIgnoreCase)) return base.Title();
            return $"{ComponentHost} - Remove Secondary Task";
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
        public string DoneButtonText { get; set; } = "Confirm Remove";

        public TaskType LoadTaskType(int id)
        {
            return TaskMan.TaskDbContext.TaskTypes.Find(id);
        }
        public void DeleteTaskType(TaskType taskType)
        {
            if (taskType.TaskInputs != null)
            {
                foreach (var taskInput in taskType.TaskInputs)
                {
                    TaskMan.TaskDbContext.Entry(taskInput).State = EntityState.Deleted;
                }
                //TaskMan.TaskDbContext.SaveChanges();
            }
            taskType.TaskInputs = null;
            TaskMan.TaskDbContext.Entry(taskType).State = EntityState.Deleted;
           // TaskMan.TaskDbContext.TaskTypes.Remove(taskType);
            TaskMan.TaskDbContext.SaveChanges();
        }

    }
}