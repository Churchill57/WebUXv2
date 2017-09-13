using System;
using System.Data.Entity;
using System.Linq;

namespace WebUXv2.Components
{
    public abstract class LogicalUnit : Component
    {
        public abstract Component GetNextComponent();
        public abstract Component GetPrevComponent();
        public abstract void ComponentCompleted(Component comp);
        public abstract void HandleComponentEvent(ComponentEventArgs eventArgs);
        public LogicalUnitStatusEnum Status { get; set; }

        public T GetLogicalUnit<T>(string clientRef, Action<T> initializer = null) where T : LogicalUnit, new()
        {
            return TaskMan.GetLogicalUnit(this, clientRef, initializer);
        }
        public T GetUserExperience<T>(string clientRef, Action<T> initializer = null) where T : UserExperience, new()
        {
            return TaskMan.GetUserExperience(this, clientRef, initializer);
        }

        public T GetRefreshedUserExperience<T>(string clientRef) where T : UserExperience, new()
        {
            return TaskMan.GetRefreshedUserExperience<T>(this, clientRef) as T;

        }

        public string UserName { get; set; }

        public override void Save()
        {
            TaskMan.SaveLogicalUnit(this);
        }

    }
}
