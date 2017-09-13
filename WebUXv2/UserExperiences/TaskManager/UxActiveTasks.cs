using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Active Tasks")]
    [PrimaryActionController("ActiveTasks", "TaskManager")]
    public class UxActiveTasks : UserExperience
    {

        public IEnumerable<ActiveTask> GetActiveTasks()
        {
            var luLauncherName = typeof(LuLauncher).Name;

            var luTasks = TaskMan.TaskDbContext.LuTasks.Where(t => t.Name == luLauncherName && t.ParentLuTaskId == null && t.Status == LogicalUnitStatusEnum.Started).ToList();

            foreach (var luTask in luTasks)
            {
                var luLauncher = TaskMan.GetLogicalUnit(luTask) as LuLauncher;
                if (luLauncher==null) continue;
                
                Component rootComponent = TaskMan.GetLogicalUnit(luLauncher.ComponentTaskId.Value);
                if (rootComponent==null) rootComponent = TaskMan.GetUserExperience(luLauncher.ComponentTaskId.Value);
                luTask.Name = rootComponent?.Title();

                if (luLauncher.ResumeTaskId.HasValue)
                {
                    var uxResume = TaskMan.GetUserExperience(luLauncher.ResumeTaskId.Value);
                    luTask.Name += " / " + uxResume?.Title();
                }
            }

            var activeTasks = from t in luTasks
                              where !String.IsNullOrEmpty(t.Name)
                              orderby t.Id descending 
                              select new ActiveTask() {TaskId = t.Id, Name = t.Name};

            return activeTasks;
        }
    }
}