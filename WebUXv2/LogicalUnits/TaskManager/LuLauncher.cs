using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Controllers;
using WebUXv2.Events;
using WebUXv2.Events.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.LogicalUnits.TaskManager
{
    //[ComponentTitle("Launcher")]
    ////[LaunchableComponent("temporary")]
    public class LuLauncher : LogicalUnit
    {
        public override string Title()
        {
            return "Launcher";
        }

        [ComponentState]
        public string ComponentName { get; internal set; }

        [ComponentState]
        public int? ComponentTaskId { get; internal set; }

        [ComponentState]
        public string ReturnUrl { get; set; }

        [ComponentState]
        public int? ReturnTaskId { get; set; }

        [ComponentState]
        public int? ResumeTaskId { get; set; }

        [ComponentState]
        public string ReturnTaskRef { get; set; }

        public LuLauncher(string componentName, string userName, string returnUrl)
        {
            ComponentName = componentName;
            UserName = userName;
            ReturnUrl = returnUrl;
        }

        public void InitialiseRootComponentAndInputs(Models.TaskType taskType)
        {
            if (!taskType.TaskInputs.Any()) return;
            var component = GetRootComponent();
            var componentInputProperties = TaskMan.ComponentInputProperties(component);
            foreach (var input in taskType.TaskInputs)
            {
                var propertyInfo = component.GetType().GetProperty(input.Name);
                if (propertyInfo != null)
                {
                    Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    if (input.Value != null)
                    {

                        // If the assigned property is a context type component input, then set the appropriate context.
                        // But first the assigned value must be integer.
                        int Id;
                        if (Int32.TryParse(input.Value, out Id))
                        {
                            if (componentInputProperties.ContainsKey(propertyInfo.Name))
                            {
                                var entityContext = CtxMan.SetContext(Id, componentInputProperties[propertyInfo.Name], null);
                                propertyInfo.SetValue(component, Convert.ChangeType(entityContext, propertyType), null);
                                continue;
                            }
                        }

                        // Non EntityContext input
                        propertyInfo.SetValue(component, Convert.ChangeType(input.Value, propertyType), null);
                    }

                }
            }
            component.Save();
        }

        public LuLauncher() {} // Parameterless constructor required for deserialisation.

        public override Component GetNextComponent()
        {
            if (Status == LogicalUnitStatusEnum.Completed) return null;
            return GetRootComponent();
        }

        public Component GetRootComponent()
        {
            var lu = TaskMan.GetLogicalUnit(this, ComponentName, UserName, ComponentName);
            ComponentTaskId = lu?.TaskId;
            if (lu != null) return lu;
            var ux = TaskMan.GetUserExperience(this, ComponentName, ComponentName);
            ComponentTaskId = ux?.TaskId;
            return ux;
        }

        public override Component GetPrevComponent()
        {
            return null;
        }


        public override void ComponentCompleted(Component comp)
        {
            if (ComponentName.Equals(comp.ClientRef,StringComparison.CurrentCultureIgnoreCase)) Status = LogicalUnitStatusEnum.Completed;
        }
        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                //BackEvent logic N/A for launcher
            }
        }

    }

}