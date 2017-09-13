using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("Completed Tasks")]
    [PrimaryActionController("CompletedTasks", "TaskManager")]
    public class UxCompletedTasks : UserExperience
    {

        public IEnumerable<CompletedTask> GetCompletedTasks()
        {
            var luLauncherName = typeof(LuLauncher).Name;

            var luTasks = TaskMan.TaskDbContext.LuTasks.Where(t => t.Name == luLauncherName && t.ParentLuTaskId == null && t.Status != LogicalUnitStatusEnum.Started).ToList();

            foreach (var luTask in luTasks)
            {
                var luLauncher = TaskMan.GetLogicalUnit(luTask) as LuLauncher;
                if (luLauncher == null) continue;

                Component rootComponent = TaskMan.GetLogicalUnit(luLauncher.ComponentTaskId.Value);
                if (rootComponent == null) rootComponent = TaskMan.GetUserExperience(luLauncher.ComponentTaskId.Value);
                luTask.Name = rootComponent?.Title();

                if (luLauncher.ResumeTaskId.HasValue)
                {
                    var uxResume = TaskMan.GetUserExperience(luLauncher.ResumeTaskId.Value);
                    luTask.Name += " / " + uxResume?.Title();
                }
            }

            var completedTasks = from t in luTasks
                              where !String.IsNullOrEmpty(t.Name)
                              orderby t.Id descending
                                 select new CompletedTask() { Id = t.Id, Name = t.Name };

            return completedTasks;

            //var luLauncherName = typeof(LuLauncher).Name;

            //var luTasks = TaskMan.TaskDbContext.LuTasks.Where(t => t.Name == luLauncherName && t.ParentLuTaskId == null && t.Status != LogicalUnitStatusEnum.Started).ToList();

            //foreach (var luTask in luTasks)
            //{
            //    var state = TaskMan.JsonToObject<Dictionary<string, object>>(luTask.State);
            //    luTask.Name = (string)state["Title"];
            //}

            //var completedTasks = from t in luTasks
            //                     where !String.IsNullOrEmpty(t.Name)
            //                     orderby t.Id descending
            //                     select new CompletedTask() { Id = t.Id, Name = t.Name };

            //return completedTasks;
        }

    }
}