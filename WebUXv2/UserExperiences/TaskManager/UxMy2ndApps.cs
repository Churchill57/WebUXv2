using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Secondary Apps")]
    [PrimaryActionController("My2ndApps", "TaskManager")]
    public class UxMy2ndApps : UserExperience
    {
        [ComponentState]
        public string Host { get; set; }

        [ComponentState]
        public bool HideInstructions { get; set; }

        public IEnumerable<TaskType> GetApps()
        {
            var secondaryTaskTypes = TaskMan.TaskDbContext.TaskTypes.Where(t => t.Host == Host).ToList();
            foreach (var taskType in secondaryTaskTypes)
            {
                var rootComponentType = TaskMan.GetType(taskType.RootComponentName);
                if (!rootComponentType.IsDefined(typeof(SecondaryActionControllerAttribute), false)) continue;
                var secondaryUxAttr = rootComponentType.GetCustomAttribute<SecondaryActionControllerAttribute>();
                taskType.SecondaryController = secondaryUxAttr.ControllerName;
                taskType.SecondaryAction = secondaryUxAttr.ActionName;
            }

            return secondaryTaskTypes;
        }

        //public IEnumerable<TaskType> GetApps()
        //{
        //    return TaskMan.TaskDbContext.TaskTypes.Where(t => t.Host == Host).ToList();
        //}



    }
}