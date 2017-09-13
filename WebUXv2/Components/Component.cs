using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using WebUXv2.Controllers;
using WebUXv2.Models;

namespace WebUXv2.Components
{
    public abstract class Component
    {
        public int TaskId { get; set; }
        public string ClientRef { get; set; }

        public virtual string Title()
        {
            string title = TaskMan.GetComponentTitle(this.GetType());

            if (title == null)
            {
                title = $"#<untitled> {this.GetType().Name}";
                //title = $"#{TaskId} <untitled> {this.GetType().Name}";
            }
            else if (title != string.Empty)
            {
                var contextDescription = TaskMan.GetContextDescription(this);
                if (!string.IsNullOrEmpty(contextDescription)) title = $"{title} - {contextDescription}";
                //if (!string.IsNullOrEmpty(contextDescription)) title = $"#{TaskId} {title} - {contextDescription}";
            }

            return title;
        }

        public string LastSetState { get; set; }

        public ITaskManager TaskMan { get; set; }
        //protected readonly ITaskManager TaskMan;
        protected readonly IEntityContextManager CtxMan;
        protected string _title;

        protected Component() : this(SingletonService.Instance.EntityContextManager)
        {

        }

        protected Component (IEntityContextManager ctxMan)
        {
            CtxMan = ctxMan;
        }

        //protected Component() : this( new TaskManager(), Singleton.Instance.EntityContextManager)
        //{

        //}

        //protected Component(ITaskManager taskMan, IEntityContextManager ctxMan)
        //{
        //    TaskMan = taskMan;
        //    CtxMan = ctxMan;
        //}

        public bool IsDirty => (TaskMan.ObjectToJson(State) != LastSetState);

        public abstract void Save();

        public ExpandoObject RouteParams()
        {
            return ActionParams();
        }

        public virtual ExpandoObject ActionParams()
        {
            return new ExpandoObject();
        }

        public Dictionary<string, object> State
        {
            get
            {
                var state = new Dictionary<string, object>();
                foreach (var propertyInfo in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var attr = propertyInfo.GetCustomAttributes(typeof(ComponentStateAttribute), false);
                    if (attr.Length == 1)
                    {
                        state.Add(propertyInfo.Name, propertyInfo.GetValue(this));
                    }
                }
                return state;
            }
            set
            {
                foreach (var propertyInfo in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var attr = propertyInfo.GetCustomAttributes(typeof(ComponentStateAttribute), false);
                    if (attr.Length == 1)
                    {
                        if (!value.ContainsKey(propertyInfo.Name) || value[propertyInfo.Name] == null)
                        {
                            //propertyInfo.SetValue(this, null);
                        }
                        else
                        {
                            Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                            propertyInfo.SetValue(this, Convert.ChangeType(value[propertyInfo.Name], propertyType), null);
                        }
                    }
                }

            }

        }


    }
}