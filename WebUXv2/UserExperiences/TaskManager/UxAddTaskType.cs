using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Add to My Apps")]
    [PrimaryActionController("AddTaskType", "TaskManager")]
    public class UxAddTaskType : UserExperience
    {
        public override string Title()
        {
            if ("app".Equals(ComponentHostToAdd, StringComparison.CurrentCultureIgnoreCase)) return base.Title();
            return $"{ComponentHostToAdd} - Add Secondary Task";
        }

        [ComponentState]
        public string ComponentNameToAdd { get; set; }

        [ComponentState]
        public string ComponentHostToAdd { get; set; }

        [ComponentState]
        public bool ShowBackButton { get; set; } = true;

        [ComponentState]
        public string BackButtonText { get; set; } = "Cancel";

        [ComponentState]
        public string DoneButtonText { get; set; } = "Add";

        public TaskType PrepareComponentTaskType()
        {
            var componentType = TaskMan.GetType(ComponentNameToAdd);

            var titleAttr = componentType.GetCustomAttribute<ComponentTitleAttribute>();
            var launchAttr = componentType.GetCustomAttribute<LaunchableComponentAttribute>();
            var taskType = new TaskType()
            {
                Id = 0,
                Host = ComponentHostToAdd,
                RootComponentName = ComponentNameToAdd,
                Name = titleAttr.Title,
                SearchTags = launchAttr.SearchTags
            };

            var component = TaskMan.CreateInstanceOfTypeName(ComponentNameToAdd);

            taskType.TaskInputs = (
                from pi in componentType.GetProperties()
                from attr in pi.GetCustomAttributes(typeof(LaunchInputAttribute), false)
                select new TaskInput() { Id = 0, Name = pi.Name, Value = pi.GetValue(component)?.ToString() }
            ).ToList();

            return taskType;
        }

        public void SaveTaskType(TaskType taskType)
        {
            TaskMan.TaskDbContext.TaskTypes.Add(taskType);
            TaskMan.TaskDbContext.SaveChanges();
        }

    }
}